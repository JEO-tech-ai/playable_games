using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using LegendChef.Visual;
using LegendChef.Combat;

namespace LegendChef.Effects
{
    /// <summary>
    /// 보스전 시네마틱 텐션 이펙트 오케스트레이터 (싱글톤)
    /// </summary>
    public class TensionManager : MonoBehaviour
    {
        public static TensionManager Instance { get; private set; }

        // 내부 상태
        private bool _bossActive;
        private EnemyController _currentBoss;
        private SpriteAnimator _bossAnimator;

        // 하트비트 펄스
        private Coroutine _heartbeatCoroutine;
        private Camera _mainCamera;
        private Color _baseBgColor = new Color(0.17f, 0.24f, 0.31f);
        private Color _darkBgColor = new Color(0.10f, 0.15f, 0.18f);

        // 경고 플래시 UI
        private Image _warningFlashImage;
        private Coroutine _warningFlashCoroutine;
        private bool _warningFlashActive;

        // 보스 레이지 상태
        private bool _rageActive;

        private void Awake()
        {
            Instance = this;
            _mainCamera = Camera.main;
        }

        /// <summary>
        /// 보스가 스폰될 때 호출
        /// </summary>
        public void OnBossSpawned(EnemyController boss)
        {
            _bossActive = true;
            _currentBoss = boss;
            _rageActive = false;
            _warningFlashActive = false;

            // 보스 애니메이터 설정
            _bossAnimator = boss.GetComponent<SpriteAnimator>();
            if (_bossAnimator == null)
                _bossAnimator = boss.gameObject.AddComponent<SpriteAnimator>();

            // 보스 입장 줌인 연출
            StartCoroutine(BossEntranceCoroutine(boss));

            // 보스 IDLE 애니메이션 시작
            _bossAnimator.Play(AnimationFrameBuilder.GetBossIdle());
        }

        /// <summary>
        /// 매 프레임 보스 타이머 업데이트
        /// </summary>
        public void UpdateBossTimer(float remaining, float total)
        {
            if (!_bossActive) return;

            // 하트비트 펄스 (타이머 < 8초)
            if (remaining < 8f && _heartbeatCoroutine == null)
            {
                _heartbeatCoroutine = StartCoroutine(HeartbeatPulseCoroutine(remaining));
            }

            // 보스 레이지 (타이머 < 5초)
            if (remaining < 5f && !_rageActive && _bossAnimator != null)
            {
                _rageActive = true;
                _bossAnimator.Play(AnimationFrameBuilder.GetBossRage());
            }

            // 경고 플래시 (타이머 < 3초)
            if (remaining < 3f && !_warningFlashActive)
            {
                _warningFlashActive = true;
                StartWarningFlash();
            }
        }

        /// <summary>
        /// 보스 처치 시 호출
        /// </summary>
        public void OnBossDefeated(Vector3 bossPosition)
        {
            _bossActive = false;
            StopAllTensionEffects();

            // 보스 사망 이펙트
            BossDeathEffect.Spawn(bossPosition);

            // 화면 흰색 플래시
            StartCoroutine(WhiteFlashCoroutine());

            // 화면 흔들림
            if (ScreenShakeEffect.Instance != null)
                ScreenShakeEffect.Instance.Shake(0.5f, 0.25f);

            // "CLEAR!" 텍스트
            StartCoroutine(ShowClearTextCoroutine());
        }

        /// <summary>
        /// 보스 타임아웃 시 호출
        /// </summary>
        public void OnBossTimeout()
        {
            _bossActive = false;
            StopAllTensionEffects();
        }

        // ── 보스 입장 줌인 ──

        private IEnumerator BossEntranceCoroutine(EnemyController boss)
        {
            if (boss == null) yield break;

            Transform bossT = boss.transform;
            Vector3 targetScale = bossT.localScale;
            bossT.localScale = targetScale * 0.3f;

            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (boss == null) yield break;
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Ease out quad
                float eased = 1f - (1f - t) * (1f - t);
                bossT.localScale = Vector3.Lerp(targetScale * 0.3f, targetScale, eased);
                yield return null;
            }

            if (boss != null)
                bossT.localScale = targetScale;
        }

        // ── 하트비트 펄스 ──

        private IEnumerator HeartbeatPulseCoroutine(float initialRemaining)
        {
            while (_bossActive && _mainCamera != null)
            {
                // 가속하는 펄스 (남은 시간이 적을수록 빨라짐)
                float pulseSpeed = Mathf.Lerp(1f, 3f, 1f - Mathf.Clamp01(initialRemaining / 8f));
                float halfPeriod = 0.5f / pulseSpeed;

                // 어두워지기
                float t = 0f;
                while (t < halfPeriod && _bossActive)
                {
                    t += Time.deltaTime;
                    float lerp = t / halfPeriod;
                    _mainCamera.backgroundColor = Color.Lerp(_baseBgColor, _darkBgColor, lerp);
                    yield return null;
                }

                // 밝아지기
                t = 0f;
                while (t < halfPeriod && _bossActive)
                {
                    t += Time.deltaTime;
                    float lerp = t / halfPeriod;
                    _mainCamera.backgroundColor = Color.Lerp(_darkBgColor, _baseBgColor, lerp);
                    yield return null;
                }

                // 다음 펄스를 위해 remaining 업데이트
                initialRemaining -= (halfPeriod * 2f);
                if (initialRemaining < 0f) initialRemaining = 0f;
            }

            // 배경색 복원
            if (_mainCamera != null)
                _mainCamera.backgroundColor = _baseBgColor;

            _heartbeatCoroutine = null;
        }

