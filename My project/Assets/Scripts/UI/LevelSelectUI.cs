using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace ColorDrive
{
    public class LevelSelectUI : MonoBehaviour
    {
        [Header("Level Button")]
        [SerializeField] private GameObject levelButtonPrefab;
        [SerializeField] private Transform levelButtonContainer;

        [Header("Mode Selection")]
        [SerializeField] private Button blockModeButton;
        [SerializeField] private Button waterModeButton;
        [SerializeField] private Button hybridModeButton;

        [Header("Back")]
        [SerializeField] private Button backButton;

        private PuzzleMode _selectedMode = PuzzleMode.BlockPuzzle;
        private List<GameObject> _levelButtons = new List<GameObject>();

        void Start()
        {
            BuildLevelButtons();
        }

        public void OnBlockModeSelected()
        {
            _selectedMode = PuzzleMode.BlockPuzzle;
            BuildLevelButtons();
        }

        public void OnWaterModeSelected()
        {
            _selectedMode = PuzzleMode.WaterSort;
            BuildLevelButtons();
        }

        public void OnHybridModeSelected()
        {
            _selectedMode = PuzzleMode.Hybrid;
            BuildLevelButtons();
        }

        private void BuildLevelButtons()
        {
            foreach (var btn in _levelButtons)
                Destroy(btn);
            _levelButtons.Clear();

            if (LevelManager.Instance == null || levelButtonPrefab == null) return;

            int total = LevelManager.Instance.TotalLevels;
            int unlocked = LevelManager.Instance.UnlockedLevels;

            for (int i = 0; i < total; i++)
            {
                var btnGO = Instantiate(levelButtonPrefab, levelButtonContainer);
                _levelButtons.Add(btnGO);

                bool isUnlocked = i < unlocked;
                int levelIdx = i;

                // Level number
                var numText = btnGO.transform.Find("LevelNumber")?.GetComponent<TextMeshProUGUI>();
                if (numText != null) numText.text = (i + 1).ToString();

                // Stars
                int stars = LevelManager.Instance.GetLevelStars(i);
                for (int s = 1; s <= 3; s++)
                {
                    var starGO = btnGO.transform.Find($"Star{s}");
                    if (starGO != null) starGO.gameObject.SetActive(s <= stars);
                }

                // Lock indicator
                var lockIcon = btnGO.transform.Find("LockIcon");
                if (lockIcon != null) lockIcon.gameObject.SetActive(!isUnlocked);

                // Button click
                var button = btnGO.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = isUnlocked;
                    button.onClick.AddListener(() => OnLevelSelected(levelIdx));
                }
            }
        }

        private void OnLevelSelected(int index)
        {
            LevelManager.Instance?.LoadLevel(index);
            GameManager.Instance?.StartGame(_selectedMode);
            // Load game scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }

        public void OnBackPressed()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
