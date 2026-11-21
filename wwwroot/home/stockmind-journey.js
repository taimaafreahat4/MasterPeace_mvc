async function startJourney() {
    const root = document.querySelector('.sm-journey');
    const canvas = root?.querySelector('canvas');
    const pin = root?.querySelector('.sm-journey__pin');
    const reduceMotion = matchMedia('(prefers-reduced-motion: reduce)');
    if (!root || !canvas || !pin || reduceMotion.matches || !window.WebGL2RenderingContext) return;

    const THREE = await import('/lib/three/three.module.min.js');
    if (reduceMotion.matches) return;

    const renderer = new THREE.WebGLRenderer({ canvas, antialias: false, alpha: true });
    renderer.toneMapping = THREE.ACESFilmicToneMapping;
    renderer.toneMappingExposure = 1.22;

    const scene = new THREE.Scene();
    scene.fog = new THREE.FogExp2(0x19294d, 0.038);
    const camera = new THREE.PerspectiveCamera(37, 1, 0.1, 100);
    const look = new THREE.Vector3(0, 0.8, 0);
    camera.position.set(0, 5.3, 12.8);
    camera.lookAt(look);

    const materials = {
        floor: new THREE.MeshStandardMaterial({ color: 0x263755, roughness: .8, metalness: .25 }),
        steel: new THREE.MeshStandardMaterial({ color: 0x526281, metalness: .76, roughness: .3 }),
        darkSteel: new THREE.MeshStandardMaterial({ color: 0x293650, metalness: .62, roughness: .4 }),
        belt: new THREE.MeshStandardMaterial({ color: 0x30384d, metalness: .18, roughness: .68 }),
        cardboard: new THREE.MeshStandardMaterial({ color: 0x9a6844, roughness: .92 }),
        cardboardLight: new THREE.MeshStandardMaterial({ color: 0xb8855b, roughness: .88 }),
        tape: new THREE.MeshStandardMaterial({ color: 0xd9b583, roughness: .55 }),
        purple: new THREE.MeshStandardMaterial({ color: 0x7543dc, metalness: .45, roughness: .26, emissive: 0x2e145f, emissiveIntensity: .7 }),
        glow: new THREE.MeshBasicMaterial({ color: 0xb9a0ff, transparent: true, opacity: .76 }),
        shelf: new THREE.MeshStandardMaterial({ color: 0x455775, metalness: .7, roughness: .44 }),
        lightPanel: new THREE.MeshStandardMaterial({ color: 0x8f82c2, emissive: 0x7b5be0, emissiveIntensity: .75, roughness: .25 }),
        beltTrim: new THREE.MeshStandardMaterial({ color: 0x9e7aff, emissive: 0x512caf, emissiveIntensity: .75, metalness: .42, roughness: .25 }),
    };
    const parcelBoxMaterial = materials.cardboardLight.clone(); parcelBoxMaterial.transparent=true; parcelBoxMaterial.opacity=1; parcelBoxMaterial.depthWrite=false;
    const parcelTapeMaterial = materials.tape.clone(); parcelTapeMaterial.transparent=true; parcelTapeMaterial.opacity=1; parcelTapeMaterial.depthWrite=false;
    scene.add(new THREE.HemisphereLight(0xc3d0ff, 0x27344e, 1.35));
    const key = new THREE.DirectionalLight(0xe3e7ff, 2.8); key.position.set(-4, 9, 6); scene.add(key);
    const violet = new THREE.PointLight(0x7646ff, 70, 20, 2); violet.position.set(1.4, 3.8, -1.2); scene.add(violet);
    const warm = new THREE.PointLight(0xe9a871, 34, 17, 2); warm.position.set(-6, 3, -2); scene.add(warm);
    const finalLight = new THREE.PointLight(0xa77aff, 0, 13, 1.7); finalLight.position.set(1.25,2.6,2.5); scene.add(finalLight);

    const addBox = (parent, geometry, material, pos, scale) => {
        const mesh = new THREE.Mesh(geometry, material);
        mesh.position.set(...pos);
        if (scale) mesh.scale.set(...scale);
        parent.add(mesh); return mesh;
    };
    // Warehouse floor, raised lanes and steel shelving establish scale and depth.
    addBox(scene, new THREE.BoxGeometry(32, .25, 28), materials.floor, [0, -.3, -4]);
    const floorGrid = new THREE.GridHelper(30, 30, 0x34416c, 0x1b294c); floorGrid.position.set(0, -.16, -3); floorGrid.material.transparent = true; floorGrid.material.opacity = .36; scene.add(floorGrid);
    for (const z of [-8, -4, 0]) {
        for (const x of [-8.2, -5.3, 5.5, 8.2]) {
            addBox(scene, new THREE.BoxGeometry(.16, 4.8, .16), materials.shelf, [x, 2.1, z]);
        }
        for (const side of [-1, 1]) {
            for (const y of [.25, 1.75, 3.35]) {
                addBox(scene, new THREE.BoxGeometry(5.9, .14, .28), materials.shelf, [side * 6.75, y, z]);
            }
            for (let row = 0; row < 2; row++) for (let n = 0; n < 3; n++) {
                const box = addBox(scene, new THREE.BoxGeometry(.8, .75, .72), n % 2 ? materials.cardboard : materials.cardboardLight,
                    [side * 7.2 + (n - 1) * .92, .72 + row * .9, z]);
                box.rotation.y = (n % 2 ? .05 : -.06);
            }
        }
    }
    // Roof lights and repeating trusses create scale and a clear industrial vanishing point.
    const roof = new THREE.Group(); scene.add(roof);
    for (const z of [-11,-8,-5,-2,1,4]) {
        for (const x of [-8.5,8.5]) addBox(roof,new THREE.BoxGeometry(.22,6.6,.22),materials.darkSteel,[x,3.2,z]);
        addBox(roof,new THREE.BoxGeometry(17,.18,.22),materials.shelf,[0,6.35,z]);
        addBox(roof,new THREE.BoxGeometry(3.2,.12,.7),materials.lightPanel,[0,6.05,z-.35]);
    }
    for (const side of [-1,1]) {
        addBox(scene,new THREE.BoxGeometry(12.8,.045,.035),materials.beltTrim,[0,.9,side*.79]);
        addBox(scene,new THREE.BoxGeometry(5.2,.04,.035),materials.beltTrim,[4.7,.74,side*1.77]);
    }
    // Conveyor sections with visible rollers and legs; the belt surface itself moves subtly with its rollers.
    const beltGroup = new THREE.Group(); scene.add(beltGroup);
    addBox(beltGroup, new THREE.BoxGeometry(12.5, .18, 1.65), materials.belt, [0, .28, 0]);
    addBox(beltGroup, new THREE.BoxGeometry(12.7, .13, .12), materials.steel, [0, .83, -.79]);
    addBox(beltGroup, new THREE.BoxGeometry(12.7, .13, .12), materials.steel, [0, .83, .79]);
    const rollerGeometry = new THREE.CylinderGeometry(.14, .14, 1.58, 8);
    for (let x = -6; x <= 6; x += .48) {
        const roller = new THREE.Mesh(rollerGeometry, materials.steel);
        roller.rotation.z = Math.PI / 2;
        roller.position.set(x, .61, 0);
        beltGroup.add(roller);
    }
    const slatPositions = Array.from({ length: 77 }, (_, i) => -6.15 + i * .16);
    const slatGeometry = new THREE.BoxGeometry(.115, .055, 1.55);
    const slats = [materials.steel, materials.darkSteel].map((material, band) => {
        const positions = slatPositions.filter((_, i) => Number(i % 3 !== 0) === band);
        const mesh = new THREE.InstancedMesh(slatGeometry, material, positions.length);
        mesh.instanceMatrix.setUsage(THREE.DynamicDrawUsage);
        mesh.frustumCulled = false;
        beltGroup.add(mesh);
        return { mesh, positions };
    });
    const slatMatrix = new THREE.Matrix4();
    const beltMotion = { offset: 0 };
    function updateBeltSlats() {
        for (const { mesh, positions } of slats) {
            positions.forEach((x, i) => {
                slatMatrix.makeTranslation((x + beltMotion.offset + 6.25) % 12.5 - 6.25, .79, 0);
                mesh.setMatrixAt(i, slatMatrix);
            });
            mesh.instanceMatrix.needsUpdate = true;
        }
    }
    for (const x of [-5.7, -2.2, 1.4, 5.2]) for (const z of [-.58, .58]) {
        addBox(beltGroup, new THREE.BoxGeometry(.12, .92, .12), materials.darkSteel, [x, .05, z]);
    }
    // Scanner gantry and its beam.
    const scanner = new THREE.Group(); scanner.position.set(.65, 0, 0); scene.add(scanner);
    for (const x of [-.8, .8]) addBox(scanner, new THREE.BoxGeometry(.12, 2.65, .18), materials.darkSteel, [x, 1.75, 0]);
    addBox(scanner, new THREE.BoxGeometry(1.85, .2, 1.88), materials.steel, [0, 3.03, 0]);
    addBox(scanner, new THREE.BoxGeometry(1.56, .08, 1.6), materials.purple, [0, 2.91, 0]);
    const beam = new THREE.Mesh(new THREE.BoxGeometry(1.18, .035, 1.18), new THREE.MeshBasicMaterial({ color: 0xb79aff, transparent: true, opacity: .72 }));
    beam.position.set(0, 1.25, 0); scanner.add(beam);
    const scanRing = new THREE.Mesh(new THREE.TorusGeometry(.42, .025, 8, 48), materials.glow); scanRing.rotation.x = Math.PI / 2; scanRing.position.set(0, 1.3, 0); scanner.add(scanRing);
    // A small in-world hologram echoes the readable data card without competing with it.
    const holoPanel = new THREE.Group(); holoPanel.position.set(1.45,2.15,.72); scanner.add(holoPanel);
    const holoCanvas = document.createElement('canvas'); holoCanvas.width=512; holoCanvas.height=384;
    const holoContext = holoCanvas.getContext('2d');
    holoContext.fillStyle='rgba(9,16,43,.88)'; holoContext.fillRect(0,0,512,384);
    holoContext.strokeStyle='#a98aff'; holoContext.lineWidth=5; holoContext.strokeRect(5,5,502,374);
    holoContext.fillStyle='#c9b8ff'; holoContext.font='700 27px Arial'; holoContext.fillText('LIVE STOCK SIGNAL',28,52);
    holoContext.fillStyle='#fff'; holoContext.font='700 34px Arial'; holoContext.fillText('DEMAND  /  HIGH',28,112);
    holoContext.fillStyle='#9985d7'; holoContext.font='18px Arial'; holoContext.fillText('DEMONSTRATION DATA',28,151);
    holoContext.strokeStyle='#40386a'; holoContext.lineWidth=2; holoContext.beginPath(); holoContext.moveTo(28,310); holoContext.lineTo(480,310); holoContext.stroke();
    const holoBars=[];
    for(let i=0;i<9;i++){const height=30+((i*37)%105);holoContext.fillStyle=i>5?'#9c79ff':'#6448a8';holoContext.fillRect(32+i*48,306-height,24,height);holoBars.push(height);}
    const holoTexture = new THREE.CanvasTexture(holoCanvas); holoTexture.colorSpace=THREE.SRGBColorSpace;
    const holoMaterial = new THREE.MeshBasicMaterial({map:holoTexture,transparent:true,opacity:0,depthWrite:false});
    const holoFace = new THREE.Mesh(new THREE.PlaneGeometry(1.18,.88),holoMaterial);
    holoPanel.add(holoFace);
    // Two clear diverging lanes communicate keep/manage vs surplus.
    const branchA = new THREE.Group(); branchA.position.set(2.5, .5, -1.1); branchA.rotation.y = -.18; scene.add(branchA);
    const branchB = new THREE.Group(); branchB.position.set(2.6, .5, 1.15); branchB.rotation.y = .2; scene.add(branchB);
    for (const branch of [branchA, branchB]) {
        addBox(branch, new THREE.BoxGeometry(5.1, .14, 1.25), materials.belt, [2.35, -.2, 0]);
        for (let i = 0; i < 10; i++) {
            const roller = new THREE.Mesh(new THREE.CylinderGeometry(.1, .1, 1.2, 10), materials.steel);
            roller.rotation.z = Math.PI / 2; roller.position.set(.15 + i * .49, -.13, 0); branch.add(roller);
        }
        for (let i=0;i<30;i++) addBox(branch,new THREE.BoxGeometry(.11,.045,1.18),i%3===0?materials.steel:materials.darkSteel,[.05+i*.17,.27,0]);
    }
    // Parcel has individual tape strips and a restrained Stock Mind cube finish for the final brand beat.
    const parcel = new THREE.Group(); scene.add(parcel);
    parcel.position.set(-2.2, 1.25, 0);
    addBox(parcel, new THREE.BoxGeometry(1.12, .82, .92), parcelBoxMaterial, [0, 0, 0]);
    addBox(parcel, new THREE.BoxGeometry(.16, .84, .94), parcelTapeMaterial, [0, .012, 0]);
    addBox(parcel, new THREE.BoxGeometry(1.14, .06, .1), parcelTapeMaterial, [0, .42, 0]);
    const labelCanvas = document.createElement('canvas');
    labelCanvas.width = 512; labelCanvas.height = 256;
    const labelContext = labelCanvas.getContext('2d');
    labelContext.fillStyle = '#f5f3ff'; labelContext.fillRect(0,0,512,256);
    labelContext.fillStyle = '#231748'; labelContext.fillRect(0,0,512,42);
    labelContext.fillStyle = '#d8caff'; labelContext.font = '700 18px Arial'; labelContext.fillText('STOCK MIND  /  SMART INVENTORY',18,28);
    labelContext.fillStyle = '#17213c'; labelContext.font = '700 28px Arial'; labelContext.fillText('SMART INVENTORY ITEM',18,88);
    labelContext.fillStyle = '#59627a'; labelContext.font = '18px Arial'; labelContext.fillText('DEMO STOCK  ·  120 UNITS',18,120);
    labelContext.fillStyle = '#6e45d6'; labelContext.fillRect(18,143,114,30);
    labelContext.fillStyle = '#fff'; labelContext.font = '700 15px Arial'; labelContext.fillText('REVIEW',43,164);
    labelContext.fillStyle = '#151a2b';
    for (let i=0;i<46;i++) labelContext.fillRect(18+i*4.1,195,(i%4===0)?4:2,39);
    const labelTexture = new THREE.CanvasTexture(labelCanvas); labelTexture.colorSpace = THREE.SRGBColorSpace;
    const parcelLabelMaterial = new THREE.MeshBasicMaterial({map:labelTexture,transparent:true,opacity:1,depthWrite:false});
    const label = new THREE.Mesh(new THREE.PlaneGeometry(.58,.29),parcelLabelMaterial);
    label.position.set(0,.02,.466); parcel.add(label);
    const cubeMaterial = new THREE.MeshStandardMaterial({ color: 0x7037ed, metalness: .38, roughness: .2, emissive: 0x3a1686, emissiveIntensity: .9, transparent:true, opacity:0, depthWrite:false });
    const cube = new THREE.Mesh(new THREE.BoxGeometry(.96, .96, .96), cubeMaterial);
    cube.scale.setScalar(.08); parcel.add(cube);
    const cubeEdgesMaterial = new THREE.LineBasicMaterial({color:0xd7c8ff,transparent:true,opacity:0});
    const cubeEdges = new THREE.LineSegments(new THREE.EdgesGeometry(new THREE.BoxGeometry(.98,.98,.98)),cubeEdgesMaterial);
    cube.add(cubeEdges);
    const logoTexture = new THREE.TextureLoader().load('/components/logo.png', () => scheduleFrame());
    logoTexture.colorSpace = THREE.SRGBColorSpace;
    const logoFaceMaterial = new THREE.MeshBasicMaterial({map:logoTexture,transparent:true,opacity:0,depthWrite:false});
    const logoFace = new THREE.Mesh(new THREE.PlaneGeometry(.48,.48),logoFaceMaterial);
    logoFace.position.set(0,0,.493); cube.add(logoFace);
    const revealPlatformMaterial = new THREE.MeshStandardMaterial({color:0x30205d,metalness:.72,roughness:.24,emissive:0x5429a6,emissiveIntensity:.8,transparent:true,opacity:0,depthWrite:false});
    const revealPlatform = new THREE.Mesh(new THREE.CylinderGeometry(.82,.92,.07,64),revealPlatformMaterial);
    revealPlatform.position.set(0,-.62,0); parcel.add(revealPlatform);
    const ringMaterial = new THREE.MeshBasicMaterial({color:0xbba2ff,transparent:true,opacity:0,depthWrite:false});
    const orbitRing = new THREE.Mesh(new THREE.TorusGeometry(1.12,.012,8,96),ringMaterial);
    orbitRing.rotation.x=Math.PI/2; orbitRing.position.y=-.55; parcel.add(orbitRing);
    const haloRing = new THREE.Mesh(new THREE.TorusGeometry(1.08,.009,8,96),ringMaterial);
    haloRing.position.set(0,0,-.58); parcel.add(haloRing);
    const keepParcel = parcel.clone(true);
    keepParcel.position.set(.65,1.37,0);
    keepParcel.scale.setScalar(.001);
    scene.add(keepParcel);
    // Background inventory piles make the opening feel unsettled, then recede during the journey.
    const clutter = new THREE.Group(); scene.add(clutter);
    const clutterPositions = [[-4,.4,2.7],[-3.4,1.25,2.3],[-5.2,.45,3.1],[3.2,.4,2.8],[4.1,1.1,3.1],[-1.7,.38,4.2],[1.1,.35,4.7]];
    clutterPositions.forEach(([x,y,z], i) => { const b = addBox(clutter, new THREE.BoxGeometry(.9 + (i%2)*.3,.72 + (i%3)*.13,.82), i%2 ? materials.cardboard : materials.cardboardLight,[x,y,z]); b.rotation.y = (i%2 ? .24 : -.19); });
    // Sparse dust motes, kept low count for integrated graphics.
    const dustGeo = new THREE.BufferGeometry(); const dustN = 120; const dustPos = new Float32Array(dustN * 3);
    for (let i = 0; i < dustN; i++) { dustPos[i*3]=(Math.random()-.5)*22; dustPos[i*3+1]=Math.random()*6; dustPos[i*3+2]=(Math.random()-.5)*18-2; }
    dustGeo.setAttribute('position', new THREE.BufferAttribute(dustPos,3));
    const dust = new THREE.Points(dustGeo, new THREE.PointsMaterial({ color: 0xc6b9ff, size: .025, transparent: true, opacity: .48 })); scene.add(dust);

    const cameraRig = { x: -.5, y: 4.6, z: 11.5, tx: -1.8, ty: .8, tz: 0 };
    function placeCamera() { camera.position.set(cameraRig.x,cameraRig.y,cameraRig.z); look.set(cameraRig.tx,cameraRig.ty,cameraRig.tz); camera.lookAt(look); }
    const stages = [...root.querySelectorAll('[data-scene]')];
    const progressBar = root.querySelector('.sm-journey__progress i');
    let activeStage = -1;
    function setStage(value) {
        const next = Math.min(5, Math.max(0, Math.floor(value)));
        if (next === activeStage) return;
        stages.forEach((el, index) => {
            const active = index === next;
            el.classList.toggle('is-active', active);
            el.inert = !active;
        });
        activeStage = next;
    }

    const cameraFrames = [
        [-.5,4.6,11.5,-1.8,.8,0], [.2,3.8,9.3,.2,1.1,0], [.2,3.35,7.9,.35,1.15,0],
        [1,4.2,10,1.4,.8,0], [1.3,4.7,12.3,1.7,.8,0], [.4,3.05,8.8,.65,1.25,0]
    ];
    const parcelFrames = [[-2.2,1.25,0],[-.5,1.38,0],[.65,1.37,0],[2.8,1.18,1.05],[5.2,1.15,1.1],[1.15,1.48,0]];
    function updateStory(progress) {
        dust.rotation.y = progress * .12;
        scanRing.rotation.z = Math.sin(progress * 20) * .16;
        beltMotion.offset = THREE.MathUtils.clamp((progress-.12)*3.4,0,2.7);
        updateBeltSlats();
        const scaled = Math.min(4.999, progress * 6);
        const index = Math.floor(scaled);
        const blend = THREE.MathUtils.smoothstep(scaled - index, 0, 1);
        const parcelTarget = parcelFrames[index].map((value, axis) => THREE.MathUtils.lerp(value, parcelFrames[index + 1][axis], blend));
        const cameraTarget = cameraFrames[index].map((value, axis) => THREE.MathUtils.lerp(value, cameraFrames[index + 1][axis], blend));
        parcel.position.set(...parcelTarget);
        [cameraRig.x,cameraRig.y,cameraRig.z,cameraRig.tx,cameraRig.ty,cameraRig.tz] = cameraTarget;
        placeCamera();
        const reveal = THREE.MathUtils.smoothstep(progress,.82,.94);
        const sortingProgress = THREE.MathUtils.clamp((progress-.49)/.17,0,1);
        const keepScale = .001 + .819 * THREE.MathUtils.smoothstep(progress,.49,.54) * (1-THREE.MathUtils.smoothstep(progress,.64,.68));
        keepParcel.scale.setScalar(keepScale);
        keepParcel.position.set(
            THREE.MathUtils.lerp(.65,5.7,THREE.MathUtils.smoothstep(sortingProgress,0,1)),
            THREE.MathUtils.lerp(1.37,1.19,sortingProgress),
            THREE.MathUtils.lerp(0,-1.75,THREE.MathUtils.smoothstep(sortingProgress,0,1))
        );
        keepParcel.rotation.y = sortingProgress*.16;
        holoMaterial.opacity = THREE.MathUtils.lerp(.92,0,THREE.MathUtils.clamp((progress-.48)*14,0,1)) * THREE.MathUtils.clamp((progress-.30)*18,0,1);
        holoPanel.position.y = 2.15 + Math.sin(progress*32)*.045;
        parcelBoxMaterial.opacity = 1-reveal;
        parcelTapeMaterial.opacity = 1-reveal;
        parcelLabelMaterial.opacity = 1-reveal;
        cubeMaterial.opacity = reveal;
        cube.scale.setScalar(THREE.MathUtils.lerp(.08,1.35,reveal));
        cubeEdgesMaterial.opacity = reveal*.92;
        logoFaceMaterial.opacity = reveal;
        revealPlatformMaterial.opacity = reveal;
        ringMaterial.opacity = reveal*.72;
        orbitRing.rotation.y = reveal*Math.PI*2;
        haloRing.rotation.z = -reveal*Math.PI*2;
        finalLight.intensity = reveal*82;
        clutter.scale.setScalar(THREE.MathUtils.lerp(THREE.MathUtils.lerp(1,.72,THREE.MathUtils.clamp(progress*2,0,1)),.035,reveal));
        branchB.position.z = THREE.MathUtils.lerp(1.15,1.45,THREE.MathUtils.clamp((progress-.45)*5,0,1));
        beam.material.opacity = THREE.MathUtils.lerp(.3,.95,Math.max(0,1-Math.abs(progress-.42)*9));
        beam.position.y = .82 + Math.sin(progress*36)*.48;
        const scanPulse = Math.max(0,1-Math.abs(progress-.42)*9);
        violet.intensity = 62 + scanPulse*36;
        materials.lightPanel.emissiveIntensity = THREE.MathUtils.lerp(.75,2.25,THREE.MathUtils.clamp((progress-.12)*5,0,1)) - reveal*1.1;
        materials.beltTrim.emissiveIntensity = THREE.MathUtils.lerp(.75,1.45,THREE.MathUtils.clamp((progress-.12)*5,0,1)) - reveal*.9;
        scanner.scale.setScalar(THREE.MathUtils.lerp(1,.001,reveal));
        branchA.scale.setScalar(THREE.MathUtils.lerp(1,.02,reveal));
        branchB.scale.setScalar(THREE.MathUtils.lerp(1,.02,reveal));
        beltGroup.scale.set(THREE.MathUtils.lerp(1,.42,reveal),THREE.MathUtils.lerp(1,.28,reveal),THREE.MathUtils.lerp(1,.42,reveal));
        roof.scale.setScalar(THREE.MathUtils.lerp(1,.72,reveal));
        floorGrid.material.opacity=THREE.MathUtils.lerp(.36,.06,reveal);
        scanRing.scale.setScalar(1 + Math.max(0,1-Math.abs(progress-.42)*8)*.5);
        scene.fog.density = THREE.MathUtils.lerp(.045,.09,THREE.MathUtils.clamp((progress-.8)*5,0,1));
        cube.rotation.set(.12,progress*Math.PI*12,0);
        parcel.rotation.y = progress > .8 ? (progress-.8)*Math.PI*10 : 0;
        progressBar.style.transform = `scaleX(${progress})`;
        setStage(progress >= .84 ? 5 : Math.min(4, Math.floor(progress*6)));
    }

    const nav = document.querySelector('.stock-nav');
    let frame = 0;
    let previousProgress = -1;
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

    function render() {
        frame = 0;
        if (!visible || stopped || document.hidden || reduceMotion.matches) return;
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
            camera.updateProjectionMatrix();
        }

        const distance = Math.max(1, root.offsetHeight - pin.offsetHeight);
        const progress = THREE.MathUtils.clamp((navOffset - root.getBoundingClientRect().top) / distance, 0, 1);
        if (progress !== previousProgress) {
            updateStory(progress);
            previousProgress = progress;
        }
        try {
            renderer.render(scene, camera);
            root.classList.add('sm-journey--drawn');
        } catch (error) {
            stop();
            console.warn('Stock Mind is showing its static warehouse image.', error);
        }
    }

    const viewport = new IntersectionObserver(([entry]) => {
        visible = entry.isIntersecting;
        if (visible) scheduleFrame();
    });
    const layout = new ResizeObserver(scheduleFrame);

    function stop() {
        if (stopped) return;
        stopped = true;
        cancelAnimationFrame(frame);
        root.classList.remove('sm-journey--ready', 'sm-journey--drawn');
        setStage(0);
        viewport.disconnect();
        layout.disconnect();
        window.removeEventListener('scroll', scheduleFrame);
        window.removeEventListener('pagehide', onPageHide);
        document.removeEventListener('visibilitychange', scheduleFrame);
        reduceMotion.removeEventListener('change', onMotionChange);
        canvas.removeEventListener('webglcontextlost', stop);
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

    function onPageHide(event) {
        if (!event.persisted) stop();
    }

    function onMotionChange() {
        if (reduceMotion.matches) stop();
    }

    root.classList.add('sm-journey--ready');
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
