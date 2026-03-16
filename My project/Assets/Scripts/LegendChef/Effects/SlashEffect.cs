using UnityEngine;
using LegendChef.Visual;

namespace LegendChef.Effects
{
    /// <summary>
    /// 공격 시 적 위치에 생성되는 슬래시 이펙트
    /// </summary>
    public class SlashEffect : MonoBehaviour
    {
        private SpriteAnimator _animator;

        /// <summary>
        /// 슬래시 이펙트를 생성한다
        /// </summary>
        /// <param name="position">월드 좌표</param>
        /// <param name="isCrit">크리티컬 여부 (크면 더 큰 스케일)</param>
        public static void Spawn(Vector3 position, bool isCrit)
        {
            GameObject go = new GameObject("SlashEffect");
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(0f, 0f, -30f);

            float scale = isCrit ? 2.5f : 1.5f;
            go.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 20;

            SpriteAnimator animator = go.AddComponent<SpriteAnimator>();
            SlashEffect effect = go.AddComponent<SlashEffect>();
            effect._animator = animator;

            animator.OnAnimationComplete += effect.OnComplete;
            animator.Play(AnimationFrameBuilder.GetSlashEffect());
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
