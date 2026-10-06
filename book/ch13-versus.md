# Chapter 13: Local multiplayer — versus

> Build status: **[unverified]** — written against Nez @ `3f8cc40`, not compiled.

## Where we are

Everything so far was single-player. This chapter cashes the book's
cheque: **two archers, one arena, first to 5 hits wins** — Player 1 on
keyboard, Player 2 on gamepad. Enemies and targets are gone (the duel is
the game now); the single-player build survives in the ch12 solution.

**New files:** `PlayerInput.cs`. **Changed:** `PlayerController.cs`
(input abstraction, `PlayerIndex`), `Arrow.cs` (ownership, hits players),
`ArenaScene.cs` (versus flow, scoring, win), `CameraFollow.cs` (midpoint),
`Archer.cs` (per-player palette).

## Concepts

### Input abstraction

`PlayerController` no longer touches `Input` directly. Each player gets a
`PlayerInput` holding virtual inputs — `VirtualIntegerAxis` for movement,
`VirtualButton` for jump/dash/shoot — configured per device:

```csharp
// player 0: keyboard
MoveX.AddKeyboardKeys(OverlapBehavior.TakeNewer, Keys.Left, Keys.Right);
// player 1: gamepad 0's stick AND d-pad feed the same axis
MoveX.AddGamePadLeftStickX(0);
MoveX.AddGamePadDPadLeftRight(0);
```

