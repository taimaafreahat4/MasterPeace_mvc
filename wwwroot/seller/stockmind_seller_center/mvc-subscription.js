const form = document.querySelector('#premium-checkout');

if (form) {
    const stages = Array.from(form.querySelectorAll('[data-step]'));
    const progress = Array.from(document.querySelectorAll('.checkout-steps li'));
    const plan = form.elements.PlanId;

    function show(step, focus = false) {
        stages.forEach((stage, index) => {
            stage.hidden = index !== step - 1;
            stage.classList.toggle('is-active', !stage.hidden);
        });
        progress.forEach((item, index) => {
            item.classList.toggle('is-active', index < step);
            if (index === step - 1) item.setAttribute('aria-current', 'step');
            else item.removeAttribute('aria-current');
        });
        if (focus) {
            const heading = stages[step - 1].querySelector('h2');
            heading.tabIndex = -1;
            heading.focus();
        }
    }

    function updateSummary() {
        const details = plan.selectedOptions[0].dataset;
        document.querySelectorAll('[data-summary]').forEach(item => {
            item.textContent = details[item.dataset.summary];
        });
    }

    form.querySelectorAll('[data-go]').forEach(button => {
        button.hidden = false;
        button.closest('.checkout-actions').hidden = false;
        button.addEventListener('click', () => {
            const step = Number(button.dataset.go);
            if (step === 3 && [...stages[1].querySelectorAll('input')].some(input => !input.reportValidity())) return;
            show(step, true);
        });
    });
    form.addEventListener('invalid', event => {
        const stage = event.target.closest('[data-step]');
        if (stage?.hidden) show(Number(stage.dataset.step));
    }, true);
    plan.addEventListener('change', updateSummary);
    updateSummary();
    show(form.dataset.invalid === 'true' ? 3 : 1);
}
