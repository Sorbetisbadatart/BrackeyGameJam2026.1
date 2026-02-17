using UnityEngine;

namespace Game
{
    public class DebugPickup : Interactable
    {
        [SerializeField] int scoreAmount = 10;
        [SerializeField] bool enableDebugInput = true;
        [SerializeField] KeyCode pickupKey = KeyCode.E;
        [SerializeField] bool requirePlayerInRange = true;
        [SerializeField] float range = 3f;
        [SerializeField] string playerTag = "Player";
        [SerializeField] bool showPopup = true;
        [SerializeField] Color popupColor = new Color(1f, 0.9f, 0.2f, 1f);
        [SerializeField] float popupDuration = 0.8f;
        [SerializeField] bool destroyOnPickup = true;

        void Update()
        {
            if (!enableDebugInput) return;
            if (Input.GetKeyDown(pickupKey))
            {
                GameObject player = null;
                var go = GameObject.FindGameObjectWithTag(playerTag);
                if (go != null) player = go;
                if (requirePlayerInRange)
                {
                    if (player == null)
                    {
                        Debug.Log("DebugPickup: no player found for proximity check", this);
                        return;
                    }
                    var d = Vector3.Distance(transform.position, player.transform.position);
                    if (d > range)
                    {
                        Debug.Log("DebugPickup: player not in range, distance=" + d, this);
                        return;
                    }
                }
                Debug.Log("DebugPickup: manual pickup triggered by key " + pickupKey, this);
                Interact(player);
            }
        }

        public override void Interact(GameObject interactor)
        {
            var before = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
            if (ScoreManager.Instance != null) ScoreManager.Instance.Add(scoreAmount);
            if (showPopup && PointsPopupSpawner.Instance != null)
            {
                PointsPopupSpawner.Instance.ShowPopup("+" + scoreAmount, transform.position, popupColor, popupDuration);
            }
            var after = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : before + scoreAmount;
            Debug.Log("DebugPickup: picked up by " + (interactor != null ? interactor.name : "null") + " score +" + scoreAmount + " " + before + "->" + after, this);
            if (destroyOnPickup) Destroy(gameObject);
        }
    }
}
