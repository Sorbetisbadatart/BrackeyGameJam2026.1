using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class PointsPopupSpawner : MonoBehaviour
    {
        public static PointsPopupSpawner Instance { get; private set; }

        [SerializeField] Canvas targetCanvas;
        [SerializeField] TMP_Text popupPrefab;
        [SerializeField] Color defaultColor = new Color(1f, 0.95f, 0.2f, 1f);
        [SerializeField] float defaultDuration = 0.8f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (targetCanvas == null) targetCanvas = FindFirstObjectByType<Canvas>();
        }

        public void ShowPopup(string text, Vector3 worldPosition)
        {
            ShowPopup(text, worldPosition, defaultColor, defaultDuration);
        }

        public void ShowPopup(string text, Vector3 worldPosition, Color color, float duration)
        {
            if (popupPrefab == null || targetCanvas == null) return;
            Camera cam = Camera.main;
            Vector2 screenPoint = cam != null ? (Vector2)cam.WorldToScreenPoint(worldPosition) : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            ShowPopupAtScreenPoint(text, screenPoint, color, duration);
        }

        public void ShowPopupAtScreenPoint(string text, Vector2 screenPoint)
        {
            ShowPopupAtScreenPoint(text, screenPoint, defaultColor, defaultDuration);
        }

        public void ShowPopupAtScreenPoint(string text, Vector2 screenPoint, Color color, float duration)
        {
            if (popupPrefab == null || targetCanvas == null) return;
            RectTransform canvasRect = targetCanvas.transform as RectTransform;
            Camera cam = targetCanvas.renderMode == RenderMode.ScreenSpaceCamera ? targetCanvas.worldCamera : null;
            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cam, out localPoint)) return;
            TMP_Text instance = Instantiate(popupPrefab, targetCanvas.transform);
            RectTransform rt = instance.GetComponent<RectTransform>();
            rt.anchoredPosition = localPoint;
            PointsPopup popup = instance.GetComponent<PointsPopup>();
            if (popup == null) popup = instance.gameObject.AddComponent<PointsPopup>();
            instance.text = text;
            instance.color = color;
            popup.Setup(text, color, duration);
        }
    }
}
