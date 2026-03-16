using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorDrive
{
    /// <summary>
    /// In-game scene controller — wires together puzzle controllers, HUD, and result screen.
    /// Place this on a GameObject in the GameScene.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Block Puzzle")]
        [SerializeField] private BlockPuzzleController blockPuzzleController;

        [Header("Water Sort")]
        [SerializeField] private WaterSortController waterSortController;

        [Header("UI")]
        [SerializeField] private HUDController hud;
        [SerializeField] private ResultScreenUI resultScreen;
        [SerializeField] private PauseMenuUI pauseMenu;

        void Start()
        {
            if (GameManager.Instance == null || LevelManager.Instance == null)
            {
                Debug.LogError("[GameController] Managers not found! Load Bootstrap scene first.");
                SceneManager.LoadScene("Bootstrap");
                return;
            }

            SetupGame();
        }

        private void SetupGame()
        {
            var levelData = LevelManager.Instance.CurrentLevel;
            if (levelData == null)
            {
                Debug.LogWarning("[GameController] No level data — defaulting to level 0");
                LevelManager.Instance.LoadLevel(0);
                levelData = LevelManager.Instance.CurrentLevel;
            }

            hud?.SetLevelText(LevelManager.Instance.CurrentLevelIndex);

            PuzzleMode mode = GameManager.Instance.CurrentMode;

            // Activate correct controller
            bool useBlock = mode == PuzzleMode.BlockPuzzle || mode == PuzzleMode.Hybrid;
            bool useWater = mode == PuzzleMode.WaterSort || mode == PuzzleMode.Hybrid;

            if (blockPuzzleController != null)
            {
                blockPuzzleController.gameObject.SetActive(useBlock);
                if (useBlock && levelData != null)
                {
                    blockPuzzleController.StartLevel(levelData);
                    hud?.RegisterBlockController(blockPuzzleController);
                    blockPuzzleController.OnLevelComplete += HandleBlockComplete;
                    blockPuzzleController.OnGameOver += HandleGameOver;
                }
            }

            if (waterSortController != null)
            {
                waterSortController.gameObject.SetActive(useWater && !useBlock);
                if (useWater && !useBlock && levelData != null)
                {
                    waterSortController.StartLevel(levelData);
                    hud?.RegisterWaterController(waterSortController);
                    waterSortController.OnLevelComplete += HandleWaterComplete;
                    waterSortController.OnGameOver += HandleGameOver;
                }
            }
        }

        private void HandleBlockComplete()
        {
            var mode = GameManager.Instance?.CurrentMode;
            if (mode == PuzzleMode.Hybrid)
            {
                // Phase 2: Switch to Water Sort
                Debug.Log("[GameController] Hybrid: Block phase complete, starting Water Sort");
                var levelData = LevelManager.Instance.CurrentLevel;
                blockPuzzleController.gameObject.SetActive(false);
                waterSortController.gameObject.SetActive(true);
                waterSortController.StartLevel(levelData);
                hud?.RegisterWaterController(waterSortController);
                waterSortController.OnLevelComplete += HandleWaterComplete;
                waterSortController.OnGameOver += HandleGameOver;
            }
            else
            {
                ShowResult(true);
            }
        }

        private void HandleWaterComplete()
        {
            ShowResult(true);
        }

        private void HandleGameOver()
        {
            ShowResult(false);
        }

        private void ShowResult(bool success)
        {
            int score = GameManager.Instance?.CurrentScore ?? 0;

            if (success)
            {
                int stars = CalculateStars(score);
                int coins = LevelManager.Instance?.CurrentLevel?.coinReward ?? 50;
                LevelManager.Instance?.SaveLevelStars(LevelManager.Instance.CurrentLevelIndex, stars);
                resultScreen?.ShowSuccess(score, coins, stars);
            }
            else
            {
                resultScreen?.ShowFail(score);
            }
        }

        private int CalculateStars(int score)
        {
            var level = LevelManager.Instance?.CurrentLevel;
            if (level == null) return 1;

            float pct = (float)score / (level.baseScoreReward * 10f) * 100f;
            if (pct >= level.starThreshold3) return 3;
            if (pct >= level.starThreshold2) return 2;
            return 1;
        }
    }
}
