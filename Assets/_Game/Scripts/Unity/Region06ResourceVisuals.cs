using TheTrailWalker.Domain.Items;
using UnityEngine;

namespace TheTrailWalker
{
    internal static class Region06ResourceVisuals
    {
        public static GameObject Attach(GameObject pickup,ItemDefinition definition)
        {
            string path=definition.Id==PrototypeItems.Reeds.Id?"Region06/Reeds":
                definition.Id==PrototypeItems.Wheat.Id?"Region06/Wheat":
                definition.Id==PrototypeItems.Sand.Id?"Region06/Sand":null;
            return Region01ResourceVisuals.AttachPath(pickup,path);
        }
    }
}
