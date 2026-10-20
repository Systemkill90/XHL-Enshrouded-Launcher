import test from "node:test";
import assert from "node:assert/strict";
import {validWorldTime, weatherText, safeMapPath, mapFractions, hudReading} from "../ShroudForge.WorldHud/wwwroot/hud.mjs";

test("world time is strictly 24-hour game time", () => {
  assert.equal(validWorldTime("00:00"), true);
  assert.equal(validWorldTime("23:59"), true);
  assert.equal(validWorldTime("24:00"), false);
  assert.equal(validWorldTime("1:09"), false);
  assert.equal(validWorldTime("not real"), false);
});
test("weather labels reject unsupported values", () => {
  assert.equal(weatherText("rain"), "☂ Rain");
  assert.equal(weatherText("BLIZZARD"), "❄ Blizzard");
  assert.equal(weatherText("<script>"), "Unknown");
});
test("map paths are constrained to local server-owned assets", () => {
  assert.equal(safeMapPath("/maps/embervale.webp"), true);
  assert.equal(safeMapPath("/maps/local/map.png"), true);
  assert.equal(safeMapPath("https://elsewhere/map.png"), false);
  assert.equal(safeMapPath("/maps/../secret.png"), false);
  assert.equal(safeMapPath("/maps/x.png?token=abc"), false);
});
test("position mapping requires explicit valid map bounds", () => {
  const b = {minX:-100,maxX:100,minZ:-100,maxZ:100};
  assert.deepEqual(mapFractions({x:0,z:0},b),{x:0.5,z:0.5});
  assert.deepEqual(mapFractions({x:100,z:-100},b),{x:1,z:0});
  assert.equal(mapFractions({x:Infinity,z:0},b),null);
  assert.equal(mapFractions({x:1000,z:0},b),null);
  assert.equal(mapFractions({x:0,z:0},{...b,maxX:-100}),null);
});
test("stale and unavailable snapshots are never displayed as live", () => {
  const snapshot = {worldTime:"12:30",weather:"rain"};
  assert.deepEqual(hudReading({status:"fresh",snapshot}),snapshot);
  assert.equal(hudReading({status:"stale",snapshot}),null);
  assert.equal(hudReading({status:"unavailable",snapshot}),null);
});
