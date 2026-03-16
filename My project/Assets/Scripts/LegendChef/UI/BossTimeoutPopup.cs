using UnityEngine;
using UnityEngine.UI;
using LegendChef.Core;
using LegendChef.Progression;

namespace LegendChef.UI
{
    /// <summary>
    /// 보스 타임아웃 시 "요리 실패!" 팝업
    /// </summary>
    public class BossTimeoutPopup : MonoBehaviour
    {
        public static BossTimeoutPopup Instance { get; private set; }

        private GameObject _popupRoot;
        private Canvas _canvas;

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize()
        {
            // 팝업 전용 캔버스
            GameObject canvasGO = new GameObject("BossTimeoutCanvas");
            canvasGO.transform.SetParent(transform);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 200;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
            canvasGO.AddComponent<GraphicRaycaster>();

            _popupRoot = new GameObject("PopupRoot");
            _popupRoot.transform.SetParent(canvasGO.transform, false);

            RectTransform rootRT = _popupRoot.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // 반투명 배경
            Image bg = _popupRoot.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);

            // 팝업 패널
            GameObject panel = CreateUIElement("Panel", _popupRoot.transform);
            RectTransform panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.15f, 0.35f);
            panelRT.anchorMax = new Vector2(0.85f, 0.65f);
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.15f, 0.15f, 0.20f, 0.95f);

            // 제목 텍스트
            GameObject titleGO = CreateUIElement("Title", panel.transform);
            RectTransform titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.1f, 0.55f);
            titleRT.anchorMax = new Vector2(0.9f, 0.9f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;
            Text titleText = titleGO.AddComponent<Text>();
            titleText.text = "요리 실패!";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 48;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.3f, 0.3f);
            titleText.fontStyle = FontStyle.Bold;

            // 설명 텍스트
            GameObject descGO = CreateUIElement("Desc", panel.transform);
            RectTransform descRT = descGO.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0.1f, 0.35f);
            descRT.anchorMax = new Vector2(0.9f, 0.55f);
            descRT.offsetMin = Vector2.zero;
            descRT.offsetMax = Vector2.zero;
            Text descText = descGO.AddComponent<Text>();
            descText.text = "보스를 물리치지 못했습니다...";
            descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            descText.fontSize = 28;
            descText.alignment = TextAnchor.MiddleCenter;
            descText.color = Color.white;

            // 확인 버튼
            GameObject btnGO = CreateUIElement("ConfirmBtn", panel.transform);
            RectTransform btnRT = btnGO.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.25f, 0.08f);
            btnRT.anchorMax = new Vector2(0.75f, 0.30f);
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;
            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.90f, 0.49f, 0.13f);
            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            GameObject btnTextGO = CreateUIElement("BtnText", btnGO.transform);
            RectTransform btnTextRT = btnTextGO.GetComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.offsetMin = Vector2.zero;
            btnTextRT.offsetMax = Vector2.zero;
            Text btnText = btnTextGO.AddComponent<Text>();
            btnText.text = "확인";
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 36;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;
            btnText.fontStyle = FontStyle.Bold;

            btn.onClick.AddListener(OnConfirmClicked);

            _popupRoot.SetActive(false);
        }

        public void Show()
        {
            if (_popupRoot != null)
                _popupRoot.SetActive(true);
        }

        public void Hide()
        {
            if (_popupRoot != null)
                _popupRoot.SetActive(false);
        }

        private void OnConfirmClicked()
        {
            Hide();
            // 스테이지 후퇴 후 게임 재개
            if (StageManager.Instance != null)
                StageManager.Instance.RetreatOnBossTimeout();
            if (LegendChefGameManager.Instance != null)
                LegendChefGameManager.Instance.SetState(GameState.Playing);
        }

        private GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }
    }
}
