using UnityEngine;
using System.Collections;

namespace ColorDrive
{
    /// <summary>
    /// Playable Ad 전체 흐름 관리: Loading → Gameplay → Endcard
    ///
    /// 흐름:
    ///   [LOADING]  브랜드 로고 + 로딩 바 (1.5초)
    ///              ↓
    ///   [GAMEPLAY] BlockPuzzleGame 실행
    ///              ↓ (레벨 클리어 or 시간 초과)
    ///   [ENDCARD]  브랜드 CTA + 점수 + 인스톨 버튼
    /// </summary>
    public class PlayableAdController : MonoBehaviour
    {
        public enum AdPhase { Loading, Gameplay, Endcard }

        [Header("Timing")]
        [SerializeField] private float loadingDuration = 1.5f;
        [SerializeField] private float gameplayTimeLimit = 30f; // seconds until forced endcard

        [Header("Brand")]
        [SerializeField] private string brandName = "Mercedes-Benz";
        [SerializeField] private string tagline = "Drive in Color.";
        [SerializeField] private string ctaText = "DISCOVER MORE";
        [SerializeField] private Color brandPrimary = new Color(0.15f, 0.15f, 0.15f);
        [SerializeField] private Color brandAccent = new Color(0.8f, 0.75f, 0.55f); // gold

        public AdPhase CurrentPhase { get; private set; } = AdPhase.Loading;

        // References
        private BlockPuzzleGame _game;
        private Sprite _white;

        // Screens
        private GameObject _loadingScreen;
        private GameObject _endcardScreen;

        // Score tracking
        private int _finalScore;
        private float _elapsed;
        private bool _forcedEndcard = false;
        private bool _gameEndedByPlayer = false;

        // Timer bar
        private GameObject _timerBarFill;

        void Awake()
        {
            _white = MakeWhite();
        }

        void Start()
        {
            _game = FindFirstObjectByType<BlockPuzzleGame>();

            // Subscribe to game-end event
            if (_game != null)
                _game.OnGameEnded += OnBlockPuzzleGameEnded;

            BuildLoadingScreen();
            BuildEndcardScreen();

            StartCoroutine(RunAdFlow());
        }

        void OnDestroy()
        {
            if (_game != null)
                _game.OnGameEnded -= OnBlockPuzzleGameEnded;
        }

        private void OnBlockPuzzleGameEnded(int score)
        {
            _finalScore = score;
            _gameEndedByPlayer = true;
        }

        void Update()
        {
            if (CurrentPhase != AdPhase.Gameplay) return;
            _elapsed += Time.deltaTime;
            if (!_forcedEndcard && _elapsed >= gameplayTimeLimit)
            {
                _forcedEndcard = true;
            }

            // Update gameplay timer bar
            if (_timerBarFill != null)
            {
                float pct = 1f - Mathf.Clamp01(_elapsed / gameplayTimeLimit);
                _timerBarFill.transform.localScale = new Vector3(12f * pct, 0.21f, 1f);
                _timerBarFill.transform.position = new Vector3(-6f + (12f * pct) / 2f, -6.5f, -1.1f);
                // Color shift: blue → yellow → red
                Color tc = pct > 0.5f ? Color.Lerp(new Color(1f, 0.8f, 0f), new Color(0.4f, 0.8f, 1f), (pct - 0.5f) * 2f)
                                      : Color.Lerp(Color.red, new Color(1f, 0.8f, 0f), pct * 2f);
                var sr = _timerBarFill.GetComponent<SpriteRenderer>();
                if (sr) sr.color = tc;
            }
        }

        // ── Flow ─────────────────────────────────────────────────────────────

        private IEnumerator RunAdFlow()
        {
            // LOADING
            CurrentPhase = AdPhase.Loading;
            _loadingScreen?.SetActive(true);
            _endcardScreen?.SetActive(false);
            yield return StartCoroutine(AnimateLoadingBar(loadingDuration));

            // GAMEPLAY
            CurrentPhase = AdPhase.Gameplay;
            _loadingScreen?.SetActive(false);
            BuildGameplayTimerBar();
            _elapsed = 0f;

            // Wait until player completes the game or time runs out
            yield return StartCoroutine(WaitForGameEnd());

            // ENDCARD — use score from event if available, else read current score
            ShowEndcard(GetScore());
        }

