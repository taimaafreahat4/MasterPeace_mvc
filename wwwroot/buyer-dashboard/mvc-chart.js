const canvas = document.getElementById("spendingChart");
if (canvas) {
    const monthly = JSON.parse(canvas.dataset.monthly);
    const context = canvas.getContext("2d");
    function draw() {
        const width = Math.max(240, canvas.parentElement.clientWidth);
        const height = 210;
        const scale = window.devicePixelRatio || 1;
        canvas.width = width * scale;
        canvas.height = height * scale;
        canvas.style.height = height + "px";
        context.scale(scale, scale);
        const maximum = Math.max(1, ...monthly.map(item => item.total));
        const baseline = height - 28;
        const slot = (width - 36) / monthly.length;
        context.font = "12px sans-serif";
        context.textAlign = "center";
        monthly.forEach((item, index) => {
            const x = 18 + slot * index;
            const barHeight = item.total / maximum * (baseline - 18);
            context.fillStyle = "#7c3aed";
            context.fillRect(x + slot * .22, baseline - barHeight, slot * .56, barHeight);
            context.fillStyle = "#61708d";
            context.fillText(item.label, x + slot / 2, baseline + 18);
        });
    }
    draw();
    window.addEventListener("resize", draw);
}
