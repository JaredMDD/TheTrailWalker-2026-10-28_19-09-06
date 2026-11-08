using System;
using System.IO;
using TheTrailWalker.Harvest;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Review tool (November 7, 2026): filmstrips of AnimalAnimator poses on the live rabbit prefab, side view over a
    // ground line, so hop/walk/graze changes can be checked without play mode. Writes Docs/Art/Tier01/Motion/.
    public static class AnimalMotionPreview
    {
        const string Rabbit = "Assets/_Game/Resources/Tier01/Interactables/Rabbit.prefab";
        const string Docs = "Docs/Art/Tier01/Motion/";

        [MenuItem("The Trail Walker/Art/Animal Motion Filmstrips")]
        public static void CaptureAll()
        {
            Directory.CreateDirectory(Docs);
            // One hop: frames at evenly spaced phases, each advanced by the hop's forward travel at that phase.
            Strip("rabbit-hop-cycle.png", 9, (anim, root, i) =>
            {
                float p = i / 8f;
                float s = Mathf.Clamp01((p - AnimalAnimator.HopTakeoff) / (AnimalAnimator.HopLanding - AnimalAnimator.HopTakeoff)); s = s * s * (3f - 2f * s);
                root.position += Vector3.forward * (s * anim.Stride);
                anim.Pose(p, 1f, 1f, 0f, .6f, 0f, 0f);
                return "p=" + p.ToString("0.00");
            });
            Strip("rabbit-hop-fleeing.png", 9, (anim, root, i) =>
            {
                float p = i / 8f;
                float s = Mathf.Clamp01((p - AnimalAnimator.HopTakeoff) / (AnimalAnimator.HopLanding - AnimalAnimator.HopTakeoff)); s = s * s * (3f - 2f * s);
                root.position += Vector3.forward * (s * anim.Stride * 2.2f);
                anim.Pose(p, 1f, 2.2f, 0f, 1f, 0f, 0f);
                return "";
            });
            // Generic four-beat walk on the rabbit rig, standing in for later walking species.
            Strip("walk-cycle-on-rabbit-rig.png", 8, (anim, root, i) =>
            {
                var parts = root.GetComponent<Tier1RabbitRig>().Parts(); parts.Gait = AnimalGait.Walk;
                var walker = new AnimalAnimator(parts); walker.Pose(i / 8f, 1f, 1f, 0f, 0f, 0f, 0f);
                return "";
            });
            Strip("rabbit-idle-poses.png", 5, (anim, root, i) =>
            {
                float[] graze = { 0, .5f, 1, 1, 0 }, alert = { 0, 0, 0, 0, 1 }, time = { 0, 0, 0, .14f, 0 }, look = { 0, 0, 0, 0, 25 };
                anim.Pose(0f, 0f, 1f, graze[i], alert[i], look[i], time[i]);
                return "";
            });
            Debug.Log("Animal motion filmstrips written to " + Docs);
        }

        static void Strip(string file, int frames, Func<AnimalAnimator, Transform, int, string> pose)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Rabbit);
            if (prefab == null) throw new InvalidOperationException("Missing " + Rabbit);
            var preview = new PreviewRenderUtility();
            try
            {
                float spacing = .75f;
                for (int i = 0; i < frames; i++)
                {
                    var go = UnityEngine.Object.Instantiate(prefab);
                    go.transform.SetPositionAndRotation(new Vector3(0, 0, 0), Quaternion.identity);
                    var anim = new AnimalAnimator(go.GetComponent<Tier1RabbitRig>().Parts());
                    pose(anim, go.transform, i);
                    // Side view: the rabbit faces +Z; lay frames out along X, keeping each frame's own forward travel.
                    float travel = go.transform.position.z;
                    go.transform.SetPositionAndRotation(new Vector3((i - (frames - 1) * .5f) * spacing, 0, 0), Quaternion.Euler(0, 90, 0));
                    go.transform.position += Vector3.right * travel * .5f;
                    preview.AddSingleGO(go);
                }
                var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ground.transform.localScale = new Vector3(frames * spacing + 1f, .01f, .6f); ground.transform.position = new Vector3(0, -.005f, 0);
                preview.AddSingleGO(ground);
                int width = 220 * frames, height = 360;
                preview.camera.orthographic = true; preview.camera.orthographicSize = .36f;
                preview.camera.aspect = (float)width / height;
                preview.camera.orthographicSize = Mathf.Max(.36f, (frames * spacing + .3f) * .5f / preview.camera.aspect);
                preview.camera.transform.position = new Vector3(0, .3f, -6); preview.camera.transform.LookAt(new Vector3(0, .3f, 0));
                preview.camera.clearFlags = CameraClearFlags.SolidColor; preview.camera.backgroundColor = new Color(.19f, .22f, .25f);
                preview.camera.nearClipPlane = .01f; preview.camera.farClipPlane = 30;
                preview.lights[0].intensity = 1.3f; preview.lights[0].transform.rotation = Quaternion.Euler(40, 30, 0);
                preview.lights[1].intensity = .6f; preview.ambientColor = new Color(.4f, .4f, .4f);
                preview.BeginStaticPreview(new Rect(0, 0, width, height)); preview.Render(true);
                var image = preview.EndStaticPreview();
                try { File.WriteAllBytes(Docs + file, image.EncodeToPNG()); } finally { UnityEngine.Object.DestroyImmediate(image); }
            }
            finally { preview.Cleanup(); }
        }
    }
}
