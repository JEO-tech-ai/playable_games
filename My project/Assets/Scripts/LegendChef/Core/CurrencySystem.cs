using System;
using UnityEngine;

namespace LegendChef.Core
{
    /// <summary>
    /// 골드와 향신료를 관리하는 싱글톤
    /// </summary>
    public class CurrencySystem : MonoBehaviour
    {
        public static CurrencySystem Instance { get; private set; }

        public event Action<float> OnGoldChanged;
        public event Action<int> OnSpiceChanged;

        private float _gold;
        private int _spice;
        private float _incomePerSec;

        public float Gold => _gold;
        public int Spice => _spice;
        public float IncomePerSec
        {
            get => _incomePerSec;
            set => _incomePerSec = value;
        }

        private void Awake()
        {
            Instance = this;
            _gold = 0f;
            _spice = 0;
            _incomePerSec = GameConstants.BaseGoldPerSec;
        }

        private void Update()
        {
            if (LegendChefGameManager.Instance == null) return;
            if (LegendChefGameManager.Instance.CurrentState != GameState.Playing) return;

            float income = _incomePerSec * Time.deltaTime;
            if (income > 0f)
            {
                _gold += income;
                OnGoldChanged?.Invoke(_gold);
            }
        }

        public void AddGold(float amount)
        {
            _gold += amount;
            OnGoldChanged?.Invoke(_gold);
        }

        public bool SpendGold(float amount)
        {
            if (_gold < amount) return false;
            _gold -= amount;
            OnGoldChanged?.Invoke(_gold);
            return true;
        }

        public bool CanAffordGold(float amount)
        {
            return _gold >= amount;
        }

        public void AddSpice(int amount)
        {
            _spice += amount;
            OnSpiceChanged?.Invoke(_spice);
        }

        public bool SpendSpice(int amount)
        {
            if (_spice < amount) return false;
            _spice -= amount;
            OnSpiceChanged?.Invoke(_spice);
            return true;
        }

        public bool CanAffordSpice(int amount)
        {
            return _spice >= amount;
        }
    }
}
