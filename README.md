# 전설의 불꽃 셰프 🔥👨‍🍳

> 방치형 클리커 플레이어블 게임 — Unity 6 · Procedural Pixel Art · Mobile-First

[![Unity](https://img.shields.io/badge/Unity-6000.3.10f1-black?logo=unity&logoColor=white)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-Mobile%20Web%20%7C%20iOS%20%7C%20Android-blue?logo=google-chrome)](https://unity.com/)
[![Genre](https://img.shields.io/badge/Genre-Idle%20Clicker-orange)](https://github.com)
[![Scripts](https://img.shields.io/badge/Scripts-25%20C%23-blueviolet)](https://github.com)
[![Branch](https://img.shields.io/badge/Branch-main-brightgreen)](https://github.com/JEO-tech-ai/playable_games/tree/main)
[![Release](https://img.shields.io/badge/Release-v1.0.0-success?logo=github)](https://github.com/JEO-tech-ai/playable_games/releases)
[![License](https://img.shields.io/badge/License-Proprietary-red)](https://github.com)

---

## 스크린샷

| 타이틀 화면 | 일반 전투 (Stage 7) | 보스전 (Stage 10) |
|:-----------:|:-------------------:|:-----------------:|
| ![메뉴](My%20project/Assets/Screenshots/legend_chef_menu.png) | ![게임플레이](My%20project/Assets/Screenshots/legend_chef_gameplay.png) | ![보스전](My%20project/Assets/Screenshots/legend_chef_boss.png) |
| 시작 화면, 요리사 캐릭터 | 슬래시 이펙트 · 콤보 · 업그레이드 UI | 보스 타이머 · 치명타 · FEVER 콤보 |

---

## 게임 소개

**전설의 불꽃 셰프**는 요리사가 되어 탭과 자동 공격으로 끝없이 몰려오는 식재료 몬스터를 요리하고 무한히 성장하는 방치형 클리커 액션 게임입니다.

```
화면을 탭하면 적을 공격! 재화로 능력을 강화! 보스를 처치하고 전설의 향신료를 획득!
```

---

## 주요 플레이 화면

### 메인 게임 화면

```
┌─────────────────────────────────┐
│  Stage 1                        │  ← 스테이지 정보 (보스전 시 빨간 타이머)
│                                 │
│  [👨‍🍳]          [🍄]            │  ← 플레이어(좌) vs 몬스터(우)
│                                 │
│                 -23  💥         │  ← 피해량 플로팅 텍스트 (노란=치명타)
├─────────────────────────────────┤
│  🪙 Gold: 125    🌶 Spice: 3    │  ← 보유 재화
│ ┌─────────────┐ ┌─────────────┐ │
│ │⚔ ATK  Lv.3 │ │⚡SPEED Lv.2│ │  ← 업그레이드 버튼 (4개)
│ │ DMG: 4     │ │ 0.9s       │ │
│ │ Cost: 34🪙  │ │ Cost: 22🪙  │ │
│ └─────────────┘ └─────────────┘ │
│ ┌─────────────┐ ┌─────────────┐ │
│ │💰INCOME Lv.1│ │🔥CRIT  Lv.2│ │
│ │ 1.5/sec    │ │ 10% x2.4   │ │
│ │ Cost: 15🪙  │ │ Cost: 3🌶   │ │
│ └─────────────┘ └─────────────┘ │
└─────────────────────────────────┘
```

### 보스 등장 화면

```
┌─────────────────────────────────┐
│  Stage 5  ████████░░░░  8.3s   │  ← 보스 타이머 바 (빨간색, 줄어듦)
│                                 │
│  [👨‍🍳]         [🐙 BOSS]       │  ← 2배 크기의 보스 등장
│                                 │
│              -156 💛 CRIT!      │  ← 치명타 텍스트 (노란색)
├─────────────────────────────────┤
│  🪙 Gold: 580    🌶 Spice: 5    │
└─────────────────────────────────┘
```

### 보스 실패 팝업

```
        ┌───────────────────┐
        │   ⚠ 요리 실패!   │
        │                   │
        │ 이전 스테이지로   │
        │ 후퇴하여 다시     │
        │ 수련합니다.       │
        │                   │
        │    [  확인  ]     │
        └───────────────────┘
```

---

## 난이도 구성 (10단계)

게임은 스테이지 진행에 따라 자연스럽게 10개의 난이도 구간으로 설계됩니다.

| 구간 | 스테이지 | 단계 | 핵심 도전 | 권장 전략 |
|------|---------|------|----------|----------|
| **1. 입문** | 1~4 | 초보 | 기본 메커니즘 학습 | ATK 업그레이드 우선 |
| **2. 첫 보스** | 5 | 쉬움 | 첫 보스 처치 (15초) | 탭 연타 + 자동공격 |
| **3. 가속** | 6~9 | 보통- | HP 급증 시작 체감 | SPEED 업그레이드 |
| **4. 두 번째 보스** | 10 | 보통 | 보스 HP 2배 이상 | 향신료로 CRIT 강화 |
| **5. 전략 분기** | 11~19 | 보통+ | 골드 부족 & 파밍 필요 | INCOME으로 수동 파밍 |
| **6. 세 번째 보스** | 20 | 어려움- | ATK만으론 한계 | CRIT Lv.3+ 필수 |
| **7. 가속 성장** | 21~29 | 어려움 | 보스 HP > 1,000 | 복합 전략 |
| **8. 네 번째 보스** | 30 | 어려움+ | 보스 HP ~3,000 | 모든 업그레이드 균형 |
| **9. 무한 도전** | 31~49 | 고수 | 자동 수익 의존도 증가 | INCOME + CRIT 집중 |
| **10. 전설** | 50+ | 마스터 | 지수 성장의 극한 | 순간 DPS 최대화 |

### 보스 HP 기준표

| 보스 스테이지 | 보스 HP (근사값) | 15초 DPS 필요량 |
|-------------|----------------|----------------|
| 5 | 35 | 2.3 |
| 10 | 101 | 6.7 |
| 15 | 289 | 19.3 |
| 20 | 825 | 55 |
| 25 | 2,355 | 157 |
| 30 | 6,722 | 448 |

---

## 게임 메커니즘

### 기본 규칙

| 요소 | 설명 |
|------|------|
| **탭 공격** | 게임 영역(상단 50%) 탭 시 즉시 공격 (쿨다운 없음) |
| **자동 공격** | 설정 주기마다 자동으로 적 공격 |
| **자동 수익** | 매초 골드 자동 획득 |
| **보스** | 5스테이지마다 등장, 15초 제한 |
| **보스 실패** | 재화 유지, 4스테이지 후퇴 |

### 수치 파라미터

| 파라미터 | 기본값 | 비고 |
|---------|--------|------|
| 기본 공격력 | 1 | 탭/자동 공격 공통 |
| 자동 공격 주기 | 1.0초 | 최소 0.1초 |
| 치명타 확률 | 0% | 업그레이드로 최대 50% |
| 치명타 배율 | ×2 | 업그레이드로 증가 |
| 기본 초당 골드 | 1 | 업그레이드로 증가 |
| 몬스터 기본 HP | 10 | ×1.4^(스테이지-1) |
| 몬스터 골드 보상 | 5 | ×1.3^(스테이지-1) |
| 보스 HP | 일반의 2.5배 | |
| 보스 제한 시간 | 15초 | |

### 업그레이드 시스템

```
골드(🪙) 소모:
  [⚔ ATK]    레벨당 공격력 +1    | 기본 비용 10골드, 레벨당 ×1.5
  [⚡ SPEED]  레벨당 간격 -0.05초 | 기본 비용 10골드, 레벨당 ×1.5
  [💰 INCOME] 레벨당 수입 +0.5/초 | 기본 비용 10골드, 레벨당 ×1.5

향신료(🌶) 소모:
  [🔥 CRIT]  레벨당 치명타 +5%, 배율 +0.2 | 기본 비용 1개, 레벨당 +1
```

---

## 아키텍처

### 폴더 구조

```
Assets/Scripts/LegendChef/
├── Core/
│   ├── GameConstants.cs          ← 모든 수치 상수 (싱글 소스)
│   ├── LegendChefGameManager.cs  ← 게임 상태 관리 (Menu/Playing/BossTimeout)
│   └── CurrencySystem.cs         ← 골드/향신료 관리 + 자동 수입
├── Combat/
│   ├── CombatSystem.cs           ← 데미지/치명타 계산
│   ├── PlayerController.cs       ← 자동공격 + 탭공격
│   └── EnemyController.cs        ← 적 이동/HP/피격
├── Progression/
│   ├── StageManager.cs           ← 스테이지 진행/보스 판정
│   └── UpgradeSystem.cs          ← 4종 업그레이드 (ATK/SPEED/INCOME/CRIT)
├── Effects/
│   ├── FloatingTextManager.cs    ← 피해량 텍스트 (떠오름+페이드)
│   └── ScreenShakeEffect.cs      ← 카메라 진동 (보스 처치/치명타)
├── Visual/
│   └── PixelArtGenerator.cs      ← 프로시저럴 픽셀 아트 스프라이트
├── UI/
│   ├── LegendChefHUD.cs          ← 하단 UI 캔버스 (재화+업그레이드)
│   ├── MenuScreen.cs             ← 타이틀 화면
│   └── BossTimeoutPopup.cs       ← 보스 실패 팝업
└── LegendChefSceneSetup.cs       ← 씬 부트스트랩 (모든 것을 런타임에 생성)
```

### 리소스 교체 가이드

`PixelArtGenerator.cs`의 메서드만 교체하면 전체 비주얼 변경 가능:

```csharp
CreatePlayerSprite()      // 플레이어 스프라이트 (64×64)
CreateEnemyBasicSprite()  // 일반 몬스터 스프라이트 (64×64)
CreateEnemyBossSprite()   // 보스 스프라이트 (128×128)
CreateCoinSprite()        // 골드 아이콘 (32×32)
CreateSpiceSprite()       // 향신료 아이콘 (32×32)
CreateBackgroundSprite()  // 배경 (전체)
```

---

## 빠른 시작

### 요구 사항

- Unity **6000.3.10f1** (Unity 6)
- com.unity.ugui 2.0.0+

### 실행 방법

1. Unity Hub에서 `My project` 폴더를 열기
2. `Assets/Scenes/LegendChef.unity` 씬 열기
3. Play 모드 실행 ▶

### 빌드 방법

1. File → Build Profiles
2. **Web - Mobile - Release** 프로파일 선택
3. Build Settings에서 `LegendChef` 씬 추가
4. Build And Run

---

## 브랜치 전략

| 브랜치 | 설명 |
|--------|------|
| `main` | 기본 샘플 프로젝트 (베이스라인) |
| `feature/hybrid-puzzle` | 하이브리드 퍼즐 플레이어블 게임 (ColorDrive) |
| `dev_legend` | **전설의 불꽃 셰프** (현재 브랜치) |

---

*Unity 6 · Procedural Pixel Art · No Prefabs · Mobile-First 9:16*
