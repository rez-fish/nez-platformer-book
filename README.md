# Nez Platformer Book

A project-based book on making games with **Nez on top of FNA**, in the spirit of
BYTEPATH for LÖVE2D: one real game built incrementally, concepts explained
before code, many exercises.

**Audience:** experienced web/backend engineers (TypeScript, Go) who are new to
C# and new to game development. No general programming basics; C# idioms and
XNA/FNA concepts are explained on first appearance.

**Format:** one living GitBook in this repo. No PDFs, zips, or standalone docs.

## Layout

- `/book` — one markdown file per chapter, plus `SUMMARY.md`
- `/code/chNN-start` — the project as it should be when you begin chapter NN
- `/solutions/chNN` — reference solutions (never quoted in chapter text)
- `/AI_NOTES.md` — assistant working notes (not listed in SUMMARY.md)

## Conventions

- Every snippet and solution must compile against the pinned versions in
  `AI_NOTES.md`. Unverified claims are marked `[unverified]`.
- Chapters never contain full solutions; solutions live only in `/solutions`.
- Each chapter builds on the real code from the previous one. If a chapter
  requires changing earlier code or text, the change is called out explicitly.

## Status

All 14 chapters written (2026-10-05). The book builds a complete local-versus
arena platformer ("Spirefall") with Nez on FNA.

## Binary placeholders

The `.wav` sound effects and `tiles.png` tilesets in `/solutions` and
`/code` are generated placeholders (sine/noise bleeps, procedural tiles).
Regenerate them any time from the repo root:

```
python3 code/tools/generate_placeholders.py
```

Replace them with real assets as described in Chapters 7 and 10.
