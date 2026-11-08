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
    // (only while the menu is open). It wears the default basewear plus the worn gear: fitted shirt, pants and boots
    // shells from the catalog (tier one so far), and the hat and backpack as item models at the art anchors.
    public sealed class EquipmentAvatarView
    {
        private const int Layer = 30, TextureWidth = 432; // texture shape close to the stage, so the figure fills it
        private static readonly Vector3 StagePosition = new Vector3(5000f, 0f, 5000f); // beyond terrain and shadow distance
        private readonly GameObject root;
        private readonly Camera camera;
        private readonly RenderTexture texture;
        private readonly Transform hatAnchor, packAnchor, bodyRoot;
        private GameObject hat, pack;
        private string hatId, packId;
        // Fitted wearables (user, November 5, 2026): shirt, pants and boots shells swapped in from the catalog; the shirt
        // and pants hide the base tunic/trousers they replace.
        private readonly PlayerAvatarArtCatalog art;
        private readonly bool female;
        private readonly GameObject baseShirt, basePants;
        private GameObject shirt, pants, boots;
        private string shirtId, pantsId, bootsId;
        // Fitted hat and backpack (tier-one art pass, November 5, 2026): worn shells at their anchors with identity TRS,
        // and a hat-compatible hair variant that keeps the saved style and colour (restored when the hat comes off).
        private readonly Transform hairAnchor;
        private readonly PlayerAppearance look;
        private GameObject baseHair, hatHair;
        private bool hatFitted, packFitted;

        public static PlayerAvatarArtCatalog Catalog => Resources.Load<PlayerAvatarArtCatalog>("Characters/PlayerDefault/PlayerAvatarArtCatalog");

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

        // Shows the worn gear (null when the slot holds default gear). Cheap when nothing changed. The hat and backpack
        // still use their item models at the anchors; shirt, pants and boots use the fitted shells when one exists.
        public void Dress(ItemDefinition hatItem, ItemDefinition packItem, ItemDefinition shirtItem = null, ItemDefinition pantsItem = null, ItemDefinition bootsItem = null)
        {
            DressAccessory(ref hat, ref hatId, ref hatFitted, hatItem, hatAnchor, () => Attach(ref hat, ref hatId, hatItem, hatAnchor, HatWidth, true, new Vector3(0, HatBrim, 0.005f), 0f));
            SyncHatHair();
            DressAccessory(ref pack, ref packId, ref packFitted, packItem, packAnchor, () => Attach(ref pack, ref packId, packItem, packAnchor, PackHeight, false, new Vector3(0, 0, PackBack), 0f));
            Wear(ref shirt, ref shirtId, shirtItem, baseShirt);
            Wear(ref pants, ref pantsId, pantsItem, basePants);
            Wear(ref boots, ref bootsId, bootsItem, null);
        }
        private delegate void LegacyAttach();
        // A fitted accessory (hat/backpack) from the art map when one exists for the item; otherwise the legacy item
        // model at the anchor. Cheap when the item did not change.
        private void DressAccessory(ref GameObject current, ref string currentId, ref bool fitted, ItemDefinition item, Transform anchor, LegacyAttach legacy)
        {
            string id = item != null ? item.Id : null;
            GameObject prefab = item != null ? FittedAccessory(ItemArt.ModelFor(item).Id, female) : null;
            if (prefab == null || anchor == null)
            {
                if (fitted) { if (current != null) Kill(current); current = null; currentId = null; fitted = false; }
                legacy();
                return;
            }
            if (fitted && id == currentId && current != null) return;
            if (current != null) Kill(current);
            current = Object.Instantiate(prefab, anchor, false);
            current.transform.localPosition = Vector3.zero; current.transform.localRotation = Quaternion.identity; current.transform.localScale = Vector3.one;
            current.name = "Worn " + item.Name;
            foreach (Collider collider in current.GetComponentsInChildren<Collider>()) Kill(collider);
            SetLayer(current);
            currentId = id; fitted = true;
        }

        // While a fitted hat is worn, the saved hairstyle shows as its hat-compatible variant (crown tucked under the
        // cap) in the saved colour; the original hair returns when the hat comes off. The saved appearance never changes.
        private void SyncHatHair()
        {
            GameObject wanted = hatFitted ? HatHair(female, look.HairStyle) : null;
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

        // ---- Tier-one accessory art map (Resources/Characters/Tier01Accessories/Tier01AccessoriesArtMap.json)
        [System.Serializable] private class AccessoryMap { public AccessoryEntry[] items; public HairBodies compatibleHair; }
        [System.Serializable] private class AccessoryEntry { public string itemId; public BodyPaths worn; }
        [System.Serializable] private class BodyPaths { public string Male, Female; }
        [System.Serializable] private class HairBodies { public HairStyles Male, Female; }
        [System.Serializable] private class HairStyles { public string Crop, Sweep, Bob, Tied; }
        private static AccessoryMap accessoryMap;
        private static bool accessoryMapLoaded;
        private static AccessoryMap Accessories
        {
            get
            {
                if (accessoryMapLoaded) return accessoryMap;
                accessoryMapLoaded = true;
                TextAsset json = Resources.Load<TextAsset>("Characters/Tier01Accessories/Tier01AccessoriesArtMap");
                if (json != null) try { accessoryMap = JsonUtility.FromJson<AccessoryMap>(json.text); } catch (System.Exception) { accessoryMap = null; }
                return accessoryMap;
            }
        }
        // The fitted worn prefab for an item model ID and body, or null when the item has no fitted accessory.
        public static GameObject FittedAccessory(string modelId, bool female)
        {
            AccessoryMap map = Accessories;
            if (map?.items == null || modelId == null) return null;
            foreach (AccessoryEntry entry in map.items)
                if (entry.itemId == modelId && entry.worn != null)
                {
                    string path = female ? entry.worn.Female : entry.worn.Male;
                    return string.IsNullOrEmpty(path) ? null : Resources.Load<GameObject>(path);
                }
            return null;
        }
        // The hat-compatible hair for a body and saved hairstyle index (Crop, Sweep, Bob, Tied).
        public static GameObject HatHair(bool female, int style)
        {
            HairStyles styles = female ? Accessories?.compatibleHair?.Female : Accessories?.compatibleHair?.Male;
            if (styles == null) return null;
            string path = style == 0 ? styles.Crop : style == 1 ? styles.Sweep : style == 2 ? styles.Bob : style == 3 ? styles.Tied : null;
            return string.IsNullOrEmpty(path) ? null : Resources.Load<GameObject>(path);
        }

        // The fitted shell for an item (debug test gear borrows its slot's tier-one model), or null for basewear.
        public static GameObject WearableFor(PlayerAvatarArtCatalog art, ItemDefinition item, bool female) =>
            art != null && item != null ? art.Wearable(ItemArt.ModelFor(item).Id, female) : null;

        private void Wear(ref GameObject current, ref string currentId, ItemDefinition item, GameObject replaced)
        {
            string id = item != null ? item.Id : null;
            if (id == currentId) return;
            currentId = id;
            if (current != null) Kill(current);
            current = null;
            GameObject prefab = WearableFor(art, item, female);
            if (prefab != null)
            {
                current = Object.Instantiate(prefab, bodyRoot, false);
                current.name = "Worn " + item.Name;
                SetLayer(current);
            }
            // Only a fitted shell replaces the basewear; an item without one leaves the base garment showing.
            if (replaced != null) replaced.SetActive(current == null);
        }
        // Head-local hat brim height and width; backpack height and offset behind the BackpackAnchor (tuned by render).
        public static float HatWidth = 0.27f, HatBrim = 0.155f, PackHeight = 0.52f, PackBack = -0.06f;

        private void Attach(ref GameObject current, ref string currentId, ItemDefinition item, Transform anchor, float size, bool sitOnBottom, Vector3 offset, float yaw)
        {
            string id = item != null ? item.Id : null;
            if (id == currentId && (current != null || id == null)) return;
            currentId = id;
            if (current != null) Kill(current);
            current = null;
            if (item == null || anchor == null) return;
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
