using UnityEngine;

namespace Game
{
    public class PanelToggle : MonoBehaviour
    {
        [SerializeField] GameObject target;
        [SerializeField] bool startHidden = false;
        [SerializeField] bool enableInput = false;
        [SerializeField] KeyCode toggleKey = KeyCode.Tab;

        void Awake()
        {
            if (target == null) target = gameObject;
            Set(!startHidden);
        }

        void Update()
        {
            if (!enableInput) return;
            if (Input.GetKeyDown(toggleKey)) Toggle();
        }

        public void Show() => Set(true);
        public void Hide() => Set(false);
        public void Toggle() => Set(!(target != null && target.activeSelf));

        public void Set(bool visible)
        {
            if (target != null) target.SetActive(visible);
        }
    }
}
