using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using LegendChef.Core;

namespace LegendChef.UI
{
    /// <summary>
    /// 메뉴 화면 - 타이틀과 "화면을 탭하여 시작" 표시
    /// </summary>
    public class MenuScreen : MonoBehaviour
    {
        public static MenuScreen Instance { get; private set; }

        private GameObject _menuRoot;
        private Text _blinkText;
        private Coroutine _blinkCoroutine;

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize()
        {
            // 메뉴 전용 캔버스
            GameObject canvasGO = new GameObject("MenuCanvas");
            canvasGO.transform.SetParent(transform);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 300;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvasGO.AddComponent<GraphicRaycaster>();

            _menuRoot = new GameObject("MenuRoot");
            _menuRoot.transform.SetParent(canvasGO.transform, false);

            RectTransform rootRT = _menuRoot.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // 배경
            Image bg = _menuRoot.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.15f, 0.22f, 0.95f);

            // 타이틀 — 불꽃 아이콘 텍스트
            GameObject titleGO = CreateUIElement("Title", _menuRoot.transform);
            RectTransform titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.05f, 0.55f);
            titleRT.anchorMax = new Vector2(0.95f, 0.80f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;
            Text titleText = titleGO.AddComponent<Text>();
            titleText.text = "전설의 불꽃 셰프";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 64;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.7f, 0.1f);
            titleText.fontStyle = FontStyle.Bold;

            // 부제
            GameObject subGO = CreateUIElement("Subtitle", _menuRoot.transform);
            RectTransform subRT = subGO.GetComponent<RectTransform>();
            subRT.anchorMin = new Vector2(0.1f, 0.45f);
            subRT.anchorMax = new Vector2(0.9f, 0.55f);
            subRT.offsetMin = Vector2.zero;
            subRT.offsetMax = Vector2.zero;
            Text subText = subGO.AddComponent<Text>();
            subText.text = "Legend Flame Chef";
            subText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            subText.fontSize = 32;
            subText.alignment = TextAnchor.MiddleCenter;
            subText.color = new Color(0.8f, 0.8f, 0.8f);

            // 깜빡이는 시작 텍스트
            GameObject blinkGO = CreateUIElement("BlinkText", _menuRoot.transform);
            RectTransform blinkRT = blinkGO.GetComponent<RectTransform>();
            blinkRT.anchorMin = new Vector2(0.1f, 0.25f);
            blinkRT.anchorMax = new Vector2(0.9f, 0.35f);
            blinkRT.offsetMin = Vector2.zero;
            blinkRT.offsetMax = Vector2.zero;
            _blinkText = blinkGO.AddComponent<Text>();
            _blinkText.text = "화면을 탭하여 시작";
            _blinkText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _blinkText.fontSize = 36;
            _blinkText.alignment = TextAnchor.MiddleCenter;
            _blinkText.color = Color.white;

            // 탭 감지용 투명 버튼
            Button tapBtn = _menuRoot.AddComponent<Button>();
            tapBtn.transition = Selectable.Transition.None;
            tapBtn.onClick.AddListener(OnTapStart);

            _blinkCoroutine = StartCoroutine(BlinkCoroutine());
        }

        private IEnumerator BlinkCoroutine()
        {
            while (true)
            {
                if (_blinkText != null)
                {
                    Color c = _blinkText.color;
                    c.a = Mathf.PingPong(Time.time * 2f, 1f);
                    _blinkText.color = c;
                }
                yield return null;
            }
        }

        private void OnTapStart()
        {
            Hide();
            if (LegendChefGameManager.Instance != null)
                LegendChefGameManager.Instance.SetState(GameState.Playing);
        }

        public void Show()
        {
            if (_menuRoot != null)
                _menuRoot.SetActive(true);
            if (_blinkCoroutine == null)
                _blinkCoroutine = StartCoroutine(BlinkCoroutine());
        }

        public void Hide()
        {
            if (_blinkCoroutine != null)
            {
                StopCoroutine(_blinkCoroutine);
                _blinkCoroutine = null;
            }
            if (_menuRoot != null)
                _menuRoot.SetActive(false);
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
