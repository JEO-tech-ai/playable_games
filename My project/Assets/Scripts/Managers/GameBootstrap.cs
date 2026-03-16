using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorDrive
{
    /// <summary>
    /// Bootstrap entry point — initializes all persistent managers,
    /// loads default theme, and transitions to MainMenu.
    /// Place this on a GameObject in the Bootstrap scene (scene index 0).
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Manager Prefabs")]
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject levelManagerPrefab;
        [SerializeField] private GameObject themeManagerPrefab;
        [SerializeField] private GameObject adManagerPrefab;
        [SerializeField] private GameObject iapManagerPrefab;

        [Header("Default Theme")]
        [SerializeField] private ThemeConfig defaultTheme;

        [Header("Scene Names")]
        [SerializeField] private string mainMenuScene = "MainMenu";

        void Awake()
        {
            // Instantiate managers if not already present
            EnsureManager<GameManager>(gameManagerPrefab, "GameManager");
            EnsureManager<LevelManager>(levelManagerPrefab, "LevelManager");
            EnsureManager<ThemeManager>(themeManagerPrefab, "ThemeManager");
            EnsureManager<AdManager>(adManagerPrefab, "AdManager");
            EnsureManager<IAPManager>(iapManagerPrefab, "IAPManager");
        }

        void Start()
        {
            // Apply default theme
            if (defaultTheme != null)
                ThemeManager.Instance?.ApplyTheme(defaultTheme);

            // Load main menu
            SceneManager.LoadScene(mainMenuScene);
        }

        private void EnsureManager<T>(GameObject prefab, string name) where T : MonoBehaviour
        {
            if (FindFirstObjectByType<T>() != null) return;

            if (prefab != null)
                Instantiate(prefab);
            else
            {
                var go = new GameObject(name);
                go.AddComponent<T>();
            }
        }
    }
}
