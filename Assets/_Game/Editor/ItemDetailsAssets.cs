using System;
using System.IO;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Inventory;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheTrailWalker.Editor
{
    internal sealed class ItemDetailsImport : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/_Game/Resources/UI/ItemDetails/",StringComparison.Ordinal)||!assetPath.EndsWith(".png",StringComparison.Ordinal))return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=100;
            importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            bool paper=assetPath.EndsWith("/parchment.png",StringComparison.Ordinal);
            importer.maxTextureSize=paper?1024:256;
            importer.spriteBorder=paper?new Vector4(160,160,160,160):Vector4.zero;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
            settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
        }
    }

    public static class ItemDetailsAssets
    {
        private static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;return rect;
        }
        private static UnityEngine.UI.Text Text(Transform parent,string value,Vector2 min,Vector2 max)
        {
            var text=Rect("Label",parent,min,max).gameObject.AddComponent<UnityEngine.UI.Text>();
            text.text=value;text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=23;
            text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;return text;
        }

        [MenuItem("The Trail Walker/Art/Capture Parchment Item Details Preview")]
        public static void CapturePreview()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Capture art preview outside Play mode.");
            for(int q=1;q<=5;q++)if(Resources.Load<Sprite>("UI/ItemDetails/quality_"+q)==null)throw new InvalidOperationException("Missing quality sprite "+q);
            if(Resources.Load<Sprite>("UI/ItemDetails/parchment")==null)throw new InvalidOperationException("Missing parchment sprite.");
            Scene previous=SceneManager.GetActiveScene();
            Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            RenderTexture target=null;Texture2D image=null;RenderTexture old=RenderTexture.active;
            try
            {
                SceneManager.SetActiveScene(scene);
                var cam=new GameObject("Art preview camera").AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=1.6f;
                cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.17f,.19f,.21f);
                cam.nearClipPlane=.01f;cam.farClipPlane=30;cam.cullingMask=1<<31;
                var canvas=new GameObject("Art preview canvas",typeof(RectTransform),typeof(Canvas)).GetComponent<Canvas>();
                canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=cam;
                canvas.GetComponent<RectTransform>().sizeDelta=new Vector2(900,360);canvas.transform.localScale=Vector3.one*.005f;
                canvas.transform.position=new Vector3(0,.35f,0);
                RectTransform panel=Rect("Parchment",canvas.transform,Vector2.zero,Vector2.one);panel.gameObject.AddComponent<UnityEngine.UI.Image>();
                var info=Text(panel,"Pine branch (Material)\nQuantity 1,000 | Ready | Value 100",Vector2.zero,Vector2.one);
                var close=Rect("Close",panel,new Vector2(.53f,.05f),new Vector2(.95f,.32f));close.gameObject.AddComponent<UnityEngine.UI.Image>();close.gameObject.AddComponent<UnityEngine.UI.Button>();
                Text(close,"CLOSE",Vector2.zero,Vector2.one);
                ItemDetailsStyle.Apply(panel.gameObject,info);
                var ledger=new SpatialInventory(6000,4000);
                ItemEntry sample=ledger.AddWorld("art-preview-branch",PrototypeItems.Branch,400,1);
                ItemEntry extra=ledger.AddWorld("art-preview-branch-extra",PrototypeItems.Branch,600,5);
                ledger.Reserve(sample,1);ledger.TryPlace(sample,1,0,0,false,out _);ledger.Release(sample,1);
                ledger.Reserve(extra,2);
                if(!ledger.TryPlace(extra,2,0,0,true,out string reason))throw new InvalidOperationException(reason);
                ledger.Release(extra,2);
                Rect("Quality breakdown",panel,new Vector2(.08f,.33f),new Vector2(.92f,.48f)).gameObject.AddComponent<QualityStarStrip>().Bind(sample,true);
                for(int q=1;q<=5;q++)
                {
                    RectTransform badge=Rect("Quality "+q,canvas.transform,new Vector2(.5f,.5f),new Vector2(.5f,.5f));
                    badge.sizeDelta=new Vector2(105,105);badge.anchoredPosition=new Vector2((q-3)*130,-275);
                    var graphic=badge.gameObject.AddComponent<UnityEngine.UI.Image>();graphic.sprite=Resources.Load<Sprite>("UI/ItemDetails/quality_"+q);graphic.preserveAspect=true;
                }
                foreach(Transform t in canvas.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
                Canvas.ForceUpdateCanvases();
                target=new RenderTexture(1280,800,24);target.Create();
                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(cam,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest { destination=target });
                RenderTexture.active=target;image=new Texture2D(1280,800,TextureFormat.RGBA32,false);image.ReadPixels(new UnityEngine.Rect(0,0,1280,800),0,0);image.Apply();
                Directory.CreateDirectory("Docs/Art/UI");File.WriteAllBytes("Docs/Art/UI/parchment-details-unity-v1.png",image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active=old;if(image!=null)UnityEngine.Object.DestroyImmediate(image);
                if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}
                EditorSceneManager.CloseScene(scene,true);if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);
            }
        }
    }
}
