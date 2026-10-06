---
description: Publish for Windows, package the FNA natives, write the player README, ship it.
---

# Chapter 14: Ship it — publishing for Windows

> Build status: **[unverified]** — packaging steps follow the standard .NET
> + FNA layout; confirm the fnalibs inventory against FNA's current docs.

## Where we are

Thirteen chapters built a complete local-versus arena platformer. This
chapter gets it off your machine: Release publish, the FNA native
libraries, a player-facing README, and the clean-machine test. No new
game code — `publish.bat` is the only artifact.

## Concepts

### Debug vs Release

You've been running DEBUG builds (`dotnet run` defaults to Debug):
unoptimized, with debug symbols, and — importantly for us — with Nez's
debug console on `~`. `dotnet publish -c Release` builds optimized, and
it's what players get. Behavior differences between the two are rare but
real (timing, uninitialized-memory luck); always playtest the Release
build before shipping it.

### Framework-dependent vs self-contained

- **Framework-dependent** (`--self-contained false`): small (~15 MB) —
  requires the .NET 8 runtime on the player's machine. Most gamers don't
  have it.
- **Self-contained** (`-r win-x64 --self-contained`): big (~70 MB) —
  runs anywhere, no install. For an itch.io game, this is the kind one.

`publish.bat` uses framework-dependent as the default and notes the
switch; Exercise 6 compares.

### The FNA natives

FNA is C#, but it talks to C libraries (SDL3 for windowing/input/audio,
etc.). Those ship as **native DLLs that must sit next to your exe** —
the `fnalibs` distribution (published as GitHub Actions artifacts on
`FNA-XNA/fnalibs-dailies`; there is deliberately no NuGet package for
FNA). Miss one and the game dies on startup with a `DllNotFoundException`
(Exercise 5). The Windows x64 set is four DLLs from the archive's
`win-x64` folder: `SDL3.dll`, `FNA3D.dll`, `FAudio.dll`,
`libtheorafile.dll`. The exact set drifts over time — the dailies track
FNA master, so re-download if yours are months old — but check FNA's docs
rather than trusting any list, including this chapter's.

### The player README

Players need: controls for both players, the win condition, requirements
(.NET runtime version if framework-dependent; gamepad recommended for P2),
and your contact/itch page. Write it *before* you zip — the zip without
it is a puzzle box.

## Minimal snippets

The publish, by hand:

```bat
dotnet publish -c Release -r win-x64 --self-contained false -o publish\win-x64
REM then copy fnalibs x64 DLLs next to Spirefall.exe
```

Self-contained instead:

```bat
dotnet publish -c Release -r win-x64 --self-contained true -o publish\win-x64-sc
```

## Exercises

### 1. Publish and run

Run `publish.bat` (adjust the fnalibs path to yours). Launch
`publish\win-x64\Spirefall.exe` — not from your project folder, from the
publish folder.

- *Nudge:* if it crashes instantly, the natives are the first suspect.
- *Bigger hint:* `DllNotFoundException` names the missing library.
- *Near-solution:* working publish-folder launch is the chapter's
  checkpoint — everything else is polish.

### 2. The clean-machine test

Copy the publish folder to a machine (or VM) without the .NET SDK and
run it. Beg, borrow, or virtualize — this is the only test that counts.

- *Nudge:* framework-dependent needs the .NET 8 *runtime* (not SDK).
- *Bigger hint:* if it fails here but worked in Exercise 1, the difference
  is the environment — that's the whole point of the test.
- *Near-solution:* self-contained if you want zero requirements.

### 3. Player README

Write `README-players.md`: title, one-line pitch, controls (both players),
win condition, requirements, credits. Put it in the zip.

- *Nudge:* write for someone who never saw this book.
- *Bigger hint:* controls tables beat paragraphs.
- *Near-solution:* this file is the difference between a build and a
  *release*.

### 4. itch.io draft

Create a draft itch.io page (don't publish): upload the zip, set the
kind to "Game", write the short description, add controls to the page.

- *Nudge:* itch lets you keep pages in draft indefinitely.
- *Bigger hint:* screenshots: Win+Shift+S during a versus match — action
  shots sell, menus don't.
- *Near-solution:* the draft existing is the win; publishing is your call.

### 5. Break it: missing native

Delete `SDL3.dll` from a *copy* of the publish folder and run. Read the
exception.

- *Nudge:* `DllNotFoundException: Unable to load DLL 'SDL3'`.
- *Bigger hint:* a missing native DLL is the #1 "it works on my machine"
  failure for FNA games — now you recognize it on sight.
- *Near-solution:* restore the DLL. Checklists beat memory: natives are
  a line on yours now.

{% hint style="warning" %}
**The #1 FNA shipping failure:** A missing native DLL (`DllNotFoundException`, often `SDL3`) is the #1 "it works on my machine" failure for FNA games — now you recognize it on sight.
{% endhint %}

### 6. Compare publish modes

Publish both framework-dependent and self-contained. Compare folder sizes
and startup behavior.

- *Nudge:* `publish\win-x64` vs `publish\win-x64-sc`.
- *Bigger hint:* ~15 MB vs ~70 MB is typical; the runtime cost is disk,
  not performance.
- *Near-solution:* pick one for the itch draft and note why.

### 7. Stamp the version

Show the version somewhere players can see it — window title
(`"Spirefall 0.1"`) or the title screen.

- *Nudge:* `Assembly.GetEntryAssembly()?.GetName().Version` or a const.
- *Bigger hint:* the title screen already has Gum text — one more label.
- *Near-solution:* version visibility turns "it crashed" reports into
  actionable bugs.

## Checkpoint

You should have: a `publish\win-x64` folder that launches on a clean
machine, a player README, and (optionally) an itch.io draft. Zip the
folder — that's the game, done.

Done means: exercises 1–3 complete; 4–7 attempted. Someone other than
you has played it, or is about to.

## Next

The book is complete: toolchain → movement → collisions → levels →
juice → flow → audio → combat → enemies → versus → ship. Where next is
yours: more arenas (Chapter 7's design exercise), powerups (Chapter 6's
dash-refill sketch), a single-player campaign (Chapter 12's AI + Chapter
13's GameMode exercise) — or a new game entirely. The patterns transfer:
state machines, forgiveness timers, input abstraction, and scenes as
states will serve every game you make. (Online multiplayer stays out of
scope — that boundary was set in the introduction, and it still stands.)
