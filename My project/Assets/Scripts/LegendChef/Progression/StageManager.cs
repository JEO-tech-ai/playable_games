using System;
using UnityEngine;
using LegendChef.Core;

namespace LegendChef.Progression
{
    /// <summary>
    /// 스테이지 진행을 관리하는 싱글톤
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        public event Action<int> OnStageChanged;

        [Header("현재 스테이지")]
        [SerializeField] private int _currentStage = 1;

        public int CurrentStage => _currentStage;

        public bool IsBossStage => GameConstants.IsBossStage(_currentStage);

        private void Awake()
        {
            Instance = this;
        }

        public float GetCurrentEnemyHP()
        {
            if (IsBossStage)
                return GameConstants.GetBossHP(_currentStage);
            return GameConstants.GetMonsterHP(_currentStage);
        }

        public float GetCurrentGoldReward()
        {
            return GameConstants.GetMonsterGoldReward(_currentStage);
        }

        public int GetCurrentSpiceReward()
        {
            return GameConstants.GetBossReward();
        }

        public void AdvanceStage()
        {
            _currentStage++;
            OnStageChanged?.Invoke(_currentStage);
        }

        /// <summary>
        /// 보스 타임아웃 시 4스테이지 후퇴
        /// </summary>
        public void RetreatOnBossTimeout()
        {
            _currentStage = Mathf.Max(1, _currentStage - GameConstants.BossTimeoutRetreatStages);
            OnStageChanged?.Invoke(_currentStage);
        }
    }
}
