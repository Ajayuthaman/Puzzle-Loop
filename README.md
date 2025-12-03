# 🔌 Puzzle Loop — Unity Project

A complete modular puzzle system where players rotate wire pieces to connect power across a grid.
Includes level loading, rotation logic, BFS-based power propagation, win detection, UI flow, save system, and audio manager hooks.

---

## 📌 Overview

This project implements a fully functional **pipe-connection / power-flow puzzle** similar to classic circuit-based logic games.
Each level is defined using a **ScriptableObject**, which is turned into a dynamic grid of interactive wire tiles.
Players rotate the tiles to create continuous connections from **power sources** to all other **wire cells**.

---

## 🎮 Features

### ✔ Dynamic Level Generation

* Levels defined via ScriptableObject (`LevelData`)
* Auto-instantiated grid
* Adjustable row/column sizes
* Supports multiple wire types & rotations

### ✔ Player Interaction

* Click to rotate wire
* Smooth rotation animation
* Real-time power propagation

### ✔ Smart Power System

* BFS flood-fill algorithm
* Auto-detection of connected neighbors
* Powered/unpowered visual feedback

### ✔ Win System

* Detects full-grid connectivity
* Plays audio + triggers win panel
* Integrates with the save system

### ✔ Save System

* Save/load completed levels
* Tracks score
* Maintains current level index

### ✔ UI Flow

* Main menu
* Levels menu
* Game scene loading
* Win/menu transitions

---

# 🗂 Project Structure

```
/Scripts
   /Core
      PowerGridManager.cs
      Wire.cs
      LevelManager.cs
      LevelData.cs
      EventManager.cs
   /UI
      MainMenuController.cs
      LevelsMenuController.cs
      LevelSceneController.cs
      LevelButtonItem.cs 
   /Saving
      LevelSaveManager.cs
   /Audio
      AudioManager.cs

/ScriptableObjects
   LevelData.asset (multiple levels)
```

---

# 📄 Script Documentation

## 1️⃣ PowerGridManager.cs

Handles **spawning**, **player input**, **power propagation**, and **win detection**.

### Responsibilities:

* Load selected level
* Spawn grid & initialize wires
* Handle player clicks
* Run BFS flood-fill from all power sources
* Update visual states
* Detect level win
* Trigger save & scene controller UI

### Flow:

1. Load selected level index
2. Spawn wires based on LevelData
3. Run initial fill
4. On rotate → re-check power & win
5. If win → save progress + show win panel

---

## 2️⃣ Wire.cs

Represents a single cell (wire) in the grid.

### Responsibilities:

* Store wire type + rotation
* Instantiate the appropriate prefab
* Handle player rotation
* Run smooth animation
* Provide connection end-points for raycast-based flood-fill
* Update powered/unpowered visuals

### Key Features:

* Raycast from connection points to find neighbors
* Prevent rotation on power sources/empty cells
* Tight animation loop using smoothstep
* Calls EventManager on rotate

---

## 3️⃣ LevelData.cs

ScriptableObject that defines **each level layout**.

### Contains:

* Rows, Columns
* List<WireCell> grid

### WireCell:

Encodes:

```
rotation * 10 + wireType
```

### Automatically validates list size on edit.

---

## 4️⃣ MainMenuController.cs

Controls the main menu UI:

* Play
* Quit
* Switch to Levels Menu
* Load game scene

---

## 5️⃣ LevelSaveManager.cs

Handles persistent save data.

### Saves:

* Current level index
* Completed level scores
* Level unlocks

Uses JSON file stored locally.

---

# 🔊 Audio

Integrated through `AudioManager`

* `PlayClick()` when rotating wires
* `PlayWin()` on level completion

---

# 🎯 Game Loop Diagram

```
Player Clicks → Wire Rotates → UpdateFill → BFS Propagation 
     ↓                                         ↓
 Check Win ← Update Visual State ← Powered Wires Marked
     ↓
 Win Panel + Save Progress
```

---

# 🧪 How To Create New Levels

1. Right-click in Project →
   **Create → Levels → New Level Data**

2. Set:

   * Rows
   * Columns

3. Configure each cell:

   * Type (Power, Straight, Corner…)
   * Rotation (0°, 90°, 180°, 270°)

4. Save the asset.

5. Add it to the `LevelManager` list.

---

# 🚀 How To Play

1. Enter **Main Menu**
2. Select level from **Levels Menu**
3. Rotate the tiles to connect all wires
4. When the entire grid is powered → **You Win!**

---

# 🔧 Dependencies

✔ Unity 2021+
✔ 2D Renderer
✔ Prefabs for each wire type
✔ LevelData ScriptableObjects
✔ Audio clips for click/win
