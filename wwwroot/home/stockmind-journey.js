async function startJourney() {
    const root = document.querySelector('.sm-journey');
    const canvas = root?.querySelector('canvas');
    const pin = root?.querySelector('.sm-journey__pin');
    const reduceMotion = matchMedia('(prefers-reduced-motion: reduce)');
    if (!root || !canvas || !pin || reduceMotion.matches || matchMedia('(max-height: 500px)').matches || !window.WebGL2RenderingContext) return;

    const THREE = await import('/lib/three/three.module.min.js');
    if (reduceMotion.matches) return;

    const renderer = new THREE.WebGLRenderer({ canvas, antialias: false, alpha: true });
    renderer.toneMapping = THREE.ACESFilmicToneMapping;
    renderer.toneMappingExposure = 1.03;
    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(40, 1, .06, 90);
    const target = new THREE.Vector3();
    scene.fog = new THREE.FogExp2(0x53677c, .033);
    scene.add(new THREE.HemisphereLight(0xc4dbff, 0x627188, 1.48));
    const sun = new THREE.DirectionalLight(0xffd1a0, 2.25);
    sun.position.set(5, 9, -18);
    sun.target.position.set(0, 0, -4);
    sun.castShadow = true;
    sun.shadow.mapSize.set(1024, 1024);
    Object.assign(sun.shadow.camera, { left: -16, right: 16, top: 17, bottom: -17, near: .5, far: 48 });
    sun.shadow.camera.updateProjectionMatrix();
    sun.shadow.normalBias = .035;
    renderer.shadowMap.enabled = true;
    renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    renderer.shadowMap.autoUpdate = false;
    renderer.shadowMap.needsUpdate = true;
    scene.add(sun, sun.target);

    const material = (color, metalness = 0) => new THREE.MeshStandardMaterial({ color, roughness: .76, metalness });
    const concrete = material(0x7e8b98, .08), trim = material(0x576579), white = material(0x9baabb);
    const navy = material(0x26374e, .29), steel = material(0x586e85, .36);
    const kraft = material(0xb67c48), lightKraft = material(0xd6a26d), tape = material(0xe9be87);
    const wood = material(0x936d4d), violet = material(0x7858bb), yellow = material(0xf2b53f);
    const rubber = material(0x263141), screen = new THREE.MeshBasicMaterial({ color: 0x82b9df });
    const shadow = new THREE.MeshBasicMaterial({ color: 0xa0afc3 });
    function grain(seed, strength) {
        const canvas = document.createElement('canvas');
        canvas.width = canvas.height = 128;
        const ctx = canvas.getContext('2d');
        const pixels = ctx.createImageData(128, 128);
        for (let i = 0; i < pixels.data.length; i += 4) {
            seed = (Math.imul(seed, 1664525) + 1013904223) >>> 0;
            const value = 255 - (seed >>> 24) / 256 * strength;
            pixels.data[i] = pixels.data[i + 1] = pixels.data[i + 2] = value;
            pixels.data[i + 3] = 255;
        }
        ctx.putImageData(pixels, 0, 0);
        const texture = new THREE.CanvasTexture(canvas);
        texture.colorSpace = THREE.SRGBColorSpace;
        return texture;
    }

    concrete.map = grain(32, 25);
    concrete.roughness = .48;
    const paper = grain(56, 19);
    kraft.map = paper;
    lightKraft.map = paper;

    const geometry = new THREE.BoxGeometry(1, 1, 1);
    const transforms = new Map();
    const dummy = new THREE.Object3D();

    function box(paint, x, y, z, w, h, d) {
        dummy.position.set(x, y, z);
        dummy.scale.set(w, h, d);
        dummy.updateMatrix();
        if (!transforms.has(paint)) transforms.set(paint, []);
        transforms.get(paint).push(dummy.matrix.clone());
    }

    function movingBox(parent, paint, x, y, z, w, h, d) {
        const mesh = new THREE.Mesh(geometry, paint);
        mesh.position.set(x, y, z);
        mesh.scale.set(w, h, d);
        parent.add(mesh);
    }

    function carton(x, ground, z, w, h, d, light = false) {
        box(light ? lightKraft : kraft, x, ground + h / 2, z, w, h, d);
        box(tape, x, ground + h + .006, z, .075, .012, d);
        box(white, x + w * .15, ground + h * .65, z + d / 2 + .005, w * .28, h * .23, .012);
        for (let line = 0; line < 4; line++) {
            box(rubber, x + w * (.075 + line * .025), ground + h * .65, z + d / 2 + .013, w * .011, h * .13, .004);
        }
    }

    function pallet(x, z) {
        for (const side of [-1, 0, 1]) box(wood, x + side * .49, .06, z, .15, .12, 1.08);
        for (let slat = -2; slat <= 2; slat++) box(wood, x, .155, z + slat * .22, 1.44, .07, .16);
    }

    function sign(name, x, y, z, width) {
        const image = document.createElement('canvas');
        image.width = 384;
        image.height = 80;
        const ctx = image.getContext('2d');
        ctx.fillStyle = '#26344f';
        ctx.fillRect(0, 0, 384, 80);
        ctx.fillStyle = '#fff';
        ctx.font = 'bold 30px sans-serif';
        ctx.textBaseline = 'middle';
        ctx.textAlign = 'center';
        ctx.fillText(name, 192, 40);
        const map = new THREE.CanvasTexture(image);
        map.colorSpace = THREE.SRGBColorSpace;
        const mesh = new THREE.Mesh(new THREE.PlaneGeometry(width, width * .208), new THREE.MeshBasicMaterial({ map }));
        mesh.position.set(x, y, z);
        scene.add(mesh);
    }

    // The repeating trusses and skylights give the aisle a consistent vanishing point.
    const litPanel = new THREE.MeshBasicMaterial({ color: 0xf1f4ff });
    box(navy, 0, 6.05, -3.6, 17, .19, 23);
    for (let z = -13; z <= 8; z += 3.4) {
        box(steel, 0, 5.75, z, 16.7, .18, .25);
        for (const side of [-1, 1]) {
            box(navy, side * 7.98, 3, z, .23, 6, .24);
            box(navy, side * 4.1, 5.49, z, 2.5, .13, .4);
            box(litPanel, side * 4.1, 5.41, z, 2.3, .04, .27);
        }
        box(litPanel, 0, 5.927, z - 1.2, 2.4, .025, .85);
    }
    box(trim, 0, -.52, -3.2, 17.5, .4, 25.2);
    box(concrete, 0, -.16, -3.2, 17, .32, 24.8);
    box(concrete, 0, -.15, 10.8, 7.4, .3, 3.4);
    for (let x = -8; x <= 8; x += 2) box(trim, x, .003, -3.2, .012, .005, 24.2);
    for (let z = -15; z <= 9; z += 2) box(trim, 0, .003, z, 16.8, .005, .012);
    for (const side of [-1, 1]) {
        box(white, side * 8.3, 1.44, -3.7, .22, 2.88, 23.2);
        box(steel, side * 8.3, 2.9, -3.7, .34, .1, 23.2);
        for (let z = -13; z <= 7; z += 4) box(steel, side * 8.14, 1.42, z, .14, 2.84, .16);
        for (let z = -14; z <= 9; z += 1.35) box(yellow, side * 1.66, .01, z, .047, .012, .72);
    }
    box(white, 0, 1.6, -15.25, 16.8, 3.2, .24);
    const windowGlass = material(0xb8d5ec, .18);
    for (const x of [-5.6, -2.8, 2.8, 5.6]) {
        box(navy, x, 2.6, -15.1, 2.22, .68, .08);
        for (const dx of [-.52, .52]) box(windowGlass, x + dx, 2.6, -15.049, .96, .53, .023);
    }
    box(navy, 0, 1.25, -15.075, 2.75, 2.5, .12);
    box(steel, 0, 1.22, -14.997, 2.49, 2.35, .05);
    for (let slat = 1; slat <= 8; slat++) box(navy, 0, slat * .26, -14.965, 2.48, .023, .022);
    sign('04  DISPATCH', 0, 2.82, -14.96, 2.55);

    function doorway(z, name, street = false) {
        for (const side of [-1, 1]) {
            const height = street ? 1.02 : 2.22;
            box(white, side * 5.33, height / 2, z, street ? 5.87 : 6.5, height, .25);
            box(navy, side * (street ? 2.4 : 1.76), 1.68, z, .2, 3.36, .32);
            box(yellow, side * 2.03, .45, z + .28, .13, .9, .13);
        }
        box(navy, 0, 3.36, z, street ? 5 : 3.7, .22, .34);
        sign(name, 0, 3.64, z + .18, 2.62);
    }
    doorway(8.15, 'STOCK MIND', true);
    doorway(.25, '02  STORAGE');
    doorway(-7.05, '03  PACKING');

    const doors = [-1, 1].map(side => {
        const panel = new THREE.Group();
        scene.add(panel);
        movingBox(panel, steel, 0, 1.6, 8.18, 2.28, 3.18, .18);
        for (let row = 1; row < 11; row++) movingBox(panel, navy, 0, row * .28, 8.285, 2.26, .023, .028);
        return { side, panel };
    });

    // Receiving room: wrapped stock on pallets and a forklift beside the marked aisle.
    for (const x of [-5.6, 5.2]) for (const z of [3.7, 6.2]) {
        pallet(x, z);
        carton(x - .34, .2, z, .64, .57, .85, true);
        carton(x + .34, .2, z, .64, .57, .85);
        if (z > 5) carton(x, .78, z, .97, .49, .71);
    }
    const forklift = new THREE.Group();
    const carriage = new THREE.Group();
    const load = new THREE.Group();
    forklift.add(carriage);
    scene.add(forklift, load);
    movingBox(forklift, yellow, 0, .49, 0, 1.12, .64, 1.43);
    movingBox(forklift, rubber, 0, .85, .22, .55, .16, .53);
    movingBox(forklift, steel, 0, 1.77, .2, 1.08, .13, 1.18);
    const beacon = new THREE.MeshBasicMaterial({ color: 0xffb12e });
    movingBox(forklift, beacon, .34, 1.89, .32, .14, .13, .14);
    const tire = new THREE.CylinderGeometry(.26, .26, .19, 14);
    for (const side of [-1, 1]) {
        movingBox(forklift, navy, side * .45, 1.24, .56, .085, 1.07, .085);
        movingBox(forklift, steel, side * .39, 1.08, -.72, .085, 2.03, .095);
        movingBox(carriage, navy, side * .32, .18, -.82, .095, .34, .11);
        movingBox(carriage, steel, side * .32, .093, -1.27, .095, .07, .98);
        for (const z of [-.49, .49]) {
            const wheel = new THREE.Mesh(tire, rubber);
            wheel.rotation.z = Math.PI / 2;
            wheel.position.set(side * .64, .26, z);
            forklift.add(wheel);
        }
        movingBox(load, wood, side * .4, .08, 0, .15, .16, .95);
        movingBox(load, lightKraft, side * .27, .42, 0, .49, .5, .7);
        movingBox(load, tape, side * .27, .673, 0, .05, .012, .71);
        movingBox(load, white, side * .27 + .11, .48, .356, .12, .08, .012);
    }
    movingBox(load, wood, 0, .17, 0, 1.14, .065, .94);

    for (const side of [-1, 1]) {
        const x = side * 6.8;
        for (const dx of [-.68, .68]) for (const z of [1.35, 6.9]) box(navy, x + dx, 2.19, z, .09, 4.38, .09);
        for (const y of [1.68, 2.62, 3.56]) {
            box(steel, x, y, 4.13, 1.42, .11, 5.63);
            box(yellow, x - side * .72, y, 4.13, .05, .14, 5.63);
            for (const z of [1.85, 2.84, 3.83, 4.82, 5.81, 6.55]) carton(x, y + .06, z, 1.08, .61, .66, z > 4);
        }
    }

    const aisleBeam = material(0x99552c, .27);
    const looseBox = new THREE.Group();
    movingBox(looseBox, lightKraft, 0, 0, 0, .64, .46, .59);
    movingBox(looseBox, tape, 0, .235, 0, .085, .012, .591);
    movingBox(looseBox, white, -.13, .055, .3, .19, .105, .012);
    scene.add(looseBox);
    const impactShadow = new THREE.Mesh(new THREE.CircleGeometry(.43, 24),
        new THREE.MeshBasicMaterial({ color: 0x101a23, transparent: true, opacity: 0, depthWrite: false }));
    impactShadow.rotation.x = -Math.PI / 2;
    scene.add(impactShadow);

    const grit = new Float32Array(30 * 3);
    const gritGeometry = new THREE.BufferGeometry();
    const gritPositions = new THREE.BufferAttribute(grit, 3).setUsage(THREE.DynamicDrawUsage);
    gritGeometry.setAttribute('position', gritPositions);
    const dust = new THREE.Points(gritGeometry,
        new THREE.PointsMaterial({ color: 0xb59a79, size: .026, transparent: true, depthWrite: false }));
    dust.frustumCulled = false;
    scene.add(dust);
    let fallStarted = null;

    function updateFallingBox(progress, seconds) {
        if (progress < .51) fallStarted = null;
        else if (fallStarted === null) fallStarted = seconds;
        // Holding on Storage plays the drop; scrubbing forward catches the same landing.
        const scrollTime = THREE.MathUtils.clamp((progress - .57) * 13, 0, 2);
        const time = Math.max(scrollTime, fallStarted === null ? 0 : seconds - fallStarted);
        const slide = THREE.MathUtils.smoothstep(time, 0, .52);
        const drop = THREE.MathUtils.clamp((time - .52) / .64, 0, 1);
        const bounce = THREE.MathUtils.clamp((time - 1.16) / .36, 0, 1);
        const x = 2.9 - .82 * slide - .52 * drop - .12 * bounce;
        const z = -3.73 + .21 * drop + .08 * bounce;
        const roll = slide * .18 + drop * 1.39 - bounce * Math.PI / 2;
        const contactHeight = .23 * Math.abs(Math.cos(roll)) + .32 * Math.abs(Math.sin(roll));
        const y = Math.max(contactHeight, 2.33 - 2.1 * drop * drop)
            + Math.sin(Math.PI * bounce) * .13;
        looseBox.position.set(x, y, z);
        looseBox.rotation.set(0, slide * .18 + drop * .42, roll);
        impactShadow.position.set(x, .018, z);
        impactShadow.material.opacity = THREE.MathUtils.lerp(.045, .26, drop);
        impactShadow.scale.setScalar(1.3 - drop * .42);

        const age = time - 1.16;
        dust.visible = age >= 0 && age <= .56;
        if (!dust.visible) return;
        dust.material.opacity = .44 * (1 - age / .56);
        for (let i = 0; i < gritPositions.count; i++) {
            const angle = i * 2.39996;
            const spread = age * (.35 + i % 7 * .12);
            gritPositions.setXYZ(i, 1.56 + Math.cos(angle) * spread,
                .02 + Math.max(0, age * (.43 + i % 4 * .16) - age * age * .95),
                -3.52 + Math.sin(angle) * spread);
        }
        gritPositions.needsUpdate = true;
    }

    // Storage room: four racks, with separate shelves, crates, tape and product labels.
    for (const x of [-6.4, -3.15, 3.15, 6.4]) {
        box(shadow, x, .006, -3.45, 1.82, .006, 5.12);
        for (const dx of [-.74, .74]) for (const z of [-.95, -3.45, -5.95]) box(navy, x + dx, 2.35, z, .086, 4.7, .086);
        for (const y of [.22, 1.13, 2.04, 2.95, 3.86]) {
            box(steel, x, y, -3.45, 1.57, .11, 5.12);
            box(aisleBeam, x - Math.sign(x) * .8, y, -3.45, .041, .14, 5.1);
            for (let slot = 0; slot < 6; slot++) {
                const z = -1.36 - slot * .79;
                if (x === 3.15 && y === 2.04 && slot === 3) continue;
                if ((slot + Math.round(y)) % 4 === 0) box(violet, x, y + .27, z, 1.16, .42, .64);
                else carton(x, y + .06, z, 1.16, .48 + slot % 2 * .13, .66, slot % 2 === 0);
            }
        }
    }

    // Packing room: workbenches with terminals and a parcel that moves across the rollers.
    for (const x of [-4.45, 4.45]) {
        box(shadow, x, .006, -10.17, 3.57, .006, 1.64);
        box(white, x, 1.03, -10.17, 3.47, .14, 1.22);
        for (const dx of [-1.52, 1.52]) for (const dz of [-.43, .43]) box(navy, x + dx, .5, -10.17 + dz, .09, 1.02, .09);
        box(navy, x - .9, 1.42, -10.52, .68, .42, .1);
        box(screen, x - .9, 1.42, -10.46, .56, .32, .024);
        carton(x + .62, 1.106, -10.17, .72, .45, .58, true);
        pallet(x, -13.8);
        carton(x, .2, -13.8, 1.2, .68, .85);
    }
    box(navy, 0, .74, -13.3, 5.95, .22, .99);
    for (const z of [-13.78, -12.82]) box(navy, 0, .88, z, 6, .12, .08);
    for (let x = -2.83; x <= 2.83; x += .18) box(steel, x, .85, -13.3, .103, .11, .91);
    for (const x of [-2.45, 2.45]) box(navy, x, .36, -13.3, .14, .72, .78);
    const parcels = Array.from({ length: 3 }, (_, i) => {
        const parcel = new THREE.Group();
        const width = .58 + i * .085;
        movingBox(parcel, i === 1 ? kraft : lightKraft, 0, 0, 0, width, .44, .62);
        movingBox(parcel, tape, 0, .225, 0, .085, .01, .62);
        movingBox(parcel, white, .13, .065, .316, .19, .12, .01);
        scene.add(parcel);
        return parcel;
    });
    for (const x of [-2.96, 2.96]) {
        box(navy, x, 1.13, -13.3, .82, .73, 1);
        box(violet, x, 1.52, -12.79, .45, .035, .026);
    }

    const robotOrange = material(0xb7612b, .24);
    const trackingLight = new THREE.MeshBasicMaterial({ color: 0x84ddff });
    const scanLight = new THREE.MeshBasicMaterial({ color: 0x63c9ff, transparent: true, opacity: .7, depthWrite: false });
    const wire = new THREE.EdgesGeometry(geometry);
    const scanFrame = new THREE.LineSegments(wire, new THREE.LineBasicMaterial({ color: 0xa5e8ff, transparent: true, opacity: .55 }));
    scanFrame.scale.set(1.25, .85, .95);
    scanFrame.position.set(2.96, 1.18, 6.8);
    scene.add(scanFrame);
    box(navy, 2.96, .68, 6.8, 1.85, .16, .96);
    box(steel, 2.96, 1.12, 6.8, 1.18, .7, .87);

    function createArm(x, z, heading) {
        const base = new THREE.Group();
        base.position.set(x, .19, z);
        scene.add(base);
        movingBox(base, navy, 0, .2, 0, .56, .44, .57);
        const shoulder = new THREE.Group();
        shoulder.position.y = .51;
        shoulder.rotation.y = heading;
        base.add(shoulder);
        movingBox(shoulder, robotOrange, 0, .47, 0, .21, .96, .22);
        movingBox(shoulder, navy, 0, .94, 0, .26, .27, .28);
        const elbow = new THREE.Group();
        elbow.position.y = .94;
        shoulder.add(elbow);
        movingBox(elbow, robotOrange, 0, .35, 0, .16, .72, .17);
        movingBox(elbow, navy, 0, .72, 0, .22, .16, .24);
        for (const side of [-1, 1]) movingBox(elbow, steel, side * .07, .8, 0, .033, .13, .12);
        return { shoulder, elbow };
    }

    const arms = [createArm(2.1, 6.9, 0), createArm(3.87, 6.62, Math.PI)];
    const carts = [-1, 1].map(side => {
        const cart = new THREE.Group();
        movingBox(cart, navy, 0, .105, 0, .76, .2, .99);
        movingBox(cart, yellow, 0, .22, 0, .82, .13, 1.02);
        movingBox(cart, trackingLight, 0, .215, .519, .42, .035, .012);
        movingBox(cart, lightKraft, 0, .5, 0, .64, .43, .69);
        movingBox(cart, tape, 0, .721, 0, .066, .009, .7);
        scene.add(cart);
        const outline = new THREE.LineSegments(wire, new THREE.LineBasicMaterial({
            color: 0x8ddfff, transparent: true, opacity: .5, depthWrite: false
        }));
        outline.scale.set(.68, .46, .73);
        outline.position.y = .5;
        cart.add(outline);
        return { side, cart, outline };
    });

    const scanner = new THREE.Group();
    const scanStripe = new THREE.Mesh(geometry, scanLight);
    scanStripe.scale.set(4.2, .016, .032);
    scanner.add(scanStripe);
    scene.add(scanner);

    function packingRobot(x, z) {
        const robot = new THREE.Group();
        robot.position.set(x, 0, z);
        robot.rotation.y = -.25;
        scene.add(robot);
        for (const side of [-1, 1]) {
            movingBox(robot, navy, side * .16, .12, .05, .23, .18, .43);
            movingBox(robot, robotOrange, side * .15, .51, 0, .18, .65, .21);
        }
        movingBox(robot, robotOrange, 0, 1.08, 0, .54, .54, .31);
        movingBox(robot, navy, 0, 1.4, 0, .16, .19, .16);
        const helmet = new THREE.Mesh(new THREE.SphereGeometry(.195, 16, 12), robotOrange);
        helmet.position.y = 1.58;
        helmet.scale.set(.93, 1.13, .88);
        robot.add(helmet);
        movingBox(robot, navy, 0, 1.59, .141, .27, .22, .085);
        movingBox(robot, trackingLight, 0, 1.61, .188, .14, .018, .008);
        return [-1, 1].map(side => {
            const shoulder = new THREE.Group();
            shoulder.position.set(side * .35, 1.29, 0);
            robot.add(shoulder);
            movingBox(shoulder, robotOrange, 0, -.153, 0, .16, .31, .16);
            const forearm = new THREE.Group();
            forearm.position.y = -.31;
            shoulder.add(forearm);
            movingBox(forearm, robotOrange, 0, -.13, .01, .14, .27, .16);
            movingBox(forearm, navy, 0, -.29, .025, .13, .09, .13);
            return { shoulder, forearm };
        });
    }

    const sorters = [packingRobot(3.35, -11.0), packingRobot(5.0, -11.1), packingRobot(-4.1, -11.2)];

    function updateAutomation(progress, seconds) {
        const motion = seconds * 1.6 + progress * 14;
        scanFrame.material.opacity = .35 + .25 * Math.sin(motion);
        for (const [i, { shoulder, elbow }] of arms.entries()) {
            const reach = .5 + .5 * Math.sin(motion + i * 1.6);
            shoulder.rotation.z = (i === 0 ? -1 : 1) * (.23 + reach * .63);
            elbow.rotation.z = (i === 0 ? -1 : 1) * (.45 + reach * .7);
        }
        for (const { side, cart, outline } of carts) {
            const t = seconds * .54 + progress * 5 + side;
            cart.position.set(side * 1.16, 0, -3.65 + Math.sin(t) * 1.87);
            cart.rotation.y = side === 1 ? 0 : Math.PI;
            outline.visible = progress > .46 && progress < .73;
        }
        scanner.visible = progress > .48 && progress < .7;
        scanner.position.set(0, .022, -3.5 + Math.sin(motion * .83) * 2.65);
        scanLight.opacity = .13 + .29 * Math.max(0, Math.sin(motion));
        for (const [i, pair] of sorters.entries()) for (const [j, { shoulder, forearm }] of pair.entries()) {
            const reach = .5 + .5 * Math.sin(motion + i * 1.7 + j * .7);
            shoulder.rotation.x = -.36 - reach * .72;
            forearm.rotation.x = -.62 - reach * .54;
        }
    }

    for (const [paint, matrices] of transforms) {
        const batch = new THREE.InstancedMesh(geometry, paint, matrices.length);
        matrices.forEach((matrix, i) => batch.setMatrixAt(i, matrix));
        batch.castShadow = paint !== navy && paint !== concrete && paint !== shadow;
        batch.receiveShadow = paint !== litPanel;
        batch.instanceMatrix.needsUpdate = true;
        scene.add(batch);
    }

    const stages = [...root.querySelectorAll('[data-scene]')];
    const chapters = [...root.querySelectorAll('[data-tour-step]')];
    const progressBar = root.querySelector('.sm-journey__progress i');
    const chapterStarts = [0, .13, .35, .49, .72, .91];
    const chapterStops = [0, .27, .41, .58, .84, 1];
    let activeStage = -1;
    function setStage(index) {
        if (index === activeStage) return;
        stages.forEach((stage, i) => {
            stage.classList.toggle('is-active', i === index);
            stage.inert = i !== index;
        });
        chapters.forEach((chapter, i) => {
            if (i === index) chapter.setAttribute('aria-current', 'step');
            else chapter.removeAttribute('aria-current');
        });
        activeStage = index;
    }

    // Each shot ends before the next doorway so the camera has a clear path.
    const shots = [
        [0, -.4, 2.15, 13.8, .2, 1.65, -8],
        [.055, -.8, 4.62, 13.2, 1.36, 1.48, 4.1],
        [.14, -.5, 3.62, 11.6, 1.65, 1.39, 6.55],
        [.26, 0, 2.36, 9.46, 1.15, 1.38, 5.9],
        [.36, 0, 2.01, 6.65, 1.5, 1.12, 2.9],
        [.41, 0, 2.08, 7, 3.5, 1.15, 3.8],
        [.49, 0, 2.25, 1.65, .3, 1.16, -4],
        [.54, 0, 2.06, -.75, 1.1, 1.6, -4.9],
        [.60, 0, 1.75, -1.25, 1.7, 1.17, -3.76],
        [.65, -.27, 1.68, -2.3, 1.28, .88, -3.65],
        [.72, 0, 2.22, -5.8, .45, 1.1, -10.9],
        [.79, -.38, 2.39, -7.6, 2.7, 1.19, -10.72],
        [.85, .75, 2.16, -8.92, 3.5, 1.28, -11.05],
        [1, 1.05, 2.22, -10.7, .7, 1.21, -14.25]
    ];
    const lerp = THREE.MathUtils.lerp;
    function updateStory(progress, seconds) {
        const next = Math.max(1, shots.findIndex(shot => shot[0] >= progress));
        const from = shots[next - 1];
        const to = shots[next];
        const blend = THREE.MathUtils.smoothstep(progress, from[0], to[0]);
        const at = i => lerp(from[i], to[i], blend);
        const intro = THREE.MathUtils.smoothstep(seconds, 0, 1.25);
        camera.position.set(at(1), at(2), at(3) + (1 - intro) * (1 - THREE.MathUtils.smoothstep(progress, 0, .13)) * .95);
        target.set(at(4), at(5), at(6));
        camera.lookAt(target);
        const opened = Math.max(intro * .83, THREE.MathUtils.smoothstep(progress, .13, .32));
        for (const { side, panel } of doors) panel.position.x = side * (1.15 + opened * 2.25);

        const cycle = seconds % 18;
        const smooth = THREE.MathUtils.smoothstep;
        const outward = smooth(cycle, 2.4, 7.1);
        const returning = smooth(cycle, 11.4, 16.1);
        const raised = smooth(cycle, 1, 2.4) - smooth(cycle, 7.2, 8.6)
            + smooth(cycle, 10, 11.4) - smooth(cycle, 16.2, 17.6);
        forklift.position.set(4.05, 0, 5.1 - 2.45 * (outward - returning));
        carriage.position.y = raised * .54;
        load.position.set(forklift.position.x, carriage.position.y, forklift.position.z - 1.26);
        beacon.color.setHex(seconds % 1.6 < .32 ? 0xffd387 : 0xab6a24);
        parcels.forEach((parcel, index) => {
            const phase = (seconds * .41 + index * 2.1) % 6.18;
            parcel.position.set(phase - 3.09, 1.125, -13.3);
            parcel.visible = phase > .17 && phase < 6.01;
        });
        updateAutomation(progress, seconds);
        updateFallingBox(progress, seconds);
        progressBar.style.transform = 'scaleX(' + progress + ')';
        setStage(chapterStarts.findLastIndex(start => progress >= start));
    }

    const nav = document.querySelector('.stock-nav');
    let frame = 0;
    let previousProgress = -1;
    let seconds = 0;
    let previousTick = 0;
    let previousWidth = 0;
    let previousHeight = 0;
    let navOffset = 64;
    let visible = true;
    let stopped = false;

    function scheduleFrame() {
        if (!frame && visible && !stopped && !document.hidden && !reduceMotion.matches) {
            frame = requestAnimationFrame(render);
        }
    }

    function render(now) {
        frame = 0;
        if (!visible || stopped || document.hidden || reduceMotion.matches) return;
        if (previousTick && now - previousTick < 30) {
            scheduleFrame();
            return;
        }
        if (previousTick) seconds += Math.min(now - previousTick, 45) / 1000;
        previousTick = now;
        const nextNavOffset = Math.round(nav?.getBoundingClientRect().height || 64);
        if (nextNavOffset !== navOffset) {
            navOffset = nextNavOffset;
            root.style.setProperty('--sm-nav-height', navOffset + 'px');
        }
        const width = canvas.clientWidth;
        const height = canvas.clientHeight;
        const resized = width !== previousWidth || height !== previousHeight;
        if (resized && width && height) {
            previousWidth = width;
            previousHeight = height;
            const pixelBudget = 900000;
            renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 1.35, Math.sqrt(pixelBudget / (width * height))));
            renderer.setSize(width, height, false);
            camera.aspect = width / height;
            const narrow = width <= 710;
            camera.setViewOffset(width, height, narrow ? 0 : -width * .155, narrow ? height * .18 : 0, width, height);
        }

        const distance = Math.max(1, root.offsetHeight - pin.offsetHeight);
        const progress = THREE.MathUtils.clamp((navOffset - root.getBoundingClientRect().top) / distance, 0, 1);
        const eased = previousProgress < 0 ? progress : lerp(previousProgress, progress, .28);
        updateStory(Math.abs(progress - eased) < .0005 ? progress : eased, seconds);
        previousProgress = eased;
        const inside = eased > .035;
        try {
            if (inside || !root.classList.contains('sm-journey--drawn')) renderer.render(scene, camera);
            root.classList.toggle('sm-journey--inside', inside);
            root.classList.add('sm-journey--drawn');
            if (inside || Math.abs(progress - eased) > .0005) scheduleFrame();
        } catch (error) {
            stop();
            console.warn('Stock Mind is showing its static warehouse image.', error);
        }
    }

    const viewport = new IntersectionObserver(([entry]) => {
        visible = entry.isIntersecting;
        previousTick = 0;
        if (visible) scheduleFrame();
    });
    const layout = new ResizeObserver(scheduleFrame);

    function stop() {
        if (stopped) return;
        stopped = true;
        cancelAnimationFrame(frame);
        root.classList.remove('sm-journey--ready', 'sm-journey--drawn', 'sm-journey--inside');
        setStage(0);
        viewport.disconnect();
        layout.disconnect();
        root.removeEventListener('click', goToChapter);
        window.removeEventListener('scroll', scheduleFrame);
        window.removeEventListener('pagehide', onPageHide);
        document.removeEventListener('visibilitychange', scheduleFrame);
        reduceMotion.removeEventListener('change', onMotionChange);
        canvas.removeEventListener('webglcontextlost', stop);
        sun.shadow.dispose();
        renderer.dispose();
        const resources = new Set();
        scene.traverse(({ geometry, material }) => {
            if (geometry) resources.add(geometry);
            for (const item of (Array.isArray(material) ? material : [material])) {
                if (!item) continue;
                if (item.map) resources.add(item.map);
                resources.add(item);
            }
        });
        resources.forEach(resource => resource.dispose());
    }

    function goToChapter(event) {
        const button = event.target.closest('[data-tour-step]');
        if (!button) return;
        const progress = chapterStops[Number(button.dataset.tourStep)];
        const distance = root.offsetHeight - pin.offsetHeight;
        window.scrollTo({
            top: window.scrollY + root.getBoundingClientRect().top - navOffset + distance * progress,
            behavior: 'smooth'
        });
    }

    function onPageHide(event) {
        if (!event.persisted) stop();
    }

    function onMotionChange() {
        if (reduceMotion.matches) stop();
    }

    root.classList.add('sm-journey--ready');
    root.addEventListener('click', goToChapter);
    viewport.observe(pin);
    layout.observe(pin);
    if (nav) layout.observe(nav);
    canvas.addEventListener('webglcontextlost', stop);
    window.addEventListener('scroll', scheduleFrame, { passive: true });
    window.addEventListener('pagehide', onPageHide);
    document.addEventListener('visibilitychange', scheduleFrame);
    reduceMotion.addEventListener('change', onMotionChange);
    scheduleFrame();
}

startJourney().catch(error => console.warn('Stock Mind is showing its static warehouse image.', error));
