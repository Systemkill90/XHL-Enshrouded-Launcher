using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace XHLLauncher
{
    // Formats *observed* launcher events. Never infers crafting, positions, combat or chat.
    internal static class ParsedEventView
    {
        internal static string Render(IEnumerable<ActivityItem> source, string search, int max)
        {
            string needle = (search ?? "").Trim();
            var matches = (source ?? Enumerable.Empty<ActivityItem>())
                .Where(e => e != null && (needle.Length == 0 ||
                    (e.Text ?? "").IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (e.Level ?? "").IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0))
                .OrderByDescending(e => e.Time)
                .Take(Math.Max(0, Math.Min(200, max))).ToArray();
            var sb = new StringBuilder("PARSED SERVER EVENTS — observed logs and local event history only.\r\n");
            sb.AppendLine("Events may be incomplete while the launcher is closed; no gameplay activity is inferred.");
            sb.AppendLine();
            if (matches.Length == 0) { sb.AppendLine("No matching recorded events."); return sb.ToString(); }
            foreach (var e in matches)
            {
                sb.Append(e.Time == DateTime.MinValue ? "Time unknown" : e.Time.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                sb.Append("  [").Append(string.IsNullOrWhiteSpace(e.Level) ? "Info" : e.Level).Append("]  ");
                // Do not let malformed multiline log messages imitate separate log records.
                sb.AppendLine((e.Text ?? "").Replace("\r", " ").Replace("\n", " "));
            }
            return sb.ToString();
        }
    }

    // Strict allowlist: no configuration JSON, paths, IPs, Steam IDs, player names,
    // credentials, device identifiers, raw events or arbitrary error text.
    internal static class SafeSupportReport
    {
        static string When(DateTime value)
        {
            return value == DateTime.MinValue ? "Not observed" :
                value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " (local)";
        }

        internal static string Readiness(HostState host, InstallationAudit audit, int backupMarkers)
        {
            string observed = host != null && host.ServerRunning ? "Process observed" : "No server process observed";
            string disk = audit == null ? "Compatibility unknown" : audit.CanStart ? "Pinned file checks pass" : "Pinned file checks need review";
            return observed + "  /  " + disk + "  /  " + (backupMarkers < 0 ? "Backup marker scan unavailable" : backupMarkers + " backup completion marker(s)");
        }

        internal static string Create(HostState host, WorldState world, InstallationAudit audit,
            int backupMarkers, DateTime latestBackup, DateTime captured)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SHROUDFORGE — PRIVACY-SAFE SUPPORT SNAPSHOT");
            sb.AppendLine("Captured: " + When(captured));
            sb.AppendLine("Read-only snapshot. Not an independent game/restore/backup verification.");
            sb.AppendLine("READINESS: " + Readiness(host, audit, backupMarkers));
            sb.AppendLine();
            sb.AppendLine("PROCESS");
            sb.AppendLine("Server process: " + (host != null && host.ServerRunning ? "Observed running" : "Not observed running"));
            sb.AppendLine("Process uptime: " + (host != null && host.ServerRunning ? FormatUtil.Duration(host.Uptime) : "Unavailable"));
            sb.AppendLine("Process CPU: " + (host != null && host.ServerRunning ? host.CpuPercent.ToString("0.0", CultureInfo.InvariantCulture) + "%" : "Unavailable"));
            sb.AppendLine("Working memory: " + (host != null && host.ServerRunning ? FormatUtil.Bytes(host.RamBytes) : "Unavailable"));
            sb.AppendLine("Free disk: " + (host != null && host.DiskTotalBytes > 0 ? FormatUtil.Bytes(host.DiskFreeBytes) : "Unavailable"));
            sb.AppendLine();
            sb.AppendLine("INSTALLATION / PINNED BUILD CHECKS");
            sb.AppendLine("Check available: " + (audit != null ? "Yes" : "No"));
            sb.AppendLine("Start-file checks: " + (audit == null ? "Unknown" : audit.CanStart ? "Pass (disk only)" : "Needs review"));
            sb.AppendLine("Mod controller build: " + (audit == null ? "Unknown" : audit.FlightCompatible ? "Pinned build matches" : "Mismatch/unknown"));
            sb.AppendLine("Stacked resources: " + (audit == null ? "Unknown" : audit.StackMatches ? "Attestation matches" : "Mismatch/unknown"));
            sb.AppendLine("Loader isolation: " + (audit == null ? "Unknown" : audit.LoadersIsolated ? "Flag present" : "Flag absent"));
            sb.AppendLine();
            sb.AppendLine("WORLD / BACKUPS");
            sb.AppendLine("Last observed world save: " + (world == null ? "Not observed" : When(world.LastSaveTime)));
            sb.AppendLine("World size estimate: " + (world == null ? "Unavailable" : FormatUtil.Bytes(world.WorldSaveBytes)));
            sb.AppendLine("Backup completion markers: " + (backupMarkers < 0 ? "Unavailable" : backupMarkers.ToString(CultureInfo.InvariantCulture)));
            sb.AppendLine("Most recent marked backup: " + When(latestBackup));
            sb.AppendLine("Note: completion marker count is not a new byte/hash verification.");
            sb.AppendLine();
            sb.AppendLine("EXCLUDED: player names, Steam IDs, IP addresses, paths, passwords, tokens,");
            sb.AppendLine("raw logs, command arguments, private phone URLs and configuration values.");
            sb.AppendLine("No server stop, restart, update, backup or configuration edit was requested.");
            return sb.ToString();
        }
    }
}
