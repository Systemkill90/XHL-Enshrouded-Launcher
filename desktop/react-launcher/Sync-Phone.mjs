import fs from 'node:fs';
import {fileURLToPath} from 'node:url';
const core=fileURLToPath(new URL('../field-audit-launcher/',import.meta.url));
const html=fs.readFileSync(core+'phone/phone-ui.html','utf8');
const path=core+'src/PhoneView.cs';const source=fs.readFileSync(path,'utf8');
const start=source.indexOf('const string Page=@"');
const end=source.indexOf('";',start+20);
if(start<0||end<0)throw Error('Embedded phone page anchor missing');
const synced=source.slice(0,start)+'const string Page=@"'+html.replaceAll('"','""')+source.slice(end);
if(process.argv.includes('--check')){if(synced!==source)throw Error('Embedded and standalone phone HTML differ');console.log('PASS exact embedded/standalone phone synchronization');}
else fs.writeFileSync(path,synced);
