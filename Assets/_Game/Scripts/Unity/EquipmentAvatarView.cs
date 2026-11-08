using TheTrailWalker.Domain.Items;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Appearance of the default player avatar: body, skin tone, hair style and hair colour. Picked at random once and
    // kept in PlayerPrefs (not the save) until the start-of-game selection exists (user decision, October 31, 2026).
    public struct PlayerAppearance
    {
        public int Body, Skin, HairStyle, HairColor; // Body 0 male, 1 female; Skin 0-5; HairStyle 0-3; HairColor 0-4
        private const string Key = "equipmentAvatar.v1";

        public static PlayerAppearance Current()
        {
            try
            {
                string saved = PlayerPrefs.GetString(Key, "");
                string[] parts = saved.Split(',');
                if (parts.Length == 4 && int.TryParse(parts[0], out int b) && int.TryParse(parts[1], out int s) && int.TryParse(parts[2], out int h) && int.TryParse(parts[3], out int c)
                    && b >= 0 && b < 2 && s >= 0 && s < 6 && h >= 0 && h < 4 && c >= 0 && c < 5)
                    return new PlayerAppearance { Body = b, Skin = s, HairStyle = h, HairColor = c };
            }
            catch (System.Exception) { }
            var random = new System.Random();
            var look = new PlayerAppearance { Body = random.Next(2), Skin = random.Next(6), HairStyle = random.Next(4), HairColor = random.Next(5) };
            try { PlayerPrefs.SetString(Key, $"{look.Body},{look.Skin},{look.HairStyle},{look.HairColor}"); PlayerPrefs.Save(); } catch (System.Exception) { }
            return look;
        }
    }

    // The standing player avatar in the equipment menu's centre stage, rendered live by its own camera into a texture
    // (only while the menu is open). It wears the default basewear plus the worn gear. Every fitted garment, any tier,
    // comes from WearableArtCatalog (equipment redesign, November 7, 2026); a hat or backpack without fitted art falls
    // back to its item model at the anchor.
    public sealed class EquipmentAvatarView
    {
        private const int Layer = 30, TextureWidth = 432; // texture shape close to the stage, so the figure fills it
        private static readonly Vector3 StagePosition = new Vector3(5000f, 0f, 5000f); // beyond terrain and shadow distance
        private readonly GameObject root;
        private readonly Camera camera;
        private readonly RenderTexture texture;
        private readonly Transform hatAnchor, packAnchor, bodyRoot;
        private readonly PlayerAvatarArtCatalog art;
        private readonly bool female;
        private readonly GameObject baseShirt, basePants;
        // One worn state per slot: the shown object, the item it shows and whether it is fitted art (not a fallback model).
        private sealed class Worn { public GameObject Shown; public string ItemId; public bool Fitted; }
        private readonly Worn hat = new Worn(), pack = new Worn(), shirt = new Worn(), pants = new Worn(), boots = new Worn();
        // While a fitted hat is worn, the saved hairstyle shows as its hat-compatible variant (restored when it comes off).
        private readonly Transform hairAnchor;
        private readonly PlayerAppearance look;
        private GameObject baseHair, hatHair;

        public static PlayerAvatarArtCatalog Catalog => Resources.Load<PlayerAvatarArtCatalog>("Characters/PlayerDefault/PlayerAvatarArtCatalog");
        // Editor reviews copy the dressed avatar out of its private stage.
        public GameObject StageAvatar => bodyRoot != null ? bodyRoot.gameObject : null;

        public static EquipmentAvatarView Create(RectTransform stage, PlayerAppearance look)
        {
            PlayerAvatarArtCatalog art = Catalog;
            if (art == null || art.MaleBody == null || art.FemaleBody == null) return null;
            return new EquipmentAvatarView(stage, art, look);
        }

        private EquipmentAvatarView(RectTransform stage, PlayerAvatarArtCatalog art, PlayerAppearance look)
        {
            root = new GameObject("Equipment avatar stage");
            root.transform.position = StagePosition;
            GameObject body = Object.Instantiate(look.Body == 0 ? art.MaleBody : art.FemaleBody, root.transform, false);
            body.name = "Player avatar";
            body.transform.localRotation = Quaternion.Euler(0, 24f, 0); // a slight three-quarter turn shows the pack
            hairAnchor = body.transform.Find("HeadAnchor/HairAnchor");
            this.look = look;
            GameObject[] styles = look.Body == 0 ? art.MaleHair : art.FemaleHair;
            if (hairAnchor != null && styles != null && look.HairStyle < styles.Length && styles[look.HairStyle] != null)
            {
                for (int i = hairAnchor.childCount - 1; i >= 0; i--) Kill(hairAnchor.GetChild(i).gameObject);
                baseHair = Object.Instantiate(styles[look.HairStyle], hairAnchor, false);
            }
            hatAnchor = body.transform.Find("HeadAnchor/HatAnchor"); packAnchor = body.transform.Find("BackpackAnchor");
            this.art = art; female = look.Body == 1; bodyRoot = body.transform;
            foreach (Transform child in body.transform)
            {
                if (child.name.EndsWith("BaseShirt")) baseShirt = child.gameObject;
                else if (child.name.EndsWith("BasePants")) basePants = child.gameObject;
            }
            Recolor(root, art.SkinMaterials, art.SkinMaterials[look.Skin]);
            Recolor(root, art.HairMaterials, art.HairMaterials[look.HairColor]);

            texture = new RenderTexture(TextureWidth, 1024, 24, RenderTextureFormat.ARGB32) { name = "Equipment avatar", antiAliasing = 4 };
            var eye = new GameObject("Equipment avatar camera");
            eye.transform.SetParent(root.transform, false);
            eye.transform.localPosition = new Vector3(0, 0.96f, 3.6f); eye.transform.LookAt(StagePosition + new Vector3(0, 0.90f, 0));
            camera = eye.AddComponent<Camera>();
            camera.cullingMask = 1 << Layer; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0, 0, 0, 0);
            camera.fieldOfView = 29f; camera.nearClipPlane = 0.5f; camera.farClipPlane = 8f; camera.targetTexture = texture; camera.enabled = false;
            var data = eye.AddComponent<UniversalAdditionalCameraData>(); data.renderShadows = false; data.renderPostProcessing = false;
            // Soft key light in front of the avatar (the camp sun can be behind it). It reaches only this distant stage.
            var key = new GameObject("Equipment avatar key light").AddComponent<Light>();
            key.transform.SetParent(root.transform, false); key.transform.localPosition = new Vector3(0.9f, 1.9f, 2.2f);
            key.type = LightType.Point; key.range = 6f; key.intensity = 2.2f; key.color = new Color(1f, 0.96f, 0.9f); key.shadows = LightShadows.None;
            SetLayer(root);
            if (Camera.main != null) Camera.main.cullingMask &= ~(1 << Layer);

            var picture = new GameObject("Avatar", typeof(RectTransform)).GetComponent<RectTransform>();
            picture.SetParent(stage, false); picture.anchorMin = Vector2.zero; picture.anchorMax = Vector2.one; picture.offsetMin = picture.offsetMax = Vector2.zero;
            RawImage image = picture.gameObject.AddComponent<RawImage>(); image.texture = texture; image.raycastTarget = false;
            var fitter = picture.gameObject.AddComponent<AspectRatioFitter>(); fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent; fitter.aspectRatio = TextureWidth / 1024f;
            root.SetActive(false);
        }

        public void SetVisible(bool visible) { if (root == null) return; root.SetActive(visible); camera.enabled = visible; }
        // Editor/validation: render one frame on demand.
        public RenderTexture RenderOnce()
        {
            root.SetActive(true);
            var request = new UniversalRenderPipeline.SingleCameraRequest { destination = texture };
            if (UnityEngine.Rendering.RenderPipeline.SupportsRenderRequest(camera, request)) UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera, request);
            else camera.Render();
            return texture;
        }

        // Shows the worn gear (null when the slot holds default gear). Cheap when nothing changed.
        public void Dress(ItemDefinition hatItem, ItemDefinition packItem, ItemDefinition shirtItem = null, ItemDefinition pantsItem = null, ItemDefinition bootsItem = null)
        {
            Wear(hat, hatItem, hatAnchor, null, () => Attach(hat, hatItem, hatAnchor, HatWidth, true, new Vector3(0, HatBrim, 0.005f)));
            SyncHatHair();
            Wear(pack, packItem, packAnchor, null, () => Attach(pack, packItem, packAnchor, PackHeight, false, new Vector3(0, 0, PackBack)));
            Wear(shirt, shirtItem, bodyRoot, baseShirt, null);
            Wear(pants, pantsItem, bodyRoot, basePants, null);
            Wear(boots, bootsItem, bodyRoot, null, null);
        }

        // The catalog's fitted garment for an item and body, or null (debug test gear borrows its slot's tier-one model).
        public static WearableArt FittedArt(ItemDefinition item) => item != null ? WearableArtCatalog.Current?.Find(ItemArt.ModelFor(item).Id) : null;
        public static GameObject FittedPrefab(ItemDefinition item, bool female) => FittedArt(item)?.For(female);
        public static GameObject HatHair(bool female, int style) => WearableArtCatalog.Current?.HatHair(female, style);

        private delegate void Fallback();
        // Fitted art under its anchor (identity transform); otherwise the fallback (item model) or nothing. A fitted
        // shirt or pants hides the base garment it replaces; anything else leaves the base showing.
        private void Wear(Worn slot, ItemDefinition item, Transform frame, GameObject replaced, Fallback fallback)
        {
            string id = item != null ? item.Id : null;
            WearableArt art = FittedArt(item);
            GameObject prefab = art?.For(female);
            Transform anchor = prefab == null ? null : art.Anchor == "Root" ? bodyRoot : bodyRoot.Find(art.Anchor) ?? frame;
            if (prefab == null || anchor == null)
            {
                if (slot.Fitted) { Clear(slot); }
                if (fallback != null) fallback(); else if (slot.ItemId != id) { Clear(slot); slot.ItemId = id; }
                if (replaced != null) replaced.SetActive(true);
                return;
            }
            if (slot.Fitted && slot.ItemId == id && slot.Shown != null) return;
            Clear(slot);
            slot.Shown = Object.Instantiate(prefab, anchor, false);
            slot.Shown.transform.localPosition = Vector3.zero; slot.Shown.transform.localRotation = Quaternion.identity; slot.Shown.transform.localScale = Vector3.one;
            slot.Shown.name = "Worn " + item.Name;
            foreach (Collider collider in slot.Shown.GetComponentsInChildren<Collider>()) Kill(collider);
            SetLayer(slot.Shown);
            slot.ItemId = id; slot.Fitted = true;
            if (replaced != null) replaced.SetActive(false);
        }
        private void Clear(Worn slot)
        {
            if (slot.Shown != null) Kill(slot.Shown);
            slot.Shown = null; slot.ItemId = null; slot.Fitted = false;
        }

        private void SyncHatHair()
        {
            GameObject wanted = hat.Fitted ? HatHair(female, look.HairStyle) : null;
            if (wanted == null)
            {
                if (hatHair != null) { Kill(hatHair); hatHair = null; }
                if (baseHair != null) baseHair.SetActive(true);
                return;
            }
            if (hatHair != null) return;
            hatHair = Object.Instantiate(wanted, hairAnchor, false);
            hatHair.name = "Hat hair";
            if (art.HairMaterials != null && look.HairColor < art.HairMaterials.Length) Recolor(hatHair, art.HairMaterials, art.HairMaterials[look.HairColor]);
            SetLayer(hatHair);
            if (baseHair != null) baseHair.SetActive(false);
        }

        // Head-local hat brim height and width; backpack height and offset behind the BackpackAnchor (tuned by render).
        public static float HatWidth = 0.27f, HatBrim = 0.155f, PackHeight = 0.52f, PackBack = -0.06f;

        // Fallback for a hat or backpack without fitted art: its item model, scaled to the anchor.
        private void Attach(Worn slot, ItemDefinition item, Transform anchor, float size, bool sitOnBottom, Vector3 offset)
        {
            string id = item != null ? item.Id : null;
            if (id == slot.ItemId && !slot.Fitted && (slot.Shown != null || id == null)) return;
            Clear(slot); slot.ItemId = id;
            if (item == null || anchor == null) return;
            ref GameObject current = ref slot.Shown; const float yaw = 0f;
            string path = Tier1CraftedItems.VisualPath(ItemArt.ModelFor(item).Id);
            GameObject prefab = path != null ? Resources.Load<GameObject>(path) : null;
            if (prefab == null) return;
            current = Object.Instantiate(prefab, anchor, false);
            current.name = "Worn " + item.Name;
            // Model bounds in its own space, from the meshes (independent of the avatar's turn).
            bool first = true; Bounds bounds = default;
            foreach (MeshFilter filter in current.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh == null) continue;
                Matrix4x4 relative = current.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                Bounds mesh = filter.sharedMesh.bounds;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 point = relative.MultiplyPoint3x4(mesh.center + Vector3.Scale(mesh.extents, new Vector3((corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1)));
                    if (first) { bounds = new Bounds(point, Vector3.zero); first = false; } else bounds.Encapsulate(point);
                }
            }
            if (first) { Kill(current); current = null; return; }
            float scale = size / Mathf.Max(0.001f, sitOnBottom ? bounds.size.x : bounds.size.y);
            Quaternion turn = Quaternion.Euler(0, yaw, 0);
            current.transform.localRotation = turn; current.transform.localScale = Vector3.one * scale;
            current.transform.localPosition = offset - turn * (bounds.center * scale) + (sitOnBottom ? Vector3.up * bounds.extents.y * scale : Vector3.zero);
            foreach (Collider collider in current.GetComponentsInChildren<Collider>()) Kill(collider);
            SetLayer(current);
        }

        public void Destroy()
        {
            if (root != null) Kill(root);
            if (texture != null) { texture.Release(); Kill(texture); }
        }

        private static void Kill(Object target) { if (Application.isPlaying) Object.Destroy(target); else Object.DestroyImmediate(target); }

        private static void Recolor(GameObject target, Material[] family, Material chosen)
        {
            if (family == null || chosen == null) return;
            foreach (MeshRenderer renderer in target.GetComponentsInChildren<MeshRenderer>(true))
            {
                Material[] materials = renderer.sharedMaterials; bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                    if (System.Array.IndexOf(family, materials[i]) >= 0 && materials[i] != chosen) { materials[i] = chosen; changed = true; }
                if (changed) renderer.sharedMaterials = materials;
            }
        }

        private static void SetLayer(GameObject target) { foreach (Transform t in target.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = Layer; }
    }
}
