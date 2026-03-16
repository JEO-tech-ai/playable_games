using UnityEngine;
using System.Collections.Generic;

namespace ColorDrive
{
    /// <summary>
    /// Procedurally builds the entire game scene at runtime.
    /// No external prefabs required — uses Unity primitives (Quad sprites).
    /// Place this component on an empty GameObject in SampleScene.
    /// </summary>
    public class GameSceneSetup : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private PuzzleMode startMode = PuzzleMode.BlockPuzzle;
        [SerializeField] private int gridWidth = 6;
        [SerializeField] private int gridHeight = 6;
        [SerializeField] private float cellSize = 0.85f;

        [Header("Colors (Fallback if ThemeManager missing)")]
        [SerializeField] private Color[] defaultColors = new Color[]
        {
            new Color(0.2f, 0.4f, 1f),
            new Color(1f, 0.2f, 0.2f),
            new Color(0.2f, 0.85f, 0.2f),
            new Color(1f, 0.8f, 0f),
            new Color(1f, 0.45f, 0f),
            new Color(0.8f, 0.2f, 1f),
        };

        // Internal references
        private BlockPuzzleController _blockCtrl;
        private WaterSortController _waterCtrl;
        private BlockGrid _blockGrid;
        private GameObject _blockRoot;
        private GameObject _slotRoot;
        private GameObject _queueRoot;
        private List<PieceQueueItem> _queueItems = new List<PieceQueueItem>();
        private List<BlockPiece> _piecesInQueue = new List<BlockPiece>();

