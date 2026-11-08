using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Collects the default player avatar art (built by PlayerDefaultAvatarAssets) into a Resources catalog so the
    // equipment menu can load it at runtime. Does not modify the art prefabs or materials.
    public static class PlayerAvatarCatalogAssets
    {
        const string Art = "Assets/_Game/Art/Characters/PlayerDefault/Unity/";
        const string CatalogPath = "Assets/_Game/Resources/Characters/PlayerDefault/PlayerAvatarArtCatalog.asset";

        [MenuItem("The Trail Walker/Art/Build Player Avatar Catalog")]
        public static void Build()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CatalogPath));
            var catalog = AssetDatabase.LoadAssetAtPath<PlayerAvatarArtCatalog>(CatalogPath);
            if (catalog == null) { catalog = ScriptableObject.CreateInstance<PlayerAvatarArtCatalog>(); AssetDatabase.CreateAsset(catalog, CatalogPath); }
            catalog.MaleBody = Load<GameObject>(Art + "Avatars/MaleSkin01.prefab");
            catalog.FemaleBody = Load<GameObject>(Art + "Avatars/FemaleSkin01.prefab");
            int styles = PlayerAvatarArtCatalog.HairStyles.Length;
            catalog.MaleHair = new GameObject[styles]; catalog.FemaleHair = new GameObject[styles];
            for (int i = 0; i < styles; i++)
            {
                catalog.MaleHair[i] = Load<GameObject>(Art + "Hair/MaleHair" + PlayerAvatarArtCatalog.HairStyles[i] + ".prefab");
                catalog.FemaleHair[i] = Load<GameObject>(Art + "Hair/FemaleHair" + PlayerAvatarArtCatalog.HairStyles[i] + ".prefab");
            }
            catalog.SkinMaterials = new Material[6]; catalog.HairMaterials = new Material[5];
            for (int i = 0; i < 6; i++) catalog.SkinMaterials[i] = Load<Material>(Art + "Materials/Skin" + (i + 1).ToString("00") + ".mat");
            for (int i = 0; i < 5; i++) catalog.HairMaterials[i] = Load<Material>(Art + "Materials/Hair" + (i + 1).ToString("00") + ".mat");
            PlayerDefaultAvatarAssets.WearableInfo[] wear = PlayerDefaultAvatarAssets.Wearables();
            catalog.WearableItemIds = new string[wear.Length]; catalog.MaleWearables = new GameObject[wear.Length]; catalog.FemaleWearables = new GameObject[wear.Length];
            for (int i = 0; i < wear.Length; i++)
            {
                catalog.WearableItemIds[i] = wear[i].itemId;
                catalog.MaleWearables[i] = Load<GameObject>(Art + "Wearables/MaleWear" + wear[i].part + ".prefab");
                catalog.FemaleWearables[i] = Load<GameObject>(Art + "Wearables/FemaleWear" + wear[i].part + ".prefab");
            }
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            Debug.Log($"Player avatar catalog: 2 bodies, 8 hair styles, 6 skin and 5 hair materials, {wear.Length} fitted wearables per body.");
        }

        static T Load<T>(string path) where T : Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new FileNotFoundException("Missing player avatar art: " + path);
            return asset;
        }
    }
}
