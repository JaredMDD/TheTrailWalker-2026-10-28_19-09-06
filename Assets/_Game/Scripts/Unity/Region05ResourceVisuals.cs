using TheTrailWalker.Domain.Items;
using UnityEngine;

namespace TheTrailWalker
{
    internal static class Region05ResourceVisuals
    {
        public static GameObject Attach(GameObject pickup,ItemDefinition definition)
        {
            string path=definition.Id==PrototypeItems.Oats.Id?"Region05/Oats":
                definition.Id==PrototypeItems.Pebbles.Id?"Region05/Pebbles":
                definition.Id==PrototypeItems.Berries.Id?"Region05/RedBerries":null;
            return Region01ResourceVisuals.AttachPath(pickup,path);
        }
    }
}
