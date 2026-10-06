---
description: "SpriteRenderer, SpriteAnimator, TiledMapRenderer, FollowCamera, and resolution policies."
---

# Rendering: sprites, camera, tilemaps

## `Nez.Sprites.SpriteRenderer`

Renders one texture (or sprite) centered on its entity. The base class of
`SpriteAnimator`.

| Member | ★ | Notes |
|---|---|---|
| `SpriteRenderer(Sprite)` / `SpriteRenderer(Texture2D)` | ★★★ | Give it art; it draws centered on the entity. |
| `FlipX` (bool field) | ★★ | Mirror horizontally — the standard facing-flip. |
| `Color` | ★★ | Tint multiplier. White = unmodified. Flash white/red for hit feedback. |
| `RenderLayer` | ★ | Draw order. Lower draws first. (Depth-sorting by Y sets this per frame.) |
| `Material` | | Blend state / sampler state overrides per renderable. |

## `Nez.Sprites.SpriteAnimator`

A `SpriteRenderer` that plays frame animations. Implements `IUpdatable`
itself — no update code needed from you.

| Member | Signature | ★ | Notes |
|---|---|---|---|
| `AddAnimation` | `AddAnimation(string name, Sprite[] sprites, float fps = 10)` | ★★ | Frames + speed. Also accepts a `SpriteAtlas` for packed sheets. |
| `Play` | `void Play(string name, LoopMode loopMode = LoopMode.Loop)` | ★★ | Starts (or restarts) a named animation. Guard with `IsAnimationActive` to avoid restarting every frame. |
| `IsAnimationActive` | `bool IsAnimationActive(string name)` | ★ | "Am I already playing this?" — the one-line animation state check. |
| `Pause()` / `UnPause()` / `Stop()` | | | Hitstop on the *sprite* without freezing the game. |
| `Speed` (float field, default 1) | | | Global playback multiplier. Slow-mo a single character. |
| `OnAnimationCompletedEvent` | `event Action<string>` | | Chain animations (attack → recover) without polling. |
| `CurrentFrame` / `CurrentAnimationName` | | | Read the playback state for animation-driven events (footstep on frame 2). |

`Sprite.SpritesFromAtlas(Texture2D, cellWidth, cellHeight)` slices a grid
spritesheet into `Sprite[]` — the standard atlas workflow.

```csharp
// setup: slice, register, play
var sprites = Sprite.SpritesFromAtlas(texture, 16, 16);
_animator.AddAnimation("Idle", new[] { sprites[0], sprites[1] }, 6f);
_animator.AddAnimation("Run", new[] { sprites[4], sprites[5], sprites[6], sprites[7] }, 10f);

// per frame: only switch when the animation actually changes
var next = moving ? "Run" : "Idle";
if (!_animator.IsAnimationActive(next))
    _animator.Play(next);

// facing flip follows movement direction
_animator.FlipX = velocity.X < 0;

// chain: attack animation flows into recover without polling
_animator.OnAnimationCompletedEvent += name =>
{
    if (name == "Attack") _animator.Play("Recover");
};
```

## `Nez.Tiled.TiledMapRenderer`

Takes a `TmxMap` (loaded via `Content.LoadTiledMap("path/arena.tmx")`).
The constructor's layer name is the **collision** layer — rendering
defaults to all layers.

| Member | Signature | ★ | Notes |
|---|---|---|---|
| ctor | `TiledMapRenderer(TmxMap map, string collisionLayerName = null, bool shouldCreateColliders = true)` | ★★ | Names the collision layer and (by default) auto-creates tile colliders from it. |
| `SetLayerToRender` | `void SetLayerToRender(string layerName, char separator = '/')` | | Restrict *rendering* to one layer. Foreground/background split = two renderers. |
| `SetLayersToRender` | `void SetLayersToRender(params string[] layerNames)` | | Multi-layer variant. |
| `LayersToRender` | `public ITmxLayer[] LayersToRender` | | Null = render everything. Set directly if you prefer. |

```csharp
// scene setup: draw everything, collide against "Ground"
var map = Content.LoadTiledMap("Content/Arena/arena.tmx");
var tiles = CreateEntity("tiles");
tiles.AddComponent(new TiledMapRenderer(map, "Ground"));
player.AddComponent(new TiledMapMover(map.GetLayer<TmxLayer>("Ground")));

// two-renderer parallax-ish split: background layers + foreground layer
var back = CreateEntity("bg-tiles");
back.AddComponent(new TiledMapRenderer(map)).SetLayersToRender("Sky", "Hills");
var front = CreateEntity("fg-tiles");
front.AddComponent(new TiledMapRenderer(map)).SetLayerToRender("Foreground");
```

Pair with `TiledMapMover` (physics page) for the collidable layer. Layer
names are case-sensitive strings — consider `const string` fields once
you have several.

## `Nez.FollowCamera`

A drop-in follow camera component — or read it as the reference
implementation and write your own (the book does the latter).

| Member | ★ | Notes |
|---|---|---|
| `FollowCamera(Entity target)` | ★ | Constructor takes the target. Add to a camera entity. |
| `FollowLerp` | ★ | Smoothing factor. |
| `Deadzone` | | Don't move the camera until the target leaves this box — steadier framing. |
| `FocusOffset` | | Lookahead, built in. |

```csharp
// drop-in follow camera on its own entity
var cam = CreateEntity("camera");
var follow = new FollowCamera(playerEntity, Camera);
follow.FollowLerp = 0.1f;
follow.Deadzone = new RectangleF(0, 0, 40, 24); // steady framing near center
cam.AddComponent(follow);
```

## `Nez.SceneResolutionPolicy`

The enum you pass to `SetDesignResolution`. The book standardizes on
`ShowAllPixelPerfect`; the others matter when you ship beyond your own
window:

| Value | Notes |
|---|---|
| `ShowAllPixelPerfect` | Integer scale, letterboxed. Pixel art stays crisp. |
| `ShowAll` | Fractional scale, letterboxed. HD art; blurry pixel art. |
| `NoBorder` | Scale to fill, cropping overflow. No bars, lost edges. |
| `FixedHeight` / `FixedWidth` | Lock one axis, adapt the other. Common for phones. |

`Scene.LetterboxColor` sets the bar color; `Scene.PixelPerfectScale`
tells you the current integer scale.
