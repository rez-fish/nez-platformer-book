---
description: Arrows, target dummies, scoring, and the HUD — the combat loop.
---

# Chapter 11: Combat — arrows, targets, score

> Build status: **[unverified]** — written against Nez @ `3f8cc40`, not compiled.

## Where we are

Movement, levels, camera, flow, sound — but nothing to *do*. This chapter
adds the verb: **C fires an arrow** in the facing direction; hitting a
target dummy scores, plays a hit sound, shakes the camera, and respawns
the dummy somewhere new. The combat loop is complete in single-player;
versus comes in Chapter 13.

**New files:** `Arrow.cs`, `Target.cs`. **Changed:** `Archer.cs` (arrow +
target placeholder textures), `PlayerController.cs` (C to shoot),
`ArenaScene.cs` (spawning, scoring, HUD, `TeardownUi`),
`PauseController.cs` (tears down arena UI on quit-to-title), `Sfx.cs`
(shoot/hit), new wavs.

## Concepts

### Projectiles as entities

An arrow is an entity with a `Mover` and a 2-frame update: move along
`Direction * Speed * dt`; on any collision, check *what* was hit and die.
`Entity.Destroy()` removes it at the end of the frame — safe to call
mid-update. Lifetime caps it at 1.5 s so missed arrows don't fly forever.
No gravity, no steering: the simplest projectile, and the template for
every enemy shot in Chapter 12.

### Collision as identification

The arrow doesn't know about targets until the moment of impact:
`hit.Collider.Entity.GetComponent<Target>()` — null means "not a target."
This is the standard Nez pattern for hit resolution: collide broadly,
identify narrowly. It keeps the arrow decoupled from everything it might
hit (tiles, targets, later: players, enemies).

### Spawning discipline

`ArenaScene.SpawnArrow` is the single choke point for arrow creation —
position, sprite, components in one place. When Chapter 13 needs
per-player arrows (different speeds, ownership), the change lands here,
not scattered across call sites. Same for `SpawnTarget`: one method, one
set of components.

### Score as scene state

`ArenaScene.Score` is a public field; `OnTargetHit` increments it, updates
the Gum HUD text (`$"SCORE {Score}"` — string interpolation), plays the
hit, shakes, and respawns the dummy at a different spot. The HUD text is
another Gum element with scene-scoped lifetime: `TeardownUi()` removes it,
and the pause controller calls it before quitting to title. (Forgetting
this is Chapter 9's orphan bug, now with score text.)

### C# for the TS/Go engineer, part 7

- **`do…while`**: runs the body *at least once*, then checks. The respawn
  picks a random spot and re-rolls only if it picked the same one — the
  check must run after the first pick, so `do…while` fits exactly.
- **Constructor arguments on components**: `new Arrow(direction)` —
  components are just classes; constructors with parameters are fine.
  (`OnAddedToEntity` still does the entity wiring — construction is for
  *data*, `OnAddedToEntity` for *neighbors*.)

## Minimal snippets

The projectile update in full — it's short enough to show the shape:

```csharp
void IUpdatable.Update()
{
    _life -= Time.DeltaTime;
    if (_life <= 0f) { Entity.Destroy(); return; }

    if (_mover.Move(Direction * Speed * Time.DeltaTime, out var hit))
    {
        var target = hit.Collider.Entity.GetComponent<Target>();
        if (target != null) ((ArenaScene)Entity.Scene).OnTargetHit(target);
        Entity.Destroy();
    }
}
```

Spawning with an offset so the arrow doesn't hit its archer:

```csharp
scene.SpawnArrow(Entity.Position + new Vector2(_facing * 14f, 0f),
                 new Vector2(_facing, 0f));
```

## Exercises

### 1. Arrow tuning

Change `Speed`, `Lifetime`, and the spawn offset. Find the fastest arrow
that still reliably hits the dummies (too fast + thin colliders = tunneling
— the Mover sweeps, but 1-frame moves have limits).

- *Nudge:* all three are constants/arguments.
- *Bigger hint:* tunneling shows as arrows passing *through* dummies at
  high speed — the sweep covers the frame's motion, nothing more.
- *Near-solution:* note the max reliable speed; Chapter 13's balance
  depends on it.

### 2. Fire rate

Add a cooldown so C can't be machine-gunned (or *allow* machine-gunning —
decide, with a one-line justification).

