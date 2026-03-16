using UnityEngine;
using UnityEngine.UI;
using LegendChef.Core;
using LegendChef.Progression;
using LegendChef.Visual;
using LegendChef;

namespace LegendChef.UI
{
    /// <summary>
    /// 하단 50% UI: 재화 표시, 업그레이드 버튼 4개, 스테이지 정보, 보스 타이머
    /// </summary>
    public class LegendChefHUD : MonoBehaviour
    {
        public static LegendChefHUD Instance { get; private set; }

        private Canvas _canvas;

        // 재화 텍스트
        private Text _goldText;
        private Text _spiceText;

        // 스테이지 정보 (게임 영역 상단)
        private Text _stageText;

        // 보스 타이머 바
        private GameObject _bossTimerBar;
        private Image _bossTimerFill;
        private Text _bossTimerText;

        // 업그레이드 버튼 & 텍스트
        private Button _atkBtn;
        private Text _atkBtnText;
        private Button _speedBtn;
        private Text _speedBtnText;
        private Button _incomeBtn;
        private Text _incomeBtnText;
        private Button _critBtn;
        private Text _critBtnText;

        // 적 HP 바
        private Image _enemyHPFill;
        private Text _enemyHPText;
        private GameObject _enemyHPBar;

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize()
        {
            // HUD 캔버스
            GameObject canvasGO = new GameObject("HUDCanvas");
            canvasGO.transform.SetParent(transform);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            Transform canvasT = canvasGO.transform;

            // ── 스테이지 정보 (상단) ──
            CreateStageDisplay(canvasT);

            // ── 적 HP 바 (게임 영역 중간) ──
            CreateEnemyHPBar(canvasT);

            // ── 보스 타이머 바 ──
            CreateBossTimerBar(canvasT);

            // ── 하단 UI 패널 (50%) ──
            GameObject bottomPanel = CreateUIElement("BottomPanel", canvasT);
            RectTransform bpRT = bottomPanel.GetComponent<RectTransform>();
            bpRT.anchorMin = new Vector2(0f, 0f);
            bpRT.anchorMax = new Vector2(1f, 0.48f);
            bpRT.offsetMin = Vector2.zero;
            bpRT.offsetMax = Vector2.zero;
            Image bpImg = bottomPanel.AddComponent<Image>();
            bpImg.color = new Color(0.12f, 0.12f, 0.18f, 0.92f);

            // ── 재화 표시 행 ──
            CreateCurrencyRow(bottomPanel.transform);

            // ── 업그레이드 버튼 4개 ──
            CreateUpgradeButtons(bottomPanel.transform);
        }

        private void CreateStageDisplay(Transform parent)
        {
            GameObject stageGO = CreateUIElement("StageDisplay", parent);
            RectTransform rt = stageGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.2f, 0.92f);
            rt.anchorMax = new Vector2(0.8f, 0.98f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = stageGO.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.5f);

            _stageText = CreateTextElement("StageText", stageGO.transform,
                "Stage 1", 36, TextAnchor.MiddleCenter, Color.white);
            SetFullStretch(_stageText.GetComponent<RectTransform>());
        }

        private void CreateEnemyHPBar(Transform parent)
        {
            _enemyHPBar = CreateUIElement("EnemyHPBar", parent);
            RectTransform hpRT = _enemyHPBar.GetComponent<RectTransform>();
            hpRT.anchorMin = new Vector2(0.15f, 0.88f);
            hpRT.anchorMax = new Vector2(0.85f, 0.91f);
            hpRT.offsetMin = Vector2.zero;
            hpRT.offsetMax = Vector2.zero;

            Image hpBg = _enemyHPBar.AddComponent<Image>();
            hpBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // 체력 필 바
            GameObject fillGO = CreateUIElement("HPFill", _enemyHPBar.transform);
            RectTransform fillRT = fillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = new Vector2(2, 2);
            fillRT.offsetMax = new Vector2(-2, -2);
            _enemyHPFill = fillGO.AddComponent<Image>();
            _enemyHPFill.color = new Color(0.85f, 0.2f, 0.2f);
            _enemyHPFill.type = Image.Type.Filled;
            _enemyHPFill.fillMethod = Image.FillMethod.Horizontal;

            _enemyHPText = CreateTextElement("HPText", _enemyHPBar.transform,
                "HP", 22, TextAnchor.MiddleCenter, Color.white);
            SetFullStretch(_enemyHPText.GetComponent<RectTransform>());
        }

