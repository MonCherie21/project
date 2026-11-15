"""Generate original, royalty-free chiptune battle music and sound effects.

Requires numpy and ffmpeg. Run: python tools/generate_battle_audio.py
"""

from pathlib import Path
import subprocess
import tempfile
import wave
import numpy as np

ROOT = Path(__file__).resolve().parent.parent
RATE = 24000
MUSIC = ROOT / "Music"
RNG = np.random.default_rng(20261115)


def pitch(midi):
    return 440 * 2 ** ((midi - 69) / 12)


def oscillator(freq, length, kind):
    t = np.arange(length) / RATE
    phase = 2 * np.pi * freq * t
    if kind == "square":
        return np.where(np.sin(phase) >= 0, 1.0, -1.0)
    if kind == "pulse":
        return np.where((freq * t) % 1 < .24, 1.0, -1.0)
    return 2 / np.pi * np.arcsin(np.sin(phase))


def env(length, attack=.007, release=.06):
    e = np.ones(length)
    a = min(length // 3, max(1, int(RATE * attack)))
    r = min(length // 2, max(1, int(RATE * release)))
    e[:a] = np.linspace(0, 1, a)
    e[-r:] *= np.linspace(1, 0, r)
    return e


def add(mix, midi, time, duration, amp, kind):
    start = int(time * RATE)
    size = min(int(duration * RATE), len(mix) - start)
    if size > 0:
        mix[start:start + size] += amp * oscillator(pitch(midi), size, kind) * env(size)


def drum(mix, time, kind, amp):
    start = int(time * RATE)
    duration = .19 if kind == "kick" else .16 if kind == "snare" else .055
    size = min(int(duration * RATE), len(mix) - start)
    if size <= 0:
        return
    t = np.arange(size) / RATE
    if kind == "kick":
        signal = np.sin(2 * np.pi * (65 * t + 3 * (1 - np.exp(-18 * t)))) * np.exp(-22 * t)
    else:
        signal = RNG.uniform(-1, 1, size) * np.exp((-32 if kind == "snare" else -90) * t)
    mix[start:start + size] += signal * amp


def wav(path, data):
    path.parent.mkdir(parents=True, exist_ok=True)
    data = np.nan_to_num(data)
    peak = np.max(np.abs(data))
    if peak:
        data = data / peak * .76
    with wave.open(str(path), "wb") as out:
        out.setnchannels(1)
        out.setsampwidth(2)
        out.setframerate(RATE)
        out.writeframes((np.clip(data, -1, 1) * 32767).astype("<i2").tobytes())


def theme():
    beat = 60 / 128
    mix = np.zeros(int(16 * 4 * beat * RATE))
    # A minor -> F -> D minor -> E major: tense, adventurous, never too busy.
    chords = [(45, [57, 60, 64, 69]), (41, [53, 57, 60, 65]),
              (38, [50, 53, 57, 62]), (40, [52, 56, 59, 64])]
    leads = [[76, 79, 81, 79, 76, 72, 74, 76],
             [77, 76, 72, 69, 72, 76, 77, 76],
             [74, 77, 81, 77, 74, 72, 69, 72],
             [76, 80, 83, 80, 76, 74, 71, 76]]
    for bar in range(16):
        bass, chord = chords[bar % 4]
        start = bar * beat * 4
        for step in range(16):
            note = chord[[0, 1, 2, 1, 3, 1, 2, 1][step % 8]] + 12
            add(mix, note, start + step * beat / 4, beat * .21, .045, "pulse")
        for step in range(4):
            add(mix, bass - 12 + (7 if step == 2 else 0), start + step * beat,
                beat * .78, .16, "tri")
            drum(mix, start + step * beat, "kick", .14 if step in (0, 2) else .07)
            if step in (1, 3):
                drum(mix, start + step * beat, "snare", .11)
        for step in range(8):
            drum(mix, start + step * beat / 2, "hat", .035)
            note = leads[bar % 4][step] + (12 if bar % 4 == 3 and step == 7 else 0)
            add(mix, note, start + step * beat / 2, beat * .45, .13, "square")
    fade = int(RATE * .07)
    mix[:fade] *= np.linspace(0, 1, fade)
    mix[-fade:] *= np.linspace(1, 0, fade)
    with tempfile.TemporaryDirectory() as tmp:
        source = Path(tmp) / "battle.wav"
        wav(source, mix)
        subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", str(source),
                        "-c:a", "libvorbis", "-qscale:a", "4",
                        str(MUSIC / "BattleTheme.ogg")], check=True)


def sweep(start, end, duration, kind="square"):
    size = int(RATE * duration)
    t = np.arange(size) / RATE
    f = start * (end / start) ** (t / duration)
    phase = 2 * np.pi * np.cumsum(f) / RATE
    signal = np.where(np.sin(phase) > 0, 1., -1.) if kind == "square" else \
        2 / np.pi * np.arcsin(np.sin(phase))
    return signal * env(size, .003, .075)


def effects():
    sounds = {
        "AttackNormal.wav": sweep(450, 170, .20, "tri"),
        "AttackUltimate.wav": sweep(170, 900, .48),
        "EnemyAttack.wav": sweep(305, 125, .30),
        "Hit.wav": RNG.uniform(-1, 1, int(RATE * .11)) *
                   np.exp(-40 * np.arange(int(RATE * .11)) / RATE),
        "ChargeReady.wav": np.concatenate([sweep(523, 659, .11, "tri"),
                                            sweep(659, 880, .15, "tri")]),
        "Victory.wav": np.concatenate([sweep(f, f, d, "tri") for f, d in
                                       [(440, .13), (554, .13), (659, .13), (880, .36)]]),
        "Defeat.wav": np.concatenate([sweep(440, 330, .20, "tri"),
                                      sweep(330, 220, .24, "tri")]),
        "Escape.wav": sweep(550, 120, .42, "tri")
    }
    for name, signal in sounds.items():
        wav(MUSIC / "Sfx" / name, signal)


if __name__ == "__main__":
    theme()
    effects()
    print("Created one original battle chiptune and eight short SFX.")
