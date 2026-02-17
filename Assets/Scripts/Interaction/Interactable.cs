using UnityEngine;

namespace Game
{
    public abstract class Interactable : MonoBehaviour, IInteractable
    {
        [SerializeField] string prompt = "";
        [SerializeField] bool isEnabled = true;

        public string Prompt => prompt;

        public virtual bool CanInteract(GameObject interactor)
        {
            return isEnabled && interactor != null;
        }

        public abstract void Interact(GameObject interactor);

        public void SetEnabled(bool value)
        {
            isEnabled = value;
        }
    }
}
