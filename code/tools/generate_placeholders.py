#!/usr/bin/env python3
"""Regenerate the binary placeholder assets for the Spirefall book.

The GitHub push path used for this repo is text-only, so the generated
.wav bleeps and tiles.png placeholders live here as a script instead of
as binaries. Run from the repo root:

    python3 code/tools/generate_placeholders.py

It writes:
  solutions/ch10/Content/Audio/{jump,land,dash}.wav
  solutions/ch11/Content/Audio/{shoot,hit}.wav   (plus ch10's three)
  solutions/ch12/Content/Audio/death.wav         (plus ch11's five)
  solutions/ch07..14 /Content/Arena/tiles.png    (ch07's is 2 tiles wide;
                                                  ch08+ uses the wider map
                                                  but the same tileset)
...and the same trees under code/ch11-start .. code/ch14-start.

Chapters that use them: 7 (tiles), 10 (sfx), 11 (shoot/hit), 12 (death).
"""

import math
import os
import random
import struct
import wave
import zlib

REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SR = 22050


def write_wav(path, samples):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    w = wave.open(path, "wb")
    w.setnchannels(1)
    w.setsampwidth(2)
    w.setframerate(SR)
    w.writeframes(
        b"".join(
            struct.pack("<h", max(-32768, min(32767, int(s * 32767))))
            for s in samples
        )
    )
    w.close()


def write_png(path, width, height, pixel):
    raw = b"".join(
        b"\x00" + b"".join(bytes(pixel(x, y)) for x in range(width))
        for y in range(height)
    )

    def chunk(ctype, data):
        c = ctype + data
        return (
            struct.pack(">I", len(data))
            + c
            + struct.pack(">I", zlib.crc32(c) & 0xFFFFFFFF)
        )

    png = (
        b"\x89PNG\r\n\x1a\n"
        + chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0))
        + chunk(b"IDAT", zlib.compress(raw))
        + chunk(b"IEND", b"")
    )
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as f:
        f.write(png)


def tile_pixel(x, y):
    # tile 0 (x < 16): stone block; tile 1: grass-topped platform
    if x < 16:
        if x == 0 or y == 0 or x == 15 or y == 15:
            return (70, 70, 82, 255)
        if x == 1 or y == 1:
            return (120, 120, 132, 255)
        return (100, 100, 112, 255)
    if y < 4:
        return (96, 150, 84, 255)
    if y == 4:
        return (74, 118, 66, 255)
    return (100, 100, 112, 255)


def gen_audio():
    # jump: rising chirp
    n = int(SR * 0.15)
    jump = [
        math.sin(2 * math.pi * (300 + 400 * i / n) * i / SR) * (1 - i / n) * 0.5
        for i in range(n)
    ]
    # land: low thud
    n = int(SR * 0.12)
    land = [
        math.sin(2 * math.pi * 90 * i / SR) * math.exp(-i / (SR * 0.03)) * 0.7
        for i in range(n)
    ]
    # dash: noise burst
    n = int(SR * 0.18)
    random.seed(7)
    dash = [
        random.uniform(-1, 1) * (1 - i / n) * 0.35 * (0.5 + 0.5 * math.sin(i / 40))
        for i in range(n)
    ]
    # shoot: square blip
    n = int(SR * 0.08)
    shoot = [
        0.4
        * (1 if math.sin(2 * math.pi * (900 - 500 * i / n) * i / SR) > 0 else -1)
        * (1 - i / n)
        for i in range(n)
    ]
    # hit: metallic ding
    n = int(SR * 0.2)
    hit = [
        (
            math.sin(2 * math.pi * 1320 * i / SR) * 0.4
            + math.sin(2 * math.pi * 1980 * i / SR) * 0.25
        )
        * math.exp(-i / (SR * 0.05))
        for i in range(n)
    ]
    # death: descending tone
    n = int(SR * 0.3)
    death = [
        math.sin(2 * math.pi * (500 - 380 * i / n) * i / SR)
        * (1 - i / n)
        * 0.5
        for i in range(n)
    ]
    sounds = {
        "jump.wav": jump,
        "land.wav": land,
        "dash.wav": dash,
        "shoot.wav": shoot,
        "hit.wav": hit,
        "death.wav": death,
    }
    return sounds


def main():
    sounds = gen_audio()

    content_dirs = []
    for ch in range(7, 15):
        content_dirs.append(os.path.join(REPO, "solutions", f"ch{ch:02d}"))
    for ch in range(8, 15):
        content_dirs.append(os.path.join(REPO, "code", f"ch{ch:02d}-start"))

    sol_dirs = [os.path.join(REPO, "solutions", f"ch{ch:02d}")
                for ch in range(10, 15)]
    start_dirs = [os.path.join(REPO, "code", f"ch{ch:02d}-start")
                  for ch in range(11, 15)]
    for d in sol_dirs + start_dirs:
        # chapter number: solutions/chNN or code/chNN-start
        base = os.path.basename(d).replace("-start", "")
        ch = int(base[2:4])
        names = ["jump.wav", "land.wav", "dash.wav"]
        if ch >= 11:
            names += ["shoot.wav", "hit.wav"]
        if ch >= 12:
            names += ["death.wav"]
        for name in names:
            write_wav(os.path.join(d, "Content", "Audio", name), sounds[name])

    for d in content_dirs:
        write_png(os.path.join(d, "Content", "Arena", "tiles.png"),
                  32, 16, tile_pixel)

    print("placeholder assets regenerated")


if __name__ == "__main__":
    main()
