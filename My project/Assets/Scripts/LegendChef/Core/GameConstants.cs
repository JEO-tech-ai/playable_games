using UnityEngine;

namespace LegendChef.Core
{
    /// <summary>
    /// 모든 게임 수치 상수를 관리하는 정적 클래스
    /// </summary>
    public static class GameConstants
    {
        // ── 기본 전투 수치 ──
        public static readonly float BaseAttack = 1f;
        public static readonly float AutoAttackInterval = 1.0f;
        public static readonly float MinAutoAttackInterval = 0.1f;
        public static readonly float BaseCritChance = 0f;
        public static readonly float BaseCritMultiplier = 2f;

        // ── 수입 ──
        public static readonly float BaseGoldPerSec = 1f;

        // ── 몬스터 ──
        public static readonly float MonsterBaseHP = 10f;
        public static readonly float MonsterHPScaling = 1.4f;
        public static readonly float MonsterBaseGoldReward = 5f;
        public static readonly float MonsterGoldRewardScaling = 1.3f;

        // ── 보스 ──
        public static readonly float BossHPMultiplier = 2.5f;
        public static readonly float BossTimerDuration = 15f;
        public static readonly int BossSpiceReward = 1;
        public static readonly int BossStageInterval = 5;
        public static readonly int BossTimeoutRetreatStages = 4;

        // ── 업그레이드 비용 ──
        public static readonly float UpgradeBaseCostGold = 10f;
        public static readonly float UpgradeCostScaling = 1.5f;
        public static readonly int CritUpgradeBaseCostSpice = 1;

        // ── 적 이동 ──
        public static readonly float EnemyMoveSpeed = 200f;
        public static readonly float EnemyTargetXRatio = 0.6f;
        public static readonly float PlayerXRatio = 0.3f;

        // ── 이펙트 ──
        public static readonly float FloatingTextRiseDistance = 50f;
        public static readonly float FloatingTextDuration = 0.5f;
        public static readonly float HitShakeMagnitude = 5f;

        // ── 레이아웃 ──
        public static readonly float GameAreaRatio = 0.5f;
        public static readonly float UIAreaRatio = 0.5f;

        // ── 수식 ──
        /// <summary>
        /// 몬스터 HP = 10 * 1.4^(stage-1)
        /// </summary>
        public static float GetMonsterHP(int stage)
        {
            return MonsterBaseHP * Mathf.Pow(MonsterHPScaling, stage - 1);
        }

        /// <summary>
        /// 보스 HP = 몬스터 HP * 2.5
        /// </summary>
        public static float GetBossHP(int stage)
        {
            return GetMonsterHP(stage) * BossHPMultiplier;
        }

        /// <summary>
        /// 몬스터 골드 보상 = 5 * 1.3^(stage-1)
        /// </summary>
        public static float GetMonsterGoldReward(int stage)
        {
            return MonsterBaseGoldReward * Mathf.Pow(MonsterGoldRewardScaling, stage - 1);
        }

        /// <summary>
        /// 보스 보상 = 1 Spice
        /// </summary>
        public static int GetBossReward()
        {
            return BossSpiceReward;
        }

        /// <summary>
        /// 골드 업그레이드 비용 = 10 * 1.5^level
        /// </summary>
        public static float GetUpgradeCostGold(int currentLevel)
        {
            return UpgradeBaseCostGold * Mathf.Pow(UpgradeCostScaling, currentLevel);
        }

        /// <summary>
        /// 크리티컬 업그레이드 비용 (향신료) = 1 + level
        /// </summary>
        public static int GetCritUpgradeCostSpice(int currentLevel)
        {
            return CritUpgradeBaseCostSpice + currentLevel;
        }

        /// <summary>
        /// 보스 스테이지인지 확인 (매 5스테이지)
        /// </summary>
        public static bool IsBossStage(int stage)
        {
            return stage % BossStageInterval == 0;
        }
    }
}
