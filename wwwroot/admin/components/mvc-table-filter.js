const search = document.querySelector("[data-workspace-search]");
const filter = document.querySelector("[data-workspace-filter]");
const rows = [...document.querySelectorAll("[data-workspace-rows] tr")];
function showMatchingRows() {
    const query = search.value.trim().toLowerCase();
    const value = filter.value;
    for (const row of rows) {
        const matchesFilter = value === "all" || (row.dataset.filter || "").split(" ").includes(value);
        row.hidden = !matchesFilter || !row.textContent.toLowerCase().includes(query);
    }
}
search.addEventListener("input", showMatchingRows);
filter.addEventListener("change", showMatchingRows);
