# Chapter 4: Game-feel movement — the ground game

> Build status: **[unverified]** — written against Nez @ `3f8cc40`, not compiled.

## Where we are

The archer renders but doesn't move. This chapter gives it a body:
acceleration-based running and a jump tuned with the three forgiveness
techniques from Maddy Thorson's Celeste articles — variable jump height,
coyote time, and jump buffering. There are still no colliders; the ground
is a flat line at y=164. Chapter 5 makes it solid.

**New file:** `PlayerController.cs` (a `Component` implementing
`IUpdatable`). **Changed:** `ArenaScene.cs` attaches it to the archer.

## Concepts

### Velocity, not position

Beginners move characters by setting position directly
(`pos.X += speed * dt` when a key is held). It works and feels terrible:
full speed instantly, full stop instantly — a hockey puck on ice, or a
shopping cart with a stuck wheel. Real-feeling movement simulates a tiny
bit of physics:

- **Velocity** persists between frames. Input doesn't set it; input
  *accelerates toward* a target velocity.
- **Acceleration** (900 px/s² here) ramps speed up — the character has
  weight.
- **Friction** (1100 px/s²) ramps speed down when no key is held — the
  character stops *near* instantly but not *exactly* instantly, which reads
  as "responsive" rather than "slippery".

`Approach(value, target, maxDelta)` moves `value` toward `target` without
overshooting. It's the one-line heart of the horizontal feel.

### Gravity and terminal velocity

Each frame: `Velocity.Y += Gravity * dt`, clamped to `MaxFallSpeed`.
Jumping sets `Velocity.Y = -JumpSpeed` (negative Y is up in screen space —
Y grows downward). The numbers are in pixels and seconds, so you can
reason about them: 250 px/s up against 800 px/s² gravity gives a jump of
`v²/2g ≈ 39` pixels — about 2.4 tiles. Tune by feel, verify by math.

### The forgiveness trio

