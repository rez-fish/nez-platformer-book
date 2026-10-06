---
description: "Core, Scene, Entity, Component, Transform, and Camera — the Nez members you'll use constantly."
---

# Core: game, scene, entity, component

The ECS heart of Nez. Everything in the book hangs off these classes.

## `Nez.Core`

Your game class extends this (it extends XNA's `Game`). Mostly you touch
its statics.

| Member | Signature | ★ | Notes |
|---|---|---|---|
| `Scene` | `public static Scene Scene { get; set; }` | ★★★ | The active scene. First assignment wires it immediately; later assignments queue for end of frame. Never null (guarded). |
| `StartCoroutine` | `public static ICoroutine StartCoroutine(IEnumerator enumerator)` | | The Nez idiom for timed sequences. `yield return Coroutine.WaitForSeconds(t)`. |
| `Schedule` | `public static ITimer Schedule(float seconds, Action<ITimer> onTime)` | | One-shot (or repeating) timer without a coroutine. Returns `ITimer` (has `Stop()`). |
| `StartSceneTransition` | `public static T StartSceneTransition<T>(T transition)` | | Animated scene changes (fade, wipe). The book uses direct assignment; this is the upgrade. |
| `DebugRenderEnabled` | `public static bool DebugRenderEnabled` | ★★ | Draws colliders as red outlines. Leave on while tuning collisions. |
| `ExitOnEscapeKeypress` | `public static bool ExitOnEscapeKeypress = true` | | Esc quits. Turn off for shipped games with their own quit flow. |
| `PauseOnFocusLost` | `public static bool PauseOnFocusLost = true` | | Auto-pause when the window loses focus. |
| `Emitter` | `public static Emitter<CoreEvents> Emitter` | | Global event bus (`CoreEvents.SceneChanged`, etc.). |
| `Exit()` | `public void Exit()` | | Quit the game. Inherited from XNA `Game`. |

## `Nez.Scene`

A level, menu, or any self-contained game state. Override `Initialize()`
to build content; the scene drives its entities every frame.

| Member | Signature | ★ | Notes |
|---|---|---|---|
| `CreateEntity` | `Entity CreateEntity(string name, Vector2 position)` | ★★★ | Constructs **and registers** the entity. Always prefer this over `new Entity()`. |
| `FindEntity` | `Entity FindEntity(string name)` | ★★★ | Name lookup. The book's pragmatic seam between systems (camera rig, etc.). |
| `SetDesignResolution` | `void SetDesignResolution(int w, int h, SceneResolutionPolicy policy)` | ★★ | The pixel-perfect contract. Call once in `Initialize`. |
| `FindEntitiesWithTag` | `List<Entity> FindEntitiesWithTag(int tag)` | | Bulk lookup by `Entity.Tag`. Cheaper than name search for groups. |
| `Initialize()` | `public virtual void Initialize()` | ★★★ | **Override this.** Build the scene here — runs after construction, before the first frame. |
| `OnStart()` / `Begin()` / `End()` | `public virtual void ...` | | Finer lifecycle: `Begin` when the scene becomes active, `End` when it leaves. |
| `Camera` | `public Camera Camera` | ★★ | The scene's camera. `Camera.Position` is the world point at screen center. |
| `ClearColor` | `public Color ClearColor` | ★★ | What the empty scene clears to. A field, not a property. |
| `LetterboxColor` | `public Color LetterboxColor` | | The bars around a pixel-perfect stage. |
| `Content` | `public NezContentManager Content` | ★★ | Scene-scoped content. Unloaded when the scene ends (unlike `Core`-level content). |
| `DestroyAllEntities` | `void DestroyAllEntities()` | | Tear down everything — useful for instant rematch flows. |

```csharp
public override void Initialize()
{
    SetDesignResolution(320, 180, SceneResolutionPolicy.ShowAllPixelPerfect);
    ClearColor = new Color(20, 12, 28);

    var archer = CreateEntity("archer", new Vector2(40, 100));
    archer.AddComponent(new PlayerController());

    // camera follows from frame one
    Camera.Position = archer.Position;
}
```

## `Nez.Entity`

A thing in the scene: a `Position` and a bag of components. Does almost
nothing itself.

| Member | Signature | ★ | Notes |
|---|---|---|---|
| `AddComponent` | `T AddComponent(T component)` | ★★★ | The most-called method in Nez. Returns the component for chaining. |
| `GetComponent` | `T GetComponent<T>()` | ★★★ | Sibling lookup. Cache in `OnAddedToEntity`, never per-frame. |
| `Position` | `public Vector2 Position` | ★★★ | Convenience over `Transform.Position`. Remember: it's a struct copy on read. |
| `Destroy()` | `void Destroy()` | ★★ | Removes at end of frame — safe to call mid-update. |
| `SetTag` / `Tag` | `Entity SetTag(int tag)` / `public int Tag` | | Integer tag for `FindEntitiesWithTag`. |
| `Clone` | `Entity Clone(Vector2 position = default)` | | Prototype pattern: stamp copies of a template entity. |
| `SetEnabled` | `Entity SetEnabled(bool isEnabled)` | | Disables the entity and its components. |

## `Nez.Component` and `Nez.IUpdatable`

| Member | Signature | ★ | Notes |
|---|---|---|---|
| `OnAddedToEntity()` | `public virtual void OnAddedToEntity()` | ★★★ | **Wire to neighbors here** (`GetComponent`), never in the constructor — `Entity` is null there. |
| `OnRemovedFromEntity()` | `public virtual void OnRemovedFromEntity()` | | Deregister virtual input, unsubscribe events. |
| `IUpdatable.Update()` | `void IUpdatable.Update()` | ★★★ | Per-frame callback — **only runs if the component implements `IUpdatable`**. |
| `Enabled` | `public bool Enabled { get; set; }` | | Toggling off stops `Update` (for `IUpdatable`s) and rendering. |
| `SetUpdateOrder` | `Component SetUpdateOrder(int order)` | | Controls component update sequence within an entity. |
| `Entity` | `public Entity Entity` | ★★★ | The owning entity. Null in the constructor. |

```csharp
public class PlayerController : Component, IUpdatable
{
    Mover _mover;

    public override void OnAddedToEntity()
    {
        // Entity is valid here — cache neighbors once, not per frame.
        _mover = Entity.GetComponent<Mover>();
    }

    void IUpdatable.Update()
    {
        // per-frame logic; Entity.Position, Entity.Scene, Entity.Tag available
    }

    public override void OnRemovedFromEntity()
    {
        // deregister virtual input, unsubscribe events
    }
}
```

## `Nez.Transform`

Every entity has one (`Entity.Transform`). `Entity.Position` is the
common shortcut.

| Member | ★ | Notes |
|---|---|---|
| `Position` / `LocalPosition` | ★★★ | World vs. parent-relative. Most games only ever touch `Position`. |
| `Scale` / `Rotation` (+ `...Degrees` variants) | ★ | Juice and aiming. Degrees variants save you the radian math. |
| `RoundPosition()` | | Snap to whole pixels — useful for crisp pixel-art movement. |
| `LookAt(Vector2)` | | Point the transform at a world position (turrets, aim indicators). |

## `Nez.Camera`

`Scene.Camera`. `Position` is the world coordinate at the **center** of
the screen.

| Member | Signature | ★ | Notes |
|---|---|---|---|
| `Position` | `public Vector2 Position` | ★★★ | Set this to follow things. Smooth it yourself (the book's exponential lerp). |
| `Zoom` | `public float Zoom` | ★ | Versus zoom-to-fit. Respect `MinimumZoom`/`MaximumZoom`. |
| `Bounds` | `public RectangleF Bounds` | | The world-space rect currently visible. Handy for culling and clamps. |
| `WorldToScreenPoint` / `ScreenToWorldPoint` | `Vector2 ...(Vector2)` | ★ | Mouse picking, HUD anchoring. |
| `MouseToWorldPoint()` | `Vector2 MouseToWorldPoint()` | | Aim-at-cursor in one call. |

```csharp
// smooth follow: snap on spawn, lerp every frame after
Camera.Position = Vector2.Lerp(Camera.Position, target.Position, 1f - MathF.Pow(0.001f, Time.DeltaTime));

// zoom-to-fit a versus arena, clamped to sane limits
Camera.SetMinimumZoom(1f);
Camera.SetMaximumZoom(3f);
Camera.SetZoom(MathHelper.Clamp(180f / arenaWidth, 1f, 3f));

// keep the camera inside the level
var p = Camera.Position;
var b = Camera.Bounds;
p.X = MathHelper.Clamp(p.X, b.Width / 2, levelWidth - b.Width / 2);
Camera.Position = p;
```
