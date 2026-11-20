const summary = document.querySelector("[data-purchase-summary]");
if (summary) {
    const form = summary.closest("form");
    const fields = summary.querySelectorAll("[data-summary]");
    const format = number => "JOD " + number.toLocaleString("en-JO", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    const update = () => {
        const read = name => Math.max(0, Number(form.elements.namedItem(name).value) || 0);
        const cost = read("UnitCost");
        const price = read("SellingPrice");
        const quantity = read("Quantity");
        const difference = price - cost;
        const values = { investment: format(cost * quantity), revenue: format(price * quantity),
            profit: format(difference * quantity), unit: format(difference),
            margin: (price ? difference / price * 100 : 0).toFixed(1) + "%" };
        for (const field of fields) field.textContent = values[field.dataset.summary];
    };
    form.addEventListener("input", update);
    update();
}
