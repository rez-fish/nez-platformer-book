#!/usr/bin/env python3
"""Generate figures and cover art for the book site.

Run from the repo root:
    python3 code/tools/make_figures.py

Outputs SVG diagrams + a tilemap overview PNG into book/assets/.
SVGs use transparent backgrounds and a dark-slate box palette that reads
well on both GitBook light and dark themes.
"""

import os
import random

REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
ASSETS = os.path.join(REPO, "book", "assets")
os.makedirs(ASSETS, exist_ok=True)

FONT = "font-family=\"ui-monospace, 'Cascadia Code', Menlo, Consolas, monospace\""
BOX = "#243044"
BOX_EDGE = "#3d5170"
INK = "#ffffff"
MUTED = "#8a94a6"
AMBER = "#e8a33d"
GREEN = "#58b368"
RED = "#e05c5c"
BLUE = "#5b9bd5"


def box(x, y, w, h, fill=BOX, edge=BOX_EDGE, rx=8):
    return (f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" '
            f'fill="{fill}" stroke="{edge}" stroke-width="1.5"/>')


def text(x, y, s, size=15, fill=INK, anchor="middle", weight="normal", ls=None):
    s = s.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")
    extra = f' letter-spacing="{ls}"' if ls else ""
    return (f'<text x="{x}" y="{y}" text-anchor="{anchor}" font-size="{size}" '
            f'fill="{fill}" font-weight="{weight}" {FONT}{extra}>{s}</text>')


def arrow(x1, y1, x2, y2, color=MUTED, dashed=False, w=1.5):
    dash = ' stroke-dasharray="6,4"' if dashed else ""
    return (
        f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{color}" '
        f'stroke-width="{w}"{dash}/>'
        f'<polygon points="{x2},{y2} {x2-7},{y2-4} {x2-7},{y2+4}" fill="{color}"/>'
    )


def vlabel(x, y, s, size=13):
    return text(x, y, s, size=size, fill=MUTED)


# ---------------------------------------------------------------- game loop
def game_loop():
    parts = ['<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 400 300">']
    boxes = [("Initialize()", 20), ("Update( dt )", 100), ("Draw()", 180)]
    for label, y in boxes:
        parts.append(box(110, y, 180, 44))
        parts.append(text(200, y + 28, label, size=16))
    parts.append(arrow(200, 64, 200, 96))
    parts.append(arrow(200, 144, 200, 176))
    # return path on the right
    parts.append(f'<path d="M 300 202 L 340 202 L 340 122 L 300 122" fill="none" '
                 f'stroke="{MUTED}" stroke-width="1.5"/>')
    parts.append('<polygon points="300,122 307,126 307,118" fill="%s"/>' % MUTED)
    parts.append(vlabel(200, 258, "simulate, then render — never mix"))
    parts.append(vlabel(200, 278, "every frame, until the window closes"))
    parts.append('</svg>')
    return "\n".join(parts)


