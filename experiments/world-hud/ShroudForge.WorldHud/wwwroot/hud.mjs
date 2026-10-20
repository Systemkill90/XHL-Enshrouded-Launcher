const WEATHER = {
  clear: "☀ Clear", cloudy: "☁ Cloudy", rain: "☂ Rain",
  snow: "❄ Snow", blizzard: "❄ Blizzard", fog: "≋ Fog", storm: "ϟ Storm"
};

export function validWorldTime(value) {
  return typeof value === "string" && /^(?:[01][0-9]|2[0-3]):[0-5][0-9]$/.test(value);
}

export function weatherText(value) {
  return typeof value === "string" ? (WEATHER[value.toLowerCase()] ?? "Unknown") : "Unknown";
}

export function safeMapPath(value) {
  return typeof value === "string"
    && /^\/maps\/[a-zA-Z0-9_-]+(?:\/[a-zA-Z0-9_-]+)*\.(?:png|webp|jpg|jpeg)$/.test(value);
}

export function mapFractions(player, bounds) {
  if (!player || !bounds) return null;
  const values = [player.x, player.z, bounds.minX, bounds.maxX, bounds.minZ, bounds.maxZ];
  if (!values.every(value => typeof value === "number" && Number.isFinite(value))) return null;
  if (bounds.minX >= bounds.maxX || bounds.minZ >= bounds.maxZ) return null;
  const x = (player.x - bounds.minX) / (bounds.maxX - bounds.minX);
  const z = (player.z - bounds.minZ) / (bounds.maxZ - bounds.minZ);
  return x >= 0 && x <= 1 && z >= 0 && z <= 1 ? {x, z} : null;
}

export function hudReading(response) {
  if (!response || response.status !== "fresh" || !response.snapshot) return null;
  return response.snapshot;
}

function setText(id, value) {
  document.getElementById(id).textContent = value;
}

function render(response) {
  const snapshot = hudReading(response);
  const connection = document.getElementById("connection");
  const map = document.getElementById("map");
  const position = document.getElementById("position");
  const mapMessage = document.getElementById("map-message");

  map.style.backgroundImage = "";
  map.style.backgroundSize = "";
  map.style.backgroundPosition = "";
  map.classList.remove("with-image");
  position.hidden = true;
  mapMessage.hidden = false;
  setText("coordinates", "Position unknown");
  setText("world-time", "--:--");
  setText("weather", "Unknown");

  if (!snapshot) {
    const stale = response?.status === "stale";
    connection.textContent = stale ? "Telemetry stale" : "Offline";
    connection.classList.remove("live");
    mapMessage.textContent = stale ? "Waiting for updated game data" : "Map data unavailable";
    setText("source", stale ? "Old game readings hidden" : "Awaiting a game telemetry provider");
    return;
  }

  connection.textContent = "Feed connected";
  connection.classList.add("live");
  setText("world-time", validWorldTime(snapshot.worldTime) ? snapshot.worldTime : "--:--");
  setText("weather", weatherText(snapshot.weather));
  setText("source", "Source: " + (typeof snapshot.source === "string"
    ? snapshot.source.slice(0, 65) : "unidentified provider"));

  const url = snapshot.map?.imageUrl;
  const coords = mapFractions(snapshot.player, snapshot.map?.bounds);
  if (!safeMapPath(url) || !coords) {
    mapMessage.textContent = "Map / position not supplied";
    return;
  }

  const viewport = map.clientWidth || 230;
  const size = viewport * 5;
  const offsetX = viewport / 2 - coords.x * size;
  const offsetY = viewport / 2 - coords.z * size;
  map.style.backgroundImage = 'url("' + url + '")';
  map.style.backgroundSize = size + "px " + size + "px";
  map.style.backgroundPosition = offsetX + "px " + offsetY + "px";
  map.style.backgroundRepeat = "no-repeat";
  map.classList.add("with-image");
  const heading = Number.isFinite(snapshot.player.heading) ? snapshot.player.heading : 0;
  position.style.setProperty("--heading", heading + "deg");
  position.hidden = false;
  mapMessage.hidden = true;
  setText("coordinates", Math.round(snapshot.player.x) + ", " + Math.round(snapshot.player.z));
}

if (typeof document !== "undefined") {
  let inFlight = false;
  async function refresh() {
    if (inFlight) return;
    inFlight = true;
    try {
      const response = await fetch("/api/hud", {cache: "no-store"});
      if (!response.ok) throw new Error("HTTP error");
      render(await response.json());
    } catch {
      render({status: "unavailable"});
    } finally {
      inFlight = false;
    }
  }
  refresh();
  setInterval(refresh, 5000);
}
