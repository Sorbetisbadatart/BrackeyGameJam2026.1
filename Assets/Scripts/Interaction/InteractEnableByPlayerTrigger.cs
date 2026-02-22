using UnityEngine;

namespace Game
{
    public class InteractEnableByPlayerTrigger : MonoBehaviour
    {
        [SerializeField] MonoBehaviour targetInteractableComponent;
        [SerializeField] string playerTag = "Player";
        [SerializeField] bool enableOnlyWhileInside = true;
        [SerializeField] bool interactOnEnter = false;

        IInteractable targetInterface;
        Interactable targetBase;

        void Awake()
        {
            ResolveTarget();
            if (!enableOnlyWhileInside)
            {
                SetEnabledState(false);
            }
        }

        void ResolveTarget()
        {
            if (targetInteractableComponent == null)
            {
                var all = GetComponents<MonoBehaviour>();
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i] is IInteractable)
                    {
                        targetInteractableComponent = all[i];
                        break;
                    }
                }
            }
            targetInterface = targetInteractableComponent as IInteractable;
            targetBase = targetInteractableComponent as Interactable;
        }

        void SetEnabledState(bool value)
        {
            if (targetBase != null)
            {
                targetBase.SetEnabled(value);
            }
            else if (targetInteractableComponent != null)
            {
                targetInteractableComponent.enabled = value;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            SetEnabledState(true);
            if (interactOnEnter && targetInterface != null)
            {
                targetInterface.Interact(other.gameObject);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            if (enableOnlyWhileInside)
            {
                SetEnabledState(false);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            SetEnabledState(true);
            if (interactOnEnter && targetInterface != null)
            {
                targetInterface.Interact(other.gameObject);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            if (enableOnlyWhileInside)
            {
                SetEnabledState(false);
            }
        }
    }
}
