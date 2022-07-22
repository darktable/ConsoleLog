using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.LowLevel;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ConsoleLog
{
    public static class Watcher
    {
        private const string FLOAT_FORMAT = "F3";
        private const string BYTE_FORMAT = "X4";

        private static readonly ConcurrentQueue<LogMessage> s_LogMessages = new ConcurrentQueue<LogMessage>();

        private static readonly ConcurrentQueue<(string key, string value)>
            s_ValueQueue = new ConcurrentQueue<(string, string)>();

        private static readonly ConcurrentQueue<string> s_ValueRemovalQueue = new ConcurrentQueue<string>();

        private static readonly Stopwatch m_logStopwatch = Stopwatch.StartNew();

        private static ulong frameCount = 0;
        private static bool clearLog = false;

        public static Action<LogMessage> OnLogMessage;
        public static Action<(string key, string value)> OnValueChange;
        public static Action<string> OnRemoveWatch;
        public static Action OnClearLog;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Application.logMessageReceivedThreaded -= OnLogMessageReceivedThreaded;
            Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;

            // subscribe to PlayerLoop PreLateUpdate
            Start();

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        public static void Start()
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();

            for (int i = 0; i < loop.subSystemList.Length; i++) {
                switch (loop.subSystemList[i].type.Name) {
                    case nameof(PreLateUpdate):
                        // To make sure the delegate doesn't subscribe twice.
                        loop.subSystemList[i].updateDelegate -= PreLateUpdate;
                        loop.subSystemList[i].updateDelegate += PreLateUpdate;

                        break;
                }
            }

            PlayerLoop.SetPlayerLoop(loop);
        }

        public static void Stop()
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();

            for (int i = 0; i < loop.subSystemList.Length; i++) {
                switch (loop.subSystemList[i].type.Name) {
                    case nameof(PreLateUpdate):
                        loop.subSystemList[i].updateDelegate -= PreLateUpdate;

                        break;
                }
            }

            PlayerLoop.SetPlayerLoop(loop);
        }

        private static void PreLateUpdate()
        {
            while (s_LogMessages.TryDequeue(out var logMessage))
            {
                OnLogMessage?.Invoke(logMessage);
            }

            if (clearLog)
            {
                clearLog = false;
                OnClearLog?.Invoke();
            }

            while (s_ValueQueue.TryDequeue(out var kvp))
            {
                OnValueChange?.Invoke(kvp);
            }

            while (s_ValueRemovalQueue.TryDequeue(out var key))
            {
                OnRemoveWatch?.Invoke(key);
            }

            frameCount++;
            m_logStopwatch.Restart();
        }

        private static void OnLogMessageReceivedThreaded(string message, string stacktrace, LogType logType)
        {
            var logMessage = new LogMessage(message, stacktrace, logType, frameCount, m_logStopwatch.ElapsedTicks);
            s_LogMessages.Enqueue(logMessage);
        }

        public static void LogValue<T>(string key, T value)
        {
            switch (value)
            {
                case Vector2 v2:
                    s_ValueQueue.Enqueue((key, v2.ToString(FLOAT_FORMAT)));
                    break;
                case Vector3 v3:
                    s_ValueQueue.Enqueue((key, v3.ToString(FLOAT_FORMAT)));
                    break;
                case Vector4 v4:
                    s_ValueQueue.Enqueue((key, v4.ToString(FLOAT_FORMAT)));
                    break;
                case float f:
                    s_ValueQueue.Enqueue((key, f.ToString(FLOAT_FORMAT)));
                    break;
                case double d:
                    s_ValueQueue.Enqueue((key, d.ToString(FLOAT_FORMAT)));
                    break;
                case decimal dec:
                    s_ValueQueue.Enqueue((key, dec.ToString(FLOAT_FORMAT)));
                    break;
                case byte b:
                    s_ValueQueue.Enqueue((key, b.ToString(BYTE_FORMAT)));
                    break;
                case sbyte:
                case bool:
                case int:
                case uint:
                case long:
                case short:
                case ushort:
                case ulong:
                    s_ValueQueue.Enqueue((key, value.ToString()));
                    break;
                default:
                    s_ValueQueue.Enqueue((key, value.ToString()));
                    break;
            }
        }

        public static void RemoveValue(string key)
        {
            s_ValueRemovalQueue.Enqueue(key);
        }

        public static void Clear()
        {
            clearLog = true;
        }

#if UNITY_EDITOR
        private static void OnPlayModeStateChanged(PlayModeStateChange playMode)
        {
            switch (playMode)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    Stop();
                    break;
            }
        }
#endif
    }
}
