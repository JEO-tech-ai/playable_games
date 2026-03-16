using UnityEngine;
using System.Collections.Generic;

namespace ColorDrive
{
    /// <summary>
    /// Represents a single colored block piece that can be placed on the grid.
    /// Pieces consist of one or more cells in a pattern.
    /// </summary>
    public class BlockPiece : MonoBehaviour
    {
        [Header("Block Properties")]
        public int colorIndex = 0;
        public List<Vector2Int> cells = new List<Vector2Int>(); // relative cell offsets
        public bool isPlaced = false;

        private SpriteRenderer[] _renderers;
        private Color _baseColor;
        private Color _highlightColor;

        void Awake()
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>();
        }

        public void Initialize(int colorIdx, List<Vector2Int> shape)
        {
            colorIndex = colorIdx;
            cells = new List<Vector2Int>(shape);

            Color c = ThemeManager.Instance != null
                ? ThemeManager.Instance.GetBlockColor(colorIdx)
                : Color.white;

            _baseColor = c;
            _highlightColor = new Color(
                Mathf.Min(c.r + 0.3f, 1f),
                Mathf.Min(c.g + 0.3f, 1f),
                Mathf.Min(c.b + 0.3f, 1f)
            );

            ApplyColor(_baseColor);
        }

        public void SetHighlight(bool highlight)
        {
            ApplyColor(highlight ? _highlightColor : _baseColor);
        }

        private void ApplyColor(Color color)
        {
            if (_renderers == null) return;
            foreach (var r in _renderers)
                r.color = color;
        }

        public void AnimatePlace()
        {
            transform.localScale = Vector3.one * 0.8f;
            TweenHelper.ScaleTo(gameObject, Vector3.one, 0.2f);
        }

        public void AnimateClear(System.Action onComplete)
        {
            TweenHelper.ScaleTo(gameObject, Vector3.zero, 0.25f, onComplete);
        }
    }
}