        // ── 경고 플래시 ──

        private void StartWarningFlash()
        {
            EnsureWarningFlashUI();
            if (_warningFlashCoroutine != null)
                StopCoroutine(_warningFlashCoroutine);
            _warningFlashCoroutine = StartCoroutine(WarningFlashCoroutine());
        }

        private void EnsureWarningFlashUI()
        {
            if (_warningFlashImage != null) return;

            // 기존 HUD Canvas 찾기
            Canvas canvas = FindCanvasForOverlay();
            if (canvas == null) return;

            GameObject flashGO = new GameObject("WarningFlash");
            flashGO.transform.SetParent(canvas.transform, false);

            RectTransform rt = flashGO.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _warningFlashImage = flashGO.AddComponent<Image>();
            _warningFlashImage.color = new Color(1f, 0f, 0f, 0f);
            _warningFlashImage.raycastTarget = false;
        }

        private Canvas FindCanvasForOverlay()
        {
            // ScreenSpaceOverlay Canvas 찾기
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var c in canvases)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay && c.sortingOrder >= 100)
                    return c;
            }

            // 없으면 새로 만들기
            GameObject canvasGO = new GameObject("TensionCanvas");
            Canvas newCanvas = canvasGO.AddComponent<Canvas>();
            newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            newCanvas.sortingOrder = 200;
            canvasGO.AddComponent<CanvasScaler>();
            return newCanvas;
        }

        private IEnumerator WarningFlashCoroutine()
        {
            float frequency = 2f; // 2Hz
            float period = 1f / frequency;
            float halfPeriod = period / 2f;

            while (_bossActive && _warningFlashImage != null)
            {
                // 페이드 인
                float t = 0f;
                while (t < halfPeriod && _bossActive)
                {
                    t += Time.deltaTime;
                    float alpha = Mathf.Lerp(0f, 0.3f, t / halfPeriod);
                    _warningFlashImage.color = new Color(1f, 0f, 0f, alpha);
                    yield return null;
                }

                // 페이드 아웃
                t = 0f;
                while (t < halfPeriod && _bossActive)
                {
                    t += Time.deltaTime;
                    float alpha = Mathf.Lerp(0.3f, 0f, t / halfPeriod);
                    _warningFlashImage.color = new Color(1f, 0f, 0f, alpha);
                    yield return null;
                }
            }

            if (_warningFlashImage != null)
                _warningFlashImage.color = new Color(1f, 0f, 0f, 0f);

            _warningFlashCoroutine = null;
        }

        // ── 보스 사망 이펙트 ──

        private IEnumerator WhiteFlashCoroutine()
        {
            Canvas canvas = FindCanvasForOverlay();
            if (canvas == null) yield break;

            GameObject flashGO = new GameObject("WhiteFlash");
            flashGO.transform.SetParent(canvas.transform, false);

            RectTransform rt = flashGO.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image flashImg = flashGO.AddComponent<Image>();
            flashImg.raycastTarget = false;

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float alpha = Mathf.Lerp(0.8f, 0f, t);
                flashImg.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }

            Destroy(flashGO);
        }

        private IEnumerator ShowClearTextCoroutine()
        {
            // TextMesh를 월드 공간에 표시
            GameObject textGO = new GameObject("ClearText");
            textGO.transform.position = new Vector3(0f, 2.5f, 0f);

            TextMesh textMesh = textGO.AddComponent<TextMesh>();
            textMesh.text = "CLEAR!";
            textMesh.characterSize = 0.3f;
            textMesh.fontSize = 100;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontStyle = FontStyle.Bold;
            textMesh.color = new Color(1f, 0.9f, 0.1f, 1f);

            MeshRenderer mr = textGO.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 200;

            // 스케일 바운스: 0 → 1.5 → 1.0
            float bounceTime = 0.3f;
            float elapsed = 0f;
            while (elapsed < bounceTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / bounceTime;
                float scale;
                if (t < 0.6f)
                    scale = Mathf.Lerp(0f, 1.5f, t / 0.6f);
                else
                    scale = Mathf.Lerp(1.5f, 1f, (t - 0.6f) / 0.4f);
                textGO.transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            textGO.transform.localScale = Vector3.one;

            // 1초 대기 후 페이드 아웃
            yield return new WaitForSeconds(1f);

            float fadeDuration = 0.5f;
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / fadeDuration);
                textMesh.color = new Color(1f, 0.9f, 0.1f, alpha);
                yield return null;
            }

            Destroy(textGO);
        }

        // ── 정리 ──

        private void StopAllTensionEffects()
        {
            if (_heartbeatCoroutine != null)
            {
                StopCoroutine(_heartbeatCoroutine);
                _heartbeatCoroutine = null;
            }

            if (_warningFlashCoroutine != null)
            {
                StopCoroutine(_warningFlashCoroutine);
                _warningFlashCoroutine = null;
            }

            // 배경색 복원
            if (_mainCamera != null)
                _mainCamera.backgroundColor = _baseBgColor;

            // 경고 플래시 숨기기
            if (_warningFlashImage != null)
                _warningFlashImage.color = new Color(1f, 0f, 0f, 0f);

            _rageActive = false;
            _warningFlashActive = false;
            _currentBoss = null;
            _bossAnimator = null;
        }
    }
}
