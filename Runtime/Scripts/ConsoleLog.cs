using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace ConsoleLog
{
    public class ConsoleLog : MonoBehaviour
    {
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

        private void Update()
        {
            if (lastScrollSize == 1.0f && verticalScrollbar.size != lastScrollSize)
            {
                // scrolling just started, move scrollbar down.
                verticalScrollbar.value = 0.0f;
            }

            lastScrollSize = verticalScrollbar.size;
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
            // TODO: determine if scrolling should be locked to the bottom or not
            // (show new entries as they are or leave current ones on screen.
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
