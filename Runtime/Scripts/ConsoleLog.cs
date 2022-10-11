using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace ConsoleLog
{
    public class ConsoleLog : MonoBehaviour
    {
        private const float minScrollDelta = 0.0001f;
        
        public static Color WarningColor { get; private set; }
        public static Color ErrorColor { get; private set; }
        public static Color ExceptionColor { get; private set; }
        public static Color DefaultColor { get; private set; }

        [SerializeField] private int scrollBack = 100;

        [SerializeField] private LogEntry logEntryPrefab;

        [SerializeField] private Transform root;
        [SerializeField] private ScrollRect logScrollRect;

        [SerializeField] private Color warningColor = Color.yellow;

        [SerializeField] private Color errorColor = Color.red;

        [SerializeField] private Color exceptionColor = Color.magenta;

        [SerializeField] private Color defaultColor = Color.white;

        private readonly Queue<LogEntry> logEntryQueue = new Queue<LogEntry>();

        private Canvas canvas;
        private Scrollbar verticalScrollbar;
        private float lastScrollY = 0;
        private float lastScrollSize = float.MinValue;
        private float currentScrollY = 0;
        private float currentScrollSize = float.MinValue;

        private void Awake()
        {
            Assert.IsNotNull(root);
            Assert.IsNotNull(logEntryPrefab);
            Assert.IsNotNull(logScrollRect);

            verticalScrollbar = logScrollRect.verticalScrollbar;

            for (int i = 0; i < scrollBack; i++)
            {
                var entry = Instantiate(logEntryPrefab, root);
                entry.Hide();

                logEntryQueue.Enqueue(entry);
            }

            Assert.IsTrue(logEntryQueue.Count > 0);

            WarningColor = warningColor;
            ErrorColor = errorColor;
            ExceptionColor = exceptionColor;
            DefaultColor = defaultColor;

            canvas = GetComponentInParent<Canvas>(true);
            if (canvas == null)
            {
                canvas = GetComponentInChildren<Canvas>(true);
            }

            Assert.IsNotNull(canvas);
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            SubscribeEvents(false);
        }

        private IEnumerator Start()
        {
            while (Camera.main == null)
            {
                yield return null;
            }

            if (canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
            }
        }

        private void LateUpdate()
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (NearlyZero(lastScrollY) && lastScrollSize != currentScrollSize)
            {
                // determine if scrolling should be locked to the bottom or not
                // (show new entries as they are added or leave current ones on screen.
                verticalScrollbar.value = 0.0f;
            }

            lastScrollSize = currentScrollSize;
            lastScrollY = currentScrollY;
        }

        private void SubscribeEvents(bool subscribe = true)
        {
            if (subscribe)
            {
                Watcher.OnLogMessage += OnLogMessage;
                Watcher.OnClearLog += OnClearLog;

                verticalScrollbar.onValueChanged.AddListener(OnVerticalScrollChanged);
            }
            else
            {
                Watcher.OnLogMessage -= OnLogMessage;
                Watcher.OnClearLog -= OnClearLog;

                verticalScrollbar.onValueChanged.RemoveListener(OnVerticalScrollChanged);
            }
        }

        private void OnLogMessage(LogMessage msg)
        {
            var logEntry = logEntryQueue.Dequeue();

            logEntry.Set(msg);

            logEntryQueue.Enqueue(logEntry);
            
            UpdateScroll(verticalScrollbar.value);
        }

        private void OnClearLog()
        {
            foreach (var entry in logEntryQueue)
            {
                entry.Hide();
            }
        }

        private void OnVerticalScrollChanged(float value)
        {
            if (value != 0.0f && NearlyZero(value))
            {
                verticalScrollbar.value = 0.0f;
                return;
            }
            
            UpdateScroll(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateScroll(float value)
        {
            currentScrollSize = verticalScrollbar.size;
            currentScrollY = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool NearlyZero(float value)
        {
            return Mathf.Abs(value) < minScrollDelta;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            WarningColor = warningColor;
            ErrorColor = errorColor;
            ExceptionColor = exceptionColor;
            DefaultColor = defaultColor;
        }
#endif
    }
}
