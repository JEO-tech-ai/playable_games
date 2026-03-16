using System;
using UnityEngine;
using LegendChef.Core;

namespace LegendChef.Progression
{
    /// <summary>
    /// 4종 업그레이드를 관리하는 싱글톤
    /// </summary>
    public class UpgradeSystem : MonoBehaviour
    {
        public static UpgradeSystem Instance { get; private set; }

        public event Action OnUpgraded;

        [Header("업그레이드 레벨")]
        [SerializeField] private int _atkLevel;
        [SerializeField] private int _speedLevel;
        [SerializeField] private int _incomeLevel;
        [SerializeField] private int _critLevel;

        public int AtkLevel => _atkLevel;
        public int SpeedLevel => _speedLevel;
        public int IncomeLevel => _incomeLevel;
        public int CritLevel => _critLevel;

        // ── 현재 수치 계산 ──

        /// <summary>
        /// 공격력 = 기본(1) + 레벨 * 1 (레벨당 +1 데미지)
        /// </summary>
        public float CurrentAttack => GameConstants.BaseAttack + _atkLevel * 1f;

        /// <summary>
        /// 자동공격 간격 = max(0.1, 1.0 - 레벨 * 0.05)
        /// </summary>
        public float CurrentAutoInterval =>
            Mathf.Max(GameConstants.MinAutoAttackInterval,
                      GameConstants.AutoAttackInterval - _speedLevel * 0.05f);

        /// <summary>
        /// 초당 골드 수입 = 1 + 레벨 * 0.5
        /// </summary>
        public float CurrentIncomePerSec => GameConstants.BaseGoldPerSec + _incomeLevel * 0.5f;

        /// <summary>
        /// 크리티컬 확률 = 레벨 * 5% (최대 50%)
        /// </summary>
        public float CurrentCritChance =>
            Mathf.Min(0.5f, GameConstants.BaseCritChance + _critLevel * 0.05f);

        /// <summary>
        /// 크리티컬 배수 = 2 + 레벨 * 0.2
        /// </summary>
        public float CurrentCritMultiplier =>
            GameConstants.BaseCritMultiplier + _critLevel * 0.2f;

        // ── 비용 ──

        public float AtkUpgradeCost => GameConstants.GetUpgradeCostGold(_atkLevel);
        public float SpeedUpgradeCost => GameConstants.GetUpgradeCostGold(_speedLevel);
        public float IncomeUpgradeCost => GameConstants.GetUpgradeCostGold(_incomeLevel);
        public int CritUpgradeCost => GameConstants.GetCritUpgradeCostSpice(_critLevel);

        private void Awake()
        {
            Instance = this;
        }

        public bool TryUpgradeATK()
        {
            float cost = AtkUpgradeCost;
            if (!CurrencySystem.Instance.CanAffordGold(cost)) return false;
            CurrencySystem.Instance.SpendGold(cost);
            _atkLevel++;
            OnUpgraded?.Invoke();
            return true;
        }

        public bool TryUpgradeSPEED()
        {
            float cost = SpeedUpgradeCost;
            if (!CurrencySystem.Instance.CanAffordGold(cost)) return false;
            CurrencySystem.Instance.SpendGold(cost);
            _speedLevel++;
            // 자동공격 간격 갱신은 PlayerController에서 참조
            OnUpgraded?.Invoke();
            return true;
        }

        public bool TryUpgradeINCOME()
        {
            float cost = IncomeUpgradeCost;
            if (!CurrencySystem.Instance.CanAffordGold(cost)) return false;
            CurrencySystem.Instance.SpendGold(cost);
            _incomeLevel++;
            // CurrencySystem 수입 갱신
            if (CurrencySystem.Instance != null)
                CurrencySystem.Instance.IncomePerSec = CurrentIncomePerSec;
            OnUpgraded?.Invoke();
            return true;
        }

        public bool TryUpgradeCRIT()
        {
            int cost = CritUpgradeCost;
            if (!CurrencySystem.Instance.CanAffordSpice(cost)) return false;
            CurrencySystem.Instance.SpendSpice(cost);
            _critLevel++;
            OnUpgraded?.Invoke();
            return true;
        }
    }
}
