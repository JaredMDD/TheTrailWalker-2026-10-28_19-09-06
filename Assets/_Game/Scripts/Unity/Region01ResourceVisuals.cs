using System.Collections.Generic;
using TheTrailWalker.Domain.Items;
using UnityEngine;

namespace TheTrailWalker
{
    internal static class Region01ResourceVisuals
    {
        private static readonly Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();

        public static GameObject Attach(GameObject pickup, ItemDefinition definition)
        {
            string name = definition.Id == PrototypeItems.Branch.Id ? "PineBranch" :
                definition.Id == PrototypeItems.RabbitPelt.Id ? "RabbitPelt" :
                definition.Id == PrototypeItems.Apples.Id ? "Apple" : null;
            if (name == null) return null;
            return AttachPath(pickup, "Region01/" + name);
        }

        public static GameObject AttachPath(GameObject pickup, string path)
        {
            if (path == null) return null;
            if (!Prefabs.TryGetValue(path, out GameObject prefab) || prefab == null)
            {
                prefab = Resources.Load<GameObject>(path);
                if (prefab != null) Prefabs[path] = prefab;
            }
            if (prefab == null) return null; // Source import may still be pending; keep the prototype fallback visible.
            GameObject visual = Object.Instantiate(prefab, pickup.transform, false);
            pickup.GetComponent<Renderer>().enabled = false;
            return visual;
        }
    }
}
