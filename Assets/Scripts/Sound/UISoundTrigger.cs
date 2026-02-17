using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UISound
{
    public class UISoundTrigger : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        [Header("Types of Interaction")]
        [SerializeField] private InteractionSoundType soundType = InteractionSoundType.Unspecified;
        [SerializeField] private InteractionSoundOn playType = InteractionSoundOn.PointerDown;
       
        [Header("Manager Reference")]
        [SerializeField] private UIInteractionSoundManager soundManager;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (soundManager != null)
            {
                Debug.LogError("UIInteractionSoundManager has not been set. " + "Either search manually or click Reset while not in play mode.", this);
            }
        }

        private void Reset()
        {
            soundManager = FindFirstObjectByType<UIInteractionSoundManager>();     
        }
        
        public void PlaySound()
        {
            soundManager.PlaySound(soundType,this);
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (playType == InteractionSoundOn.PointerDown && soundManager != null)
            {
                soundManager.PlaySound(soundType, this);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (playType == InteractionSoundOn.PointerUp && soundManager != null)
            {
                soundManager.PlaySound(soundType, this);
            }
        }
    }
}

namespace UISound
{
    public enum InteractionSoundType
    {
        Unspecified,
        Click,
        Parchment,
        Coin,
    }

    public enum InteractionSoundOn
    {
        PointerUp, //on release mouse
        PointerDown,//on click
    }
}


