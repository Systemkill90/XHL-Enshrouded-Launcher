// Offline, no-server contract checks for the ShroudForge phone companion.
// Runs on CI with Node; never opens an authenticated endpoint or starts the game.
import assert from 'node:assert/strict';
import fs from 'node:fs';
import {fileURLToPath} from 'node:url';

const core=fileURLToPath(new URL('../field-audit-launcher/',import.meta.url));
const html=fs.readFileSync(core+'phone/phone-ui.html','utf8');
const phoneSource=fs.readFileSync(core+'src/PhoneView.cs','utf8');
const start=phoneSource.indexOf('const string Page=@"');
const end=phoneSource.indexOf('</html>";',start+20)+'</html>'.length;
assert(start>=0&&end>start,'Expected embedded C# page');
const synchronized=phoneSource.slice(0,start)+'const string Page=@"'+html.replaceAll('"','""')+phoneSource.slice(end);
assert.equal(phoneSource,synchronized,'Standalone and embedded phone pages must match exactly');

const scripts=[...html.matchAll(/<script(?:\s[^>]*)?>([\s\S]*?)<\/script>/g)];
assert.equal(scripts.length,1,'Exactly one internal phone UI script');
new Function(scripts[0][1]); // Syntax check only; no DOM, network or privileged operation.
const ids=[...html.matchAll(/\bid="([^"]+)"/g)].map(m=>m[1]);
assert.equal(new Set(ids).size,ids.length,'No duplicate HTML element IDs');
for(const id of ['dashboardView','playersView','activityView','mapView','overviewTab','playersTab','activityTab','mapTab','roster','knownPlayers','activity','stateText','lastBackup','backup','start','restart','stop','autoBackup','recovery','openWorldMap','gameMap']){
 assert(ids.includes(id),'Expected dynamic control: '+id);
}
assert.equal((html.match(/<button[^>]*\brole="tab"/g)||[]).length,4,'Four accessible phone sections');
assert(html.includes("event.key==='ArrowRight'")&&html.includes("event.key==='Home'"),'Four-tab navigation supports keyboard arrows and Home/End');
assert(html.includes('id="result"')&&html.indexOf('id="result"')>html.indexOf('</details>'),'Operation feedback is visible even when controls are collapsed');
assert(html.includes('env(safe-area-inset-bottom')&&html.includes('prefers-reduced-motion'),'Safe area and reduced motion styling required');
assert(html.includes('class="control-details"')&&html.includes("run('Restart')")&&html.includes("run('Stop')"),'Existing guarded game controls preserved');
assert(html.includes('id="openWorldMap" href="https://enshrouded.gamevault.in/map" target="_blank" rel="noopener noreferrer" referrerpolicy="no-referrer"'),'Working HTTPS map is first-class safe action');
assert(html.includes('class="map-experiment"')&&html.includes('window.isSecureContext===false'),'Embedded map remains optional with an insecure-parent diagnostic');
assert(!/<script[^>]+src=/.test(html),'No unreviewed remote scripts');
assert(scripts[0][1].includes('fetch(\'status\'')&&scripts[0][1].includes("fetch('command/'+op"),'Existing private status and command request paths retained');
console.log('PASS phone UI syntax, responsive controls, safe-map behavior, authentication paths, and synchronized fallback');
