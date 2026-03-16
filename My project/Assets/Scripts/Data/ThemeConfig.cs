using UnityEngine;

namespace ColorDrive
{
    [CreateAssetMenu(fileName = "ThemeConfig", menuName = "ColorDrive/Theme Config")]
    public class ThemeConfig : ScriptableObject
    {
        [Header("Brand Identity")]
        public string brandName = "Generic";
        public Sprite logoSprite;
        public Sprite backgroundSprite;

        [Header("Brand Colors")]
        public Color primaryColor = new Color(0.15f, 0.15f, 0.15f);   // Mercedes dark
        public Color secondaryColor = new Color(0.8f, 0.8f, 0.8f);    // Silver
        public Color accentColor = new Color(0f, 0.6f, 1f);           // Highlight blue
        public Color uiTextColor = Color.white;
        public Color uiPanelColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        [Header("Block Colors (override default palette)")]
        public Color[] blockColors = new Color[]
        {
            new Color(0.2f, 0.4f, 1f),
            new Color(1f, 0.2f, 0.2f),
            new Color(0.2f, 0.8f, 0.2f),
            new Color(1f, 0.8f, 0f),
            new Color(1f, 0.5f, 0f),
            new Color(0.7f, 0.2f, 0.9f),
        };

        [Header("Materials & Visuals")]
        public Material blockMaterial;
        public Material tubeMaterial;
        public Color tubeGlassColor = new Color(0.9f, 0.9f, 1f, 0.4f);

        [Header("Audio")]
        public AudioClip bgmClip;
        public AudioClip blockPlaceSound;
        public AudioClip blockClearSound;
        public AudioClip liquidPourSound;
        public AudioClip levelCompleteSound;
        public AudioClip gameOverSound;

        [Header("Ad Brand")]
        [TextArea(1, 3)]
        public string adTagline = "Drive in Color.";
        public Sprite adEndCardSprite;
        public string ctaButtonText = "Learn More";
        public string ctaUrl = "https://www.mercedes-benz.com";
    }
}
