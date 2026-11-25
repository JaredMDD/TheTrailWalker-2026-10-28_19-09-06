using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Milestone 16 slice 2: the AUDIT debug toggle (development builds only) that starts and stops the AuditRecorder.
    public sealed partial class CollectionPrototype
    {
        public static readonly UnityEngine.Rect AuditBand = UnityEngine.Rect.MinMaxRect(0.20f, 0.43f, 0.45f, 0.48f);   // left of TIER +1, under LEARN T1 RECIPES
        public const string AuditOff = "AUDIT", AuditOn = "AUDIT: REC";
        private AuditRecorder auditRecorder;
        private Text auditLabel;

        private void CreateAuditToggle()
        {
            if (!Debug.isDebugBuild) return;
            auditRecorder = gameObject.AddComponent<AuditRecorder>();
            auditRecorder.Bind(AuditContext);
            auditLabel = Button(safe, AuditOff, AuditBand.xMin, AuditBand.yMin, AuditBand.xMax, AuditBand.yMax, ToggleAudit);
            counterHiddenButtons.Add(auditLabel.transform.parent.gameObject);
            // Still on from the previous launch (a relaunch inside the scripted round): keep recording into a new file.
            float launch = AuditRecorder.TakeLaunchSeconds();   // only the process's first scene is a relaunch; later ones are scene loads
            if (AuditRecorder.Wanted) { AuditRecorder.FirstFrameSeconds = launch; auditRecorder.Begin(); }
            auditLabel.text = auditRecorder.Recording ? AuditOn : AuditOff;
        }

        private void ToggleAudit()
        {
            if (auditRecorder == null) return;
            if (auditRecorder.Recording) auditRecorder.End(); else { AuditRecorder.FirstFrameSeconds = 0; auditRecorder.Begin(); }
            auditLabel.text = auditRecorder.Recording ? AuditOn : AuditOff;
            Say(auditRecorder.Recording ? "Audit recording: frame time and memory per context" : "Audit stopped; file saved for adb");
        }

        // The spec's measurement contexts, most specific first.
        public static string AuditContextName(bool counter, bool sheet, HomeView view, bool settlement, bool camp)
        {
            if (counter) return "counters";
            if (sheet) return "sheets";
            if (view == HomeView.Backyard) return "backyard";
            if (view != HomeView.None) return "house interior";
            if (settlement) return "settlement street";
            return camp ? "camp" : "trail";
        }
        private string AuditContext() => AuditContextName(WarehouseOpen || LumberOpen || ShopOpen || BalloonOpen, HomeSheetsOpen || ContainerOpen,
            homeView, InSettlementScene, milestone5 && AtCamp);
    }
}