        private void CreateBossTimerBar(Transform parent)
        {
            _bossTimerBar = CreateUIElement("BossTimerBar", parent);
            RectTransform rt = _bossTimerBar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.15f, 0.84f);
            rt.anchorMax = new Vector2(0.85f, 0.87f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = _bossTimerBar.AddComponent<Image>();
            bg.color = new Color(0.3f, 0.1f, 0.1f, 0.8f);

            GameObject fillGO = CreateUIElement("TimerFill", _bossTimerBar.transform);
            RectTransform fillRT = fillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = new Vector2(2, 2);
            fillRT.offsetMax = new Vector2(-2, -2);
            _bossTimerFill = fillGO.AddComponent<Image>();
            _bossTimerFill.color = new Color(1f, 0.4f, 0.1f);
            _bossTimerFill.type = Image.Type.Filled;
            _bossTimerFill.fillMethod = Image.FillMethod.Horizontal;

            _bossTimerText = CreateTextElement("TimerText", _bossTimerBar.transform,
                "BOSS", 20, TextAnchor.MiddleCenter, Color.white);
            SetFullStretch(_bossTimerText.GetComponent<RectTransform>());

            _bossTimerBar.SetActive(false);
        }

        private void CreateCurrencyRow(Transform parent)
        {
            GameObject row = CreateUIElement("CurrencyRow", parent);
            RectTransform rowRT = row.GetComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0.02f, 0.82f);
            rowRT.anchorMax = new Vector2(0.98f, 0.98f);
            rowRT.offsetMin = Vector2.zero;
            rowRT.offsetMax = Vector2.zero;

            // 골드 아이콘 + 텍스트
            GameObject goldIcon = CreateUIElement("GoldIcon", row.transform);
            RectTransform giRT = goldIcon.GetComponent<RectTransform>();
            giRT.anchorMin = new Vector2(0.02f, 0.1f);
            giRT.anchorMax = new Vector2(0.10f, 0.9f);
            giRT.offsetMin = Vector2.zero;
            giRT.offsetMax = Vector2.zero;
            Image goldImg = goldIcon.AddComponent<Image>();
            goldImg.sprite = PixelArtGenerator.CreateCoinSprite();
            goldImg.preserveAspect = true;

            _goldText = CreateTextElement("GoldText", row.transform,
                "0", 32, TextAnchor.MiddleLeft, new Color(1f, 0.85f, 0.1f));
            RectTransform gtRT = _goldText.GetComponent<RectTransform>();
            gtRT.anchorMin = new Vector2(0.11f, 0f);
            gtRT.anchorMax = new Vector2(0.45f, 1f);
            gtRT.offsetMin = Vector2.zero;
            gtRT.offsetMax = Vector2.zero;

            // 향신료 아이콘 + 텍스트
            GameObject spiceIcon = CreateUIElement("SpiceIcon", row.transform);
            RectTransform siRT = spiceIcon.GetComponent<RectTransform>();
            siRT.anchorMin = new Vector2(0.52f, 0.1f);
            siRT.anchorMax = new Vector2(0.60f, 0.9f);
            siRT.offsetMin = Vector2.zero;
            siRT.offsetMax = Vector2.zero;
            Image spiceImg = spiceIcon.AddComponent<Image>();
            spiceImg.sprite = PixelArtGenerator.CreateSpiceSprite();
            spiceImg.preserveAspect = true;

