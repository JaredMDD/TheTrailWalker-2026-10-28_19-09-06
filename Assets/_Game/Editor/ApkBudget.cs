using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build.Reporting;

namespace TheTrailWalker.Editor
{
    /// <summary>Test APK size gate: reports where Resources bytes go and fails the build above the budget.</summary>
    public static class ApkBudget
    {
        public const long BudgetBytes = 150L * 1024 * 1024;
        private const string ResourcesPrefix = "Assets/_Game/Resources/";

        public static IReadOnlyDictionary<string, ulong> ByResourcesFolder(IEnumerable<(string path, ulong size)> packed)
        {
            var sums = new Dictionary<string, ulong>();
            foreach (var (path, size) in packed)
            {
                if (path == null || !path.StartsWith(ResourcesPrefix, StringComparison.Ordinal)) continue;
                string[] parts = path.Substring(ResourcesPrefix.Length).Split('/');
                string key = parts.Length >= 3 ? parts[0] + "/" + parts[1] : parts.Length == 2 ? parts[0] : "(root)";
                sums[key] = (sums.TryGetValue(key, out ulong v) ? v : 0) + size;
            }
            return sums;
        }

        public static void Enforce(string apkPath, BuildReport report)
        {
            long total = new FileInfo(apkPath).Length;
            var packed = report.packedAssets.SelectMany(a => a.contents).Select(c => (c.sourceAssetPath, c.packedSize));
            var lines = new List<string> { $"APK total: {total / 1048576.0:F2} MB (budget {BudgetBytes / 1048576} MB)" };
            // The file can be larger than its entries (dead space left by incremental APK packaging); a large gap here, not
            // the assets, is what moves the size between builds with the same content (investigated November 14, 2026).
            try
            {
                using (var zip = System.IO.Compression.ZipFile.OpenRead(apkPath))
                {
                    long entries = zip.Entries.Sum(e => e.CompressedLength);
                    lines.Add($"Entries: {zip.Entries.Count} files, {entries / 1048576.0:F2} MB compressed; the rest of the file (headers, signing, any dead space): {(total - entries) / 1048576.0:F2} MB");
                }
            }
            catch (Exception e) { lines.Add("Entries: not measured (" + e.Message + ")"); }
            var folders = ByResourcesFolder(packed);
            // Unity records packed assets only on full builds; an incremental build's report has none.
            lines.Add(folders.Count == 0 ? "Packed Resources folders: not recorded (incremental build; Unity lists packed assets only on full builds)" : "Packed Resources folders, largest first:");
            lines.AddRange(folders.OrderByDescending(p => p.Value).Select(p => $"  Resources/{p.Key}: {p.Value / 1048576.0:F2} MB"));
            Directory.CreateDirectory("Builds/Validation");
            File.WriteAllLines("Builds/Validation/apk-size.txt", lines);
            if (total > BudgetBytes) throw new Exception($"APK is {total / 1048576.0:F2} MB, over the {BudgetBytes / 1048576} MB budget. See Builds/Validation/apk-size.txt.");
        }
    }
}
