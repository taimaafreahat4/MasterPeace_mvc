(function () {
  'use strict';

  var TIERS = [
    { n: 'Receiving & Dispatch', d: 'Incoming goods are scanned and registered into inventory. Outgoing orders leave from this floor.' },
    { n: 'Active Inventory', d: 'Fast-moving products, tracked against demand signals so restocking happens at the right time.' },
    { n: 'Bulk Storage', d: 'Full pallets and reserve quantities, organized by product and ready to draw from.' },
    { n: 'Surplus for Sale', d: 'Excess stock set aside to publish on the marketplace, by quantity or as a whole lot.' },
    { n: 'Review & Planning', d: 'Products checked against stock and demand before the next purchase order is placed.' }
  ];
  var LEVEL_P = [0.31, 0.42, 0.53, 0.64, 0.75];
  var CHAPTERS = [
    { a: 0, b: 0.05, lvl: 'Overview', n: 'The whole tower', d: 'Five storage levels stacked above one receiving floor, with the conveyor feeding goods in at the base.', btn: 0 },
    { a: 0.05, b: 0.27, lvl: 'Inbound', n: 'Goods arrive on the belt', d: 'A package rides the conveyor through the scanner arch and into the receiving floor.', btn: 0 },
    { a: 0.27, b: 0.39, lvl: 'Level 1', n: TIERS[0].n, d: TIERS[0].d, btn: 1 },
    { a: 0.39, b: 0.50, lvl: 'Level 2', n: TIERS[1].n, d: TIERS[1].d, btn: 2 },
    { a: 0.50, b: 0.61, lvl: 'Level 3', n: TIERS[2].n, d: TIERS[2].d, btn: 3 },
    { a: 0.61, b: 0.72, lvl: 'Level 4', n: TIERS[3].n, d: TIERS[3].d, btn: 4 },
    { a: 0.72, b: 0.83, lvl: 'Level 5', n: TIERS[4].n, d: TIERS[4].d, btn: 5 },
    { a: 0.83, b: 1.01, lvl: 'Stock Mind', n: 'Smarter inventory. Stronger business.', d: 'Analyze smarter. Manage better. Trade with confidence.', btn: 6 }
  ];

  var stage = document.getElementById('stock-tower-stage');
  var noglEl = document.getElementById('stock-tower-nogl');
  var trackEl = document.getElementById('stock-tower-track');
  var levelsEl = document.getElementById('stock-tower-levels');
  var panels = Array.from(trackEl.querySelectorAll('[data-tower-story]'));
  var pinned = document.getElementById('stock-tower-app');
  var detailEl = document.getElementById('stock-tower-detail');
  var hudTop = document.getElementById('stock-tower-hudTop');
  var hintEl = document.getElementById('stock-tower-hint');
  var railFill = document.getElementById('stock-tower-railFill');
  var dLvl = document.getElementById('stock-tower-d-lvl'), dName = document.getElementById('stock-tower-d-name'), dDesc = document.getElementById('stock-tower-d-desc');
  var reduce = !!(window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches);
  var glOK = false, buttons = [], chapterIdx = -1;

  function clamp(v, a, b) { return Math.max(a, Math.min(b, v)); }
  function sstep(a, b, x) { var t = clamp((x - a) / (b - a), 0, 1); return t * t * (3 - 2 * t); }
  function mod(a, n) { return ((a % n) + n) % n; }
  function scrollMax() { return Math.max(1, trackEl.offsetHeight - pinned.offsetHeight); }
  function readP() {
    var navHeight = parseFloat(getComputedStyle(pinned).top) || 0;
    return clamp((navHeight - trackEl.getBoundingClientRect().top) / scrollMax(), 0, 1);
  }

  function showChapter(idx) {
    if (idx === chapterIdx) return;
    chapterIdx = idx;
    var c = CHAPTERS[idx];
    var story = idx === 4 ? 'sorting' : idx === 5 ? 'marketplace' : idx === 7 ? 'brand' : '';
    panels.forEach(function (panel) { panel.hidden = panel.dataset.towerStory !== story; });
    trackEl.classList.toggle('has-story', !!story);
    dLvl.textContent = c.lvl; dName.textContent = c.n; dDesc.textContent = c.d;
    for (var b = 0; b < buttons.length; b++) buttons[b].setAttribute('aria-pressed', String(b === c.btn));
    if (!reduce) { detailEl.classList.remove('is-in'); void detailEl.offsetWidth; detailEl.classList.add('is-in'); }
  }
  function chapterFor(p) { for (var i = 0; i < CHAPTERS.length; i++) if (p >= CHAPTERS[i].a && p < CHAPTERS[i].b) return i; return CHAPTERS.length - 1; }

  function goTo(btnIndex) {
    var p = btnIndex === 0 ? 0 : btnIndex === 6 ? .92 : LEVEL_P[btnIndex - 1];
    if (!glOK) { showChapter(btnIndex === 0 ? 0 : btnIndex + 1); return; }
    window.scrollTo({ top: window.scrollY + trackEl.getBoundingClientRect().top - (parseFloat(getComputedStyle(pinned).top) || 0) + p * scrollMax(), behavior: reduce ? 'auto' : 'smooth' });
  }
  (function buildButtons() {
    var all = document.createElement('button');
    all.type = 'button'; all.className = 'lv'; all.setAttribute('aria-pressed', 'true');
    all.innerHTML = '<span class="n">Start</span><span class="t">Overview</span>';
    all.addEventListener('click', function () { goTo(0); });
    levelsEl.appendChild(all); buttons.push(all);
    TIERS.forEach(function (t, i) {
      var b = document.createElement('button');
      b.type = 'button'; b.className = 'lv'; b.setAttribute('aria-pressed', 'false');
      var n = document.createElement('span'); n.className = 'n'; n.textContent = 'Level ' + (i + 1);
      var s = document.createElement('span'); s.className = 't'; s.textContent = t.n;
      b.appendChild(n); b.appendChild(s);
      b.addEventListener('click', function () { goTo(i + 1); });
      levelsEl.appendChild(b); buttons.push(b);
    });
    var finish = document.createElement('button');
    finish.type = 'button'; finish.className = 'lv'; finish.setAttribute('aria-pressed', 'false');
    finish.innerHTML = '<span class="n">Finish</span><span class="t">Stock Mind</span>';
    finish.addEventListener('click', function () { goTo(6); });
    levelsEl.appendChild(finish); buttons.push(finish);
  })();
  showChapter(0);

  function noGL() { trackEl.classList.remove('has-3d'); noglEl.hidden = reduce; }
  if (reduce || !window.THREE) { noGL(); return; }
  var renderer;
  try {
    renderer = new THREE.WebGLRenderer({ antialias: true, powerPreference: 'high-performance' });
    if (!renderer.getContext()) throw new Error('no context');
  } catch (e) { noGL(); return; }
  glOK = true;
  trackEl.classList.add('has-3d');

  stage.appendChild(renderer.domElement);
  var canvas = renderer.domElement;
  canvas.setAttribute('role', 'img');
  canvas.setAttribute('aria-label', '3D scroll animation: a package rides a conveyor into a five-level Stock Mind warehouse tower, where crates drop onto floors and turntables rotate, then the view rises up through every level');
  renderer.shadowMap.enabled = true;
  renderer.shadowMap.type = THREE.PCFSoftShadowMap;

  /* ---------- helpers ---------- */
  function rng(seed) {
    return function () {
      seed |= 0; seed = seed + 0x6D2B79F5 | 0;
      var t = Math.imul(seed ^ seed >>> 15, 1 | seed);
      t = t + Math.imul(t ^ t >>> 7, 61 | t) ^ t;
      return ((t ^ t >>> 14) >>> 0) / 4294967296;
    };
  }
  var rand = rng(11);
  function canvasTex(w, h, draw) {
    var c = document.createElement('canvas'); c.width = w; c.height = h;
    draw(c.getContext('2d'), w, h);
    return new THREE.CanvasTexture(c);
  }

  /* ---------- scene, camera, lights ---------- */
  var scene = new THREE.Scene();
  scene.background = canvasTex(4, 512, function (g, w, h) {
    var gr = g.createLinearGradient(0, 0, 0, h);
    gr.addColorStop(0, '#060b1f'); gr.addColorStop(0.36, '#0c1538'); gr.addColorStop(0.5, '#1a2250'); gr.addColorStop(1, '#1a2250');
    g.fillStyle = gr; g.fillRect(0, 0, w, h);
  });
  scene.fog = new THREE.FogExp2(0x1a2250, 0.0105);
  var camera = new THREE.PerspectiveCamera(38, 1, 0.1, 420);

  scene.add(new THREE.HemisphereLight(0x8c96d0, 0x0a1030, 0.6));
  var sun = new THREE.DirectionalLight(0xdfe3f5, 0.55);
  sun.position.set(20, 34, 26); sun.target.position.set(0, 6, 4);
  sun.castShadow = true;
  sun.shadow.mapSize.set(2048, 2048);
  sun.shadow.camera.left = -26; sun.shadow.camera.right = 26; sun.shadow.camera.top = 26; sun.shadow.camera.bottom = -26;
  sun.shadow.camera.near = 1; sun.shadow.camera.far = 100; sun.shadow.bias = -0.0005;
  scene.add(sun); scene.add(sun.target);
  var rim = new THREE.DirectionalLight(0x5b4fb5, 0.8); rim.position.set(-26, 12, -32); scene.add(rim);
  var front = new THREE.DirectionalLight(0x8077cc, 0.3); front.position.set(8, 6, 40); scene.add(front);

  /* ---------- tower dimensions ---------- */
  var FH = 2.7, XS = 1.3, RS = [7.2, 6.6, 6.0, 5.4, 4.8], ROOF_R = 5.6, ROOF_Y = 5 * FH;
  var GROUND_Y = -0.64, HOLE = 1.2;

  var slabMat = new THREE.MeshStandardMaterial({ color: 0x151e55, metalness: 0.5, roughness: 0.42 });
  var plinthMat = new THREE.MeshStandardMaterial({ color: 0x0c1440, metalness: 0.4, roughness: 0.5 });
  var dimRing = new THREE.MeshBasicMaterial({ color: 0x7f77c9, transparent: true, opacity: 0.45 });
  var brightRing = new THREE.MeshBasicMaterial({ color: 0xc3bfe8 });
  var solidGeo = new THREE.CylinderGeometry(1, 1, 0.28, 72);
  var ringGeo = new THREE.TorusGeometry(1, 0.0045, 6, 140);

  function slabGeo(rx, rz) {
    var sh = new THREE.Shape(); sh.absellipse(0, 0, rx, rz, 0, Math.PI * 2, false, 0);
    var hole = new THREE.Path(); hole.absarc(0, 0, HOLE, 0, Math.PI * 2, true); sh.holes.push(hole);
    var g = new THREE.ExtrudeGeometry(sh, { depth: 0.28, bevelEnabled: false, curveSegments: 72 });
    g.rotateX(-Math.PI / 2); g.translate(0, -0.14, 0);
    return g;
  }
  function ringMesh(rx, rz, y, mat) {
    var m = new THREE.Mesh(ringGeo, mat);
    m.rotation.x = Math.PI / 2; m.scale.set(rx, rz, Math.min(rx, rz)); m.position.y = y; return m;
  }

  var ground = new THREE.Mesh(new THREE.PlaneGeometry(500, 500), new THREE.MeshStandardMaterial({ color: 0x0a1338, metalness: 0.35, roughness: 0.34 }));
  ground.rotation.x = -Math.PI / 2; ground.position.y = GROUND_Y; ground.receiveShadow = true; scene.add(ground);

  var plinth = new THREE.Mesh(solidGeo, plinthMat);
  plinth.scale.set(8.5 * XS, 1.8, 8.5); plinth.position.y = -0.39; plinth.receiveShadow = true; scene.add(plinth);
  scene.add(ringMesh(8.5 * XS, 8.5, -0.14, new THREE.MeshBasicMaterial({ color: 0x9a93d9, transparent: true, opacity: 0.8 })));

  var poolTex = canvasTex(256, 256, function (g, w, h) {
    var gr = g.createRadialGradient(w / 2, h / 2, 4, w / 2, h / 2, w / 2);
    gr.addColorStop(0, 'rgba(110,100,200,0.55)'); gr.addColorStop(0.5, 'rgba(110,100,200,0.16)'); gr.addColorStop(1, 'rgba(110,100,200,0)');
    g.fillStyle = gr; g.fillRect(0, 0, w, h);
  });
  var pool = new THREE.Mesh(new THREE.PlaneGeometry(64, 64), new THREE.MeshBasicMaterial({ map: poolTex, transparent: true, depthWrite: false, opacity: 0.5 }));
  pool.rotation.x = -Math.PI / 2; pool.position.y = GROUND_Y + 0.01; scene.add(pool);
  scene.add(ringMesh(12.5 * XS, 12.5, GROUND_Y + 0.02, new THREE.MeshBasicMaterial({ color: 0x7f77c9, transparent: true, opacity: 0.22 })));

  /* ---------- tiers ---------- */
  var tierFx = [];
  var wallGeoFull = new THREE.CylinderGeometry(1, 1, FH - 0.28, 72, 1, true);
  var wallGeoGap = new THREE.CylinderGeometry(1, 1, FH - 0.28, 72, 1, true, 0.2, Math.PI * 2 - 0.4);
  var colGeo = new THREE.CylinderGeometry(0.045, 0.045, FH - 0.28, 6);
  var colMat = new THREE.MeshStandardMaterial({ color: 0x4d5ba8, metalness: 0.7, roughness: 0.3 });
  var colMesh = new THREE.InstancedMesh(colGeo, colMat, 5 * 60);
  var colN = 0, tmpM = new THREE.Matrix4();

  for (var i = 0; i < 5; i++) {
    var R = RS[i], y = i * FH;
    var slab = new THREE.Mesh(slabGeo(R * XS, R), slabMat);
    slab.position.y = y; slab.receiveShadow = true; slab.castShadow = true; scene.add(slab);

    var ringMat = new THREE.MeshBasicMaterial({ color: 0x9a93d9, transparent: true, opacity: 0.95 });
    scene.add(ringMesh(R * XS, R, y + 0.15, ringMat));
    scene.add(ringMesh(HOLE, HOLE, y + 0.15, brightRing));
    scene.add(ringMesh(R * 0.93 * XS, R * 0.93, y + 0.145, dimRing));

    var glassMat = new THREE.MeshBasicMaterial({ color: 0x7f77c9, transparent: true, opacity: 0.06, side: THREE.DoubleSide, depthWrite: false });
    var wall = new THREE.Mesh(i === 0 ? wallGeoGap : wallGeoFull, glassMat);
    wall.scale.set(R * XS * 0.985, 1, R * 0.985); wall.position.y = y + FH / 2; scene.add(wall);

    for (var c = 0; c < 56; c++) {
      var a = c / 56 * Math.PI * 2, cx = Math.cos(a) * R * XS * 0.985, cz = Math.sin(a) * R * 0.985;
      if (i === 0 && cz > 0 && Math.abs(cx) < 2.4) continue;
      tmpM.makeTranslation(cx, y + FH / 2, cz); colMesh.setMatrixAt(colN++, tmpM);
    }

    var light = new THREE.PointLight(0xf2e9d8, 1.3, 14, 2);
    light.position.set(0, y + 2.4, 0); scene.add(light);
    tierFx.push({ ring: ringMat, glass: glassMat, light: light });
  }
  colMesh.count = colN; colMesh.castShadow = true; scene.add(colMesh);

  var roof = new THREE.Mesh(slabGeo(ROOF_R * XS, ROOF_R), slabMat);
  roof.scale.y = 1.4; roof.position.y = ROOF_Y; roof.castShadow = true; scene.add(roof);
  scene.add(ringMesh(ROOF_R * XS, ROOF_R, ROOF_Y + 0.2, brightRing));
  scene.add(ringMesh(HOLE, HOLE, ROOF_Y + 0.2, brightRing));

  // lift shaft: a translucent tube with cabs that run along its wall
  var coreH = ROOF_Y + 0.4;
  var core = new THREE.Mesh(new THREE.CylinderGeometry(0.98, 0.98, coreH, 40, 1, true), new THREE.MeshBasicMaterial({ color: 0x7f77c9, transparent: true, opacity: 0.09, side: THREE.DoubleSide, depthWrite: false }));
  core.position.y = coreH / 2 - 0.2; scene.add(core);
  var cabMat = new THREE.MeshBasicMaterial({ color: 0xdcd8f2 });
  var cabs = [];
  [4.4, 5.0, 5.6].forEach(function (ang, ci) {
    var cab = new THREE.Mesh(new THREE.BoxGeometry(0.34, 0.6, 0.34), cabMat);
    cab.position.x = Math.cos(ang) * 0.82; cab.position.z = Math.sin(ang) * 0.82;
    scene.add(cab); cabs.push({ m: cab, ph: ci * 2.1 });
  });

  /* ---------- racks and goods (instanced) ---------- */
  var boxGeo = new THREE.BoxGeometry(1, 1, 1);
  var postMesh = new THREE.InstancedMesh(boxGeo, new THREE.MeshStandardMaterial({ color: 0x3b4a8f, metalness: 0.6, roughness: 0.4 }), 1700);
  var shelfMesh = new THREE.InstancedMesh(boxGeo, new THREE.MeshStandardMaterial({ color: 0x222d72, metalness: 0.5, roughness: 0.5 }), 1300);
  var goodsMesh = new THREE.InstancedMesh(boxGeo, new THREE.MeshStandardMaterial({ color: 0xffffff, roughness: 0.85, metalness: 0 }), 3400);
  var nPost = 0, nShelf = 0, nGoods = 0;
  var yAx = new THREE.Vector3(0, 1, 0), q = new THREE.Quaternion(), v = new THREE.Vector3(), sc = new THREE.Vector3(), rp = new THREE.Vector3(), m4 = new THREE.Matrix4(), col = new THREE.Color();

  function put(mesh, idx, phi, lx, ly, lz, sx, sy, sz) {
    q.setFromAxisAngle(yAx, phi);
    v.set(lx, ly, lz).applyQuaternion(q).add(rp);
    sc.set(sx, sy, sz); m4.compose(v, q, sc); mesh.setMatrixAt(idx, m4);
  }
  function perim(a, b) { return Math.PI * (3 * (a + b) - Math.sqrt((3 * a + b) * (a + 3 * b))); }
  var RW = 1.1, RD = 0.5, RH = 2.2, FRACS = [0.4, 0.62, 0.84];

  for (var ti = 0; ti < 5; ti++) {
    var Rt = RS[ti], base = ti * FH + 0.14;
    for (var fi = 0; fi < FRACS.length; fi++) {
      var f = FRACS[fi], ax = Rt * XS * f, az = Rt * f;
      var n = Math.floor(perim(ax, az) / 1.55);
      for (var j = 0; j < n; j++) {
        var ang = (j + 0.5) / n * Math.PI * 2 + ti * 0.37;
        var px = Math.cos(ang) * ax, pz = Math.sin(ang) * az;
        if (pz > -0.5 && Math.abs(px) < 2.7) continue; // loading lane toward the front of every floor
        var phi = Math.atan2(-Math.cos(ang), -XS * Math.sin(ang));
        rp.set(px, base, pz);
        for (var sx = -1; sx <= 1; sx += 2) for (var sz = -1; sz <= 1; sz += 2)
          put(postMesh, nPost++, phi, sx * RW / 2, RH / 2, sz * RD / 2, 0.05, RH, 0.05);
        for (var k = 0; k < 3; k++) {
          var shY = 0.05 + k * 0.72;
          put(shelfMesh, nShelf++, phi, 0, shY, 0, RW, 0.05, RD);
          for (var s = -1; s <= 1; s++) {
            if (rand() < 0.12) continue;
            var gw = 0.28 + rand() * 0.04, gh = 0.34 + rand() * 0.22, gd = 0.38;
            put(goodsMesh, nGoods, phi, s * 0.36, shY + 0.025 + gh / 2, 0, gw, gh, gd);
            var r = rand();
            if (r < 0.3) col.setHSL(0.075 + rand() * 0.01, 0.36 + rand() * 0.06, 0.66 + rand() * 0.05);
            else col.setHSL(0.068 + rand() * 0.012, 0.4 + rand() * 0.08, 0.54 + rand() * 0.08);
            goodsMesh.setColorAt(nGoods, col); nGoods++;
          }
        }
      }
    }
  }
  postMesh.count = nPost; shelfMesh.count = nShelf; goodsMesh.count = nGoods;
  goodsMesh.instanceColor.needsUpdate = true;
  [postMesh, shelfMesh, goodsMesh].forEach(function (mm) { mm.castShadow = true; mm.receiveShadow = true; scene.add(mm); });

  /* ---------- roof logo cube ---------- */
  var logo = new THREE.Group();
  var logoCube = new THREE.Mesh(new THREE.BoxGeometry(1.8, 1.8, 1.8), new THREE.MeshStandardMaterial({ color: 0x6355c4, metalness: 0.3, roughness: 0.3, emissive: 0x3d3399, emissiveIntensity: 0.45 }));
  logoCube.add(new THREE.LineSegments(new THREE.EdgesGeometry(logoCube.geometry), new THREE.LineBasicMaterial({ color: 0xc3bfe8 })));
  logo.add(logoCube); logo.position.set(0, ROOF_Y + 3.4, 0); scene.add(logo);
  var logoLight = new THREE.PointLight(0x8d86d4, 1.0, 16, 2); logoLight.position.copy(logo.position); scene.add(logoLight);

  /* ---------- conveyor (driven by scroll) ---------- */
  var BZ0 = 1.8, BZ1 = 48, BL = BZ1 - BZ0, BY = 0.3;
  var beltBody = new THREE.Mesh(boxGeo, new THREE.MeshStandardMaterial({ color: 0x111a4d, metalness: 0.6, roughness: 0.4 }));
  beltBody.scale.set(2.5, 0.22, BL); beltBody.position.set(0, 0.19, (BZ0 + BZ1) / 2); beltBody.receiveShadow = true; scene.add(beltBody);

  var beltTex = canvasTex(64, 64, function (g, w, h) {
    g.fillStyle = '#0a1236'; g.fillRect(0, 0, w, h);
    g.fillStyle = '#26337c'; g.fillRect(0, h - 5, w, 3);
    g.strokeStyle = 'rgba(127,119,201,0.5)'; g.lineWidth = 3; g.lineCap = 'round'; g.lineJoin = 'round';
    g.beginPath(); g.moveTo(10, 40); g.lineTo(32, 22); g.lineTo(54, 40); g.stroke();
  });
  beltTex.wrapS = beltTex.wrapT = THREE.RepeatWrapping; beltTex.repeat.set(1, BL / 1.1); beltTex.anisotropy = 8;
  var beltTop = new THREE.Mesh(new THREE.PlaneGeometry(2.2, BL), new THREE.MeshStandardMaterial({ map: beltTex, roughness: 0.6, metalness: 0.2 }));
  beltTop.rotation.x = -Math.PI / 2; beltTop.position.set(0, BY + 0.002, (BZ0 + BZ1) / 2); beltTop.receiveShadow = true; scene.add(beltTop);

  var railMat = new THREE.MeshStandardMaterial({ color: 0x2a3890, metalness: 0.7, roughness: 0.35 });
  var stripMat = new THREE.MeshBasicMaterial({ color: 0x9a93d9 });
  [-1, 1].forEach(function (sd) {
    var rail = new THREE.Mesh(boxGeo, railMat); rail.scale.set(0.1, 0.12, BL); rail.position.set(sd * 1.3, 0.36, (BZ0 + BZ1) / 2); scene.add(rail);
    var strip = new THREE.Mesh(boxGeo, stripMat); strip.scale.set(0.03, 0.02, BL); strip.position.set(sd * 1.3, 0.43, (BZ0 + BZ1) / 2); scene.add(strip);
  });
  var legs = new THREE.InstancedMesh(boxGeo, railMat, 40), nLeg = 0;
  for (var lz = BZ0 + 5.5; lz < BZ1; lz += 5) [-1, 1].forEach(function (sd) {
    tmpM.compose(new THREE.Vector3(sd * 1.15, (GROUND_Y + BY) / 2 - 0.05, lz), new THREE.Quaternion(), new THREE.Vector3(0.12, BY - GROUND_Y - 0.1, 0.12));
    legs.setMatrixAt(nLeg++, tmpM);
  });
  legs.count = nLeg; legs.castShadow = true; scene.add(legs);

  var roadTex = canvasTex(64, 8, function (g, w, h) {
    var gr = g.createLinearGradient(0, 0, w, 0);
    gr.addColorStop(0, 'rgba(110,100,200,0)'); gr.addColorStop(0.5, 'rgba(127,119,201,0.55)'); gr.addColorStop(1, 'rgba(110,100,200,0)');
    g.fillStyle = gr; g.fillRect(0, 0, w, h);
  });
  var road = new THREE.Mesh(new THREE.PlaneGeometry(8, BL + 8), new THREE.MeshBasicMaterial({ map: roadTex, transparent: true, depthWrite: false, opacity: 0.4 }));
  road.rotation.x = -Math.PI / 2; road.position.set(0, GROUND_Y + 0.015, (BZ0 + BZ1) / 2 + 2); scene.add(road);

  /* ---------- parcels ---------- */
  var cardMat = new THREE.MeshStandardMaterial({ color: 0xc99a74, roughness: 0.85 });
  var tapeMat = new THREE.MeshStandardMaterial({ color: 0xe9d9bf, roughness: 0.6 });
  var heroTapeMat = new THREE.MeshBasicMaterial({ color: 0x7480c8 });
  var heroLabelMat = new THREE.MeshBasicMaterial({ color: 0xd9dcf3 });
  var labelGeo = new THREE.PlaneGeometry(1, 1), labelMat = new THREE.MeshStandardMaterial({ color: 0xe8e4da, roughness: 0.8 });
  function makeCrate(w, h, d, tm) {
    var g = new THREE.Group();
    var body = new THREE.Mesh(boxGeo, cardMat); body.scale.set(w, h, d); body.castShadow = true; body.receiveShadow = true; g.add(body);
    var tape = new THREE.Mesh(boxGeo, tm); tape.scale.set(w * 0.18, h * 1.012, d * 1.012); g.add(tape);
    var lab = new THREE.Mesh(labelGeo, labelMat); lab.scale.set(w * 0.4, h * 0.3, 1); lab.position.set(-w * 0.2, -h * 0.05, d / 2 + 0.003); g.add(lab);
    return g;
  }
  var HERO_W = 1.15, HERO_H = 0.85, HERO_D = 1.05;
  var heroG = makeCrate(HERO_W, HERO_H, HERO_D, heroTapeMat);
  var heroLab = new THREE.Mesh(new THREE.PlaneGeometry(HERO_W * 0.42, HERO_D * 0.34), heroLabelMat);
  heroLab.rotation.x = -Math.PI / 2; heroLab.position.set(HERO_W * 0.27, HERO_H / 2 + 0.004, -HERO_D * 0.18); heroG.add(heroLab);
  var heroLight = new THREE.PointLight(0xa49ee0, 0.9, 6, 2); heroLight.position.y = HERO_H / 2 + 0.8; heroG.add(heroLight);
  scene.add(heroG);
  var heroRing = new THREE.Group();
  heroRing.add(new THREE.Mesh(new THREE.TorusGeometry(0.95, 0.012, 6, 64), new THREE.MeshBasicMaterial({ color: 0xd0cdf0 })));
  heroRing.visible = false; scene.add(heroRing);

  var belt = [], BASES = [45.5, 34.5, 28.5, 17.5, 13.5, 8.5, 50.5];
  BASES.forEach(function (bz) {
    var w = 0.8 + rand() * 0.4, h = 0.55 + rand() * 0.35, d = 0.8 + rand() * 0.3;
    var g = makeCrate(w, h, d, tapeMat); scene.add(g);
    belt.push({ g: g, h: h, base: bz });
  });

  // scanner arch (the beam sweeps while the hero package passes it)
  var AZ = 22;
  var arch = new THREE.Group(); arch.position.z = AZ;
  var archMat = new THREE.MeshStandardMaterial({ color: 0x1c2780, metalness: 0.75, roughness: 0.3 });
  [-1, 1].forEach(function (sd) {
    var post = new THREE.Mesh(boxGeo, archMat); post.scale.set(0.24, 4.3, 0.3); post.position.set(sd * 1.78, BY + 2.1, 0); post.castShadow = true; arch.add(post);
    var edge = new THREE.Mesh(boxGeo, stripMat); edge.scale.set(0.03, 4.1, 0.03); edge.position.set(sd * 1.64, BY + 2.1, 0); arch.add(edge);
  });
  var beam = new THREE.Mesh(boxGeo, archMat); beam.scale.set(3.8, 0.3, 0.34); beam.position.set(0, BY + 4.4, 0); beam.castShadow = true; arch.add(beam);
  var beamEdge = new THREE.Mesh(boxGeo, stripMat); beamEdge.scale.set(3.3, 0.03, 0.03); beamEdge.position.set(0, BY + 4.22, 0); arch.add(beamEdge);
  var curtain = new THREE.Mesh(new THREE.PlaneGeometry(3.3, 4), new THREE.MeshBasicMaterial({ color: 0x7f77c9, transparent: true, opacity: 0.06, side: THREE.DoubleSide, depthWrite: false }));
  curtain.position.set(0, BY + 2.1, 0); arch.add(curtain);
  var scanLine = new THREE.Mesh(new THREE.PlaneGeometry(3.3, 0.06), new THREE.MeshBasicMaterial({ color: 0xdcd8f2, side: THREE.DoubleSide }));
  var scanGlow = new THREE.Mesh(new THREE.PlaneGeometry(3.3, 0.45), new THREE.MeshBasicMaterial({ color: 0x7f77c9, transparent: true, opacity: 0.16, side: THREE.DoubleSide, depthWrite: false }));
  arch.add(scanLine); arch.add(scanGlow);
  var archLight = new THREE.PointLight(0x8d86d4, 1.2, 12, 2); archLight.position.set(0, BY + 3, 0); arch.add(archLight);
  scene.add(arch);


  /* ---------- inside each floor: falling crates, turntables, halos ---------- */
  var chuteMat = new THREE.MeshBasicMaterial({ color: 0x9a93d9, transparent: true, opacity: 0.85 });
  var turnSteel = new THREE.MeshStandardMaterial({ color: 0x1a2570, metalness: 0.7, roughness: 0.35 });
  var scanRingMat = new THREE.MeshBasicMaterial({ color: 0xc3bfe8 });
  var haloGeo = new THREE.TorusGeometry(1.45, 0.018, 6, 64, 4.5);
  var haloMat = new THREE.MeshBasicMaterial({ color: 0xc3bfe8 });
  var DROP_H = 1.8, FALL_P = 3.8, crates = [], tables = [], halos = [];
  var SPOTS0 = [[-1.95, 2.7, 0], [1.95, 3.4, 1.3]];
  var SPOTS = [[-1.95, 2.7, 0], [1.95, 3.4, 1.3], [0.0, 2.4, 2.5]];

  for (var fl = 0; fl < 5; fl++) {
    var fy = fl * FH + 0.14, tm = fl === 3 ? heroTapeMat : tapeMat;
    (fl === 0 ? SPOTS0 : SPOTS).forEach(function (sp, si) {
      var w = 0.55 + rand() * 0.25, h = 0.4 + rand() * 0.25, d = 0.55 + rand() * 0.2;
      var g = makeCrate(w, h, d, tm); scene.add(g);
      var chute = ringMesh(0.5, 0.5, fl * FH + FH - 0.22, chuteMat); chute.position.x = sp[0]; chute.position.z = sp[1]; scene.add(chute);
      crates.push({ g: g, x: sp[0], z: sp[1], base: fy, h: h, ph: sp[2] + fl * 0.7 + si * 0.3, rx: (rand() - 0.5) * 1.8, rz: (rand() - 0.5) * 1.8, yaw: rand() * 3, t1: Math.sqrt(2 * DROP_H / 9.8) });
    });
    [-1, 1].forEach(function (sd) {
      var tg = new THREE.Group(); tg.position.set(sd * 1.82, fy, 4.4);
      var disc = new THREE.Mesh(new THREE.CylinderGeometry(0.4, 0.4, 0.06, 32), turnSteel); disc.position.y = 0.03; disc.receiveShadow = true; tg.add(disc);
      var rimT = new THREE.Mesh(new THREE.TorusGeometry(0.4, 0.012, 6, 48), stripMat); rimT.rotation.x = Math.PI / 2; rimT.position.y = 0.065; tg.add(rimT);
      var spin = new THREE.Group(); spin.position.y = 0.06;
      var pc = makeCrate(0.36, 0.32, 0.36, tm); pc.position.y = 0.16; spin.add(pc); tg.add(spin);
      var ringHolder = null;
      if (sd === 1) {
        ringHolder = new THREE.Group(); ringHolder.position.y = 0.5;
        ringHolder.add(new THREE.Mesh(new THREE.TorusGeometry(0.52, 0.012, 6, 48), scanRingMat)); tg.add(ringHolder);
      }
      scene.add(tg); tables.push({ spin: spin, ring: ringHolder, dir: sd });
    });
    var halo = new THREE.Group(); halo.position.y = fl * FH + 0.7;
    var hm = new THREE.Mesh(haloGeo, haloMat); hm.rotation.x = Math.PI / 2; halo.add(hm);
    scene.add(halo); halos.push(halo);
  }
  function dropY(tau) {
    var g = 9.8, t1 = Math.sqrt(2 * DROP_H / g);
    if (tau < t1) return DROP_H - 0.5 * g * tau * tau;
    var vv = g * t1 * 0.42, tt = tau - t1, tf = 2 * vv / g;
    if (tt < tf) return vv * tt - 0.5 * g * tt * tt;
    tt -= tf; vv *= 0.42; tf = 2 * vv / g;
    if (tt < tf) return vv * tt - 0.5 * g * tt * tt;
    return 0;
  }
  function updateCrates(t) {
    for (var ci = 0; ci < crates.length; ci++) {
      var cr = crates[ci], tau = reduce ? 1.8 : mod(t + cr.ph, FALL_P);
      var s = clamp(tau / 0.12, 0, 1) * clamp((FALL_P - tau) / 0.5, 0, 1);
      var tilt = Math.max(0, 1 - tau / (cr.t1 + 0.1));
      cr.g.scale.setScalar(Math.max(s, 0.001));
      cr.g.rotation.set(cr.rx * tilt, cr.yaw, cr.rz * tilt);
      cr.g.position.set(cr.x, cr.base + cr.h / 2 * s + dropY(tau), cr.z);
    }
  }

  /* ---------- city backdrop ---------- */
  for (var bi = 0; bi < 28; bi++) {
    var ba = rand() * Math.PI * 2, bd = 50 + rand() * 48;
    var bx = Math.cos(ba) * bd, bz = Math.sin(ba) * bd;
    if (bz > 18 || (Math.abs(bx) < 9 && bz > -10)) continue;
    var bw = 4 + rand() * 6, bh = 9 + rand() * 28, bdp = 4 + rand() * 6;
    var wt = canvasTex(64, 128, function (g, w, h) {
      g.fillStyle = '#0a1235'; g.fillRect(0, 0, w, h);
      var cols = ['#d9d2bd', '#e9e4d6', '#9aa5de', '#c7c9ea'];
      for (var yy = 4; yy < h - 6; yy += 12) for (var xx = 4; xx < w - 6; xx += 12) {
        if (rand() < 0.28) { g.fillStyle = cols[Math.floor(rand() * cols.length)]; g.fillRect(xx, yy, 5, 7); }
      }
    });
    wt.wrapS = wt.wrapT = THREE.RepeatWrapping; wt.repeat.set(Math.max(1, Math.round(bw / 3)), Math.max(1, Math.round(bh / 7)));
    var bMat = new THREE.MeshStandardMaterial({ color: 0xffffff, map: wt, emissive: 0xffffff, emissiveMap: wt, emissiveIntensity: 0.55, roughness: 0.7, metalness: 0.2 });
    var bm = new THREE.Mesh(boxGeo, bMat);
    bm.scale.set(bw, bh, bdp); bm.position.set(bx, GROUND_Y + bh / 2, bz); bm.rotation.y = rand() * 0.6; scene.add(bm);
  }

  /* ---------- dust ---------- */
  var DN = 420, dpos = new Float32Array(DN * 3), dseed = new Float32Array(DN);
  for (var di = 0; di < DN; di++) {
    dpos[di * 3] = (rand() - 0.5) * 60; dpos[di * 3 + 1] = rand() * 26; dpos[di * 3 + 2] = (rand() - 0.5) * 70 + 5; dseed[di] = rand() * 6.28;
  }
  var dgeo = new THREE.BufferGeometry(); dgeo.setAttribute('position', new THREE.BufferAttribute(dpos, 3));
  scene.add(new THREE.Points(dgeo, new THREE.PointsMaterial({ color: 0xc3bfe8, size: 2.2, transparent: true, opacity: 0.35, depthWrite: false, sizeAttenuation: false })));

  /* ---------- scroll timeline: hero package + camera path ---------- */
  // Everything below is a pure function of scroll progress p (0..1), so scrolling back plays the story in reverse.
  function heroZ(p) {
    if (p <= 0.05) return 40;
    if (p <= 0.14) return 40 - 18 * sstep(0.05, 0.14, p);
    if (p <= 0.17) return 22 - 1 * sstep(0.14, 0.17, p);
    if (p <= 0.27) return 21 - 18.4 * sstep(0.17, 0.27, p);
    return 2.6;
  }
  function chase(p, side, h, back) { var hz = heroZ(p); return { p: p, pos: [side, h, hz + back], look: [0, 0.9, hz - 4] }; }
  function eye(i) { return i * FH + 1.6; }

  var KEYS = [
    { p: 0, pos: [19.5, 14, 32], look: [0, 6.8, 2], hold: true },
    chase(0.05, 3.8, 2.5, 6.5),
    chase(0.09, 3.6, 2.4, 6.5),
    { p: 0.14, pos: [6.4, 2.5, 22.8], look: [0, 2.1, 22], hold: true },
    chase(0.20, 2.6, 2.3, 6.0),
    chase(0.24, 0, 2.2, 5.0),
    { p: 0.27, pos: [0, 2.5, 7.4], look: [0, 0.7, 2.6] }
  ];
  var H1P = [0.31, 0.42, 0.53, 0.64, 0.75], H2P = [0.36, 0.47, 0.58, 0.69, 0.80];
  for (var kt = 0; kt < 5; kt++) {
    var ey = eye(kt), side = kt % 2 === 0 ? -1 : 1;
    KEYS.push({ p: H1P[kt], pos: [0, ey, kt === 0 ? -0.1 : 0], look: [0, ey - 0.7, 8], hold: true });
    KEYS.push({ p: H2P[kt], pos: [0, ey, 0.9], look: [side * 5.2, ey - 0.4, -1.2], hold: true });
  }
  KEYS.push({ p: 0.86, pos: [0, ROOF_Y + 1.3, 0.2], look: [0, ROOF_Y + 2.6, 6] });
  KEYS.push({ p: 0.92, pos: [6.5, 18.2, 14], look: [0, 14.4, 0] });
  KEYS.push({ p: 1.0, pos: [15, 18.5, 27], look: [0, 10.8, 0], hold: true });
  KEYS.forEach(function (k) { k.v = k.pos.concat(k.look); k.m = [0, 0, 0, 0, 0, 0]; });
  KEYS.forEach(function (k, idx) {
    if (idx === 0 || idx === KEYS.length - 1 || k.hold) return;
    var a = KEYS[idx - 1], b = KEYS[idx + 1];
    for (var cc = 0; cc < 6; cc++) k.m[cc] = (b.v[cc] - a.v[cc]) / (b.p - a.p);
  });
  var camPos = new THREE.Vector3(), camLook = new THREE.Vector3(), tmpV = [0, 0, 0, 0, 0, 0];
  function samplePath(p) {
    var idx = 0;
    while (idx < KEYS.length - 2 && p > KEYS[idx + 1].p) idx++;
    var K0 = KEYS[idx], K1 = KEYS[idx + 1], h = K1.p - K0.p, t = clamp((p - K0.p) / h, 0, 1);
    var t2 = t * t, t3 = t2 * t, h00 = 2 * t3 - 3 * t2 + 1, h10 = t3 - 2 * t2 + t, h01 = -2 * t3 + 3 * t2, h11 = t3 - t2;
    for (var cc = 0; cc < 6; cc++) tmpV[cc] = h00 * K0.v[cc] + h10 * h * K0.m[cc] + h01 * K1.v[cc] + h11 * h * K1.m[cc];
    camPos.set(tmpV[0], tmpV[1], tmpV[2]); camLook.set(tmpV[3], tmpV[4], tmpV[5]);
  }

  /* ---------- pointer parallax ---------- */
  var mx = 0, my = 0, smx = 0, smy = 0, fwd = new THREE.Vector3(), rgt = new THREE.Vector3(), upV = new THREE.Vector3(0, 1, 0);
  if (!reduce) window.addEventListener('pointermove', function (e) {
    mx = (e.clientX / window.innerWidth - 0.5) * 2; my = (e.clientY / window.innerHeight - 0.5) * 2;
  }, { passive: true });

  /* ---------- sizing ---------- */
  function resize() {
    var w = stage.clientWidth, h = stage.clientHeight;
    if (!w || !h) return;
    renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 1.3, Math.sqrt(1000000 / (w * h))));
    renderer.setSize(w, h, false);
    camera.aspect = w / h;
    if (w / h > 1.35) camera.setViewOffset(w, h, -w * 0.09, 0, w, h); else camera.clearViewOffset();
    camera.updateProjectionMatrix();
  }
  var ro = null;
  if (window.ResizeObserver) { ro = new ResizeObserver(resize); ro.observe(stage); }
  else window.addEventListener('resize', resize);
  resize();

  /* ---------- frame ---------- */
  var clock = new THREE.Clock(), raf = 0, sp = readP();
  function update(dt, t) {
    var target = readP();
    sp = reduce ? target : sp + (target - sp) * (1 - Math.exp(-dt * 6));
    var kk = 1 - Math.exp(-dt * 3.5);

    // camera along the scripted path
    samplePath(sp);
    smx += (mx - smx) * kk; smy += (my - smy) * kk;
    fwd.subVectors(camLook, camPos); var dist = fwd.length(); fwd.normalize();
    rgt.crossVectors(fwd, upV).normalize();
    camLook.addScaledVector(rgt, smx * dist * 0.035).addScaledVector(upV, -smy * dist * 0.02);
    camera.position.copy(camPos); camera.lookAt(camLook);
    var rr = Math.hypot(camPos.x, camPos.z);
    var ins = (1 - sstep(3, 7.5, rr)) * (1 - sstep(ROOF_Y - 0.6, ROOF_Y + 1.2, camPos.y));
    var fov = 38 + 22 * ins + (camera.aspect < 1 ? 10 : 0);
    if (Math.abs(fov - camera.fov) > 0.01) { camera.fov = fov; camera.updateProjectionMatrix(); }

    // floor lighting follows the floor the camera is on
    for (var i = 0; i < 5; i++) {
      var w = clamp(1 - Math.abs(camPos.y - eye(i)) / (FH * 0.9), 0, 1), fx = tierFx[i];
      fx.light.intensity += ((1.0 + 2.0 * w) - fx.light.intensity) * kk;
      fx.ring.opacity += ((0.55 + 0.45 * w) - fx.ring.opacity) * kk;
      fx.glass.opacity += ((0.04 + 0.05 * w) - fx.glass.opacity) * kk;
    }

    // conveyor: belt texture, parcels and hero all advance by the same scroll distance
    var hz = heroZ(sp), D = 40 - hz;
    beltTex.offset.y = mod(-D / 1.1, 1);
    heroG.position.set(0, BY + HERO_H / 2, hz);
    heroRing.visible = sp > 0.265;
    heroRing.position.set(0, BY + HERO_H / 2 + 0.1, hz);
    heroRing.rotation.set(reduce ? 1.2 : t * 1.4, 0, 0.3);
    for (var b = 0; b < belt.length; b++) {
      var bp = belt[b], z = BZ0 + mod(bp.base - D - BZ0, BL);
      var s = clamp((z - BZ0) / 1.2, 0, 1) * clamp((BZ1 - z) / 1.0, 0, 1);
      bp.g.scale.setScalar(Math.max(s, 0.001));
      bp.g.position.set(0, BY + bp.h / 2 * s, z);
    }

    // scanner sweeps once while the hero is inside the arch
    var ss = clamp((sp - 0.115) / 0.06, 0, 1), ph = 0.5 - 0.5 * Math.cos(Math.PI * 2 * ss), sy = BY + 0.25 + ph * 3.8;
    var on = ss > 0 && ss < 1;
    scanLine.visible = on; scanGlow.visible = on; scanLine.position.y = sy; scanGlow.position.y = sy;

    // time-driven motion inside the tower
    updateCrates(t);
    for (var ti2 = 0; ti2 < tables.length; ti2++) {
      var tb = tables[ti2];
      tb.spin.rotation.y = reduce ? 0.6 : t * 0.9 * tb.dir;
      if (tb.ring) tb.ring.rotation.set(reduce ? 0.8 : t * 1.3, 0, reduce ? 0.4 : t * 0.5);
    }
    for (var hi = 0; hi < halos.length; hi++) halos[hi].rotation.y = reduce ? hi : t * 0.8 + hi;
    for (var c2 = 0; c2 < cabs.length; c2++) cabs[c2].m.position.y = 1 + (reduce ? 0.5 : Math.sin(t * 0.5 + cabs[c2].ph) * 0.5 + 0.5) * (ROOF_Y - 2);
    logoCube.rotation.set(0.45, reduce ? 0.6 : t * 0.45, 0);
    logo.position.y = ROOF_Y + 3.4 + (reduce ? 0 : Math.sin(t * 0.9) * 0.12);

    if (!reduce) {
      var arr = dgeo.attributes.position.array;
      for (var d = 0; d < DN; d++) {
        arr[d * 3 + 1] += dt * 0.14; arr[d * 3] += Math.sin(t * 0.3 + dseed[d]) * dt * 0.05;
        if (arr[d * 3 + 1] > 26) arr[d * 3 + 1] = 0;
      }
      dgeo.attributes.position.needsUpdate = true;
    }

    // HUD
    showChapter(chapterFor(target));
    railFill.style.height = (target * 100).toFixed(1) + '%';
    hudTop.classList.toggle('is-away', target > 0.04);
    hintEl.style.opacity = target > 0.02 ? '0' : '1';
  }
  var visible = true, elapsed = 0;
  function scheduleFrame() {
    if (!raf && visible && !document.hidden) raf = requestAnimationFrame(frame);
  }
  function frame() {
    raf = 0;
    if (!visible || document.hidden) return;
    var dt = Math.min(clock.getDelta(), 0.05);
    elapsed += dt;
    update(dt, elapsed);
    renderer.render(scene, camera);
    scheduleFrame();
  }
  var observer = new IntersectionObserver(function (entries) {
    visible = entries[0].isIntersecting;
    scheduleFrame();
  });
  document.addEventListener('visibilitychange', scheduleFrame);
  observer.observe(pinned);
  var nav = document.querySelector('.stock-nav');
  var navObserver = nav && window.ResizeObserver ? new ResizeObserver(function () {
    trackEl.style.setProperty('--tower-nav', nav.offsetHeight + 'px');
  }) : null;
  if (navObserver) navObserver.observe(nav);
  scheduleFrame();

  window.addEventListener('pagehide', function (event) {
    if (event.persisted) return;
    cancelAnimationFrame(raf);
    observer.disconnect();
    document.removeEventListener('visibilitychange', scheduleFrame);
    if (navObserver) navObserver.disconnect();
    if (ro) ro.disconnect();
    scene.traverse(function (o) {
      if (o.geometry) o.geometry.dispose();
      if (o.material) { (Array.isArray(o.material) ? o.material : [o.material]).forEach(function (mt) { if (mt.map) mt.map.dispose(); mt.dispose(); }); }
    });
    renderer.dispose();
  });
})();
