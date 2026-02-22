using UnityEngine;

namespace Game
{
    public enum PlaneNormalMode
    {
        InteractorForward,
        ReferenceTransformForward,
        CameraForward
    }

    public class InteractablePlaneFilter : MonoBehaviour, IInteractable
    {
        [SerializeField] MonoBehaviour targetInteractableComponent;
        [SerializeField] PlaneNormalMode normalMode = PlaneNormalMode.InteractorForward;
        [SerializeField] Transform referenceTransform;
        [SerializeField] float thickness = 0.2f;
        [SerializeField] float planeOffset = 0f;
        [SerializeField] bool showGizmo = true;
        [SerializeField] Vector2 planeSize = new Vector2(3f, 3f);
        [SerializeField] Color gizmoColor = new Color(1f, 0f, 1f, 0.25f);

        IInteractable target;

        public string Prompt => target != null ? target.Prompt : string.Empty;

        void Awake()
        {
            target = targetInteractableComponent as IInteractable;
        }

        Vector3 GetNormal(GameObject interactor)
        {
            if (normalMode == PlaneNormalMode.InteractorForward && interactor != null)
                return interactor.transform.forward.normalized;
            if (normalMode == PlaneNormalMode.ReferenceTransformForward && referenceTransform != null)
                return referenceTransform.forward.normalized;
            if (normalMode == PlaneNormalMode.CameraForward)
            {
                var cam = Camera.main;
                if (cam != null) return cam.transform.forward.normalized;
            }
            return Vector3.forward;
        }

        Vector3 GetOrigin(GameObject interactor, Vector3 normal)
        {
            var origin = interactor != null ? interactor.transform.position : transform.position;
            return origin + normal * planeOffset;
        }

        public bool CanInteract(GameObject interactor)
        {
            if (target == null || interactor == null) return false;
            if (!target.CanInteract(interactor)) return false;
            var n = GetNormal(interactor);
            var o = GetOrigin(interactor, n);
            var d = Vector3.Dot(n, transform.position - o);
            return Mathf.Abs(d) <= Mathf.Max(0.0001f, thickness) * 0.5f;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor)) return;
            if (target != null) target.Interact(interactor);
        }

        void OnDrawGizmosSelected()
        {
            if (!showGizmo) return;
            var n = referenceTransform != null ? referenceTransform.forward : Vector3.forward;
            var rot = Quaternion.LookRotation(n, Vector3.up);
            var size = new Vector3(Mathf.Max(0.01f, planeSize.x), Mathf.Max(0.01f, planeSize.y), Mathf.Max(0.01f, thickness));
            var prev = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, rot, Vector3.one);
            var c = gizmoColor;
            Gizmos.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(c.a));
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.matrix = prev;
        }
    }
}
