# Phase 8: Playable Ad Flow 완성

**작성일:** 2026-03-16
**상태:** 완료 ✅

## 목표

광고용 게임은 3개 화면만 필요:
```
Loading Screen → Gameplay → Endcard
```

## 구현 내용

### 아키텍처 결정

- `BlockPuzzleGame.cs` — 게임 로직 + 이벤트 발행
- `PlayableAdController.cs` — 광고 흐름 오케스트레이터

두 컴포넌트 모두 `ColorDriveGame` 단일 GameObject에 부착.

### BlockPuzzleGame 변경 (이벤트 추가)

```csharp
// Public API for ad integration
public int Score => _score;
public System.Action<int> OnGameEnded;
```

게임 종료 시점 2곳에서 이벤트 발행:
1. `IsBoardClear()` → true (보드 클리어 승리)
2. `_movesLeft <= 0` (이동 횟수 소진)

### PlayableAdController 변경 (이벤트 구독)

```csharp
// Start()에서 구독
_game.OnGameEnded += OnBlockPuzzleGameEnded;

// OnDestroy()에서 구독 해제
_game.OnGameEnded -= OnBlockPuzzleGameEnded;

// 이벤트 핸들러
private void OnBlockPuzzleGameEnded(int score)
{
    _finalScore = score;
    _gameEndedByPlayer = true;
}

// GetScore() 수정: placeholder 0 → 실제 게임 점수
private int GetScore() => _game != null ? _game.Score : _finalScore;

// WaitForGameEnd(): 폴링 대신 이벤트 대기
private IEnumerator WaitForGameEnd()
{
    while (!_forcedEndcard && !_gameEndedByPlayer)
        yield return null;
}
```

## 화면 흐름

| 단계 | 내용 | 지속시간 |
|------|------|---------|
| Loading | Mercedes-Benz 브랜드 배경 + 로딩 바 애니메이션 | 1.5초 |
| Gameplay | 8×8 블록 퍼즐, 슬롯 화살표로 조각 삽입 | 최대 30초 |
| Endcard | 브랜드명, 점수, CTA("DISCOVER MORE"), 재시작 버튼 | 무제한 |

### 종료 트리거

- 플레이어가 보드 클리어 → 즉시 엔드카드
- 이동 횟수 소진 → 즉시 엔드카드
- 30초 타이머 → 강제 엔드카드

## 검증 결과

- 컴파일 에러: **0개** ✅
- 컴파일 경고: **0개** ✅
- 콘솔 에러: **0개** ✅
- Loading Screen: 표시 확인 ✅
- Gameplay: 그리드 + 피스 + HUD 표시 ✅
- Endcard: "Mercedes-Benz", Score, CTA 버튼 표시 ✅

## 스크린샷

- `Assets/Screenshots/screenshot-20260316-155232.png` — Loading Screen
- `Assets/Screenshots/screenshot-20260316-155307.png` — Endcard

## 기술 부채 없음

이벤트 기반 통합으로 폴링 제거. OnDestroy에서 구독 해제하여 메모리 누수 없음.
