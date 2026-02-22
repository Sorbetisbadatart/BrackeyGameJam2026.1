using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class ItemsPageUI : MonoBehaviour
    {
        [Serializable]
        public struct ItemIcon
        {
            public ItemId id;
            public Image image;
        }

        [Header("Icons")]
        [SerializeField] ItemIcon[] icons = new ItemIcon[5];

        [Header("Alpha States")]
        [SerializeField] float ownedAlpha = 1f;
        [SerializeField] float missingAlpha = 0.5f;

        [Header("Updates")]
        [SerializeField] bool refreshOnEnable = true;
        [SerializeField] bool periodicRefresh = false;
        [SerializeField] float refreshInterval = 0.5f;

        float timer;

        void OnEnable()
        {
            if (refreshOnEnable) RefreshAll();
        }

        void Update()
        {
            if (!periodicRefresh) return;
            timer += Time.unscaledDeltaTime;
            if (timer >= Mathf.Max(0.05f, refreshInterval))
            {
                timer = 0f;
                RefreshAll();
            }
        }

        public void RefreshAll()
        {
            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i].image == null) continue;
                bool has = FakeInventory.Instance != null && FakeInventory.Instance.Has(icons[i].id);
                SetAlpha(icons[i].image, has ? ownedAlpha : missingAlpha);
            }
        }

        public void OnInventoryChanged(ItemId id, bool has)
        {
            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i].id != id) continue;
                if (icons[i].image == null) continue;
                SetAlpha(icons[i].image, has ? ownedAlpha : missingAlpha);
                break;
            }
        }

        static void SetAlpha(Image img, float a)
        {
            var c = img.color;
            c.a = Mathf.Clamp01(a);
            img.color = c;
        }
    }
}
