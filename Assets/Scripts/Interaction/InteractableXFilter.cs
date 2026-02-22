using UnityEngine;

namespace Game
{
    public enum XSide
    {
        Any,
        Left,
        Right
    }

    public class InteractableXFilter : MonoBehaviour, IInteractable
    {
        [SerializeField] MonoBehaviour targetInteractableComponent;
        [SerializeField] float maxDeltaX = 1.5f;
        [SerializeField] XSide sideConstraint = XSide.Any;
        [SerializeField] float xOffset = 0f;
        [SerializeField] bool showGizmo = true;
        [SerializeField] float gizmoHeight = 2f;
        [SerializeField] float gizmoDepth = 2f;
        [SerializeField] Color gizmoColor = new Color(0f, 1f, 1f, 0.35f);

        IInteractable target;

        public string Prompt => target != null ? target.Prompt : string.Empty;

        void Awake()
        {
            target = targetInteractableComponent as IInteractable;
        }

        public bool CanInteract(GameObject interactor)
        {
            if (target == null || interactor == null) return false;
            if (!target.CanInteract(interactor)) return false;
            float px = interactor.transform.position.x;
            float cx = transform.position.x + xOffset;
            float dx = px - cx;
            if (Mathf.Abs(dx) > maxDeltaX) return false;
            if (sideConstraint == XSide.Left && dx > 0f) return false;
            if (sideConstraint == XSide.Right && dx < 0f) return false;
            return true;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor)) return;
            if (target != null) target.Interact(interactor);
        }

        void OnDrawGizmosSelected()
        {
            if (!showGizmo) return;
            float cx = transform.position.x + xOffset;
            Vector3 center = new Vector3(cx, transform.position.y, transform.position.z);
            Vector3 size = new Vector3(maxDeltaX * 2f, gizmoHeight, gizmoDepth);
            Color c = gizmoColor;
            Gizmos.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(c.a));
            Gizmos.DrawWireCube(center, size);
            if (sideConstraint == XSide.Left || sideConstraint == XSide.Right)
            {
                Gizmos.color = new Color(c.r, c.g, c.b, 0.6f);
                float sx = sideConstraint == XSide.Left ? cx - maxDeltaX : cx + maxDeltaX;
                Vector3 sCenter = new Vector3(sx, center.y, center.z);
                Vector3 sSize = new Vector3(0.02f, gizmoHeight, gizmoDepth);
                Gizmos.DrawCube(sCenter, sSize);
            }
        }
    }
}
