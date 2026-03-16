# Phase 2: Unity 아키텍처 & 폴더 구조

**작성일:** 2026-03-16
**상태:** 완료

## 폴더 구조

```
Assets/
├── Scenes/
│   └── SampleScene.unity         (기존)
├── Scripts/
│   ├── Managers/
│   │   ├── GameManager.cs        ✅ 싱글톤, 게임 상태 관리
│   │   ├── LevelManager.cs       ✅ 레벨 로딩, 잠금 해제
│   │   ├── ThemeManager.cs       ✅ 브랜드 테마 시스템
│   │   ├── AdManager.cs          ✅ 광고 Mock (Unity Ads 호환)
│   │   └── IAPManager.cs         ✅ IAP Mock (Unity IAP 호환)
│   ├── BlockPuzzle/
│   │   ├── BlockGrid.cs          (Phase 3에서 생성)
│   │   ├── BlockPiece.cs         (Phase 3에서 생성)
│   │   └── BlockPuzzleController.cs (Phase 3에서 생성)
│   ├── WaterSort/
│   │   ├── Tube.cs               (Phase 4에서 생성)
│   │   ├── LiquidRenderer.cs     (Phase 4에서 생성)
│   │   └── WaterSortController.cs (Phase 4에서 생성)
│   ├── UI/
│   │   ├── HUDController.cs      (Phase 5에서 생성)
│   │   ├── LevelSelectUI.cs      (Phase 5에서 생성)
│   │   └── ResultScreenUI.cs     (Phase 5에서 생성)
│   └── Data/
│       ├── LevelData.cs          ✅ ScriptableObject
│       └── ThemeConfig.cs        ✅ ScriptableObject
├── Prefabs/
├── Materials/
├── UI/
├── Resources/
│   └── Themes/
└── ScriptableObjects/
```

## 주요 아키텍처 패턴

### 싱글톤 매니저
- `GameManager`: 게임 상태 FSM (MainMenu → Playing → Paused → LevelComplete/GameOver)
- `LevelManager`: 레벨 진행, 잠금해제
- `ThemeManager`: 런타임 테마 교체
- `AdManager`: 광고 추상화 레이어
- `IAPManager`: 결제 추상화 레이어

### ScriptableObject 데이터
- `LevelData`: 레벨별 설정 (그리드 크기, 색상, 제한)
- `ThemeConfig`: 브랜드별 비주얼/오디오 설정

### 이벤트 시스템
- `GameManager.OnStateChanged` → UI 반응
- `GameManager.OnScoreChanged` → HUD 업데이트
- `ThemeManager.OnThemeChanged` → 전체 UI 리스킨

## 의존성 흐름
```
GameManager
    ↓ 상태 이벤트
LevelManager → LevelData (SO)
ThemeManager → ThemeConfig (SO)
AdManager    ← GameManager (레벨 클리어 시)
IAPManager   → AdManager, ThemeManager, GameManager
```

## IAP 상품 목록
| ID | 가격 | 타입 |
|----|------|------|
| no_ads | $3.99 | NonConsumable |
| hint_pack_5 | $0.99 | Consumable |
| extra_moves_10 | $0.99 | Consumable |
| coins_1000 | $0.99 | Consumable |
| coins_6000 | $4.99 | Consumable |
| coins_15000 | $9.99 | Consumable |
| theme_silver | $1.99 | NonConsumable |
| theme_amgblack | $2.99 | NonConsumable |
| starter_pack | $2.99 | Consumable |
| vip_weekly | $0.99/주 | Subscription |
| vip_monthly | $2.99/월 | Subscription |
