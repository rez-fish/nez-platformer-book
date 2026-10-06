# AI_NOTES.md — working notes for the Nez/FNA platformer book

Keep under ~150 lines. No solutions here. If notes and repo disagree, repo wins:
flag it and fix the notes.

## Repo locations

- Local: `~/workspace/nez-platformer-book` (git, branch `main`)
- GitHub: `https://github.com/rez-fish/nez-platformer-book` (private).
  Push via `github call-tool push_files` (MCP) — the App can't create repos
  (403), so Reza created it manually. Plain `git push` has no auth here.
- GitBook: `.gitbook.yaml` at root points at `./book/`. Reza connects the
  space himself (GitBook → new space → Git Sync → select repo, branch `main`).
  STATUS 2026-10-05: repo pushed; GitBook space not yet connected.

## Project snapshot

Project-based book: build ONE arena platformer with Nez on FNA, BYTEPATH-style.
Game (working title "Spirefall"): single-screen pixel-art arena; a hooded
archer runs, jumps, dashes (Celeste-like feel), shoots arrows; later chapters
add enemies, then local versus (TowerFall-like). Online multiplayer DROPPED
per Reza (2026-10-05): skip rather than half-bake. UI: Gum (vchelaru/Gum)
with its FNA runtime, not Nez.UI. Reza provides his own pixel-art assets
at common sizes (16x16 tiles). Reza develops on Windows.

## Chapter status (outline approved 2026-10-05; all 14 written 2026-10-05)

| # | Status | One-line summary |
|---|--------|------------------|
| 01 | done | Toolchain on Windows: SDK, fnalibs, Nez source ref, first window |
| 02 | done | C# for TS/Go devs + game loop (Update/Draw, input polling) |
| 03 | done | Scene/Entity/Component + pixel-perfect rendering |
| 04 | done | Ground movement feel: accel, variable jump, coyote, buffering |
| 05 | done | Collisions: colliders, SpatialHash, Mover, axis-separated resolve |
| 06 | done | Wall slide/jump, dash, movement state machine |
| 07 | done | Tilemap levels via Tiled + TiledMapMover |
| 08 | done | Camera follow/lookahead, screenshake, particles |
| 09 | done | Game flow (title/pause) + UI with Gum |
| 10 | done | Audio: SFX via FromStream, music notes |
| 11 | done | Combat: projectiles, target dummies, score, HUD |
| 12 | done | Enemies: FSM AI, line-of-sight linecasts, death/respawn |
| 13 | done | Local versus: virtual-input abstraction, owned arrows, first to 5 |
| 14 | done | Ship it: publish, fnalibs, player README, itch draft |

## Pinned versions (verified 2026-10-05 via shallow clone)

- Nez: `net10-update` branch @ `3f8cc40` (prime31/Nez). NOTE: `Directory.Build.props`
  targets `net10.0`, but `Nez.Portable/Nez.FNA.Core.csproj` still targets `net8.0`
  — discrepancy to resolve before ch01.
- Nez-Samples: master @ `c2d0996` (prime31/Nez-Samples). Has `Platformer` sample
  (TiledMapMover, triggers, items) — closest reference for our game.
- FNA: master @ `24031e5b` (2026-10-04, FNA-XNA/FNA). Pairs with Nez.FNA.Core
  (both net8.0). Pairing [unverified] — nothing compiled yet.
- Gum: source @ `dbb35814` (2026-10-05, vchelaru/Gum). Verified from source:
  `GumService` in namespace `Gum`; `Initialize(Game, DefaultVisualsVersion)`,
  `Update(GameTime)`, `Draw()`; `GraphicalUiElement.AddToRoot/RemoveFromRoot`
  instance methods; `TextRuntime` in `MonoGameGum.GueDeriving` with `Text`.
  FNA runtime = NuGet **`Gum.FNA` 2026.10.1.1** (confirmed on nuget.org).
  Element layout/positioning/centering details [unverified] — ch09/11/13 mark them.
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
- ch02: `Update`/`Draw` overrides, `Nez.Input` (`IsKeyDown/Pressed/Released`),
  `Time.DeltaTime` (static field), `IsFixedTimeStep=false`, `Graphics.Instance.Batcher`,
  `Texture2D.SetData`, `MathHelper`; C#: struct-vs-class, fields-vs-properties.
- ch03: `Scene.CreateEntity`, `Entity.Position` (struct-copy gotcha),
  `AddComponent`, `Component` + `IUpdatable` (explicit impl),
  `OnAddedToEntity`, `Scene.SetDesignResolution` + `ShowAllPixelPerfect`,
  `SpriteRenderer(Texture2D)`; C#: static class, XML docs.
