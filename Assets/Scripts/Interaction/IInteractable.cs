using UnityEngine;

namespace Game
{
    public interface IInteractable
    {
        bool CanInteract(GameObject interactor);
        void Interact(GameObject interactor);
        string Prompt { get; }
    }
}
