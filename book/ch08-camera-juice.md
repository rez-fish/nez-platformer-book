# Chapter 8: Juice — camera, screenshake, particles

> Build status: **[unverified]** — written against Nez @ `3f8cc40`, not compiled.
> The particle field semantics (angle units etc.) are from source inspection,
> not a runtime check.

## Where we are

The game plays but feels dead: a static camera, no feedback on landing or
dashing. This chapter adds the three cheapest juice systems — a following
camera with lookahead, trauma-based screenshake, and dust particles — and
widens the arena to 40 tiles so the camera has somewhere to go.

**New files:** `CameraFollow.cs`. **Changed:** `ArenaScene.cs` (camera rig
+ dust emitter entities, wider map), `PlayerController.cs` (trauma/dust
hooks on land and dash), `Content/Arena/arena.tmx` (regenerated 40×11).

## Concepts

### The camera is a position

Nez's `Camera.Position` is the world coordinate at the *center* of the
screen (verified in `Camera`'s view-matrix source: it translates by
`-Position` then by the screen center). So "follow the player" is just
"set `Camera.Position` to the player, smoothed." Two refinements:

- **Lookahead**: aim at `position + velocity * 0.25s`, not the position.
  The camera leans into motion, showing where you're *going*. Clamp the
  result so the view never leaves the level.