        // Piece shapes
        private static readonly List<List<Vector2Int>> SHAPES = new List<List<Vector2Int>>
        {
            new List<Vector2Int> { new Vector2Int(0,0) },
            new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(1,0) },
            new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0) },
            new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(0,1) },
            new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1) },
        };

        void Awake()
        {
            EnsureManagers();
        }

        void Start()
        {
            SetupCamera();
            SetupBackground();
            BuildBlockPuzzleScene();
            Debug.Log("[GameSceneSetup] Scene ready — click a queue piece, then click a slot arrow to place it!");
        }

        // ── Manager bootstrap ─────────────────────────────────────────────────

        private void EnsureManagers()
        {
            if (GameManager.Instance == null)
            {
                var go = new GameObject("GameManager");
                go.AddComponent<GameManager>();
            }
            if (ThemeManager.Instance == null)
            {
                var go = new GameObject("ThemeManager");
                go.AddComponent<ThemeManager>();
            }
            if (AdManager.Instance == null)
            {
                var go = new GameObject("AdManager");
                go.AddComponent<AdManager>();
            }
            if (IAPManager.Instance == null)
            {
                var go = new GameObject("IAPManager");
                go.AddComponent<IAPManager>();
            }

            // Minimal LevelManager
            if (LevelManager.Instance == null)
            {
                var go = new GameObject("LevelManager");
                go.AddComponent<LevelManager>();
            }
        }

        // ── Camera ────────────────────────────────────────────────────────────

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            cam.transform.position = new Vector3(0, 0, -10f);
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        }

        // ── Background ────────────────────────────────────────────────────────

        private void SetupBackground()
        {
            var bg = CreateQuad("Background", Vector3.zero, new Vector2(20f, 15f),
                new Color(0.08f, 0.08f, 0.12f), 10);
        }

        // ── Block Puzzle Scene ────────────────────────────────────────────────

        private void BuildBlockPuzzleScene()
        {
            _blockRoot = new GameObject("BlockPuzzle");

            // Grid background panel
            float gridW = gridWidth * cellSize;
            float gridH = gridHeight * cellSize;
            CreateQuad("GridBG", Vector3.zero, new Vector2(gridW + 0.2f, gridH + 0.2f),
                new Color(0.15f, 0.15f, 0.2f), 5);

            // Draw cell backgrounds
            DrawGridCells();

            // Slot buttons on 4 edges
            _slotRoot = new GameObject("Slots");
            _slotRoot.transform.SetParent(_blockRoot.transform);
            CreateSlotButtons();

            // Block Grid component
            var gridGO = new GameObject("BlockGrid");
            gridGO.transform.SetParent(_blockRoot.transform);
            _blockGrid = gridGO.AddComponent<BlockGrid>();
            _blockGrid.Setup(gridWidth, gridHeight);

            // BlockPuzzleController
            var ctrlGO = new GameObject("BlockPuzzleController");
            ctrlGO.transform.SetParent(_blockRoot.transform);
            _blockCtrl = ctrlGO.AddComponent<BlockPuzzleController>();

            // Queue area
            _queueRoot = new GameObject("PieceQueue");
            _queueRoot.transform.position = new Vector3(0, -(gridH / 2f + 1.5f), 0);
            _queueRoot.transform.SetParent(_blockRoot.transform);
            CreateQueueLabel();

            // Build and display initial piece queue
            BuildInitialQueue(3);

            // Start game state
            GameManager.Instance?.SetState(GameState.Playing);

            // HUD labels
            BuildHUDLabels();

            Debug.Log("[GameSceneSetup] Block Puzzle scene built. Click a piece then click a slot ▶ to place.");
        }

        private void DrawGridCells()
        {
            float originX = -(gridWidth * cellSize) / 2f + cellSize / 2f;
            float originY = -(gridHeight * cellSize) / 2f + cellSize / 2f;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 pos = new Vector3(originX + x * cellSize, originY + y * cellSize, 0.1f);
                    bool dark = (x + y) % 2 == 0;
                    Color c = dark ? new Color(0.2f, 0.2f, 0.28f) : new Color(0.22f, 0.22f, 0.3f);
                    CreateQuad($"Cell_{x}_{y}", pos, Vector2.one * (cellSize - 0.05f), c, 4);
                }
            }
        }

        private void CreateSlotButtons()
        {
            float halfW = gridWidth * cellSize / 2f;
            float halfH = gridHeight * cellSize / 2f;
            float slotSize = cellSize * 0.7f;
            float arrowOffset = cellSize * 0.8f;

            // Left slots (arrows pointing right →)
            for (int y = 0; y < gridHeight; y++)
            {
                float posY = -(gridHeight * cellSize) / 2f + cellSize / 2f + y * cellSize;
                Vector3 pos = new Vector3(-halfW - arrowOffset, posY, -0.1f);
                var slot = CreateSlotQuad($"Slot_L_{y}", pos, slotSize, "▶", SlotDirection.Left, y);
            }

            // Right slots (arrows pointing left ◀)
            for (int y = 0; y < gridHeight; y++)
            {
                float posY = -(gridHeight * cellSize) / 2f + cellSize / 2f + y * cellSize;
                Vector3 pos = new Vector3(halfW + arrowOffset, posY, -0.1f);
                var slot = CreateSlotQuad($"Slot_R_{y}", pos, slotSize, "◀", SlotDirection.Right, y);
            }

            // Bottom slots (arrows pointing up ▲)
            for (int x = 0; x < gridWidth; x++)
            {
                float posX = -(gridWidth * cellSize) / 2f + cellSize / 2f + x * cellSize;
                Vector3 pos = new Vector3(posX, -halfH - arrowOffset, -0.1f);
                var slot = CreateSlotQuad($"Slot_B_{x}", pos, slotSize, "▲", SlotDirection.Bottom, x);
            }

            // Top slots (arrows pointing down ▼)
            for (int x = 0; x < gridWidth; x++)
            {
                float posX = -(gridWidth * cellSize) / 2f + cellSize / 2f + x * cellSize;
                Vector3 pos = new Vector3(posX, halfH + arrowOffset, -0.1f);
                var slot = CreateSlotQuad($"Slot_T_{x}", pos, slotSize, "▼", SlotDirection.Top, x);
            }
        }

        private GameObject CreateSlotQuad(string name, Vector3 pos, float size, string arrow,
            SlotDirection dir, int idx)
        {
            var go = CreateQuad(name, pos, Vector2.one * size,
                new Color(0.4f, 0.8f, 0.4f, 0.3f), 3);
            go.transform.SetParent(_slotRoot.transform);

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one * size;

            var btn = go.AddComponent<SlotButton>();
            btn.direction = dir;
            btn.slotIndex = idx;
            // Controller will be set after _blockCtrl is created, see wiring below

            // Arrow text label
            var label = new GameObject("ArrowLabel");
            label.transform.SetParent(go.transform);
            label.transform.localPosition = Vector3.zero;
            var tm = label.AddComponent<TextMesh>();
            tm.text = arrow;
            tm.fontSize = 18;
            tm.characterSize = 0.08f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = new Color(0.6f, 1f, 0.6f);

            return go;
        }

        private void BuildInitialQueue(int count)
        {
            _piecesInQueue.Clear();
            _queueItems.Clear();

            for (int i = 0; i < count; i++)
            {
                int shapeIdx = Random.Range(0, SHAPES.Count);
                int colorIdx = Random.Range(0, defaultColors.Length);
                var shape = SHAPES[shapeIdx];
                Color color = defaultColors[colorIdx];

                // Create piece visual container
                var pieceGO = new GameObject($"QueuePiece_{i}");
                pieceGO.transform.SetParent(_queueRoot.transform);
                pieceGO.transform.localPosition = new Vector3((i - (count - 1) / 2f) * 2f, 0, 0);

                // Background
                var bgGO = CreateQuad("PieceBG", pieceGO.transform.position,
                    Vector2.one * 1.6f, new Color(1, 1, 1, 0.08f), 2);
                bgGO.transform.SetParent(pieceGO.transform);
                bgGO.transform.localPosition = Vector3.zero;
                var bgCol = bgGO.AddComponent<BoxCollider2D>();
                bgCol.size = Vector2.one * 1.6f;

                // Create BlockPiece component
                var bp = pieceGO.AddComponent<BlockPiece>();
                BuildBlockVisuals(bp, shape, color, pieceGO.transform);
                bp.Initialize(colorIdx, shape);

                // Queue item click handler
                var queueItem = bgGO.AddComponent<PieceQueueItem>();
                queueItem.Setup(bp, _blockCtrl);

                _piecesInQueue.Add(bp);
                _queueItems.Add(queueItem);
            }

            // Wire controller references now that everything exists
            WireSlotButtons();
        }

        private void BuildBlockVisuals(BlockPiece bp, List<Vector2Int> shape, Color color, Transform parent)
        {
            foreach (var cell in shape)
            {
                var cellGO = CreateQuad($"Cell_{cell.x}_{cell.y}",
                    parent.position + new Vector3(cell.x * 0.5f, cell.y * 0.5f, -0.05f),
                    Vector2.one * 0.45f, color, 1);
                cellGO.transform.SetParent(parent);

                // Add border highlight
                var border = CreateQuad($"Border",
                    cellGO.transform.position + new Vector3(0, 0, -0.02f),
                    Vector2.one * 0.48f,
                    new Color(1f, 1f, 1f, 0.25f), 0);
                border.transform.SetParent(cellGO.transform);
            }
        }

        private void WireSlotButtons()
        {
            if (_slotRoot == null || _blockCtrl == null) return;
            foreach (var btn in _slotRoot.GetComponentsInChildren<SlotButton>())
                btn.Setup(_blockCtrl, btn.direction, btn.slotIndex);
        }

        private void CreateQueueLabel()
        {
            var labelGO = new GameObject("QueueLabel");
            labelGO.transform.SetParent(_queueRoot.transform);
            labelGO.transform.localPosition = new Vector3(0, 1f, 0);
            var tm = labelGO.AddComponent<TextMesh>();
            tm.text = "SELECT PIECE";
            tm.fontSize = 20;
            tm.characterSize = 0.06f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = new Color(0.8f, 0.8f, 0.8f);
        }

        private void BuildHUDLabels()
        {
            float gridH = gridHeight * cellSize / 2f;

            // Title
            CreateTextMesh("ColorDrive", new Vector3(0, gridH + 1.8f, 0),
                "COLOR DRIVE", 0.1f, new Color(1f, 0.8f, 0.2f));

            // Instructions
            CreateTextMesh("Instructions", new Vector3(0, -(gridHeight * cellSize / 2f) - 3.2f, 0),
                "① Click piece  ② Click ▶ slot to place  ③ Match 3+ same color to clear!",
                0.045f, new Color(0.6f, 0.8f, 1f));

            // Score display
            CreateTextMesh("ScoreLabel", new Vector3(-4f, gridH + 1.8f, 0),
                "SCORE: 0", 0.065f, Color.white);
        }

        private GameObject CreateTextMesh(string name, Vector3 pos, string text, float charSize, Color color)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 24;
            tm.characterSize = charSize;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = color;
            return go;
        }

        // ── Primitives ────────────────────────────────────────────────────────

        private GameObject CreateQuad(string name, Vector3 pos, Vector2 size, Color color, int sortOrder)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateWhiteSprite();
            sr.color = color;
            sr.sortingOrder = sortOrder;

            return go;
        }

        private Sprite _whiteSprite;

        private Sprite CreateWhiteSprite()
        {
            if (_whiteSprite != null) return _whiteSprite;

            Texture2D tex = new Texture2D(4, 4);
            Color[] pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();

            _whiteSprite = Sprite.Create(tex,
                new Rect(0, 0, 4, 4),
                new Vector2(0.5f, 0.5f),
                4f);
            return _whiteSprite;
        }
    }
}
