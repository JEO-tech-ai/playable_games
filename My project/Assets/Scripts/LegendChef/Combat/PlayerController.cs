using System.Collections;
using UnityEngine;
using LegendChef.Core;
using LegendChef.Progression;
using LegendChef.Visual;
using LegendChef.Effects;

namespace LegendChef.Combat
{
    /// <summary>
    /// 플레이어(셰프) 컨트롤러. 자동 공격 + 탭 공격
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("자동 공격")]
        [SerializeField] private float _autoAttackTimer;

        private SpriteRenderer _spriteRenderer;
        private SpriteAnimator _spriteAnimator;
        private AnimationClipData _idleClip;
        private AnimationClipData _attackClip;

        /// <summary>
        /// 현재 타겟 적
        /// </summary>
        public EnemyController CurrentEnemy { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        public void InitializeVisual()
        {
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            _spriteRenderer.sprite = PixelArtGenerator.CreatePlayerSprite();
            _spriteRenderer.sortingOrder = 10;
            transform.localScale = new Vector3(1.5f, 1.5f, 1f);

            // 애니메이션 설정
            _spriteAnimator = gameObject.AddComponent<SpriteAnimator>();
            _idleClip = AnimationFrameBuilder.GetChefIdle();
            _attackClip = AnimationFrameBuilder.GetChefAttack();
            _spriteAnimator.Play(_idleClip);
        }

        private void Update()
        {
            if (LegendChefGameManager.Instance == null) return;
            if (LegendChefGameManager.Instance.CurrentState != GameState.Playing) return;
            if (CurrentEnemy == null) return;

            // 자동 공격 타이머
            float interval = UpgradeSystem.Instance != null
                ? UpgradeSystem.Instance.CurrentAutoInterval
                : GameConstants.AutoAttackInterval;

            _autoAttackTimer += Time.deltaTime;
            if (_autoAttackTimer >= interval)
            {
                _autoAttackTimer -= interval;
                AttackEnemy();
            }
        }

        /// <summary>
        /// 탭 공격 (즉시, 쿨다운 없음)
        /// </summary>
        public void PerformTapAttack()
        {
            if (LegendChefGameManager.Instance == null) return;
            if (LegendChefGameManager.Instance.CurrentState != GameState.Playing) return;
            if (CurrentEnemy == null) return;

            // 콤보 등록
            if (ComboSystem.Instance != null)
                ComboSystem.Instance.RegisterTap();

            AttackEnemy();
        }

        private void AttackEnemy()
        {
            if (CurrentEnemy == null) return;
            if (!CurrentEnemy.IsArrived) return;

            var (damage, isCrit) = CombatSystem.Instance.CalculateDamage();
            var (actualDmg, _) = CurrentEnemy.TakeDamage(damage, isCrit);

            if (actualDmg > 0f)
            {
                // 플로팅 텍스트
                if (FloatingTextManager.Instance != null)
                {
                    Vector3 textPos = CurrentEnemy.transform.position + new Vector3(0f, 0.5f, 0f);
                    FloatingTextManager.Instance.SpawnFloatingText(textPos, actualDmg, isCrit);
                }

                // 슬래시 이펙트
                SlashEffect.Spawn(CurrentEnemy.transform.position, isCrit);

                // 크리티컬 버스트 이펙트
                if (isCrit)
                {
                    CritBurstEffect.Spawn(CurrentEnemy.transform.position);
                }

                // 공격 애니메이션 재생 후 IDLE로 복귀
                if (_spriteAnimator != null)
                {
                    PlayAttackAnimation();
                }
            }

            // 화면 흔들림 (크리티컬만)
            if (isCrit && ScreenShakeEffect.Instance != null)
            {
                ScreenShakeEffect.Instance.Shake(0.15f, 0.08f);
            }
        }

        private void PlayAttackAnimation()
        {
            _spriteAnimator.OnAnimationComplete -= OnAttackAnimationComplete;
            _spriteAnimator.OnAnimationComplete += OnAttackAnimationComplete;
            _spriteAnimator.Play(_attackClip);
        }

        private void OnAttackAnimationComplete()
        {
            _spriteAnimator.OnAnimationComplete -= OnAttackAnimationComplete;
            if (_idleClip != null)
                _spriteAnimator.Play(_idleClip);
        }
    }
}
