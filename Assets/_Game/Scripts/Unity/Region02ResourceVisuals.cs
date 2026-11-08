using TheTrailWalker.Domain.Items;
using UnityEngine;

namespace TheTrailWalker
{
    internal static class Region02ResourceVisuals
    {
        public static GameObject Attach(GameObject pickup, ItemDefinition definition)
        {
            string path = definition.Id == PrototypeItems.Hemp.Id ? "Region02/HempFiber" :
                definition.Id == PrototypeItems.Flint.Id ? "Region02/Flint" :
                definition.Id == PrototypeItems.PineCone.Id ? "Region02/PineCone" : null;
            return Region01ResourceVisuals.AttachPath(pickup, path);
        }
    }
}
