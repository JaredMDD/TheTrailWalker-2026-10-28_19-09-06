using System.Collections.Generic;
using TheTrailWalker.Domain.Travel;
using UnityEngine;

namespace TheTrailWalker.Travel
{
    // Places the authored camp art: burned-wood campfire, the region's wood-tier log seats and the three seated traders.
    // Returns false for any piece whose art is missing, so the caller keeps its placeholder primitive.
    public static class CampAssembly
    {
        private static CampPropArtCatalog props;
        private static CampNpcArtCatalog npcs;
        private static CampPropArtCatalog Props => props != null ? props : props = Resources.Load<CampPropArtCatalog>("Environment/CampProps/CampPropArtCatalog");
        private static CampNpcArtCatalog Npcs => npcs != null ? npcs : npcs = Resources.Load<CampNpcArtCatalog>("Characters/CampCommon/CampNpcArtCatalog");
        private static SpecialNpcArtCatalog special;
        public static SpecialNpcArtCatalog Special => special != null ? special : special = Resources.Load<SpecialNpcArtCatalog>("Characters/CampSpecial/SpecialNpcArtCatalog");

        // Balloon merchant art (its Animator plays the gentle bob); the door (-Z) faces the given point. Its only collider is a tap
        // trigger over the whole balloon (seated at the host camp, a tap opens the interior), so walking and pickups ignore it.
        // Null when the art is missing.
        public static GameObject BalloonMerchant(Transform root, Vector3 position, Vector3 faceTowards)
        {
            GameObject prefab = Resources.Load<GameObject>("Environment/BalloonMerchant/BalloonMerchant");
            if (prefab == null) return null;
            Vector3 away = Vector3.ProjectOnPlane(position - faceTowards, Vector3.up);
            GameObject merchant = Object.Instantiate(prefab, position, away.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(away) : Quaternion.identity, root);
            merchant.name = "Balloon merchant";
            foreach (Collider collider in merchant.GetComponentsInChildren<Collider>()) Object.Destroy(collider);
            Transform t = merchant.transform; bool any = false; var local = new Bounds();
            foreach (Renderer r in merchant.GetComponentsInChildren<Renderer>())
                for (int k = 0; k < 8; k++)
                {
                    Bounds b = r.bounds;
                    Vector3 p = t.InverseTransformPoint(b.center + Vector3.Scale(b.extents, new Vector3((k & 1) == 0 ? -1 : 1, (k & 2) == 0 ? -1 : 1, (k & 4) == 0 ? -1 : 1)));
                    if (!any) { local = new Bounds(p, Vector3.zero); any = true; } else local.Encapsulate(p);
                }
            var tap = merchant.AddComponent<BoxCollider>(); tap.isTrigger = true;
            if (any) { tap.center = local.center; tap.size = local.size; }
            return merchant;
        }

        // Static special NPC (mission lady, ferryman) standing at ground level, facing the given direction. Scenery only:
        // any collider is removed so it never blocks pickups or touches.
        public static bool SpecialNpc(Transform root, string name, GameObject prefab, Vector3 position, Vector3 facing)
        {
            if (prefab == null) return false;
            facing = Vector3.ProjectOnPlane(facing, Vector3.up);
            GameObject actor = Object.Instantiate(prefab, position, facing.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(facing) : Quaternion.identity, root);
            actor.name = name;
            foreach (Collider collider in actor.GetComponentsInChildren<Collider>()) Object.Destroy(collider);
            return true;
        }

        // The region's wood-tier fire when one exists (tier two: the burning birch fire), otherwise the static burned fire.
        public static bool Campfire(Transform root, Vector3 centre, int regionNumber = 1)
        {
            if (Props == null) return false;
            if (Props.Campfires != null)
                foreach (CampfireArt fire in Props.Campfires)
                    if (fire != null && fire.Prefab != null && regionNumber >= fire.FirstRegion && regionNumber <= fire.LastRegion)
                    {
                        Object.Instantiate(fire.Prefab, centre, Quaternion.Euler(0, 23f, 0), root).name = "Campfire";
                        return true;
                    }
            if (Props.StaticBurnedWoodCampfire == null) return false;
            Object.Instantiate(Props.StaticBurnedWoodCampfire, centre, Quaternion.Euler(0, 23f, 0), root).name = "Campfire";
            return true;
        }

        // Seat choice follows the region's wood tier (pine for regions 1-6), never the traders' clothing.
        public static Transform LogSeat(Transform root, string name, Vector3 position, Vector3 centre, int regionNumber)
        {
            if (Props == null || Props.LogSeats == null) return null;
            foreach (CampLogSeatArt seat in Props.LogSeats)
            {
                if (seat.Prefab == null || regionNumber < seat.FirstRegion || regionNumber > seat.LastRegion) continue;
                // The log's local +Z is the sitting direction: face the fire.
                Quaternion facing = Quaternion.LookRotation(Vector3.ProjectOnPlane(centre - position, Vector3.up));
                GameObject log = Object.Instantiate(seat.Prefab, position, facing, root); log.name = name;
                return log.transform;
            }
            return null;
        }

