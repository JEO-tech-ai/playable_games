# Game Design Document (GDD)
## 하이브리드 퍼즐게임 — Mercedes-Benz Playable Ad Framework

**작성일:** 2026-03-16
**버전:** 1.0
**상태:** 확정

---

## 1. 프로젝트 개요

### 1.1 게임 명칭
**"Color Drive" — Block & Sort Hybrid Puzzle**

### 1.2 타겟 플랫폼
- iOS / Android (모바일 퍼스트)
- Playable Ad 포맷 (HTML5 WebGL 경량 빌드 가능)

### 1.3 장르
하이브리드 캐주얼 퍼즐 — Block Slide Puzzle + Liquid Color Sort

### 1.4 핵심 컨셉
브랜드(Mercedes-Benz) 컬러 팔레트를 기반으로:
- **Block Puzzle**: LEGO 스타일 컬러 블록을 그리드 가장자리에서 밀어 넣어 같은 색 3개 이상을 연결해 클리어
- **Water Sort**: 튜브에 담긴 자동차 페인트 컬러를 정렬
- **Hybrid Mode**: 두 메카닉을 단계별로 교차 적용

---

## 2. 게임 메카닉 (Core Mechanics)

### 2.1 Block Puzzle Mode
```
[그리드: 8x8]
- 컬러 블록이 그리드 4면 가장자리 슬롯에서 대기
- 플레이어가 슬롯을 선택 → 블록이 안쪽으로 슬라이드
- 같은 색 3×1 이상 라인 형성 시 자동 클리어
- 제한 시간 3분 또는 제한 무브 내 보드 클리어
- 보드가 가득 찰 경우 게임오버
```

**블록 타입:**
| 타입 | 크기 | 점수 배수 |
|------|------|---------|
| Small | 1×1 | 1x |
| Medium | 1×2 | 1.5x |
| Large | 1×3 | 2x |
| Special | L/T형 | 3x |

### 2.2 Water Sort Mode
```
[튜브: 6~12개]
- 각 튜브에 4~5레이어 컬러 액체
- 빈 튜브 또는 상단 동일 색 튜브로만 이동 가능
- 모든 튜브가 단일 색으로 정렬되면 클리어
- 무제한 시간, 무브 제한으로 난이도 조절
```

### 2.3 Hybrid Mode (핵심 차별화)
- **Phase 1**: Block Puzzle로 컬러 블록 클리어 → 클리어한 색이 Water Sort 튜브에 추가됨
- **Phase 2**: Water Sort로 블록 클리어에서 수집한 색들을 정렬
- **연계 보너스**: 두 단계 연속 콤보 달성 시 보너스 점수 및 광고 스킵 쿠폰 획득

---

## 3. 레벨 구조 & 난이도 곡선

### 3.1 레벨 구성
```
Lv.1~10   : Block Puzzle 기초 (4×4 그리드, 2~3 색상)
Lv.11~20  : Water Sort 기초 (4 튜브, 3 색상)
Lv.21~30  : Block Puzzle 심화 (6×6, 4~5 색상)
Lv.31~40  : Water Sort 심화 (8 튜브, 5 색상)
Lv.41~50  : Hybrid Mode 도입 (두 메카닉 교차)
Lv.51~100 : 무한 생성 (Procedural)
```

### 3.2 난이도 파라미터
- `gridSize`: 4×4 ~ 10×10
- `colorCount`: 2~8
- `tubeCount`: 4~12
- `layerDepth`: 3~6
- `moveLimit`: 10~50
- `timeLimit`: 60~180초 (0=무제한)

---

## 4. 광고 수익화 (Ad Monetization)

### 4.1 보상형 광고 (Rewarded Video)
| 트리거 | 보상 |
|--------|------|
| 게임오버 후 | 추가 5무브 / 시간 30초 추가 |
| 힌트 요청 | 최적 이동 1회 표시 |
| 레벨 클리어 | 코인 2배 획득 |
| 부스터 획득 | 무료 부스터 1개 |

