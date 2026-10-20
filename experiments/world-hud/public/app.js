'use strict';

const canvas = document.querySelector('#radar');
const ctx = canvas.getContext('2d');
const signal = document.querySelector('#signal');
const mapStatus = document.querySelector('#mapStatus');
const time = document.querySelector('#time');
const weather = document.querySelector('#weather');
const phase = document.querySelector('#phase');
const weatherDetail = document.querySelector('#weatherDetail');
const updated = document.querySelector('#updated');

function drawRadar(snapshot) {
  const w = canvas.width, h = canvas.height, c = w / 2;
  ctx.clearRect(0, 0, w, h);
  const bg = ctx.createRadialGradient(c, c, 10, c, c, c);
  bg.addColorStop(0, '#344746'); bg.addColorStop(1, '#102123');
  ctx.fillStyle = bg; ctx.fillRect(0, 0, w, h);
  ctx.strokeStyle = 'rgba(180, 185, 148, .18)';
  ctx.lineWidth = 1;
  for (const r of [70, 140, 210]) {
    ctx.beginPath(); ctx.arc(c, c, r, 0, Math.PI * 2); ctx.stroke();
  }
  ctx.beginPath(); ctx.moveTo(c, 0); ctx.lineTo(c, h); ctx.moveTo(0, c); ctx.lineTo(w, c); ctx.stroke();

  if (snapshot.connection !== 'fresh' || !snapshot.player) {
    mapStatus.textContent = snapshot.connection === 'stale' ? 'Position feed stale' : 'Position unavailable';
    canvas.setAttribute('aria-label', mapStatus.textContent);
    return;
  }
  const player = snapshot.player;
  const pixelsPerWorldUnit = 210 / 300;
  const rotation = player.heading * Math.PI / 180;
  for (const marker of snapshot.markers) {
    const dx = marker.x - player.x, dy = marker.y - player.y;
    if (Math.hypot(dx, dy) > 300) continue;
    const x = c + dx * pixelsPerWorldUnit;
    const y = c - dy * pixelsPerWorldUnit;
    ctx.fillStyle = '#e1b968';
    ctx.beginPath(); ctx.arc(x, y, 5, 0, Math.PI * 2); ctx.fill();
    ctx.font = '14px Segoe UI, sans-serif';
    ctx.textAlign = 'center'; ctx.fillStyle = '#eadcc0';
    ctx.fillText(marker.label.slice(0, 24), x, y - 11, 130);
  }
  ctx.save(); ctx.translate(c, c); ctx.rotate(rotation);
  ctx.fillStyle = '#e8eadf'; ctx.strokeStyle = '#253d38'; ctx.lineWidth = 4;
  ctx.beginPath(); ctx.moveTo(0, -18); ctx.lineTo(-11, 14); ctx.lineTo(0, 9); ctx.lineTo(11, 14);
  ctx.closePath(); ctx.fill(); ctx.stroke(); ctx.restore();
  mapStatus.textContent = 'Position feed active';
  canvas.setAttribute('aria-label', 'Player-centered 300-unit range radar, heading ' + Math.round(player.heading) + ' degrees');
}

function display(snapshot) {
  const fresh = snapshot.connection === 'fresh';
  signal.textContent = fresh ? 'FEED RECEIVED' : snapshot.connection === 'stale' ? 'STALE FEED' : 'NO FEED';
  signal.classList.toggle('fresh', fresh);
  const mins = fresh ? snapshot.worldMinute : null;
  time.textContent = Number.isInteger(mins) ? String(Math.floor(mins / 60)).padStart(2, '0') + ':' + String(mins % 60).padStart(2, '0') : '--:--';
  phase.textContent = Number.isInteger(mins) ? (mins >= 360 && mins < 1080 ? 'Daytime · observed' : 'Nighttime · observed') : 'Awaiting game time';
  weather.textContent = fresh && snapshot.weather && snapshot.weather !== 'unknown' ?
    snapshot.weather.charAt(0).toUpperCase() + snapshot.weather.slice(1) : 'Unknown';
  weatherDetail.textContent = fresh && snapshot.weather ? 'Publisher-reported' : 'Awaiting game weather';
  updated.textContent = snapshot.receivedAt ?
    (fresh ? 'Received ' : 'Last seen ') + new Date(snapshot.receivedAt).toLocaleTimeString() : 'No telemetry received';
  drawRadar(snapshot);
}

async function refresh() {
  try {
    const response = await fetch('/api/status', { cache: 'no-store' });
    if (!response.ok) throw new Error('Status unavailable');
    display(await response.json());
  } catch (_) {
    display({ connection: 'not-connected', receivedAt: null, worldMinute: null, weather: null, player: null, markers: [] });
    signal.textContent = 'NETWORK ERROR';
  }
}

refresh();
setInterval(refresh, 3000);
