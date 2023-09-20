const apiName = "contracts";

document.getElementById("saveBtn").addEventListener("click", async () => {
    let id = "";
    var tr = document.getElementsByClassName('active')[0];
    if (tr) {
        id = tr.childNodes[0].innerHTML;
    }

    const date = document.getElementById("date").value;

    if (date === '' || (document.getElementById("manufacturer").value === '' && document.getElementById("client").value === '')) return;

    let manufacturer;
    if (document.getElementById("manufacturer").value !== '') {
        const response = await fetch(`/api/manufacturers/${document.getElementById("manufacturer").value}`, {
            method: "GET",
            headers: { "Accept": "application/json" }
        });
        if (response.ok === true) {
            manufacturer = await response.json();
        } else {
            manufacturer = null;
        }
    } else {
        manufacturer = null;
    }

    let client;
    if (document.getElementById("client").value !== '') {
        const response = await fetch(`/api/clients/${document.getElementById("client").value}`, {
            method: "GET",
            headers: { "Accept": "application/json" }
        });
        if (response.ok === true) {
            client = await response.json();
        } else {
            client = null;
        }
    } else {
        client = null;
    }

    let isSupply = null;
    if (document.getElementById("manufacturer").value !== '') {
        isSupply = true;
    }

    if (document.getElementById("client").value !== '') {
        isSupply = false;
    }

    if (id === "")
        await createModel(apiName, JSON.stringify({
            date: {
                "createdDate": new Date(date)
            },
            realDate: new Date(date),
            isSupply: isSupply,
            manufacturer: manufacturer,
            client: client
        }));
    else
        await editModel(apiName, JSON.stringify({
            id: id,
            date: {
                "createdDate": new Date(date)
            },
            realDate: new Date(date),
            isSupply: isSupply,
            manufacturer: manufacturer,
            client: client
        }));

    document.querySelector("tbody").innerHTML = "<tbody></tbody>";
    getContracts(!document.getElementById("flexSwitchCheck").checked);
    await bootstrap.Modal.getInstance(document.getElementById('addModal')).hide();
    reset();
});

document.getElementById("edit").addEventListener("click", async () => {
    deleteWasValidated();
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
        const contract = await response.json();
        document.getElementById("date").value = new Date(contract.date).toISOString().slice(0, -1);
        document.getElementById("manufacturer").value = contract.manufacturer == null ? '' : contract.manufacturer.id;
        document.getElementById("client").value = contract.client == null ? '' : contract.client.id;
    }
});

//todo: по ощущениям, можно в site.js закинуть
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
});

document.getElementById("flexSwitchCheck").addEventListener("change", async () => {
    let checkLabel = document.getElementById("checkLabel");
    let manufacturerSelect = document.getElementById("manufacturerSelect");
    let clientSelect = document.getElementById("clientSelect");

    if (document.getElementById("flexSwitchCheck").checked) {
        checkLabel.innerHTML = "На продажу";
        manufacturerSelect.hidden = true;
        clientSelect.hidden = false;
    } else {
        checkLabel.innerHTML = "На поставку";
        manufacturerSelect.hidden = false;
        clientSelect.hidden = true;
    }

    document.querySelector("tbody").innerHTML = "<tbody></tbody>";
    getContracts(!document.getElementById("flexSwitchCheck").checked);
    reset();
});

document.getElementById("add").addEventListener("click", () => {
    document.getElementById("manufacturer").value = "";
    document.getElementById("client").value = "";
});

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

async function getClients() {
    const response = await fetch(`/api/clients`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        const clients = await response.json();
        let select = document.getElementById("client");
        clients.forEach(client => select.append(new Option(Object.values(client)[1], Object.values(client)[0])));
    }
}

async function getContracts(isSupply) {
    const responce = await fetch(`/api/${apiName}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (responce.ok === true) {
        const contracts = await responce.json();
        const rows = document.querySelector("tbody");
        for (const contract of contracts) {
            if (contract.isSupply != isSupply) {
                continue;
            }
            rows.append(row([
                contract.id,
                new Date(contract.date).toUTCString(),
                isSupply ? contract.manufacturer.name : contract.client.fullName
            ]));
        }
    }
}

getClients();
getManufacturers();
getContracts(!document.getElementById("flexSwitchCheck").checked);