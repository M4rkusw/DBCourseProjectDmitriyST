const apiName = "clients";

// отправка формы
document.getElementById("saveBtn").addEventListener("click", async () => {
    let id = "";
    var tr = document.getElementsByClassName('active')[0];
    if (tr) {
        id = tr.childNodes[0].innerHTML;
    }

    const fullName = document.getElementById("fullName").value;
    const telephone = document.getElementById("telephone").value;
    const bankAccount = document.getElementById("bankAccount").value;
    if (fullName === '' || telephone === '' || bankAccount === '') return;
    if (id === "")
        await createModel(apiName, JSON.stringify({
            fullName: fullName,
            telephone: telephone,
            bankAccount: bankAccount
        }));
    else
        await editModel(apiName, JSON.stringify({
            id: id,
            fullName: fullName,
            telephone: telephone,
            bankAccount: bankAccount
        }));

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

    const response = await fetch(`/api/clients/${id}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        const client = await response.json();
        document.getElementById("fullName").value = client.fullName;
        document.getElementById("telephone").value = client.telephone;
        document.getElementById("bankAccount").value = client.bankAccount;
    }
});

async function getClient() {
    // отправляет запрос и получаем ответ
    const response = await fetch("/api/clients", {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    // если запрос прошел нормально
    if (response.ok === true) {
        // получаем данные
        const clients = await response.json();
        const rows = document.querySelector("tbody");
        // добавляем полученные элементы в таблицу
        clients.forEach(async client => rows.append(row(Object.values(client))));
    }
}

// Изменение пользователя
async function editClient(clientId, fullName, telephone, bankAccount) {
    const response = await fetch("/api/clients", {
        method: "PUT",
        headers: { "Accept": "application/json", "Content-Type": "application/json" },
        body: JSON.stringify({
            clientId: clientId,
            fullName: fullName,
            telephone: telephone,
            bankAccount: bankAccount
        })
    });
    if (response.ok === true) {
        const client = await response.json();

        document.querySelector(`tr[data-rowid='${client.clientId}']`).replaceWith(row(Object.values(client)));
    }
    else {
        const error = await response.json();
        console.log(error.message);
    }
}

// Удаление пользователя
document.getElementById("deleteBtn").addEventListener("click", async () => {
    let id = "";
    var tr = document.getElementsByClassName('active')[0];
    if (tr) {
        id = tr.childNodes[0].innerHTML;
    }

    //todo: попробовать это в site.js кинуть
    const response = await fetch(`/api/clients/${id}`, {
        method: "DELETE",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        const client = await response.json();
        document.querySelector(`tr[data-rowid='${client.id}']`).remove();
    }
    else {
        const error = await response.json();
        console.log(error.message);
    }

    reset();
})

getClient();