        public static bool Trader(Transform root, string name, Transform seat, NpcAppearance look)
        {
            CampNpcArtCatalog art = Npcs;
            Transform seatAnchor = seat != null ? seat.Find("SeatAnchor") : null;
            int tier = ClothingTier(look.ClothingSetId), packTier = BackpackTier(look.BackpackId);
            if (art == null || seatAnchor == null || art.OutfitStudies == null || tier >= art.OutfitStudies.Length || packTier >= art.OutfitStudies.Length) return false;
            GameObject prefab = look.Body == NpcBody.Female ? art.OutfitStudies[tier].Female : art.OutfitStudies[tier].Male;
            if (prefab == null) return false;
            GameObject trader = Object.Instantiate(prefab, root); trader.name = name;
            var rig = trader.GetComponent<CampNpcAvatarRig>();
            if (rig == null) { Object.Destroy(trader); return false; }
            // Hips sit on the log: line the avatar's SeatAnchor up with the log's, both facing the fire.
            trader.transform.rotation = seat.rotation;
            trader.transform.position += seatAnchor.position - rig.SeatAnchor.position;
            Paint(rig.SkinSurfaces, art.SkinTones[look.SkinTone % art.SkinTones.Length]);
            Paint(rig.HairSurfaces, art.HairColors[look.HairColor % art.HairColors.Length]);
            // Rolled hair style and facial hair: exactly one style shows; the beard only on men who rolled one.
            if (rig.HairStyles != null)
                for (int i = 0; i < rig.HairStyles.Length; i++) if (rig.HairStyles[i] != null) rig.HairStyles[i].SetActive(i == look.HairStyle % rig.HairStyles.Length);
            if (rig.Beard != null) rig.Beard.SetActive(look.Beard);
            // The backpack is independent of the outfit: swap in the rolled pack and recolour its straps.
            CampNpcOutfitArt pack = art.OutfitStudies[packTier];
            if (pack.Backpack != null && rig.BackpackAnchor != null)
            {
                foreach (Transform child in rig.BackpackAnchor) Object.Destroy(child.gameObject);
                Object.Instantiate(pack.Backpack, rig.BackpackAnchor, false);
                Paint(rig.BackpackSurfaces, pack.ClothingMaterial);
            }
            trader.AddComponent<CampNpcIdle>().Bind(rig, look.IdlePhase, look.IdleSpeed);
            return true;
        }

        private static int ClothingTier(string id) { foreach (NpcClothingSet set in CampNpcs.ClothingSets) if (set.Id == id) return set.Tier; return 0; }
        private static int BackpackTier(string id) { foreach (NpcBackpack pack in CampNpcs.Backpacks) if (pack.Id == id) return pack.Tier; return 0; }
        private static void Paint(IEnumerable<Renderer> surfaces, Material material)
        {
            if (surfaces == null || material == null) return;
            foreach (Renderer renderer in surfaces) if (renderer != null) renderer.sharedMaterial = material;
        }
    }

    // Seated idle: slow breathing sway, head turns and a small shoulder shift. Hips, legs and feet stay planted on
    // the log; arms and backpack follow the torso by a few degrees only, so hands stay near the knees.
    public sealed class CampNpcIdle : MonoBehaviour
    {
        private CampNpcAvatarRig rig;
        private float phase, speed;
        private Quaternion torso, head, left, right;
        public void Bind(CampNpcAvatarRig avatar, float idlePhase, float idleSpeed)
        {
            rig = avatar; phase = idlePhase * 100f; speed = idleSpeed;
            torso = rig.TorsoPivot.localRotation; head = rig.HeadPivot.localRotation;
            left = rig.LeftShoulder.localRotation; right = rig.RightShoulder.localRotation;
        }
        private void Update()
        {
            if (rig == null) return;
            float t = phase + Time.time * speed;
            float breath = Mathf.Sin(t * 1.6f), look = Mathf.Sin(t * 0.37f) + 0.4f * Mathf.Sin(t * 0.91f);
            rig.TorsoPivot.localRotation = torso * Quaternion.Euler(1.5f * breath, 0, 0.8f * Mathf.Sin(t * 0.5f));
            rig.HeadPivot.localRotation = head * Quaternion.Euler(2f * Mathf.Sin(t * 0.7f), 9f * look, 0);
            rig.LeftShoulder.localRotation = left * Quaternion.Euler(-1.5f * breath, 0, 0);
            rig.RightShoulder.localRotation = right * Quaternion.Euler(1.2f * Mathf.Sin(t * 1.3f), 0, 0);
        }
    }
}
