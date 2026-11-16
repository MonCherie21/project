"""Compose an original, gentle score for Whisper's first-bond prologue.

No samples, MIDI files or third-party songs are used. Run from any directory:
    python3 tools/generate_opening_score.py
Requires numpy and ffmpeg with libvorbis; writes Music/WhisperPromise.ogg.
"""

from __future__ import annotations

import math
import subprocess
import tempfile
import wave
from pathlib import Path

import numpy as np


ROOT = Path(__file__).resolve().parents[1]
RATE = 22050
BEAT = 60.0 / 78.0
BAR = BEAT * 4
LENGTH = 16 * BAR + 1.5
LEFT = np.zeros(round(LENGTH * RATE), dtype=np.float64)
RIGHT = np.zeros_like(LEFT)


def freq(note: str) -> float:
    names = {"C": 0, "C#": 1, "D": 2, "D#": 3, "E": 4, "F": 5,
             "F#": 6, "G": 7, "G#": 8, "A": 9, "A#": 10, "B": 11}
    pitch, octave = note[:-1], int(note[-1]); midi = (octave + 1) * 12 + names[pitch]
    return 440 * 2 ** ((midi - 69) / 12)


def note(start: float, duration: float, pitch: str, volume: float,
         voice: str = "bell", pan: float = 0.0) -> None:
    offset = int(start * RATE)
    count = min(int(duration * RATE), len(LEFT) - offset)
    if count <= 0:
        return
    t = np.arange(count) / RATE
    f = freq(pitch)
    flutter = 1 + 0.0015 * np.sin(2 * np.pi * 4.2 * t)
    phase = 2 * np.pi * f * t * flutter

    if voice == "bell":
        wave = np.sin(phase) + 0.25 * np.sin(phase * 2.01) + 0.1 * np.sin(phase * 3.98)
        env = (1 - np.exp(-t * 65)) * np.exp(-t * 2.1)
    elif voice == "pad":
        wave = np.sin(phase) + 0.19 * np.sin(phase * 2) + 0.08 * np.sin(phase * 3)
        env = np.minimum(1, t / 0.6) * np.minimum(1, (duration - t) / 1.1)
    else:  # Gentle expressive flute for the melody
        wave = np.sin(phase) + 0.075 * np.sin(phase * 2)
        env = np.minimum(1, t / 0.095) * np.minimum(1, (duration - t) / 0.25)
        wave *= 0.94 + 0.06 * np.sin(2 * np.pi * 5.1 * t)

    signal = np.maximum(0, env) * volume * wave
    LEFT[offset:offset + count] += signal * math.sqrt((1 - pan) * 0.5)
    RIGHT[offset:offset + count] += signal * math.sqrt((1 + pan) * 0.5)


# A four-chord promise motif: G - Em - C - D, with a quiet returning refrain.
CHORDS = [
    ("G3", "B3", "D4"),
    ("E3", "G3", "B3"),
    ("C3", "E3", "G3"),
    ("D3", "F#3", "A3"),
]
MELODIES = [
    [(0, "D5"), (1, "B4"), (2, "G4"), (3, "B4")],
    [(0, "E5"), (1, "D5"), (2, "B4"), (3, "G4")],
    [(0, "C5"), (1, "E5"), (2, "D5"), (3, "C5")],
    [(0, "A4"), (1, "B4"), (2, "A4"), (3, "G4")],
]

for bar in range(16):
    chord = CHORDS[bar % 4]
    begin = bar * BAR
    for j, pitch in enumerate(chord):
        note(begin, BAR + 0.35, pitch, 0.038, "pad", pan=(-0.32 + j * 0.32))
    note(begin, BEAT * 2.3, chord[0], 0.065, "pad")
    for beat in range(4):
        note(begin + beat * BEAT, BEAT * 0.9, chord[beat % 3],
             0.09 if beat == 0 else 0.060, "bell", -0.19 if beat % 2 else 0.20)
    for beat, pitch in MELODIES[bar % 4]:
        melodic = pitch
        # Each repeated phrase grows gently, as Whisper gains courage.
        strength = 0.042 + 0.007 * min(bar // 4, 3)
        note(begin + beat * BEAT + 0.08, BEAT * 0.87,
             melodic, strength, "flute", pan=0.11)

# A soft echo gives the synthesized instrument a small room's warmth.
delay1, delay2 = int(0.24 * RATE), int(0.48 * RATE)
LEFT[delay1:] += RIGHT[:-delay1].copy() * 0.13
RIGHT[delay2:] += LEFT[:-delay2].copy() * 0.075
fade = min(int(1.2 * RATE), len(LEFT))
LEFT[-fade:] *= np.linspace(1, 0, fade)
RIGHT[-fade:] *= np.linspace(1, 0, fade)
scale = max(np.max(np.abs(LEFT)), np.max(np.abs(RIGHT)), 0.001)
stereo = np.column_stack((LEFT / scale * 0.69, RIGHT / scale * 0.69))
stereo = (np.clip(stereo, -1, 1) * 32767).astype("<i2")

out = ROOT / "Music" / "WhisperPromise.ogg"
out.parent.mkdir(parents=True, exist_ok=True)
with tempfile.NamedTemporaryFile(suffix=".wav") as temp:
    with wave.open(temp.name, "wb") as wav:
        wav.setnchannels(2)
        wav.setsampwidth(2)
        wav.setframerate(RATE)
        wav.writeframes(stereo.tobytes())
    subprocess.run(["ffmpeg", "-y", "-hide_banner", "-loglevel", "error",
                    "-i", temp.name, "-codec:a", "libvorbis", "-qscale:a", "5",
                    str(out)], check=True)

print(f"Created original opening score: {out} ({out.stat().st_size:,} bytes)")
