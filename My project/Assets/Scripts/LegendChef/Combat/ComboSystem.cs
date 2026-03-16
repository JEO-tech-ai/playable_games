using System;
using UnityEngine;

namespace LegendChef.Combat
{
    /// <summary>
    /// 탭 콤보 시스템 (싱글톤). 연속 탭으로 데미지 배율 증가
    /// </summary>
    public class ComboSystem : MonoBehaviour
    {
        public static ComboSystem Instance { get; private set; }

        public event Action<int, float> OnComboChanged;
        public event Action OnComboBreak;
        public event Action<int> OnComboMilestone;

        public int CurrentCombo { get; private set; }
        public float ComboMultiplier { get; private set; } = 1f;

        private float _lastTapTime;
        private const float ComboTimeout = 1.5f;

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// 탭 공격마다 호출
        /// </summary>
        public void RegisterTap()
        {
            _lastTapTime = Time.time;
            CurrentCombo++;

            UpdateMultiplier();
            OnComboChanged?.Invoke(CurrentCombo, ComboMultiplier);

            // 마일스톤 체크
            if (CurrentCombo == 10 || CurrentCombo == 25 || CurrentCombo == 50)
            {
                OnComboMilestone?.Invoke(CurrentCombo);
            }
        }

        private void Update()
        {
            // 타임아웃 체크
            if (CurrentCombo > 0 && Time.time - _lastTapTime > ComboTimeout)
            {
                BreakCombo();
            }
        }

        private void BreakCombo()
        {
            if (CurrentCombo > 0)
            {
                CurrentCombo = 0;
                ComboMultiplier = 1f;
                OnComboBreak?.Invoke();
            }
        }

        private void UpdateMultiplier()
        {
            if (CurrentCombo >= 50)
                ComboMultiplier = 3.0f;
            else if (CurrentCombo >= 25)
                ComboMultiplier = 2.0f;
            else if (CurrentCombo >= 10)
                ComboMultiplier = 1.5f;
            else
                ComboMultiplier = 1.0f;
        }

        /// <summary>
        /// 현재 콤보 등급 이름
        /// </summary>
        public string GetComboTierName()
        {
            if (CurrentCombo >= 50) return "LEGENDARY!";
            if (CurrentCombo >= 25) return "BLAZING!";
            if (CurrentCombo >= 10) return "FEVER!";
            return "";
        }
    }
}
