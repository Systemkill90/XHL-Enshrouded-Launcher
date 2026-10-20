export type Command='State'|'Servers'|'Start'|'Stop'|'Restart'|'Backup'|'Restore'|'Recovery'|'AutoBackup'|'Capture'|'Refresh'|'ModStartup'|'RemoveMod'|'SaveField'|'Classic'|'OpenMap'|'CopyPhone'|'ImportMod';
export type Player={name:string;online:boolean;steamId:string;profile:Record<'Name'|'Status'|'Identity'|'Sessions'|'Connection'|'Incidents'|'Activity'|'Limitations',string>;role:string;ping:number;quality:number;reason:string;seen:string;networkSample:string|null;session:string;sessionHistorical:boolean;lastPing:number;lastQuality:number;historyRecordedUtc:string|null};
export type ConfigField={path:string;key:string;label:string;section:string;value:string|number|boolean;editable:boolean;reason:string|null;issue:string|null;min:number|null;max:number|null;choices:{Value:string;Text:string}[]|null};
export type State={
 name:string;online:boolean;ready:boolean;observedUtc:string;busy:boolean;operation:string;
 scanError:string;scanAttemptUtc:string;scanObservedUtc:string|null;processObservation:'Unknown'|'Running'|'Stopped'|'Failed';observationError:string;
 readOnly:boolean;version:string;pid:number;uptime:string;cpu:number;memory:string;count:string;countSource:string;operationFailed:boolean;compatible:boolean;audit:string;auditDetails:string;
 cpuHistory:number[];memoryHistory:number[];players:Player[];
 files:{available:boolean;count:number;size:string;lastWrite:string;error:string|null;items:{name:string;size:string;written:string}[]};
 backups:{id:string;date:string;kind:string;sort:string;offline:boolean}[];
 config:ConfigField[];mods:{id:string;name:string;version:string;kind:string;builtIn:boolean;locked:boolean;configured:boolean;detail:string}[];runtime:string;
 resilience:{enabled:boolean;phase:string;detail:string;attempts:number;budget:number;manualPause:boolean;dueUtc:string|null;locked:boolean};
 backupSchedule:{NextUtc:string;LastAttemptUtc:string;LastSuccessUtc:string;Failures:number;Paused:boolean;Outcome:string;Phase:string};autoBackup:boolean;backupMinutes:number;nextBackup:string;
 incident:{CapturedUtc:string;Trigger:string;Conclusion:string;Items:{Source:string;Name:string;Text:string;Path:string;WrittenUtc:string}[]}|null;
 logTail:string|null;activity:{time:string;level:string;message:string}[];capabilities:{kick:boolean;ban:boolean;individualRoles:boolean;reason:string};
};
export type HostMessage={data:{type:'state';data:State}};
