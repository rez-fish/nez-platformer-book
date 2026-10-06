---
description: Install the .NET SDK, gather the FNA natives, reference pinned Nez/FNA sources, and open your first window.
---

# Chapter 1: Toolchain and first window

> Build status: **[unverified]** — every snippet, project file, and build step
> below is written against the pinned sources but has not been compiled yet
> (no .NET SDK was available when this chapter was drafted). If a step fails
> for you, that is a bug in the chapter: note exactly where and we will fix it.

## Where we are

The book repo has a skeleton but no game. In this chapter you install the
.NET 8 SDK, check out Nez and FNA at their pinned commits, scaffold the
`Spirefall` project, and open a window. By the end, `dotnet run` shows a
cornflower-blue window titled "Spirefall" — the empty stage everything else
in the book will play on.

## Concepts

### The stack, bottom to top

- **.NET 8** is the runtime and toolchain (`dotnet build`, `dotnet run`).
  Nez's FNA-flavored project (`Nez.FNA.Core.csproj`) and FNA itself
  (`FNA.Core.csproj`) both target `net8.0`, so the game project targets
  `net8.0` too. (Nez's repo-wide `Directory.Build.props` says `net10.0`,
  but the FNA projects override it to `net8.0` — the FNA projects are the
  ones that matter here.)
- **FNA** is a reimplementation of Microsoft's XNA 4.0 game framework.
  Think of it as the portable "operating system" for the game: it owns the
  window, the graphics device, input, audio, and the `Game` base class with
  its `Initialize`/`Update`/`Draw` lifecycle. FNA is managed C#, but it
  calls into **native libraries** (SDL2 and friends) at runtime — on Windows
  these ship as DLLs that must sit next to your built `.exe`. The FNA
  project publishes them as the "fnalibs" archives.
- **Nez** is a 2D framework built on top of FNA (or MonoGame). Its root is
  the `Core` class, which *subclasses* XNA's `Game`. A Nez game therefore
  *is* an XNA game; Nez adds the Scene/Entity/Component machinery,
  cameras, physics queries, and content helpers on top.

### Why pinned source checkouts instead of NuGet

Nez's FNA projects reference FNA as a **sibling source checkout**, not a
NuGet package (`Nez.FNA.Core.csproj` contains
`<ProjectReference Include="..\..\FNA\FNA.Core.csproj" />`). The Nez README
blesses exactly one layout:

```
TopLevelFolderHousingEverything\
    FNA\
    YourGameProject\
    Nez\
```

We follow it. Pinning both repos to exact commits keeps the book
reproducible: the Nez APIs quoted here are the ones at that commit.

Pinned for this book (see `/AI_NOTES.md`):

| Repo | Ref | Commit |
|------|-----|--------|
| prime31/Nez | `net10-update` branch | `3f8cc40` |
| FNA-XNA/FNA | `master` | `24031e5b` (2026-10-04) |

### The bootstrap flow

Every XNA-family game boots the same way:

1. `Main` creates an instance of your game class and calls `Run()`.
2. `Run()` starts the message loop and calls your `Initialize()` override.
3. Your `Initialize()` assigns `Core.Scene` — the first scene to simulate
   and render.

`Core`'s constructor takes the window configuration as optional parameters:

```csharp
// Nez/Nez.Portable/Core.cs @ 3f8cc40  [unverified signature excerpt]
public Core(int width = 1280, int height = 720, bool isFullScreen = false,
            string windowTitle = "Nez", string contentDirectory = "Content",
            bool hardwareModeSwitch = true)
```

`Core.Scene` is a **static property**: assigning it the first time wires the
scene up immediately (`Scene.Begin()` runs); assigning it later queues a
scene change for the end of the frame. A brand-new `Scene` with no entities
does exactly one thing each frame: clear the screen to `Scene.ClearColor`,
which defaults to `Color.CornflowerBlue`.

### C# idioms on first appearance

You know these ideas from TypeScript and Go; only the spelling is new.

- **Inheritance:** `class SpirefallGame : Core` — the `: Core` is
  TypeScript's `extends Core`. C# has single class inheritance (plus
  interfaces, later).
- **`protected override void Initialize()`** — `Initialize` is declared
  `virtual` in XNA's `Game`; `override` replaces it, `protected` means
  visible to subclasses (like TS `protected`). Forgetting `override` when
  you meant it is a compile error, which is a kindness.