### 4.2 전면 광고 (Interstitial)
- 레벨 클리어 3회마다 자동 노출
- 5초 스킵 가능 (스킵 불가 3초 보장)

### 4.3 배너 광고
- 게임 HUD 상단 고정 (120×50dp)

---

## 5. 인앱결제 (IAP System)

### 5.1 소모성 아이템
| 아이템 | 가격 | 기능 |
|--------|------|------|
| Hint Pack (5) | $0.99 | 최적 이동 힌트 5회 |
| Extra Moves (10) | $0.99 | 이동 횟수 +10 |
| Time Boost | $1.99 | 시간 +60초 |
| Coin Pack S | $0.99 | 코인 1,000개 |
| Coin Pack M | $4.99 | 코인 6,000개 |
| Coin Pack L | $9.99 | 코인 15,000개 |

### 5.2 비소모성 아이템
| 아이템 | 가격 | 기능 |
|--------|------|------|
| No Ads | $3.99 | 광고 제거 (보상형 제외) |
| Premium Theme: Benz Silver | $1.99 | Mercedes 실버 테마 |
| Premium Theme: AMG Black | $2.99 | AMG 블랙 테마 |
| Starter Pack | $2.99 | 코인5000 + 힌트10 + 테마1 |

### 5.3 구독
| 플랜 | 가격 | 혜택 |
|------|------|------|
| Weekly VIP | $0.99/주 | 광고 제거 + 일일 보너스 코인 |
| Monthly VIP | $2.99/월 | 광고 제거 + 코인 2× + 전용 테마 |

---

## 6. 브랜드 테마 시스템 (Mass Production Framework)

### 6.1 ThemeConfig ScriptableObject
```
BrandName: string
PrimaryColor: Color
SecondaryColor: Color
AccentColor: Color
LogoSprite: Sprite
BackgroundSprite: Sprite
BlockMaterial: Material
TubeColor: Color[]
UIFont: Font
SFXPack: AudioClip[]
BGMClip: AudioClip
```

### 6.2 내장 테마
- **Mercedes-Benz**: 실버/블랙/화이트 (기본)
- **Generic Puzzle**: 컬러풀 기본 테마
- **[브랜드N]**: ThemeConfig만 교체로 즉시 적용

### 6.3 리소스 파이프라인
1. `Resources/Themes/[BrandName]/` 폴더 생성
2. ThemeConfig SO 작성
3. ThemeManager.LoadTheme(brandName) 호출
4. 전체 UI/블록/튜브 자동 적용

---

## 7. 기술 스택

- **엔진**: Unity 6000.3.10f1
- **렌더링**: URP (Universal Render Pipeline)
- **UI**: Unity UI (UGUI) + DOTween 애니메이션
- **광고 SDK**: Unity Ads (+ AdMob 모듈 구조)
- **IAP**: Unity IAP
- **저장**: PlayerPrefs (경량) + JSON (레벨데이터)
- **패턴**: 싱글톤 매니저 + ScriptableObject 데이터
- **씬 구조**: Bootstrap → MainMenu → Game → Result

---

## 8. 수익화 KPI 목표 (Playable Ad 기준)

| 지표 | 목표값 |
|------|--------|
| CTR (Install Rate) | > 15% |
| Day 1 Retention | > 40% |
| Day 7 Retention | > 20% |
| ARPDAU | > $0.05 |
| Session Length | > 5분 |
| Ad Impression/DAU | > 3회 |

---

## 9. 개발 우선순위 (MVP)

1. Block Puzzle 코어 메카닉 (그리드 + 슬라이드 + 클리어)
2. Water Sort 코어 메카닉 (튜브 + 붓기 + 정렬)
3. 레벨 데이터 ScriptableObject 시스템
4. 기본 UI (HUD + 결과화면)
5. 광고 연동 구조 (실제 SDK 없이 Mock)
6. 브랜드 테마 시스템
7. IAP 구조 (Mock)

---

*이 문서는 `history/` 폴더에 저장되며 각 Phase 완료 시 업데이트됩니다.*
