---
description: Title, pause, and scene flow, plus UI with Gum's FNA runtime.
---

# Chapter 9: Game flow and UI with Gum

> Build status: **[unverified]** — Nez APIs verified against source; Gum
> integration (service init/update/draw, `AddToRoot`/`RemoveFromRoot`) verified
> against Gum source @ `dbb35814`; **Gum element layout details (positioning,
> centering, fonts) are [unverified]** — confirm against Gum's docs.

## Where we are

One scene, no flow: the game boots straight into the arena. This chapter
adds the skeleton every game needs — **title → play ⇄ pause** — and wires
up **Gum** (with its FNA runtime) as the UI layer. Game-over arrives in
Chapter 12, when something can actually kill you.

**New files:** `TitleScene.cs`, `PauseController.cs`. **Changed:**
`SpirefallGame.cs` (Gum init/update/draw, boots to title),
`Spirefall.csproj` (`Gum.FNA` package), `ArenaScene.cs` (pause entity).

## Concepts

### Scenes as game states

`Core.Scene = new XScene()` is a full state change: the old scene ends,
the new one begins. Title, arena, pause overlay, game over — each is a
scene (or a scene plus overlay components). Keep them small and single-
purpose; shared logic lives in components, not in scene inheritance
hierarchies. (Nez *has* `SceneTransition`s for animated changes; we use
direct assignment — reliable beats fancy until the game needs fancy.)

### Gum: UI as an overlay, not a scene

Gum renders *after* the scene (`GumService.Default.Draw()` runs after
`base.Draw`), into screen space — it knows nothing about Nez scenes,
cameras, or design resolution. That split is the whole mental model:

- **Nez** owns the world (entities, camera, 320×180 stage).
- **Gum** owns the chrome (titles, menus, HUD) in window pixels.

The wiring, verified in Gum's source, is three calls:

```csharp
// SpirefallGame.Initialize, after base.Initialize():
GumService.Default.Initialize(this, Gum.Forms.DefaultVisualsVersion.Newest);
// SpirefallGame.Update: GumService.Default.Update(gameTime);  (before base.Update)
// SpirefallGame.Draw:   GumService.Default.Draw();            (after base.Draw)
```

{% hint style="warning" %}
**Stale Gum docs:** `GumService` lives in namespace `Gum` — it moved there in mid-2026. Older docs and AI-generated snippets still say `MonoGameGum`; they're stale.
{% endhint %}
The FNA runtime ships as the **`Gum.FNA`** NuGet package (2026.10.1.1
confirmed on nuget.org; Gum source pinned @ `dbb35814` for API reference).

UI elements (`TextRuntime`, `RectangleRuntime`, …) are created in code
and attached with the instance methods `AddToRoot()` / `RemoveFromRoot()`.
{% hint style="warning" %}
**Gum elements outlive scenes:** Because Gum doesn't know about scenes, **every scene that creates elements must remove them when it exits** — orphans from the title screen will haunt the arena.
{% endhint %}

### Pausing with TimeScale

`Time.TimeScale` (verified: a static field; `DeltaTime = dt * TimeScale`)
pauses the *simulation* without pausing the *engine*: every movement in
our game multiplies by `Time.DeltaTime`, so `TimeScale = 0` freezes the
world while `Update()` methods — and therefore input polling — keep
running. Pause is three lines: toggle the flag, set the scale, show/hide
the overlay. No special "paused" update paths anywhere else.

### Nested private components

`TitleScene.TitleInput` is a `private` class inside the scene. When a
component exists only to serve one scene, nesting keeps it out of the
project's namespace — the reader sees the coupling instead of discovering
it.

## Minimal snippets

Gum element lifecycle (layout details [unverified] — see exercises):

```csharp
using Gum;
using MonoGameGum.GueDeriving;

var label = new TextRuntime { Text = "SPIREFALL" };
label.X = 160; label.Y = 60;   // [unverified] positioning/anchor semantics
label.AddToRoot();              // show
label.RemoveFromRoot();         // hide / clean up
```

