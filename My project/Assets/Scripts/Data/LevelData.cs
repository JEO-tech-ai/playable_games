using UnityEngine;

namespace ColorDrive
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "ColorDrive/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Basic Info")]
        public string levelName = "Level 1";
        public int levelIndex = 0;
        public PuzzleMode puzzleMode = PuzzleMode.BlockPuzzle;

        [Header("Block Puzzle Settings")]
        public int gridWidth = 8;
        public int gridHeight = 8;
        public int moveLimit = 30;
        public float timeLimit = 180f; // 0 = unlimited

        [Header("Water Sort Settings")]
        public int tubeCount = 6;
        public int layerDepth = 4;
        public int waterSortMoveLimit = 50;

        [Header("Color Palette")]
        public Color[] colorPalette = new Color[]
        {
            new Color(0.2f, 0.4f, 1f),   // Blue
            new Color(1f, 0.2f, 0.2f),   // Red
            new Color(0.2f, 0.9f, 0.2f), // Green
            new Color(1f, 0.8f, 0f),     // Yellow
            new Color(1f, 0.5f, 0f),     // Orange
            new Color(0.8f, 0.2f, 1f),   // Purple
        };

        [Header("Block Puzzle Layout")]
        [Tooltip("Optional: preset block layout as JSON. Leave empty for procedural generation.")]
        [TextArea(3, 6)]
        public string presetLayoutJson = "";

        [Header("Water Sort Layout")]
        [Tooltip("Tube color layers as CSV rows. Each row = one tube, values = color indices.")]
        [TextArea(3, 8)]
        public string tubeLayoutCsv = "";

        [Header("Rewards")]
        public int baseScoreReward = 100;
        public int coinReward = 50;
        public int starThreshold2 = 75;   // % of max score for 2 stars
        public int starThreshold3 = 90;   // % of max score for 3 stars

        [Header("Difficulty")]
        [Range(1, 10)]
        public int difficultyRating = 3;
    }
}
