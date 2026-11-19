const feedback = [...document.querySelectorAll(".stock-feedback")];
const previous = document.getElementById("tPrev");
const next = document.getElementById("tNext");
let active = 0;

function showFeedback(direction) {
    feedback[active].hidden = true;
    active = (active + direction + feedback.length) % feedback.length;
    feedback[active].hidden = false;
}

if (feedback.length > 1) {
    previous.hidden = next.hidden = false;
    previous.addEventListener("click", () => showFeedback(-1));
    next.addEventListener("click", () => showFeedback(1));
}