- **Attributes:** `[STAThread]` above `Main` is metadata the runtime reads
  — roughly a decorator. It marks the entry thread as single-threaded
  apartment; XNA templates have always carried it.
- **`using Nez;`** at the top of a file is `import` — it brings a
  namespace into scope. (`using var x = ...` inside a method is a
  different, unrelated feature: deterministic disposal. Context
  disambiguates.)
- **Named arguments:** `base(width: 1280, height: 720)` passes constructor
  arguments by name. `base(...)` in a constructor is the `super(...)` call;
  it must be the first thing the constructor does.
- **Static members:** `Core.Scene` and `Core.GraphicsDevice` are statics —
  global access points Nez sets up for you. Convenient; we will discuss
  when *not* to reach for them in a later chapter.

## Minimal snippets

These demonstrate the APIs. They are fragments, not the full files — the
chapter's task is to assemble them yourself.

Assigning the first scene (the pattern from `Nez-Samples/Game1.cs`):

```csharp
protected override void Initialize()
{
    base.Initialize();
    Scene = new Scene(); // first assignment: wired up immediately
}
```

Referencing the pinned sources from your game's `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\FNA\FNA.Core.csproj" />
  <ProjectReference Include="..\Nez\Nez.Portable\Nez.FNA.Core.csproj" />
