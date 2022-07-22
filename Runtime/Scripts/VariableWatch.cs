using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ConsoleLog
{
    public class VariableWatch : MonoBehaviour
    {
        [SerializeField] private int maxValues = 100;
        [SerializeField] private ValueEntry valueEntryPrefab;
        [SerializeField] private Transform root;

        private Canvas canvas;

        private readonly Queue<ValueEntry> valueEntryQueue = new Queue<ValueEntry>();
        private readonly Dictionary<string, ValueEntry> currentValues = new Dictionary<string, ValueEntry>();
        private readonly List<string> sortedKeys = new List<string>();

        private void Awake()
        {
            for (int i = 0; i < maxValues; i++)
            {
                var entry = Instantiate(valueEntryPrefab, root);
                entry.Hide();

                valueEntryQueue.Enqueue(entry);
            }

            Assert.IsTrue(valueEntryQueue.Count > 0);

            canvas = GetComponentInChildren<Canvas>(true);

            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>(true);
            }

            Assert.IsNotNull(canvas);
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

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            SubscribeEvents(false);
        }

        private void SubscribeEvents(bool subscribe = true)
        {
            if (subscribe)
            {
                Watcher.OnValueChange += OnValueChange;
                Watcher.OnRemoveWatch += OnRemoveWatch;
            }
            else
            {
                Watcher.OnValueChange -= OnValueChange;
                Watcher.OnRemoveWatch -= OnRemoveWatch;
            }
        }

        private void OnRemoveWatch(string key)
        {
            if (currentValues.Remove(key, out var valueEntry))
            {
                valueEntry.Hide();
                valueEntryQueue.Enqueue(valueEntry);

                sortedKeys.Remove(key);
            }
        }

        private void OnValueChange((string key, string value) kvp)
        {
            if (currentValues.TryGetValue(kvp.key, out var entry))
            {
                entry.Set(kvp.key, kvp.value);
            }
            else if (valueEntryQueue.TryDequeue(out var valueEntry))
            {
                valueEntry.Set(kvp.key, kvp.value);
                currentValues[kvp.key] = valueEntry;

                sortedKeys.Add(kvp.key);
                sortedKeys.Sort();

                var count = sortedKeys.Count;
                for (int i = 0; i < count; i++)
                {
                    currentValues[sortedKeys[i]].SetOrder(i);
                }
            }
        }
    }
}
