using UnityEngine;

namespace Game
{
    public class PickupShrinkPop : MonoBehaviour
    {
        [SerializeField] float duration = 0.25f;
        [SerializeField] AnimationCurve scaleCurve = null;
        [SerializeField] AudioClip popClip;
        [SerializeField] float volume = 1f;
        [SerializeField] Vector2 pitchRange = new Vector2(1f, 1f);
        [SerializeField] bool spawnAudioObject = true;

        Vector3 initialScale;
        float t;
        bool active;

        void Awake()
        {
            if (scaleCurve == null) scaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        }

        public void PlayAndDestroy()
        {
            if (active) return;
            active = true;
            t = 0f;
            initialScale = transform.localScale;
            DisableColliders();
            PlayPop();
        }

        void Update()
        {
            if (!active) return;
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / Mathf.Max(0.0001f, duration));
            float s = Mathf.Clamp01(scaleCurve.Evaluate(u));
            transform.localScale = initialScale * s;
            if (u >= 1f)
            {
                Destroy(gameObject);
            }
        }

        void DisableColliders()
        {
            var cols3D = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < cols3D.Length; i++) cols3D[i].enabled = false;
            var cols2D = GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < cols2D.Length; i++) cols2D[i].enabled = false;
        }

        void PlayPop()
        {
            if (popClip == null) return;
            float p = (pitchRange.x == pitchRange.y) ? pitchRange.x : Random.Range(pitchRange.x, pitchRange.y);
            if (spawnAudioObject)
            {
                var go = new GameObject("PickupPopAudio");
                go.transform.position = transform.position;
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                src.pitch = p;
                src.volume = 1f;
                src.clip = popClip;
                src.PlayOneShot(popClip, Mathf.Clamp01(volume));
                Destroy(go, popClip.length + 0.1f);
            }
            else
            {
                var src = GetComponent<AudioSource>();
                if (src == null) src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                float originalPitch = src.pitch;
                src.pitch = p;
                src.PlayOneShot(popClip, Mathf.Clamp01(volume));
                src.pitch = originalPitch;
            }
        }
    }
}
