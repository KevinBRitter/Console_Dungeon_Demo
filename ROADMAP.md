<a id="top" />

# Console Dungeon – Development Roadmap

This document contains detailed design notes, phased implementation plans,
and experimental gameplay ideas for Console Dungeon.  As each is implemented I'll add dates or commits indicating when they were implemented.

<-- Back to [README](README.md#roadmap)

---

<a id="phase-1" />

## Phase 1 — Foundations (Player Can “Play”)

Goal: Transition from menus to a minimal but complete playable loop.
`Implemented: 01/25/26 - KR`

- Introduce a GameState model
    - Introduce a `GameState` object (player, dungeon level, RNG seed, flags).
    - Keep it serializable from day one, even before persistence is implemented.
    - Menus should accept and return `GameState` references rather than owning state.
- Implement New Game / Continue stubs
    - Implement `New Game` to initialize a fresh `GameState`.
    - Implement `Continue` as disabled or placeholder (clearly surfaced in UI).
    - Wire `PlayMenu` to transition into a `GameLoop` class.
- Create a turn-based game loop
    - Turn-based loop:
        - Render current state
        - Present player actions
        - Resolve action
        - Update world
        - Check exit/death conditions
    - Keep logic separate from rendering and input
    
<-- Back to [README](README.md#roadmap)

---

<a id="phase-2" />

## Phase 2 — Minimal Dungeon & Movement

Goal: Let the player exist somewhere and move meaningfully. 
`Implemented: 01/26/26 - KR`

- Dungeon Representation
    - Start with a small fixed grid (e.g., 5×5) or linear rooms.
    - Represent rooms with simple metadata (visited, description, encounters).
    - Add a basic procedural generator later; hard-coded layouts are fine initially.
- Movement Actions
    - North / South / East / West options where valid.
    - Clear feedback when movement is blocked.
    - Track visited rooms to change descriptions.
- Fog of War
    - Unknown rooms display minimal information until visited.
    - Reinforces exploration without additional mechanics.

<-- Back to [README](README.md#roadmap)

---

<a id="phase-3" />

## Phase 3 — Player Identity & Progression

Goal: Give the player stats and decisions that matter.

- Player Stats
    - Health, max health
    - Attack / defense (even if combat is trivial at first)
    - Optional: stamina or mana (can be unused initially)
- Character Creation (Lightweight)
    - Choose a class or background with minor stat differences.
    - Avoid skill trees early; keep choices readable and testable.
- Death Handling
    - On death: show summary screen and return to main menu.
    - Keep death cheap and fast to encourage iteration.

<-- Back to [README](README.md#roadmap)

---

<a id="phase-4" />

## Phase 4 — Interaction & Combat

Goal: Introduce risk and consequence.

- Simple Combat System
    - Turn-based, deterministic or lightly randomized.
    - One enemy type initially.
    - Player choices: attack, flee, inspect.
- Encounters
    - Some rooms trigger combat.
    - Others trigger events (traps, shrines, empty rooms).
- Combat as a Sub-Loop
    - Combat should be its own loop, returning control cleanly to the dungeon loop.

<-- Back to [README](README.md#roadmap)

---

<a id="phase-5" />

## Phase 5 — Inventory & Items

Goal: Add long-term decisions and rewards.

- Inventory System
    - Fixed-size list or weightless slots.
    - Items with simple effects (heal, damage bonus).
- Equipment
    - One weapon, one armor slot initially.
    - Stats update dynamically when equipped.
- Meta / Divine Items
    - Track unlocks outside normal save data.
    - Items persist across runs once earned.
    - Explicitly acknowledge the roguelike loop in item flavor text.

---

<a id="phase-6" />

## Phase 6 — Persistence & Polish

Goal: Make runs meaningful beyond a single session.

- Save/Load
    - Serialize `GameState` to JSON.
    - Support one save slot initially.
    - Handle versioning defensively.
- Options Expansion
    - Text speed
    - Color themes
    - Key rebinding (later)
- UX Improvements
    - Clear screen transitions
    - Consistent headers/footers
    - Contextual help prompts
    
<-- Back to [README](README.md#roadmap)

---

<a id="early-game-features" />

## Early Game Feature Ideas (Low Cost, High Value)

These are intentionally small but impactful and fit well before “full systems” exist:

- Room Descriptors
    - Rooms change text after first visit.
    - Adds narrative depth with no mechanical complexity.
- Inspect Action
    - Available in any room.
    - Sometimes yields flavor, sometimes a clue or item.
- Shrines / Altars
    - One-time interaction per run.
    - Small buffs, curses, or meta unlock progress.
- Run Modifiers
    - “This dungeon is cursed” or “Enemies hit harder.”
    - Selected at New Game or randomly applied.
- Debug / Developer Menu
    - Toggle god mode, reveal map, spawn item.
    - Keep it gated but present during development.
- Event Log
    - Simple rolling text log of recent actions.
    - Useful both for players and debugging.

<-- Back to [README](README.md#roadmap)

---

<a id="late-game-enhancements" />

## Late Game Enhancements

- Enemy difficulty scaling based on _gameState.CurrentLevel.LevelNumber
- Critical hits (10% chance for double damage?)
- Loot drops (enemies sometimes drop items, not just gold)
- Rare encounters (mimics, treasure guardians, etc.)
- Environmental hazards (traps, poison gas, etc.)
- Multiple encounter files - TreasureEncounters.json, Enemies.json, etc.
- Localization - Encounters_en.json, Encounters_es.json
- Enemy difficulty tiers - Load different enemy sets per dungeon level
- Hot reload - Watch JSON files and reload without restarting
- Validation - Check for missing placeholders or malformed data

<-- Back to [README](README.md#roadmap)

<-- Back to [top](#top)
