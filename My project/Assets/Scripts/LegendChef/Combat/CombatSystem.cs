using UnityEngine;
using LegendChef.Core;
using LegendChef.Progression;

namespace LegendChef.Combat
{
    /// <summary>
    /// 데미지 계산을 담당하는 싱글톤
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        public static CombatSystem Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// 데미지와 크리티컬 여부를 계산
        /// </summary>
        public (float damage, bool isCrit) CalculateDamage()
        {
            if (UpgradeSystem.Instance == null)
                return (GameConstants.BaseAttack, false);

            float baseDmg = UpgradeSystem.Instance.CurrentAttack;
            float critChance = UpgradeSystem.Instance.CurrentCritChance;
            float critMult = UpgradeSystem.Instance.CurrentCritMultiplier;

            bool isCrit = Random.value < critChance;
            float finalDmg = isCrit ? baseDmg * critMult : baseDmg;

            // 콤보 배율 적용
            if (ComboSystem.Instance != null)
                finalDmg *= ComboSystem.Instance.ComboMultiplier;

            return (finalDmg, isCrit);
        }
    }
}
