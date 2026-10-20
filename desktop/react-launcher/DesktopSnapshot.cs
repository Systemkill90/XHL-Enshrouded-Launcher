using System;
namespace XHLLauncher {
 internal sealed class DesktopSnapshot {
  public string scanError,scanAttemptUtc,scanObservedUtc,processObservation,observationError,version,name,observedUtc,uptime,memory,count,countSource,operation,audit,auditDetails,runtime,nextBackup,logTail;
  public bool readOnly,online,ready,busy,operationFailed,compatible,autoBackup;
  public int pid,backupMinutes;public double cpu;public double[] cpuHistory,memoryHistory;
  public PlayerRow[] players;public WorldFiles files;public BackupRow[] backups;public ConfigRow[] config;public ModRow[] mods;
  public ResilienceSnapshot resilience;public BackupScheduleRecord backupSchedule;public IncidentBundle incident;public ActivityRow[] activity;public CapabilitySnapshot capabilities;
 }
 internal sealed class PlayerRow {public string name,steamId,role,reason,seen,networkSample,session;public bool online;public double ping,quality;public PlayerSheetSnapshot profile;}
 internal sealed class ResilienceSnapshot {public bool enabled,manualPause,locked;public string phase,detail,dueUtc;public int attempts,budget;}
 internal sealed class ActivityRow {public string time,level,message;}
 internal sealed class CapabilitySnapshot {public bool kick,ban,individualRoles;public string reason;}
}