- *Nudge:* a `_shootCooldown` field, same shape as `_dashCooldown`.
- *Bigger hint:* TowerFall limits arrows *in flight* (one at a time until
  it lands) rather than by time — consider which feels better here.
- *Near-solution:* implement one, note the other. Versus balance lives in
  these numbers.

### 3. Arrows stick in walls

Instead of vanishing on tile impact, stick the arrow into the wall for 2 s
(stop moving, keep rendering), then fade.

- *Nudge:* on tile hit (no target), don't `Destroy()` — set a `_stuck`
  flag and skip movement.
- *Bigger hint:* you need the *collision point* — `CollisionResult.Point`
  (verified in source) or just the arrow's current position.
- *Near-solution:* TowerFall's stuck arrows are pickups — that's Exercise 4.

### 4. Pick up stuck arrows (design + code)

Combine with Exercise 3: walking over a stuck arrow picks it up (restore
ammo if you implemented Exercise 2's limit).

- *Nudge:* the stuck arrow needs a trigger check against the player —
  `Physics.OverlapRectangle` or a trigger collider + `OnTriggerEnter`.
- *Bigger hint:* `Collider.IsTrigger = true` colliders report overlaps
  without blocking — check Nez's trigger docs/source for the callback name.
- *Near-solution:* this is the full TowerFall loop: shoot → miss → retrieve.

### 5. Break it: zero spawn offset

Spawn the arrow at the archer's exact position. Fire.

- *Nudge:* the arrow's Mover immediately collides with the archer's own
  collider.
- *Bigger hint:* every shot dies instantly — possibly *scoring* if a
  dummy overlaps the player (it doesn't, but notice the shape of the bug).
- *Near-solution:* every shot dies instantly on your own collider — which is
  why the spawn offset exists. Spawn points are collision hygiene, not
  decoration.

{% hint style="warning" %}
**Spawn offsets are collision hygiene:** Arrows spawn slightly ahead of the shooter — a zero offset means the arrow's `Mover` collides with the archer on the first frame. Versus mode (Chapter 13) needs the same care with *two* archers.
{% endhint %}

### 6. Moving targets

Make the dummy drift left-right between two x bounds (a tiny component
with `IUpdatable`).

- *Nudge:* `Entity.Position += new Vector2(dir * speed * dt, 0)` — but it
  has no Mover; it doesn't need one (nothing collides *with* it except
  arrows, which check on impact).
- *Bigger hint:* flip `dir` at the bounds; keep it inside the arena.
- *Near-solution:* leading a moving target is the first *skill* shot in
  the game.

### 7. HUD polish

Style the score text (Chapter 9's exercises applied here): better
position, a label that pops (scale punch) on score.

- *Nudge:* the pop is a 0.2 s scale tween on the Gum element — or fake it
  with the camera trauma you already have.
- *Bigger hint:* `OnTargetHit` is the event; the HUD is the reaction.
- *Near-solution:* juice for UI, same philosophy as Chapter 8.

## Checkpoint

Run: `dotnet run`, Z past the title, C to shoot.

You should see: "SCORE 0" top-left; arrows flying straight, sticking
nowhere yet — they vanish on impact; hitting the dummy: hit sound,
camera kick, score increments, dummy reappears elsewhere. Missed arrows
die after 1.5 s. The loop — aim, fire, hit, score — is playable.

Done means: exercises 1–2 complete (with decisions recorded); 3–7
attempted.

## Next

Chapter 12 adds opposition: patrolling enemies with line-of-sight chase,
and what happens when they touch you.
