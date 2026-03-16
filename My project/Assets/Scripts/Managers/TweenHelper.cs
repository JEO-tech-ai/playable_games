using UnityEngine;
using System.Collections;
using System;

namespace ColorDrive
{
    /// <summary>
    /// Lightweight tween utility — replaces LeanTween/DOTween dependency.
    /// All tweens run as coroutines on a persistent MonoBehaviour.
    /// </summary>
    public class TweenHelper : MonoBehaviour
    {
        private static TweenHelper _instance;

        public static TweenHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[TweenHelper]");
                    _instance = go.AddComponent<TweenHelper>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Scale
        public static void ScaleTo(GameObject target, Vector3 toScale, float duration, Action onComplete = null)
        {
            Instance.StartCoroutine(ScaleCoroutine(target, toScale, duration, onComplete));
        }

        private static IEnumerator ScaleCoroutine(GameObject target, Vector3 to, float duration, Action onComplete)
        {
            if (target == null) yield break;
            Vector3 from = target.transform.localScale;
            float t = 0f;
            while (t < duration)
            {
                if (target == null) yield break;
                t += Time.unscaledDeltaTime;
                float pct = Mathf.Clamp01(t / duration);
                // EaseOutBack
                float eased = EaseOutBack(pct);
                target.transform.localScale = Vector3.LerpUnclamped(from, to, eased);
                yield return null;
            }
            if (target != null)
                target.transform.localScale = to;
            onComplete?.Invoke();
        }

        // Move Y
        public static void MoveY(GameObject target, float toY, float duration)
        {
            Instance.StartCoroutine(MoveYCoroutine(target, toY, duration));
        }

        private static IEnumerator MoveYCoroutine(GameObject target, float toY, float duration)
        {
            if (target == null) yield break;
            float fromY = target.transform.position.y;
            float t = 0f;
            while (t < duration)
            {
                if (target == null) yield break;
                t += Time.unscaledDeltaTime;
                float pct = Mathf.Clamp01(t / duration);
                float y = Mathf.Lerp(fromY, toY, EaseOutQuad(pct));
                Vector3 pos = target.transform.position;
                pos.y = y;
                target.transform.position = pos;
                yield return null;
            }
            if (target != null)
            {
                Vector3 pos = target.transform.position;
                pos.y = toY;
                target.transform.position = pos;
            }
        }

        // Color fade
        public static void ColorTo(SpriteRenderer sr, Color from, Color to, float duration)
        {
            Instance.StartCoroutine(ColorCoroutine(sr, from, to, duration));
        }

        private static IEnumerator ColorCoroutine(SpriteRenderer sr, Color from, Color to, float duration)
        {
            if (sr == null) yield break;
            float t = 0f;
            while (t < duration)
            {
                if (sr == null) yield break;
                t += Time.unscaledDeltaTime;
                sr.color = Color.Lerp(from, to, t / duration);
                yield return null;
            }
            if (sr != null) sr.color = to;
        }

        // Scale punch (bounce in)
        public static void PunchScale(GameObject target, float overScale, float duration)
        {
            Instance.StartCoroutine(PunchScaleCoroutine(target, overScale, duration));
        }

        private static IEnumerator PunchScaleCoroutine(GameObject target, float over, float duration)
        {
            if (target == null) yield break;
            Vector3 orig = target.transform.localScale;
            float half = duration / 2f;

            // Scale up
            float t = 0f;
            while (t < half)
            {
                if (target == null) yield break;
                t += Time.unscaledDeltaTime;
                target.transform.localScale = Vector3.Lerp(orig, orig * over, t / half);
                yield return null;
            }
            // Scale back
            t = 0f;
            while (t < half)
            {
                if (target == null) yield break;
                t += Time.unscaledDeltaTime;
                target.transform.localScale = Vector3.Lerp(orig * over, orig, t / half);
                yield return null;
            }
            if (target != null)
                target.transform.localScale = orig;
        }

        // Scale to zero then destroy
        public static void ScaleOutAndDestroy(GameObject target, float duration, Action onComplete = null)
        {
            Instance.StartCoroutine(ScaleCoroutine(target, Vector3.zero, duration, () =>
            {
                onComplete?.Invoke();
                if (target != null) Destroy(target);
            }));
        }

        // Easing functions
        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private static float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return c3 * t * t * t - c1 * t * t;
        }
    }
}
