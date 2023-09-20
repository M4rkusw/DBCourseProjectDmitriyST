const apiName = "orderedGoods";

async function getOrderedGoods() {
    const response = await fetch(`/api/${apiName}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        const orderedGoods = await response.json();
        const rows = document.querySelector("tbody");
        orderedGoods.forEach(orderedGood => {
            if (orderedGood.purchaseOrder == null) {
                let input = document.createElement("input");
                input.className = "form-check-input";
                input.type = "checkbox";
                input.name = "orderedGood";
                input.id = orderedGood.electronics.id;
                rows.append(row(Object.values({
                    isCheck: input,
                    id: orderedGood.id,
                    name: orderedGood.electronics.name,
                    quantity: orderedGood.quantity,
                    sum: orderedGood.sum
                })));
            }
        });

        await setListeners();
    }
}

async function getContracts() {
    const responce = await fetch(`/api/contracts`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (responce.ok === true) {
        const contracts = await responce.json();
        let select = document.getElementById("contract");
        contracts.forEach(contract => select.append(new Option(
            "Номер договора: " + Object.values(contract)[0] + (Object.values(contract)[3] === true ? (", Поставщик: " + Object.values(contract)[4].name) : (", Покупатель: " + Object.values(contract)[5].fullName)),
            Object.values(contract)[0])));

    }
}

document.getElementById("edit").addEventListener("click", async () => {
    deleteWasValidated();
    let id = "";
    var tr = document.getElementsByClassName('active')[0];
    if (tr) {
        id = tr.childNodes[1].innerHTML;
    }

    const response = await fetch(`/api/${apiName}/${id}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        const orderedGood = await response.json();
        document.getElementById("quantity").value = orderedGood.quantity;
    }
});

document.getElementById("editBtn").addEventListener("click", async () => {
    let id = "";
    let electronicsId = "";

    var tr = document.getElementsByClassName('active')[0];
    if (tr) {
        id = tr.childNodes[1].innerHTML;
        electronicsId = tr.childNodes[0].childNodes[0].id;
    }

    const quantity = document.getElementById("quantity").value;

    if (quantity === '') return;

    let electronics;
    const response = await fetch(`/api/electronics/${electronicsId}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        electronics = await response.json();
    }

    await editModel(apiName, JSON.stringify({
        id: id,
        quantity: quantity,
        electronics: electronics,
        sum: quantity * electronics.price,
        purchaseOrder: null
    }), false);

    document.querySelector("tbody").innerHTML = "<tbody></tbody>"
    getOrderedGoods();
    await bootstrap.Modal.getInstance(document.getElementById('editModal')).hide();
    reset();
});

async function setListeners() {
    document.querySelectorAll("input[type=checkbox][name=orderedGood]").forEach(async (checkbox) => {
        checkbox.addEventListener('change', async () => {
            let isActive = false;
            document.querySelectorAll("input[type=checkbox][name=orderedGood]").forEach(async (checkbox1) => {
                if (checkbox1.checked) {
                    isActive = true;
                }
            });

            if (isActive) {
                document.getElementById("add").disabled = false;
            } else {
                document.getElementById("add").disabled = true;
            }
        })
    });
}

document.getElementById("addBtn").addEventListener("click", async () => {
    let contractId = document.getElementById("contract").value;

    if (contractId === '') return;

    let contract;
    const response = await fetch(`/api/contracts/${contractId}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        contract = await response.json();
    }

    let sum = 0;

    document.querySelectorAll("input[type=checkbox][name=orderedGood]").forEach(async (checkbox1) => {
        if (checkbox1.checked) {
            sum += parseInt(checkbox1.parentNode.parentNode.childNodes[4].innerHTML);
        }
    });

    let purchaseOrder = await createModel("purchaseOrder", JSON.stringify({
        sum: sum,
        paymentStat: false,
        contract: {
            id: contract.id,
            date: {
                "createdDate": contract.date
            },
            realDate: contract.date,
            isSupply: contract.isSupply,
            manufacturer: contract.manufacturer,
            client: contract.client
        },
    }), false);

    document.querySelectorAll("input[type=checkbox][name=orderedGood]").forEach(async (checkbox1) => {
        if (checkbox1.checked) {
            let id = checkbox1.parentNode.parentNode.childNodes[1].innerHTML;

            let orderedGoods;
            const responseO = await fetch(`/api/${apiName}/${id}`, {
                method: "GET",
                headers: { "Accept": "application/json" }
            });
            if (responseO.ok === true) {
                orderedGoods = await responseO.json();
            }

            await editModel(apiName, JSON.stringify({
                id: orderedGoods.id,
                quantity: orderedGoods.quantity,
                electronics: orderedGoods.electronics,
                sum: orderedGoods.quantity * orderedGoods.electronics.price,
                purchaseOrder: purchaseOrder
            }), false);

            checkbox1.parentNode.parentNode.innerHTML = "";
        }
    });

    reset();
    await bootstrap.Modal.getInstance(document.getElementById('addModal')).hide();
    document.getElementById("add").disabled = true;
});

document.getElementById("add").addEventListener("click", () => {
    document.getElementById("contract").value = "";
});

document.getElementById("deleteBtn").addEventListener("click", async () => {
    let id = "";
    var tr = document.getElementsByClassName('active')[0];
    if (tr) {
        id = tr.childNodes[1].innerHTML;
    }

    const response = await fetch(`/api/${apiName}/${id}`, {
        method: "DELETE",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        const orderedGoods = await response.json();
        document.querySelector(`tr[class='active']`).remove();
    }
    else {
        const error = await response.json();
        console.log(error.message);
    }
});

getContracts()
getOrderedGoods();