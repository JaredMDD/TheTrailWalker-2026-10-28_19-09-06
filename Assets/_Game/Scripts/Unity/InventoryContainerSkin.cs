using UnityEngine;

namespace TheTrailWalker
{
    public enum InventoryContainerKind { Rags, Backpack, Pocket, Belt, ToolLoop, Opening }

    // Presentation only. Capacity, item eligibility and durability remain in the domain model.
    public sealed class InventoryContainerSkin : ScriptableObject
    {
        public InventoryContainerKind Kind;
        public string EquipmentDefinitionId;
        public Color Cloth, Shade, Binding, Thread, Patch;
    }
}
