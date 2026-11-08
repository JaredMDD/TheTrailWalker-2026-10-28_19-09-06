using TheTrailWalker.Domain.Items;
using UnityEngine;

namespace TheTrailWalker
{
    internal static class Region03ResourceVisuals
    {
        public static GameObject Attach(GameObject pickup, ItemDefinition definition)
        {
            string path = definition.Id == PrototypeItems.NettleFiber.Id ? "Region03/NettleFiber" :
                definition.Id == PrototypeItems.Clay.Id ? "Region03/Clay" :
                definition.Id == PrototypeItems.RockSalt.Id ? "Region03/RockSalt" : null;
            return Region01ResourceVisuals.AttachPath(pickup, path);
        }
    }
}
