using UnityEngine;
using System.Collections.Generic;

namespace LegendChef.Core
{
    /// <summary>
    /// 10단계 난이도 구성 및 플레이어 가이드 힌트
    /// </summary>
    public static class DifficultyConfig
    {
        public struct DifficultyMilestone
        {
            public int StartStage;
            public int EndStage;
            public string TierName;       // "입문 (Beginner)"
            public string HintText;       // 플레이어에게 보여줄 힌트
            public int RecATK;            // 권장 ATK 레벨
            public int RecSPEED;          // 권장 SPEED 레벨
            public int RecINCOME;         // 권장 INCOME 레벨
            public int RecCRIT;           // 권장 CRIT 레벨
            public string BossStrategy;   // 보스 전략 메모 (해당 구간 보스가 있을 때)
        }

        public static readonly List<DifficultyMilestone> Milestones = new List<DifficultyMilestone>
        {
            new DifficultyMilestone {
                StartStage = 1, EndStage = 4,
                TierName = "Tier 1 — 입문 (Beginner)",
                HintText = "화면을 탭해서 몬스터를 공격하세요!\n[⚔ ATK] 업그레이드로 공격력을 올리세요.",
                RecATK = 3, RecSPEED = 1, RecINCOME = 1, RecCRIT = 0,
                BossStrategy = ""
            },
            new DifficultyMilestone {
                StartStage = 5, EndStage = 5,
                TierName = "Tier 2 — 첫 보스 (First Boss)",
                HintText = "15초 안에 보스를 처치하세요!\n탭을 최대한 빠르게 눌러 추가 딜을 넣으세요.",
                RecATK = 4, RecSPEED = 2, RecINCOME = 1, RecCRIT = 0,
                BossStrategy = "HP 35 / 필요 DPS ~2.3 / 탭 연타 필수"
            },
            new DifficultyMilestone {
                StartStage = 6, EndStage = 9,
                TierName = "Tier 3 — 가속 (Acceleration)",
                HintText = "적 HP가 빠르게 증가합니다.\n[⚡ SPEED] 업그레이드로 공격 속도를 높이세요.",
                RecATK = 5, RecSPEED = 4, RecINCOME = 2, RecCRIT = 1,
                BossStrategy = ""
            },
            new DifficultyMilestone {
                StartStage = 10, EndStage = 10,
                TierName = "Tier 4 — 두 번째 보스 (2nd Boss)",
                HintText = "보스 HP가 100을 넘습니다.\n[🔥 CRIT] 향신료를 투자해 치명타를 활성화하세요.",
                RecATK = 6, RecSPEED = 5, RecINCOME = 2, RecCRIT = 2,
                BossStrategy = "HP 101 / 필요 DPS ~6.7 / CRIT 10% 이상 권장"
            },
            new DifficultyMilestone {
                StartStage = 11, EndStage = 19,
                TierName = "Tier 5 — 전략 분기 (Strategy Fork)",
                HintText = "골드가 부족하면 낮은 스테이지에서 파밍하세요.\n[💰 INCOME] 업그레이드로 자동 수익을 늘리세요.",
                RecATK = 8, RecSPEED = 6, RecINCOME = 5, RecCRIT = 3,
                BossStrategy = ""
            },
            new DifficultyMilestone {
                StartStage = 20, EndStage = 20,
                TierName = "Tier 6 — 세 번째 보스 (3rd Boss)",
                HintText = "공격력만으로는 한계입니다.\nCRIT Lv.3 이상이 사실상 필수입니다.",
                RecATK = 10, RecSPEED = 8, RecINCOME = 6, RecCRIT = 4,
                BossStrategy = "HP 825 / 필요 DPS ~55 / CRIT 20% + ATK 11 이상"
            },
            new DifficultyMilestone {
                StartStage = 21, EndStage = 29,
                TierName = "Tier 7 — 가속 성장 (Accelerated Growth)",
                HintText = "자동공격 간격을 0.5초 이하로 줄이는 것이 핵심입니다.\n파밍 구간을 잘 선택하세요.",
                RecATK = 13, RecSPEED = 10, RecINCOME = 8, RecCRIT = 5,
                BossStrategy = ""
            },
            new DifficultyMilestone {
                StartStage = 30, EndStage = 30,
                TierName = "Tier 8 — 네 번째 보스 (4th Boss)",
                HintText = "보스 HP가 3,000을 넘습니다.\n모든 업그레이드를 균형 있게 강화하세요.",
                RecATK = 15, RecSPEED = 12, RecINCOME = 10, RecCRIT = 7,
                BossStrategy = "HP 6,722 → 2.5배 후 ~2,900 / DPS 193+ 필요"
            },
            new DifficultyMilestone {
                StartStage = 31, EndStage = 49,
                TierName = "Tier 9 — 무한 도전 (Endless Challenge)",
                HintText = "이 구간부터는 자동 수익의 비중이 커집니다.\n방치 후 귀환하여 대량 업그레이드하는 패턴이 효과적입니다.",
                RecATK = 20, RecSPEED = 15, RecINCOME = 15, RecCRIT = 10,
                BossStrategy = ""
            },
            new DifficultyMilestone {
                StartStage = 50, EndStage = int.MaxValue,
                TierName = "Tier 10 — 전설 (Legend)",
                HintText = "당신은 진정한 전설의 불꽃 셰프입니다!\n순간 DPS를 최대화하기 위해 CRIT + SPEED에 집중하세요.",
                RecATK = 25, RecSPEED = 20, RecINCOME = 18, RecCRIT = 15,
                BossStrategy = "HP > 100,000 / CRIT 50% + 배율 4.0x 목표"
            }
        };

        /// <summary>
        /// 현재 스테이지에 해당하는 난이도 마일스톤 반환
        /// </summary>
        public static DifficultyMilestone GetMilestone(int stage)
        {
            foreach (var m in Milestones)
            {
                if (stage >= m.StartStage && stage <= m.EndStage)
                    return m;
            }
            return Milestones[Milestones.Count - 1];
        }

        /// <summary>
        /// 현재 업그레이드 상태로 보스를 처치할 수 있는지 예측
        /// </summary>
        public static bool CanDefeatBoss(int stage, int atkLevel, int speedLevel, int critLevel)
        {
            float attackDmg = GameConstants.BaseAttack + atkLevel;
            float critChance = Mathf.Min(0.5f, critLevel * 0.05f);
            float critMult = GameConstants.BaseCritMultiplier + critLevel * 0.2f;
            float avgDmgPerHit = attackDmg * (1f + critChance * (critMult - 1f));

            float interval = Mathf.Max(GameConstants.MinAutoAttackInterval,
                                        GameConstants.AutoAttackInterval - speedLevel * 0.05f);
            // 자동 DPS만 계산 (탭 제외)
            float autoDPS = avgDmgPerHit / interval;

            float bossHP = GameConstants.GetBossHP(stage);
            float requiredDPS = bossHP / GameConstants.BossTimerDuration;

            // 탭을 분당 60회 가정 (1tps)
            float tapDPS = avgDmgPerHit * 1f;
            float totalDPS = autoDPS + tapDPS;

            return totalDPS >= requiredDPS * 0.8f; // 80% 기준 (탭 여유분)
        }
    }
}
