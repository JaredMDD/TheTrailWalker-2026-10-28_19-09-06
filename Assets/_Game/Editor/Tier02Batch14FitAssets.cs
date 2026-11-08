using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheTrailWalker.Editor
{
    // Read-only current-body sources and review-only rigid poses. No runtime equip/rig changes.
    public static class Tier02Batch14FitAssets
    {
        const string Avatar = "Assets/_Game/Art/Characters/PlayerDefault/";
        const string Earlier = "Assets/_Game/Art/Characters/Tier01Accessories/";
        const string Docs = "Docs/Art/Tier02/Batch14/";
        [Serializable] sealed class Part { public string name; public string[] materials; }
        [Serializable] sealed class Parts { public Part[] parts; }
        static readonly List<Mesh> OwnedMeshes = new List<Mesh>();
        static Parts Read(string folder, string file) => JsonUtility.FromJson<Parts>(File.ReadAllText(folder + file));

        static GameObject Source(Transform parent, string body, string suffix, bool earlier = false)
        {
            string folder = earlier ? Earlier : Avatar;
            var manifest = Read(folder, earlier ? "Tier01AccessoriesManifest.json" : "PlayerAvatarManifest.json");
            var part = Array.Find(manifest.parts, p => p.name == body + suffix);
            if (part == null) throw new InvalidOperationException("Missing source part " + body + suffix);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(folder + part.name + ".obj");
            var instance = Object.Instantiate(model, parent, false); instance.name = part.name;
            var materials = Array.ConvertAll(part.materials, key =>
            {
                string name = key == "Hair" || key == "Brows" ? "Hair02" : key == "Skin" ? "Skin03" : key;
                var mat = AssetDatabase.LoadAssetAtPath<Material>(Avatar + "Unity/Materials/" + name + ".mat");
                if (mat == null) mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Resources/Characters/Tier01Accessories/Materials/" + name + ".mat");
                if (mat == null) throw new FileNotFoundException("Missing read-only body material: " + name);
                return mat;
            });
            foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>()) renderer.sharedMaterials = materials;
            return instance;
        }

        static Transform Pivot(Transform parent, string name, Vector3 position)
        {
            var obj = new GameObject(name).transform; obj.SetParent(parent, false); obj.localPosition = position; return obj;
        }

        static GameObject NewArt(Transform parent, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Resources/Tier02/Wearables/" + name + ".prefab");
            if (prefab == null) throw new FileNotFoundException("Build batch 14 first: " + name);
            return Object.Instantiate(prefab, parent, false);
        }

        static Vector3 Stride(Vector3 point, bool boot)
        {
            if (!boot && point.y > .84f) return point;
            bool left = point.x < 0;
            float angle = left ? 12 : -24;
            float amount = boot ? 1 : Mathf.Clamp01((.84f - point.y) / .07f);
            Vector3 hip = new Vector3(left ? -.10f : .10f, .88f, 0);
            return hip + Quaternion.AngleAxis(angle * amount, Vector3.right) * (point - hip);
        }

        static void PoseMesh(GameObject root, bool boots)
        {
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                var clone = Object.Instantiate(filter.sharedMesh); OwnedMeshes.Add(clone);
                var vertices = clone.vertices;
                for (int i = 0; i < vertices.Length; i++) vertices[i] = Stride(vertices[i], boots);
                clone.vertices = vertices; clone.RecalculateNormals(); clone.RecalculateBounds(); filter.sharedMesh = clone;
            }
        }

        static void PoseSeated(GameObject root)
        {
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                var clone = Object.Instantiate(filter.sharedMesh); OwnedMeshes.Add(clone);
                var points = clone.vertices;
                for (int i=0;i<points.Length;i++)
                {
                    var p=points[i];
                    var hip=new Vector3(p.x < 0 ? -.10f : .10f,.88f,0);
                    float blend=Mathf.Clamp01((.84f-p.y)/.08f);
                    var thigh=hip+Quaternion.AngleAxis(-90*blend,Vector3.right)*(p-hip)+new Vector3(0,-.38f,0);
                    var shin=new Vector3(p.x,p.y,p.z+.38f);
                    points[i]=Vector3.Lerp(shin,thigh,Mathf.Clamp01((p.y-.48f)/.04f));
                }
                clone.vertices=points;clone.RecalculateNormals();clone.RecalculateBounds();filter.sharedMesh=clone;
            }
        }

        static void PoseReach(GameObject root, string body, bool skin)
        {
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                var clone=Object.Instantiate(filter.sharedMesh);OwnedMeshes.Add(clone);var points=clone.vertices;
                for(int i=0;i<points.Length;i++)
                {
                    var p=points[i];float amount;
                    if(skin) amount=Mathf.Abs(p.x)>.13f ? 1 : 0;
                    else amount=p.y>1.08f ? Mathf.Clamp01((Mathf.Abs(p.x)-.135f)/.08f) : 0;
                    float sx=body=="Female" ? .188f : .205f, sy=body=="Female" ? 1.340f : 1.355f;
                    var shoulder=new Vector3(p.x<0 ? -sx : sx,sy,0);
                    points[i]=shoulder+Quaternion.AngleAxis(-40*amount,Vector3.right)*(p-shoulder);
                }
                clone.vertices=points;clone.RecalculateNormals();clone.RecalculateBounds();filter.sharedMesh=clone;
            }
        }

        static GameObject Review(string body, string style, string pose)
        {
            var root = new GameObject(body + style + pose + "StaticArtStudy");
            bool seated = pose == "seated", stride = pose == "stride";
            float drop = seated ? -.38f : stride ? .022f : 0;
            var upper = Pivot(root.transform, "UpperBodyArtStudy", new Vector3(0, drop, 0));
            var skin = Source(upper, body, "SeatedUpperSkinStudy", true);
            var shirt = NewArt(upper, body + "WearFlaxWorkShirt");
            var pants = NewArt(root.transform, body + "WearFlaxTrousers");
            if (seated) PoseSeated(pants);
            if (pose == "reach") { PoseReach(skin, body, true); PoseReach(shirt, body, false); }
            var boots = NewArt(root.transform, body + "WearMarshBoots");
            if (seated) boots.transform.localPosition = new Vector3(0, 0, .38f);
            if (stride)
            {
                PoseMesh(pants, false); PoseMesh(boots, true);
                pants.transform.localPosition = new Vector3(0, drop, 0); boots.transform.localPosition = new Vector3(0, drop, 0);
            }
            var head = Pivot(upper, "HeadAnchor", new Vector3(0, 1.43f, 0));
            Source(head, body, "Head"); Source(head, body, "Eyes");
            Source(head, body, "HatHair" + style, true);
            NewArt(head, body + "WearBeaverFurCap");
            if (pose == "reach") head.localRotation = Quaternion.Euler(18, 0, 0);
            return root;
        }

        static void Render(string file, GameObject root, float yaw, Vector3 center, float size)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                root.transform.rotation = Quaternion.Euler(0, yaw, 0); preview.AddSingleGO(root);
                var camera = preview.camera; camera.orthographic = true; camera.orthographicSize = size;
                camera.transform.position = center + new Vector3(0, .025f, 4); camera.transform.LookAt(center);
                camera.nearClipPlane = .01f; camera.farClipPlane = 20;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.18f,.22f,.24f);
                preview.lights[0].intensity = 1.25f; preview.lights[0].transform.rotation = Quaternion.Euler(30,210,0);
                preview.lights[1].intensity = .8f; preview.ambientColor = new Color(.5f,.5f,.5f);
                preview.BeginStaticPreview(new Rect(0,0,800,900)); preview.Render(true);
                var image = preview.EndStaticPreview();
                try { File.WriteAllBytes(Docs + file, image.EncodeToPNG()); }
                finally { Object.DestroyImmediate(image); }
            }
            finally
            {
                preview.Cleanup();
                foreach (var mesh in OwnedMeshes) Object.DestroyImmediate(mesh);
                OwnedMeshes.Clear();
            }
        }

        public static void Capture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Fit studies require Edit mode.");
            Directory.CreateDirectory(Docs);
            foreach (string body in new[] { "Male", "Female" })
            {
                foreach (string pose in new[] { "standing", "seated", "stride", "reach" })
                    foreach (int yaw in new[] { 0, 90, 180, 35 })
                        Render(body + "-fit-" + pose + "-" + yaw + ".png", Review(body,"Tied",pose), yaw,
                               new Vector3(0,pose == "seated" ? .68f : .88f,0),pose == "seated" ? .80f : .99f);
                foreach (int yaw in new[] { 0,90,180,35 })
                {
                    Render(body+"-shirt-close-"+yaw+".png",Review(body,"Tied","standing"),yaw,new Vector3(0,1.18f,0),.40f);
                    Render(body+"-pants-close-"+yaw+".png",Review(body,"Tied","standing"),yaw,new Vector3(0,.62f,0),.57f);
                }
            }
            File.WriteAllText(Docs + "fit-capture-result.txt", "SUCCEEDED: 48 current-body static fit/garment views. Standing/seated/rigid stride/arm reach; not skinned animation, runtime binding or phone verification. " + DateTime.UtcNow.ToString("O"));
        }
    }
}
