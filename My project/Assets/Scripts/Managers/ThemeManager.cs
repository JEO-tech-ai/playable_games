using UnityEngine;

namespace ColorDrive
{
    public class ThemeManager : MonoBehaviour
    {
        public static ThemeManager Instance { get; private set; }

        [SerializeField] private ThemeConfig defaultTheme;

        public ThemeConfig CurrentTheme { get; private set; }

        public event System.Action<ThemeConfig> OnThemeChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (defaultTheme != null)
                ApplyTheme(defaultTheme);
        }

        public void ApplyTheme(ThemeConfig theme)
        {
            if (theme == null) return;
            CurrentTheme = theme;
            OnThemeChanged?.Invoke(theme);
            Debug.Log($"[ThemeManager] Theme applied: {theme.brandName}");
        }

        public void LoadThemeByName(string brandName)
        {
            ThemeConfig loaded = Resources.Load<ThemeConfig>($"Themes/{brandName}/ThemeConfig");
            if (loaded != null)
                ApplyTheme(loaded);
            else
                Debug.LogWarning($"[ThemeManager] Theme not found: {brandName}");
        }

        public Color GetBlockColor(int colorIndex)
        {
            if (CurrentTheme == null || CurrentTheme.blockColors == null) return Color.white;
            return colorIndex < CurrentTheme.blockColors.Length
                ? CurrentTheme.blockColors[colorIndex]
                : Color.white;
        }

        public Color GetPrimaryColor() => CurrentTheme?.primaryColor ?? Color.black;
        public Color GetSecondaryColor() => CurrentTheme?.secondaryColor ?? Color.gray;
        public Color GetAccentColor() => CurrentTheme?.accentColor ?? Color.blue;
    }
}
