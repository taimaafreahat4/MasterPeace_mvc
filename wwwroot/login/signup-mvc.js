const card = document.getElementById("signupCard");
const cardWrap = document.getElementById("cardWrap");
const passInput = document.getElementById("password");
const buyer = document.getElementById("buyer");
const seller = document.getElementById("seller");
const selectedRole = document.getElementById("selectedRole");
const roles = document.getElementById("roles");
const image = document.getElementById("image");
const preview = document.getElementById("preview");
let previewUrl;
window.lucide?.createIcons();

function setRole(isSeller) {
    selectedRole.value = String(isSeller);
    buyer.classList.toggle("active", !isSeller);
    seller.classList.toggle("active", isSeller);
    buyer.setAttribute("aria-pressed", String(!isSeller));
    seller.setAttribute("aria-pressed", String(isSeller));
    roles.style.setProperty("--i", isSeller ? 1 : 0);
}
buyer.addEventListener("click", () => setRole(false));
seller.addEventListener("click", () => setRole(true));
setRole(selectedRole.value === "true");

image.addEventListener("change", () => {
    const file = image.files[0];
    if (previewUrl) URL.revokeObjectURL(previewUrl);
    previewUrl = undefined;
    const valid = !file || (["image/png", "image/jpeg"].includes(file.type) && file.size <= 2097152);
    image.setCustomValidity(valid ? "" : "Choose a PNG or JPEG photo up to 2 MB.");
    if (file && valid) previewUrl = URL.createObjectURL(file);
    preview.src = previewUrl ?? "/components/logo.png";
    if (!valid) image.reportValidity();
});

document.querySelectorAll(".eye").forEach(btn => {

    btn.addEventListener("click", () => {

        const input  = document.getElementById(btn.dataset.target);
        const hidden = input.type === "password";

        input.type = hidden ? "text" : "password";
        btn.classList.toggle("on", hidden);

    });

});


/* =====================================================
   PASSWORD STRENGTH + MATCH
===================================================== */

const strength     = document.getElementById("strength");
const strengthText = document.getElementById("strengthText");

const LABELS = ["Use 12+ characters", "Weak", "Fair", "Good", "Strong"];

function scorePassword(p) {

    if (!p) return 0;

    let s = 0;

    if (p.length >= 12)  s++;
    if (p.length >= 10) s++;
    if (/[A-Z]/.test(p) && /[a-z]/.test(p)) s++;
    if (/\d/.test(p) && /[^A-Za-z0-9]/.test(p)) s++;
    else if (/\d/.test(p) && p.length >= 8) s++;

    return Math.max(p.length >= 12 ? 1 : 0, Math.min(s, 4));

}

passInput.addEventListener("input", () => {

    const s = scorePassword(passInput.value);

    strength.dataset.s = s;
    strengthText.textContent = LABELS[s];

    checkMatch();

});


if (window.matchMedia("(hover:hover)").matches) {

    document.addEventListener("mousemove", e => {

        const r  = cardWrap.getBoundingClientRect();
        const cx = r.left + r.width / 2;
        const cy = r.top + r.height / 2;

        const dx = (e.clientX - cx) / (window.innerWidth / 2);
        const dy = (e.clientY - cy) / (window.innerHeight / 2);

        card.style.transform =
            `perspective(1400px) rotateY(${dx * 3}deg) rotateX(${-dy * 2}deg)`;

        card.style.setProperty("--mx", ((e.clientX - r.left) / r.width * 100) + "%");
        card.style.setProperty("--my", ((e.clientY - r.top) / r.height * 100) + "%");

    });

    document.addEventListener("mouseleave", () => {
        card.style.transform = "";
    });

}
// ===============================
// Animated Network Background
// ===============================

const canvas = document.getElementById("bgCanvas");
const ctx = canvas.getContext("2d");

let points = [];

function resizeCanvas(){

    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;

}

resizeCanvas();
window.addEventListener("resize", resizeCanvas);



class Point{

    constructor(){

        this.x = Math.random() * canvas.width;
        this.y = Math.random() * canvas.height;

        this.vx = (Math.random()-0.5) * 0.35;
        this.vy = (Math.random()-0.5) * 0.35;

        this.size = Math.random()*2 + 2;

    }


    move(){

        this.x += this.vx;
        this.y += this.vy;


        if(this.x < 0 || this.x > canvas.width)
            this.vx *= -1;


        if(this.y < 0 || this.y > canvas.height)
            this.vy *= -1;

    }


    draw(){

        ctx.beginPath();

        ctx.arc(
            this.x,
            this.y,
            this.size,
            0,
            Math.PI*2
        );


        ctx.fillStyle =
        "rgba(190,170,255,0.8)";


        ctx.fill();

    }

}



function createPoints(){

    points=[];

    for(let i=0;i<45;i++){

        points.push(new Point());

    }

}


createPoints();



function drawLines(){

    for(let i=0;i<points.length;i++){

        for(let j=i+1;j<points.length;j++){


            let dx =
            points[i].x-points[j].x;


            let dy =
            points[i].y-points[j].y;


            let distance =
            Math.sqrt(dx*dx+dy*dy);



            if(distance < 150){


                ctx.beginPath();

                ctx.moveTo(
                    points[i].x,
                    points[i].y
                );


                ctx.lineTo(
                    points[j].x,
                    points[j].y
                );


                ctx.strokeStyle =
                "rgba(170,150,240,0.15)";


                ctx.lineWidth=1;

                ctx.stroke();


            }

        }

    }

}




function animateNetwork(){

    ctx.clearRect(
        0,
        0,
        canvas.width,
        canvas.height
    );


    points.forEach(point=>{

        point.move();
        point.draw();

    });


    drawLines();


    requestAnimationFrame(
        animateNetwork
    );

}


animateNetwork();
