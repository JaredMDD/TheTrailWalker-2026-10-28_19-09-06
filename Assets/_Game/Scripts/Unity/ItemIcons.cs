using System.Collections.Generic;
using TheTrailWalker.Domain.Items;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TheTrailWalker
{
    // Per-item presentation tuning for meshed resources (user feedback, October 30, 2026).
    internal static class ResourceVisualTuning
    {
        // Shape: non-uniform scale on the mesh itself (branches are slimmer than the source bundle mesh).
        // World: extra multiplier on the trail only; the bag footprint is unaffected (pelts read large on the trail).
        // The hemp shirt model is low and read as a folded shirt: stretched to its taller footprint (588 -> 780; user, October 31, 2026).
        public static Vector3 Shape(ItemDefinition definition) => definition.Id == PrototypeItems.Branch.Id ? new Vector3(1f, 0.45f, 0.45f)
            : definition.Id == "item.hemp_shirt" ? new Vector3(1f, 780f / 588f, 1f) : Vector3.one;
        // Confirmed size rule: apple-sized items (stones, ores, berry-type fruits and similar) share the apple footprint and
        // trail size.
        private static readonly HashSet<string> AppleSized = new HashSet<string> {
            PrototypeItems.Apples.Id, PrototypeItems.Flint.Id, PrototypeItems.PineCone.Id, PrototypeItems.Berries.Id, PrototypeItems.Clay.Id,
            PrototypeItems.RockSalt.Id, PrototypeItems.Limestone.Id, PrototypeItems.Strawberries.Id, PrototypeItems.Pebbles.Id, PrototypeItems.Sand.Id, "ingredient.mushrooms" };
        private static readonly HashSet<string> SoftBundles = new HashSet<string> {
            PrototypeItems.RabbitPelt.Id, PrototypeItems.Hemp.Id, PrototypeItems.NettleFiber.Id, PrototypeItems.TeaHerbs.Id,
            PrototypeItems.Oats.Id, PrototypeItems.Reeds.Id, PrototypeItems.Wheat.Id };
        // Trail-only size multipliers (user feedback, October 31, 2026): logs, raw meat, fat and tools read too large.
        private static readonly Dictionary<string, float> TrailScale = new Dictionary<string, float> {
            { PrototypeItems.Log.Id, 0.65f }, { PrototypeItems.RawRabbitMeat.Id, 0.55f }, { PrototypeItems.AnimalFat.Id, 0.7f },
            { "item.pine_slingshot_normal", 0.28f }, // slingshot read too big on the trail only
            // Pelt/leather footprints shrank to match their bag pictures (November 6, 2026); these keep their trail size
            // unchanged (rabbit pelt 0.6 x 850/480, processed hides 900/600 before the 1.05 m cap).
            { PrototypeItems.RabbitPelt.Id, 0.6f * 850f / 480f }, { "item.cured_rabbit_pelt", 1.5f }, { "item.fine_rabbit_pelt", 1.5f },
            { "item.rabbit_leather", 1.5f }, { "item.fine_rabbit_leather", 1.5f } };
        // Bag-picture orientation overrides: the plank is seen on its broad face, tilted, instead of edge-on.
        // Flat pelts, leathers and tiles get the same tilt (November 5, 2026): their front view showed only an edge.
        private static readonly HashSet<string> TiltedIcons = new HashSet<string> {
            "item.pine_plank", PrototypeItems.RabbitPelt.Id, "item.cured_rabbit_pelt", "item.fine_rabbit_pelt",
            "item.rabbit_leather", "item.fine_rabbit_leather", "item.limestone_tiles" };
        public static Quaternion IconRotation(ItemDefinition definition) =>
            TiltedIcons.Contains(definition.Id) ? Quaternion.Euler(-62f, 0f, 0f) : Quaternion.identity;

        // Items whose trail size the user already reviewed (regions 1-2 loose resources, tier-one harvest drops, tools).
        private static readonly HashSet<string> Reviewed = new HashSet<string> {
            PrototypeItems.Branch.Id, PrototypeItems.Log.Id, PrototypeItems.RabbitPelt.Id, PrototypeItems.Apples.Id, PrototypeItems.Hemp.Id, PrototypeItems.Flint.Id,
            PrototypeItems.PineCone.Id, PrototypeItems.PineNuts.Id, PrototypeItems.RawRabbitMeat.Id, PrototypeItems.Bone.Id, PrototypeItems.AnimalFat.Id,
            PrototypeItems.RainbowTrout.Id, PrototypeItems.Bluegill.Id, PrototypeItems.LargemouthBass.Id, PrototypeItems.YellowPerch.Id };
        // New models (user, October 31, 2026): the current reviewed sizes are the default maximum, so any other item's
        // largest trail dimension (at a full 1,000 stack) stays within about an apple/log (1.05 m).
        private const float NewModelMaxMetres = 1.05f;

        public static float World(ItemDefinition definition)
        {
            float raw = WorldUncapped(definition);
            if (Reviewed.Contains(definition.Id) || !definition.Stackable) return raw;
            float largest = Mathf.Max(definition.Width, definition.Height) / 1000f * raw * 1.5f;
            return largest > NewModelMaxMetres ? raw * NewModelMaxMetres / largest : raw;
        }

        private static float WorldUncapped(ItemDefinition definition)
        {
            if (TrailScale.TryGetValue(definition.Id, out float scale)) return scale;
            // Debug slingshots/bows match the pine slingshot; tier 3 ones show the 1.5 m mesquite bow and use the tool scale below.
            if (definition.Id.StartsWith("debug.tool.hunting.") && !Tier3Visuals.Has(definition.Id)) return 0.28f;
            if (!definition.Stackable && !definition.GearSlot.HasValue) return 0.45f; // tools
            return SoftBundles.Contains(definition.Id) ? 0.6f : AppleSized.Contains(definition.Id) ? 0.8f : 1f;
        }
        // Long horizontal resources (branches and fibre bundles) stand upright on the trail so they are easy to touch;
        // the bag picture stays horizontal. Stalk bundles (reeds, wheat, oats) are already modelled upright.
        private static readonly HashSet<string> Upright = new HashSet<string> { PrototypeItems.Branch.Id, PrototypeItems.Hemp.Id, PrototypeItems.NettleFiber.Id };
        public static Quaternion TrailRotation(ItemDefinition definition) => Upright.Contains(definition.Id) ? Quaternion.Euler(0, 0, 90f) : Quaternion.identity;

        private static BalloonGoodsArtCatalog specialCatalog;
        // Special balloon gear shows the catalog's item display model.
        private static GameObject AttachSpecial(GameObject pickup, ItemDefinition definition)
        {
            if (!SpecialGear.Is(definition.Id)) return null;
            if (specialCatalog == null) specialCatalog = Resources.Load<BalloonGoodsArtCatalog>("BalloonGoods/BalloonGoodsArtCatalog");
            BalloonSpecialItemArt art = specialCatalog == null ? null : System.Array.Find(specialCatalog.SpecialItems, x => x.Id == definition.Id);
            if (art == null || art.Display == null) return null;
            GameObject visual = Object.Instantiate(art.Display, pickup.transform, false);
            pickup.GetComponent<Renderer>().enabled = false;
            return visual;
        }

        // Same model lookup the trail uses; returns the attached visual or null for items without a mesh yet.
        public static GameObject Attach(GameObject pickup, ItemDefinition definition)
        {
            GameObject visual = Region01ResourceVisuals.Attach(pickup, definition) ?? Region02ResourceVisuals.Attach(pickup, definition)
                ?? Region03ResourceVisuals.Attach(pickup, definition) ?? Region04ResourceVisuals.Attach(pickup, definition)
                ?? Region05ResourceVisuals.Attach(pickup, definition) ?? Region06ResourceVisuals.Attach(pickup, definition)
                ?? Harvest.Tier1DropVisuals.Attach(pickup, definition)
                ?? Region01ResourceVisuals.AttachPath(pickup, Tier1CraftedItems.VisualPath(definition.Id))
                ?? Region01ResourceVisuals.AttachPath(pickup, Tier2ShopVisuals.PathFor(definition.Id))
                ?? Region01ResourceVisuals.AttachPath(pickup, Tier3Visuals.PathFor(definition.Id))
                ?? AttachSpecial(pickup, definition);
            if (visual != null) visual.transform.localScale = Shape(definition);
            return visual;
        }
    }

    // Renders each meshed item once, front view on a transparent background, so the bag shows the model instead of a
    // coloured box. The front view matches the footprint's width and height axes.
    internal static class ItemIconRenderer
    {
        private const int CaptureLayer = 31, LongSide = 256;
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        private static readonly HashSet<string> NoModel = new HashSet<string>();
        private static int captures;

        public static Sprite For(ItemDefinition definition)
        {
            if (Cache.TryGetValue(definition.Id, out Sprite cached)) return cached;
            if (NoModel.Contains(definition.Id)) return null;
            // Each capture gets its own spot and is destroyed immediately: a deferred Destroy left earlier captures in view,
            // so pictures rendered in the same frame showed each other's meshes (apple pierced by a branch).
            var root = new GameObject("Item icon capture", typeof(MeshRenderer), typeof(MeshFilter));
            root.transform.position = new Vector3(++captures * 50f, -1000f, 0);
            GameObject visual = ResourceVisualTuning.Attach(root, definition);
            if (visual == null) { Object.DestroyImmediate(root); NoModel.Add(definition.Id); return null; }
            visual.transform.localRotation = ResourceVisualTuning.IconRotation(definition) * visual.transform.localRotation;
            Sprite sprite = null;
            GameObject cameraObject = null; RenderTexture target = null; RenderTexture previous = RenderTexture.active;
            try
            {
                var bounds = new Bounds(); bool first = true;
                foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.gameObject.layer = CaptureLayer;
                    if (first) { bounds = renderer.bounds; first = false; } else bounds.Encapsulate(renderer.bounds);
                }
                float aspect = Mathf.Max(0.05f, bounds.size.x) / Mathf.Max(0.05f, bounds.size.y);
                int width = aspect >= 1 ? LongSide : Mathf.Max(16, Mathf.RoundToInt(LongSide * aspect));
                int height = aspect >= 1 ? Mathf.Max(16, Mathf.RoundToInt(LongSide / aspect)) : LongSide;
                cameraObject = new GameObject("Item icon camera");
                var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true; camera.orthographicSize = bounds.extents.y; camera.aspect = (float)width / height;
                camera.cullingMask = 1 << CaptureLayer; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0, 0, 0, 0);
                camera.nearClipPlane = 0.01f; camera.farClipPlane = bounds.size.z + 10f; camera.enabled = false;
                camera.transform.SetPositionAndRotation(bounds.center - Vector3.forward * (bounds.extents.z + 5f), Quaternion.identity);
                camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
                target = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32);
                camera.targetTexture = target; camera.Render();
                RenderTexture.active = target;
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = definition.Id + " icon" };
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0); texture.Apply();
                sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
            }
            finally
            {
                RenderTexture.active = previous;
                if (target != null) RenderTexture.ReleaseTemporary(target);
                if (cameraObject != null) Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(root);
            }
            Cache[definition.Id] = sprite;
            return sprite;
        }

        // Quantity only, compact: "10", full stack "1k". Unstackable tools and gear show nothing.
        public static string Quantity(ItemEntry item) => !item.Definition.Stackable ? "" : item.Quantity >= 1000 ? "1k" : item.Quantity.ToString();
    }
}
