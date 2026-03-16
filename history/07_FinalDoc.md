# 최종 기술 문서 — Color Drive: Hybrid Puzzle Game Framework

**작성일:** 2026-03-16
**버전:** 1.0.0
**Unity 버전:** 6000.3.10f1

---

## 1. 프로젝트 개요

Mercedes-Benz 광고 타겟 하이브리드 퍼즐 게임. LEGO 블록 슬라이드 퍼즐 + 컬러 워터 소트의 두 메카닉을 결합한 Playable Ad 프레임워크.

### 핵심 특징
- **양산형 아키텍처**: ThemeConfig SO 교체만으로 다른 브랜드 즉시 적용
- **3가지 게임 모드**: BlockPuzzle / WaterSort / Hybrid
- **수익화 구조 내장**: 광고(Rewarded/Interstitial/Banner) + IAP 완전 추상화
- **Mock-to-Real 전환**: AdManager, IAPManager를 실제 SDK로 교체 가능

---

## 2. 스크립트 구조

```
Assets/Scripts/
├── Managers/
│   ├── GameManager.cs          — 게임 상태 FSM, 점수/코인
│   ├── LevelManager.cs         — 레벨 로딩, 잠금해제
│   ├── ThemeManager.cs         — 런타임 브랜드 테마 교체
│   ├── AdManager.cs            — 광고 추상화 (Unity Ads 호환)
│   ├── IAPManager.cs           — IAP 추상화 (Unity IAP 호환)
│   ├── GameBootstrap.cs        — 부트스트랩 씬 진입점
│   ├── GameController.cs       — 게임 씬 와이어링
│   └── TweenHelper.cs          — 경량 코루틴 기반 트윈
├── BlockPuzzle/
│   ├── BlockGrid.cs            — 8×8 그리드, 슬라이드, 클리어
│   ├── BlockPiece.cs           — 컬러 블록 조각
│   └── BlockPuzzleController.cs — 퍼즐 로직, 입력, 타이머
├── WaterSort/
│   ├── Tube.cs                 — 액체 튜브, Push/Pop/Pour
│   └── WaterSortController.cs  — 정렬 로직, Undo, 승리 판정
├── UI/
│   ├── HUDController.cs        — 점수/타이머/무브/부스터 HUD
│   ├── ResultScreenUI.cs       — 결과 화면 (성공/실패)
│   ├── PauseMenuUI.cs          — 일시정지 메뉴
│   └── LevelSelectUI.cs        — 레벨 선택 (모드 전환 포함)
└── Data/
    ├── LevelData.cs            — ScriptableObject 레벨 설정
    └── ThemeConfig.cs          — ScriptableObject 브랜드 설정
```

---

## 3. 씬 구조

| 씬 | 역할 |
|----|------|
| Bootstrap | 매니저 초기화, ThemeConfig 로드 |
| MainMenu | 메뉴, 레벨 선택, 스토어 |
| GameScene | 실제 퍼즐 게임 플레이 |

---

## 4. 신규 브랜드 적용 방법

### 4.1 ThemeConfig 생성
1. Project → Create → ColorDrive → Theme Config
2. 브랜드명, 컬러, 로고 스프라이트, CTA 설정
3. `Assets/Resources/Themes/[BrandName]/ThemeConfig.asset`으로 저장

### 4.2 테마 적용
```csharp
// 코드에서
ThemeManager.Instance.LoadThemeByName("Mercedes");

// 또는 Inspector에서
// ThemeManager.defaultTheme 슬롯에 ThemeConfig 할당
```

---

## 5. 신규 레벨 추가 방법

1. Project → Create → ColorDrive → Level Data
2. 파라미터 설정:
   - `puzzleMode`: BlockPuzzle / WaterSort / Hybrid
   - Block: `gridWidth`, `gridHeight`, `moveLimit`, `timeLimit`
   - Water: `tubeCount`, `layerDepth`, `waterSortMoveLimit`
   - 공통: `colorPalette`, `difficultyRating`
3. LevelManager.allLevels[] 배열에 추가

---

## 6. 광고 SDK 교체 방법

`AdManager.cs`의 주석 처리된 줄을 활성화:
```csharp
// TODO 주석 → 실제 Unity Ads SDK 코드로 교체
// Advertisement.Show(placement, listener);
// Advertisement.Banner.Show("Banner");
```

---

## 7. IAP SDK 교체 방법

1. Unity Package Manager에서 Unity IAP 설치
2. `IAPManager.cs`의 `Purchase()` 메서드에서 실제 IAP 호출:
```csharp
// TODO → m_StoreController.InitiatePurchase(productId);
```
3. `IStoreListener` 인터페이스 구현하여 콜백 처리

---

## 8. 게임 플레이 플로우

```
Bootstrap
    └→ MainMenu
           └→ LevelSelect (모드 선택: Block / Water / Hybrid)
                  └→ GameScene
                        ├→ GameController.SetupGame()
                        │     ├→ BlockPuzzleController.StartLevel()
                        │     └→ WaterSortController.StartLevel() (또는 순차)
                        ├→ HUD (점수, 타이머, 무브, 부스터)
                        └→ ResultScreen
                              ├→ 성공: 별점, 코인, 다음 레벨
                              └→ 실패: 재시작, 광고로 계속, 홈
```

---

## 9. 수익화 통합 포인트

| 이벤트 | 광고/IAP 트리거 |
|--------|----------------|
| 게임오버 | 보상형 광고 → 무브 +5 |
| 힌트 버튼 | 보상형 광고 또는 힌트팩 IAP |
| 레벨 클리어 | 보상형 광고 → 코인 2배 |
| 레벨 클리어 3회마다 | 전면 광고 자동 노출 |
| 게임 중 | 배너 광고 (상단 고정) |
| 스토어 버튼 | IAP 상품 목록 |

---

## 10. 생성된 에셋 목록

### ScriptableObjects
- `Level_001_Block.asset` — Block 기초 Lv1
- `Level_002_Block.asset` — Block 심화 Lv2
- `Level_011_Water.asset` — Water 기초 Lv11
- `Level_041_Hybrid.asset` — Hybrid Lv41
- `ThemeConfig_Mercedes.asset` — Mercedes-Benz 테마
- `ThemeConfig_Generic.asset` — 기본 테마

---

## 11. Ralph Loop 실행 기록

| Phase | 상태 | 내용 |
|-------|------|------|
| 1 GDD | ✅ | 게임 기획, 메카닉, 수익화 설계 |
| 2 Architecture | ✅ | 폴더 구조, 매니저 시스템 |
| 3 Block Puzzle | ✅ | 그리드, 피스, 컨트롤러 구현 |
| 4 Water Sort | ✅ | 튜브, 붓기, 정렬 구현 |
| 5 UI/UX | ✅ | HUD, 결과화면, 일시정지, 레벨선택 |
| 6 Level Data | ✅ | SO 에셋 생성, 테마 파이프라인 |
| 7 Code Review | ✅ | CRITICAL 2 + HIGH 4 수정 완료 |

**컴파일 상태: 에러 0개** ✅
