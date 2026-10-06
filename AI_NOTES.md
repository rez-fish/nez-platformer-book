# AI_NOTES.md — working notes for the Nez/FNA platformer book

Keep under ~150 lines. No solutions here. If notes and repo disagree, repo wins:
flag it and fix the notes.

## Project snapshot

Project-based book: build ONE arena platformer with Nez on FNA, BYTEPATH-style.
Game (working title "Spirefall"): single-screen pixel-art arena; a hooded
archer runs, jumps, dashes (Celeste-like feel), shoots arrows; later chapters
add enemies, then local versus (TowerFall-like). Online multiplayer DROPPED
per Reza (2026-10-05): skip rather than half-bake. UI: Gum (vchelaru/Gum)
with its FNA runtime, not Nez.UI. Reza provides his own pixel-art assets
at common sizes (16x16 tiles). Reza develops on Windows.

## Chapter status (outline approved 2026-10-05, 14 chapters)

| # | Status | One-line summary |
|---|--------|------------------|
| 01 | done | Toolchain on Windows: SDK, fnalibs, Nez source ref, first window |
| 02 | planned | C# for TS/Go devs + game loop (Update/Draw, input polling) |
| 03 | planned | Scene/Entity/Component + pixel-perfect rendering |
| 04 | planned | Ground movement feel: accel, variable jump, coyote, buffering |
| 05 | planned | Collisions: colliders, SpatialHash, Mover, axis-separated resolve |
| 06 | planned | Wall slide/jump, dash, movement state machine |
| 07 | planned | Tilemap levels via Tiled + TiledMapMover |
| 08 | planned | Camera follow/lookahead, screenshake, particles |
| 09 | planned | Game flow (title/pause/game-over) + UI with Gum |
| 10 | planned | Audio: SFX, music, mixing |
| 11 | planned | Combat: projectiles, triggers, pickups, score |
| 12 | planned | Enemies: state machines, line-of-sight linecasts |
| 13 | planned | Local multiplayer: multi-device input, versus arena |
| 14 | planned | Ship it: content pipeline, Windows packaging with fnalibs |

## Pinned versions (verified 2026-10-05 via shallow clone)

- Nez: `net10-update` branch @ `3f8cc40` (prime31/Nez). NOTE: `Directory.Build.props`
  targets `net10.0`, but `Nez.Portable/Nez.FNA.Core.csproj` still targets `net8.0`
  — discrepancy to resolve before ch01.
- Nez-Samples: master @ `c2d0996` (prime31/Nez-Samples). Has `Platformer` sample
  (TiledMapMover, triggers, items) — closest reference for our game.
- FNA: master @ `24031e5b` (2026-10-04, FNA-XNA/FNA). Pairs with Nez.FNA.Core
  (both net8.0). Pairing [unverified] — nothing compiled yet.
- Gum (UI): vchelaru/Gum has an FNA runtime and a Nez integration doc
  (`docs/code/nez.md`); README lists Nez as supported. TODO: pin Gum version;
  the nez.md doc predates Gum's June-2026 namespace move (`GumService` → `Gum`
  namespace), so verify setup code at write time. NOTE: on FNA, shape *fill*
  needs a support package that doesn't ship for FNA — UI styling must avoid
  relying on filled rounded shapes.
- .NET SDK: none installed on this machine — nothing has been compiled yet.
- OS (build machine): Linux. Reza's dev OS: Windows — ch01/ch14 target Windows.

## Architecture / conventions

- Repo: `/book` (chapters + SUMMARY.md), `/code/chNN-start`, `/solutions/chNN`.
- Game: `Spirefall/`, net8.0 exe; sibling layout `TopLevel/{FNA,Nez,Spirefall}`;
  refs: `Nez.Portable/Nez.FNA.Core.csproj` + `FNA/FNA.Core.csproj`.
- C# naming: PascalCase types/methods, camelCase locals, `_camelCase` private fields
  (match Nez source style).
- Fixed timestep, pixel-perfect design resolution for pixel art (details in ch03).
- Movement: custom kinematic controller on Nez `Mover` + `BoxCollider`, NOT Farseer.

## APIs / concepts introduced (by chapter)

- ch01: `Core` ctor (window config), `Core.Scene` static prop (first-set semantics),
  `Core.GraphicsDevice`, `Scene.ClearColor`, XNA `Game.Run()/Initialize()`,
  `[STAThread]`, `ProjectReference` to pinned sources.

## Style / exercise conventions

- Documentation voice, not video-tutorial voice. Reference first, then exercises.
- Skip general programming; explain C# idioms + XNA/FNA concepts on first use.
- Chapters: Where-we-are (2-3 sentences) → concepts/reference → minimal snippets
  (API demo only, never the exercise answer) → 5-10 exercises, easiest first,
  each with 3-tier hints (nudge / bigger hint / near-solution) → checkpoint
  (what it looks like, how to run) → "Next" (1 line).
- Full solutions ONLY in `/solutions/chNN`, never in chapter text or here.
- Every snippet/solution must compile at pinned versions; mark `[unverified]`
  when it can't be checked.

## Known issues / unverified / TODOs

- No .NET SDK on this machine: nothing compiled. ALL ch01 snippets/solutions
  are [unverified]; verify on first SDK access, fix chapter if anything fails.
- fnalibs: exact Windows x64 archive name + DLL list not confirmed — ch01
  exercise 7 tells Reza to check the FNA "Setting Up FNA" docs page live.
- Nez-Samples targets older solutions; Platformer sample may need tweaks for
  net10-update [unverified].
- Vinluo/Celeste controller repo not yet inspected (license + API TBD).
- Online multiplayer DROPPED per Reza 2026-10-05 — do not reintroduce without asking.
- FNA native libs (fnalibs) packaging for Windows needed for ch01/ch14.

## Resume protocol

1. Read this file, `book/SUMMARY.md`, and the most recent chapter file.
2. `git log --oneline -5` and `git status` in repo root.
3. Tell Reza in 2-3 sentences where we are, then await instruction.
4. One chapter per request unless asked otherwise; update this file + SUMMARY.md
   at session end.
