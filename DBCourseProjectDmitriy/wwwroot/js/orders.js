const apiName = "purchaseOrder";

async function getPurchaseOrder() {
    const response = await fetch(`/api/${apiName}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        const purchaseOrders = await response.json();
        const rows = document.querySelector("tbody");
        purchaseOrders.forEach(purchaseOrder => {
            rows.append(row([
                purchaseOrder.id,
                purchaseOrder.sum,
                purchaseOrder.contract.isSupply ? purchaseOrder.contract.manufacturer.name : purchaseOrder.contract.client.fullName,
                purchaseOrder.paymentStat ? "Заказ оплачен" : "Заказ не оплачен",
            ]));
        });
    }
}

document.getElementById("info").addEventListener("click", async () => {
    let id = "";
    var tr = document.getElementsByClassName('active')[0];
    if (tr) {
        id = tr.childNodes[0].innerHTML;
    }

    let purchaseOrder;
    const response = await fetch(`/api/${apiName}/${id}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        purchaseOrder = await response.json();
    }

    let contractOwnerName = purchaseOrder.contract.isSupply ? "Поставщик: " + purchaseOrder.contract.manufacturer.name : "Покупатель: " + purchaseOrder.contract.client.fullName;

    document.getElementById("contractOwnerName").innerText = contractOwnerName;
    document.getElementById("sumOrder").innerText = "Сумма заказа: " + purchaseOrder.sum;
    document.getElementById("paymentStatOrder").innerText = "Статус заказа: " + (purchaseOrder.paymentStat ? "Заказ оплачен" : "Заказ не оплачен");

    const responseO = await fetch(`/api/orderedGoods/fromPurchaseOrder/${id}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (responseO.ok === true) {
        const orderedGoods = await responseO.json();
        const rows = document.getElementById("infoTable");
        rows.innerHTML = "<tbody id=\"infoTable\"></tbody>";
        orderedGoods.forEach(orderedGood => {
            rows.append(row([
                orderedGood.electronics.id,
                orderedGood.electronics.name,
                orderedGood.electronics.price,
                orderedGood.electronics.electronicsType.type,
                orderedGood.quantity
            ]));
        });
    }
});

document.getElementById("payBtn").addEventListener('click', async () => {
    let id = "";
    var tr = document.getElementsByClassName('active')[0];
    if (tr) {
        id = tr.childNodes[0].innerHTML;
    }

    let purchaseOrder;
    const response = await fetch(`/api/${apiName}/${id}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        purchaseOrder = await response.json();
    }

    if (purchaseOrder.paymentStat) return;

    let ret = false;

    const responseO = await fetch(`/api/orderedGoods/fromPurchaseOrder/${id}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (responseO.ok === true) {
        const orderedGoods = await responseO.json();
        orderedGoods.forEach(async orderedGood => {
            let electronics = orderedGood.electronics;

            const responseE = await fetch(`/api/electronics/${orderedGood.electronics.id}`, {
                method: "GET",
                headers: { "Accept": "application/json" }
            });
            if (responseE.ok === true) {
                electronics = await responseE.json();
            }

            if (purchaseOrder.contract.isSupply) {
                electronics.electronicsInStock += orderedGood.quantity;
                electronics.deliveredNumber += orderedGood.quantity;
            } else {
                if (electronics.electronicsInStock < orderedGood.quantity) {
                    var myModal = new bootstrap.Modal(document.getElementById('errorInfoModal'));
                    myModal.show();
                    ret = true;
                    return;
                }

                electronics.electronicsInStock -= orderedGood.quantity;
                electronics.soldNumber += orderedGood.quantity;
            }

            await editModel("electronics", JSON.stringify({
                id: electronics.id,
                name: electronics.name,
                price: electronics.price,
                electronicsInStock: electronics.electronicsInStock,
                soldNumber: electronics.soldNumber,
                deliveredNumber: electronics.deliveredNumber,
                electronicsType: electronics.electronicsType,
                manufacturer: electronics.manufacturer
            }), false);
        });

        if (ret) return;
    }

    await sleep(100).then(async () => {
        if (ret) return;

        if (!ret) {
            purchaseOrder.paymentStat = true;

            await editModel(apiName, JSON.stringify({
                id: purchaseOrder.id,
                paymentStat: true,
                sum: purchaseOrder.sum,
                contract: {
                    id: purchaseOrder.contract.id,
                    date: {
                        "createdDate": purchaseOrder.contract.date
                    },
                    realDate: purchaseOrder.contract.date,
                    isSupply: purchaseOrder.contract.isSupply,
                    manufacturer: purchaseOrder.contract.manufacturer,
                    client: purchaseOrder.contract.client
                },
            }), false);

            document.querySelector(`tr[data-rowid='${purchaseOrder.id}']`).replaceWith(row([
                purchaseOrder.id,
                purchaseOrder.sum,
                purchaseOrder.contract.isSupply ? purchaseOrder.contract.manufacturer.name : purchaseOrder.contract.client.fullName,
                purchaseOrder.paymentStat ? "Заказ оплачен" : "Заказ не оплачен",
            ]));

            await createModel("bill", JSON.stringify({
                sum: purchaseOrder.sum,
                date: {
                    "createdDate": "Костыль"
                },
                purchaseOrder: {
                    id: purchaseOrder.id,
                    paymentStat: true,
                    sum: purchaseOrder.sum,
                    contract: {
                        id: purchaseOrder.contract.id,
                        date: {
                            "createdDate": purchaseOrder.contract.date
                        },
                        realDate: purchaseOrder.contract.date,
                        isSupply: purchaseOrder.contract.isSupply,
                        manufacturer: purchaseOrder.contract.manufacturer,
                        client: purchaseOrder.contract.client
                    },
                }
            }), false);
        }
    });
});

document.getElementById("deleteBtn").addEventListener("click", async () => {
    let id = "";
    var tr = document.getElementsByClassName('active')[0];
    if (tr) {
        id = tr.childNodes[0].innerHTML;
    }

    const response = await fetch(`/api/${apiName}/${id}`, {
        method: "DELETE",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        const purchaseOrder = await response.json();
        document.querySelector(`tr[data-rowid='${purchaseOrder.id}']`).remove();
    }
    else {
        const error = await response.json();
        console.log(error.message);
    }
});

var table = document.getElementsByTagName("table")[0];
table.addEventListener("click", (event) => {
    let tr = event.target.closest('tr');

    if (!tr) return;

    if (!table.contains(tr)) return;

    if (tr.classList.contains('head')) return;

    if (tr.childNodes[3].innerHTML === "Заказ оплачен") {
        sleep(0).then(async () => {
            document.getElementById("delete").setAttribute("disabled", true);
            document.getElementById("pay").setAttribute("disabled", true);
        });
    }
});

async function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

getPurchaseOrder();