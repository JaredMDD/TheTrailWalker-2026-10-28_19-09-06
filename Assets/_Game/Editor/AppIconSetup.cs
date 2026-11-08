using System;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Assigns the generated app icon (Tools/Art/generate_app_icon.py) to the Android player settings: adaptive icons
    // (scene background + walker foreground) for Android 8+, the full-bleed icon for round and legacy launchers, and
    // the default icon. Rebuild the PNGs with the script, then run this menu item.
    public static class AppIconSetup
    {
        private const string Folder = "Assets/_Game/Art/AppIcon/";

        [MenuItem("The Trail Walker/Art/Apply App Icon")]
        public static void Apply()
        {
            Texture2D full = Prepare("AppIcon.png"), background = Prepare("AppIconBackground.png"), foreground = Prepare("AppIconForeground.png");
            NamedBuildTarget android = NamedBuildTarget.Android;
            PlatformIcon[] adaptive = PlayerSettings.GetPlatformIcons(android, AndroidPlatformIconKind.Adaptive);
            foreach (PlatformIcon icon in adaptive) icon.SetTextures(background, foreground);
            PlayerSettings.SetPlatformIcons(android, AndroidPlatformIconKind.Adaptive, adaptive);
            foreach (PlatformIconKind kind in new[] { AndroidPlatformIconKind.Round, AndroidPlatformIconKind.Legacy })
            {
                PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(android, kind);
                foreach (PlatformIcon icon in icons) icon.SetTexture(full);
                PlayerSettings.SetPlatformIcons(android, kind, icons);
            }
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { full }, IconKind.Any);
            AssetDatabase.SaveAssets();
            Debug.Log("App icon applied: adaptive, round, legacy and default icons.");
        }

        // Icons are read at build time; keep them uncompressed, without mipmaps, and with an alpha channel for the foreground.
        private static Texture2D Prepare(string file)
        {
            string path = Folder + file;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter ?? throw new InvalidOperationException("Missing icon texture: " + path);
            importer.textureType = TextureImporterType.Default; importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 1024;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
