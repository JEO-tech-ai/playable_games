using System;
using UnityEngine;

namespace LegendChef.Core
{
    public enum GameState
    {
        Menu,
        Playing,
        BossTimeout
    }

    /// <summary>
    /// 게임 상태를 관리하는 싱글톤
    /// </summary>
    public class LegendChefGameManager : MonoBehaviour
    {
        public static LegendChefGameManager Instance { get; private set; }

        public event Action<GameState> OnStateChanged;

        private GameState _currentState = GameState.Menu;

        public GameState CurrentState => _currentState;

        private void Awake()
        {
            Instance = this;
        }

        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
            OnStateChanged?.Invoke(_currentState);
        }

        public GameState GetState()
        {
            return _currentState;
        }
    }
}
