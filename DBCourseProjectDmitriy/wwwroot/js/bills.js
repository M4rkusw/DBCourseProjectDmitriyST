const apiName = "bill";

async function getBills() {
    const response = await fetch(`/api/${apiName}`, {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        const bills = await response.json();
        const rows = document.querySelector("tbody");
        bills.forEach(bill => {
            rows.append(row([
                bill.id,
                bill.date,
                bill.sum,
                bill.purchaseOrder.id,
                bill.purchaseOrder.contract.isSupply ? bill.purchaseOrder.contract.manufacturer.name : bill.purchaseOrder.contract.client.fullName,
                bill.purchaseOrder.contract.isSupply ? bill.purchaseOrder.contract.manufacturer.bankDetails : bill.purchaseOrder.contract.client.bankAccount,
            ]));
        });
    }
}

getBills();