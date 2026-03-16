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
            new Color(0.91f, 0.00f, 0.18f),  // Candy Red    #E8002D
            new Color(1.00f, 0.47f, 0.00f),  // Vivid Orange #FF7800
            new Color(0.96f, 0.76f, 0.00f),  // Vivid Yellow #F5C200
            new Color(0.00f, 0.77f, 0.31f),  // Vivid Green  #00C44F
            new Color(0.10f, 0.50f, 1.00f),  // Royal Blue   #1A7FFF
            new Color(0.61f, 0.15f, 0.69f),  // Vivid Purple #9B27AF
            new Color(0.00f, 0.85f, 0.90f),  // Teal Cyan    #00D9E6
        };

        private Color[] HIGHLIGHT_COLORS = new Color[]
        {
            new Color(1.00f, 0.48f, 0.54f),  // Red highlight
            new Color(1.00f, 0.75f, 0.48f),  // Orange highlight
            new Color(1.00f, 0.89f, 0.48f),  // Yellow highlight
            new Color(0.48f, 1.00f, 0.70f),  // Green highlight
            new Color(0.48f, 0.75f, 1.00f),  // Blue highlight
            new Color(0.82f, 0.48f, 0.91f),  // Purple highlight
            new Color(0.48f, 1.00f, 1.00f),  // Cyan highlight
        };

        private Color[] SHADOW_COLORS = new Color[]
        {
            new Color(0.55f, 0.00f, 0.10f),  // Red shadow
            new Color(0.60f, 0.28f, 0.00f),  // Orange shadow
            new Color(0.58f, 0.46f, 0.00f),  // Yellow shadow
            new Color(0.00f, 0.46f, 0.19f),  // Green shadow
            new Color(0.06f, 0.25f, 0.67f),  // Blue shadow
            new Color(0.36f, 0.09f, 0.41f),  // Purple shadow
            new Color(0.00f, 0.50f, 0.55f),  // Cyan shadow
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
        private Sprite _roundedSprite;
        private Sprite[] _pieceSprites;  // per-color 3D gem sprites

        // ── Slot pulse state ──────────────────────────────────────────────────
        private List<SpriteRenderer> _slotRenderers = new List<SpriteRenderer>();
        private Coroutine _slotPulseCoroutine;
        private readonly Color _slotDefaultColor = new Color(0.3f, 0.9f, 0.3f, 0.35f);
        private readonly Color _slotActiveColor  = new Color(0.2f, 1.0f, 0.2f, 0.85f);

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
            _roundedSprite = MakeRoundedSprite();
            _pieceSprites = new Sprite[COLORS.Length];
            for (int i = 0; i < COLORS.Length; i++)
                _pieceSprites[i] = MakePieceSprite(COLORS[i], HIGHLIGHT_COLORS[i], SHADOW_COLORS[i]);
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

            // Auto-select piece 0 so player knows what to do
            _selectedPiece = 0;
            HighlightQueueSlot(0);
            ShowStatus("Place the highlighted piece \u2192 click any green arrow");
            StartSlotPulse();
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
            cam.orthographicSize = 5.8f;
            cam.transform.position = new Vector3(0, 0, -10);
            cam.backgroundColor = new Color(0.10f, 0.04f, 0.18f);
        }

        void SetupBackground()
        {
            MakeQuad("Background", Vector3.forward * 2f, new Vector2(25f, 18f),
                new Color(0.10f, 0.04f, 0.18f), -10);
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

            // Grid panel background — royal purple
            float gw = gridCols * cellSize;
            float gh = gridRows * cellSize;
            MakeQuad("GridPanel", new Vector3(0, 0, 0.5f), new Vector2(gw + 0.4f, gh + 0.4f),
                new Color(0.165f, 0.122f, 0.306f), -2);
            // Grid panel inner shadow (slightly darker, slightly smaller)
            MakeQuad("GridPanelInner", new Vector3(0, 0, 0.4f), new Vector2(gw + 0.1f, gh + 0.1f),
                new Color(0.118f, 0.086f, 0.251f), -1);

            for (int x = 0; x < gridCols; x++)
                for (int y = 0; y < gridRows; y++)
                {
                    float px = ox + x * cellSize;
                    float py = oy + y * cellSize;
                    // Inset cell slot — dark purple base
                    Color c = new Color(0.118f, 0.086f, 0.251f);
                    var cell = MakeQuad($"C{x},{y}", new Vector3(px, py, 0.2f),
                        Vector2.one * (cellSize - 0.06f), c, -1);
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
                var slotGO = MakeSlot($"SL{y}", new Vector3(-hw - off, py, -0.2f), "\u2192",
                    _slotDefaultColor, Dir.Left, y);
                var sr = slotGO.GetComponent<SpriteRenderer>();
                if (sr != null) _slotRenderers.Add(sr);
            }
            // Right (←)
            for (int y = 0; y < gridRows; y++)
            {
                float py = oy + y * cellSize;
                var slotGO = MakeSlot($"SR{y}", new Vector3(hw + off, py, -0.2f), "\u2190",
                    _slotDefaultColor, Dir.Right, y);
                var sr = slotGO.GetComponent<SpriteRenderer>();
                if (sr != null) _slotRenderers.Add(sr);
            }
            // Bottom (↑)
            for (int x = 0; x < gridCols; x++)
            {
                float px = ox + x * cellSize;
                var slotGO = MakeSlot($"SB{x}", new Vector3(px, -hh - off, -0.2f), "\u2191",
                    new Color(0.9f, 0.6f, 0.3f, 0.35f), Dir.Down, x);
                // Bottom/Top slots use orange palette — don't add to pulse list
                _ = slotGO;
            }
            // Top (↓)
            for (int x = 0; x < gridCols; x++)
            {
                float px = ox + x * cellSize;
                var slotGO = MakeSlot($"ST{x}", new Vector3(px, hh + off, -0.2f), "\u2193",
                    new Color(0.9f, 0.6f, 0.3f, 0.35f), Dir.Up, x);
                _ = slotGO;
            }
        }

        GameObject MakeSlot(string name, Vector3 pos, string arrow, Color color, Dir dir, int idx)
        {
            var go = MakeQuad(name, pos, Vector2.one * (cellSize * 0.85f), color, 1);
            go.AddComponent<BoxCollider2D>().size = Vector2.one * (cellSize * 0.85f);

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

            return go;
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
                    Vector2.one * 1.8f, new Color(0.165f, 0.122f, 0.306f, 0.9f), 0);
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
                "COLOR DRIVE", 0.09f, new Color(1f, 0.9f, 0.4f));

            // Score
            var scoreGO = MakeTextMesh("Score", new Vector3(-hw - 0.2f, hh + 1.6f, 0),
                "SCORE\n0", 0.075f, Color.white);
            _scoreText = scoreGO.GetComponent<TextMesh>();

            // Moves
            var movesGO = MakeTextMesh("Moves", new Vector3(hw + 0.2f, hh + 1.6f, 0),
                $"MOVES\n{movesAllowed}", 0.075f, new Color(1f, 0.9f, 0.4f));
            _movesText = movesGO.GetComponent<TextMesh>();

            // Status/instruction
            var statusGO = MakeTextMesh("Status",
                new Vector3(0, -(hh + gridRows * cellSize / 2f + 3.9f), 0),
                "", 0.048f, new Color(0.7f, 0.85f, 1f));
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
            ShowStatus($"Place piece {idx + 1} \u2192 click a slot arrow");
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

            // Stop pulse while placing
            StopSlotPulse();

            // Punch scale on placed cells
            foreach (var c in worldCells)
            {
                var cellGO = _cellVis[c.x, c.y];
                TweenHelper.ScaleTo(cellGO, Vector3.one * 1.25f, 0.08f, () =>
                    TweenHelper.ScaleTo(cellGO, Vector3.one, 0.12f));
            }

            _movesLeft--;
            UpdateHUD();

            // Check clears
            int clearedCount = CheckAndClear();
            if (clearedCount > 0)
            {
                int pts = clearedCount * 10 * (1 + clearedCount / 5);
                _score += pts;
                UpdateHUD();
                ShowStatus($"NICE! Cleared {clearedCount} blocks! +{pts}pts");
                ShowFloatingText($"+{pts}", new Vector3(0, 0, 0), new Color(1f, 0.9f, 0.2f));
            }

            // Check win (board clear)
            if (IsBoardClear())
            {
                StopSlotPulse();
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
                StopSlotPulse();
                SetGameActive(false);
                ShowStatus("Out of moves! Score: " + _score + "  [R] to restart");
                OnGameEnded?.Invoke(_score);
                return;
            }

            // Auto-select next piece
            _selectedPiece = 0;
            HighlightQueueSlot(0);
            ShowStatus("Keep placing! Click a green arrow.");
            StartSlotPulse();
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
            sr.color = new Color(0.118f, 0.086f, 0.251f);  // empty color (deep purple)
        }

        private bool IsBoardClear()
        {
            for (int x = 0; x < gridCols; x++)
                for (int y = 0; y < gridRows; y++)
                    if (_grid[x, y] >= 0) return false;
            return true;
        }

        // ── Slot Pulse ────────────────────────────────────────────────────────

        void StartSlotPulse()
        {
            StopSlotPulse();
            _slotPulseCoroutine = StartCoroutine(SlotPulseCoroutine());
        }

        void StopSlotPulse()
        {
            if (_slotPulseCoroutine != null) { StopCoroutine(_slotPulseCoroutine); _slotPulseCoroutine = null; }
            foreach (var sr in _slotRenderers) if (sr != null) sr.color = _slotDefaultColor;
        }

        IEnumerator SlotPulseCoroutine()
        {
            while (true)
            {
                float t = Mathf.PingPong(Time.unscaledTime * 2.5f, 1f);
                float ease = t * t * (3f - 2f * t); // smoothstep
                Color c = Color.Lerp(_slotDefaultColor, _slotActiveColor, ease);
                foreach (var sr in _slotRenderers) if (sr != null) sr.color = c;
                yield return null;
            }
        }

        // ── Floating Score Text ───────────────────────────────────────────────

        void ShowFloatingText(string text, Vector3 pos, Color color)
        {
            StartCoroutine(FloatTextCoroutine(text, pos, color));
        }

        IEnumerator FloatTextCoroutine(string text, Vector3 pos, Color color)
        {
            var go = new GameObject("_FloatText");
            go.transform.position = pos + Vector3.back * 2f;
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 32;
            tm.characterSize = 0.10f;
            tm.anchor = TextAnchor.MiddleCenter;
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 80;

            float dur = 1.4f, elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float pct = elapsed / dur;
                go.transform.position = pos + Vector3.up * (pct * 2.0f) + Vector3.back * 2f;
                tm.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(1.5f - pct * 1.5f));
                yield return null;
            }
            Destroy(go);
        }

        // ── Visuals ───────────────────────────────────────────────────────────

        void UpdateCellVisual(int x, int y, Color color)
        {
            if (_cellVis[x, y] == null) return;
            var sr = _cellVis[x, y].GetComponent<SpriteRenderer>();
            if (sr == null) return;
            // Find color index
            int colorIdx = _grid[x, y];
            if (colorIdx >= 0 && _pieceSprites != null && colorIdx < _pieceSprites.Length)
            {
                sr.sprite = _pieceSprites[colorIdx];
                sr.color = Color.white;
            }
            else
            {
                sr.sprite = _roundedSprite;
                sr.color = color;
            }
        }

        void UpdateQueueVisuals()
        {
            for (int i = 0; i < 3; i++)
            {
                if (_queueVis[i] == null) continue;

                // Clear previous piece visuals
                var sr = _queueVis[i].GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(0.165f, 0.122f, 0.306f, 0.9f);

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
                    var cellSR = cellGO.GetComponent<SpriteRenderer>();
                    if (cellSR != null) { cellSR.color = Color.white; cellSR.sprite = _pieceSprites[piece.colorIdx]; }
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
                        : new Color(0.165f, 0.122f, 0.306f, 0.9f);
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
                    Color c = new Color(0.118f, 0.086f, 0.251f);
                    UpdateCellVisual(x, y, c);
                }

            _score = 0;
            _movesLeft = movesAllowed;
            RefillQueue();
            UpdateQueueVisuals();
            UpdateHUD();
            SetGameActive(true);

            // Auto-select piece 0 on restart
            _selectedPiece = 0;
            HighlightQueueSlot(0);
            ShowStatus("Game restarted! Place the highlighted piece.");
            StartSlotPulse();
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
            sr.sprite = _roundedSprite ?? (_roundedSprite = MakeRoundedSprite());
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

        Sprite MakeRoundedSprite(int size = 64, float cornerPct = 0.25f)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float r = cornerPct * size * 0.5f;
            float cx = size * 0.5f - 0.5f, cy = size * 0.5f - 0.5f;
            float inner = size * 0.5f - r;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(0f, Mathf.Abs(x - cx) - inner);
                float dy = Mathf.Max(0f, Mathf.Abs(y - cy) - inner);
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(r - d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        Sprite MakePieceSprite(Color baseColor, Color highlightColor, Color shadowColor, int size = 64)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[size * size];

            Vector2 center = new Vector2(size * 0.5f - 0.5f, size * 0.5f - 0.5f);
            float outerR  = size * 0.44f;
            float innerR  = size * 0.40f;  // rim darkening starts here
            float glowR   = size * 0.20f;

            // Specular highlight ellipse (upper-left)
            Vector2 hlCenter = center + new Vector2(-size * 0.14f, size * 0.14f);
            float hlRx = size * 0.18f;
            float hlRy = size * 0.13f;

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                Vector2 pos    = new Vector2(x, y);
                Vector2 fromC  = pos - center;
                float dist     = fromC.magnitude;

                if (dist > outerR) { pixels[y * size + x] = new Color32(0,0,0,0); continue; }

                // 1. Base fill
                Color col = baseColor;

                // 2. Directional shadow (bottom-right darkening)
                Vector2 lightDir = new Vector2(0.55f, -0.55f).normalized;
                float shadowDot  = Vector2.Dot(fromC.magnitude > 0f ? fromC.normalized : Vector2.zero, -lightDir);
                col = Color.Lerp(col, shadowColor, Mathf.Clamp01(shadowDot) * 0.55f);

                // 3. Inner glow (center brightness lift)
                float glowT = Mathf.Clamp01(1f - dist / glowR);
                col = Color.Lerp(col, highlightColor, glowT * glowT * 0.40f);

                // 4. Rim darkening
                if (dist >= innerR)
                {
                    float rimT = (dist - innerR) / (outerR - innerR);
                    col = Color.Lerp(col, Color.black, rimT * 0.55f);
                }

                // 5. Specular highlight (upper-left ellipse)
                Vector2 fromHl = pos - hlCenter;
                float hlEllipse = (fromHl.x * fromHl.x) / (hlRx * hlRx)
                                + (fromHl.y * fromHl.y) / (hlRy * hlRy);
                if (hlEllipse <= 1f)
                {
                    float alpha = Mathf.Clamp01(1f - hlEllipse) * 0.72f;
                    col = Color.Lerp(col, Color.white, alpha);
                }

                pixels[y * size + x] = col;
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), (float)size);
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
            if (sr != null) sr.color = new Color(0.165f, 0.122f, 0.306f, 0.9f);
        }
    }
}
