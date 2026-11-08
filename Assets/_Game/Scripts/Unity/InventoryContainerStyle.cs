using TheTrailWalker.Domain.Items;
using UnityEngine;

namespace TheTrailWalker
{
    internal static class InventoryContainerStyle
    {
        private static InventoryContainerSkin[] authored;
        public static InventoryContainerSkin Load(string name)=>Resources.Load<InventoryContainerSkin>("UI/Inventory/"+name);
        public static void Apply(RectTransform target,string name,bool front=false)
        {
            var background=target.GetComponent<UnityEngine.UI.Image>();
            if(background!=null){background.enabled=false;background.raycastTarget=false;}
            var graphic=target.GetComponent<InventoryContainerGraphic>();
            if(graphic==null)graphic=target.gameObject.AddComponent<InventoryContainerGraphic>();
            graphic.Bind(Load(name),front);
        }
        // Explicit IDs allow the container art to follow the worn item, independently of capacity/quality.
        public static string BagSkin(ItemDefinition equipped)
        {
            if(equipped==null)return "BaseRags";
            return ForEquipment(equipped,InventoryContainerKind.Backpack,"Tier01Backpack");
        }
        public static string PocketSkin(ItemDefinition equipped)=>ForEquipment(equipped,InventoryContainerKind.Pocket,"Tier01ShirtPocket");
        // Confirmed: the belt and its tool loops are one universal design; only backpacks and pockets follow the worn item.
        public static string BeltSkin(ItemDefinition equipped)=>"Tier01PantsBelt";
        public static string LoopSkin(ItemDefinition equipped)=>"Tier01ToolLoop";
        public static string OpeningSkin(ItemDefinition equipped)=>equipped==null?"BaseRagsOpening":ForEquipment(equipped,InventoryContainerKind.Opening,"Tier01BackpackOpening");
        private static string ForEquipment(ItemDefinition equipped,InventoryContainerKind kind,string fallback)
        {
            if(authored==null)authored=Resources.LoadAll<InventoryContainerSkin>("UI/Inventory");
            if(equipped!=null)foreach(var skin in authored)
                if(skin.Kind==kind&&skin.EquipmentDefinitionId==equipped.Id)return skin.name;
            return fallback; // Later-tier/debug equipment retains a neutral skin until its own art is authored.
        }
    }
}