        private IEnumerator WaitForGameEnd()
        {
            // Wait until time limit OR player ends the game naturally
            while (!_forcedEndcard && !_gameEndedByPlayer)
                yield return null;
        }

        private int GetScore() => _game != null ? _game.Score : _finalScore;

        // ── Loading Screen ────────────────────────────────────────────────────

        private void BuildLoadingScreen()
        {
            _loadingScreen = new GameObject("LoadingScreen");

            // Full-screen dark overlay
            var bg = MakeQuad("LoadBG", Vector3.zero, new Vector2(25, 18),
                brandPrimary, 50);
            bg.transform.SetParent(_loadingScreen.transform);

            // Brand name text
            var brandGO = new GameObject("BrandName");
            brandGO.transform.SetParent(_loadingScreen.transform);
            brandGO.transform.position = new Vector3(0, 1.8f, -1f);
            var tm = brandGO.AddComponent<TextMesh>();
            tm.text = brandName;
            tm.fontSize = 48;
            tm.characterSize = 0.18f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = brandAccent;
            brandGO.GetComponent<MeshRenderer>().sortingOrder = 55;

            // Thin horizontal divider between brand name and tagline
            var divider = MakeQuad("Divider", new Vector3(0, 1.1f, -1f), new Vector2(3f, 0.02f), brandAccent, 52);
            divider.transform.SetParent(_loadingScreen.transform);

            // Tagline
            var tagGO = new GameObject("Tagline");
            tagGO.transform.SetParent(_loadingScreen.transform);
            tagGO.transform.position = new Vector3(0, 0.6f, -1f);
            var tag = tagGO.AddComponent<TextMesh>();
            tag.text = tagline;
            tag.fontSize = 28;
            tag.characterSize = 0.10f;
            tag.anchor = TextAnchor.MiddleCenter;
            tag.color = new Color(0.9f, 0.9f, 0.9f);
            tagGO.GetComponent<MeshRenderer>().sortingOrder = 55;

            // Loading bar background
            var barBG = MakeQuad("BarBG", new Vector3(0, -1.2f, -1f),
                new Vector2(4f, 0.22f), new Color(0.2f, 0.2f, 0.25f), 52);
            barBG.transform.SetParent(_loadingScreen.transform);

            // Loading bar fill
            var barFill = MakeQuad("BarFill", new Vector3(-2f, -1.2f, -1.1f),
                new Vector2(0.01f, 0.14f), brandAccent, 53);
            barFill.transform.SetParent(_loadingScreen.transform);
            barFill.name = "LoadingBarFill";

            // "Loading..." text
            var loadTxt = new GameObject("LoadingTxt");
            loadTxt.transform.SetParent(_loadingScreen.transform);
            loadTxt.transform.position = new Vector3(0, -1.8f, -1f);
            var ltm = loadTxt.AddComponent<TextMesh>();
            ltm.text = "Loading...";
            ltm.fontSize = 16;
            ltm.characterSize = 0.065f;
            ltm.anchor = TextAnchor.MiddleCenter;
            ltm.color = new Color(0.5f, 0.5f, 0.6f);
            loadTxt.GetComponent<MeshRenderer>().sortingOrder = 55;
        }

        private IEnumerator AnimateLoadingBar(float duration)
        {
            var fill = GameObject.Find("LoadingBarFill");
            if (fill == null) { yield return new WaitForSeconds(duration); yield break; }

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float pct = Mathf.Clamp01(t / duration);
                float w = 4f * pct;
                fill.transform.localScale = new Vector3(w, 0.14f, 1f);
                fill.transform.position = new Vector3(-2f + w / 2f, -1.2f, -1.1f);
                yield return null;
            }
        }

        // ── Gameplay Timer Bar ────────────────────────────────────────────────

