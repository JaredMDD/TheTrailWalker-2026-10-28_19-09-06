using TheTrailWalker.Domain.Items;
using UnityEngine;

namespace TheTrailWalker
{
    internal static class Region04ResourceVisuals
    {
        public static GameObject Attach(GameObject pickup, ItemDefinition definition)
        {
            string path = definition.Id == PrototypeItems.Limestone.Id ? "Region04/Limestone" :
                definition.Id == PrototypeItems.Strawberries.Id ? "Region04/Strawberry" :
                definition.Id == PrototypeItems.TeaHerbs.Id ? "Region04/TeaHerbs" : null;
            return Region01ResourceVisuals.AttachPath(pickup, path);
        }
    }
}
