<a id="top" />

# Console Dungeon (Demo)

Console Dungeon is a small, extendable console-based roguelike demo written in C# targeting .NET 8. The project is intentionally minimal so you can iterate on gameplay, UI, and architecture without heavy tooling.

## Elevator pitch
Navigate a simple dungeon through a text-based UI. Explore, manage a party or single adventurer, and make choices through clearly structured menus. The demo focuses on clean code, easy extensibility, and a polished console presentation.

## Core gameplay loop
- Player starts at the `Main Menu`.
- Choosing `Play` opens the `Play` menu where a new run may be started or continued.
- In a full game, `Play` would transition into the dungeon loop: generate a level -> present options (move, inspect, inventory) -> resolve player actions -> update world and render -> repeat until death/exit.
- Menus return booleans to indicate navigation (stay, back), keeping control flow explicit and testable.

## Features (current & planned)
- Clean, bordered console rendering via `UI/ScreenRenderer.cs`.
- Text wrapping and formatting in `UI/TextFormatter.cs`.
- Input capture through `Input/InputHandler.cs`.
- Menu system implemented as instance menus (implement `IMenu`) to support DI and unit testing.
Planned:
- Player stats, simple combat, and inventory.
- Persistent saves (Continue).
- Difficulty and display options.
- More menus (Options, Inventory, Character).
- Meta objects like divine weapons and armor that act as soul bound gifts from gods aware of the game loop and available in any playthrough after earning

## Controls
- Keyboard input via the console.
- Numeric menu choices (e.g., `1`, `2`, `3`).
- `Console.ReadKey` used for pause/acknowledgement screens.

## Navigation model
- Menus implement `IMenu` and return a bool from `Show()`:
- Values such as Stay, Back, Options, Play, and Exit indicated navigation outcomes
- `MainMenu` instantiates `PlayMenu` and `OptionsMenu` looping while each menu returns an appropriate `MenuAction`, keeping control flow clear and easy to extend.

## Project structure (important files)
Console_Dungeon/
├── Actions/
│   ├── IGameAction.cs
│   ├── RestAction.cs
│   └── ShowStatusAction.cs
├── Data/
│   ├── CharacterClasses.json
│   ├── Encounters.json
│   ├── Messages.json
│   └── RoomDescriptions.json
├── Encounters/
│   └── EncounterHandler.cs
├── Enums/
│   ├── `MenuAction.cs` - enum representing possible menu actions.
│   └── PlayerClass.cs
├── Generation/
│   └── LevelGenerator.cs
├── Input/
│   └── `InputHandler.cs` — central place for console input.
├── Managers/
│   ├── CharacterClassManager.cs
│   ├── EncounterManager.cs
│   ├── MessageManager.cs
│   └── RoomDescriptionManager.cs
├── Menus/
│   ├── CharacterCreationMenu.cs
│   ├── `IMenu.cs` — menu interface.
│   ├── `MainMenu.cs` — entry menu; instantiates other menus.
│   ├── `OptionsMenu.cs` — options submenu with audio and controls settings.
│   └── `PlayMenu.cs` — play submenu with New/Continue/Return options.
├── Models/
│   ├── CharacterClassData.cs
│   ├── CombatResult.cs
│   ├── DungeonLevel.cs
│   ├── EncounterData.cs
│   ├── GameState.cs
│   ├── Player.cs
│   └── Room.cs
├── Movement/
│   └── MovementHandler.cs
├── UI/
│   ├── `ScreenRenderer.cs` — draws framed screens and handles layout.
│   └── `TextFormatter.cs` — wraps text to fit the screen width.
├── DebugLogger.cs
├── GameLoop.cs
└── `Program.cs` — application entry and main loop.

## Technical notes for contributors
- Language: C# 12; Target: .NET 8.0
- Prefer instance menu classes (not static) to allow dependency injection and mocking.
- Keep UI rendering stateless and pure — let menus supply text content.
- Follow existing naming and formatting style in `UI` and `Menus`.

## How to run

- From the repository root:
  - `dotnet run --project Console_Dungeon` (or open in Visual Studio and press F5).
- Console window width: the renderer assumes ~80 columns; maximize or set console width for best presentation.

## Running unit tests

A test project `Console_Dungeon.Tests` (xUnit) is included to validate layout and formatting.

From the repo root:

- Run all tests: `dotnet test`

In Visual Studio:

- Open __Test Explorer__ and run tests there.

Notes:
- Tests capture renderer output via `ScreenRenderer.Output` (a `TextWriter`) — you don't need to redirect `Console.Out`.
- `ScreenRenderer.DrawScreen(string body)` uses built-in defaults for header/footer. Pass `""` to suppress defaults or pass explicit `header`/`footer` arguments to override.
- `Console.Clear()` is intentionally guarded to avoid failing tests when no console is attached.

<a id="roadmap" />

## Roadmap / Next Steps
- [Phase 1 — Foundations (Player Can “Play”)](ROADMAP.md#phase-1)
- [Phase 2 — Minimal Dungeon & Movement](ROADMAP.md#phase-2)
- [Phase 3 — Player Identity & Progression](ROADMAP.md#phase-3)
- [Phase 4 — Interaction & Combat](ROADMAP.md#phase-4)
- [Phase 5 — Inventory & Items](ROADMAP.md#phase-5)
- [Phase 6 — Persistence & Polish](ROADMAP.md#phase-6)
- [Early Game Feature Ideas (Low Cost, High Value)](ROADMAP.md#early-game-features))

## Contribution
- Open a PR for any feature or bugfix.
- Keep changes small and focused; prefer single-responsibility commits.
- Write unit tests for non-UI logic where feasible.

## Notes for development
- Consider converting input/output to interfaces for automated testing (e.g., `IConsole`).
- Keep `ScreenRenderer` and `TextFormatter` static for now since they are stateless helpers; convert only if instance customization is needed.

<-- Back to [top](#top)