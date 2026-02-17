using UnityEngine;

namespace Game
{
    public class DebugScoreControls : MonoBehaviour
    {
        [SerializeField] int addAmount = 10;
        [SerializeField] int removeAmount = 5;
        [SerializeField] bool spawnPopup = true;
        [SerializeField] bool showOverlay = true;
        [SerializeField] Color addColor = new Color(1f, 0.95f, 0.2f, 1f);
        [SerializeField] Color removeColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] float popupDuration = 0.8f;
        [SerializeField] Vector2 overlayPosition = new Vector2(10f, 10f);

        void Update()
        {
            var s = ScoreManager.Instance;
            if (s == null) return;
            if (Input.GetKeyDown(KeyCode.P))
            {
                s.Add(addAmount);
                if (spawnPopup && PointsPopupSpawner.Instance != null)
                    PointsPopupSpawner.Instance.ShowPopup("+" + addAmount, transform.position, addColor, popupDuration);
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                s.Remove(removeAmount);
                if (spawnPopup && PointsPopupSpawner.Instance != null)
                    PointsPopupSpawner.Instance.ShowPopup("-" + removeAmount, transform.position, removeColor, popupDuration);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                s.ResetScore();
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                s.Save();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                s.Load();
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                s.ClearSaves();
            }
        }

        void OnGUI()
        {
            if (!showOverlay) return;
            var s = ScoreManager.Instance;
            if (s == null) return;
            string text = "Score: " + s.CurrentScore + "  High: " + s.HighScore +
                          "\nP:+ " + addAmount + "  O:- " + removeAmount +
                          "  R:Reset  K:Save  L:Load  C:Clear";
            GUI.Label(new Rect(overlayPosition.x, overlayPosition.y, 820f, 200f), text,GUIStyle.none);
        }
    }
}
