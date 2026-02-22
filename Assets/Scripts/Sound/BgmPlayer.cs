using UnityEngine;
using System.Collections;

namespace Game
{
    public class BgmPlayer : MonoBehaviour
    {
        public static BgmPlayer Instance { get; private set; }

        [SerializeField] AudioClip bgm;
        [SerializeField] float volume = 0.6f;
        [SerializeField] bool playOnStart = true;
        [SerializeField] bool loop = true;
        [SerializeField] bool persistBetweenScenes = true;
        [SerializeField] float fadeInSeconds = 0f;
        [SerializeField] float fadeOutSeconds = 0f;
        [SerializeField] bool useUnscaledTime = false;

        AudioSource source;
        Coroutine fadeRoutine;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (persistBetweenScenes) DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            source = GetComponent<AudioSource>();
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            source.clip = bgm;
            source.volume = fadeInSeconds > 0f ? 0f : Mathf.Clamp01(volume);
        }

        void Start()
        {
            if (playOnStart && bgm != null)
            {
                Play(bgm, fadeInSeconds);
            }
        }

        public void Play(AudioClip clip, float fadeIn = 0f)
        {
            if (clip == null) return;
            bgm = clip;
            source.loop = loop;
            source.clip = clip;
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            source.volume = fadeIn > 0f ? 0f : Mathf.Clamp01(volume);
            source.Play();
            if (fadeIn > 0f) fadeRoutine = StartCoroutine(FadeTo(Mathf.Clamp01(volume), fadeIn, false));
        }

        public void Stop()
        {
            if (!source.isPlaying) return;
            if (fadeOutSeconds > 0f)
            {
                if (fadeRoutine != null) StopCoroutine(fadeRoutine);
                fadeRoutine = StartCoroutine(FadeTo(0f, fadeOutSeconds, true));
            }
            else
            {
                source.Stop();
            }
        }

        public void SetVolume(float v)
        {
            volume = Mathf.Clamp01(v);
            source.volume = volume;
        }

        IEnumerator FadeTo(float target, float duration, bool stopWhenDone)
        {
            float start = source.volume;
            float t = 0f;
            while (t < duration)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float u = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;
                source.volume = Mathf.Lerp(start, target, u);
                yield return null;
            }
            source.volume = target;
            fadeRoutine = null;
            if (stopWhenDone) source.Stop();
        }
    }
}
