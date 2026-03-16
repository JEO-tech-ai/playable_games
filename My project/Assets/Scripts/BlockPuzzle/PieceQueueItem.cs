using UnityEngine;

namespace ColorDrive
{
    /// <summary>
    /// A piece shown in the queue below the grid.
    /// Clicking selects it for placement.
    /// </summary>
    public class PieceQueueItem : MonoBehaviour
    {
        private BlockPiece _piece;
        private BlockPuzzleController _controller;
        private SpriteRenderer _bg;
        private bool _isSelected = false;

        void Awake()
        {
            _bg = GetComponent<SpriteRenderer>();
        }

        public void Setup(BlockPiece piece, BlockPuzzleController controller)
        {
            _piece = piece;
            _controller = controller;
        }

        void OnMouseDown()
        {
            if (_controller == null || _piece == null) return;
            _controller.SelectPiece(_piece);
            SetSelected(true);
        }

        public void SetSelected(bool sel)
        {
            _isSelected = sel;
            if (_bg != null)
                _bg.color = sel
                    ? new Color(1f, 1f, 0.3f, 0.5f)
                    : new Color(1f, 1f, 1f, 0.1f);
        }
    }
}
