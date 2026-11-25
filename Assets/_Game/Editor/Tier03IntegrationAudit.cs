using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Settlement;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Tier 3 settlement integration, slice A step 0 (read-only). For every runtime tier 3 item and every display id in the
    // FINAL02 ledger: does the id exist at runtime, where can the player meet it, and which model does the real visual
    // lookup show today. Writes Temp/tier3-audit.json; changes no asset.
    public static class Tier03IntegrationAudit
    {
        private const string Ledger = "Docs/Art/Tier03/BatchFINAL02/coverage-audit.json";
        private const string Output = "Temp/tier3-audit.json";

        [System.Serializable] private class LedgerFile { public LedgerRow[] Displays; }
        [System.Serializable] private class LedgerRow { public string ItemId, Model, Packet; public int Triangles; }
        [System.Serializable] private class Row
        {
            public string ItemId, Name, Model, LedgerModel, LedgerPacket;
            public bool Runtime, InLedger;
            public int Tier;
            public List<string> Sources = new List<string>();
        }
        [System.Serializable] private class Report { public List<Row> Rows = new List<Row>(); }

        [MenuItem("The Trail Walker/Tier 3/Integration audit (read-only)")]
        public static void Run()
        {
            var ledger = JsonUtility.FromJson<LedgerFile>(File.ReadAllText(Ledger)).Displays;
            var defs = new Dictionary<string, ItemDefinition>();
            foreach (var group in new[] { PrototypeItems.All, HarvestTierItems.All, ShopItems.All, BakeryItems.All, SpecialGear.All, BoxItems.All })
                foreach (ItemDefinition d in group) if (!defs.ContainsKey(d.Id)) defs[d.Id] = d;

            var sources = new Dictionary<string, List<string>>();
            void Add(string id, string where) { if (id == null) return; if (!sources.TryGetValue(id, out var l)) sources[id] = l = new List<string>(); if (!l.Contains(where)) l.Add(where); }
            foreach (ShopEntry e in ShopCatalog.All) Add(e.ItemId, e.ShopId + (e.Region == 0 ? " t" + e.Tier : " r" + e.Region));
            foreach (RecipeDefinition r in RecipeBook.All)
            {
                string kind = r.MysteryBoxEligible ? "box recipe" : BakeryRecipes.All.Contains(r) ? "bakery recipe" : "recipe";
                Add(r.OutputId, kind + " output");
                foreach (RecipeInput i in r.Inputs) Add(i.ItemId, kind + " input");
            }
            foreach (ItemDefinition d in SpecialGear.All) Add(d.Id, "balloon special gear");

            var ids = new SortedSet<string>(defs.Keys.Where(id => ItemTiers.Of(id) == 3 || id.StartsWith("special.t3.")));
            foreach (LedgerRow l in ledger) ids.Add(l.ItemId);

            var attach = typeof(Tier2ShopVisuals).Assembly.GetType("TheTrailWalker.ResourceVisualTuning").GetMethod("Attach", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var report = new Report();
            foreach (string id in ids)
            {
                defs.TryGetValue(id, out ItemDefinition d);
                LedgerRow l = ledger.FirstOrDefault(x => x.ItemId == id);
                var row = new Row { ItemId = id, Runtime = d != null, Name = d?.Name, Tier = ItemTiers.Of(id), InLedger = l != null, LedgerModel = l?.Model, LedgerPacket = l?.Packet };
                if (sources.TryGetValue(id, out var s)) row.Sources = s;
                if (d != null)
                {
                    var probe = new GameObject("tier3 audit probe", typeof(MeshRenderer), typeof(MeshFilter));
                    try { var visual = (GameObject)attach.Invoke(null, new object[] { probe, d }); row.Model = visual == null ? null : visual.name.Replace("(Clone)", ""); }
                    finally { Object.DestroyImmediate(probe); }
                }
                report.Rows.Add(row);
            }
            Directory.CreateDirectory("Temp");
            File.WriteAllText(Output, JsonUtility.ToJson(report, true));
            Debug.Log("Tier 3 integration audit: " + report.Rows.Count + " rows written to " + Output);
        }
    }
}
