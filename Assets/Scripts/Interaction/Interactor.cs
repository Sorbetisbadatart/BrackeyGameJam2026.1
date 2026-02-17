using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField] KeyCode interactKey = KeyCode.E;
        [SerializeField] bool autoInteract = false;
        readonly HashSet<IInteractable> candidates = new HashSet<IInteractable>();

        void Update()
        {
            if (autoInteract && candidates.Count > 0)
            {
                foreach (var c in candidates)
                {
                    if (c != null && c.CanInteract(gameObject))
                    {
                        c.Interact(gameObject);
                        break;
                    }
                }
            }
            else if (Input.GetKeyDown(interactKey) && candidates.Count > 0)
            {
                foreach (var c in candidates)
                {
                    if (c != null && c.CanInteract(gameObject))
                    {
                        c.Interact(gameObject);
                        break;
                    }
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            var i = other.GetComponent<IInteractable>();
            if (i != null) candidates.Add(i);
        }

        void OnTriggerExit(Collider other)
        {
            var i = other.GetComponent<IInteractable>();
            if (i != null) candidates.Remove(i);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var i = other.GetComponent<IInteractable>();
            if (i != null) candidates.Add(i);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            var i = other.GetComponent<IInteractable>();
            if (i != null) candidates.Remove(i);
        }
    }
}
