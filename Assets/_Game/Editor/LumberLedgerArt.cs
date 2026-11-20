using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Renders the four log pictures for the lumber ledger page (Resources/UI/Lumber/<Wood>Log.png).
    public static class LumberLedgerArt
    {
        // Tier order; mesquite and oak are still the camp log seats until real logs land.
        public static readonly string[] Sources =
        {
            "Tier01/Drops/PineLog", "Tier02/Drops/BirchLog",
            "Environment/CampProps/MesquiteLogSeat", "Environment/CampProps/OakLogSeat",
        };

        [MenuItem("The Trail Walker/Art/Build Lumber Ledger Pictures")]
        public static void Build()
        {
            Directory.CreateDirectory("Assets/_Game/Resources/UI/Lumber");
            AssetDatabase.Refresh(); // the importer rule must see the folder before the PNGs land
            for (int t = 1; t <= Sources.Length; t++)
            {
                var prefab = Resources.Load<GameObject>(Sources[t - 1]);
                if (prefab == null) throw new System.Exception("Lumber ledger art: missing prefab " + Sources[t - 1]);
                var instance = Object.Instantiate(prefab); instance.transform.position = Vector3.zero;
                var bounds = new Bounds(Vector3.zero, Vector3.zero); bool first = true;
                foreach (var r in instance.GetComponentsInChildren<Renderer>()) { if (first) { bounds = r.bounds; first = false; } else bounds.Encapsulate(r.bounds); }
                Object.DestroyImmediate(instance);
                var centre = bounds.center;
                float size = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * 0.62f + 0.05f;
                FurnitureArtAssets.Render(prefab, "Assets/_Game/Resources/" + CollectionPrototype.LedgerArtPath(t) + ".png",
                    centre + new Vector3(-6, 4.5f, 8).normalized * 12, centre, size, 512, 512, new Color(0, 0, 0, 0));
            }
            AssetDatabase.Refresh();
        }
    }
}
