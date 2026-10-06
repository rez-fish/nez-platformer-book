# Chapter 12: Enemies — AI with state machines and line of sight

> Build status: **[unverified]** — written against Nez @ `3f8cc40`, not compiled.

## Where we are

Targets don't fight back. This chapter adds opposition: blob enemies that
patrol platforms, chase on sight, hop over obstacles — and kill on touch.
Arrows kill them back (worth 2 points). Death respawns you at the start
with brief spawn protection. The single-player game is now complete.

**New files:** `Enemy.cs`. **Changed:** `Archer.cs` (enemy texture),
`PlayerController.cs` (spawn protection), `Arrow.cs` (enemies are
hittable), `ArenaScene.cs` (spawns, death/respawn), `Sfx.cs` + `death.wav`.

## Concepts

### AI as sense → think → act

The enemy's `Update` is three labeled phases — a structure worth stealing
for every AI you write:

1. **Sense**: where's the player? `Physics.Linecast(eyes, playerPos)`
   answers "is the line of sight clear?" — `hit.Collider == null` means
   nothing in the way; `hit.Collider.Entity == _player` means the first
   thing hit *is* the player. (Verified pattern from Nez's FAQ.) Gated by
   `AggroRange` so we don't raycast across the map for no reason.
2. **Think**: `seen ? Chase : Patrol` — the same enum-state-machine shape
   as the player's dash (Chapter 6), now driven by perception instead of
   input.
3. **Act**: patrol speed or chase speed toward the player, gravity,
   axis-separated `Mover` movement (same as the player — movers aren't
   player-specific). Patrolling into a wall flips direction; chasing into
   a wall hops.

### Touch kills, arrows kill back

Player–enemy contact is a distance check (`dist < 14f`) — no trigger
colliders, no callbacks, no API surface to verify. It calls
`ArenaScene.OnPlayerDeath`: teleport to spawn, zero velocity, 1.5 s of
invulnerability, death sound, big trauma. The reverse direction reuses
Chapter 11's identification pattern: the arrow's `GetComponent<Enemy>()`
check now sits next to `GetComponent<Target>()`, and `OnEnemyKilled`
scores 2, shakes, and destroys.

### Spawn protection

`PlayerController.IsVulnerable` (an expression-bodied property —
`=> _invulnTimer <= 0f`) gates the touch kill. Respawning *into* an enemy
with no protection is a death loop; 1.5 s of grace is the standard fix.
(Blinking the sprite during grace is Exercise 7.)

### C# for the TS/Go engineer, part 8

- **Expression-bodied members**: `public bool IsVulnerable =>
  _invulnTimer <= 0f;` — a property (or method) whose body is a single
  expression. Sugar for the one-liner; don't use it for real logic.
- **`readonly` constructor args**: `readonly float _minX` set in the
  constructor — the patrol bounds can't change after spawn, and the
  compiler enforces it.

## Minimal snippets

Line-of-sight sensing (the FAQ pattern):

```csharp
var hit = Physics.Linecast(eyes, playerPos);
bool seen = hit.Collider == null || hit.Collider.Entity == player;
```

The AI skeleton:

```csharp
enum AiState { Patrol, Chase }
// sense -> _state = seen ? AiState.Chase : AiState.Patrol;
// think -> speed/dir per state
// act   -> mover-based movement, same as the player
```

## Exercises

### 1. Tune the hunt

`PatrolSpeed`, `ChaseSpeed`, `AggroRange`, `HopSpeed`. Make one enemy
fast-but-blind (short range) and one slow-but-eagle-eyed.

- *Nudge:* constructor args could carry these per-enemy — promote the
  consts to parameters.
- *Bigger hint:* `new Enemy(minX, maxX, chaseSpeed: 90f)` — optional
  parameters with defaults keep call sites readable.
- *Near-solution:* two enemy *personalities* from one class is the payoff
  of data-driven AI.

### 2. A flyer

New enemy type: no gravity, sine-wave vertical drift, same patrol/chase
brain. 

- *Nudge:* subclass `Enemy`? Or a `Flyer : Component, IUpdatable` from
  scratch reusing the sense code?
- *Bigger hint:* composition beats inheritance here — extract the
  sensing into a helper both use.
- *Near-solution:* note which you chose and why. Chapter 13's versus
  mode won't use enemies, but your *next* game will.

### 3. Smarter hopping

The chase hop fires on *any* wall contact — including the arena border,
where hopping is pointless. Only hop when the wall is low enough to clear
(Exercise: how would you know?).

- *Nudge:* a short upward `Linecast` from the enemy's head.
- *Bigger hint:* if the head-ray is clear, the wall is hop-height.
- *Near-solution:* this is *planning* (one step) vs *reacting* (zero
  steps) — the gradient all AI climbs.

### 4. Break it: no line of sight

Replace the `seen` computation with `seen = dist < AggroRange`. Lure an
enemy to the far side of a wall.

- *Nudge:* it chases through the wall, hopping forever.
- *Bigger hint:* the linecast is what makes the AI feel *fair* — it can
  only want what it can see.
- *Near-solution:* revert. Sensing is what separates AI from homing
  missiles.

### 5. Stomp

Landing on an enemy (falling, feet above its head) kills *it*; any other
touch kills *you*.

- *Nudge:* in the touch check, `controller.Velocity.Y > 0` (falling) and
  player-above-enemy means stomp.
- *Bigger hint:* stomp → `OnEnemyKilled` + bounce the player
  (`Velocity.Y = -JumpSpeed * 0.7f`).
- *Near-solution:* the classic Mario rule, five lines. Notice how the
  existing event points (`OnEnemyKilled`, velocity) make it trivial.

### 6. Death penalty (design)

Right now death costs nothing but time. Should it cost score? Drop the
player's arrows? In your notes, pick a penalty (or none) with a one-line
justification.

- *Nudge:* TowerFall versus: death gives the *opponent* a point.
- *Bigger hint:* penalties shape behavior — score loss makes players
  cautious; no penalty makes them reckless (sometimes the goal!).
- *Near-solution:* Chapter 13 implements versus scoring — bring this
  decision.

### 7. Spawn blink

While invulnerable, blink the archer's sprite (toggle
`SpriteRenderer.Enabled` every 0.1 s).

- *Nudge:* cache the `SpriteRenderer` in `OnAddedToEntity`; drive the
  blink off `_invulnTimer` in `Update`.
- *Bigger hint:* when the timer expires, force `Enabled = true` — don't
  leave it blinking off.
- *Near-solution:* telegraphing state to the player, visually. Every
  hidden timer deserves a visible signal.

## Checkpoint

Run: `dotnet run`, play the arena.

You should see: two purple blobs patrolling their platforms, turning at
edges. Walk into one's sight line — it chases, hopping over bumps. Touch
one: death sound, shake, respawn at start. Shoot one: +2, it pops. The
game has stakes now.

Done means: exercises 1–2 complete; 3–7 attempted. Your AI tuning noted
next to movement tuning.

## Next

Chapter 13: the reason for all of this — local versus multiplayer, two
archers, one arena.
