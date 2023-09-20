const apiName = "electronics";

document.getElementById("saveBtn").addEventListener("click", async () => {
    let id = "";
    var tr = document.getElementsByClassName('active')[0];
    if (tr) {
        id = tr.childNodes[0].innerHTML;
    }

    const name = document.getElementById("name").value;
    const price = document.getElementById("price").value;

    if (name === '' || price === '' || document.getElementById("electronicsType").value === '' || document.getElementById("manufacturer").value === '') return;

    let electronicsType;
    const responseT = await fetch(`/api/electronicsType/${document.getElementById("electronicsType").value}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (responseT.ok === true) {
        electronicsType = await responseT.json();
    }

    let manufacturer;
    const responseM = await fetch(`/api/manufacturers/${document.getElementById("manufacturer").value}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (responseM.ok === true) {
        manufacturer = await responseM.json();
    }

    if (id === "")
        await createModel(apiName, JSON.stringify({
            name: name,
            price: price,
            electronicsType: electronicsType,
            manufacturer: manufacturer
        }));
    else
        await editModel(apiName, JSON.stringify({
            id: id,
            name: name,
            price: price,
            electronicsInStock: document.getElementsByClassName('active')[0].childNodes[3].innerHTML,
            soldNumber: document.getElementsByClassName('active')[0].childNodes[4].innerHTML,
            deliveredNumber: document.getElementsByClassName('active')[0].childNodes[5].innerHTML,
            electronicsType: electronicsType,
            manufacturer: manufacturer
        }));

    document.querySelector("tbody").innerHTML = "<tbody></tbody>"
    getElectronics();
    await bootstrap.Modal.getInstance(document.getElementById('addModal')).hide();
    reset();
});

document.getElementById("edit").addEventListener("click", async () => {
    let id = "";
    var tr = document.getElementsByClassName('active')[0];
    if (tr) {
        id = tr.childNodes[0].innerHTML;
    }

    const response = await fetch(`/api/${apiName}/${id}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        const electronics = await response.json();
        document.getElementById("name").value = electronics.name;
        document.getElementById("price").value = electronics.price;
        document.getElementById("electronicsType").value = electronics.electronicsType.id;
        document.getElementById("manufacturer").value = electronics.manufacturer.id;
    }

    deleteWasValidated();
});

async function getElectronics() {
    const response = await fetch(`/api/${apiName}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        const electronics = await response.json();
        const rows = document.querySelector("tbody");
        electronics.forEach(electronic => {
            rows.append(row([
                electronic.id,
                electronic.name,
                electronic.price,
                electronic.electronicsInStock,
                electronic.soldNumber,
                electronic.deliveredNumber,
                electronic.electronicsType.type,
                electronic.manufacturer.name
            ]));
        });
    }
}

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
        const electronics = await response.json();
        document.querySelector(`tr[data-rowid='${electronics.id}']`).remove();
    }
    else {
        const error = await response.json();
        console.log(error.message);
    }

    reset();
})

async function getManufacturers() {
    const response = await fetch(`/api/manufacturers`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        const manufacturers = await response.json();
        let select = document.getElementById("manufacturer");
        manufacturers.forEach(manufacturer => select.append(new Option(Object.values(manufacturer)[1], Object.values(manufacturer)[0])));
    }
}

async function getElectronicsType(id = -1) {
    const response = await fetch(`/api/electronicsType`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        const electronicsTypes = await response.json();
        let select = document.getElementById("electronicsType");
        let other;
        while (select.lastChild) {
            if (select.lastChild.value == "other") {
                other = select.lastChild;
            }
            if (select.lastChild.value == "") {
                break;
            }
            select.removeChild(select.lastChild);
        }
        select.appendChild(other);
        electronicsTypes.forEach(electronicsType => select.append(new Option(Object.values(electronicsType)[1], Object.values(electronicsType)[0])));
        if (id != -1) {
            document.getElementById("electronicsType").value = id;
        }
    }
}

document.getElementById("electronicsType").addEventListener("change", async () => {
    var selcet = document.getElementById("electronicsType");
    if (selcet.value == "other") {
        var myModal = new bootstrap.Modal(document.getElementById('addType'));
        document.getElementById('type').value = '';
        myModal.show();
    }
})

document.getElementById("saveTypeBtn").addEventListener("click", async () => {
    const type = document.getElementById("type").value;
    if (type === '') return;
    const response = await fetch("/api/electronicsType", {
        method: "POST",
        headers: { "Accept": "application/json", "Content-Type": "application/json" },
        body: JSON.stringify({ type: type })
    });
    if (response.ok === true) {
        const t = await response.json();
        getElectronicsType(t.id);
    }
    else {
        const error = await response.json();
        console.log(error.message);
    }

    await bootstrap.Modal.getInstance(document.getElementById('addType')).hide();
});

document.getElementById("addBtn").addEventListener("click", async () => {
    let id = "";
    var tr = document.getElementsByClassName('active')[0];
    if (tr) {
        id = tr.childNodes[0].innerHTML;
    }

    let electronics;
    const response = await fetch(`/api/${apiName}/${id}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        electronics = await response.json();
    }

    let quantity = document.getElementById("quantity").value;

    if (quantity === '') return;

    await createModel("orderedGoods", JSON.stringify({
        quantity: quantity,
        electronics: electronics,
        sum: quantity * electronics.price,
        purchaseOrder: null
    }), false);

    await bootstrap.Modal.getInstance(document.getElementById('addInCartModal')).hide();
});

document.getElementById("add").addEventListener("click", () => {
    document.getElementById("electronicsType").value = "";
    document.getElementById("manufacturer").value = "";
});

document.getElementById("addInCart").addEventListener('click', () => {
    document.getElementById("quantity").value = "";
    deleteWasValidated();
});

document.getElementById("cancellationBtn").addEventListener('click', () => {
    document.getElementById("addTypeForm").classList.remove('was-validated');
    document.getElementById("electronicsType").value = "";
});

document.getElementById("closeBtn").addEventListener('click', () => {
    document.getElementById("addTypeForm").classList.remove('was-validated');
    document.getElementById("electronicsType").value = "";
});

getManufacturers();
getElectronicsType();
getElectronics();