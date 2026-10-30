using System;
using System.IO;
using TheTrailWalker.Domain.Equipment;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Isolated model-only validation: intentionally does not open or modify the game save.
    public sealed class Milestone4ModelProbe : MonoBehaviour
    {
        [Serializable] private sealed class Report { public string utc, status, message; public int checks; }
        private void Start()
        {
            var report = new Report { utc = DateTime.UtcNow.ToString("O") };
            try { report.checks = EquipmentModelChecks.Run(); report.status = "PASS"; report.message = "Equipment and stamina models passed. Gameplay integration is still pending."; }
            catch (Exception error) { report.status = "FAIL"; report.message = error.ToString(); Debug.LogException(error); }
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "milestone4-model-check.json"), JsonUtility.ToJson(report, true));
            var root = new GameObject("Model validation", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scale = root.GetComponent<CanvasScaler>(); scale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scale.referenceResolution = new Vector2(720, 1280);
            var background = new GameObject("Background", typeof(RectTransform), typeof(Image)); background.transform.SetParent(root.transform, false);
            var rect = background.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            background.GetComponent<Image>().color = new Color(0.05f, 0.12f, 0.13f);
            var labelObject = new GameObject("Result", typeof(RectTransform), typeof(Text)); labelObject.transform.SetParent(root.transform, false);
            rect = labelObject.GetComponent<RectTransform>(); rect.anchorMin = new Vector2(.08f, .2f); rect.anchorMax = new Vector2(.92f, .8f); rect.offsetMin = rect.offsetMax = Vector2.zero;
            Text label = labelObject.GetComponent<Text>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 32; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white;
            label.text = $"MILESTONE 4\nMODEL TEST ONLY\n\n{report.status}: {report.checks} checks\n\n{report.message}\n\nYour existing game save was not opened.";
            Debug.Log("[Milestone4] " + report.status + " " + report.checks + " " + report.message);
        }
    }
}
