using System;
using System.Collections;
using UnityEngine;
using LegendChef.Core;
using LegendChef.Visual;

namespace LegendChef.Combat
{
    /// <summary>
    /// 적 몬스터의 이동, 체력, 피격 처리
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        public event Action OnDied;
        public event Action<float, bool> OnDamaged;

        [Header("상태")]
        [SerializeField] private float _maxHP;
        [SerializeField] private float _currentHP;
        [SerializeField] private bool _isArrived;
        [SerializeField] private bool _isBoss;

        private float _targetX;
        private float _spawnX;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        private bool _isDead;
        private SpriteAnimator _spriteAnimator;
        private AnimationClipData _walkClip;
        private AnimationClipData _hitClip;
        private AnimationClipData _idleClip;

        public float MaxHP => _maxHP;
        public float CurrentHP => _currentHP;
        public bool IsArrived => _isArrived;
        public bool IsBoss => _isBoss;

        /// <summary>
        /// 적 초기화. spawnX에서 targetX로 이동한다.
        /// </summary>
        public void Initialize(float hp, bool isBoss, float spawnX, float targetX)
        {
            _maxHP = hp;
            _currentHP = hp;
            _isBoss = isBoss;
            _isArrived = false;
            _isDead = false;
            _spawnX = spawnX;
            _targetX = targetX;

            transform.position = new Vector3(spawnX, transform.position.y, 0f);

            // 스프라이트 생성
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            _spriteRenderer.sortingOrder = 10;

            if (isBoss)
            {
                _spriteRenderer.sprite = PixelArtGenerator.CreateEnemyBossSprite();
                float bossScale = 2f;
                transform.localScale = new Vector3(bossScale, bossScale, 1f);
            }
            else
            {
                _spriteRenderer.sprite = PixelArtGenerator.CreateEnemyBasicSprite();
                float enemyScale = 1.5f;
                transform.localScale = new Vector3(enemyScale, enemyScale, 1f);
            }

            _originalColor = _spriteRenderer.color;

            // 애니메이션 설정
            _spriteAnimator = gameObject.AddComponent<SpriteAnimator>();
            if (isBoss)
            {
                _idleClip = AnimationFrameBuilder.GetBossIdle();
                _hitClip = AnimationFrameBuilder.GetEnemyHit();
            }
            else
            {
                _walkClip = AnimationFrameBuilder.GetEnemyWalk();
                _hitClip = AnimationFrameBuilder.GetEnemyHit();
                _spriteAnimator.Play(_walkClip);
            }
        }

        private void Update()
        {
            if (_isDead) return;

            if (!_isArrived)
            {
                // 왼쪽으로 이동 (spawnX > targetX이므로)
                float step = GameConstants.EnemyMoveSpeed * Time.deltaTime * 0.01f; // 월드 단위 변환
                Vector3 pos = transform.position;
                pos.x = Mathf.MoveTowards(pos.x, _targetX, step);
                transform.position = pos;

                if (Mathf.Abs(pos.x - _targetX) < 0.01f)
                {
                    _isArrived = true;
                    // 도착 시 걷기 애니메이션 정지
                    if (_spriteAnimator != null && !_isBoss)
                    {
                        _spriteAnimator.Stop();
                        // 원본 스프라이트 복원
                        if (_spriteRenderer != null)
                            _spriteRenderer.sprite = _isBoss
                                ? PixelArtGenerator.CreateEnemyBossSprite()
                                : PixelArtGenerator.CreateEnemyBasicSprite();
                    }
                }
            }
        }

        /// <summary>
        /// 데미지를 받는다. 이동 중이면 무적.
        /// 반환: 실제 적용된 데미지, isCrit
        /// </summary>
        public (float damage, bool isCrit) TakeDamage(float amount, bool isCrit)
        {
            if (_isDead) return (0f, false);
            if (!_isArrived) return (0f, false); // 이동 중 무적

            float actual = Mathf.Min(amount, _currentHP);
            _currentHP -= actual;

            OnDamaged?.Invoke(actual, isCrit);

            // 피격 애니메이션
            if (_spriteAnimator != null && _hitClip != null)
            {
                _spriteAnimator.OnAnimationComplete -= OnHitAnimationComplete;
                _spriteAnimator.OnAnimationComplete += OnHitAnimationComplete;
                _spriteAnimator.Play(_hitClip);
            }

            // 피격 이펙트
            StartCoroutine(HitFlashCoroutine());

            if (_currentHP <= 0f)
            {
                _isDead = true;
                OnDied?.Invoke();
            }

            return (actual, isCrit);
        }

        private void OnHitAnimationComplete()
        {
            if (_spriteAnimator != null)
                _spriteAnimator.OnAnimationComplete -= OnHitAnimationComplete;

            // 히트 후 원본 스프라이트 복원
            if (_spriteRenderer != null && !_isDead)
            {
                _spriteRenderer.sprite = _isBoss
                    ? PixelArtGenerator.CreateEnemyBossSprite()
                    : PixelArtGenerator.CreateEnemyBasicSprite();
            }
        }

        private IEnumerator HitFlashCoroutine()
        {
            if (_spriteRenderer == null) yield break;

            // 투명도 깜빡임
            Color flashColor = _originalColor;
            flashColor.a = 0.3f;
            _spriteRenderer.color = flashColor;

            // 흔들림
            Vector3 originalPos = transform.position;
            float shakeAmount = 0.05f;
            transform.position = originalPos + new Vector3(
                UnityEngine.Random.Range(-shakeAmount, shakeAmount), 0f, 0f);

            yield return new WaitForSeconds(0.1f);

            if (_spriteRenderer != null)
                _spriteRenderer.color = _originalColor;
            transform.position = new Vector3(
                _isArrived ? _targetX : transform.position.x,
                originalPos.y, 0f);
        }

        private void OnDestroy()
        {
            OnDied = null;
            OnDamaged = null;
        }
    }
}
