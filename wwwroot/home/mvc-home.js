(() => {
    "use strict";

    const stage = document.getElementById("tStage");
    const track = document.getElementById("tTrack");
    const dotsBox = document.getElementById("tDots");
    if (!stage || !track || !dotsBox) return;

    const hideControls = () => {
        stage.querySelectorAll(".t-arrow, .t-arc").forEach(control => { control.hidden = true; });
        dotsBox.hidden = true;
    };

    const cards = Array.from(track.children);
    const count = cards.length;
    if (!count) return;
    const STEP = 0.36;
    const RADIUS = 600;
    const DELAY = 4200;
    const EASE = 0.075;
    const DRAG = 240;
    const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    const submitted = new URLSearchParams(location.search).get("feedback");
    const selected = submitted ? cards.findIndex(card => card.dataset.feedbackId === submitted) : -1;
    const initial = Math.max(0, selected);
    let pos = initial, target = initial, spread = 0, spreadTarget = 0;
    let visible = false, hovering = false, dragging = false;
    let startX = 0, startPos = 0, moved = 0, activeIdx = -1, timer = null;
    const wrap = d => ((d + count / 2) % count + count) % count - count / 2;

    const dots = cards.map((card, i) => {
        const button = document.createElement("button");
        button.type = "button";
        button.setAttribute("aria-label", `Show feedback ${i + 1} by ${card.querySelector(".t-name").textContent}`);
        button.addEventListener("click", () => {
            const base = Math.round(target);
            target = base + wrap(i - base);
            restart();
        });
        dotsBox.appendChild(button);
        return button;
    });

    function restart() {
        clearInterval(timer);
        if (reduceMotion || count < 2) return;
        timer = setInterval(() => {
            if (visible && !hovering && !dragging) target = Math.round(target) + 1;
        }, DELAY);
    }

    document.getElementById("tPrev")?.addEventListener("click", () => { target = Math.round(target) - 1; restart(); });
    document.getElementById("tNext")?.addEventListener("click", () => { target = Math.round(target) + 1; restart(); });
    if (count < 2) hideControls();

    cards.forEach((card, i) => card.addEventListener("click", () => {
        if (moved > 6) return;
        const base = Math.round(target);
        const distance = wrap(i - base);
        if (distance !== 0) { target = base + distance; restart(); }
    }));
    stage.addEventListener("mouseenter", () => { hovering = true; });
    stage.addEventListener("mouseleave", () => { hovering = false; });
    stage.addEventListener("pointerdown", event => {
        if (event.target.closest(".t-arrow") || count < 2) return;
        dragging = true; startX = event.clientX; startPos = pos; moved = 0;
        stage.classList.add("dragging");
    });
    window.addEventListener("pointermove", event => {
        if (!dragging) return;
        const dx = event.clientX - startX;
        moved = Math.max(moved, Math.abs(dx));
        pos = startPos - dx / DRAG;
        target = pos;
    });
    window.addEventListener("pointerup", () => {
        if (!dragging) return;
        dragging = false; stage.classList.remove("dragging"); target = Math.round(pos); restart();
    });

    const reveal = entries => {
        visible = entries[0].isIntersecting;
        if (visible) spreadTarget = 1;
    };
    if ("IntersectionObserver" in window) new IntersectionObserver(reveal, { threshold: 0.25 }).observe(stage);
    else { visible = true; spreadTarget = 1; }

    function render(time) {
        const kx = Math.min(1, stage.clientWidth / 1180);
        let nearest = 0, best = Infinity;
        for (let i = 0; i < count; i++) {
            const distance = wrap(i - pos);
            const absDistance = Math.abs(distance);
            const angle = distance * STEP * spread;
            const x = Math.sin(angle) * RADIUS * kx;
            const y = (1 - Math.cos(angle)) * RADIUS;
            const rotation = angle * 40.1;
            const scale = 1 - Math.min(absDistance, 2.6) * 0.115 * spread;
            const bob = reduceMotion ? 0 : Math.sin(time / 650 + i * 1.7) * 6 * Math.max(0, 1 - absDistance);
            const fade = Math.max(0, Math.min(1, (2.6 - absDistance) / 0.8)) * (1 - 0.16 * Math.min(absDistance, 2));
            cards[i].style.transform = `translate3d(${x.toFixed(1)}px, ${(y + bob).toFixed(1)}px, 0) rotate(${rotation.toFixed(2)}deg) scale(${scale.toFixed(3)})`;
            cards[i].style.opacity = (fade * Math.min(1, spread * 2)).toFixed(3);
            cards[i].style.zIndex = 100 - Math.round(absDistance * 10);
            cards[i].style.pointerEvents = absDistance > 2.4 ? "none" : "auto";
            if (absDistance < best) { best = absDistance; nearest = i; }
        }
        if (nearest !== activeIdx) {
            activeIdx = nearest;
            cards.forEach((card, i) => card.classList.toggle("active", i === nearest));
            dots.forEach((dot, i) => dot.classList.toggle("active", i === nearest));
        }
    }

    function frame(time) {
        if (visible) {
            if (!dragging) pos += (target - pos) * EASE;
            spread += (spreadTarget - spread) * 0.06;
            render(time);
        }
        requestAnimationFrame(frame);
    }

    if (selected >= 0 || location.hash === "#testimonials") {
        const revealFeedback = () => stage.closest("section").scrollIntoView({ behavior: "instant", block: "start" });
        if (document.readyState === "complete") revealFeedback();
        else window.addEventListener("load", revealFeedback, { once: true });
    }

    render(0);
    restart();
    requestAnimationFrame(frame);
})();


