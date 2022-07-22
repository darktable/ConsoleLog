using UnityEngine;
using UnityEngine.UI;

namespace ConsoleLog
{

    public class ValueEntry : MonoBehaviour
    {
        [SerializeField] private Text key;
        [SerializeField] private Text value;

        private bool active = false;

        private void Awake()
        {
            name = $"{nameof(ValueEntry)}_{GetInstanceID():X8}";
        }

        private void OnEnable()
        {
            active = true;
        }

        private void OnDisable()
        {
            active = false;
        }

        internal void Set(string k, string v)
        {
            if (!active)
            {
                gameObject.SetActive(true);
            }

            key.text = k;
            value.text = v;
        }

        public void Hide()
        {
            if (active)
            {
                gameObject.SetActive(false);
            }
        }

        public void SetOrder(int order)
        {
            transform.SetSiblingIndex(order);
        }
    }
}
