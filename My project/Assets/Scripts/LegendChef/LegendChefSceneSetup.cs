using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using LegendChef.Core;
using LegendChef.Combat;
using LegendChef.Progression;
using LegendChef.Effects;
using LegendChef.Visual;
using LegendChef.UI;

namespace LegendChef
{
    /// <summary>
    /// 메인 엔트리 포인트. 씬의 모든 오브젝트를 런타임에 생성한다.
    /// </summary>
    public class LegendChefSceneSetup : MonoBehaviour
    {
        public static LegendChefSceneSetup Instance { get; private set; }

        [Header("게임 영역 경계")]
        private float _worldLeft;
        private float _worldRight;
        private float _worldBottom;
        private float _worldTop;
        private float _gameAreaTop;    // 게임 영역 상단 (화면 상단)
        private float _gameAreaBottom; // 게임 영역 하단 (화면 중간)

        private float _playerX;
        private float _enemyTargetX;
        private float _enemySpawnX;
        private float _gameAreaCenterY;

        // 현재 적
        private EnemyController _currentEnemy;
        public EnemyController CurrentEnemy => _currentEnemy;

        // 보스 타이머
        private Coroutine _bossTimerCoroutine;
        private bool _bossTimedOut;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SetupCamera();
            CalculateWorldBounds();
            CreateBackground();
            CreateSingletons();
            CreatePlayer();
            CreateHUD();
            CreateMenuScreen();
            CreateBossTimeoutPopup();
            CreateEventSystem();

            // 게임 상태 변경 이벤트 구독
            LegendChefGameManager.Instance.OnStateChanged += OnGameStateChanged;
        }

