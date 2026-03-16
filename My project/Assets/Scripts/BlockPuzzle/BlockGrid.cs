using UnityEngine;
using System.Collections.Generic;

namespace ColorDrive
{
    public enum SlotDirection { Top, Bottom, Left, Right }

    /// <summary>
    /// Manages the block puzzle grid — placement, clearing, and state.
    /// </summary>
    public class BlockGrid : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int width = 8;
        [SerializeField] private int height = 8;
        [SerializeField] private float cellSize = 1f;

        [Header("Prefabs")]
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GameObject blockPrefab;

        // Grid state: -1 = empty, >= 0 = colorIndex
        private int[,] _grid;
        private GameObject[,] _cellObjects;
        private List<BlockPiece> _placedPieces = new List<BlockPiece>();

        public int Width => width;
        public int Height => height;
        public bool IsFull => CheckFull();

        public event System.Action<int, int> OnBlocksCleared; // colorIndex, count
        public event System.Action OnGridFull;

        void Awake()
        {
            InitGrid();
        }

        public void Setup(int w, int h)
        {
            width = w;
            height = h;
            InitGrid();
        }

        private void InitGrid()
        {
            _grid = new int[width, height];
            _cellObjects = new GameObject[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _grid[x, y] = -1;

            BuildVisualGrid();
        }

        private void BuildVisualGrid()
        {
            // Clear existing visual cells
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            if (cellPrefab == null) return;

            Vector3 origin = GetGridOrigin();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 pos = origin + new Vector3(x * cellSize, y * cellSize, 0);
                    var cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
                    cell.name = $"Cell_{x}_{y}";
                    _cellObjects[x, y] = cell;
                }
            }
        }

        private Vector3 GetGridOrigin()
        {
            float offsetX = -(width * cellSize) / 2f + cellSize / 2f;
            float offsetY = -(height * cellSize) / 2f + cellSize / 2f;
            return transform.position + new Vector3(offsetX, offsetY, 0);
        }

        public Vector3 GridToWorld(int x, int y)
        {
            return GetGridOrigin() + new Vector3(x * cellSize, y * cellSize, 0);
        }

        /// <summary>
        /// Try to slide a piece in from the given edge slot.
        /// Returns the furthest position the piece can slide to.
        /// </summary>
        public bool TryPlacePiece(BlockPiece piece, SlotDirection direction, int slotIndex)
        {
            List<Vector2Int> worldCells = GetPieceWorldCells(piece, direction, slotIndex);
            if (worldCells == null) return false;

            // Check all cells are empty
            foreach (var cell in worldCells)
            {
                if (!IsInBounds(cell.x, cell.y) || _grid[cell.x, cell.y] != -1)
                    return false;
            }

            // Place the piece
            foreach (var cell in worldCells)
                _grid[cell.x, cell.y] = piece.colorIndex;

            // Position piece visually
            Vector2Int anchorCell = worldCells[0];
            piece.transform.position = GridToWorld(anchorCell.x, anchorCell.y);
            piece.isPlaced = true;
            piece.AnimatePlace();
            _placedPieces.Add(piece);

            // Check for clears
            CheckAndClear(piece.colorIndex);

            if (IsFull) OnGridFull?.Invoke();

            return true;
        }

        private List<Vector2Int> GetPieceWorldCells(BlockPiece piece, SlotDirection dir, int slot)
        {
            var cells = new List<Vector2Int>();
            // Determine starting position based on direction
            int startX, startY;
            switch (dir)
            {
                case SlotDirection.Left:
                    startX = 0; startY = slot;
                    foreach (var c in piece.cells)
                        cells.Add(new Vector2Int(startX + c.x, startY + c.y));
                    break;
                case SlotDirection.Right:
                    startX = width - 1; startY = slot;
                    foreach (var c in piece.cells)
                        cells.Add(new Vector2Int(startX - c.x, startY + c.y));
                    break;
                case SlotDirection.Top:
                    startX = slot; startY = height - 1;
                    foreach (var c in piece.cells)
                        cells.Add(new Vector2Int(startX + c.y, startY - c.x));
                    break;
                case SlotDirection.Bottom:
                    startX = slot; startY = 0;
                    foreach (var c in piece.cells)
                        cells.Add(new Vector2Int(startX + c.y, startY + c.x));
                    break;
                default:
                    return null;
            }
            return cells;
        }

        private void CheckAndClear(int targetColor)
        {
            int cleared = 0;
            var toClearSet = new HashSet<Vector2Int>();

            // Check full rows of same color
            for (int y = 0; y < height; y++)
            {
                bool rowMatch = true;
                for (int x = 0; x < width; x++)
                    if (_grid[x, y] != targetColor) { rowMatch = false; break; }

                if (rowMatch)
                    for (int x = 0; x < width; x++)
                        toClearSet.Add(new Vector2Int(x, y));
            }

            // Check full columns of same color
            for (int x = 0; x < width; x++)
            {
                bool colMatch = true;
                for (int y = 0; y < height; y++)
                    if (_grid[x, y] != targetColor) { colMatch = false; break; }

                if (colMatch)
                    for (int y = 0; y < height; y++)
                        toClearSet.Add(new Vector2Int(x, y));
            }

            // Also check 3-in-a-row (horizontal/vertical) of same color
            foreach (var cell in FindColorMatches(targetColor, 3))
                toClearSet.Add(cell);

            var toClear = new List<Vector2Int>(toClearSet);

            if (toClear.Count > 0)
            {
                cleared = toClear.Count;
                foreach (var pos in toClear)
                {
                    _grid[pos.x, pos.y] = -1;
                    if (_cellObjects != null && _cellObjects[pos.x, pos.y] != null)
                    {
                        // Flash the cell
                        var cellSR = _cellObjects[pos.x, pos.y].GetComponent<SpriteRenderer>();
                        if (cellSR != null)
                        {
                            Color flashColor = ThemeManager.Instance?.GetBlockColor(targetColor) ?? Color.white;
                            cellSR.color = flashColor;
                            TweenHelper.ColorTo(cellSR, flashColor, Color.white, 0.3f);
                        }
                    }
                }
                OnBlocksCleared?.Invoke(targetColor, cleared);
                GameManager.Instance?.AddScore(cleared * 10 * (1 + cleared / 5));
            }
        }

        private List<Vector2Int> FindColorMatches(int colorIndex, int minLength)
        {
            var matches = new HashSet<Vector2Int>();

            // Horizontal matches
            for (int y = 0; y < height; y++)
            {
                int run = 0;
                int runStart = 0;
                for (int x = 0; x <= width; x++)
                {
                    if (x < width && _grid[x, y] == colorIndex)
                    {
                        if (run == 0) runStart = x;
                        run++;
                    }
                    else
                    {
                        if (run >= minLength)
                            for (int rx = runStart; rx < runStart + run; rx++)
                                matches.Add(new Vector2Int(rx, y));
                        run = 0;
                    }
                }
            }

            // Vertical matches
            for (int x = 0; x < width; x++)
            {
                int run = 0;
                int runStart = 0;
                for (int y = 0; y <= height; y++)
                {
                    if (y < height && _grid[x, y] == colorIndex)
                    {
                        if (run == 0) runStart = y;
                        run++;
                    }
                    else
                    {
                        if (run >= minLength)
                            for (int ry = runStart; ry < runStart + run; ry++)
                                matches.Add(new Vector2Int(x, ry));
                        run = 0;
                    }
                }
            }

            return new List<Vector2Int>(matches);
        }

        private bool CheckFull()
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (_grid[x, y] == -1) return false;
            return true;
        }

        public bool IsInBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

        public int GetCell(int x, int y) => IsInBounds(x, y) ? _grid[x, y] : -2;

        public void ClearAll()
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _grid[x, y] = -1;
            _placedPieces.Clear();
        }

        public bool IsBoardCleared()
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (_grid[x, y] != -1) return false;
            return true;
        }
    }
}
