using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using LegendChef.Core;
using LegendChef.Progression;

namespace LegendChef.UI
{
    /// <summary>
    /// 스테이지 변경 시 현재 난이도 티어와 힌트를 잠깐 표시
    /// </summary>
    public class DifficultyHintUI : MonoBehaviour
    {
        public static DifficultyHintUI Instance { get; private set; }

        private Canvas _canvas;
        private GameObject _panel;
        private Text _tierText;
        private Text _hintText;
        private Image _panelImage;

        private Coroutine _hideCoroutine;

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize()
        {
            GameObject canvasGO = new GameObject("DifficultyHintCanvas");
            canvasGO.transform.SetParent(transform);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 200;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // 패널 (화면 상단 중앙, 작은 배너)
            _panel = CreateUIElement("HintPanel", canvasGO.transform);
            RectTransform rt = _panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.82f);
            rt.anchorMax = new Vector2(0.9f, 0.92f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _panelImage = _panel.AddComponent<Image>();
            _panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.88f);

            // 티어 텍스트
            GameObject tierGO = CreateUIElement("TierText", _panel.transform);
            RectTransform tierRT = tierGO.GetComponent<RectTransform>();
            tierRT.anchorMin = new Vector2(0f, 0.55f);
            tierRT.anchorMax = new Vector2(1f, 1f);
            tierRT.offsetMin = new Vector2(8f, 0f);
            tierRT.offsetMax = new Vector2(-8f, 0f);
            _tierText = tierGO.AddComponent<Text>();
            _tierText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _tierText.fontSize = 22;
            _tierText.alignment = TextAnchor.MiddleCenter;
            _tierText.color = new Color(1f, 0.84f, 0.1f);

            // 힌트 텍스트
            GameObject hintGO = CreateUIElement("HintText", _panel.transform);
            RectTransform hintRT = hintGO.GetComponent<RectTransform>();
            hintRT.anchorMin = new Vector2(0f, 0f);
            hintRT.anchorMax = new Vector2(1f, 0.55f);
            hintRT.offsetMin = new Vector2(8f, 0f);
            hintRT.offsetMax = new Vector2(-8f, 0f);
            _hintText = hintGO.AddComponent<Text>();
            _hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _hintText.fontSize = 16;
            _hintText.alignment = TextAnchor.MiddleCenter;
            _hintText.color = new Color(0.85f, 0.85f, 0.85f);

            _panel.SetActive(false);

            // 스테이지 변경 이벤트 구독
            if (StageManager.Instance != null)
                StageManager.Instance.OnStageChanged += OnStageChanged;
        }

        private void OnStageChanged(int stage)
        {
            var milestone = DifficultyConfig.GetMilestone(stage);

            // 티어가 새로 시작되는 스테이지일 때만 표시
            if (stage == milestone.StartStage || stage == 1)
            {
                ShowHint(milestone.TierName, milestone.HintText);
            }
        }

        public void ShowHint(string tier, string hint)
        {
            _tierText.text = tier;
            _hintText.text = hint;
            _panel.SetActive(true);

            if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
            _hideCoroutine = StartCoroutine(HideAfterDelay(3.5f));
        }

        private IEnumerator HideAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            _panel.SetActive(false);
        }

        private GameObject CreateUIElement(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private void OnDestroy()
        {
            if (StageManager.Instance != null)
                StageManager.Instance.OnStageChanged -= OnStageChanged;
        }
    }
}
