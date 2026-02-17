using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    [RequireComponent(typeof(TMP_Text))]
    public class ScoreDisplay : MonoBehaviour
    {
        [SerializeField] TMP_Text label;
        [SerializeField] bool showHighScore;

        void Awake()
        {
            if (label == null) label = GetComponent<TMP_Text>();
        }

        void OnEnable()
        {
            var scoremanager = ScoreManager.Instance ?? FindFirstObjectByType<ScoreManager>();
            if (scoremanager == null) return;
            if (showHighScore)
            {
                UpdateHighScore(scoremanager.HighScore);
                scoremanager.OnHighScoreChanged += UpdateHighScore;
            }
            else
            {
                UpdateCurrentScore(scoremanager.CurrentScore);
                scoremanager.OnScoreChanged += UpdateCurrentScore;
            }
        }

        void OnDisable()
        {
            var mgr = ScoreManager.Instance;

            if (mgr == null) return;

            if (showHighScore)
            {
                mgr.OnHighScoreChanged -= UpdateHighScore;
            }
            else
            {
                mgr.OnScoreChanged -= UpdateCurrentScore;
            }
        }

        void UpdateCurrentScore(int value)
        {
            if (label != null) label.text = value.ToString();
        }

        void UpdateHighScore(int value)
        {
            if (label != null) label.text = value.ToString();
        }
    }
}
