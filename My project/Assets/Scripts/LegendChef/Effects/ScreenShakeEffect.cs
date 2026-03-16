using System.Collections;
using UnityEngine;

namespace LegendChef.Effects
{
    /// <summary>
    /// 카메라 흔들림 이펙트
    /// </summary>
    public class ScreenShakeEffect : MonoBehaviour
    {
        public static ScreenShakeEffect Instance { get; private set; }

        private Vector3 _originalPosition;
        private bool _isShaking;

        private void Awake()
        {
            Instance = this;
            _originalPosition = transform.localPosition;
        }

        /// <summary>
        /// 카메라 흔들림
        /// </summary>
        public void Shake(float duration = 0.3f, float magnitude = 0.15f)
        {
            if (!_isShaking)
            {
                StartCoroutine(ShakeCoroutine(duration, magnitude));
            }
        }

        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            _isShaking = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = _originalPosition + new Vector3(x, y, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = _originalPosition;
            _isShaking = false;
        }
    }
}
