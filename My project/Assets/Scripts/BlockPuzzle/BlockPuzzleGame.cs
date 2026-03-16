using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace ColorDrive
{
    /// <summary>
    /// Self-contained Block Puzzle game that actually works.
    /// Creates its own visuals, handles input, manages game loop.
    /// Attach to an empty GameObject in SampleScene.
    /// </summary>
    public class BlockPuzzleGame : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private int gridCols = 8;
        [SerializeField] private int gridRows = 8;
        [SerializeField] private float cellSize = 0.75f;

        [Header("Rules")]
        [SerializeField] private int movesAllowed = 30;

        private Color[] COLORS = new Color[]
        {
            new Color(0.2f, 0.45f, 1f),    // Blue
            new Color(1f, 0.25f, 0.25f),   // Red
            new Color(0.2f, 0.85f, 0.3f),  // Green
            new Color(1f, 0.85f, 0f),      // Yellow
            new Color(1f, 0.5f, 0f),       // Orange
            new Color(0.75f, 0.2f, 1f),    // Purple
        };

        // ── Game State ────────────────────────────────────────────────────────
        private int[,] _grid;            // -1 = empty
        private GameObject[,] _cellVis;  // visual per cell
        private int _movesLeft;
        private int _score;
        private bool _gameActive = false;

        // ── Public API for ad integration ─────────────────────────────────────
        public int Score => _score;
        public System.Action<int> OnGameEnded;

        // ── Piece Queue ───────────────────────────────────────────────────────
        private struct Piece { public List<Vector2Int> cells; public int colorIdx; }
        private Piece[] _queue = new Piece[3];
        private int _selectedPiece = -1;

        // Slot direction for input
        public enum Dir { Left, Right, Up, Down }

        // ── Visuals ───────────────────────────────────────────────────────────
        private GameObject[] _queueVis = new GameObject[3];
        private TextMesh _scoreText, _movesText, _statusText;
        private Sprite _whiteSprite;

        // ── Piece Shapes ──────────────────────────────────────────────────────
        private List<List<Vector2Int>> SHAPES = new List<List<Vector2Int>>
        {
            new List<Vector2Int> { V(0,0) },
            new List<Vector2Int> { V(0,0), V(1,0) },
            new List<Vector2Int> { V(0,0), V(1,0), V(2,0) },
            new List<Vector2Int> { V(0,0), V(0,1) },
            new List<Vector2Int> { V(0,0), V(0,1), V(0,2) },
            new List<Vector2Int> { V(0,0), V(1,0), V(0,1) },
            new List<Vector2Int> { V(0,0), V(1,0), V(1,1) },
        };
        private static Vector2Int V(int x, int y) => new Vector2Int(x, y);

        // ── Unity Lifecycle ───────────────────────────────────────────────────

        void Start()
        {
            _whiteSprite = MakeWhiteSprite();
            SetupCamera();
            SetupBackground();
            InitGrid();
            BuildGridVisuals();
            BuildSlots();
            BuildQueueArea();
            BuildHUD();
            RefillQueue();
            UpdateQueueVisuals();
            SetGameActive(true);
            ShowStatus("Click a piece, then click a slot arrow!");
        }

        void Update()
        {
            if (!_gameActive) return;

            // Keyboard shortcut: R to restart (supports both Input systems)
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
                RestartGame();
#else
            if (Input.GetKeyDown(KeyCode.R)) RestartGame();
#endif
        }

        // ── Setup ─────────────────────────────────────────────────────────────

        void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.orthographic = true;
            cam.orthographicSize = 6.5f;
            cam.transform.position = new Vector3(0, 0, -10);
            cam.backgroundColor = new Color(0.07f, 0.07f, 0.12f);
        }

        void SetupBackground()
        {
            MakeQuad("Background", Vector3.forward * 2f, new Vector2(25f, 18f),
                new Color(0.07f, 0.07f, 0.12f), -10);
        }

        void InitGrid()
        {
            _grid = new int[gridCols, gridRows];
            _cellVis = new GameObject[gridCols, gridRows];
            for (int x = 0; x < gridCols; x++)
                for (int y = 0; y < gridRows; y++)
                    _grid[x, y] = -1;
            _movesLeft = movesAllowed;
            _score = 0;
        }

        void BuildGridVisuals()
        {
            float ox = GridOriginX();
            float oy = GridOriginY();

            // Grid panel background
            float gw = gridCols * cellSize;
            float gh = gridRows * cellSize;
            MakeQuad("GridPanel", new Vector3(0, 0, 0.5f), new Vector2(gw + 0.3f, gh + 0.3f),
                new Color(0.13f, 0.13f, 0.2f), -2);

            for (int x = 0; x < gridCols; x++)
                for (int y = 0; y < gridRows; y++)
                {
                    float px = ox + x * cellSize;
                    float py = oy + y * cellSize;
                    bool dark = (x + y) % 2 == 0;
                    Color c = dark ? new Color(0.18f, 0.18f, 0.26f) : new Color(0.2f, 0.2f, 0.28f);
                    var cell = MakeQuad($"C{x},{y}", new Vector3(px, py, 0.2f),
                        Vector2.one * (cellSize - 0.04f), c, -1);
                    _cellVis[x, y] = cell;
                }
        }

        void BuildSlots()
        {
            float ox = GridOriginX();
            float oy = GridOriginY();
            float hw = gridCols * cellSize / 2f;
            float hh = gridRows * cellSize / 2f;
            float off = cellSize * 0.75f;

            // Left (→)
            for (int y = 0; y < gridRows; y++)
            {
                float py = oy + y * cellSize;
                MakeSlot($"SL{y}", new Vector3(-hw - off, py, -0.2f), "→",
                    new Color(0.3f, 0.9f, 0.3f, 0.4f), Dir.Left, y);
            }
            // Right (←)
            for (int y = 0; y < gridRows; y++)
            {
                float py = oy + y * cellSize;
                MakeSlot($"SR{y}", new Vector3(hw + off, py, -0.2f), "←",
                    new Color(0.3f, 0.9f, 0.3f, 0.4f), Dir.Right, y);
            }
            // Bottom (↑)
            for (int x = 0; x < gridCols; x++)
            {
                float px = ox + x * cellSize;
                MakeSlot($"SB{x}", new Vector3(px, -hh - off, -0.2f), "↑",
                    new Color(0.9f, 0.6f, 0.3f, 0.4f), Dir.Down, x);
            }
            // Top (↓)
            for (int x = 0; x < gridCols; x++)
            {
                float px = ox + x * cellSize;
                MakeSlot($"ST{x}", new Vector3(px, hh + off, -0.2f), "↓",
                    new Color(0.9f, 0.6f, 0.3f, 0.4f), Dir.Up, x);
            }
        }

        void MakeSlot(string name, Vector3 pos, string arrow, Color color, Dir dir, int idx)
        {
            var go = MakeQuad(name, pos, Vector2.one * (cellSize * 0.7f), color, 1);
            go.AddComponent<BoxCollider2D>().size = Vector2.one * (cellSize * 0.7f);

            var tm = new GameObject("Lbl");
            tm.transform.SetParent(go.transform);
            tm.transform.localPosition = new Vector3(0, 0, -0.1f);
            var t = tm.AddComponent<TextMesh>();
            t.text = arrow;
            t.fontSize = 24;
            t.characterSize = 0.07f;
            t.anchor = TextAnchor.MiddleCenter;
            t.color = new Color(0.5f, 1f, 0.5f);

            var trigger = go.AddComponent<SlotTrigger>();
            trigger.Init(dir, idx, this);
        }

        void BuildQueueArea()
        {
            float hh = gridRows * cellSize / 2f;
            var qRoot = new GameObject("QueueArea");
            qRoot.transform.position = new Vector3(0, -hh - 2.1f, 0);

            var lbl = new GameObject("QueueLbl");
            lbl.transform.SetParent(qRoot.transform);
            lbl.transform.localPosition = new Vector3(0, 0.7f, 0);
            var tm = lbl.AddComponent<TextMesh>();
            tm.text = "SELECT PIECE";
            tm.fontSize = 20;
            tm.characterSize = 0.055f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = new Color(0.7f, 0.7f, 0.9f);

            for (int i = 0; i < 3; i++)
            {
                float px = (i - 1) * 2.2f;
                var slot = MakeQuad($"QSlot{i}",
                    qRoot.transform.position + new Vector3(px, 0, -0.2f),
                    Vector2.one * 1.8f, new Color(0.15f, 0.15f, 0.25f, 0.8f), 0);
                slot.transform.SetParent(qRoot.transform);
                slot.AddComponent<BoxCollider2D>().size = Vector2.one * 1.8f;

                int capturedI = i;
                var qt = slot.AddComponent<QueueTrigger>();
                qt.Init(capturedI, this);

                _queueVis[i] = slot;
            }
        }

        void BuildHUD()
        {
            float hh = gridRows * cellSize / 2f;
            float hw = gridCols * cellSize / 2f;

            // Title
            var titleGO = MakeTextMesh("Title", new Vector3(0, hh + 1.6f, 0),
                "COLOR DRIVE", 0.09f, new Color(1f, 0.85f, 0.2f));

            // Score
            var scoreGO = MakeTextMesh("Score", new Vector3(-hw - 0.2f, hh + 1.6f, 0),
                "SCORE\n0", 0.06f, Color.white);
            _scoreText = scoreGO.GetComponent<TextMesh>();

            // Moves
            var movesGO = MakeTextMesh("Moves", new Vector3(hw + 0.2f, hh + 1.6f, 0),
                $"MOVES\n{movesAllowed}", 0.06f, new Color(1f, 0.6f, 0.2f));
            _movesText = movesGO.GetComponent<TextMesh>();

            // Status/instruction
            var statusGO = MakeTextMesh("Status",
                new Vector3(0, -(hh + gridRows * cellSize / 2f + 3.9f), 0),
                "", 0.048f, new Color(0.5f, 0.8f, 1f));
            _statusText = statusGO.GetComponent<TextMesh>();

            // R key hint
            MakeTextMesh("RHint", new Vector3(hw + 1.5f, -hh - 0.3f, 0),
                "[R] Restart", 0.042f, new Color(0.4f, 0.4f, 0.5f));
        }

        // ── Game Logic ────────────────────────────────────────────────────────

        void RefillQueue()
        {
            for (int i = 0; i < 3; i++)
            {
                _queue[i] = new Piece
                {
                    cells = SHAPES[Random.Range(0, SHAPES.Count)],
                    colorIdx = Random.Range(0, COLORS.Length)
                };
            }
            _selectedPiece = -1;
        }

        public void OnQueueSlotClicked(int idx)
        {
            if (!_gameActive) return;
            _selectedPiece = idx;
            HighlightQueueSlot(idx);
            ShowStatus($"Place piece {idx + 1} → click a slot arrow");
        }

        public void OnSlotClicked(Dir dir, int slotIdx)
        {
            if (!_gameActive || _selectedPiece < 0) return;

            var piece = _queue[_selectedPiece];
            List<Vector2Int> worldCells = GetWorldCells(piece.cells, dir, slotIdx);

            if (worldCells == null || !CanPlace(worldCells))
            {
                ShowStatus("Can't place there! Try another slot.");
                ShakeStatus();
                return;
            }

            // Place
            foreach (var c in worldCells)
            {
                _grid[c.x, c.y] = piece.colorIdx;
                UpdateCellVisual(c.x, c.y, COLORS[piece.colorIdx]);
            }

            _movesLeft--;
            UpdateHUD();

            // Check clears
            int clearedCount = CheckAndClear();
            if (clearedCount > 0)
            {
                _score += clearedCount * 10 * (1 + clearedCount / 5);
                UpdateHUD();
                ShowStatus($"NICE! Cleared {clearedCount} blocks! +{clearedCount * 10}pts");
            }

            // Check win (board clear)
            if (IsBoardClear())
            {
                SetGameActive(false);
                ShowStatus("BOARD CLEARED! Score: " + _score + "  [R] to play again");
                OnGameEnded?.Invoke(_score);
                return;
            }

            // Assign next piece
            _queue[_selectedPiece] = new Piece
            {
                cells = SHAPES[Random.Range(0, SHAPES.Count)],
                colorIdx = Random.Range(0, COLORS.Length)
            };
            _selectedPiece = -1;
            UpdateQueueVisuals();

            // Check moves
            if (_movesLeft <= 0)
            {
                SetGameActive(false);
                ShowStatus("Out of moves! Score: " + _score + "  [R] to restart");
                OnGameEnded?.Invoke(_score);
                return;
            }

            ShowStatus("Click a piece, then click a slot arrow");
        }

        private List<Vector2Int> GetWorldCells(List<Vector2Int> shape, Dir dir, int slotIdx)
        {
            var result = new List<Vector2Int>();
            foreach (var cell in shape)
            {
                int wx, wy;
                switch (dir)
                {
                    case Dir.Left:   wx = cell.x;               wy = slotIdx + cell.y; break;
                    case Dir.Right:  wx = (gridCols - 1) - cell.x; wy = slotIdx + cell.y; break;
                    case Dir.Down:   wx = slotIdx + cell.y;     wy = cell.x;           break;
                    case Dir.Up:     wx = slotIdx + cell.y;     wy = (gridRows - 1) - cell.x; break;
                    default: return null;
                }
                if (wx < 0 || wx >= gridCols || wy < 0 || wy >= gridRows)
                    return null;
                result.Add(new Vector2Int(wx, wy));
            }
            return result;
        }

        private bool CanPlace(List<Vector2Int> cells)
        {
            foreach (var c in cells)
            {
                if (c.x < 0 || c.x >= gridCols || c.y < 0 || c.y >= gridRows) return false;
                if (_grid[c.x, c.y] != -1) return false;
            }
            return true;
        }

        private int CheckAndClear()
        {
            var toClear = new HashSet<Vector2Int>();

            // Full rows of same color
            for (int y = 0; y < gridRows; y++)
            {
                int fc = _grid[0, y];
                if (fc < 0) continue;
                bool match = true;
                for (int x = 1; x < gridCols; x++)
                    if (_grid[x, y] != fc) { match = false; break; }
                if (match)
                    for (int x = 0; x < gridCols; x++)
                        toClear.Add(new Vector2Int(x, y));
            }

            // Full columns of same color
            for (int x = 0; x < gridCols; x++)
            {
                int fc = _grid[x, 0];
                if (fc < 0) continue;
                bool match = true;
                for (int y = 1; y < gridRows; y++)
                    if (_grid[x, y] != fc) { match = false; break; }
                if (match)
                    for (int y = 0; y < gridRows; y++)
                        toClear.Add(new Vector2Int(x, y));
            }

            // Horizontal runs of 3+
            for (int y = 0; y < gridRows; y++)
            {
                int run = 0, runStart = 0, runColor = -1;
                for (int x = 0; x <= gridCols; x++)
                {
                    int c = x < gridCols ? _grid[x, y] : -1;
                    if (c >= 0 && c == runColor) { run++; }
                    else
                    {
                        if (run >= 3)
                            for (int rx = runStart; rx < runStart + run; rx++)
                                toClear.Add(new Vector2Int(rx, y));
                        run = c >= 0 ? 1 : 0;
                        runStart = x;
                        runColor = c;
                    }
                }
            }

            // Vertical runs of 3+
            for (int x = 0; x < gridCols; x++)
            {
                int run = 0, runStart = 0, runColor = -1;
                for (int y = 0; y <= gridRows; y++)
                {
                    int c = y < gridRows ? _grid[x, y] : -1;
                    if (c >= 0 && c == runColor) { run++; }
                    else
                    {
                        if (run >= 3)
                            for (int ry = runStart; ry < runStart + run; ry++)
                                toClear.Add(new Vector2Int(x, ry));
                        run = c >= 0 ? 1 : 0;
                        runStart = y;
                        runColor = c;
                    }
                }
            }

            foreach (var pos in toClear)
            {
                _grid[pos.x, pos.y] = -1;
                StartCoroutine(ClearCellAnim(pos.x, pos.y));
            }

            return toClear.Count;
        }

        private IEnumerator ClearCellAnim(int x, int y)
        {
            if (_cellVis[x, y] == null) yield break;
            var sr = _cellVis[x, y].GetComponent<SpriteRenderer>();
            if (sr == null) yield break;

            // Flash white then back to dark
            Color clearColor = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            sr.color = new Color(0.18f, 0.18f, 0.26f);  // empty color
        }

        private bool IsBoardClear()
        {
            for (int x = 0; x < gridCols; x++)
                for (int y = 0; y < gridRows; y++)
                    if (_grid[x, y] >= 0) return false;
            return true;
        }

        // ── Visuals ───────────────────────────────────────────────────────────

        void UpdateCellVisual(int x, int y, Color color)
        {
            if (_cellVis[x, y] == null) return;
            var sr = _cellVis[x, y].GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = color;
        }

        void UpdateQueueVisuals()
        {
            for (int i = 0; i < 3; i++)
            {
                if (_queueVis[i] == null) continue;

                // Clear previous piece visuals
                var sr = _queueVis[i].GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(0.15f, 0.15f, 0.25f, 0.8f);

                // Destroy old cell children
                for (int c = _queueVis[i].transform.childCount - 1; c >= 0; c--)
                {
                    var child = _queueVis[i].transform.GetChild(c);
                    if (child.name.StartsWith("PC_")) Destroy(child.gameObject);
                }

                // Draw piece cells
                var piece = _queue[i];
                Color pColor = COLORS[piece.colorIdx];
                foreach (var cell in piece.cells)
                {
                    float px = _queueVis[i].transform.position.x + cell.x * 0.38f;
                    float py = _queueVis[i].transform.position.y + cell.y * 0.38f;
                    var cellGO = MakeQuad($"PC_{cell.x}_{cell.y}",
                        new Vector3(px - 0.2f, py - 0.15f, -0.5f),
                        Vector2.one * 0.34f, pColor, 3);
                    cellGO.name = $"PC_{cell.x}_{cell.y}";
                    cellGO.transform.SetParent(_queueVis[i].transform);
                }
            }
        }

        void HighlightQueueSlot(int selected)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_queueVis[i] == null) continue;
                var sr = _queueVis[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = i == selected
                        ? new Color(1f, 0.9f, 0.2f, 0.5f)
                        : new Color(0.15f, 0.15f, 0.25f, 0.8f);
            }
        }

        void UpdateHUD()
        {
            if (_scoreText != null) _scoreText.text = $"SCORE\n{_score}";
            if (_movesText != null)
            {
                _movesText.text = $"MOVES\n{_movesLeft}";
                _movesText.color = _movesLeft <= 5 ? Color.red : new Color(1f, 0.6f, 0.2f);
            }
        }

        void ShowStatus(string msg)
        {
            if (_statusText != null) _statusText.text = msg;
        }

        void ShakeStatus()
        {
            StartCoroutine(ShakeCoroutine());
        }

        IEnumerator ShakeCoroutine()
        {
            if (_statusText == null) yield break;
            _statusText.color = Color.red;
            yield return new WaitForSeconds(0.4f);
            _statusText.color = new Color(0.5f, 0.8f, 1f);
        }

        void SetGameActive(bool active)
        {
            _gameActive = active;
        }

        void RestartGame()
        {
            // Clear grid
            for (int x = 0; x < gridCols; x++)
                for (int y = 0; y < gridRows; y++)
                {
                    _grid[x, y] = -1;
                    bool dark = (x + y) % 2 == 0;
                    Color c = dark ? new Color(0.18f, 0.18f, 0.26f) : new Color(0.2f, 0.2f, 0.28f);
                    UpdateCellVisual(x, y, c);
                }

            _score = 0;
            _movesLeft = movesAllowed;
            RefillQueue();
            UpdateQueueVisuals();
            UpdateHUD();
            SetGameActive(true);
            ShowStatus("Game restarted! Click a piece, then a slot arrow.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        float GridOriginX() => -(gridCols * cellSize) / 2f + cellSize / 2f;
        float GridOriginY() => -(gridRows * cellSize) / 2f + cellSize / 2f;

        GameObject MakeQuad(string name, Vector3 pos, Vector2 size, Color color, int sortOrder)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _whiteSprite ?? (_whiteSprite = MakeWhiteSprite());
            sr.color = color;
            sr.sortingOrder = sortOrder;
            return go;
        }

        GameObject MakeTextMesh(string name, Vector3 pos, string text, float charSize, Color color)
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

        Sprite MakeWhiteSprite()
        {
            var tex = new Texture2D(4, 4);
            var px = new Color[16];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }
    }

    // ── Trigger helpers (nested so they don't conflict with main game) ─────────

    internal class SlotTrigger : MonoBehaviour
    {
        private BlockPuzzleGame.Dir _dir;
        private int _idx;
        private BlockPuzzleGame _game;

        public void Init(BlockPuzzleGame.Dir dir, int idx, BlockPuzzleGame game)
        {
            _dir = dir; _idx = idx; _game = game;
        }

        void OnMouseDown() => _game?.OnSlotClicked(_dir, _idx);

        void OnMouseEnter()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.5f, 1f, 0.5f, 0.7f);
        }

        void OnMouseExit()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.3f, 0.9f, 0.3f, 0.4f);
        }
    }

    internal class QueueTrigger : MonoBehaviour
    {
        private int _idx;
        private BlockPuzzleGame _game;

        public void Init(int idx, BlockPuzzleGame game) { _idx = idx; _game = game; }

        void OnMouseDown() => _game?.OnQueueSlotClicked(_idx);

        void OnMouseEnter()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(1f, 1f, 0.4f, 0.5f);
        }

        void OnMouseExit()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.15f, 0.15f, 0.25f, 0.8f);
        }
    }
}
