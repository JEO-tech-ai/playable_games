using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ColorDrive
{
    /// <summary>
    /// Shown after level complete or game over.
    /// Handles star rating, coin reward, retry, next level, and ad triggers.
    /// </summary>
    public class ResultScreenUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject successPanel;
        [SerializeField] private GameObject failPanel;

        [Header("Success UI")]
        [SerializeField] private TextMeshProUGUI successScoreText;
        [SerializeField] private TextMeshProUGUI coinsEarnedText;
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite starFilledSprite;
        [SerializeField] private Sprite starEmptySprite;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button doubleCoinsButton; // watch ad for 2x

        [Header("Fail UI")]
        [SerializeField] private TextMeshProUGUI failScoreText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button continueWithAdButton; // watch ad to continue

        [Header("Common")]
        [SerializeField] private Button homeButton;
        [SerializeField] private Image brandEndCard;
        [SerializeField] private TextMeshProUGUI ctaText;
        [SerializeField] private Button ctaButton;

        private int _earnedCoins = 0;
        private bool _coinsDoubled = false;

        void Awake()
        {
            gameObject.SetActive(false);
        }

        public void ShowSuccess(int score, int coins, int stars)
        {
            gameObject.SetActive(true);
            successPanel?.SetActive(true);
            failPanel?.SetActive(false);

            _earnedCoins = coins;
            _coinsDoubled = false;

            if (successScoreText != null) successScoreText.text = score.ToString("N0");
            if (coinsEarnedText != null) coinsEarnedText.text = $"+{coins}";

            // Star display
            if (starImages != null)
                for (int i = 0; i < starImages.Length; i++)
                    if (starImages[i] != null)
                        starImages[i].sprite = i < stars ? starFilledSprite : starEmptySprite;

            // Animate in
            transform.localScale = Vector3.zero;
            TweenHelper.ScaleTo(gameObject, Vector3.one, 0.4f);;

            ApplyBrandEndCard();

            // Trigger interstitial logic
            AdManager.Instance?.OnLevelComplete();
        }

        public void ShowFail(int score)
        {
            gameObject.SetActive(true);
            successPanel?.SetActive(false);
            failPanel?.SetActive(true);

            if (failScoreText != null) failScoreText.text = score.ToString("N0");

            transform.localScale = Vector3.zero;
            TweenHelper.ScaleTo(gameObject, Vector3.one, 0.35f);
        }

        private void ApplyBrandEndCard()
        {
            var theme = ThemeManager.Instance?.CurrentTheme;
            if (theme == null) return;

            if (brandEndCard != null && theme.adEndCardSprite != null)
                brandEndCard.sprite = theme.adEndCardSprite;

            if (ctaText != null)
                ctaText.text = theme.ctaButtonText;
        }

        // --- Button Handlers ---

        public void OnNextLevelPressed()
        {
            gameObject.SetActive(false);
            LevelManager.Instance?.LoadNextLevel();
            GameManager.Instance?.StartGame(GameManager.Instance.CurrentMode);
        }

        public void OnRetryPressed()
        {
            gameObject.SetActive(false);
            GameManager.Instance?.RestartLevel();
        }

        public void OnDoubleCoinsPressed()
        {
            if (_coinsDoubled) return;
            AdManager.Instance?.ShowRewardedAd("DoubleCoins", success =>
            {
                if (success)
                {
                    _coinsDoubled = true;
                    GameManager.Instance?.AddCoins(_earnedCoins); // add again for 2x
                    if (coinsEarnedText != null) coinsEarnedText.text = $"+{_earnedCoins * 2}";
                    if (doubleCoinsButton != null) doubleCoinsButton.interactable = false;
                }
            });
        }

        public void OnContinueWithAdPressed()
        {
            AdManager.Instance?.ShowRewardedAd("ContinueGame", success =>
            {
                if (success)
                {
                    gameObject.SetActive(false);
                    // Grant extra moves
                    FindFirstObjectByType<BlockPuzzleController>()?.UseExtraMoves(5);
                    GameManager.Instance?.SetState(GameState.Playing);
                }
            });
        }

        public void OnHomePressed()
        {
            gameObject.SetActive(false);
            GameManager.Instance?.GoToMainMenu();
        }

        public void OnCTAPressed()
        {
            var theme = ThemeManager.Instance?.CurrentTheme;
            if (theme != null && !string.IsNullOrEmpty(theme.ctaUrl))
                Application.OpenURL(theme.ctaUrl);
        }
    }
}
