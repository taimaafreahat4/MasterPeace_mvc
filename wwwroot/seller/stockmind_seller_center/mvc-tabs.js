const sellerNavigation = document.querySelector(".sm-seller-tabs");
const sellerToggle = sellerNavigation?.querySelector(".sm-seller-tabs__toggle");
sellerToggle?.addEventListener("click", () => {
    const collapsed = sellerNavigation.classList.toggle("is-collapsed");
    sellerToggle.setAttribute("aria-expanded", String(!collapsed));
});
