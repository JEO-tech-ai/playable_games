using UnityEngine;
using LegendChef.Visual;

namespace LegendChef.Effects
{
    /// <summary>
    /// 보스 사망 시 생성되는 폭발 이펙트
    /// </summary>
    public class BossDeathEffect : MonoBehaviour
    {
        private SpriteAnimator _animator;

        /// <summary>
        /// 보스 사망 이펙트를 생성한다
        /// </summary>
        public static void Spawn(Vector3 position)
        {
            GameObject go = new GameObject("BossDeathEffect");
            go.transform.position = position;
            go.transform.localScale = new Vector3(3f, 3f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 22;
            sr.color = new Color(0.8f, 0.5f, 1f); // 보라/흰 보스 테마

            SpriteAnimator animator = go.AddComponent<SpriteAnimator>();
            BossDeathEffect effect = go.AddComponent<BossDeathEffect>();
            effect._animator = animator;

            // 화면 흔들림
            if (ScreenShakeEffect.Instance != null)
                ScreenShakeEffect.Instance.Shake(0.5f, 0.3f);

            animator.OnAnimationComplete += effect.OnComplete;
            animator.Play(AnimationFrameBuilder.GetBossDeath());
        }

        private void OnComplete()
        {
            if (_animator != null)
                _animator.OnAnimationComplete -= OnComplete;
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_animator != null)
                _animator.OnAnimationComplete -= OnComplete;
        }
    }
}
