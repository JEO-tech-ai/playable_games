using UnityEngine;
using System.Collections.Generic;

namespace ColorDrive
{
    /// <summary>
    /// Top-level controller for Block Puzzle mode.
    /// Manages piece queue, input, timer, move counter, and win/lose conditions.
    /// </summary>
    public class BlockPuzzleController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BlockGrid grid;
        [SerializeField] private Transform pieceQueueParent;
        [SerializeField] private GameObject blockPiecePrefab;
        [SerializeField] private GameObject blockCellPrefab; // single 1x1 cell visual

        [Header("Gameplay")]
        [SerializeField] private int queueSize = 3;
        private int _movesRemaining;
        private float _timeRemaining;
        private bool _gameActive = false;
        private LevelData _levelData;

        private List<BlockPiece> _pieceQueue = new List<BlockPiece>();
        private BlockPiece _selectedPiece = null;

        // Piece shapes: list of relative cell offsets
        private static readonly List<List<Vector2Int>> PIECE_SHAPES = new List<List<Vector2Int>>
        {
            // 1x1
            new List<Vector2Int> { new Vector2Int(0, 0) },
            // 1x2 horizontal
            new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0) },
            // 1x3 horizontal
            new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
            // 1x2 vertical
            new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(0, 1) },
            // 2x2 square
            new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            // L-shape
            new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 0) },
            // T-shape
            new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1) },
        };

        public int MovesRemaining => _movesRemaining;
        public float TimeRemaining => _timeRemaining;
        public bool IsGameActive => _gameActive;

        public event System.Action<int> OnMovesChanged;
        public event System.Action<float> OnTimeChanged;
        public event System.Action OnLevelComplete;
        public event System.Action OnGameOver;

        void Update()
        {
            if (!_gameActive) return;
            if (_levelData != null && _levelData.timeLimit > 0)
            {
                _timeRemaining -= Time.deltaTime;
                OnTimeChanged?.Invoke(_timeRemaining);
                if (_timeRemaining <= 0f)
                {
                    _timeRemaining = 0f;
                    TriggerGameOver();
                }
            }
        }

        public void StartLevel(LevelData levelData)
        {
            _levelData = levelData;
            _movesRemaining = levelData.moveLimit;
            _timeRemaining = levelData.timeLimit;

            grid.Setup(levelData.gridWidth, levelData.gridHeight);
            grid.OnBlocksCleared -= HandleBlocksCleared;
            grid.OnBlocksCleared += HandleBlocksCleared;
            grid.OnGridFull -= TriggerGameOver;
            grid.OnGridFull += TriggerGameOver;

            LoadPresetOrGenerate(levelData);
            RefillQueue();
            _gameActive = true;
        }

        private void LoadPresetOrGenerate(LevelData levelData)
        {
            if (!string.IsNullOrEmpty(levelData.presetLayoutJson))
            {
                // TODO: Parse preset JSON layout
                Debug.Log("[BlockPuzzle] Loading preset layout");
            }
            // else: empty grid, pieces are placed via queue
        }

        private void RefillQueue()
        {
            // Clear previous queue visuals
            foreach (Transform child in pieceQueueParent)
                Destroy(child.gameObject);
            _pieceQueue.Clear();

            for (int i = 0; i < queueSize; i++)
                _pieceQueue.Add(SpawnQueuePiece(i));
        }

        private BlockPiece SpawnQueuePiece(int queueIndex)
        {
            if (blockPiecePrefab == null || _levelData == null) return null;

            int shapeIdx = Random.Range(0, PIECE_SHAPES.Count);
            int colorIdx = Random.Range(0, _levelData.colorPalette.Length);

            var go = Instantiate(blockPiecePrefab, pieceQueueParent);
            go.transform.localPosition = new Vector3(queueIndex * 2f, 0, 0);

            var piece = go.GetComponent<BlockPiece>();
            if (piece == null) piece = go.AddComponent<BlockPiece>();

            // Build visual cells
            BuildPieceVisuals(piece, PIECE_SHAPES[shapeIdx], colorIdx);
            piece.Initialize(colorIdx, PIECE_SHAPES[shapeIdx]);

            return piece;
        }

        private void BuildPieceVisuals(BlockPiece piece, List<Vector2Int> shape, int colorIdx)
        {
            if (blockCellPrefab == null) return;
            Color color = _levelData?.colorPalette[colorIdx] ?? Color.white;

            foreach (var cell in shape)
            {
                var cellGO = Instantiate(blockCellPrefab, piece.transform);
                cellGO.transform.localPosition = new Vector3(cell.x * 0.9f, cell.y * 0.9f, 0);
                var sr = cellGO.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = color;
            }
        }

        public void SelectPiece(BlockPiece piece)
        {
            if (!_gameActive) return;
            if (_selectedPiece != null) _selectedPiece.SetHighlight(false);
            _selectedPiece = piece;
            piece.SetHighlight(true);
        }

        public bool PlacePieceAtSlot(SlotDirection direction, int slotIndex)
        {
            if (!_gameActive || _selectedPiece == null || _movesRemaining <= 0) return false;

            bool placed = grid.TryPlacePiece(_selectedPiece, direction, slotIndex);
            if (!placed) return false;

            _selectedPiece.SetHighlight(false);
            _pieceQueue.Remove(_selectedPiece);
            _selectedPiece = null;

            _movesRemaining--;
            OnMovesChanged?.Invoke(_movesRemaining);

            if (_movesRemaining <= 0 && !grid.IsBoardCleared())
                TriggerGameOver();

            if (_pieceQueue.Count == 0)
                RefillQueue();

            return true;
        }

        private void HandleBlocksCleared(int colorIndex, int count)
        {
            Debug.Log($"[BlockPuzzle] Cleared {count} blocks of color {colorIndex}");
            if (grid.IsBoardCleared())
                TriggerLevelComplete();
        }

        private void TriggerLevelComplete()
        {
            _gameActive = false;
            OnLevelComplete?.Invoke();
            GameManager.Instance?.CompleteLevel();
        }

        private void TriggerGameOver()
        {
            _gameActive = false;
            OnGameOver?.Invoke();
            GameManager.Instance?.TriggerGameOver();
        }

        public void UseExtraMoves(int count)
        {
            _movesRemaining += count;
            OnMovesChanged?.Invoke(_movesRemaining);
        }

        public void AddTime(float seconds)
        {
            _timeRemaining += seconds;
        }

        /// <summary>
        /// Returns a hint: the best piece + slot to play next.
        /// Simple heuristic: find the move that would create the most clears.
        /// </summary>
        public (BlockPiece piece, SlotDirection dir, int slot) GetHint()
        {
            // Simplified: just suggest the first available slot for the first piece
            if (_pieceQueue.Count > 0)
                return (_pieceQueue[0], SlotDirection.Left, 0);
            return (null, SlotDirection.Left, 0);
        }
    }
}
