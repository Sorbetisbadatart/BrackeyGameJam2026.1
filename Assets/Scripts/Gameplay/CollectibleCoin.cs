using UnityEngine;

namespace Game
{
    public class CollectibleCoin : MonoBehaviour
    {
        [SerializeField] int amount = 1;
        [SerializeField] string playerTag = "Player";
        [SerializeField] bool showPopup = true;
        [SerializeField] Color popupColor = new Color(1f, 0.9f, 0.2f, 1f);
        [SerializeField] float popupDuration = 0.8f;

        void Collect()
        {
            if (ScoreManager.Instance != null) ScoreManager.Instance.Add(amount);
            if (showPopup && PointsPopupSpawner.Instance != null)
            {
                PointsPopupSpawner.Instance.ShowPopup("+" + amount, transform.position, popupColor, popupDuration);
            }
            var vfx = GetComponent<Game.PickupShrinkPop>();
            if (vfx != null)
            {
                vfx.PlayAndDestroy();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            Collect();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            Collect();
        }
    }
}
