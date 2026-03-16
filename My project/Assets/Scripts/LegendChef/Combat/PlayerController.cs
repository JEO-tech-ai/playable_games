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

            AttackEnemy();
        }

        private void AttackEnemy()
        {
            if (CurrentEnemy == null) return;
            if (!CurrentEnemy.IsArrived) return;

            var (damage, isCrit) = CombatSystem.Instance.CalculateDamage();
            var (actualDmg, _) = CurrentEnemy.TakeDamage(damage, isCrit);

            if (actualDmg > 0f && FloatingTextManager.Instance != null)
            {
                Vector3 textPos = CurrentEnemy.transform.position + new Vector3(0f, 0.5f, 0f);
                FloatingTextManager.Instance.SpawnFloatingText(textPos, actualDmg, isCrit);
            }

            // 화면 흔들림 (크리티컬만)
            if (isCrit && ScreenShakeEffect.Instance != null)
            {
                ScreenShakeEffect.Instance.Shake(0.15f, 0.08f);
            }
        }
    }
}
