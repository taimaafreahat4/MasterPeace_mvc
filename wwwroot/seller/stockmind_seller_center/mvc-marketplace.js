const listingForm = document.querySelector('#market-product-form');
if (listingForm) {
    const method = listingForm.elements.WholeLot;
    const minimum = listingForm.elements.Minimum;
    const quantity = listingForm.elements.Quantity;
    const upload = listingForm.elements.Photo;
    const url = listingForm.elements.ImageUrl;
    const preview = listingForm.querySelector('.market-image-preview');
    const image = preview.querySelector('img');
    const placeholder = preview.querySelector('span');
    let objectUrl;

    function updateMethod() {
        const whole = method.value === 'true';
        minimum.disabled = whole;
        minimum.closest('label').hidden = whole;
        quantity.min = whole ? '1' : '0';
        const lotUnits = listingForm.elements.LotUnits;
        if (lotUnits) {
            lotUnits.disabled = !whole;
            lotUnits.closest('label').hidden = !whole;
        }
        listingForm.querySelector('#market-price-label').textContent = whole ? 'Price for the entire lot (JOD) *' : 'Price per unit (JOD) *';
        listingForm.querySelector('#market-quantity-label').textContent = whole && !lotUnits ? 'Units in the lot *' : 'Available quantity *';
    }

    function updatePhoto() {
        if (objectUrl) URL.revokeObjectURL(objectUrl);
        const file = upload.files[0];
        objectUrl = file && file.size <= 5 * 1024 * 1024 && /^image\/(png|jpeg|webp|gif)$/.test(file.type)
            ? URL.createObjectURL(file) : undefined;
        const link = url.value.trim();
        const source = objectUrl || (/^https:\/\//i.test(link) ? link : preview.dataset.currentPhoto);
        if (source) image.src = source;
        else image.removeAttribute('src');
        image.hidden = !source;
        placeholder.hidden = Boolean(source);
    }

    method.addEventListener('change', updateMethod);
    upload.addEventListener('change', updatePhoto);
    url.addEventListener('change', updatePhoto);
    image.addEventListener('error', () => {
        image.hidden = true;
        placeholder.hidden = false;
    });
    listingForm.addEventListener('reset', () => setTimeout(() => {
        updateMethod();
        updatePhoto();
    }, 0));
    updateMethod();
}

document.querySelectorAll('[data-confirm-archive]').forEach(form => {
    form.addEventListener('submit', event => {
        if (!window.confirm('Archive this listing? It will be removed from the marketplace.')) event.preventDefault();
    });
});
