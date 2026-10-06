---
description: "A curated Nez API reference extracted from the pinned source, organized by subsystem with popularity-ranked members."
---

# Nez API Reference

The chapters teach Nez by using it. This section is the companion: the
classes and members you'll reach for, organized the way you'll think
about them, with the most-used members first.

## How this reference was built

Every signature here was extracted from the Nez source at the book's
pinned commit (`net10-update` @ `3f8cc40`) — not from memory, not from
docs. If a signature looks different in your checkout, the source wins;
report it as a chapter bug.

**Popularity (★★★/★★/★)** is empirical, not vibes: each member was
counted across the 49 files of Nez-Samples plus the book's 100 solution
files. ★★★ means 40+ uses, ★★ means 10+, ★ means 3+. Unstarred members
are documented for completeness — they're real, just rarely needed.

This is a *curated* reference, not an exhaustive one. It covers the
subsystems a working Nez game actually touches. For everything else —
the full class list, the AI namespace beyond FSM, post-processing,
deferred lighting — the source is the documentation:

- Nez source @ the pinned commit: `https://github.com/prime31/Nez/tree/net10-update/Nez.Portable`
- Nez FAQs: `https://github.com/prime31/Nez/tree/net10-update/FAQs`
- Nez-Samples: `https://github.com/prime31/Nez-Samples`

## Pages

- [Core: game, scene, entity, component](core.md) — the ECS heart. Start here.
- [Physics: movers, colliders, queries](physics.md) — `Mover`, the spatial hash, linecasts.
- [Rendering: sprites, camera, tilemaps](rendering.md) — what draws, and how the camera sees it.
- [Input: polling and virtual input](input.md) — `Input` statics and the `VirtualInput` abstraction.
- [Utilities: coroutines, tweens, timers](utils.md) — the game-feel toolbox the chapters hand-roll.
- [AI: state machines](ai.md) — `StateMachine<T>` and where the rest of the AI namespace lives.