        // ── 카메라 설정 ──

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camGO = new GameObject("MainCamera");
                cam = camGO.AddComponent<Camera>();
                camGO.tag = "MainCamera";
            }

            cam.orthographic = true;
            // 9:16 비율에 맞춘 orthographic size
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.backgroundColor = new Color(0.17f, 0.24f, 0.31f);
            cam.clearFlags = CameraClearFlags.SolidColor;

            // 화면 흔들림 이펙트 추가
            cam.gameObject.AddComponent<ScreenShakeEffect>();
        }

        // ── 월드 좌표 계산 ──

        private void CalculateWorldBounds()
        {
            Camera cam = Camera.main;
            float orthoSize = cam.orthographicSize;
            float aspect = (float)Screen.width / Screen.height;

            _worldTop = orthoSize;
            _worldBottom = -orthoSize;
            _worldLeft = -orthoSize * aspect;
            _worldRight = orthoSize * aspect;

            float worldWidth = _worldRight - _worldLeft;
            float worldHeight = _worldTop - _worldBottom;

            // 게임 영역: 상단 50%
            _gameAreaTop = _worldTop;
            _gameAreaBottom = 0f; // 화면 중간
            _gameAreaCenterY = (_gameAreaTop + _gameAreaBottom) / 2f;

            // 플레이어 위치: 화면 너비의 30%
            _playerX = _worldLeft + worldWidth * GameConstants.PlayerXRatio;

            // 적 목표 위치: 화면 너비의 60%
            _enemyTargetX = _worldLeft + worldWidth * GameConstants.EnemyTargetXRatio;

            // 적 스폰 위치: 화면 오른쪽 밖
            _enemySpawnX = _worldRight + 1f;
        }

        // ── 배경 생성 ──

        private void CreateBackground()
        {
            // 게임 영역 배경
            GameObject bgGO = new GameObject("GameBackground");
            SpriteRenderer sr = bgGO.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArtGenerator.CreateBackgroundSprite(256, 256);
            sr.sortingOrder = -10;

            float scaleX = (_worldRight - _worldLeft) / sr.sprite.bounds.size.x;
            float scaleY = (_gameAreaTop - _gameAreaBottom) / sr.sprite.bounds.size.y;
            bgGO.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            bgGO.transform.position = new Vector3(
                (_worldLeft + _worldRight) / 2f,
                _gameAreaCenterY,
                1f);

            // 바닥선
            GameObject floorGO = new GameObject("Floor");
            SpriteRenderer floorSR = floorGO.AddComponent<SpriteRenderer>();
            floorSR.sprite = PixelArtGenerator.CreateWhiteSprite();
            floorSR.color = new Color(0.3f, 0.4f, 0.3f);
            floorSR.sortingOrder = 0;

            float floorWidth = (_worldRight - _worldLeft) / floorSR.sprite.bounds.size.x;
            floorGO.transform.localScale = new Vector3(floorWidth, 0.15f, 1f);
            floorGO.transform.position = new Vector3(
                (_worldLeft + _worldRight) / 2f,
                _gameAreaBottom + 0.3f,
                0f);
        }

        // ── 싱글톤 생성 ──

        private void CreateSingletons()
        {
            // GameManager
            GameObject gmGO = new GameObject("GameManager");
            gmGO.AddComponent<LegendChefGameManager>();

            // CurrencySystem
            GameObject csGO = new GameObject("CurrencySystem");
            csGO.AddComponent<CurrencySystem>();

            // StageManager
            GameObject smGO = new GameObject("StageManager");
            smGO.AddComponent<StageManager>();

            // UpgradeSystem
            GameObject usGO = new GameObject("UpgradeSystem");
            usGO.AddComponent<UpgradeSystem>();

            // CombatSystem
            GameObject combatGO = new GameObject("CombatSystem");
            combatGO.AddComponent<CombatSystem>();

            // FloatingTextManager
            GameObject ftGO = new GameObject("FloatingTextManager");
            ftGO.AddComponent<FloatingTextManager>();

            // ComboSystem
            GameObject comboGO = new GameObject("ComboSystem");
            comboGO.AddComponent<ComboSystem>();

            // TensionManager
            GameObject tensionGO = new GameObject("TensionManager");
            tensionGO.AddComponent<TensionManager>();
        }

        // ── 플레이어 생성 ──

        private void CreatePlayer()
        {
            GameObject playerGO = new GameObject("Player");
            PlayerController pc = playerGO.AddComponent<PlayerController>();
            pc.InitializeVisual();
            playerGO.transform.position = new Vector3(_playerX, _gameAreaCenterY - 0.5f, 0f);
        }

        // ── HUD 생성 ──

        private void CreateHUD()
        {
            GameObject hudGO = new GameObject("HUD");
            LegendChefHUD hud = hudGO.AddComponent<LegendChefHUD>();
            hud.Initialize();

            // 난이도 힌트 UI
            GameObject diffHintGO = new GameObject("DifficultyHintUI");
            DifficultyHintUI diffHint = diffHintGO.AddComponent<DifficultyHintUI>();
            diffHint.Initialize();

            // 콤보 디스플레이 UI
            GameObject comboDisplayGO = new GameObject("ComboDisplayUI");
            ComboDisplayUI comboDisplay = comboDisplayGO.AddComponent<ComboDisplayUI>();
            comboDisplay.Initialize();
        }

        // ── 메뉴 화면 ──

        private void CreateMenuScreen()
        {
            GameObject menuGO = new GameObject("MenuScreen");
            MenuScreen menu = menuGO.AddComponent<MenuScreen>();
            menu.Initialize();
        }

        // ── 보스 타임아웃 팝업 ──

        private void CreateBossTimeoutPopup()
        {
            GameObject popupGO = new GameObject("BossTimeoutPopup");
            BossTimeoutPopup popup = popupGO.AddComponent<BossTimeoutPopup>();
            popup.Initialize();
        }

        // ── EventSystem (UI 이벤트 처리에 필요) ──

        private void CreateEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<StandaloneInputModule>();
            }
        }

        // ── 게임 상태 변경 핸들러 ──

        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Playing:
                    OnStartPlaying();
                    break;
                case GameState.BossTimeout:
                    OnBossTimeout();
                    break;
            }
        }

        private void OnStartPlaying()
        {
            // 적이 없으면 새로 스폰
            if (_currentEnemy == null)
            {
                SpawnEnemy();
            }
        }

        private void OnBossTimeout()
        {
            // 텐션 매니저 알림
            if (TensionManager.Instance != null)
                TensionManager.Instance.OnBossTimeout();

            // 현재 적 제거
            if (_currentEnemy != null)
            {
                Destroy(_currentEnemy.gameObject);
                _currentEnemy = null;
            }

            if (PlayerController.Instance != null)
                PlayerController.Instance.CurrentEnemy = null;

            // 팝업 표시
            if (BossTimeoutPopup.Instance != null)
                BossTimeoutPopup.Instance.Show();
        }

        // ── 적 생성 및 라이프사이클 ──

        public void SpawnEnemy()
        {
            if (StageManager.Instance == null) return;

            bool isBoss = StageManager.Instance.IsBossStage;
            float hp = StageManager.Instance.GetCurrentEnemyHP();

            GameObject enemyGO = new GameObject(isBoss ? "Boss" : "Enemy");
            _currentEnemy = enemyGO.AddComponent<EnemyController>();
            _currentEnemy.Initialize(hp, isBoss, _enemySpawnX, _enemyTargetX);
            enemyGO.transform.position = new Vector3(
                _enemySpawnX,
                _gameAreaCenterY - 0.3f,
                0f);

            // 적 사망 이벤트
            _currentEnemy.OnDied += OnEnemyDied;

            // 플레이어에게 타겟 설정
            if (PlayerController.Instance != null)
                PlayerController.Instance.CurrentEnemy = _currentEnemy;

            // 보스면 타이머 시작 + 텐션 매니저 알림
            if (isBoss)
            {
                StartBossTimer();
                if (TensionManager.Instance != null)
                    TensionManager.Instance.OnBossSpawned(_currentEnemy);
            }
            else
            {
                if (LegendChefHUD.Instance != null)
                    LegendChefHUD.Instance.ShowBossTimer(false);
            }
        }

        private void OnEnemyDied()
        {
            if (_currentEnemy == null) return;

            bool wasBoss = _currentEnemy.IsBoss;
            Vector3 deathPos = _currentEnemy.transform.position;

            // 보스 타이머 중지
            StopBossTimer();

            // 보스 사망 이펙트
            if (wasBoss && TensionManager.Instance != null)
                TensionManager.Instance.OnBossDefeated(deathPos);

            // 보상 지급
            if (CurrencySystem.Instance != null)
            {
                float goldReward = StageManager.Instance.GetCurrentGoldReward();
                CurrencySystem.Instance.AddGold(goldReward);

                if (wasBoss)
                {
                    int spiceReward = StageManager.Instance.GetCurrentSpiceReward();
                    CurrencySystem.Instance.AddSpice(spiceReward);
                }
            }

            // 적 오브젝트 제거
            Destroy(_currentEnemy.gameObject);
            _currentEnemy = null;

            if (PlayerController.Instance != null)
                PlayerController.Instance.CurrentEnemy = null;

            // 스테이지 진행
            if (StageManager.Instance != null)
                StageManager.Instance.AdvanceStage();

            // 다음 적 스폰 (약간의 딜레이)
            StartCoroutine(SpawnEnemyDelayed(0.3f));
        }

        private IEnumerator SpawnEnemyDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (LegendChefGameManager.Instance != null &&
                LegendChefGameManager.Instance.CurrentState == GameState.Playing)
            {
                SpawnEnemy();
            }
        }

        // ── 보스 타이머 ──

        private void StartBossTimer()
        {
            StopBossTimer();
            _bossTimedOut = false;

            if (LegendChefHUD.Instance != null)
                LegendChefHUD.Instance.ShowBossTimer(true);

            _bossTimerCoroutine = StartCoroutine(BossTimerCoroutine());
        }

        private void StopBossTimer()
        {
            if (_bossTimerCoroutine != null)
            {
                StopCoroutine(_bossTimerCoroutine);
                _bossTimerCoroutine = null;
            }

            if (LegendChefHUD.Instance != null)
                LegendChefHUD.Instance.ShowBossTimer(false);
        }

        private IEnumerator BossTimerCoroutine()
        {
            float total = GameConstants.BossTimerDuration;
            float remaining = total;

            while (remaining > 0f)
            {
                remaining -= Time.deltaTime;
                if (remaining < 0f) remaining = 0f;

                if (LegendChefHUD.Instance != null)
                    LegendChefHUD.Instance.UpdateBossTimer(remaining, total);

                // 텐션 매니저 업데이트
                if (TensionManager.Instance != null)
                    TensionManager.Instance.UpdateBossTimer(remaining, total);

                yield return null;
            }

            // 타임아웃
            _bossTimedOut = true;
            _bossTimerCoroutine = null;

            if (LegendChefGameManager.Instance != null)
                LegendChefGameManager.Instance.SetState(GameState.BossTimeout);
        }

        // ── 입력 처리 ──

        private void Update()
        {
            HandleTapInput();
        }

        private void HandleTapInput()
        {
            if (LegendChefGameManager.Instance == null) return;
            if (LegendChefGameManager.Instance.CurrentState != GameState.Playing) return;

            // 마우스 클릭 또는 터치 — UI 위가 아닌 경우만
            if (Input.GetMouseButtonDown(0))
            {
                // UI 위 터치인지 확인
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                // 터치 위치가 게임 영역(상단 50%)인지 확인
                float screenY = Input.mousePosition.y;
                float screenMid = Screen.height * 0.48f; // HUD 영역 제외

                if (screenY > screenMid)
                {
                    if (PlayerController.Instance != null)
                        PlayerController.Instance.PerformTapAttack();
                }
            }

            // 모바일 터치 지원
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                        return;

                    float screenY = touch.position.y;
                    float screenMid = Screen.height * 0.48f;

                    if (screenY > screenMid)
                    {
                        if (PlayerController.Instance != null)
                            PlayerController.Instance.PerformTapAttack();
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (LegendChefGameManager.Instance != null)
                LegendChefGameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }
    }
}
