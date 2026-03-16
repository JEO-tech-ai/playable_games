using UnityEngine;

namespace ColorDrive
{
    /// <summary>
    /// Invisible trigger zone at the edge of the grid.
    /// When clicked, tells BlockPuzzleController to place the selected piece.
    /// </summary>
    public class SlotButton : MonoBehaviour
    {
        public SlotDirection direction;
        public int slotIndex;

        private BlockPuzzleController _controller;
        private SpriteRenderer _sr;
        private Color _normalColor;
        private Color _hoverColor;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null)
            {
                _normalColor = new Color(1f, 1f, 1f, 0.15f);
                _hoverColor = new Color(1f, 1f, 0f, 0.4f);
                _sr.color = _normalColor;
            }
        }

        public void Setup(BlockPuzzleController controller, SlotDirection dir, int idx)
        {
            _controller = controller;
            direction = dir;
            slotIndex = idx;
        }

        void OnMouseDown()
        {
            if (_controller == null) return;
            _controller.PlacePieceAtSlot(direction, slotIndex);
        }

        void OnMouseEnter()
        {
            if (_sr != null) _sr.color = _hoverColor;
        }

        void OnMouseExit()
        {
            if (_sr != null) _sr.color = _normalColor;
        }
    }
}
