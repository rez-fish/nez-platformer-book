# Chapter 6: The full moveset — wall slide, wall jump, dash

> Build status: **[unverified]** — written against Nez @ `3f8cc40`, not compiled.

## Where we are

The archer runs and jumps on platforms. This chapter completes the
Celeste-style moveset: wall slide, wall jump, and an 8-way dash, organized
by the smallest state machine that can hold them. Two walls were added to
the test room so there's something to slide on.

**Changed:** `PlayerController.cs` (states, walls, dash),
`ArenaScene.cs` (two wall platforms).

## Concepts

### State machines, minimal

A character controller is a set of *modes* with different rules. The
honest way to write that is a state variable plus one branch per state:

```csharp
enum MoveState { Normal, Dashing }
```

`Normal` runs the Chapter 5 logic; `Dashing` runs a fixed-velocity move
with no gravity and no input, then returns. The early `return` after the
dash branch is load-bearing: it guarantees the two states never
half-execute in the same frame. When the moveset grows (Chapter 12's
enemies use the same pattern), the shape scales: one enum, one branch
each, transitions only at the edges.

Timers drive the transitions: `_stateTimer` counts the dash down,
`_dashCooldown` gates the next one. Landing resets the cooldown —
Celeste's rule that makes dashes a renewable resource rather than a
panic button.

### Wall slide: three conditions

Wall sliding isn't "touching a wall" — it's touching a wall *while
airborne, pressing into it, and falling*. Each condition matters:

- **Touching**: the X-axis `Move` returned true this frame. We capture the
  direction with `Math.Sign(xDelta)` — the sign of the *attempted* motion,
  which is the direction of the wall.
- **Airborne**: `_grounded` from the *previous* frame's Y move (order
  matters; X resolves before Y here).
- **Pressing in**: `inputX == _wallDir`. Let go and you fall normally —
  that's what makes wall *jumps* aimable.

While sliding, fall speed clamps to 45 px/s. It reads as friction against
the wall, and it gives the player time to aim the jump.

### Wall jump

A wall jump is a velocity assignment, not physics: push away
(`-_wallDir * WallJumpX`) and up (`-WallJumpY`), flip `_facing`. It takes
priority over the buffered ground jump — the `if (wallSliding)` branch
runs first. Note what it *doesn't* do: no coyote time, no buffering.
Those get added the same way the ground ones did, which is Exercise 3.

### Dash

Eight-way from the current input (or facing if neutral), normalized so
diagonals aren't faster — the Chapter 2 lesson, reused. During the dash:
no gravity, no steering, collisions still resolve (dashing into a wall
stops you; dashing into the floor doesn't stick you to it because
`_grounded` stays false). On exit, velocity keeps 35% — cutting to zero
feels like hitting a wall; keeping 100% makes every dash a launch.

### C# for the TS/Go engineer, part 4

- **`enum`**: a named set of integer constants. `MoveState.Dashing` reads
  better than `1` and the compiler stops you assigning nonsense.
- **`Math.Sign(float)`** returns -1, 0, or 1 as an `int`.
- **Early return as structure**: the dash branch returns before the normal
  logic. Two flat blocks beat one nested `if/else` pyramid — especially as
  states multiply.

## Minimal snippets

The state-machine shape:

```csharp
enum MoveState { Normal, Dashing }

void IUpdatable.Update()
{
    if (_state == MoveState.Dashing) { UpdateDash(dt); return; }
    // ... normal logic ...
}
```

Wall direction from a blocked horizontal move:

```csharp
var hitWall = xDelta != 0f && _mover.Move(new Vector2(xDelta, 0f), out _);
_wallDir = hitWall && !_grounded ? Math.Sign(xDelta) : 0;
```

## Exercises

### 1. Tune the dash

Play with `DashSpeed`, `DashTime`, `DashCooldown`. How far does a dash
travel (speed × time)? Compare that distance to your jump distance.

- *Nudge:* 230 × 0.15 ≈ 34px — about two tiles.
- *Bigger hint:* Celeste's dash is roughly 3× the jump height in distance.
  Ours is shorter; decide if that's right for an arena game.
- *Near-solution:* write the numbers down. Chapter 8's dash particles will
  be timed to `DashTime` — you'll need it.

### 2. Horizontal-only dash

Remove the Y input from the dash direction (dash stays level). Play both
versions and pick one for the game, with a one-line justification.

- *Nudge:* `var dir = new Vector2(inputX, 0f);`
- *Bigger hint:* 8-way dashes trivialize vertical level design; horizontal
  dashes keep pits meaningful.
- *Near-solution:* TowerFall's dash is effectively horizontal — but our
  arenas are single-screen, so 8-way may be fine. Your call; note it.

### 3. Wall-jump forgiveness

Give wall jumps the coyote treatment: after leaving a wall slide, allow
the wall jump for 0.08 s.

- *Nudge:* a `_wallCoyoteTimer`, refreshed while `wallSliding`.
- *Bigger hint:* the wall-jump branch becomes `if (wallSliding ||
  _wallCoyoteTimer > 0f)` — but you need the *last* wall direction, so
  don't zero `_wallDir` on jump until the timer expires.
- *Near-solution:* same shape as Chapter 4's ground coyote. Forgiveness
  composes.

### 4. Face the movement

Flip the sprite to face `_facing`. (The exact API is yours to discover —
inspect `SpriteRenderer`'s members in the Nez source.)

- *Nudge:* renderers that draw sprites usually expose a flip or an effect.
- *Bigger hint:* cache the `SpriteRenderer` in `OnAddedToEntity` alongside
  the `Mover`.
- *Near-solution:* set it when `_facing` changes, not every frame — same
  result, less noise. [unverified: member name — confirm in source.]

### 5. Break it: full momentum after dash

Delete the `Velocity *= 0.35f` line. Dash repeatedly and describe the feel.

- *Nudge:* every dash is now a 230 px/s launch in any direction.
- *Bigger hint:* try dashing upward repeatedly — you can fly.
- *Near-solution:* "Keeping full momentum turns the dash into flight."
  The 0.35 keeps the dash punchy without breaking gravity's authority.

### 6. Design: dash refresh pickups

In your notes, sketch a pickup that grants an extra mid-air dash
(Celeste's mechanic). What state does the player need? What does the
pickup entity need?

- *Nudge:* `_dashesLeft` instead of a cooldown; pickups set it to 2.
- *Bigger hint:* the pickup needs a trigger collider and a respawn timer —
  Chapter 11 builds exactly this for arrows.
- *Near-solution:* no code yet — but notice the design is *state +
  triggers*, the two tools you now own.

### 7. Log the transitions

`Debug.Log` each entry into `Dashing` and each wall jump. Play for a
minute and check the log matches what you felt.

- *Nudge:* log at the transition points, not every frame.
- *Bigger hint:* include the direction: `Debug.Log("dash {0}", dir);`
- *Near-solution:* state machines are debugged by their transitions, not
  their states — this habit pays off in Chapter 12's enemy AI.

## Checkpoint

Run: `dotnet run`.

You should see: the test room with two walls. Run at a wall while
airborne and hold toward it — the archer slides down slowly. Z kicks away
from the wall. X dashes in 8 directions (or your Exercise 2 choice);
landing refreshes it. The moveset should feel complete: run, jump, slide,
wall-jump, dash, all chained.

Done means: exercises 1–2 complete (with a decision recorded); 3–7
attempted. The moveset is frozen after this chapter — later chapters add
*context* (levels, enemies), not moves.

## Next

Chapter 7 replaces the hand-placed platforms with a real tilemap level
built in the Tiled editor.
