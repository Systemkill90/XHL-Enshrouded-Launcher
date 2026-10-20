'use strict';

// Standalone read-only World HUD web service. It does not hook into Enshrouded.
const http = require('node:http');
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');

const STALE_AFTER_MS = 90_000;
const MAX_BODY_BYTES = 64 * 1024;
const PUBLIC_DIR = path.join(__dirname, 'public');

function finiteCoordinate(value) {
  return typeof value === 'number' && Number.isFinite(value) && Math.abs(value) <= 10_000_000;
}

function validateTelemetry(input) {
  if (!input || typeof input !== 'object' || Array.isArray(input)) {
    throw new Error('Telemetry must be an object.');
  }
  const minute = input.worldMinute;
  if (minute !== null && minute !== undefined && (!Number.isInteger(minute) || minute < 0 || minute >= 1440)) {
    throw new Error('worldMinute must be a minute of day (0-1439) or null.');
  }
  const weatherOptions = ['clear', 'cloudy', 'rain', 'storm', 'fog', 'snow', 'unknown'];
  const weather = input.weather == null ? null : input.weather;
  if (weather !== null && !weatherOptions.includes(weather)) {
    throw new Error('Unsupported weather state.');
  }
  let player = null;
  if (input.player != null) {
    const p = input.player;
    if (!p || typeof p !== 'object' || !finiteCoordinate(p.x) || !finiteCoordinate(p.y) ||
        typeof p.heading !== 'number' || !Number.isFinite(p.heading) || p.heading < 0 || p.heading >= 360) {
      throw new Error('player must contain finite x/y and heading (0-359.999).');
    }
    player = { x: p.x, y: p.y, heading: p.heading };
  }
  if (input.markers != null && (!Array.isArray(input.markers) || input.markers.length > 100)) {
    throw new Error('markers must be an array of 100 or fewer items.');
  }
  const markers = (input.markers || []).map((m) => {
    if (!m || typeof m !== 'object' || typeof m.label !== 'string' || m.label.length < 1 || m.label.length > 48 ||
        !finiteCoordinate(m.x) || !finiteCoordinate(m.y)) {
      throw new Error('Each marker needs a short label and finite x/y.');
    }
    return { label: m.label, x: m.x, y: m.y };
  });
  return { worldMinute: minute == null ? null : minute, weather, player, markers };
}

function tokenMatches(expected, supplied) {
  if (!expected || expected.length < 32 || typeof supplied !== 'string' || !supplied.startsWith('Bearer ')) return false;
  const received = supplied.slice(7);
  const a = Buffer.from(expected), b = Buffer.from(received);
  return a.length === b.length && crypto.timingSafeEqual(a, b);
}

function createHudServer(options = {}) {
  const token = options.token === undefined ? process.env.HUD_INGEST_TOKEN : options.token;
  const now = options.now || Date.now;
  let latest = null;
  let receivedAt = null;
  const assets = {
    '/': ['index.html', 'text/html; charset=utf-8'],
    '/app.css': ['app.css', 'text/css; charset=utf-8'],
    '/app.js': ['app.js', 'text/javascript; charset=utf-8']
  };

  function send(res, status, data, type = 'application/json; charset=utf-8') {
    res.writeHead(status, {
      'Content-Type': type, 'Cache-Control': 'no-store', 'X-Content-Type-Options': 'nosniff',
      'Referrer-Policy': 'no-referrer', 'X-Frame-Options': 'SAMEORIGIN',
      'Content-Security-Policy': "default-src 'none'; script-src 'self'; style-src 'self'; connect-src 'self'; img-src 'self' data:; frame-ancestors 'self'; base-uri 'none'"
    });
    res.end(typeof data === 'string' || Buffer.isBuffer(data) ? data : JSON.stringify(data));
  }

  return http.createServer(async (req, res) => {
    const pathname = (req.url || '').split('?')[0];
    if (req.method === 'GET' && pathname === '/healthz') return send(res, 200, { ok: true, service: 'shroudforge-world-hud' });
    if (req.method === 'GET' && pathname === '/api/status') {
      const age = receivedAt == null ? null : now() - receivedAt;
      return send(res, 200, {
        connection: latest == null ? 'not-connected' : age <= STALE_AFTER_MS && age >= 0 ? 'fresh' : 'stale',
        source: latest == null ? 'none' : 'external-publisher-unverified',
        receivedAt: receivedAt == null ? null : new Date(receivedAt).toISOString(),
        ageSeconds: age == null ? null : Math.max(0, Math.floor(age / 1000)),
        worldMinute: latest ? latest.worldMinute : null,
        weather: latest ? latest.weather : null,
        player: latest ? latest.player : null,
        markers: latest ? latest.markers : []
      });
    }
    if (req.method === 'POST' && pathname === '/api/telemetry') {
      if (!token || token.length < 32) return send(res, 503, { error: 'Publisher disabled: configure a strong HUD_INGEST_TOKEN.' });
      if (!tokenMatches(token, req.headers.authorization)) return send(res, 401, { error: 'Unauthorized publisher.' });
      if (!(req.headers['content-type'] || '').startsWith('application/json')) {
        return send(res, 415, { error: 'Expected application/json.' });
      }
      try {
        let total = 0;
        const chunks = [];
        for await (const chunk of req) {
          total += chunk.length;
          if (total > MAX_BODY_BYTES) return send(res, 413, { error: 'Payload too large.' });
          chunks.push(chunk);
        }
        const next = validateTelemetry(JSON.parse(Buffer.concat(chunks).toString('utf8')));
        latest = next;
        receivedAt = now();
        return send(res, 202, { accepted: true, receivedAt: new Date(receivedAt).toISOString() });
      } catch (error) {
        return send(res, 400, { error: 'Invalid JSON or telemetry fields.' });
      }
    }
    if (req.method === 'GET' && Object.prototype.hasOwnProperty.call(assets, pathname)) {
      const [filename, type] = assets[pathname];
      return fs.readFile(path.join(PUBLIC_DIR, filename), (err, body) => {
        if (err) return send(res, 500, { error: 'HUD asset missing.' });
        send(res, 200, body, type);
      });
    }
    return send(res, 404, { error: 'Not found.' });
  });
}

if (require.main === module) {
  const host = process.env.HUD_HOST || '127.0.0.1';
  const port = Number(process.env.HUD_PORT || 8787);
  if (!Number.isInteger(port) || port < 1 || port > 65535) throw new Error('Invalid HUD_PORT.');
  createHudServer().listen(port, host, () => {
    console.log('World HUD available at http://' + host + ':' + port + '/');
    if (host !== '127.0.0.1' && host !== '::1' && host !== 'localhost') {
      console.warn('WARNING: Network binding enabled. Protect this service with authenticated HTTPS reverse proxy before wider exposure.');
    }
  });
}

module.exports = { createHudServer, validateTelemetry, STALE_AFTER_MS };
