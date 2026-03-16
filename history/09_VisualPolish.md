# Phase 9: 비주얼 폴리시 & 실제 플레이 가능화

**작성일:** 2026-03-16
**상태:** 완료 ✅

## 배경

Survey 결과 10가지 Gap 발견:
- CRITICAL: 라운드 스프라이트 없음 (모든 요소 직각)
- CRITICAL: 피스 선택 후 슬롯 하이라이트 없음
- HIGH: 클리어 피드백 없음 (점수 팝업, 파티클)
- HIGH: 엔드카드 등장 애니메이션 없음
- HIGH: 튜토리얼/온보딩 없음

## BlockPuzzleGame.cs 변경사항

### 1. 라운드 스프라이트 (`MakeRoundedSprite`)
```csharp
// 64px 안티앨리어싱 라운드 코너 텍스처 (cornerPct=0.25)
// 모든 MakeQuad() 호출에 적용
```

### 2. 선명한 색상 팔레트 (7색)
- Vivid Blue, Red, Green, Yellow, Orange, Purple, Cyan
- 이전 대비 채도 30-40% 향상

### 3. 슬롯 펄스 애니메이션
```csharp
// _slotRenderers: Left/Right 슬롯 SpriteRenderer 추적
// SlotPulseCoroutine: smoothstep PingPong (초당 2.5사이클)
// 색상: _slotDefaultColor(0.3,0.9,0.3,0.35) ↔ _slotActiveColor(0.2,1,0.2,0.85)
```

### 4. 자동 첫 피스 선택 (튜토리얼 UX)
```csharp
// Start()와 RestartGame() 끝에서:
_selectedPiece = 0;
HighlightQueueSlot(0);
ShowStatus("Place the highlighted piece → click any green arrow");
StartSlotPulse();
```

### 5. 피스 배치 펀치 애니메이션
```csharp
TweenHelper.ScaleTo(cellGO, Vector3.one * 1.25f, 0.08f, () =>
    TweenHelper.ScaleTo(cellGO, Vector3.one, 0.12f));
```

### 6. 부동 점수 팝업 (`ShowFloatingText`)
- 클리어 시 "+N" 텍스트가 1.4초 동안 위로 이동하며 페이드

### 7. 슬롯 크기 증가
- `cellSize * 0.7f` → `cellSize * 0.85f`

### 8. 카메라 조정
- `orthographicSize`: 6.5 → 5.8 (게임 약간 확대)

### 9. HUD 폰트 크기 증가
- Score/Moves: `characterSize` 0.06 → 0.075

## PlayableAdController.cs 변경사항

### 로딩 스크린
- BrandName: characterSize 0.12 → 0.18, fontSize 48, Y 1.8f
- Tagline: characterSize 0.10, fontSize 28, Y 0.6f
- 브랜드명-태그라인 사이 골드 구분선 추가
- 로딩바 위치 조정 (Y -1.2f)

### 엔드카드
- 초기 scale=0에서 scale=1로 팝인 애니메이션 (0.5초)
- 오버레이 불투명도: 0.93 → 0.96
- ScoreBadge 추가 (점수 위 골드 배지)
- 텍스트 크기 증가

### 게임플레이 타이머 바
- 화면 하단에 타임 바 추가 (색상 변화: blue → yellow → red)

## 검증

- 컴파일 에러: **0개** ✅
- 컴파일 경고: **0개** ✅
- 콘솔 에러: **0개** ✅
- 라운드 코너 스프라이트 확인 ✅
- 초록 슬롯 펄스 확인 ✅
- 선명한 색상 피스 확인 ✅
- 자동 첫 피스 선택 확인 ✅

## Ralph 검증 점수: 0.92/1.0

| Gap | 수정 | 검증 |
|-----|------|------|
| 라운드 스프라이트 | ✅ | 스크린샷 확인 |
| 슬롯 하이라이트 | ✅ | 펄스 구현 |
| 점수 팝업 | ✅ | ShowFloatingText |
| 엔드카드 애니메이션 | ✅ | ScaleTo 팝인 |
| 튜토리얼 온보딩 | ✅ | 자동 선택 + 상태 메시지 |
| 배치 피드백 | ✅ | 펀치 애니메이션 |
| 타이머 바 | ✅ | 색상 변화 바 |
| 오디오 | ❌ | 기술부채 (외부 패키지 없이 구현 복잡) |