</ItemGroup>
```

Changing what the empty scene clears to:

```csharp
var scene = new Scene();
scene.ClearColor = Color.Black; // ClearColor is a public field, not a property
Scene = scene;
```

## Exercises

Ordered easiest first. Do them in order; each builds on the last. Hints are
tiered: **nudge**, **bigger hint**, **near-solution**. Try to need as few as
possible.

### 1. Install the .NET 8 SDK and prove it

Install the .NET 8 SDK for Windows (the SDK, not just the runtime) from
Microsoft's download page, then open a fresh terminal and run
`dotnet --version`. It should print an `8.x.xxx` version.

- *Nudge:* the SDK installer is separate from the runtime installer; the
  page lists both.
- *Bigger hint:* `dotnet --list-sdks` shows every SDK on the machine. If
  the version you just installed is missing, the terminal predates the
  install — close and reopen it.
- *Near-solution:* download "SDK 8.0.x" for Windows x64, run the installer
  with defaults, open a *new* PowerShell, `dotnet --list-sdks` should list
  `8.0.xxx`.

### 2. Check out the pinned sources in the blessed layout

Create `C:\dev\spirefall-stack\` (any parent folder works). Clone
`https://github.com/prime31/Nez.git` into `Nez\`, check out the
`net10-update` branch at `3f8cc40`; clone
`https://github.com/FNA-XNA/FNA.git` into `FNA\` at `24031e5b`.
Verify the file `Nez\Nez.Portable\Nez.FNA.Core.csproj` contains a
`ProjectReference` to `..\..\FNA\FNA.Core.csproj`.

- *Nudge:* `git clone --branch net10-update <url>` then `git checkout 3f8cc40`.
  A shallow clone (`--depth 1`) cannot check out an older commit — clone fully.
- *Bigger hint:* after checking out, `git -C Nez rev-parse --short HEAD`
  should print `3f8cc40`, and `git -C FNA rev-parse --short HEAD` should
  print `24031e5b`.
- *Near-solution:* open `Nez.FNA.Core.csproj` in a text editor and search for
  `ProjectReference`; confirm the `Include` path points at the sibling `FNA`
  folder you just cloned.

### 3. Scaffold the game project and reference Nez

Inside `spirefall-stack\`, create `Spirefall\Spirefall.csproj`: an
`Exe` project targeting `net8.0`, with `ProjectReference`s to
`..\FNA\FNA.Core.csproj` and `..\Nez\Nez.Portable\Nez.FNA.Core.csproj`.
Run `dotnet build` from `Spirefall\`. It should succeed with no game code
at all — an empty `Exe` still needs a `Main`, so expect a *specific*
error about a missing entry point, and confirm the *references* resolved
(no "project not found" errors).

- *Nudge:* `<Project Sdk="Microsoft.NET.Sdk">` with
  `<OutputType>Exe</OutputType>` and `<TargetFramework>net8.0</TargetFramework>`.
- *Bigger hint:* the "does not contain a static 'Main' method" error is the
  *expected* outcome here — it proves the SDK found your project and parsed
  the references. "Unable to find project" means a path is wrong.
- *Near-solution:* compare your file against `/solutions/ch01/Spirefall.csproj`,
  but only *after* you have a build attempt of your own.

### 4. Bootstrap: the first window

Write `Program.cs` (`Main` creates your game class, calls `Run()`) and
`SpirefallGame.cs` (`public class SpirefallGame : Core`, constructor passes
`width: 1280, height: 720, windowTitle: "Spirefall"` to `base(...)`,
`Initialize` override assigns `Scene = new Scene();`). Run `dotnet run`.

- *Nudge:* `Main` needs `[STAThread]` and the `System` namespace import.
  `SpirefallGame` needs `using Nez;`.
- *Bigger hint:* `Run()` is inherited from XNA's `Game` — you don't write it.
  `Initialize` must call `base.Initialize()` first, then assign the scene.
- *Near-solution:* `/solutions/ch01/Program.cs` and
  `/solutions/ch01/SpirefallGame.cs` are the reference. Yours should differ
  at most cosmetically.

### 5. Make it yours: title, size, clear color

Change the window title to something else, the size to 960×540, and the
clear color to black. Re-run and confirm all three changed.

- *Nudge:* two of the three are `Core` constructor arguments; the third is
  set on the `Scene` before assigning it.
- *Bigger hint:* `scene.ClearColor = Color.Black;` — `Color` lives in the
  `Microsoft.Xna.Framework` namespace, which `Nez` re-exports via its own
  usings… actually no: add `using Microsoft.Xna.Framework;` yourself.
- *Near-solution:* construct the `Scene` into a local, set its `ClearColor`
  field, then assign `Scene = scene;`.

### 6. Break it on purpose: the null-scene guard

Assign `Scene = null;` (you'll need the null-forgiving operator `= null!;`
to satisfy the compiler — ask yourself why the compiler complains) and run.
Read the exception message, then revert.

- *Nudge:* Nez validates the assignment with `Insist.IsNotNull`.
- *Bigger hint:* the message tells you the invariant: "Scene cannot be null!".
  `Insist` is Nez's assert helper (see the Nez-Core FAQ); it only exists in
  DEBUG builds.
- *Near-solution:* the takeaway to write down: `Core.Scene` has a guard;
  a game can never be in a state with no scene, and the failure is loud and
  immediate rather than a null-reference three frames later.

### 7. Native libs: make FNA find SDL

{% hint style="warning" %}
**Missing natives:** `dotnet run` may fail with a `DllNotFoundException` (often `SDL2`): FNA's managed code P/Invokes into native libraries that must sit next to your `.exe`. Download the Windows x64 **fnalibs** archive (see the FNA docs, "Setting Up FNA"), and copy the DLLs next to the built exe. [unverified: exact archive name and DLL list — confirm against the FNA docs page at the time you do this.]
{% endhint %}

- *Nudge:* the exception message names the missing DLL. That name is your
  search query.
- *Bigger hint:* you don't install these system-wide; they live next to the
  exe, per-game. This is also how you will ship the game in Chapter 14.
- *Near-solution:* if the window opens without any DLL copying, your
  environment already resolved them (e.g. via PATH) — note that down, and
  still do the copy step once so you know where they belong.

## Checkpoint

How to run: from `spirefall-stack\Spirefall\`, `dotnet run` (after the
fnalibs from Exercise 7 are beside the exe).

What you should see: a **1280×720 window titled "Spirefall"** (or your
Exercise 5 values) filled with **cornflower blue** (or your Exercise 5
color). No entities, no input, no text — just the clear color. Resizing the
window stretches the blue. Closing the window ends the process cleanly.

What "done" means: exercises 1–5 complete; 6–7 attempted and written up
(one sentence each in your own notes is fine). Your `Spirefall\` folder now
contains `Spirefall.csproj`, `Program.cs`, and `SpirefallGame.cs`, and it
matches `/code/ch02-start` — which is just the reference copy of what you
built here.

## Next

Chapter 2 introduces the C# idioms a TypeScript/Go engineer actually needs
and the XNA game loop, and gets a square moving under keyboard input.
