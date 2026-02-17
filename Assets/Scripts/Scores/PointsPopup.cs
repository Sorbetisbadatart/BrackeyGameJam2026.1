using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    [RequireComponent(typeof(RectTransform))]
    public class PointsPopup : MonoBehaviour
    {
        [SerializeField] TMP_Text label;
        [SerializeField] float duration = 0.8f;
        [SerializeField] Vector2 moveOffset = new Vector2(0f, 60f);
        [SerializeField] AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0.9f, 1f, 1f);

        RectTransform rt;
        Vector2 startPos;
        float t;
        Color baseColor = Color.white;

        public void Setup(string text, Color color, float d)
        {
            if (label != null) label.text = text;
            baseColor = color;
            duration = d > 0 ? d : duration;
            t = 0f;
        }

        void Awake()
        {
            rt = GetComponent<RectTransform>();
            if (label == null) label = GetComponent<TMP_Text>();
            if (label != null) baseColor = label.color;
        }

        void OnEnable()
        {
            startPos = rt.anchoredPosition;
            t = 0f;
        }

        void Update()
        {
            if (label == null) return;
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / Mathf.Max(0.0001f, duration));
            Vector2 pos = Vector2.LerpUnclamped(startPos, startPos + moveOffset, u);
            rt.anchoredPosition = pos;
            float a = Mathf.Clamp01(alphaCurve.Evaluate(u));
            float s = Mathf.Max(0.01f, scaleCurve.Evaluate(u));
            label.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            rt.localScale = new Vector3(s, s, 1f);
            if (u >= 1f) Destroy(gameObject);
        }
    }
}
