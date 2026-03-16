using UnityEngine;
using LegendChef.Visual;

namespace LegendChef.Effects
{
    /// <summary>
    /// 크리티컬 히트 시 생성되는 버스트 이펙트
    /// </summary>
    public class CritBurstEffect : MonoBehaviour
    {
        private SpriteAnimator _animator;

        /// <summary>
        /// 크리티컬 버스트 이펙트를 생성한다
        /// </summary>
        public static void Spawn(Vector3 position)
        {
            GameObject go = new GameObject("CritBurstEffect");
            go.transform.position = position;
            go.transform.localScale = new Vector3(2f, 2f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 21;
            sr.color = new Color(1f, 0.8f, 0.3f); // 노란-주황 틴트

            SpriteAnimator animator = go.AddComponent<SpriteAnimator>();
            CritBurstEffect effect = go.AddComponent<CritBurstEffect>();
            effect._animator = animator;

            animator.OnAnimationComplete += effect.OnComplete;
            animator.Play(AnimationFrameBuilder.GetCritBurst());
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