        void BuildGameplayTimerBar()
        {
            var barBG = MakeQuad("TimerBG", new Vector3(0, -6.5f, -1f), new Vector2(12f, 0.25f),
                new Color(0.1f, 0.1f, 0.15f), 20);
            var barFill = MakeQuad("TimerFill", new Vector3(0, -6.5f, -1.1f), new Vector2(12f, 0.21f),
                new Color(0.4f, 0.8f, 1f), 21);
            _timerBarFill = barFill;
        }

        // ── Endcard Screen ────────────────────────────────────────────────────

        private void BuildEndcardScreen()
        {
            _endcardScreen = new GameObject("EndcardScreen");

            // ── Dark gradient overlay ────────────────────────────────────────
            var overlay = MakeQuad("EndBG", Vector3.back * 0.5f, new Vector2(25, 18),
                new Color(0.05f, 0.02f, 0.12f, 0.97f), 60);
            overlay.transform.SetParent(_endcardScreen.transform);

            // ── Decorative top bar (gold line) ───────────────────────────────
            MakeQuad("TopBar", new Vector3(0, 4.2f, -1.5f), new Vector2(12f, 0.04f),
                brandAccent, 61).transform.SetParent(_endcardScreen.transform);
            MakeQuad("BottomBar", new Vector3(0, -4.2f, -1.5f), new Vector2(12f, 0.04f),
                brandAccent, 61).transform.SetParent(_endcardScreen.transform);

            // ── Center panel (slightly lighter bg card) ──────────────────────
            var panel = MakeQuad("Panel", new Vector3(0, 0.5f, -1f), new Vector2(8f, 8.5f),
                new Color(0.10f, 0.06f, 0.22f, 1f), 61);
            panel.transform.SetParent(_endcardScreen.transform);

            // Panel border (gold outline simulation — 4 thin quads)
            var pBorderColors = new Color(brandAccent.r, brandAccent.g, brandAccent.b, 0.6f);
            MakeQuad("PBorderT", new Vector3(0, 4.78f, -1.1f), new Vector2(8.06f, 0.06f), pBorderColors, 62).transform.SetParent(_endcardScreen.transform);
            MakeQuad("PBorderB", new Vector3(0, -3.78f, -1.1f), new Vector2(8.06f, 0.06f), pBorderColors, 62).transform.SetParent(_endcardScreen.transform);
            MakeQuad("PBorderL", new Vector3(-4.03f, 0.5f, -1.1f), new Vector2(0.06f, 8.56f), pBorderColors, 62).transform.SetParent(_endcardScreen.transform);
            MakeQuad("PBorderR", new Vector3(4.03f, 0.5f, -1.1f), new Vector2(0.06f, 8.56f), pBorderColors, 62).transform.SetParent(_endcardScreen.transform);

            // ── Brand name ───────────────────────────────────────────────────
            // Glow behind brand text
            MakeQuad("BrandGlow", new Vector3(0, 3.5f, -1.05f), new Vector2(7f, 0.9f),
                new Color(brandAccent.r, brandAccent.g, brandAccent.b, 0.15f), 62).transform.SetParent(_endcardScreen.transform);

            var bGO = new GameObject("EndBrand");
            bGO.transform.SetParent(_endcardScreen.transform);
            bGO.transform.position = new Vector3(0, 3.5f, -2f);
            var btm = bGO.AddComponent<TextMesh>();
            btm.text = brandName;
            btm.fontSize = 48;
            btm.characterSize = 0.17f;
            btm.anchor = TextAnchor.MiddleCenter;
            btm.fontStyle = FontStyle.Bold;
            btm.color = brandAccent;
            bGO.GetComponent<MeshRenderer>().sortingOrder = 65;

            // ── Gold divider ─────────────────────────────────────────────────
            MakeQuad("BrandDivider", new Vector3(0, 2.85f, -1.5f), new Vector2(4f, 0.025f),
                brandAccent, 63).transform.SetParent(_endcardScreen.transform);

            // ── Tagline ──────────────────────────────────────────────────────
            var tGO = new GameObject("EndTagline");
            tGO.transform.SetParent(_endcardScreen.transform);
            tGO.transform.position = new Vector3(0, 2.45f, -2f);
            var ttm = tGO.AddComponent<TextMesh>();
            ttm.text = tagline;
            ttm.fontSize = 22;
            ttm.characterSize = 0.075f;
            ttm.anchor = TextAnchor.MiddleCenter;
            ttm.color = new Color(0.9f, 0.88f, 0.82f);
            tGO.GetComponent<MeshRenderer>().sortingOrder = 65;

            // ── Score badge ──────────────────────────────────────────────────
            // Outer badge
            MakeQuad("BadgeOuter", new Vector3(0, 1.1f, -1.5f), new Vector2(4.5f, 1.6f),
                new Color(brandAccent.r * 0.4f, brandAccent.g * 0.4f, brandAccent.b * 0.25f, 1f), 62).transform.SetParent(_endcardScreen.transform);
            // Inner badge shine
            MakeQuad("BadgeInner", new Vector3(0, 1.35f, -1.6f), new Vector2(4.2f, 0.45f),
                new Color(1f, 1f, 1f, 0.06f), 63).transform.SetParent(_endcardScreen.transform);
            // Badge border
            MakeQuad("BadgeBorder", new Vector3(0, 1.1f, -1.55f), new Vector2(4.56f, 1.66f),
                new Color(brandAccent.r, brandAccent.g, brandAccent.b, 0.5f), 62).transform.SetParent(_endcardScreen.transform);

            var scoreGO = new GameObject("EndScore");
            scoreGO.transform.SetParent(_endcardScreen.transform);
            scoreGO.transform.position = new Vector3(0, 1.1f, -2f);
            var stm = scoreGO.AddComponent<TextMesh>();
            stm.text = "SCORE  0";
            stm.fontSize = 32;
            stm.characterSize = 0.13f;
            stm.anchor = TextAnchor.MiddleCenter;
            stm.fontStyle = FontStyle.Bold;
            stm.color = Color.white;
            scoreGO.GetComponent<MeshRenderer>().sortingOrder = 65;
            scoreGO.name = "EndScoreText";

            // ── CTA Button ───────────────────────────────────────────────────
            // Button outer glow
            MakeQuad("CTAGlow", new Vector3(0, -0.55f, -1.5f), new Vector2(4.2f, 1.0f),
                new Color(brandAccent.r, brandAccent.g, brandAccent.b, 0.25f), 62).transform.SetParent(_endcardScreen.transform);

            var ctaBtnBG = MakeQuad("CTABtn", new Vector3(0, -0.55f, -2.1f),
                new Vector2(3.8f, 0.78f), brandAccent, 63);
            ctaBtnBG.transform.SetParent(_endcardScreen.transform);
            ctaBtnBG.AddComponent<BoxCollider2D>().size = new Vector2(3.8f, 0.78f);
            var ctaTrigger = ctaBtnBG.AddComponent<CTAButtonTrigger>();
            ctaTrigger.Init(this);

            var ctaLbl = new GameObject("CTALabel");
            ctaLbl.transform.SetParent(ctaBtnBG.transform);
            ctaLbl.transform.localPosition = new Vector3(0, 0, -0.1f);
            var ctm = ctaLbl.AddComponent<TextMesh>();
            ctm.text = ctaText;
            ctm.fontSize = 24;
            ctm.characterSize = 0.065f;
            ctm.anchor = TextAnchor.MiddleCenter;
            ctm.fontStyle = FontStyle.Bold;
            ctm.color = new Color(0.05f, 0.02f, 0.12f);
            ctaLbl.GetComponent<MeshRenderer>().sortingOrder = 67;

            // ── Play Again Button ─────────────────────────────────────────────
            var replayBtn = MakeQuad("ReplayBtn", new Vector3(0, -1.65f, -2.1f),
                new Vector2(2.8f, 0.58f), new Color(0.18f, 0.12f, 0.32f), 63);
            replayBtn.transform.SetParent(_endcardScreen.transform);
            replayBtn.AddComponent<BoxCollider2D>().size = new Vector2(2.8f, 0.58f);
            // Replay btn border
            MakeQuad("ReplayBorder", new Vector3(0, -1.65f, -2.05f), new Vector2(2.86f, 0.64f),
                new Color(0.5f, 0.4f, 0.7f, 0.6f), 62).transform.SetParent(_endcardScreen.transform);
            var replayTrigger = replayBtn.AddComponent<ReplayButtonTrigger>();
            replayTrigger.Init(this);

            var replayLbl = new GameObject("ReplayLabel");
            replayLbl.transform.SetParent(replayBtn.transform);
            replayLbl.transform.localPosition = new Vector3(0, 0, -0.1f);
            var rtm = replayLbl.AddComponent<TextMesh>();
            rtm.text = "PLAY AGAIN";
            rtm.fontSize = 20;
            rtm.characterSize = 0.058f;
            rtm.anchor = TextAnchor.MiddleCenter;
            rtm.color = new Color(0.85f, 0.82f, 1f);
            replayLbl.GetComponent<MeshRenderer>().sortingOrder = 67;

            // ── Disclaimer ───────────────────────────────────────────────────
            var noteGO = new GameObject("Note");
            noteGO.transform.SetParent(_endcardScreen.transform);
            noteGO.transform.position = new Vector3(0, -3.2f, -2f);
            var ntm = noteGO.AddComponent<TextMesh>();
            ntm.text = "This is a playable advertisement.";
            ntm.fontSize = 14;
            ntm.characterSize = 0.042f;
            ntm.anchor = TextAnchor.MiddleCenter;
            ntm.color = new Color(0.35f, 0.32f, 0.45f);
            noteGO.GetComponent<MeshRenderer>().sortingOrder = 65;

            _endcardScreen.SetActive(false);
            _endcardScreen.transform.localScale = Vector3.zero;
        }

