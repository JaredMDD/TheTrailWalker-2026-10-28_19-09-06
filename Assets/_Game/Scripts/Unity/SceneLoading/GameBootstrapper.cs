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
            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0;
            StartCoroutine(LoadTrail());
        }
        private IEnumerator LoadTrail()
        {
            Movement.SetPause(MovementPauseReasons.Loading, true);
            if (SceneManager.GetActiveScene().name != initialScene)
                yield return SceneManager.LoadSceneAsync(initialScene, LoadSceneMode.Single);
            var trail = FindAnyObjectByType<MovementPrototype>();
            if (trail == null) { Debug.LogError("[Milestone1] MovementTrail has no prototype binding."); yield break; }
            trail.Bind(Movement);
            Movement.SetPause(MovementPauseReasons.Loading, false);
        }
        private void OnApplicationPause(bool paused) => Movement.SetPause(MovementPauseReasons.Suspended, paused);
        private void OnApplicationFocus(bool focused) => Movement.SetPause(MovementPauseReasons.Unfocused, !focused);
    }
}