            _spiceText = CreateTextElement("SpiceText", row.transform,
                "0", 32, TextAnchor.MiddleLeft, new Color(0.9f, 0.5f, 0.15f));
            RectTransform stRT = _spiceText.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0.61f, 0f);
            stRT.anchorMax = new Vector2(0.98f, 1f);
            stRT.offsetMin = Vector2.zero;
            stRT.offsetMax = Vector2.zero;
        }

        private void CreateUpgradeButtons(Transform parent)
        {
            // 4개 버튼을 2x2 그리드로 배치
            float margin = 0.02f;
            float btnW = 0.46f;
            float btnH = 0.34f;

            // ATK (좌상)
            _atkBtn = CreateUpgradeButton(parent, "ATKBtn",
                new Vector2(margin, 0.44f), new Vector2(margin + btnW, 0.44f + btnH),
                new Color(0.85f, 0.30f, 0.25f), out _atkBtnText);
            _atkBtn.onClick.AddListener(() => { UpgradeSystem.Instance?.TryUpgradeATK(); });

            // SPEED (우상)
            _speedBtn = CreateUpgradeButton(parent, "SPEEDBtn",
                new Vector2(0.52f, 0.44f), new Vector2(0.52f + btnW, 0.44f + btnH),
                new Color(0.20f, 0.60f, 0.85f), out _speedBtnText);
            _speedBtn.onClick.AddListener(() => { UpgradeSystem.Instance?.TryUpgradeSPEED(); });

            // INCOME (좌하)
            _incomeBtn = CreateUpgradeButton(parent, "INCOMEBtn",
                new Vector2(margin, 0.06f), new Vector2(margin + btnW, 0.06f + btnH),
                new Color(0.18f, 0.75f, 0.30f), out _incomeBtnText);
            _incomeBtn.onClick.AddListener(() => { UpgradeSystem.Instance?.TryUpgradeINCOME(); });

            // CRIT (우하)
            _critBtn = CreateUpgradeButton(parent, "CRITBtn",
                new Vector2(0.52f, 0.06f), new Vector2(0.52f + btnW, 0.06f + btnH),
                new Color(0.70f, 0.25f, 0.75f), out _critBtnText);
            _critBtn.onClick.AddListener(() => { UpgradeSystem.Instance?.TryUpgradeCRIT(); });
        }

        private Button CreateUpgradeButton(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Color bgColor, out Text labelText)
        {
            GameObject btnGO = CreateUIElement(name, parent);
            RectTransform rt = btnGO.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = btnGO.AddComponent<Image>();
            img.color = bgColor;

            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;

            // 버튼 색상 전환
            ColorBlock colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = bgColor * 1.1f;
            colors.pressedColor = bgColor * 0.8f;
            colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            btn.colors = colors;

            labelText = CreateTextElement("Label", btnGO.transform,
                name, 24, TextAnchor.MiddleCenter, Color.white);
            SetFullStretch(labelText.GetComponent<RectTransform>(),
                new Vector2(8, 4), new Vector2(-8, -4));

            return btn;
        }

        private void Update()
        {
            UpdateCurrencyDisplay();
            UpdateUpgradeButtons();
            UpdateStageDisplay();
            UpdateEnemyHPBar();
        }

        private void UpdateCurrencyDisplay()
        {
            if (CurrencySystem.Instance == null) return;

            if (_goldText != null)
                _goldText.text = FormatNumber(CurrencySystem.Instance.Gold);
            if (_spiceText != null)
                _spiceText.text = CurrencySystem.Instance.Spice.ToString();
        }

        private void UpdateUpgradeButtons()
        {
            if (UpgradeSystem.Instance == null || CurrencySystem.Instance == null) return;

            var us = UpgradeSystem.Instance;
            var cs = CurrencySystem.Instance;

            // ATK
            if (_atkBtnText != null)
            {
                _atkBtnText.text = string.Format(
                    "ATK Lv.{0}\nDMG: {1:F1}\n{2} Gold",
                    us.AtkLevel, us.CurrentAttack, FormatNumber(us.AtkUpgradeCost));
            }
            if (_atkBtn != null)
                _atkBtn.interactable = cs.CanAffordGold(us.AtkUpgradeCost);

            // SPEED
            if (_speedBtnText != null)
            {
                _speedBtnText.text = string.Format(
                    "SPEED Lv.{0}\nInterval: {1:F2}s\n{2} Gold",
                    us.SpeedLevel, us.CurrentAutoInterval, FormatNumber(us.SpeedUpgradeCost));
            }
            if (_speedBtn != null)
                _speedBtn.interactable = cs.CanAffordGold(us.SpeedUpgradeCost);

            // INCOME
            if (_incomeBtnText != null)
            {
                _incomeBtnText.text = string.Format(
                    "INCOME Lv.{0}\n{1:F1}/s\n{2} Gold",
                    us.IncomeLevel, us.CurrentIncomePerSec, FormatNumber(us.IncomeUpgradeCost));
            }
            if (_incomeBtn != null)
                _incomeBtn.interactable = cs.CanAffordGold(us.IncomeUpgradeCost);

            // CRIT
            if (_critBtnText != null)
            {
                _critBtnText.text = string.Format(
                    "CRIT Lv.{0}\n{1:F0}% x{2:F1}\n{3} Spice",
                    us.CritLevel, us.CurrentCritChance * 100f, us.CurrentCritMultiplier, us.CritUpgradeCost);
            }
            if (_critBtn != null)
                _critBtn.interactable = cs.CanAffordSpice(us.CritUpgradeCost);
        }

        private void UpdateStageDisplay()
        {
            if (StageManager.Instance == null || _stageText == null) return;

            int stage = StageManager.Instance.CurrentStage;
            bool isBoss = StageManager.Instance.IsBossStage;
            _stageText.text = isBoss
                ? string.Format("Stage {0} - BOSS!", stage)
                : string.Format("Stage {0}", stage);
            _stageText.color = isBoss ? new Color(1f, 0.3f, 0.1f) : Color.white;
        }

        private void UpdateEnemyHPBar()
        {
            if (LegendChefSceneSetup.Instance == null) return;

            var enemy = LegendChefSceneSetup.Instance.CurrentEnemy;
            if (enemy == null)
            {
                if (_enemyHPBar != null) _enemyHPBar.SetActive(false);
                return;
            }

            if (_enemyHPBar != null) _enemyHPBar.SetActive(true);

            if (_enemyHPFill != null)
                _enemyHPFill.fillAmount = enemy.CurrentHP / enemy.MaxHP;

            if (_enemyHPText != null)
                _enemyHPText.text = string.Format("{0} / {1}",
                    FormatNumber(enemy.CurrentHP), FormatNumber(enemy.MaxHP));
        }

        // ── 보스 타이머 ──

        public void ShowBossTimer(bool show)
        {
            if (_bossTimerBar != null)
                _bossTimerBar.SetActive(show);
        }

        public void UpdateBossTimer(float remaining, float total)
        {
            if (_bossTimerFill != null)
                _bossTimerFill.fillAmount = remaining / total;
            if (_bossTimerText != null)
                _bossTimerText.text = string.Format("BOSS: {0:F1}s", remaining);
        }

        // ── 유틸리티 ──

        private string FormatNumber(float value)
        {
            if (value >= 1000000f) return string.Format("{0:F1}M", value / 1000000f);
            if (value >= 1000f) return string.Format("{0:F1}K", value / 1000f);
            return string.Format("{0:F0}", value);
        }

        private GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private Text CreateTextElement(string name, Transform parent,
            string text, int fontSize, TextAnchor anchor, Color color)
        {
            GameObject go = CreateUIElement(name, parent);
            Text t = go.AddComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.alignment = anchor;
            t.color = color;
            t.fontStyle = FontStyle.Bold;
            return t;
        }

        private void SetFullStretch(RectTransform rt, Vector2? offsetMin = null, Vector2? offsetMax = null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = offsetMin ?? Vector2.zero;
            rt.offsetMax = offsetMax ?? Vector2.zero;
        }
    }
}
