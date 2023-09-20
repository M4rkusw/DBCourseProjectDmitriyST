let selectedTr;

var table = document.getElementsByTagName("table")[0];
table.addEventListener("click", (event) => {
    let tr = event.target.closest('tr');

    if (!tr) return;

    if (!table.contains(tr)) return;

    if (tr.classList.contains('head')) return;

    if (document.getElementById("edit")) document.getElementById("edit").removeAttribute("disabled");
    if (document.getElementById("delete")) {
        document.getElementById("delete").removeAttribute("disabled");
    } else {
        return;
    }

    if (document.getElementById("addInCart")) document.getElementById("addInCart").removeAttribute("disabled");

    if (document.getElementById("info")) document.getElementById("info").removeAttribute("disabled");
    if (document.getElementById("pay")) document.getElementById("pay").removeAttribute("disabled");

    highlight(tr);
});

function highlight(tr) {
    if (selectedTr) {
        selectedTr.classList.remove('active');
    }
    selectedTr = tr;
    selectedTr.classList.add('active');
}

function reset() {
    var elements = document.getElementsByTagName("input");
    for (let i = 0; i < elements.length; i++) {
        elements[i].value = "";
    }

    if (document.getElementById("edit")) document.getElementById("edit").setAttribute("disabled", true);
    if (document.getElementById("delete")) document.getElementById("delete").setAttribute("disabled", true);

    if (document.getElementById("addInCart")) document.getElementById("addInCart").setAttribute("disabled", true);

    if (document.getElementById("info")) document.getElementById("info").setAttribute("disabled", true);
    if (document.getElementById("pay")) document.getElementById("pay").setAttribute("disabled", true);
}

function deleteWasValidated() {
    let forms = document.getElementsByClassName('was-validated');
    Array.prototype.forEach.call(forms, function (form) {
        form.classList.remove('was-validated');
    });
}

if (document.getElementById("add")) document.getElementById("add").addEventListener("click", () => {
    deleteWasValidated();
    reset();
    if (document.getElementsByClassName('active')[0] != null) {
        document.getElementsByClassName('active')[0].classList.remove('active');
    }
});

if (document.getElementById("unload")) document.getElementById("unload").addEventListener("click", async () => {
    const response = await fetch(`/api/${document.getElementById("unload").name}/unload`, {
        method: "POST",
        headers: { "Accept": "application/json", "Content-Type": "application/json" },
        body: null
    });
    if (response.ok === true) {
        var modal = new bootstrap.Modal(document.getElementById('unloadInfoModal'));
        modal.show();
    }
})

function row(model) {
    const tr = document.createElement("tr");
    tr.setAttribute("data-rowid", model[0]);

    for (let i = 0; i < model.length; i++) {
        const td = document.createElement("td");
        td.append(model[i]);
        tr.append(td);
    }

    return tr;
}

async function createModel(modelsName, newModel, createRow = true) {
    const response = await fetch(`/api/${modelsName}`, {
        method: "POST",
        headers: { "Accept": "application/json", "Content-Type": "application/json" },
        body: newModel
    });
    if (response.ok === true) {
        const model = await response.json();
        if (createRow) document.querySelector("tbody").append(row(Object.values(model)));
        return model;
    }
    else {
        const error = await response.json();
        console.log(error.message);
    }
}

async function editModel(modelsName, oldModel, createRow = true) {
    const response = await fetch(`/api/${modelsName}`, {
        method: "PUT",
        headers: { "Accept": "application/json", "Content-Type": "application/json" },
        body: oldModel
    });
    if (response.ok === true) {
        const model = await response.json();
        if (createRow) document.querySelector(`tr[data-rowid='${model.id}']`).replaceWith(row(Object.values(model)));
    }
    else {
        const error = await response.json();
        console.log(error.message);
    }
}

var forms = document.getElementsByClassName('needs-validation');
var validation = Array.prototype.filter.call(forms, function (form) {
    form.addEventListener('submit', function (event) {
        event.preventDefault();
        if (form.checkValidity() === false) {
            event.stopPropagation();
            form.classList.add('was-validated');
        }
    });
});