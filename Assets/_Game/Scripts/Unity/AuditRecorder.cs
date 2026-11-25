using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace TheTrailWalker
{
    // Milestone 16 slice 2 (debug, development builds only; removed with the other debug tools before release). Samples
    // frame time per frame and memory/GC once a second, grouped by the context the game reports (trail, camp, settlement
    // street, house interior, backyard, counters, sheets). Context changes, pauses and resumes are logged as events with
    // the allocated memory at that moment (the leak trend across repeated transitions). Writes
    // persistentDataPath/audit-<build>-<started>.json every 30 s and on stop/pause, so a killed app keeps its data.
    // While on, a marker file keeps recording across restarts (save/load and relaunch cycles); each launch is a new file.
    public sealed class AuditRecorder : MonoBehaviour
    {
        public static string Marker => Path.Combine(Application.persistentDataPath, "audit-on.txt");
        public static bool Wanted { get { try { return File.Exists(Marker); } catch (Exception) { return false; } } }

        [Serializable] public sealed class ContextStats
        {
            public string context; public int frames; public float seconds;
            public float medianMs, p95Ms, worstMs;
            public float allocatedMinMb, allocatedMaxMb, allocatedMeanMb, reservedMaxMb; public int gcCount;
        }
        [Serializable] public sealed class AuditEvent { public float t; public string kind, context; public float allocatedMb; }
        [Serializable] public sealed class AuditFile
        {
            public string build, version, device, os, gpu, started, saved; public int screenWidth, screenHeight, targetFps;
            public float firstFrameSeconds; public List<ContextStats> contexts = new List<ContextStats>(); public List<AuditEvent> events = new List<AuditEvent>();
        }

        private sealed class Samples { public readonly List<float> Frames = new List<float>(); public readonly List<float> Allocated = new List<float>(); public float Seconds, ReservedMax; public int Gc; }

        private Func<string> context;
        private readonly Dictionary<string, Samples> samples = new Dictionary<string, Samples>();
        private readonly List<AuditEvent> events = new List<AuditEvent>();
        private string current, path; private DateTime started; private float memoryTimer, flushTimer, startedAt; private int lastGc;
        private bool skipFrame;   // the first frame after a resume spans the time away (slice 3: not a frame-time sample)
        public bool Recording { get; private set; }

        public void Bind(Func<string> contextSource) { context = contextSource; }

        public void Begin()
        {
            if (Recording) return;
            try { File.WriteAllText(Marker, "on"); } catch (IOException) { }
            samples.Clear(); events.Clear();
            started = DateTime.UtcNow; startedAt = Time.realtimeSinceStartup;
            path = Path.Combine(Application.persistentDataPath, "audit-" + Short(Application.buildGUID) + "-" + started.ToString("yyyyMMdd-HHmmss") + ".json");
            current = null; lastGc = GC.CollectionCount(0); memoryTimer = flushTimer = 0;
            Recording = true;
            Event("start");
        }

        public void End()
        {
            if (!Recording) return;
            Event("stop"); Flush();
            Recording = false;
            try { File.Delete(Marker); } catch (IOException) { }
        }

        private void Update()
        {
            if (!Recording) return;
            string now = context != null ? context() : "unknown";
            if (now != current) { current = now; Event("enter"); }
            Samples s = Get(current);
            float dt = Time.unscaledDeltaTime;
            if (skipFrame) { skipFrame = false; return; }
            s.Frames.Add(dt * 1000f); s.Seconds += dt;
            memoryTimer += dt; flushTimer += dt;
            if (memoryTimer >= 1f)
            {
                memoryTimer = 0;
                s.Allocated.Add(Mb(Profiler.GetTotalAllocatedMemoryLong()));
                s.ReservedMax = Mathf.Max(s.ReservedMax, Mb(Profiler.GetTotalReservedMemoryLong()));
                int gc = GC.CollectionCount(0); s.Gc += gc - lastGc; lastGc = gc;
            }
            if (flushTimer >= 30f) { flushTimer = 0; Flush(); }
        }

        private void OnApplicationPause(bool paused)
        {
            if (!Recording) return;
            Event(paused ? "pause" : "resume");
            if (paused) Flush(); else skipFrame = true;
        }
        private void OnApplicationQuit() { if (Recording) { Event("quit"); Flush(); } }

        private Samples Get(string key) { if (!samples.TryGetValue(key, out Samples s)) samples[key] = s = new Samples(); return s; }
        private void Event(string kind) => events.Add(new AuditEvent { t = Time.realtimeSinceStartup - startedAt, kind = kind, context = current, allocatedMb = Mb(Profiler.GetTotalAllocatedMemoryLong()) });
        private static float Mb(long bytes) => bytes / (1024f * 1024f);
        private static string Short(string guid) => string.IsNullOrEmpty(guid) ? "editor" : guid.Substring(0, Math.Min(8, guid.Length));

        public static ContextStats Summarise(string name, IList<float> frameMs, IList<float> allocatedMb, float seconds, float reservedMax, int gc)
        {
            var stats = new ContextStats { context = name, frames = frameMs.Count, seconds = seconds, reservedMaxMb = reservedMax, gcCount = gc };
            if (frameMs.Count > 0)
            {
                var sorted = frameMs.OrderBy(f => f).ToList();
                stats.medianMs = sorted[sorted.Count / 2]; stats.p95Ms = sorted[Math.Max(0, (int)Math.Ceiling(sorted.Count * 0.95) - 1)]; stats.worstMs = sorted[sorted.Count - 1];   // p95 by nearest rank
            }
            if (allocatedMb.Count > 0) { stats.allocatedMinMb = allocatedMb.Min(); stats.allocatedMaxMb = allocatedMb.Max(); stats.allocatedMeanMb = allocatedMb.Average(); }
            return stats;
        }

        private void Flush()
        {
            var file = new AuditFile
            {
                build = Application.buildGUID, version = Application.version, device = SystemInfo.deviceModel, os = SystemInfo.operatingSystem, gpu = SystemInfo.graphicsDeviceName,
                started = started.ToString("O"), saved = DateTime.UtcNow.ToString("O"), screenWidth = Screen.width, screenHeight = Screen.height, targetFps = Application.targetFrameRate,
                firstFrameSeconds = FirstFrameSeconds, events = events
            };
            foreach (var pair in samples) file.contexts.Add(Summarise(pair.Key, pair.Value.Frames, pair.Value.Allocated, pair.Value.Seconds, pair.Value.ReservedMax, pair.Value.Gc));
            try { File.WriteAllText(path, JsonUtility.ToJson(file, true)); } catch (IOException e) { Debug.LogWarning("Audit recorder: " + e.Message); }
        }

        // Launch-to-first-recorded-frame time when recording resumed at startup (the relaunch half of a save/load cycle).
        // 0 for a file started by a scene load inside the same process (slice 3: those were reported as relaunches).
        public static float FirstFrameSeconds;
        private static bool launchSeen;
        public static float TakeLaunchSeconds() { if (launchSeen) return 0f; launchSeen = true; return Time.realtimeSinceStartup; }
    }
}