        public void ShowEndcard(int score)
        {
            CurrentPhase = AdPhase.Endcard;
            _forcedEndcard = true;
            _finalScore = score;
            _endcardScreen?.SetActive(true);
            TweenHelper.ScaleTo(_endcardScreen, Vector3.one, 0.5f);

            // Update score text
            var scoreGO = GameObject.Find("EndScoreText");
            if (scoreGO != null)
            {
                var tm = scoreGO.GetComponent<TextMesh>();
                if (tm != null) tm.text = $"SCORE  {score}";
            }
        }

        public void OnCTAPressed()
        {
            Debug.Log($"[PlayableAd] CTA pressed — would open: https://www.mercedes-benz.com");
            Application.OpenURL("https://www.mercedes-benz.com");
        }

        public void OnReplayPressed()
        {
            _forcedEndcard = false;
            _elapsed = 0f;
            _endcardScreen?.SetActive(false);
            CurrentPhase = AdPhase.Gameplay;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private GameObject MakeQuad(string name, Vector3 pos, Vector2 size, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _white;
            sr.color = color;
            sr.sortingOrder = order;
            return go;
        }

        private Sprite MakeWhite()
        {
            var t = new Texture2D(4, 4);
            var p = new Color[16];
            for (int i = 0; i < p.Length; i++) p[i] = Color.white;
            t.SetPixels(p); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }
    }

    // ── Button Triggers ───────────────────────────────────────────────────────

    internal class CTAButtonTrigger : MonoBehaviour
    {
        private PlayableAdController _ctrl;
        public void Init(PlayableAdController c) { _ctrl = c; }
        void OnMouseDown() => _ctrl?.OnCTAPressed();
        void OnMouseEnter()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.white;
        }
        void OnMouseExit()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.8f, 0.75f, 0.55f);
        }
    }

    internal class ReplayButtonTrigger : MonoBehaviour
    {
        private PlayableAdController _ctrl;
        public void Init(PlayableAdController c) { _ctrl = c; }
        void OnMouseDown() => _ctrl?.OnReplayPressed();
        void OnMouseEnter()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.4f, 0.4f, 0.55f);
        }
        void OnMouseExit()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.25f, 0.25f, 0.35f);
        }
    }
}
