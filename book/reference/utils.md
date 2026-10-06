---
description: "Coroutines, timers, tweens, Time, Debug, and Storage — the utilities that replace hand-rolled timers."
---

# Utilities: coroutines, tweens, timers

The chapters hand-roll float timers for everything (dash duration, coyote
time, spawn protection). Nez ships three better tools. Learn these and
half the timer fields in the book disappear.

## Coroutines — `Core.StartCoroutine`

```csharp
Core.StartCoroutine(RespawnSequence());

IEnumerator RespawnSequence()
{
    yield return Coroutine.WaitForSeconds(1.5f);
    RespawnPlayer();
    yield return Coroutine.WaitForSeconds(0.5f);
    EnableInput();
}
```

| Member | Signature | ★ | Notes |
|---|---|---|---|
| `Core.StartCoroutine` | `static ICoroutine StartCoroutine(IEnumerator enumerator)` | ★ | Runs the enumerator; returns a handle with `Stop()`. |
| `Coroutine.WaitForSeconds` | `static object WaitForSeconds(float seconds)` | ★ | Yield this to pause the sequence. Scaled by `Time.TimeScale` — a frozen game freezes its coroutines too. |
| `ICoroutine.Stop()` | | | Cancel the sequence early. |
| `ICoroutine.SetUseUnscaledDeltaTime(bool)` | | | Opt out of time-scaling (pause menus, hitstop recovery). |

Coroutines are *the* Nez idiom for "do A, wait, do B." Anywhere the book
writes `_timer -= dt; if (_timer <= 0)` across frames is a candidate.

## Timers — `Core.Schedule`

For single fire-and-forget delays, a timer is lighter than a coroutine:

```csharp
Core.Schedule(2f, t => SpawnEnemy());                    // one shot
Core.Schedule(0.5f, true, this, t => Blink());           // repeating, with context
```

| Member | Signature | ★ | Notes |
|---|---|---|---|
| `Core.Schedule` | `static ITimer Schedule(float seconds, Action<ITimer> onTime)` | ★ | Overloads add `repeats` and a `context` object. |
| `ITimer.Stop()` / `ITimer.Reset()` | | | Cancel or restart. |

## Tweens — `Nez.Tweens`

LeanTween-port with a fluent API. Extension methods on `Entity` and
`Transform` — juice without update code:

```csharp
entity.TweenPositionTo(target, 0.3f).SetEaseType(EaseType.QuartOut).Start();
```

| Member | ★ | Notes |
|---|---|---|
| `TweenPositionTo` / `TweenScaleTo` / `TweenRotationDegreesTo` | ★ | The big three. Local variants (`TweenLocalPositionTo`) for children. |
| `SetEaseType(EaseType)` | ★ | `QuartOut`, `BackOut`, `ElasticOut`… — the feel lives here. |
| `.Start()` | ★★★ | Tweens are built, then started. Forgetting `Start()` is the classic bug. |
| `Tweens.Create()` factories | | `FloatTween`, `Vector2Tween`, `ColorTween` for non-transform values. |
| `TweenManager.RemoveAllTweensOnLevelLoad` | | Static flag — kill in-flight tweens on scene change. |

## `Nez.Time`

| Member | ★ | Notes |
|---|---|---|
| `Time.DeltaTime` (static field) | ★★★ | Seconds since last frame, **scaled** by `TimeScale`. All movement multiplies this. |
| `Time.TimeScale` (static field) | ★★ | 1 = normal, 0 = frozen. Pause, hitstop, game-over freeze — one field. |
| `Time.AltDeltaTime` | | Unscaled delta. UI animation and pause menus that must run while frozen. |
| `Time.TotalTime` | | Seconds since boot. Idle bobbing, shader clocks. |

## `Nez.Debug`

| Member | ★ | Notes |
|---|---|---|
| `Debug.Log(string, params)` | ★★ | Format-string logging. The book's printf-debugging workhorse. |
| `Debug.DrawLine` / `DrawCircle` / `DrawText` | ★ | Immediate-mode debug drawing for one frame — sight lines, ranges, vectors. |
| `Debug.RenderEnabled` | | Master switch for the debug draw layer. |
| `Core.DebugRenderEnabled` | ★★ | The collider outlines (see Core page). |

## `Nez.Utils.Storage` — save data

The book ships without persistence; this is the missing piece:

```csharp
Storage.Save("scores.json", new { High = 5 });
var data = Storage.Load<ScoreData>("scores.json");
```

Flat JSON in the platform's app-data folder. Scores, settings, unlocks —
the last 5% of "shipped."
