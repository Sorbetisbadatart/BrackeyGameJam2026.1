using UnityEngine;
using Game;

namespace UISound
{
    [DisallowMultipleComponent]
    public class InteractableSoundTrigger : MonoBehaviour, IInteractable
    {
        [Header("Types of Interaction")]
        [SerializeField] private InteractionSoundType soundType = InteractionSoundType.Unspecified;

        [Header("Manager Reference")]
        [SerializeField] private UIInteractionSoundManager soundManager;

        [Header("Forward To")]
        [SerializeField] private MonoBehaviour targetInteractableComponent;

        IInteractable target;

        public string Prompt => target != null ? target.Prompt : string.Empty;

        void Start()
        {
            if (soundManager == null)
            {
                Debug.LogError("UIInteractionSoundManager has not been set. Either search manually or click Reset while not in play mode.", this);
            }
        }

        void Reset()
        {
            soundManager = FindFirstObjectByType<UIInteractionSoundManager>();
            target = targetInteractableComponent as IInteractable;
        }

        void Awake()
        {
            target = targetInteractableComponent as IInteractable;
        }

        public void PlaySound()
        {
            if (soundManager != null)
            {
                soundManager.PlaySound(soundType, this);
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            return target != null && target.CanInteract(interactor);
        }

        public void Interact(GameObject interactor)
        {
            PlaySound();
            if (target != null && target != (IInteractable)this)
            {
                target.Interact(interactor);
            }
        }
    }
}
