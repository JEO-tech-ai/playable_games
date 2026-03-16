# Phase 6: 레벨 데이터 & 콘텐츠 파이프라인

**작성일:** 2026-03-16
**상태:** 완료

## 생성된 ScriptableObject 에셋

### LevelData 에셋
| 파일명 | 레벨 | 모드 | 그리드 | 제한 |
|--------|------|------|--------|------|
| Level_001_Block.asset | 0 | BlockPuzzle | 6×6 | 무브20, 120초 |
| Level_002_Block.asset | 1 | BlockPuzzle | 8×8 | 무브30, 180초 |
| Level_011_Water.asset | 10 | WaterSort | 튜브4, 깊이3 | 무브20 |
| Level_041_Hybrid.asset | 40 | Hybrid | 6×6 + 튜브6 | 복합 |

### ThemeConfig 에셋
| 파일명 | 브랜드 | CTA |
|--------|--------|-----|
| ThemeConfig_Mercedes.asset | Mercedes-Benz | Discover Mercedes |
| ThemeConfig_Generic.asset | Generic | Play Now |

## 레벨 데이터 파이프라인

```
LevelData SO (ScriptableObjects/Levels/)
    ↓ LevelManager.allLevels[]
    ↓ LevelManager.LoadLevel(index)
    ↓ GameController.SetupGame()
    ↓ BlockPuzzleController.StartLevel(levelData)
       또는 WaterSortController.StartLevel(levelData)
```

## 신규 레벨 추가 방법

1. Project 창 우클릭 → Create → ColorDrive → Level Data
2. 파라미터 설정 (그리드 크기, 색상, 제한 등)
3. LevelManager GameObject의 allLevels[] 배열에 추가
4. 순서 = 레벨 인덱스 (0-based)

## 브랜드 테마 교체 방법

1. Project 창 우클릭 → Create → ColorDrive → Theme Config
2. 브랜드 에셋 설정 (로고, 색상, 오디오, CTA)
3. ThemeManager.ApplyTheme(themeConfig) 호출
   또는 ThemeManager.LoadThemeByName("BrandName") (Resources 기반)

## 양산화 체크리스트

- [x] LevelData ScriptableObject — 파라미터 기반 레벨 설정
- [x] ThemeConfig ScriptableObject — 브랜드 일괄 교체
- [x] ThemeManager.LoadThemeByName() — 런타임 테마 스왑
- [x] AdManager Mock — 실제 SDK로 교체 가능
- [x] IAPManager Mock — Unity IAP로 교체 가능
- [ ] LevelEditor 툴 (향후: EditorWindow로 비주얼 레벨 편집)
- [ ] 레벨 JSON Export/Import (향후: 서버 기반 레벨 배포)
