using System;
using UnityEngine;

namespace Game
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [SerializeField] string currentScoreKey = "CurrentScore";
        [SerializeField] string highScoreKey = "HighScore";
        [SerializeField] bool autoSaveOnChange = true;
        [SerializeField] bool persistBetweenScenes = true;

        int currentScore;
        int highScore;

        public int CurrentScore => currentScore;
        public int HighScore => highScore;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnHighScoreChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (persistBetweenScenes) DontDestroyOnLoad(gameObject);
           
            Load();

            //events
            OnScoreChanged?.Invoke(currentScore);
            OnHighScoreChanged?.Invoke(highScore);
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            Set(currentScore + amount);
        }

        public void Remove(int amount)
        {
            if (amount <= 0) return;
            Set(Mathf.Max(0, currentScore - amount));
        }

        public void Set(int value)
        {
            value = Mathf.Max(0, value);
            if (currentScore == value) return;
            currentScore = value;
            if (currentScore > highScore)
            {
                highScore = currentScore;
                if (autoSaveOnChange)
                {
                    PlayerPrefs.SetInt(highScoreKey, highScore);
                    PlayerPrefs.Save();
                }
                OnHighScoreChanged?.Invoke(highScore);
            }
            if (autoSaveOnChange)
            {
                PlayerPrefs.SetInt(currentScoreKey, currentScore);
                PlayerPrefs.Save();
            }
            OnScoreChanged?.Invoke(currentScore);
        }

        public void ResetScore()
        {
            Set(0);
        }

        public void Save()
        {
            PlayerPrefs.SetInt(currentScoreKey, currentScore);
            PlayerPrefs.SetInt(highScoreKey, highScore);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            currentScore = PlayerPrefs.HasKey(currentScoreKey) ? PlayerPrefs.GetInt(currentScoreKey) : 0;
            highScore = PlayerPrefs.HasKey(highScoreKey) ? PlayerPrefs.GetInt(highScoreKey) : currentScore;
            if (currentScore > highScore)
            {
                highScore = currentScore;
            }
        }

        public void ClearSaves()
        {
            PlayerPrefs.DeleteKey(currentScoreKey);
            PlayerPrefs.DeleteKey(highScoreKey);
            PlayerPrefs.Save();
            currentScore = 0;
            highScore = 0;
            OnScoreChanged?.Invoke(currentScore);
            OnHighScoreChanged?.Invoke(highScore);
        }
    }
}