One axis, many sources: keyboard keys, stick, d-pad all write the same
`Value`. The controller reads `_input.MoveX.Value` and doesn't know or
care what's behind it. `IsPressed` (edge) vs `IsDown` (held) preserve the
Chapter 4 semantics. Virtual inputs self-update every frame (verified in
`Input`'s source) — no update calls needed.

This is the same seam as the texture factory: the controller depends on
an *abstraction* (`PlayerInput`), and players differ only in how it's
constructed. Adding player 3–4, or remappable keys, touches one file.

### Ownership and friendly fire

Every arrow carries its shooter's index. On impact:

```csharp
var player = hit.Collider.Entity.GetComponent<PlayerController>();
if (player != null && player.PlayerIndex != _ownerIndex && player.IsVulnerable)
    arena.OnPlayerHit(_ownerIndex, player);
```

Three conditions, each load-bearing: it's a player, it's not *me*, and
it's not spawn-protected. Remove the middle one (Exercise 4) and the game
becomes unplayable — which is exactly why the check exists.

### The versus loop

`OnPlayerHit`: score++, HUD update, trauma, and — unless someone just won
— respawn the victim at their spawn with 1.5 s of grace. At 5: show the
winner text and `Time.TimeScale = 0` (the Chapter 9 pause trick, reused as
a game-over freeze). A nested `WinInput` component watches for Z and
returns to title. Score is `int[2]`; the HUD reads `P1 0 : 0 P2`.

### Shared-screen camera

One screen, two players: the camera frames their **midpoint**, with
lookahead from the *average* velocity, clamped to the level as before.
When both players cluster, it behaves like the old follow camera; when
they split, it compromises. (Zoom-to-fit is Exercise 6.)

### C# for the TS/Go engineer, part 9

- **`readonly` field initialization**: `public readonly VirtualIntegerAxis
  MoveX = new VirtualIntegerAxis();` — constructed inline, assigned once,
  never replaced. The *object* is mutable (you call `AddKeyboardKeys` on
  it); the *reference* isn't.
- **String interpolation, again**: `$"archer-{index}"`, `$"P1 {Scores[0]}"`.
  After this chapter you've seen it in UI, entity names, and logging —
  it's the default string tool.

## Minimal snippets

Per-player input, the complete pattern:

```csharp
var input = new PlayerInput(playerIndex); // 0 = keyboard, 1 = gamepad
var controller = archer.AddComponent(new PlayerController(input));
controller.PlayerIndex = playerIndex;
```

Owned projectile hits:

```csharp
if (player != null && player.PlayerIndex != _ownerIndex && player.IsVulnerable)
    arena.OnPlayerHit(_ownerIndex, player);
```

## Exercises

### 1. Both modes: a GameMode enum

Right now versus *replaced* single-player. Add `enum GameMode
{ SinglePlayer, Versus }` (a const or a static on `ArenaScene`) and make
`Initialize` branch: single-player spawns targets/enemies and one archer;
versus spawns two.

- *Nudge:* the ch12 solution is your reference implementation.
- *Bigger hint:* `CameraFollow` needs to handle one player again — branch
  on player count, or keep two code paths.
- *Near-solution:* the title screen picks the mode (Exercise: add a Gum
  menu — Chapter 9's exercises prepared you).

### 2. Player 2 keyboard fallback

No gamepad? Give player 2 a keyboard scheme (WASD + F/G/H?) when no pad
is connected.

- *Nudge:* detect the pad — `GamePad.GetState(0).IsConnected` (XNA).
- *Bigger hint:* build the `PlayerInput` conditionally in `ArenaScene`.
- *Near-solution:* two humans, one keyboard, zero excuses.

### 3. One arrow at a time

TowerFall's rule: you can't shoot while your arrow is still flying.
Track arrows per player.

- *Nudge:* `int[] _arrowsInFlight`; increment on shoot, decrement when the
  arrow dies.
- *Bigger hint:* the arrow needs to report its death — an event, or the
  arena scans for `"arrow"` entities (cheap at this scale).
- *Near-solution:* missing is now a real cost — it changes the whole
  risk calculus.

### 4. Break it: friendly fire

Delete the `player.PlayerIndex != _ownerIndex` check. Duel for two minutes.

- *Nudge:* your own arrows can kill you now.
- *Bigger hint:* notice how the *geometry* of play changes — no more
  shooting through your own position.
- *Near-solution:* revert. Ownership isn't politeness, it's design.

### 5. Win by 2 (design)

First-to-5 can end 5–4 on a lucky shot. Should versus require winning by
2? Decide in your notes, implement if yes.

- *Nudge:* the check is `Scores[i] >= WinningScore` — add the margin.
- *Bigger hint:* deuce rules need a cap or games stall — TowerFall doesn't
  bother; why?
- *Near-solution:* no wrong answer, but "it felt bad when…" is the right
  kind of evidence.

### 6. Zoom to fit

As the players separate, zoom the camera out (down to ~0.7×) so both stay
visible; zoom back in as they close.

- *Nudge:* `Camera.Zoom` (verified property). Distance between players →
  lerp zoom.
- *Bigger hint:* zoom changes the *effective* view size — your clamp math
  must use `viewWidth / zoom`.
- *Near-solution:* this is the last camera exercise; nail it and the
  versus feel is complete.

### 7. Instant rematch

On the win screen, R starts a new versus match immediately (new
`ArenaScene`), Z still goes to title.

- *Nudge:* `WinInput` already watches keys — add the branch.
- *Bigger hint:* `Time.TimeScale = 1f` before switching (frozen timescale
  persists across scenes — it's global).
- *Near-solution:* rematch speed is a versus essential — never make
  winners navigate a menu to play again.

## Checkpoint

Run: `dotnet run`, Z past title. Player 1: arrows/ZXC. Player 2: gamepad
(left stick or d-pad, A jump, X dash, B shoot).

You should see: two archers (orange vs teal hoods), "P1 0 : 0 P2"
top-left, camera framing both. Arrows fly; hitting your rival scores,
kills, respawns them. At 5: "PLAYER N WINS", frozen; Z → title. Play a
real match — the book's thesis in one screen.

Done means: exercises 1–2 complete (or a note saying why not — hardware);
3–7 attempted.

## Next

Chapter 14: ship it — publish, package the FNA natives, and hand the game
to someone else's computer.