Scene change with cleanup:

```csharp
((TitleScene)Entity.Scene).HideMenu();
Core.Scene = new ArenaScene();
```

## Exercises

### 1. Style the title

Make the title screen yours: bigger text, a subtitle ("a tiny arena
platformer"), maybe a `RectangleRuntime` behind the title. Use Gum's docs
for the layout properties this chapter marked [unverified].

- *Nudge:* `TextRuntime` has font/size/scale properties — find them in
  Gum's docs or source.
- *Bigger hint:* on FNA, filled rounded shapes don't render (no shape
  support package for FNA) — outlines and sprites are safe.
- *Near-solution:* record every [unverified] claim you confirm; we'll
  fold the corrections back into the chapter.

### 2. Center properly

The `X = 160` positioning in the solution is a guess at centering. Find
Gum's real anchoring (horizontal alignment / origin) and center the title
for real.

- *Nudge:* look for `HorizontalAlignment` or an origin property on the
  runtime.
- *Bigger hint:* Gum elements usually anchor relative to their parent —
  the root is the window.
- *Near-solution:* the window is 1280×720 while the game stage is 320×180 —
  note which coordinate space Gum lives in.

### 3. Pause stops particles too?

Pause the game while dust is mid-air. Does the dust freeze? Why or why
not?

- *Nudge:* particles move by *their* delta time — which one?
- *Bigger hint:* if they use unscaled time, they ignore the pause.
- *Near-solution:* decide whether that's a bug or a feature (frozen dust
  looks dead; drifting dust looks alive), and note it.

### 4. Break it: orphan the title

Remove the `HideMenu()` call. Start the game, then go back to title (T
from pause). Describe what happens.

- *Nudge:* Gum elements aren't scene children.
- *Bigger hint:* the title text is still on the root — over the new title.
- *Near-solution:* every scene owns its Gum teardown. Consider a helper
  (Exercise 5).

### 5. A Menu helper

Write a tiny `Menu` class: `Add(TextRuntime)`, `Show()`, `Hide()` (adds/
removes all from root). Refactor `TitleScene` and `PauseController` onto it.

- *Nudge:* a `List<GraphicalUiElement>` field does it.
- *Bigger hint:* `Hide()` iterates and calls `RemoveFromRoot()`.
- *Near-solution:* this is the seed of your UI toolkit — Chapter 11's HUD
  will use it.

### 6. Game-over hook (design)

In your notes: what *should* kill the player in this game, and what the
game-over flow looks like (scene? overlay? respawn timer?). No code —
Chapter 12 implements it.

- *Nudge:* TowerFall: one arrow hit = death, instant respawn.
- *Bigger hint:* instant respawn keeps versus flowing; a game-over *scene*
  fits single-player better.
- *Near-solution:* decide now; Chapter 12 holds you to it.

### 7. Transitions

Read Nez's `SceneTransition` source (it's abstract; find a concrete one
like `FadeTransition`… [unverified] name — check). In your notes, sketch
where you'd hook a fade between title and arena.

- *Nudge:* `Core.Scene = new ArenaScene()` is the seam.
- *Bigger hint:* transitions need the *outgoing* scene's last frame —
  that's why they wrap the assignment, not the scene.
- *Near-solution:* no code yet. Juice for scene changes, later.

## Checkpoint

Run: `dotnet run`.

You should see: a title screen ("SPIREFALL / press Z to start"). Z →
arena, as before. Esc → "PAUSED" overlay, world frozen; Esc → resume;
T → back to title (clean — no orphaned text). The flow skeleton is
complete.

Done means: exercises 1–2 complete (with [unverified] items confirmed or
corrected); 3–7 attempted. Gum is a dependency now — `Gum.FNA` restores
with the build.

## Next

Chapter 10 adds sound: jump/land/dash effects and music, loaded through
the content pipeline.
