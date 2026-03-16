using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using LegendChef.Combat;

namespace LegendChef.UI
{
    /// <summary>
    /// 콤보 카운터 UI (싱글톤). 스케일 바운스 애니메이션과 색상 변화
    /// </summary>
    public class ComboDisplayUI : MonoBehaviour
    {
        public static ComboDisplayUI Instance { get; private set; }

        private Text _comboText;
        private Text _tierText;
        private CanvasGroup _canvasGroup;
        private RectTransform _comboRT;

        private Coroutine _bounceCoroutine;
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize()
        {
            // 기존 HUD Canvas 찾기
            Canvas canvas = null;
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var c in canvases)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay && c.sortingOrder >= 100)
                {
                    canvas = c;
                    break;
                }
            }

            if (canvas == null) return;

            // 콤보 컨테이너
            GameObject containerGO = new GameObject("ComboDisplay");
            containerGO.transform.SetParent(canvas.transform, false);

            RectTransform containerRT = containerGO.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.3f, 0.55f);
            containerRT.anchorMax = new Vector2(0.7f, 0.65f);
            containerRT.offsetMin = Vector2.zero;
            containerRT.offsetMax = Vector2.zero;

            _canvasGroup = containerGO.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;

            // 콤보 숫자 텍스트
            GameObject comboGO = new GameObject("ComboText");
            comboGO.transform.SetParent(containerGO.transform, false);

            _comboRT = comboGO.AddComponent<RectTransform>();
            _comboRT.anchorMin = Vector2.zero;
            _comboRT.anchorMax = new Vector2(1f, 0.65f);
            _comboRT.offsetMin = Vector2.zero;
            _comboRT.offsetMax = Vector2.zero;

            _comboText = comboGO.AddComponent<Text>();
            _comboText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _comboText.fontSize = 42;
            _comboText.alignment = TextAnchor.MiddleCenter;
            _comboText.fontStyle = FontStyle.Bold;
            _comboText.color = Color.white;
            _comboText.text = "";

            // 등급 텍스트
            GameObject tierGO = new GameObject("TierText");
            tierGO.transform.SetParent(containerGO.transform, false);

            RectTransform tierRT = tierGO.AddComponent<RectTransform>();
            tierRT.anchorMin = new Vector2(0f, 0.65f);
            tierRT.anchorMax = Vector2.one;
            tierRT.offsetMin = Vector2.zero;
            tierRT.offsetMax = Vector2.zero;

            _tierText = tierGO.AddComponent<Text>();
            _tierText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _tierText.fontSize = 28;
            _tierText.alignment = TextAnchor.MiddleCenter;
            _tierText.fontStyle = FontStyle.Bold;
            _tierText.color = Color.yellow;
            _tierText.text = "";

            // 이벤트 구독
            if (ComboSystem.Instance != null)
            {
                ComboSystem.Instance.OnComboChanged += OnComboChanged;
                ComboSystem.Instance.OnComboBreak += OnComboBreak;
                ComboSystem.Instance.OnComboMilestone += OnComboMilestone;
            }
        }

        private void OnComboChanged(int combo, float multiplier)
        {
            if (combo < 2)
            {
                // 1 콤보는 표시하지 않음
                _canvasGroup.alpha = 0f;
                return;
            }

            // 텍스트 업데이트
            _comboText.text = string.Format("{0} COMBO!", combo);
            string tierName = ComboSystem.Instance.GetComboTierName();
            _tierText.text = tierName;

            // 색상 업데이트
            UpdateColors(combo);

            // 표시
            _canvasGroup.alpha = 1f;

            // 바운스 애니메이션
            if (_bounceCoroutine != null)
                StopCoroutine(_bounceCoroutine);
            _bounceCoroutine = StartCoroutine(BounceCoroutine());

            // 페이드 아웃 타이머 리셋
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeOutAfterDelay());
        }

        private void OnComboBreak()
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            _canvasGroup.alpha = 0f;
        }

        private void OnComboMilestone(int combo)
        {
            // 마일스톤: 더 큰 바운스
            if (_bounceCoroutine != null)
                StopCoroutine(_bounceCoroutine);
            _bounceCoroutine = StartCoroutine(BounceCoroutine(1.8f));
        }

        private void UpdateColors(int combo)
        {
            Color textColor;
            Color tierColor;

            if (combo >= 50)
            {
                textColor = new Color(1f, 0.2f, 0.1f); // 빨강 + glow
                tierColor = new Color(1f, 0.4f, 0.1f);
            }
            else if (combo >= 25)
            {
                textColor = new Color(1f, 0.6f, 0.1f); // 주황
                tierColor = new Color(1f, 0.5f, 0f);
            }
            else if (combo >= 10)
            {
                textColor = new Color(1f, 0.9f, 0.1f); // 노랑
                tierColor = new Color(1f, 0.8f, 0f);
            }
            else
            {
                textColor = Color.white;
                tierColor = Color.white;
            }

            _comboText.color = textColor;
            _tierText.color = tierColor;
        }

        private IEnumerator BounceCoroutine(float maxScale = 1.3f)
        {
            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale;

                if (t < 0.5f)
                    scale = Mathf.Lerp(0.8f, maxScale, t / 0.5f);
                else
                    scale = Mathf.Lerp(maxScale, 1f, (t - 0.5f) / 0.5f);

                if (_comboRT != null)
                    _comboRT.localScale = new Vector3(scale, scale, 1f);

                yield return null;
            }

            if (_comboRT != null)
                _comboRT.localScale = Vector3.one;

            _bounceCoroutine = null;
        }

        private IEnumerator FadeOutAfterDelay()
        {
            yield return new WaitForSeconds(1.5f);

            float fadeDuration = 0.3f;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = 1f - (elapsed / fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            _fadeCoroutine = null;
        }

        private void OnDestroy()
        {
            if (ComboSystem.Instance != null)
            {
                ComboSystem.Instance.OnComboChanged -= OnComboChanged;
                ComboSystem.Instance.OnComboBreak -= OnComboBreak;
                ComboSystem.Instance.OnComboMilestone -= OnComboMilestone;
            }
        }
    }
}
