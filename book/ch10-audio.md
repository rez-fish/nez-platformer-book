---
description: "Sound effects without a content pipeline, plus music."
---

# Chapter 10: Sound — effects and music

> Build status: **[unverified]** — XNA audio APIs are stable and standard;
> `SoundEffect.FromStream` + `TitleContainer` is the FNA-friendly path, but
> confirm on first build.

## Where we are

The game is silent. This chapter adds a central sound board (`Sfx`),
wires jump/land/dash effects to the existing event points, and covers
music. The solution ships generated placeholder bleeps so it runs as-is;
Exercise 1 replaces them with real sounds.

**New files:** `Sfx.cs`, `Content/Audio/*.wav` (generated placeholders).
**Changed:** `PlayerController.cs` (four `Sfx.Play*` calls at existing
event points), `ArenaScene.cs` (`Sfx.Load()`).

## Concepts

### Two audio systems

XNA has two, and they serve different purposes:

- **`SoundEffect`**: short, fire-and-forget, possibly overlapping —
  jumps, lands, shots. Load once, `Play()` anywhere. `Play()` returns a
  `SoundEffectInstance` if you need per-play control; the static `Play`
  is fine for one-shots.
- **`Song` + `MediaPlayer`**: streamed background music, one at a time.
  `MediaPlayer.Play(song)`, `IsRepeating = true`, `Volume`.

### Loading without a pipeline

XNA's `Content.Load<SoundEffect>` wants compiled `.xnb` files (built by
the content pipeline). FNA games often skip the pipeline: `SoundEffect.
FromStream` loads a plain `.wav` at runtime, and `TitleContainer.
OpenStream` resolves the path relative to the exe folder — where our
csproj's `Content/**` rule already puts the files. {% hint style="success" %}
**No content pipeline:** Sounds are plain `.wav` files loaded with `SoundEffect.FromStream` — edit a wav, re-run, hear the change. No build step, no pipeline tool on the path.
{% endhint %}

For music, `Song.FromUri(name, uri)` plays `.ogg`/`.mp3` the same way
([unverified] exact format support — confirm against FNA docs; Exercise 5).

### The sound board pattern

`Sfx` is a static class: `Load()` once per scene, `PlayJump()` etc. from
anywhere. Statics are usually a smell; for a game's sound board they're
the pragmatic choice — dozens of call sites, no state worth injecting,
lifetime equals the game's. The rule from Chapter 8 applies: the seam is
explicit (`Sfx.PlayLand()` reads as what it is).

Note the `?.` in `PlayJump()`: if a wav is missing, `LoadWav` throws at
load time (loud, early — good). The null-conditional is belt-and-braces
for call sites that run before `Load`.

### C# for the TS/Go engineer, part 6

- **`using` the namespace vs the statement**: `using System.IO;` at the
  top imports names. (The *statement* `using (var s = ...)` disposes —
  different feature, same keyword, disambiguated by position.)
- **String interpolation**: `$"Content/Audio/{name}.wav"` — the `$`
  prefix enables `{...}` holes. Prefer it over concatenation.
- **`SoundEffect.Play(volume, pitch, pan)`**: the overload is the design
  tool — same sample, different feel per call (Exercise 2).

## Minimal snippets

The whole loading pattern:

```csharp
var stream = TitleContainer.OpenStream("Content/Audio/jump.wav");
var jump = SoundEffect.FromStream(stream);
jump.Play();
```

Expressive playback:

```csharp
// volume 0-1, pitch -1..1, pan -1 (left)..1 (right)
_shoot.Play(0.8f, RandomPitch(), pan: 0.3f);
```

## Exercises

### 1. Make real sounds

Generate (sfxr/jsfxr/whatever) or record `jump.wav`, `land.wav`,
`dash.wav` and drop them into `Content/Audio/`. Re-run — no code changes.

- *Nudge:* keep them short (< 0.3 s) and punchy; game SFX are felt, not
  listened to.
- *Bigger hint:* 16-bit mono 22050 Hz wav is the safe format.
- *Near-solution:* the placeholders are sine/noise bleeps from a script —
  anything with character beats them.

### 2. Pitch variation

Same sample twice in a row sounds robotic. Add a small random pitch
(-0.1..0.1) to every play.

- *Nudge:* `SoundEffect.Play(volume, pitch, pan)` overload.
- *Bigger hint:* a `static readonly Random` on `Sfx`; `(float)(_random.
  NextDouble() * 0.2 - 0.1)`.
- *Near-solution:* centralize it in the `Play*` methods so call sites
  stay clean — variation is a property of the *board*, not the caller.

### 3. Mute toggle

M key toggles all sound. 

- *Nudge:* `SoundEffect.MasterVolume` (static, 0..1).
- *Bigger hint:* poll in a component's `Update` — `PauseController` is a
  natural home, or a tiny new one.
- *Near-solution:* remember the previous volume to restore it; muting to
  0 and back to 1 is fine for now.

### 4. Break it: missing file

Rename `jump.wav` temporarily and run. Read the exception, note *where*
it throws, restore the file.

- *Nudge:* it throws in `Sfx.Load`, during `ArenaScene.Initialize`.
- *Bigger hint:* `TitleContainer.OpenStream` throws `FileNotFoundException`
  with the attempted path.
- *Near-solution:* loud-and-early beats silent-and-late: a missing sound
  *should* crash in development. (Ship builds might fall back — note the
  idea, don't implement it.)

### 5. Music

Add background music: `Song.FromUri` + `MediaPlayer`, looping, started in
`ArenaScene.Initialize`. [unverified] — confirm format support in FNA.

- *Nudge:* `MediaPlayer.IsRepeating = true; MediaPlayer.Volume = 0.5f;`
- *Bigger hint:* generate a placeholder loop or reuse a short ambient wav
  converted to ogg.
- *Near-solution:* stop it when leaving the arena (`MediaPlayer.Stop()`
  in the title flow) — music is scene-scoped, like Gum elements.

### 6. Positional pan

Pan land/dash sounds by the archer's screen X: left side → left speaker.

- *Nudge:* pan = `(x / 320f) * 2f - 1f`.
- *Bigger hint:* the `Play*` methods need a pan parameter, or compute it
  inside `Sfx` from a settable `ListenerX`.
- *Near-solution:* subtle at this scale, but it's the seed of real
  positional audio for versus mode.

### 7. The sound inventory (design)

List every event in the game so far that *should* make a sound but
doesn't (wall jump? wall slide loop? UI clicks?). One line each.

- *Nudge:* walk through the moveset mentally, action by action.
- *Bigger hint:* the rule of thumb: if the player *did* it, it makes a
  sound. If the world did it to the player, it makes a bigger sound.
- *Near-solution:* Chapters 11–12 will add shots, hits, pickups, enemy
  deaths — bring this list.

## Checkpoint

Run: `dotnet run` (with your Exercise 1 wavs, or the placeholders).

You should hear: a chirp on every jump (higher when held), a thud on
landing, a whoosh on dash. M mutes. The game finally has a soundtrack to
its movement — play for a minute and notice how much *faster* it feels.

Done means: exercises 1–3 complete; 4–7 attempted. `Content/Audio/`
holds your wavs.

## Next

Chapter 11 adds the point of the game: arrows, targets, and score — the
combat loop.
