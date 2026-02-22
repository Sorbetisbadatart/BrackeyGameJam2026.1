using TMPro;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(RectTransform))]
    public class ItemToast : MonoBehaviour
    {
        [SerializeField] TMP_Text label;
        [SerializeField] float slideInTime = 0.25f;
        [SerializeField] float holdTime = 1.2f;
        [SerializeField] float slideOutTime = 0.25f;
        [SerializeField] Vector2 offscreenPos = new Vector2(500f, -100f);
        [SerializeField] Vector2 onscreenPos = new Vector2(-20f, -100f);

        RectTransform rt;
        float t;
        int phase;

        public void Setup(string text)
        {
            if (label != null) label.text = text;
            t = 0f;
            phase = 0;
        }

        void Awake()
        {
            rt = GetComponent<RectTransform>();
            if (label == null) label = GetComponent<TMP_Text>();
            t = 0f;
            phase = 0;
            if (rt != null) rt.anchoredPosition = offscreenPos;
        }

        void Update()
        {
            t += Time.unscaledDeltaTime;
            if (phase == 0)
            {
                float u = slideInTime > 0 ? Mathf.Clamp01(t / slideInTime) : 1f;
                if (rt != null) rt.anchoredPosition = Vector2.LerpUnclamped(offscreenPos, onscreenPos, u);
                if (u >= 1f) { phase = 1; t = 0f; }
            }
            else if (phase == 1)
            {
                if (t >= holdTime) { phase = 2; t = 0f; }
            }
            else
            {
                float u = slideOutTime > 0 ? Mathf.Clamp01(t / slideOutTime) : 1f;
                if (rt != null) rt.anchoredPosition = Vector2.LerpUnclamped(onscreenPos, offscreenPos, u);
                if (u >= 1f) Destroy(gameObject);
            }
        }
    }
}
