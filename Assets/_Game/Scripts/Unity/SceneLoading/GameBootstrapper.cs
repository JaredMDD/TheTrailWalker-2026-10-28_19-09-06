using System.Collections;
using TheTrailWalker.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheTrailWalker.SceneLoading
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        public const string TrailScene = "MovementTrail";
        [SerializeField] private string initialScene = TrailScene;
        public void Configure(string sceneName) => initialScene = sceneName;
        public MovementState Movement { get; } = new MovementState();
        private void Awake()
        {
            // Instance discovery is confined to bootstrap, not used as a gameplay service locator.
            if (FindObjectsByType<GameBootstrapper>().Length > 1) { Destroy(gameObject); return; }
            DontDestroyOnLoad(gameObject);
            // High-detail test profile (October 30, 2026): 60 FPS while exploring device limits; graphics tiers come later.
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            StartCoroutine(LoadTrail());
        }
        private bool loading;
        private IEnumerator LoadTrail(bool force = false)
        {
            loading = true;
            Movement.SetPause(MovementPauseReasons.Loading, true);
            if (force || SceneManager.GetActiveScene().name != initialScene)
                yield return SceneManager.LoadSceneAsync(initialScene, LoadSceneMode.Single);
            var trail = FindAnyObjectByType<MovementPrototype>();
            if (trail == null) { Debug.LogError("[Milestone1] MovementTrail has no prototype binding."); loading = false; yield break; }
            trail.Bind(Movement, RequestReload);
            Movement.SetPause(MovementPauseReasons.Loading, false);
            loading = false;
        }
        // Camp travel: the destination is already saved, so the reloaded scene presents it. Duplicate requests are ignored.
        public void RequestReload() { if (!loading) StartCoroutine(LoadTrail(true)); }
        private void OnApplicationPause(bool paused) => Movement.SetPause(MovementPauseReasons.Suspended, paused);
        private void OnApplicationFocus(bool focused) => Movement.SetPause(MovementPauseReasons.Unfocused, !focused);
    }
}
