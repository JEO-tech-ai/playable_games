using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorDrive
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        LevelComplete,
        GameOver
    }

    public enum PuzzleMode
    {
        BlockPuzzle,
        WaterSort,
        Hybrid
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        public GameState CurrentState { get; private set; }
        public PuzzleMode CurrentMode { get; private set; }

        [Header("Score")]
        public int CurrentScore { get; private set; }
        public int HighScore => PlayerPrefs.GetInt($"HighScore_Level_{LevelManager.Instance?.CurrentLevelIndex}", 0);
        public int TotalCoins => PlayerPrefs.GetInt("TotalCoins", 0);

        public event System.Action<GameState> OnStateChanged;
        public event System.Action<int> OnScoreChanged;
        public event System.Action<int> OnCoinsChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            SetState(GameState.MainMenu);
        }

        public void SetState(GameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);

            switch (newState)
            {
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.LevelComplete:
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    break;
            }
        }

        public void StartGame(PuzzleMode mode)
        {
            CurrentMode = mode;
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore);
            SetState(GameState.Playing);
        }

        public void AddScore(int points)
        {
            CurrentScore += points;
            OnScoreChanged?.Invoke(CurrentScore);

            int currentBest = HighScore;
            if (CurrentScore > currentBest)
                PlayerPrefs.SetInt($"HighScore_Level_{LevelManager.Instance?.CurrentLevelIndex}", CurrentScore);
        }

        public void AddCoins(int amount)
        {
            int coins = TotalCoins + amount;
            PlayerPrefs.SetInt("TotalCoins", coins);
            OnCoinsChanged?.Invoke(coins);
        }

        public bool SpendCoins(int amount)
        {
            int current = TotalCoins;
            if (current < amount) return false;
            int newTotal = current - amount;
            PlayerPrefs.SetInt("TotalCoins", newTotal);
            PlayerPrefs.Save();
            OnCoinsChanged?.Invoke(newTotal);
            return true;
        }

        public void PauseGame() => SetState(GameState.Paused);

        public void ResumeGame() => SetState(GameState.Playing);

        public void CompleteLevel()
        {
            SetState(GameState.LevelComplete);
            LevelManager.Instance?.UnlockNextLevel();
            // Reward coins based on performance
            int bonus = Mathf.Max(100, CurrentScore / 10);
            AddCoins(bonus);
        }

        public void TriggerGameOver() => SetState(GameState.GameOver);

        public void RestartLevel()
        {
            SetState(GameState.Playing);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SetState(GameState.MainMenu);
            SceneManager.LoadScene("MainMenu");
        }
    }
}
