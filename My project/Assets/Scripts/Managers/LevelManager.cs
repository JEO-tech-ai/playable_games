using UnityEngine;

namespace ColorDrive
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [SerializeField] private LevelData[] allLevels;

        public int CurrentLevelIndex { get; private set; }
        public LevelData CurrentLevel => allLevels != null && CurrentLevelIndex < allLevels.Length
            ? allLevels[CurrentLevelIndex] : null;

        public int TotalLevels => allLevels?.Length ?? 0;
        public int UnlockedLevels => PlayerPrefs.GetInt("UnlockedLevels", 1);

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

        public void LoadLevel(int index)
        {
            if (index < 0 || allLevels == null || index >= allLevels.Length)
            {
                Debug.LogWarning($"[LevelManager] Invalid level index: {index}");
                return;
            }
            CurrentLevelIndex = index;
            Debug.Log($"[LevelManager] Loading level {index}: {allLevels[index].levelName}");
        }

        public void UnlockNextLevel()
        {
            int nextUnlock = CurrentLevelIndex + 2; // +2 because index is 0-based, unlocked is 1-based
            if (nextUnlock > UnlockedLevels)
                PlayerPrefs.SetInt("UnlockedLevels", nextUnlock);
        }

        public bool IsLevelUnlocked(int index) => index < UnlockedLevels;

        public void LoadNextLevel()
        {
            int next = CurrentLevelIndex + 1;
            if (next < TotalLevels)
                LoadLevel(next);
            else
                Debug.Log("[LevelManager] No more levels — all levels complete!");
        }

        public int GetLevelStars(int index) =>
            PlayerPrefs.GetInt($"Stars_Level_{index}", 0);

        public void SaveLevelStars(int index, int stars)
        {
            int current = GetLevelStars(index);
            if (stars > current)
                PlayerPrefs.SetInt($"Stars_Level_{index}", stars);
        }
    }
}
