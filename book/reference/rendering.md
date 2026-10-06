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

## `Nez.Tiled.TiledMapRenderer`

| Member | ★ | Notes |
|---|---|---|
| `TiledMapRenderer(TiledMap map, string layerName)` | ★★ | Draws one tile layer. Add one component per layer you want visible. |
| `TiledMapRenderer(TiledMap map)` | | Renders all layers. |

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
