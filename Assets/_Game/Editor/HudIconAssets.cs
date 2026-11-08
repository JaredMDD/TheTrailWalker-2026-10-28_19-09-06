using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    internal sealed class HudIconImport : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/_Game/Resources/UI/Icons/", StringComparison.Ordinal) || !assetPath.EndsWith(".png", StringComparison.Ordinal)) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 256;
        }
    }

    public static class HudIconAssets
    {
        [MenuItem("The Trail Walker/Art/Capture HUD Icon Preview")]
        public static void CapturePreview()
        {
            const int width = 1200, height = 480;
            var image = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];
            string[] names = { "inventory", "trash", "map", "equipment", "user" };
            try
            {
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        pixels[y * width + x] = y >= height / 2 ? new Color(.83f,.83f,.83f) : new Color(.18f,.18f,.18f);
                for (int row = 0; row < 2; row++)
                {
                    for (int i = 0; i < names.Length; i++)
                    {
                        string path = "Assets/_Game/Resources/UI/Icons/" + names[i] + ".png";
                        if (AssetDatabase.LoadAssetAtPath<Sprite>(path) == null) throw new InvalidOperationException("Sprite import missing: " + names[i]);
                        var icon = new Texture2D(2, 2);
                        try
                        {
                            icon.LoadImage(File.ReadAllBytes(path));
                            int cx = 120 + i * 240, cy = 120 + row * 240;
                            if (names[i] != "trash")
                                for (int y = cy-82; y < cy+82; y++)
                                    for (int x = cx-90; x < cx+90; x++)
                                        pixels[y*width+x] = x == cx-90 || x == cx+89 || y == cy-82 || y == cy+81 ? Color.black : new Color(.94f,.94f,.94f);
                            for (int y = 0; y < 144; y++)
                                for (int x = 0; x < 144; x++)
                                {
                                    Color ink = icon.GetPixelBilinear((x+.5f)/144,(y+.5f)/144);
                                    int at = (cy-72+y)*width+cx-72+x;
                                    Color color = Color.Lerp(pixels[at],ink,ink.a);color.a=1;pixels[at]=color;
                                }
                        }
                        finally { UnityEngine.Object.DestroyImmediate(icon); }
                    }
                }
                image.SetPixels(pixels);image.Apply();
                Directory.CreateDirectory("Docs/Art/UI");File.WriteAllBytes("Docs/Art/UI/hud-icons-unity-v1.png",image.EncodeToPNG());
            }
            finally { UnityEngine.Object.DestroyImmediate(image); }
        }
    }
}
