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

## Controls
- Keyboard input via the console.
- Numeric menu choices (e.g., `1`, `2`, `3`).
- `Console.ReadKey` used for pause/acknowledgement screens.

## Navigation model
- Menus implement `IMenu` and return a bool from `Show()`:
  - `true` means remain in the current flow (or continue the loop).
  - `false` signals a return to the previous menu or exit.
- `MainMenu` instantiates `PlayMenu` and loops while `PlayMenu.Show()` returns `true`, keeping control flow clear and easy to extend.

## Project structure (important files)
- `Program.cs` — application entry and main loop.
- `Menus/IMenu.cs` — menu interface.
- `Menus/MainMenu.cs` — entry menu; instantiates other menus.
- `Menus/PlayMenu.cs` — play submenu with New/Continue/Return options.
- `UI/ScreenRenderer.cs` — draws framed screens and handles layout.
- `UI/TextFormatter.cs` — wraps text to fit the screen width.
- `Input/InputHandler.cs` — central place for console input.

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

## Roadmap / next steps
- Wire `OptionsMenu` into `MainMenu` with the same navigation model.
- Replace boolean returns with a `MenuAction` enum if multiple navigation outcomes are required.
- Add a simple dungeon generator and a turn-based action loop.
- Add unit tests for menus and text formatting.

## Contribution
- Open a PR for any feature or bugfix.
- Keep changes small and focused; prefer single-responsibility commits.
- Write unit tests for non-UI logic where feasible.

## Notes for development
- Consider converting input/output to interfaces for automated testing (e.g., `IConsole`).
- Keep `ScreenRenderer` and `TextFormatter` static for now since they are stateless helpers; convert only if instance customization is needed.