# ------------------------------------------------------- entity/component
def ec_hierarchy():
    parts = ['<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 560 340">']
    parts.append(box(210, 16, 140, 44))
    parts.append(text(280, 44, "Scene", size=16, weight="bold"))
    for i, (name, comps) in enumerate([
            ("archer", ["SpriteRenderer", "BoxCollider", "Mover", "PlayerController"]),
            ("platform", ["BoxCollider"])]):
        ex = 70 + i * 260
        parts.append(arrow(280, 60, ex + 75, 96))
        parts.append(box(ex, 96, 150, 40))
        parts.append(text(ex + 75, 122, f'Entity "{name}"', size=14))
        for j, c in enumerate(comps):
            cx = ex - 45 + (j % 2) * 140
            cy = 152 + (j // 2) * 40
            parts.append(f'<rect x="{cx}" y="{cy}" width="130" height="30" rx="15" '
                         f'fill="#1b2740" stroke="{BLUE}" stroke-width="1.2"/>')
            parts.append(text(cx + 65, cy + 20, c, size=11.5))
            parts.append(f'<line x1="{ex + 75}" y1="136" x2="{cx + 65}" y2="{cy}" '
                         f'stroke="{BOX_EDGE}" stroke-width="1"/>')
    parts.append(vlabel(280, 330, "entities are bags of components; systems read the bags"))
    parts.append('</svg>')
    return "\n".join(parts)


# ------------------------------------------------------- axis separation
def axis_separation():
    parts = ['<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 620 250">']
    for panel, title, px in [(0, "one diagonal move", 0), (1, "X first, then Y", 320)]:
        parts.append(vlabel(140 + px, 24, title, size=14))
        # corner: vertical wall + floor forming an inner corner
        parts.append(f'<rect x="{150 + px}" y="{50}" width="26" height="130" '
                     f'fill="{BOX}" stroke="{BOX_EDGE}"/>')
        parts.append(f'<rect x="{150 + px}" y="{154}" width="110" height="26" '
                     f'fill="{BOX}" stroke="{BOX_EDGE}"/>')
        # player square
        parts.append(f'<rect x="{80 + px}" y="{90}" width="22" height="22" rx="3" '
                     f'fill="{AMBER}"/>')
        if panel == 0:
            parts.append(arrow(102 + px, 101, 148 + px, 152, color=RED, dashed=True, w=2))
            parts.append(text(140 + px, 215, "✕  sticks on the corner", size=14, fill=RED))
        else:
            parts.append(arrow(102 + px, 101, 146 + px, 101, color=AMBER, w=2))
            parts.append(arrow(144 + px, 112, 144 + px, 150, color=AMBER, w=2))
            parts.append(text(160 + px, 215, "✓  slides cleanly", size=14, fill=GREEN))
    parts.append('</svg>')
    return "\n".join(parts)


# ------------------------------------------------------- state machine
def state_machine():
    parts = ['<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 560 210">']
    parts.append(box(40, 70, 150, 56))
    parts.append(text(115, 104, "Normal", size=16, weight="bold"))
    parts.append(box(370, 70, 150, 56))
    parts.append(text(445, 104, "Dashing", size=16, weight="bold"))
    parts.append(arrow(190, 86, 366, 86, color=AMBER, w=2))
    parts.append(text(278, 74, "press X, dash ready", size=12, fill=MUTED))
    parts.append(f'<path d="M 370 110 L 190 110" stroke="{MUTED}" stroke-width="1.5"/>')
    parts.append('<polygon points="190,110 197,106 197,114" fill="%s"/>' % MUTED)
    parts.append(text(280, 132, "timer expires", size=12, fill=MUTED))
    parts.append(vlabel(280, 178, "one enum, one branch per state, transitions at the edges"))
    parts.append('</svg>')
    return "\n".join(parts)


# ------------------------------------------------------- ai pipeline
def ai_pipeline():
    parts = ['<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 640 170">']
    steps = [("Sense", "linecast to player"),
             ("Think", "seen ? Chase : Patrol"),
             ("Act", "mover + hop")]
    for i, (title, sub) in enumerate(steps):
        x = 20 + i * 205
        parts.append(box(x, 40, 180, 64))
        parts.append(text(x + 90, 66, title, size=16, weight="bold"))
        parts.append(text(x + 90, 88, sub, size=12, fill=MUTED))
        if i < 2:
            parts.append(arrow(x + 180, 72, x + 205, 72, color=AMBER, w=2))
    parts.append(vlabel(320, 140, "the same shape as the player's state machine, driven by perception"))
    parts.append('</svg>')
    return "\n".join(parts)


# ------------------------------------------------------- arena overview
def arena_overview():
    """Render the actual arena tilemap as an SVG (flat colors per tile).

    Uses the ch07 map: the figure is embedded in ch07, where the arena is
    still 20x11 (it widens to 40x11 in ch08).
    """
    tmx_path = os.path.join(REPO, "solutions", "ch07", "Content", "Arena", "arena.tmx")
    rows = []
    with open(tmx_path) as f:
        txt = f.read()
    data = txt.split("<data encoding=\"csv\">")[1].split("</data>")[0]
    for line in data.strip().splitlines():
        line = line.strip().rstrip(",")
        if line:
            rows.append([int(v) for v in line.split(",")])

    cols, nrows = len(rows[0]), len(rows)
    W, H = cols * 16, nrows * 16
    parts = [f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {W} {H}">']
    parts.append(f'<rect width="{W}" height="{H}" fill="#141a26"/>')
    for ry, row in enumerate(rows):
        for rx, gid in enumerate(row):
            if gid == 0:
                continue
            x, y = rx * 16, ry * 16
            parts.append(f'<rect x="{x}" y="{y}" width="16" height="16" '
                         f'fill="#646470"/>')
            if gid == 2:  # grass-top tile
                parts.append(f'<rect x="{x}" y="{y}" width="16" height="5" '
                             f'fill="#609654"/>')
    parts.append('</svg>')
    out = os.path.join(ASSETS, "arena-overview.svg")
    with open(out, "w") as f:
        f.write("\n".join(parts))
    print("wrote arena-overview.svg")


# ------------------------------------------------------- cover
def cover():
    random.seed(42)
    W, H = 1600, 900
    parts = [f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {W} {H}">']
    parts.append(
        '<defs><linearGradient id="bg" x1="0" y1="0" x2="0" y2="1">'
        f'<stop offset="0" stop-color="#0c1120"/><stop offset="1" '
        f'stop-color="#1b2540"/></linearGradient></defs>')
    parts.append(f'<rect width="{W}" height="{H}" fill="url(#bg)"/>')
    for _ in range(90):
        x, y = random.randint(0, W), random.randint(0, H)
        s = random.choice([2, 2, 3])
        o = random.uniform(0.15, 0.6)
        parts.append(f'<rect x="{x}" y="{y}" width="{s}" height="{s}" '
                     f'fill="#ffffff" opacity="{o:.2f}"/>')

    # platforms (tile palette)
    plats = [(180, 700, 420, 40), (1050, 640, 380, 40), (620, 520, 300, 40),
             (80, 420, 260, 40), (1250, 400, 270, 40)]
    for x, y, w, h in plats:
        parts.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="4" '
                     f'fill="#646470" stroke="#3f3f4a" stroke-width="2"/>')
        parts.append(f'<rect x="{x}" y="{y}" width="{w}" height="12" rx="4" '
                     f'fill="#609654"/>')

    # pixel archer (from the ch03 placeholder: body + hood band)
    def archer(px_, py_, s, hood):
        body, out = (52, 52, 64), []
        for y in range(16):
            for x in range(16):
                c = "rgb(52,52,64)"
                if 2 <= y < 6 and 3 <= x < 13:
                    c = hood
                out.append(
                    f'<rect x="{px_ + x * s}" y="{py_ + y * s}" width="{s}" '
                    f'height="{s}" fill="{c}"/>')
        return "".join(out)

    parts.append(archer(240, 380, 14, "rgb(139,90,43)"))
    parts.append(archer(1230, 250, 9, "rgb(45,130,140)"))
    # arrow streak
    parts.append(f'<rect x="560" y="430" width="220" height="10" rx="5" fill="{AMBER}"/>')
    parts.append('<polygon points="780,418 820,435 780,452" fill="%s"/>' % AMBER)

    # title block
    parts.append(text(880, 330, "SPIREFALL", size=118, weight="bold", ls="18"))
    parts.append(text(880, 400, "a project-based book", size=34, fill="#9aa7c2"))
    parts.append(text(880, 448, "one arena platformer, built incrementally",
                      size=34, fill="#9aa7c2"))
    parts.append(text(880, 496, "with Nez on FNA", size=34, fill="#9aa7c2"))
    for i, chip in enumerate(["NEZ", "FNA", "C#", "16×16 PIXEL ART"]):
        cx = 640 + i * 170
        parts.append(f'<rect x="{cx}" y="560" width="150" height="44" rx="22" '
                     f'fill="none" stroke="{AMBER}" stroke-width="2"/>')
        parts.append(text(cx + 75, 589, chip, size=20, fill=AMBER))
    parts.append(text(880, 830, "rez-fish / nez-platformer-book", size=22, fill="#5b6b8c"))
    parts.append('</svg>')
    return "\n".join(parts)


def main():
    figs = {
        "loop.svg": game_loop(),
        "ec-hierarchy.svg": ec_hierarchy(),
        "axis-separation.svg": axis_separation(),
        "state-machine.svg": state_machine(),
        "ai-pipeline.svg": ai_pipeline(),
        "cover.svg": cover(),
    }
    for name, svg in figs.items():
        with open(os.path.join(ASSETS, name), "w") as f:
            f.write(svg)
        print("wrote", name)
    arena_overview()


if __name__ == "__main__":
    main()
