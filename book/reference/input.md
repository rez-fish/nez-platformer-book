---
description: "The Input statics and the VirtualInput abstraction — keyboard, gamepad, and mouse behind one API."
---

# Input: polling and virtual input

Two layers. `Nez.Input` answers "what is down *right now*" (polling).
`VirtualInput` (`VirtualButton`, `VirtualIntegerAxis`, `VirtualJoystick`)
lets many physical sources feed one logical control — the layer that
makes local multiplayer and remapping sane.

## `Nez.Input` — polling statics

| Member | Signature | ★ | Notes |
|---|---|---|---|
| `IsKeyDown` | `static bool IsKeyDown(Keys key)` | ★★★ | Held right now. Movement. |
| `IsKeyPressed` | `static bool IsKeyPressed(Keys key)` | ★★★ | Went down *this frame*. Jumps, shots — anything edge-triggered. |
| `IsKeyReleased` | `static bool IsKeyReleased(Keys key)` | | Went up this frame. Variable jump height. |
| `IsMouseButtonDown` / `IsMouseButtonPressed` | | ★ | UI, aim-at-cursor. |
| `GamePads` | | ★ | `Input.GamePads[0]` — per-pad state. Check `.IsConnected` before reading a pad that might not exist. |
| `DEFAULT_DEADZONE` | `const float DEFAULT_DEADZONE = 0.1f` | | Stick deadzone used by the virtual-input stick bindings. |

All three key methods also take `Keys[]` for "any of these."

## `VirtualButton`

One logical button, many physical sources. Reads `IsDown` (held),
`IsPressed` (edge), `IsReleased` (edge) — the same semantics as the
polling API, so controllers don't care what's behind them.

```csharp
_jump = new VirtualButton();
_jump.AddKeyboardKey(Keys.Z);
_jump.AddGamePadButton(0, Buttons.A);
// controller: if (_jump.IsPressed) ...
```

| Member | ★ | Notes |
|---|---|---|
| `AddKeyboardKey(Keys)` | ★★ | `AddKeyboardKey(Keys, Keys modifier)` variant for chords. |
| `AddGamePadButton(int index, Buttons button)` | ★ | Player index + XNA `Buttons` enum. |
| `AddMouseLeftButton()` (etc.) | | Mouse as a button source. |
| `IsDown` / `IsPressed` / `IsReleased` | ★★★ | The three reads. Properties, not methods. |
| `BufferTime` (float field) | ★ | **Built-in input buffering.** Set to e.g. 0.12 and `IsPressed` stays true that long after the physical press — the book's jump buffer, for free. |
| `ConsumeBuffer()` | | Clear a buffered press manually (used it? eat it). |
| `Deregister()` | ★★ | Call in `OnRemovedFromEntity`. Virtual inputs self-update; deregistering is the cleanup. |

## `VirtualIntegerAxis`

One logical axis (-1/0/+1), many sources. Movement.

```csharp
_moveX = new VirtualIntegerAxis();
_moveX.AddKeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right);
_moveX.AddGamePadLeftStickX(0);
_moveX.AddGamePadDPadLeftRight(0);
// controller: float x = _moveX.Value;
```

| Member | ★ | Notes |
|---|---|---|
| `AddKeyboardKeys(OverlapBehavior, Keys negative, Keys positive)` | ★ | `TakeNewer` vs `TakeOlder` decides opposing-key behavior. |
| `AddGamePadLeftStickX/Y`, `AddGamePadRightStickX/Y` | ★ | Optional deadzone parameter (defaults to `Input.DEFAULT_DEADZONE`). |
| `AddGamePadDPadLeftRight` / `AddGamePadDPadUpDown` | | D-pad as an axis. |
| `Value` (int, -1/0/1) | ★★★ | The read. |
| `Deregister()` | ★★ | Same cleanup as buttons. |

`VirtualJoystick` is the 2D analog version (a `Vector2` `Value`) for
twin-stick aim/move. Same `Add…` builder pattern.
