using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace XHLLauncher
{
    // Read-only checks. A matching file is not proof of gameplay behavior.
    public sealed class InstallationAudit
    {
        public const string FlightBuild = "4DE2ACDD3B18E562F129BC488A2DAEA2DA16B00341497304423E1E18C49A83BA";
        public bool FlightCompatible;
        public bool StackMatches;
        public bool LoadersIsolated;
        public string ExecutableHash = "Unavailable";
        public string Summary = "Checking installation";
        public string Details = "";
        public bool CanStart { get { return FlightCompatible && StackMatches && LoadersIsolated; } }

        public static string Hash(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        public static InstallationAudit Inspect(string root)
        {
            var result = new InstallationAudit();
            var notes = new StringBuilder();
            try
            {
                result.ExecutableHash = Hash(Path.Combine(root, "enshrouded_server.exe"));
                result.FlightCompatible = result.ExecutableHash.Equals(FlightBuild, StringComparison.OrdinalIgnoreCase);
                notes.AppendLine("Server executable SHA256: " + result.ExecutableHash);
                notes.AppendLine("Flight / gifting build: " + (result.FlightCompatible ? "Matches pinned build; runtime unverified." : "Unsupported by the installed pinned controllers."));
                string attestation = Path.Combine(root, "XHL-STACK-1.0-ATTESTATION.json");
                if (File.Exists(attestation))
                {
                    var data = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(File.ReadAllText(attestation));
                    string[] files = { "enshrouded_server.exe", "enshrouded_server.kfc", "enshrouded_server.kfc_resources" };
                    string[] keys = { "ExeSha256", "KfcSha256", "ResourcesSha256" };
                    result.StackMatches = true;
                    for (int i = 0; i < files.Length; i++)
                    {
                        object expected;
                        bool match = data.TryGetValue(keys[i], out expected) &&
                            string.Equals(i == 0 ? result.ExecutableHash : Hash(Path.Combine(root, files[i])), Convert.ToString(expected), StringComparison.OrdinalIgnoreCase);
                        result.StackMatches &= match;
                        notes.AppendLine(files[i] + ": " + (match ? "Matches stacking attestation" : "Does not match stacking attestation"));
                    }
                }
                else notes.AppendLine("Stacking attestation is missing.");
                result.LoadersIsolated = File.Exists(Path.Combine(root, "XHL-SAFE-MODE.flag"));
                foreach (string name in new[] { "winmm.dll", "dinput8.dll", "dbghelp.dll" })
                {
                    if (File.Exists(Path.Combine(root, name))) { result.LoadersIsolated = false; notes.AppendLine("Legacy loader present: " + name); }
                }
                notes.AppendLine("Safe mode / loader isolation: " + (result.LoadersIsolated ? "Present" : "Needs attention"));
                notes.AppendLine("These checks read files only. Memory and gameplay require separate verification.");
                result.Summary = result.CanStart ? "Installation checks passed" : "Compatibility needs attention";
            }
            catch (Exception ex) { result.Summary = "Installation could not be verified"; notes.AppendLine(ex.Message); }
            result.Details = notes.ToString();
            return result;
        }
    }
}
