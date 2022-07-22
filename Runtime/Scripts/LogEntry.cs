using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ConsoleLog
{
    public class LogEntry : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Text log;
        [SerializeField] private Text stacktrace;

        private LogType m_logType;
        private float ms;
        private ulong frame;

        private string className;
        private string methodName;

        private bool active = false;

        private GameObject stacktraceGameObject;
        private bool stacktraceVisible = false;

        private void Awake()
        {
            name = $"{nameof(LogEntry)}_{GetInstanceID():X8}";

            Assert.IsNotNull(log);
            Assert.IsNotNull(stacktrace);

            stacktraceGameObject = stacktrace.gameObject;
        }

        private void OnEnable()
        {
            active = true;
        }

        private void OnDisable()
        {
            active = false;

            stacktraceVisible = false;
            stacktraceGameObject.SetActive(false);
        }

        internal void Set(in LogMessage logMessage)
        {
            if (!active)
            {
                gameObject.SetActive(true);
            }

            frame = logMessage.frame;
            ms = logMessage.milliseconds;
            m_logType = logMessage.type;

#if UNITY_EDITOR || DEBUG || DEVELOPMENT_BUILD
            (className, methodName) = LogMessage.GetClassAndMethodName(logMessage.stacktrace);

            log.text = $"[{frame}:{ms:F2}][{className}.{methodName}]: {logMessage.message}";
#else
            log.text = $"[{frame}:{ms:F2}]: {logMessage.message}";
#endif

            switch (m_logType)
            {
                case LogType.Assert:
                case LogType.Error:
                    log.color = ConsoleLog.ErrorColor;
                    break;
                case LogType.Exception:
                    log.color = ConsoleLog.ExceptionColor;
                    break;
                case LogType.Warning:
                    log.color = ConsoleLog.WarningColor;
                    break;
                default:
                    log.color = ConsoleLog.DefaultColor;
                    break;
            }

            stacktrace.text = logMessage.stacktrace;

            transform.SetAsLastSibling();
        }

        public void Hide()
        {
            if (active)
            {
                gameObject.SetActive(false);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            stacktraceVisible = !stacktraceVisible;

            stacktraceGameObject.SetActive(stacktraceVisible);
        }
    }
}
