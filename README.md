# Color Drive: Hybrid Puzzle Game

> A Mercedes-Benz branded playable advertisement combining Block Slide Puzzle and Water Sort mechanics into one mass-production framework.

![Unity Version](https://img.shields.io/badge/Unity-6000.3.10f1-blue.svg)
![Platform](https://img.shields.io/badge/Platform-WebGL%20%7C%20Mobile-green.svg)
![Puzzle Types](https://img.shields.io/badge/Puzzles-Block%20Slide%20%7C%20Water%20Sort-orange.svg)

---

## Table of Contents

- [Project Overview](#project-overview)
- [Game Flow](#game-flow)
- [Screenshots](#screenshots)
- [How to Play](#how-to-play)
  - [Block Puzzle Mode](#block-puzzle-mode)
  - [Water Sort Mode](#water-sort-mode)
- [Game Mechanics](#game-mechanics)
  - [Block Puzzle Rules](#block-puzzle-rules)
  - [Scoring System](#scoring-system)
  - [Win & Lose Conditions](#win--lose-conditions)
  - [Controls](#controls)
- [Technical Architecture](#technical-architecture)
  - [Project Structure](#project-structure)
  - [Key Components](#key-components)
  - [Dependencies](#dependencies)
- [Brand Customization](#brand-customization)
  - [Quick Start (5 Minutes)](#quick-start-5-minutes)
  - [Theme Configuration Fields](#theme-configuration-fields)
  - [Creating a New Brand](#creating-a-new-brand)
- [Level Design Guide](#level-design-guide)
  - [Level Data Properties](#level-data-properties)
  - [Creating Block Puzzle Levels](#creating-block-puzzle-levels)
  - [Creating Water Sort Levels](#creating-water-sort-levels)
- [Ad Integration](#ad-integration)
  - [Playable Ad Flow](#playable-ad-flow)
  - [Rewarded Ads](#rewarded-ads)
  - [Endcard Customization](#endcard-customization)
- [Development Notes](#development-notes)
  - [Architecture Patterns](#architecture-patterns)
  - [Color Management](#color-management)
  - [No External Dependencies](#no-external-dependencies)

---

## Project Overview

**Color Drive: Hybrid Puzzle** is a production-ready playable advertisement framework built with Unity 6000.3.10f1. It demonstrates how two distinct puzzle mechanics (block placement and liquid sorting) can coexist in a single game, controlled through a unified ScriptableObject-based configuration system.

### Key Features

- **Two Puzzle Modes** — Block Slide (8×8 grid) and Water Sort (6 tubes)
- **Brand-Agnostic Architecture** — Swap a single ScriptableObject to change all branding, colors, and CTAs
- **Lightweight Ad Flow** — Loading screen → Gameplay (30s max) → Endcard with CTA
- **Procedural & Preset Levels** — Support both random generation and hand-crafted layouts
- **Zero External Dependencies** — Custom lightweight tweening, no LeanTween or DOTween required
- **Responsive Scoring** — Combo multipliers and real-time score feedback
- **Mobile & WebGL Ready** — Supports both touch and mouse input

---

## Game Flow

```
┌─────────────────────────────────────────────────────┐
│  LOADING SCREEN (1.5 seconds)                        │
│  • Brand logo & tagline                              │
│  • Animated loading bar                              │
├─────────────────────────────────────────────────────┤
│  GAMEPLAY (30 seconds max)                           │
│  • Active puzzle board                               │
│  • Real-time score & moves/timer display             │
│  • Next piece preview queue                          │
├─────────────────────────────────────────────────────┤
│  ENDCARD                                             │
│  • Final score display                               │
│  • Brand CTA button ("Learn More", "Shop", etc.)     │
│  • Play Again option                                 │
│  • Installation/tracking pixel fired                 │
└─────────────────────────────────────────────────────┘
```

---

## Screenshots

Place screenshots at: `Assets/Screenshots/`

Example screenshot locations (auto-generated):
- `Assets/Screenshots/screenshot-gameplay-block.png`
- `Assets/Screenshots/screenshot-gameplay-water.png`
- `Assets/Screenshots/screenshot-endcard.png`
- `Assets/Screenshots/screenshot-loading.png`

Screenshot examples are captured via the Unity Editor. Replace with branded versions as needed.

---

## How to Play

### Block Puzzle Mode

> **Goal:** Clear as many colored blocks as possible in 30 moves.

#### Step-by-Step

1. **Look at the Queue** — Three colored pieces are shown at the bottom (SELECT PIECE area)
2. **First Piece Auto-Selects** — The first piece is highlighted in **gold** automatically
3. **Click an Arrow** — Click any **green arrow** (left/right sides) or **orange arrow** (top/bottom) to place the piece
   - **Green arrows (← →)** — Place from left or right side; piece slides into first open cell
   - **Orange arrows (↑ ↓)** — Place from top or bottom; piece slides into first open cell
4. **Piece Slides In** — The piece moves into the grid and occupies the first available row/column
5. **Automatic Clearing** — If a complete row/column is one color, it clears instantly
6. **Next Piece Auto-Selects** — After placing, the next piece in queue is automatically highlighted
7. **Repeat** — Place pieces until moves run out or board clears
8. **Score Display** — Floating "+N" popups show points earned for each clear

#### Keyboard Shortcuts

| Key  | Action      |
|------|-------------|
| `R`  | Restart     |

---

### Water Sort Mode

> **Goal:** Sort colored water into tubes so each tube contains only one color.

#### Step-by-Step

1. **Select a Tube** — Click a tube containing water you want to pour
2. **Pick a Target** — Click another tube to pour the first tube's top layer into it
3. **Pouring Rules:**
   - You can only pour the topmost same-colored water
   - A tube can accept water only if its top color matches (or tube is empty)
   - A tube can hold at most one extra layer per pour
4. **Win Condition** — All tubes contain only one color each (or are empty)
5. **Move Limit** — You have a limited number of pours before game over
6. **Undo** — Use the undo button to reverse mistakes (limited undo allowed)

---

## Game Mechanics

### Block Puzzle Rules

#### Placement

- **Grid Size:** 8×8 cells (64 total cells)
- **Piece Shapes:** 7 distinct shapes available (1x1 to L-shapes)
- **Queue Size:** 3 pieces shown at once
- **Direction-Based Placement:**
  - **Left arrow (→):** Piece enters from left, slides to first open cell in that row
  - **Right arrow (←):** Piece enters from right, slides to first open cell in that row
  - **Top arrow (↓):** Piece enters from top, slides to first open cell in that column
  - **Bottom arrow (↑):** Piece enters from bottom, slides to first open cell in that column

#### Clearing Rules

Blocks clear when:

1. **Full Row Match** — An entire row (all 8 cells) is the same color → **entire row clears**
2. **Full Column Match** — An entire column (all 8 cells) is the same color → **entire column clears**
3. **3+ Consecutive Horizontal** — Three or more same-colored blocks in a row → **all consecutive blocks clear**
4. **3+ Consecutive Vertical** — Three or more same-colored blocks in a column → **all consecutive blocks clear**

**Note:** Clearing checks happen immediately after each piece placement. Multiple patterns can clear in a single move.

### Scoring System

```
Points Per Clear = Cleared Blocks × 10 × (1 + Cleared Blocks / 5)
```

**Examples:**

| Blocks Cleared | Multiplier | Points   |
|----------------|------------|----------|
| 1              | 1.2        | 12       |
| 4              | 1.8        | 72       |
| 8              | 2.6        | 208      |
| 16             | 4.2        | 672      |

**Combo System:** Clearing more blocks in one move yields exponential rewards. Strategic play = higher scores.

### Win & Lose Conditions

| Condition             | Result                                           |
|-----------------------|--------------------------------------------------|
| **Board Fully Clear** | **WIN** → Show endcard with final score          |
| **Moves Reach 0**     | **LOSE** → Show endcard with final score         |
| **30s Time Limit**    | **FORCED ENDCARD** → Show endcard with final score |

> Note: Block Puzzle uses "moves" as the primary limiter (30 default). Water Sort uses "pour moves" (50 default).

### Controls

| Input              | Action                      | Platform |
|--------------------|-----------------------------|----|
| **Click piece**    | Select from queue           | Mouse / Touch |
| **Click arrow**    | Place selected piece        | Mouse / Touch |
| **[R] key**        | Restart current level       | Keyboard |
| **Hover arrow**    | Arrow pulses (visual cue)   | Mouse |

---

## Technical Architecture

### Project Structure

```
Assets/
├── Scripts/
│   ├── BlockPuzzle/
│   │   ├── BlockPuzzleGame.cs      [Core puzzle logic: grid, pieces, scoring]
│   │   ├── BlockGrid.cs             [Grid data & queries]
│   │   ├── BlockPiece.cs            [Piece shape & color management]
│   │   ├── BlockPuzzleController.cs [High-level puzzle controller]
│   │   ├── SlotButton.cs            [Input handler for directional arrows]
│   │   └── PieceQueueItem.cs        [Visual queue piece display]
│   │
│   ├── WaterSort/
│   │   ├── WaterSortController.cs   [Main Water Sort controller]
│   │   ├── Tube.cs                  [Single tube: liquid state & pouring]
│   │   └── [Water layer visuals]
│   │
│   ├── Managers/
│   │   ├── PlayableAdController.cs  [Ad flow: Loading → Gameplay → Endcard]
│   │   ├── GameController.cs        [Master game state & scene flow]
│   │   ├── LevelManager.cs          [Level selection & progression]
│   │   ├── ThemeManager.cs          [Brand config & theming]
│   │   ├── TweenHelper.cs           [Lightweight coroutine tweens]
│   │   ├── GameBootstrap.cs         [Scene initialization]
│   │   ├── AdManager.cs             [Ad network integration stubs]
│   │   ├── IAPManager.cs            [In-app purchase stubs]
│   │   └── GameSceneSetup.cs        [Runtime scene construction]
│   │
│   ├── UI/
│   │   ├── HUDController.cs         [Score, moves, timer display]
│   │   ├── ResultScreenUI.cs        [Level completion screen]
│   │   ├── LevelSelectUI.cs         [Level picker interface]
│   │   └── PauseMenuUI.cs           [Pause & resume controls]
│   │
│   └── Data/
│       ├── ThemeConfig.cs           [Brand identity ScriptableObject]
│       └── LevelData.cs             [Level configuration ScriptableObject]
│
├── ScriptableObjects/
│   ├── Levels/
│   │   ├── Level_001_Block.asset    [Example Block Puzzle level]
│   │   ├── Level_002_Block.asset    [Example Block Puzzle level]
│   │   ├── Level_011_Water.asset    [Example Water Sort level]
│   │   └── Level_041_Hybrid.asset   [Hybrid mode (feature in progress)]
│   │
│   └── Themes/
│       ├── ThemeConfig_Generic.asset   [Base generic theme]
│       └── ThemeConfig_Mercedes.asset  [Mercedes-Benz brand theme]
│
├── Prefabs/
│   ├── BlockGrid.prefab
│   ├── WaterSortBoard.prefab
│   └── [UI prefabs]
│
├── Scenes/
│   └── SampleScene.unity            [Main playable ad scene]
│
├── Materials/
│   ├── BlockDefault.mat
│   ├── TubeGlass.mat
│   └── [Brand-specific materials]
│
└── Resources/
    └── [Runtime-loaded config & sprites]
```

### Key Components

#### BlockPuzzleGame.cs

The self-contained Block Puzzle implementation. Handles:

- **Grid Management** — 8×8 cells, color state, empty slots
- **Piece Queue** — 3 pieces shown, random shape/color generation
- **Placement Logic** — Direction-based sliding placement with collision detection
- **Clearing System** — Pattern detection (rows, columns, 3+ runs)
- **Scoring** — Combo multipliers on clear
- **UI Visuals** — Built-in SpriteRenderer grid, animated slot arrows, queue display
- **Input** — Slot triggers and queue triggers via OnMouseDown

**Key Public API:**

```csharp
public int Score => _score;
public event System.Action<int> OnGameEnded;  // Fire when moves=0 or board clears

public void OnSlotClicked(Dir dir, int slotIdx);    // Dir: Left/Right/Up/Down
public void OnQueueSlotClicked(int idx);            // idx: 0-2 (which piece to select)
```

#### PlayableAdController.cs

Orchestrates the full ad flow:

- **Loading Screen** — Brand logo + animated progress bar (1.5s)
- **Gameplay Phase** — Runs BlockPuzzleGame for up to 30 seconds
- **Timer Bar** — Blue → Yellow → Red color shift; real-time countdown
- **Endcard Phase** — Score display + CTA button + Play Again
- **Score Tracking** — Listens to OnGameEnded event
- **Forced Endcard** — Triggers if time limit exceeded (even if game incomplete)

**Flow Example:**

```csharp
// In PlayableAdController
public enum AdPhase { Loading, Gameplay, Endcard }
public AdPhase CurrentPhase { get; private set; }

// Subscribe to game end
_game.OnGameEnded += (score) => { /* capture score */ };

// CTA button opens branded URL
public void OnCTAPressed()
{
    Application.OpenURL(ThemeConfig.ctaUrl);  // e.g., https://www.mercedes-benz.com
}
```

#### ThemeConfig.cs (ScriptableObject)

Brand identity container. Update these 5 fields to rebrand:

```csharp
public string brandName = "Mercedes-Benz";
public Sprite logoSprite;
public Color primaryColor = new Color(0.15f, 0.15f, 0.15f);   // Dark
public Color accentColor = new Color(0.8f, 0.75f, 0.55f);     // Gold
public string tagline = "Drive in Color.";
public string ctaUrl = "https://www.mercedes-benz.com";
public string ctaButtonText = "DISCOVER MORE";

// Plus audio clips, materials, and block color palette
```

#### LevelData.cs (ScriptableObject)

Level configuration. Create one per level:

```csharp
public string levelName = "Level 1";
public PuzzleMode puzzleMode = PuzzleMode.BlockPuzzle;

// Block Puzzle settings
public int gridWidth = 8, gridHeight = 8;
public int moveLimit = 30;

// Water Sort settings
public int tubeCount = 6;
public int layerDepth = 4;

// Optional preset layouts (JSON or CSV)
public string presetLayoutJson = "";  // For procedural block placement
public string tubeLayoutCsv = "";     // For preset tube configurations
```

#### TweenHelper.cs

Lightweight alternative to DOTween/LeanTween. Provides:

```csharp
TweenHelper.ScaleTo(gameObject, Vector3.one * 1.25f, 0.08f, () => {
    TweenHelper.ScaleTo(gameObject, Vector3.one, 0.12f);
});
```

No external dependencies; all tweens run as coroutines on a persistent MonoBehaviour.

### Dependencies

- **Unity 6000.3.10f1** (core engine)
- **TextMesh** (built-in UI rendering)
- **C# 9.0+** (newer language features supported)
- **No external plugins** — Everything custom-built for performance

---

## Brand Customization

### Quick Start (5 Minutes)

1. **Create a New ThemeConfig**
   - Right-click in Project → Create → ColorDrive → Theme Config
   - Name it `ThemeConfig_YourBrand.asset`

2. **Fill in 5 Key Fields**
   - Brand Name (e.g., "Nike")
   - Primary Color (e.g., black)
   - Accent Color (e.g., neon green)
   - Tagline (e.g., "Just Do It.")
   - CTA URL (e.g., "https://www.nike.com")

3. **Assign to PlayableAdController**
   - Select the PlayableAdController in the scene
   - Drag your new ThemeConfig into the Inspector
   - Play & test

4. **Done** — All menus, buttons, and branding update automatically

### Theme Configuration Fields

| Field                | Purpose                                    | Example                          |
|----------------------|--------------------------------------------|----------------------------------|
| `brandName`          | Displayed on loading screen & endcard      | "Mercedes-Benz"                  |
| `logoSprite`         | Optional brand mark                        | mercedes_logo.png                |
| `primaryColor`       | Main background & dark UI elements         | RGB(0.15, 0.15, 0.15)           |
| `secondaryColor`     | Secondary UI elements                      | RGB(0.8, 0.8, 0.8)              |
| `accentColor`        | Buttons, highlights, progress bars         | RGB(0.8, 0.75, 0.55) [gold]    |
| `uiTextColor`        | Primary text color                         | Color.white                      |
| `uiPanelColor`       | Semi-transparent UI backgrounds            | RGBA(0.1, 0.1, 0.1, 0.9)        |
| `blockColors[]`      | Override Block Puzzle colors (7 colors)    | [Red, Orange, Yellow, Green...] |
| `bgmClip`            | Background music                           | your_bgm.wav                     |
| `blockPlaceSound`    | Sound played on piece placement             | place.wav                        |
| `blockClearSound`    | Sound played on block clear                 | clear.wav                        |
| `adTagline`          | Tagline text (loading + endcard)           | "Drive in Color."                |
| `ctaButtonText`      | CTA button label                           | "DISCOVER MORE"                  |
| `ctaUrl`             | URL opened when CTA clicked                 | "https://www.mercedes-benz.com"  |

### Creating a New Brand

#### Steps

1. **Duplicate Existing Theme** (faster)
   - Right-click `ThemeConfig_Mercedes.asset` → Duplicate
   - Rename to `ThemeConfig_Nike.asset`
   - Edit the copied asset's fields

2. **Or Create from Scratch**
   - Assets → Create → ColorDrive → Theme Config
   - Fill in all fields matching your brand

3. **Add to Scene**
   - Open `Scenes/SampleScene.unity`
   - Select GameObject: `PlayableAdController`
   - In Inspector, assign your theme to the ThemeConfig reference
   - Set brand colors and URL

4. **Test in Play Mode**
   - Click Play in Editor
   - Verify loading screen shows your brand
   - Check endcard displays correctly
   - Click CTA to verify URL opens

#### Example: Nike Brand Customization

```csharp
// Nike branding (ThemeConfig_Nike.asset)
brandName = "Nike"
primaryColor = Color.black                          // Deep black
secondaryColor = new Color(1, 1, 1)                 // White
accentColor = new Color(1f, 0.84f, 0f)             // Neon yellow
uiTextColor = Color.white
blockColors = [
    new Color(1f, 0f, 0f),     // Red
    new Color(0f, 0f, 1f),     // Blue
    new Color(0f, 0f, 0f),     // Black
    new Color(1f, 1f, 1f),     // White
    ...
]
adTagline = "Just Do It."
ctaButtonText = "SHOP NOW"
ctaUrl = "https://www.nike.com"
```

---

## Level Design Guide

### Level Data Properties

Create a new LevelData ScriptableObject for each level:

```csharp
[CreateAssetMenu(fileName = "LevelData", menuName = "ColorDrive/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName = "Level 1";
    public int levelIndex = 0;
    public PuzzleMode puzzleMode = PuzzleMode.BlockPuzzle;  // or WaterSort

    // Block Puzzle
    public int gridWidth = 8, gridHeight = 8;
    public int moveLimit = 30;

    // Water Sort
    public int tubeCount = 6;
    public int layerDepth = 4;
    public int waterSortMoveLimit = 50;

    // Colors
    public Color[] colorPalette = new Color[] { ... };

    // Optional preset layouts
    public string presetLayoutJson = "";   // For block grid
    public string tubeLayoutCsv = "";      // For water tubes

    // Rewards
    public int baseScoreReward = 100;
    public int starThreshold2 = 75;   // % for 2-star
    public int starThreshold3 = 90;   // % for 3-star
}
```

### Creating Block Puzzle Levels

#### Procedural (Easiest)

1. Create a new LevelData asset: `Level_003_Block.asset`
2. Set:
   - `levelName` = "Level 3"
   - `puzzleMode` = BlockPuzzle
   - `gridWidth` = 8, `gridHeight` = 8
   - `moveLimit` = 30 (adjust difficulty)
   - `colorPalette` = [Red, Orange, Yellow, Green, Blue, Purple, Cyan]
3. Leave `presetLayoutJson` empty — board generates randomly at start
4. Save & reference in level list

#### Hand-Crafted (Advanced)

For preset block layouts, populate `presetLayoutJson` with:

```json
{
  "cells": [
    {"x": 0, "y": 0, "color": 0},
    {"x": 1, "y": 0, "color": 0},
    {"x": 2, "y": 0, "color": 1},
    ...
  ]
}
```

> **Note:** Preset layout support is framework-ready but not yet fully implemented in BlockPuzzleGame. Currently all boards are procedurally generated.

### Creating Water Sort Levels

#### Define Tube Layouts

In LevelData's `tubeLayoutCsv`, format as rows (each row = one tube):

```csv
0,0,0,0
1,1,2,3
2,3,3
0,1
```

Reading from bottom to top:
- Tube 0: [Color 0, Color 0, Color 0, Color 0]
- Tube 1: [Color 1, Color 1, Color 2, Color 3]
- Tube 2: [Color 2, Color 3, Color 3]
- Tube 3: [Color 0, Color 1]

#### Steps

1. Create `Level_012_Water.asset`
2. Set:
   - `puzzleMode` = WaterSort
   - `tubeCount` = 6
   - `layerDepth` = 4 (max layers per tube)
   - `waterSortMoveLimit` = 50 (adjust difficulty)
3. Paste tube layout into `tubeLayoutCsv`
4. Set `colorPalette` with 6 distinct colors
5. Save & reference in level select

**Difficulty Tuning:**

- **Easy:** All tubes pre-sorted except 1-2 tubes mixed
- **Medium:** 3-4 tubes mixed; obvious solution path
- **Hard:** All tubes mixed; requires forward planning

---

## Ad Integration

### Playable Ad Flow

The ad runs three phases:

1. **Loading (1.5 seconds)**
   - Brand logo visible
   - Animated progress bar (0% → 100%)
   - Tagline shown

2. **Gameplay (up to 30 seconds)**
   - Timer bar visible (blue → yellow → red)
   - Active puzzle board with pieces & slots
   - Score & move counter displayed
   - Exits when: player wins, moves reach 0, or 30s elapsed

3. **Endcard**
   - Final score prominently displayed
   - Brand name & tagline
   - CTA button (opens URL)
   - "Play Again" button (restarts scene)
   - Tracking pixel fires (ad network integration point)

### Rewarded Ads

**Current Status:** Stubs ready in `AdManager.cs`

To integrate rewarded ads:

1. In `AdManager.cs`, implement:
   ```csharp
   public void ShowRewardedAd(System.Action onRewardEarned)
   {
       // Call your ad network SDK
       // On reward earned, invoke onRewardEarned()
   }
   ```

2. In `GameController`, hook up rewards:
   ```csharp
   _adManager.ShowRewardedAd(() => {
       // Grant reward: bonus moves, extra level unlock, etc.
   });
   ```

3. Supported ad networks (stubs):
   - Google AdMob (Android/iOS)
   - Facebook Audience Network
   - IronSource
   - AppLovin

### Endcard Customization

**To change endcard appearance:**

1. Edit `PlayableAdController.BuildEndcardScreen()` method
2. Modify:
   - `brandAccent` color for button background
   - `ctaButtonText` for button label
   - Score badge position/styling
   - Tagline text size

**To add analytics/tracking:**

```csharp
// In PlayableAdController.ShowEndcard()
public void ShowEndcard(int score)
{
    // ... existing code ...

    // Fire tracking pixel
    FireAnalyticsEvent("playable_ad_complete", new Dictionary<string, object> {
        {"score", score},
        {"brand", ThemeConfig.brandName},
        {"timestamp", System.DateTime.Now}
    });
}
```

---

## Development Notes

### Architecture Patterns

#### Event-Driven Communication

Puzzles don't directly reference the ad controller. Instead, they fire events:

```csharp
// BlockPuzzleGame.cs
public event System.Action<int> OnGameEnded;

// In game logic
if (IsBoardClear() || _movesLeft <= 0)
{
    OnGameEnded?.Invoke(_score);  // Notify listeners
}

// PlayableAdController.cs
_game.OnGameEnded += (score) => {
    ShowEndcard(score);
};
```

This pattern allows:
- Puzzle logic to remain independent
- Easy swapping of different puzzle modes
- Clean separation between game & ad flow

#### ScriptableObject-Based Configuration

All brand & level data lives in ScriptableObjects, not hardcoded:

```csharp
// Bad (hardcoded)
if (brandName == "Mercedes") { /* special logic */ }

// Good (config-driven)
var tagline = ThemeConfig.tagline;  // Works for any brand
```

Benefits:
- Zero code changes to swap brands
- Non-programmers can adjust levels & visuals
- Version control friendly (binary assets vs. code)

#### No Editor Dependencies

All visuals are built at runtime:

```csharp
// BlockPuzzleGame builds its own UI
BuildGridVisuals();    // Creates cell GameObjects
BuildSlots();          // Creates arrow buttons
BuildHUD();            // Creates score/moves text

// Result: scene can be empty at start
// Entire game constructs itself at runtime
```

### Color Management

Three color systems:

1. **Block Colors** — The 7 distinct puzzle piece colors
2. **Theme Colors** — Brand primary, secondary, accent (UI only)
3. **UI Colors** — Hardcoded for contrast (text, panel backgrounds)

**To customize block colors:**

Edit the `COLORS` array in BlockPuzzleGame.cs:

```csharp
private Color[] COLORS = new Color[]
{
    new Color(0.91f, 0.00f, 0.18f),  // Red
    new Color(1.00f, 0.47f, 0.00f),  // Orange
    // ... etc
};
```

Or override via ThemeConfig's `blockColors[]` array (if implemented in LevelManager).

### No External Dependencies

This project includes **zero** external plugins:

- ✓ No LeanTween
- ✓ No DOTween
- ✓ No Addressables (manual loading used)
- ✓ No TextMesh Pro (built-in TextMesh used)

**Why?**

1. **Playable ads have strict file size limits** — Every KB counts
2. **Fast load times** — Minimal dependencies = faster compile
3. **Simple debugging** — All code is visible and modifiable
4. **Production-ready** — Proven in live campaigns

**Trade-offs:**

- Tweening is basic (no complex curves or sequences)
- UI rendering uses TextMesh (some platforms may have rendering quirks)
- No advanced audio management

For serious production work, consider adding DOTween if file size permits.

---

## Troubleshooting

### Block Grid Not Appearing

- Check `BlockPuzzleGame` is attached to a GameObject in the scene
- Verify Main Camera exists and is orthographic (`cam.orthographic = true`)
- Check camera Z position: should be `-10`

### Theme Colors Not Applying

- Ensure `ThemeConfig` reference is assigned in PlayableAdController Inspector
- Check `brandName` field is not empty
- Verify color values are in 0-1 range (not 0-255)

### Pieces Not Placeable

- Verify slot arrows are visible (green on sides, orange on top/bottom)
- Check grid has empty space (can't place if board full)
- Try pressing [R] to restart and reset board state

### Endcard Not Showing

- Verify `PlayableAdController` is in the scene
- Check game duration — endcard only shows after 30s or when game ends
- Look for console errors (check Unity Editor Console)

---

## File Paths Reference

| Path | Purpose |
|------|---------|
| `Assets/Scripts/BlockPuzzle/BlockPuzzleGame.cs` | Core puzzle logic |
| `Assets/Scripts/Managers/PlayableAdController.cs` | Ad flow orchestration |
| `Assets/Scripts/Data/ThemeConfig.cs` | Brand configuration template |
| `Assets/Scripts/Data/LevelData.cs` | Level configuration template |
| `Assets/ScriptableObjects/Themes/ThemeConfig_Mercedes.asset` | Mercedes brand config |
| `Assets/ScriptableObjects/Levels/Level_001_Block.asset` | Example block puzzle level |
| `Assets/Scenes/SampleScene.unity` | Main playable ad scene |

---

## License & Attribution

This project is built as a production playable advertisement framework for Mercedes-Benz. All custom code is proprietary.

**Brand Logos:** Use branded sprite assets via ThemeConfig customization.

---

## Quick Reference: 5-Minute Brand Swap

```
1. Create ThemeConfig_NewBrand.asset
   └─ Set brandName, colors, tagline, ctaUrl

2. Open SampleScene
   └─ Select PlayableAdController

3. In Inspector, assign your new ThemeConfig
   └─ Drag asset into ThemeConfig field

4. Press Play
   └─ All branding updates automatically

5. Click CTA to verify URL works
   └─ Done!
```

---

**Last Updated:** March 16, 2026
**Unity Version:** 6000.3.10f1
**Platform:** WebGL, Mobile (Android/iOS via export)
