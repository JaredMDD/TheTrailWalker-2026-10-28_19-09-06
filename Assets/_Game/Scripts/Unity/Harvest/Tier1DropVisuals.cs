using TheTrailWalker.Domain.Items;
using UnityEngine;
namespace TheTrailWalker.Harvest
{
    internal static class Tier1DropVisuals
    {
        public static GameObject Attach(GameObject pickup,ItemDefinition item)
        {
            string name=item.Id==PrototypeItems.Log.Id?"PineLog":item.Id==PrototypeItems.PineNuts.Id?"PineNuts":
                item.Id==PrototypeItems.RawRabbitMeat.Id?"RawRabbitMeat":item.Id==PrototypeItems.Bone.Id?"Bone":
                item.Id==PrototypeItems.AnimalFat.Id?"AnimalFat":item.Id==PrototypeItems.RainbowTrout.Id?"RainbowTrout":
                item.Id==PrototypeItems.LargemouthBass.Id?"LargemouthBass":item.Id==PrototypeItems.Bluegill.Id?"Bluegill":
                item.Id==PrototypeItems.YellowPerch.Id?"YellowPerch":null;
            return Region01ResourceVisuals.AttachPath(pickup,name==null?null:"Tier01/Drops/"+name);
        }
    }
}
