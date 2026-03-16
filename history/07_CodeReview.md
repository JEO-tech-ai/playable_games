# Phase 7: 코드 리뷰 결과

**작성일:** 2026-03-16
**상태:** 완료

## 코드 리뷰 요약

| 심각도 | 발견 | 수정 |
|--------|------|------|
| CRITICAL | 2 | 2 ✅ |
| HIGH | 4 | 4 ✅ |
| MEDIUM | 5 | 2 (나머지 3 기술부채) |
| LOW | 3 | 1 |
| **합계** | **14** | **9** |

## 수정된 이슈

### [CRITICAL] WaterSortController.Undo() 로직 버그
- **문제**: `from.PourFrom(to)`를 역방향으로 호출 → Undo가 전혀 동작 안 함
- **수정**: Tube에 `Push(int)` 메서드 추가 후 직접 push 방식으로 교체
- **파일**: `WaterSort/Tube.cs`, `WaterSort/WaterSortController.cs`

### [CRITICAL] GameManager.SpendCoins() stale value 버그
- **문제**: PlayerPrefs를 2번 읽어 TOCTOU race condition 가능
- **수정**: 로컬 변수로 캐싱 + `PlayerPrefs.Save()` 추가
- **파일**: `Managers/GameManager.cs`

### [HIGH] BlockPuzzleController 이벤트 중복 구독
- **문제**: StartLevel 재호출 시 핸들러가 누적됨
- **수정**: unsubscribe-before-subscribe 패턴 적용
- **파일**: `BlockPuzzle/BlockPuzzleController.cs`

### [HIGH] BlockGrid.CheckAndClear 중복 셀 카운팅
- **문제**: List가 중복을 포함해 점수 과다 계산
- **수정**: HashSet으로 교체하여 자동 중복 제거
- **파일**: `BlockPuzzle/BlockGrid.cs`

### [HIGH] TweenHelper ColorTo가 Time.deltaTime 사용
- **문제**: 타임스케일 0 시 (레벨클리어/게임오버) 색상 애니메이션이 멈춤
- **수정**: `Time.unscaledDeltaTime`으로 교체
- **파일**: `Managers/TweenHelper.cs`

### [HIGH] GameManager 싱글톤 dangling reference
- **문제**: 씬 전환 시 `Instance`가 null이 아닌 destroyed 상태
- **수정**: `OnDestroy()`에서 `Instance = null` 처리
- **파일**: `Managers/GameManager.cs`

### [MEDIUM] Tube.SetSelected 위치 드리프트
- **문제**: 빠른 select/deselect 반복 시 튜브가 위로 밀려올라감
- **수정**: `_restY` 기준 위치 저장 후 절대값 기준 이동
- **파일**: `WaterSort/Tube.cs`

## 기술 부채 (향후 개선)

| 이슈 | 설명 | 우선순위 |
|------|------|---------|
| BlockPiece.AnimateClear 콜백 순서 | 외부 destroy 시 onComplete 호출 가능 | MEDIUM |
| IAPManager 싱글톤 OnDestroy 없음 | dangling reference 가능 | MEDIUM |
| BlockGrid 혼합색 행 제거 미지원 | 전체 행이 같은 색일 때만 제거 (디자인 의도 확인 필요) | MEDIUM |
| PlayerPrefs.Save() 미호출 위치 | AddCoins, UnlockNextLevel 후 Save() 미호출 | LOW |
| TweenHelper ApplicationQuit 가드 없음 | 앱 종료 중 새 인스턴스 생성 가능 | LOW |

## 컴파일 상태

```
✅ 컴파일 에러: 0개
⚠️  컴파일 경고: 0개
```
