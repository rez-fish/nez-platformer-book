---
description: "Mover, colliders, and the Physics query API — Nez's kinematic collision system."
---

# Physics: movers, colliders, queries

Nez's physics is **kinematic and query-based**: you move things with a
`Mover` (which sweeps and stops at contact), and you ask the spatial hash
direct questions (`Physics.Linecast`, `OverlapRectangle`). There is no
rigid-body simulation here — for that, Nez bundles Farseer separately
(see the `RigidBodies` sample).

## `Nez.Mover`

The component that moves colliders without tunneling. Add it alongside a
collider; call `Move` (or the split pair) every frame instead of setting
`Position` directly.

| Member | Signature | ★ | Notes |
|---|---|---|---|
| `Move` | `bool Move(Vector2 motion, out CollisionResult result)` | ★★★ | Sweeps along `motion`, stops at first contact. Returns whether anything was hit. **Move X and Y as separate calls** — that's what makes corners slide. |
| `CalculateMovement` | `bool CalculateMovement(ref Vector2 motion, out CollisionResult result)` | ★ | Computes the swept movement *without applying it*. Inspect `result.Collider` to decide (one-way platforms, filters), then… |
| `ApplyMovement` | `void ApplyMovement(Vector2 motion)` | ★ | …apply the (possibly modified) motion. The split pair exists for exactly the decide-in-between pattern. |

Related movers: `TiledMapMover` (collides against a tilemap layer — *is* a
`Mover`, so `GetComponent<Mover>()` keeps working), `ProjectileMover`
(straight-line projectiles), `ArcadeRigidbody` (velocity-based alternative).

```csharp
// the standard pattern: axis-separated moves, corners slide
var motion = velocity * Time.DeltaTime;
_mover.Move(new Vector2(motion.X, 0), out _);
_mover.Move(new Vector2(0, motion.Y), out var hit);

// the split pair: decide in between (one-way platforms)
var m = velocity * Time.DeltaTime;
if (_mover.CalculateMovement(ref m, out var result) && ShouldIgnore(result))
    m = Vector2.Zero;   // or: keep X, drop Y — your call
_mover.ApplyMovement(m);
if (result.Collider != null)
    OnLanded(result.Normal);   // Normal tells you floor vs. wall
```

## Colliders

A collider is a shape component — data the physics system queries.
Nothing visible or physical by itself (turn on `Core.DebugRenderEnabled`
to see them).

| Class | Constructor shapes | ★ | Notes |
|---|---|---|---|
| `BoxCollider` | `new BoxCollider(w, h)` centered · `new BoxCollider(x, y, w, h)` offset | ★★★ | The workhorse. Offset form places top-left at the entity position. |
| `CircleCollider` | `new CircleCollider(radius)` | ★ | Cheap overlap tests, pickups, explosions. |
| `PolygonCollider` | `new PolygonCollider(Vector2[] points)` | | Slopes and odd shapes. Must be convex. |

Every collider registers in the **spatial hash** automatically. One tuning
knob exists if you ever need it (hundreds of colliders): the cell size —
set before creating the scene. `Collider.IsTrigger = true` makes a
collider report overlaps without blocking (pickups, zones).

## `Nez.Physics` — the query API

Static-style queries against the spatial hash. The public surface for
"what's near this point/line/box?"

| Member | Signature | ★ | Notes |
|---|---|---|---|
| `Linecast` | `RaycastHit Linecast(Vector2 start, Vector2 end, int layerMask = AllLayers)` | ★ | "Is the line of sight clear?" `hit.Collider == null` → nothing in the way. The AI chapter's sensing primitive. |
| `OverlapRectangle` | `Collider OverlapRectangle(RectangleF rect, int layerMask = AllLayers)` | | "What's inside this box?" Pickups, touch kills, area triggers. |
| `OverlapCircle` | `Collider OverlapCircle(Vector2 center, float radius, int layerMask = AllLayers)` | | Radial version — explosions, aggro radii. |
| `LinecastAll` / `OverlapRectangleAll` | `int ...(..., T[] hits, ...)` | | Fill an array with *all* hits. Reuse the array — no per-frame allocation. |

`RaycastHit` carries `Collider`, `Point`, `Normal`, `Distance`,
`Fraction`. `CollisionResult` (from `Mover.Move`) carries `Collider`,
`Normal`, `MinimumTranslationVector`, `Point` — the `Normal` is what
chapter 6 reads for wall detection.

```csharp
// line of sight: "can the enemy see the player?"
var hit = Physics.Linecast(eyePosition, player.Position);
bool canSee = hit.Collider == null || hit.Collider.Entity == playerEntity;

// pickup query: "did I touch anything edible?"
var box = new RectangleF(Entity.Position - new Vector2(8), new Vector2(16, 16));
var other = Physics.OverlapRectangle(box);
if (other != null && other.Entity.GetComponent<Pickup>() != null)
    other.Entity.Destroy();

// explosion: everyone in radius takes the query, decides for itself
var hits = new Collider[16];
int count = Physics.OverlapCircleAll(Entity.Position, 48f, hits);
for (int i = 0; i < count; i++)
    hits[i].Entity.GetComponent<Health>()?.Damage(1);
```

{% hint style="info" %}
**Collide broadly, identify narrowly.** The standard Nez hit-resolution
pattern: query or sweep against *everything*, then
`hit.Collider.Entity.GetComponent<Target>()` — null means "not a target."
It keeps projectiles decoupled from everything they might hit.
{% endhint %}