- **Framerate-independent smoothing**: `t = 1 - e^(-k·dt)` then
  `Lerp(current, desired, t)`. A fixed `Lerp(a, b, 0.1)` is frame-rate
  dependent (Chapter 2's lesson, again); the exponential form converges the
  same way at any fps.

### Trauma-based screenshake

Shake as a single decaying number — *trauma* — is the cleanest formulation
(game-feel canon, from "Juice it or lose it" talks): events *add* trauma
(land: 0.22, dash: 0.18), it decays linearly, and the applied offset is
`trauma² × max`. Squaring means small bumps are subtle and big hits are
violent — the response curve does the design work. The offset is random
per frame in ±range, applied *after* the follow, so shake never fights
the smoothing.

### Particles, Nez-style

`ParticleEmitter` + `ParticleEmitterConfig`: max particles, emission rate,
lifespan, speed/angle (+ variances), gravity, start→finish color and size,
duration. Our dust is a *one-shot*: `playOnAwake: false`, `Duration`
0.12 s, and `Play()` re-fires it. One reusable emitter entity, repositioned
to the feet on each landing — cheaper than spawning entities per puff.

Two gotchas, both found in source, not docs: `Angle`'s units are
[unverified] (assumed degrees here — tune by eye), and the blend fields
default to `0`, which is *not* a valid XNA `Blend` — set
`SourceAlpha`/`InverseSourceAlpha` explicitly or nothing renders.

### Wiring without coupling

`PlayerController` finds the rig and dust via
`Entity.Scene.FindEntity("camera-rig")?.GetComponent<CameraFollow>()` in
`OnAddedToEntity`. The `?.` (null-conditional) means the controller works
even in a scene without juice — no crash, just no shake. Name-based lookup
is a pragmatic seam: explicit, greppable, and fine at this scale.

### C# for the TS/Go engineer, part 5

- **`?.`**: `a?.B()` evaluates to null instead of throwing if `a` is null.
  Use it at seams between systems that shouldn't hard-depend on each other.
- **`readonly` fields**: `readonly Random _random` — assigned once (here,
  inline), never again. `const` is compile-time; `readonly` is runtime-once.

## Minimal snippets

Trauma, the whole pattern:

```csharp
public void AddTrauma(float amount) => Trauma = Math.Min(Trauma + amount, 1f);
// per frame:
Trauma = Math.Max(Trauma - decay * dt, 0f);
var mag = Trauma * Trauma * maxOffset;
```

A one-shot emitter:

```csharp
var emitter = new ParticleEmitter(config, playOnAwake: false);
// later, at the event:
emitter.Entity.Position = eventPosition;
emitter.Play();
```

## Exercises

### 1. Tune the shake

Change land trauma (0.22), dash trauma (0.18), and `MaxShakeOffset`.
Find the line between "punchy" and "nauseating".

- *Nudge:* all three are constants/arguments, no logic changes.
- *Bigger hint:* because offset scales with trauma², doubling trauma
  quadruples the shake — small adjustments.
- *Near-solution:* write your numbers down; Chapter 11's arrow hits will
  want their own trauma values.

### 2. Lookahead on Y

The camera currently only looks ahead horizontally (Y is fixed at 88).
Add vertical lookahead when the level gets taller — or argue in your notes
why fixed-Y is correct for a single-screen-tall arena.

- *Nudge:* `desired.Y = player.Y + velocity.Y * scale`, clamped to the
  level bounds.
- *Bigger hint:* vertical lookahead during fast falls feels bad — consider
  scaling it down or only looking *up*.
- *Near-solution:* no code required if your argument is good. Design
  judgment counts.

### 3. Dash trail

Emit dust *continuously* during the dash, not just at its start.

- *Nudge:* the dash branch runs every frame for 0.15 s.
- *Bigger hint:* reposition + `Play()` each frame is 9 restarts — instead,
  set the emitter's `Duration` ≥ dash time and call `Play()` once at dash
  start, `Stop()` at dash end.
- *Near-solution:* `ParticleEmitter` has `Stop()` and `Pause()` — check
  which one freezes existing particles vs stops new ones.

### 4. Break it: shake without decay

Set `TraumaDecay = 0`. Dash and land a few times.

- *Nudge:* trauma only grows now.
- *Bigger hint:* it clamps at 1 — permanent max shake.
- *Near-solution:* revert. Decay is what makes trauma a *resource* rather
  than a *state*.

### 5. Hitstop (design + code)

Freeze the game for 60 ms on landing from a high fall: set
`Time.TimeScale = 0f`, then restore it after a timer.

- *Nudge:* `Time.TimeScale` exists (Nez-Core FAQ). A coroutine
  (`Core.StartCoroutine`) with `Coroutine.WaitForSeconds` restores it.
- *Bigger hint:* `WaitForSeconds` uses scaled time — a 0 timeScale would
  freeze the waiter too. Use unscaled time or count frames manually.
- *Near-solution:* this is the classic juice technique; even 3 frames of
  freeze makes impacts land. [unverified: exact Time API names — check
  `Time.cs` in source.]

### 6. Your own particle

Add a second emitter for wall-slide sparks (gray, sideways). Trigger it
while `wallSliding`.

- *Nudge:* `wallSliding` is a local in `Update` — promote what you need.
- *Bigger hint:* reuse the dust entity pattern: second entity, second
  config, `Play()` while sliding, `Stop()` when not.
- *Near-solution:* tune `Angle`/`AngleVariance` until the sparks fly
  *away* from the wall — this is also how you verify the angle units.

### 7. Camera bounds for your arena

Exercise 6 of Chapter 7 asked for a two-player arena sketch. Compute the
camera clamp values it would need, in your notes.

- *Nudge:* `clamp(x, halfView, levelWidth - halfView)`.
- *Bigger hint:* symmetric arenas make this trivial — one less thing to
  get wrong.
- *Near-solution:* Chapter 13 will hold you to it.

## Checkpoint

Run: `dotnet run`.

You should see: the wider arena; the camera gliding ahead of your runs
and settling when you stop; a soft thud-shake and dust puff on every
landing; a sharper shake + puff on dash start. Wall-slide down the border
walls with the camera holding steady.

Done means: exercises 1–2 complete; 3–7 attempted. Juice values written
down next to your movement tuning.

## Next

Chapter 9 adds game flow — title screen, pause, game over — and the UI to
go with it, built with Gum.
