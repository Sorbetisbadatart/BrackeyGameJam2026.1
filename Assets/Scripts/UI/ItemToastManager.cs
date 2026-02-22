using TMPro;
using UnityEngine;

namespace Game
{
    public class ItemToastManager : MonoBehaviour
    {
        public static ItemToastManager Instance { get; private set; }

        [SerializeField] Canvas targetCanvas;
        [SerializeField] ItemToast toastPrefab;
        [SerializeField] string defaultText = "New item obtained!";
        [SerializeField] Vector2 spawnOffset = new Vector2(0f, 60f);

        int activeCount;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        public void Show(string text)
        {
            if (toastPrefab == null || targetCanvas == null) return;
            var t = Instantiate(toastPrefab, targetCanvas.transform);
            var rt = t.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(0f, -20f) - spawnOffset * activeCount;
            }
            t.Setup(string.IsNullOrEmpty(text) ? defaultText : text);
            activeCount++;
            t.gameObject.AddComponent<ToastCompletion>().Init(this);
        }

        class ToastCompletion : MonoBehaviour
        {
            ItemToastManager owner;
            public void Init(ItemToastManager o) { owner = o; }
            void OnDestroy()
            {
                if (owner != null) owner.activeCount = Mathf.Max(0, owner.activeCount - 1);
            }
        }
    }
}