From Maddy Thorson's articles on Celeste's design ("Celeste and
forgiveness"):

1. **Variable jump height.** Holding jump = full jump; releasing early
   multiplies the upward velocity by 0.45. One button, analog expression.
   Detect the *release* with the previous frame's held state (`_jumpHeld`).
2. **Coyote time.** After walking off an edge, the game pretends you're
   grounded for 0.10 s. Humans press jump *after* the visual edge more
   often than you'd think; the game forgives it.
3. **Jump buffering.** A jump pressed up to 0.12 s *before* landing is
   remembered and fires on touchdown. Landing and jumping become one
   motion instead of two precisely-timed ones.

All three are timers and edge detection — no physics involved. They are
the cheapest, highest-impact game-feel work you will ever do.

### Explicit interface implementation

`void IUpdatable.Update()` — with the interface name as prefix and no
access modifier — implements `IUpdatable.Update` *without* adding a public
`Update` method to the class. Callers holding a `PlayerController` can't
see it; Nez, holding it as `IUpdatable`, can. It keeps component public
surfaces honest: `Velocity` is public because other systems will read it;
`Update` is an engine callback, not API. The Nez sample's `Caveman`
component uses the same pattern.

## Minimal snippets

Edge detection, the primitive everything here is built from:

```csharp
bool pressedNow = Input.IsKeyDown(Keys.Z);
if (_wasDown && !pressedNow) { /* released this frame */ }
_wasDown = pressedNow;
```

(Nez also offers `Input.IsKeyPressed`/`IsKeyReleased` for the common cases —
we use `IsKeyPressed` for buffering and hand-rolled edge detection for the
variable jump, since we need the *previous* held state.)

The forgiveness pattern in miniature:

```csharp
if (Input.IsKeyPressed(Keys.Z)) _bufferTimer = JumpBuffer;
else _bufferTimer -= dt;
// ... later, when grounded:
if (_bufferTimer > 0f) { Jump(); _bufferTimer = 0f; }
```

## Exercises

### 1. Tune it until it feels right

Play with `MoveSpeed`, `JumpSpeed`, `Gravity`, `CoyoteTime`,
`JumpBuffer`. Find numbers *you* like and write them in your notes with
one line each on why.

- *Nudge:* change one number at a time.
- *Bigger hint:* jump height ≈ `JumpSpeed² / (2 * Gravity)` pixels; airtime
  ≈ `2 * JumpSpeed / Gravity` seconds. Compute before you play.
- *Near-solution:* there is no correct answer — but "I changed Gravity to
  1000 because jumps felt floaty" is the entire skill this book teaches.

### 2. Double jump

Allow one extra jump mid-air. Reset the count on landing.

- *Nudge:* an `int _jumpsLeft`, set to 1 (or 2) when grounded.
- *Bigger hint:* the air jump fires when `IsKeyPressed` and `_jumpsLeft > 0`
  and *not* grounded — decrement on use.
- *Near-solution:* be careful the ground jump doesn't consume the air jump:
  handle the grounded case first with the buffer/coyote logic, `else if`
  for the air jump.

### 3. Weaker air control

Real platformers often give less control mid-air. Multiply `Acceleration`
by 0.6 when airborne.

- *Nudge:* you already branch on `grounded`.
- *Bigger hint:* `var rate = inputX != 0f ? Acceleration * (grounded ? 1f : 0.6f) : Friction;`
- *Near-solution:* friction in air is debatable — many games keep air
  friction at zero so jumps preserve momentum. Try both; note the feel.

### 4. Make coyote jumps visible

`Debug.Log` a message whenever a jump fires *because of* coyote time
(buffered press, but not grounded when pressed).

- *Nudge:* you need last frame's grounded state in a field.
- *Bigger hint:* `if (_bufferTimer > 0f && _coyoteTimer > 0f)` fires in both
  cases; add `&& !_wasGrounded` to isolate the coyote case.
- *Near-solution:* you'll be surprised how often it fires — that's the
  point of the article.

### 5. Break it: no jump cut

Set `JumpCutMultiplier = 1f`. Play for a minute. Describe the difference
in one sentence.

- *Nudge:* every jump is now full height.
- *Bigger hint:* tap Z rapidly vs hold it — before the change these
  differed; now they don't.
- *Near-solution:* "Without the cut, short hops are impossible and the
  jump feels binary." Variable height is *the* precision tool.

### 6. Jump apex hang

Reduce gravity while the upward velocity is small (near the jump's apex)
for a floatier, more controllable arc — a Celeste staple.

- *Nudge:* `var gravity = Math.Abs(Velocity.Y) < 30f ? Gravity * 0.5f : Gravity;`
- *Bigger hint:* apply it in the `Velocity.Y += gravity * dt` line.
- *Near-solution:* tune the threshold and multiplier; too much hang feels
  like moon gravity.

### 7. Stay on the stage

Clamp the archer's X to the 320-wide stage so it can't leave the screen.

- *Nudge:* same `MathHelper.Clamp` from Chapter 2, but the bounds are
  design pixels now.
- *Bigger hint:* clamp after integrating position; account for the 8px
  half-width.
- *Near-solution:* `Entity.Position = new Vector2(MathHelper.Clamp(
  Entity.Position.X, 8f, 312f), Entity.Position.Y);` — needs
  `using Microsoft.Xna.Framework;` for `MathHelper` (already imported).

## Checkpoint

Run: `dotnet run`.

You should see: the archer standing on the invisible ground line (feet at
y=164, center y=156). Left/right runs with weight; Z jumps — hold for
full height, tap for a hop. Run off... nothing — the ground is infinite
for now. Press Z slightly before landing: the jump still fires (buffer).
The feel should already be *close* to a real platformer.

Done means: exercises 1–3 complete; 4–7 attempted. Your tuning numbers are
written down — Chapter 5's collisions will change the feel slightly, and
you'll want the baseline.

## Next

Chapter 5 replaces the imaginary ground line with real colliders: Nez's
`Mover`, the spatial hash, and platforms you can stand on and bonk under.
