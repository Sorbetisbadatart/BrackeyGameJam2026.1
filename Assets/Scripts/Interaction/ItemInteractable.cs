using UnityEngine;
using UISound;

namespace Game
{
    public class ItemInteractable : Interactable
    {
        [SerializeField] ItemId itemId = ItemId.Key;
        [SerializeField] bool showPopup = true;
        [SerializeField] Color popupColor = new Color(0.4f, 1f, 0.6f, 1f);
        [SerializeField] float popupDuration = 0.8f;
        [SerializeField] bool destroyOnPickup = true;
        [SerializeField] bool playSound = true;
        [SerializeField] InteractionSoundType soundType = InteractionSoundType.Coin;
        [SerializeField] UIInteractionSoundManager soundManager;

        public override void Interact(GameObject interactor)
        {
            if (FakeInventory.Instance != null) FakeInventory.Instance.Acquire(itemId);
            if (ItemToastManager.Instance != null)
            {
                ItemToastManager.Instance.Show("New item obtained! " + itemId);
            }
            if (playSound)
            {
                if (soundManager == null) soundManager = Object.FindFirstObjectByType<UIInteractionSoundManager>();
                if (soundManager != null) soundManager.PlaySound(soundType, this);
            }
            if (showPopup && PointsPopupSpawner.Instance != null)
            {
                PointsPopupSpawner.Instance.ShowPopup("+" + itemId, transform.position, popupColor, popupDuration);
            }
            if (destroyOnPickup)
            {
                var vfx = GetComponent<PickupShrinkPop>();
                if (vfx != null) vfx.PlayAndDestroy();
                else Object.Destroy(gameObject);
            }
        }
    }
}
