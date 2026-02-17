using System;
using System.Collections.Generic;
using UnityEngine;

namespace UISound
{
    public class UIInteractionSoundManager : MonoBehaviour
    {
        [Serializable]
        public class SoundFeedback
        {
            public InteractionSoundType soundType;
            public AudioClip soundClip;
        }

        [Header("Feedback List")]
        public List<SoundFeedback> soundFeedback = new();

        [Header("Audio Setup")]
        public AudioSource audioSource;

        //sfx dictionary
        private Dictionary<InteractionSoundType, AudioClip> _soundFeedbacksDictionary = new Dictionary<InteractionSoundType, AudioClip>();

        private void Reset()
        {
            audioSource = GetComponent<AudioSource>();

            foreach (InteractionSoundType soundType in Enum.GetValues(typeof(InteractionSoundType)))
            {
                soundFeedback.Add(new SoundFeedback { soundType = soundType, soundClip = null });
            }
        }

        private void Start()
        {
            foreach (var entry in soundFeedback)
            {
                _soundFeedbacksDictionary.Add(entry.soundType, entry.soundClip);
            }
        }


        public void PlaySound(InteractionSoundType soundType, UnityEngine.Object senderObject)
        {
            if (!_soundFeedbacksDictionary.TryGetValue(soundType, out var soundClip))
            {
                Debug.LogWarning($"Sound for {soundType} not found!", senderObject);
                return;
            }

            if (soundClip == null)
            {
                Debug.LogWarning($"Soundclip for {soundType} is null!", senderObject);
            }

            if (soundType == InteractionSoundType.Unspecified)
            {
                Debug.Log($"{senderObject} plays an unspecified sound.", senderObject);
            }

            if (audioSource != null)
            {
                audioSource.PlayOneShot(soundClip);
            }
        }

       
    }
}