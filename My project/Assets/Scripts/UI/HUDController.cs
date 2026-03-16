using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ColorDrive
{
    /// <summary>
    /// HUD overlay — shows score, timer, moves, coins, and booster buttons.
    /// Subscribes to GameManager events.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Score & Level")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI coinsText;

        [Header("Timer")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image timerFill;

        [Header("Moves")]
        [SerializeField] private TextMeshProUGUI movesText;

        [Header("Boosters")]
        [SerializeField] private Button hintButton;
        [SerializeField] private TextMeshProUGUI hintCountText;
        [SerializeField] private Button extraMovesButton;
        [SerializeField] private Button undoButton;
        [SerializeField] private Button pauseButton;

        [Header("Brand")]
        [SerializeField] private Image brandLogo;
        [SerializeField] private Image headerBackground;

        private float _maxTime = 180f;
        private BlockPuzzleController _blockController;
        private WaterSortController _waterController;

        void Start()
        {
            SubscribeToEvents();
            ApplyTheme();
            UpdateCoinDisplay(GameManager.Instance?.TotalCoins ?? 0);
        }

        void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += UpdateScore;
                GameManager.Instance.OnCoinsChanged += UpdateCoinDisplay;
            }
            if (ThemeManager.Instance != null)
                ThemeManager.Instance.OnThemeChanged += _ => ApplyTheme();
        }

        private void UnsubscribeFromEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScore;
                GameManager.Instance.OnCoinsChanged -= UpdateCoinDisplay;
            }
        }

        public void RegisterBlockController(BlockPuzzleController ctrl)
        {
            _blockController = ctrl;
            _maxTime = LevelManager.Instance?.CurrentLevel?.timeLimit ?? 180f;
            ctrl.OnMovesChanged += UpdateMovesDisplay;
            ctrl.OnTimeChanged += UpdateTimerDisplay;
        }

        public void RegisterWaterController(WaterSortController ctrl)
        {
            _waterController = ctrl;
            ctrl.OnMovesChanged += UpdateMovesDisplay;
            if (timerText != null) timerText.gameObject.SetActive(false);
            if (timerFill != null) timerFill.gameObject.SetActive(false);
        }

        void Update()
        {
            UpdateHintButton();
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = score.ToString("N0");
        }

        private void UpdateCoinDisplay(int coins)
        {
            if (coinsText != null) coinsText.text = $"+{FormatCoins(coins)}";
        }

        private string FormatCoins(int coins)
        {
            if (coins >= 1000) return $"{coins / 1000f:0.0}k";
            return coins.ToString();
        }

        private void UpdateTimerDisplay(float time)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(time / 60);
                int seconds = Mathf.FloorToInt(time % 60);
                timerText.text = $"{minutes:00}:{seconds:00}";

                // Red flash warning under 30 seconds
                timerText.color = time < 30f ? Color.red : Color.white;
            }
            if (timerFill != null && _maxTime > 0)
                timerFill.fillAmount = time / _maxTime;
        }

        private void UpdateMovesDisplay(int moves)
        {
            if (movesText != null) movesText.text = moves.ToString();
        }

        private void UpdateHintButton()
        {
            if (hintCountText != null)
                hintCountText.text = IAPManager.Instance?.GetHints().ToString() ?? "0";
        }

        private void ApplyTheme()
        {
            var theme = ThemeManager.Instance?.CurrentTheme;
            if (theme == null) return;

            if (headerBackground != null)
                headerBackground.color = theme.primaryColor;
            if (brandLogo != null && theme.logoSprite != null)
                brandLogo.sprite = theme.logoSprite;
        }

        // --- Button Handlers ---

        public void OnHintPressed()
        {
            int hints = IAPManager.Instance?.GetHints() ?? 0;
            if (hints <= 0)
            {
                // Offer rewarded ad for free hint
                AdManager.Instance?.ShowRewardedAd("Hint", success =>
                {
                    if (success) ShowHint();
                });
                return;
            }
            IAPManager.Instance?.UseHint();
            ShowHint();
        }

        private void ShowHint()
        {
            if (_blockController != null)
            {
                var (piece, dir, slot) = _blockController.GetHint();
                if (piece != null) piece.SetHighlight(true);
            }
            else if (_waterController != null)
            {
                var (from, to) = _waterController.GetHint();
                Debug.Log($"[HUD] Hint: pour tube {from} into tube {to}");
            }
        }

        public void OnExtraMovesPressed()
        {
            AdManager.Instance?.ShowRewardedAd("ExtraMoves", success =>
            {
                if (success)
                {
                    _blockController?.UseExtraMoves(5);
                    _waterController?.GetType(); // WaterSort doesn't have move limit in this context
                }
            });
        }

        public void OnUndoPressed()
        {
            _waterController?.Undo();
        }

        public void OnPausePressed()
        {
            GameManager.Instance?.PauseGame();
        }

        public void SetLevelText(int levelIndex)
        {
            if (levelText != null) levelText.text = $"LEVEL {levelIndex + 1}";
        }
    }
}