- ch04: forgiveness timers (coyote/buffer/jump-cut) on `IUpdatable.Update`.
- ch05: `BoxCollider` ctors, `Mover.Move` + `out` discard, `CollisionResult`,
  `Physics`/`SpatialHash`, `Entity.GetComponent`; C#: `out _`, local functions.
- ch06: wall/dir capture via `Math.Sign`, enum state machine; C#: enums, early return.
- ch07: `Scene.Content.LoadTiledMap`, `TiledMapRenderer`, `TiledMapMover(TmxLayer)`,
  `Nez.Tiled`; csproj `Content/**` copy rule.
- ch08: `Camera.Position` = view center (verified), `ParticleEmitter(Config)`
  (`Play/Stop`, blend fields must be set), trauma shake, `?.`, `FindEntity`.
- ch09: `Core.Scene =` transitions, `Time.TimeScale` (static field, pauses sim),
  Gum wiring (`GumService.Default.Initialize/Update/Draw`); nested components.
- ch10: `SoundEffect.FromStream`, `TitleContainer.OpenStream`, `Sfx` static board.
- ch11: `Entity.Destroy`, hit identification via `GetComponent` on
  `hit.Collider.Entity`; C#: `do…while`, component ctor args.
- ch12: `Physics.Linecast` LOS (`hit.Collider == null`), AI sense→think→act;
  C#: expression-bodied members, `readonly` ctor fields.
- ch13: `VirtualIntegerAxis`/`VirtualButton` (`AddKeyboardKeys`,
  `AddGamePadLeftStickX/DPad…/Button`, `Value`, `IsPressed`/`IsDown`,
  auto-update via `Input`), arrow ownership, midpoint camera.
- ch14: `dotnet publish` (fd vs sc), fnalibs natives next to exe.

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

- Retroactive fix 2026-10-06: ch04 title dropped the dangling "part 1"
  ("Game-feel movement — the ground game"); ch05/ch06 were its continuation
  but never labeled part 2/3. Changed in ch04 file, SUMMARY.md. Per Reza's
  question — nothing was missing.
- Visual polish pass 2026-10-06 (per Reza: "make the book site look visually
  more appealing"): added `description` frontmatter to all 15 pages;
  converted 16 key callouts to GitBook `{% hint %}` blocks (info/success/
  warning/danger); generated figures in `book/assets/` via
  `code/tools/make_figures.py` (loop, ec-hierarchy, axis-separation,
  state-machine, ai-pipeline, arena-overview SVGs + cover.svg) and embedded
  them in ch02/ch03/ch05/ch06/ch07/ch12 + intro hero; rewrote intro.md
  (was a stale skeleton placeholder). All assets are text (SVG) so they
  push via push_files. Theme colors/fonts/logo are GitBook UI settings,
  not repo-controlled — checklist given to Reza.
- Full read-through review 2026-10-06 (per Reza): read all 14 chapters as
  a student + audited Nez FAQs/Samples, FNA docs, BYTEPATH. Fixed:
  fnalibs verified (SDL3.dll/FNA3D.dll/FAudio.dll/libtheorafile.dll from
  fnalibs-dailies win-x64 — dropped [unverified], was "SDL2"); ch07 figure
  re-rendered from ch07's 20x11 map (was ch14's 40x11); 3 hint blocks moved
  out of bullet lists (GitBook can't render hints as list items);
  ch09 Next no longer says "content pipeline" (ch10 is pipeline-free);
  scope boundary (local-only multiplayer) stated in intro, ch14 references
  it; `Assembly.GetEntryAssembly()?.GetName().Version` fix. Open items from
  the review (not yet done): nothing compiled — the build-verification pass
  on Reza's Windows machine is still the highest-value next step; see the
  review's tweak list for structural candidates (coroutines chapter, sprite
  animation, VirtualInput earlier, save/settings, coding-practices essay).

- No .NET SDK on this machine: nothing compiled. ALL chapters' snippets and
  solutions are [unverified]; Reza reports build failures as chapter bugs.
- Gum element layout/positioning/centering marked [unverified] in ch09/11/13;
  confirm against Gum docs on first build and fold corrections back.
- Particle field semantics (angle units etc.) [unverified] — tune by eye (ch08).
- Song/music loading via `Song.FromUri` [unverified] (ch10 ex5).
- fnalibs: exact Windows x64 archive name + DLL list not confirmed — ch01
  exercise 7 tells Reza to check the FNA "Setting Up FNA" docs page live.
- Online multiplayer DROPPED per Reza 2026-10-05 — do not reintroduce without asking.

## Resume protocol

1. Read this file, `book/SUMMARY.md`, and the most recent chapter file.
2. `git log --oneline -5` and `git status` in repo root.
3. Tell Reza in 2-3 sentences where we are, then await instruction.
4. One chapter per request unless asked otherwise; update this file + SUMMARY.md
   at session end.
