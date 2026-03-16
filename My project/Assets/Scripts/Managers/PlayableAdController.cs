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
            brandGO.transform.position = new Vector3(0, 1.2f, -1f);
            var tm = brandGO.AddComponent<TextMesh>();
            tm.text = brandName;
            tm.fontSize = 36;
            tm.characterSize = 0.12f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = brandAccent;

            // Tagline
            var tagGO = new GameObject("Tagline");
            tagGO.transform.SetParent(_loadingScreen.transform);
            tagGO.transform.position = new Vector3(0, 0.4f, -1f);
            var tag = tagGO.AddComponent<TextMesh>();
            tag.text = tagline;
            tag.fontSize = 20;
            tag.characterSize = 0.07f;
            tag.anchor = TextAnchor.MiddleCenter;
            tag.color = new Color(0.9f, 0.9f, 0.9f);

            // Loading bar background
            var barBG = MakeQuad("BarBG", new Vector3(0, -0.8f, -1f),
                new Vector2(4f, 0.18f), new Color(0.2f, 0.2f, 0.25f), 52);
            barBG.transform.SetParent(_loadingScreen.transform);

            // Loading bar fill
            var barFill = MakeQuad("BarFill", new Vector3(-2f, -0.8f, -1.1f),
                new Vector2(0.01f, 0.14f), brandAccent, 53);
            barFill.transform.SetParent(_loadingScreen.transform);
            barFill.name = "LoadingBarFill";

            // "Loading..." text
            var loadTxt = new GameObject("LoadingTxt");
            loadTxt.transform.SetParent(_loadingScreen.transform);
            loadTxt.transform.position = new Vector3(0, -1.3f, -1f);
            var ltm = loadTxt.AddComponent<TextMesh>();
            ltm.text = "Loading...";
            ltm.fontSize = 16;
            ltm.characterSize = 0.055f;
            ltm.anchor = TextAnchor.MiddleCenter;
            ltm.color = new Color(0.5f, 0.5f, 0.6f);
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
                fill.transform.position = new Vector3(-2f + w / 2f, -0.8f, -1.1f);
                yield return null;
            }
        }

        // ── Endcard Screen ────────────────────────────────────────────────────

        private void BuildEndcardScreen()
        {
            _endcardScreen = new GameObject("EndcardScreen");

            // Overlay (semi-transparent)
            var overlay = MakeQuad("EndBG", Vector3.back * 0.5f, new Vector2(25, 18),
                new Color(brandPrimary.r, brandPrimary.g, brandPrimary.b, 0.93f), 60);
            overlay.transform.SetParent(_endcardScreen.transform);

            // Brand name large
            var bGO = new GameObject("EndBrand");
            bGO.transform.SetParent(_endcardScreen.transform);
            bGO.transform.position = new Vector3(0, 2.5f, -2f);
            var btm = bGO.AddComponent<TextMesh>();
            btm.text = brandName;
            btm.fontSize = 44;
            btm.characterSize = 0.13f;
            btm.anchor = TextAnchor.MiddleCenter;
            btm.color = brandAccent;

            // Score display
            var scoreGO = new GameObject("EndScore");
            scoreGO.transform.SetParent(_endcardScreen.transform);
            scoreGO.transform.position = new Vector3(0, 1.2f, -2f);
            var stm = scoreGO.AddComponent<TextMesh>();
            stm.text = "Score: 0";
            stm.fontSize = 28;
            stm.characterSize = 0.09f;
            stm.anchor = TextAnchor.MiddleCenter;
            stm.color = Color.white;
            scoreGO.name = "EndScoreText";

            // Tagline
            var tGO = new GameObject("EndTagline");
            tGO.transform.SetParent(_endcardScreen.transform);
            tGO.transform.position = new Vector3(0, 0.3f, -2f);
            var ttm = tGO.AddComponent<TextMesh>();
            ttm.text = tagline;
            ttm.fontSize = 22;
            ttm.characterSize = 0.075f;
            ttm.anchor = TextAnchor.MiddleCenter;
            ttm.color = new Color(0.85f, 0.85f, 0.85f);

            // CTA Button
            var ctaBtnBG = MakeQuad("CTABtn", new Vector3(0, -0.8f, -2.1f),
                new Vector2(3.5f, 0.75f), brandAccent, 62);
            ctaBtnBG.transform.SetParent(_endcardScreen.transform);
            ctaBtnBG.AddComponent<BoxCollider2D>().size = new Vector2(3.5f, 0.75f);
            var ctaTrigger = ctaBtnBG.AddComponent<CTAButtonTrigger>();
            ctaTrigger.Init(this);

            var ctaLbl = new GameObject("CTALabel");
            ctaLbl.transform.SetParent(ctaBtnBG.transform);
            ctaLbl.transform.localPosition = new Vector3(0, 0, -0.1f);
            var ctm = ctaLbl.AddComponent<TextMesh>();
            ctm.text = ctaText;
            ctm.fontSize = 22;
            ctm.characterSize = 0.065f;
            ctm.anchor = TextAnchor.MiddleCenter;
            ctm.color = brandPrimary;

            // Play Again
            var replayBtn = MakeQuad("ReplayBtn", new Vector3(0, -1.9f, -2.1f),
                new Vector2(2.5f, 0.55f), new Color(0.25f, 0.25f, 0.35f), 62);
            replayBtn.transform.SetParent(_endcardScreen.transform);
            replayBtn.AddComponent<BoxCollider2D>().size = new Vector2(2.5f, 0.55f);
            var replayTrigger = replayBtn.AddComponent<ReplayButtonTrigger>();
            replayTrigger.Init(this);

            var replayLbl = new GameObject("ReplayLabel");
            replayLbl.transform.SetParent(replayBtn.transform);
            replayLbl.transform.localPosition = new Vector3(0, 0, -0.1f);
            var rtm = replayLbl.AddComponent<TextMesh>();
            rtm.text = "PLAY AGAIN";
            rtm.fontSize = 18;
            rtm.characterSize = 0.055f;
            rtm.anchor = TextAnchor.MiddleCenter;
            rtm.color = new Color(0.8f, 0.8f, 0.9f);

            // Note
            var noteGO = new GameObject("Note");
            noteGO.transform.SetParent(_endcardScreen.transform);
            noteGO.transform.position = new Vector3(0, -3f, -2f);
            var ntm = noteGO.AddComponent<TextMesh>();
            ntm.text = "This is a playable advertisement.";
            ntm.fontSize = 14;
            ntm.characterSize = 0.045f;
            ntm.anchor = TextAnchor.MiddleCenter;
            ntm.color = new Color(0.4f, 0.4f, 0.5f);

            _endcardScreen.SetActive(false);
        }

        public void ShowEndcard(int score)
        {
            CurrentPhase = AdPhase.Endcard;
            _forcedEndcard = true;
            _finalScore = score;
            _endcardScreen?.SetActive(true);

            // Update score text
            var scoreGO = GameObject.Find("EndScoreText");
            if (scoreGO != null)
            {
                var tm = scoreGO.GetComponent<TextMesh>();
                if (tm != null) tm.text = $"Score: {score}";
            }

            TweenHelper.ScaleTo(_endcardScreen, Vector3.one, 0.4f);
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
