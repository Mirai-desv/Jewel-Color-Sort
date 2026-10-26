# 💎 Jewel Color Sort

A color-sorting bead puzzle game built with **Unity 6**, playable on desktop and mobile. Each level is a scrambled pixel-art picture: pick up groups of same-colored beads and drop them onto the matching background cells until the picture is complete. Clearing a level for the first time plays a short dialogue scene and awards a collectible item.

## How to Play

Each level has a **grid** made of background cells (the target picture), **beads** sitting in the wrong places, and a **tray** for parking beads temporarily.

1. **Select beads:** tap a bead and the whole group of same-colored beads connected to it (up, down, left, right) is lifted.
2. **Drop beads:** tap an empty cell whose background color matches the selected beads. They fly to fill the connected region of empty cells of that color (as many as the empty region and the selection allow).
3. **Park in the tray:** tap an empty tray slot to store the selected beads. Tapping a bead in the tray selects every tray bead of the same color (they don't need to be adjacent) so you can drop them back onto the grid.
4. **Locking:** a bead sitting on a cell of its own color is locked and can no longer be moved.
5. **Win** when every background cell holds a bead of the right color. **Lose** when no valid move remains.
6. **Undo** reverts the last move (even after losing), at a score penalty (`but it's just a plan in current time, we're still not put it into the newest version`).

Invalid actions (wrong color, an occupied tray slot, a full tray) put the beads back down and play an error sound.

**Controls:** tap/click to select and drop, drag to pan the camera, pinch or mouse wheel to zoom.

## Features

- 15 levels of varying size and color count (roughly 300 to 2,500 cells, up to 10 colors).
- Tray storage and a full per-level undo history.
- Staggered bead-fly animations, tunable through `GameConfig`.
- Camera that auto-fits the grid, with pan and zoom (pinch on touch, scroll wheel on desktop).
- Scoring based on time, move count, and number of undos.
- Character dialogues (Fasto, Ichira, Boum, ...) and reward items after a first clear.
- Progress, coins, items, and audio settings saved with `PlayerPrefs`.
- Main menu, level select, pause/settings, and win/lose screens.
- Safe-area support for notched phone screens (`SafeAreaFitter`).
- Editor tools for validating level data and building for WebGL.

## Requirements and Setup

- **Unity `6000.4.4f1`** (Unity 6).
- Render pipeline: Universal RP 17.4. Main packages: Input System 1.19, uGUI 2.0, TextMesh Pro, 2D Animation / Sprite / Tilemap.

```bash
git clone https://github.com/Mirai-desv/Jewel-Color-Sort.git
```

1. Open the cloned folder in Unity Hub (using the editor version above).
2. Open the scene `Assets/Scenes/SampleScene.unity`.
3. Press **Play**.

`GameManager` has a `Skip Menu` option that jumps straight into gameplay for testing.

## Project Structure

```
Assets/
├── Data/           # ScriptableObjects: LevelData, DialogueData, RewardData, ColorPalette, GameConfig
├── Scripts/
│   ├── Core/       # Type enum (bead/color kinds), GameState, constants
│   ├── Data/       # ScriptableObject definitions, ScoreCalculator
│   ├── Logic/      # Pure game rules (independent of views)
│   ├── Managers/   # GameManager, LevelManager, PostLevelManager, SaveManager, AudioManager
│   ├── Input/      # GestureDetector (tap/drag/pinch), InputHandler, CameraControll
│   ├── View/       # GridView, TrayView, BeadView, BeadPool, BeadFlyAnimator, BackgroundView
│   ├── UI/         # Panels (Menu, LevelSelect, HUD, Setting, Win, Lose, Dialogue, Reward) and Buttons
│   └── Editor/     # LevelDataEditor, LevelDataGeneratorWindow, WebGLBuilder
├── Prefabs/        # Beads (Jelly), level images, UI screens, tray
├── Music/ Sounds/  # Background music and sound effects
└── Scenes/         # SampleScene
Jewel Sort UML.asta # UML diagram (open with Astah)
```

## Architecture

The code separates **model** (data and rules), **view** (rendering and animation), and **coordination** (session and managers).

- **Logic (model):**
  - `GridModel` stores the background layer, bead layer, lock state, and a running count of correct cells (updated incrementally, never by rescanning the grid).
  - `TrayModel` manages the tray slots.
  - `RegionFinder` uses BFS to find groups of same-colored beads or regions of same-colored empty cells, reusing its buffers to avoid per-tap allocations.
  - `WinLoseChecker` decides win or lose.
  - `MoveCommand` and `UndoStack` record each move as a list of bead steps so it can be undone.
- **`LevelSession`:** receives tap events, updates the model first, then drives the views and animations. Raises `Won`, `Lost`, `LevelStarted`, and `ProgressChanged`. Win/lose is only reported after the beads finish flying.
- **`GameManager`:** a `GameState` state machine (Menu, Playing, Paused, Win, Lose, LevelSelect, PostLevel) that shows and hides the matching panels and is the entry point for button actions. Handles rewards and progress saving on a first win.
- **`LevelManager`:** holds the ordered list of levels.
- **`PostLevelManager`:** runs the Dialogue then Reward sequence after a first clear.
- **`SaveManager`:** a static wrapper around `PlayerPrefs`, so it needs no scene object.
- **Input:** `GestureDetector` tells taps, drags, and pinches apart and ignores touches on UI; `InputHandler` converts screen positions to grid cells; `CameraControll` handles pan and zoom.

## Adding or Editing Levels

Each level is a `LevelData` asset (**Create → Scriptable Objects → LevelData**) containing:

- `background`: list of cells (color, x, y) forming the target picture.
- `beads`: list of beads (color, x, y) for the starting layout.
- `traySlotCount`: number of tray slots.
- `previewImage`, `dialogue`, `reward`, `parMoves` (0 = use the default Par in `GameConfig`).

Data rules: for every color, the number of beads must equal the number of background cells of that color; beads may only sit on cells that have a background; no duplicate coordinates. Use the **Validate** button in the Inspector or **Tools → Jewel Sort → Level Manager Helper** to check. Then drag the asset into the `levels` list on `LevelManager` in play order.

Bead colors are configured in `ColorPalette.asset`. Any type missing a color renders magenta so it is easy to spot.
