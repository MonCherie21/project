using Godot;
using System;

// The battleground shares the same curse motif as the overworld: soot,
// fractures, ruined pavements, faint red runes and drifting spirit ash.
// Drawn between background art and fighters so skills/HP remain readable.
public partial class DisturbedBattleBackdrop : Control
{
    public int Stage { get; set; } = 1;

    private double _time;
    private double _redrawTimer;

    private static readonly Color[] Influence =
    {
        new("7a78a1"), new("86648b"), new("a27a53"),
        new("ba6b4f"), new("af4e59"), new("b34e7d"), new("789b68")
    };

    public override void _Ready()
    {
        Size = GetViewportRect().Size;
        MouseFilter = MouseFilterEnum.Ignore;
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        _time += delta;
        _redrawTimer += delta;
        if (_redrawTimer < 1.0 / 15.0) return;
        _redrawTimer = 0;
        QueueRedraw();
    }

    public override void _Draw()
    {
        float w = Size.X;
        float h = Size.Y;
        Color curse = Influence[Math.Clamp(Stage - 1, 0, Influence.Length - 1)];
        Color soot = new("101417ad");
        Color fracture = new("171219c2");
        var rng = new RandomNumberGenerator { Seed = (ulong)(Stage * 19571 + 438012) };

        // The center stays visible for the trainers, fighters, and HP bars.
        // Damage clusters at the walls and ground, where it reads at 720p.
        DrawRect(new Rect2(0, 0, w, 29), soot);
        DrawRect(new Rect2(0, h - 39, w, 39), soot);
        DrawRect(new Rect2(0, 0, 30, h), soot);
        DrawRect(new Rect2(w - 30, 0, 30, h), soot);
        for (int i = 0; i < 12; i++)
        {
            float x = i % 2 == 0 ? 30 : w - 50;
            float y = 45 + i * (h - 95) / 12;
            DrawRect(new Rect2(x, y, rng.RandfRange(13, 36), rng.RandfRange(5, 18)),
                new Color(0.13f, 0.10f, 0.16f, 0.43f));
        }

        // Uneven branching fractures on both sides; they look different in
        // every level because stage is part of the deterministic seed.
        for (int i = 0; i < 25; i++)
        {
            bool left = i % 2 == 0;
            float x = left ? rng.RandfRange(22, w * 0.22f)
                           : rng.RandfRange(w * 0.78f, w - 22);
            float y = rng.RandfRange(38, h - 57);
            Vector2 start = new(x, y);
            Vector2 bend = start + new Vector2(rng.RandfRange(-10, 8), 15);
            Vector2 end = bend + new Vector2(rng.RandfRange(-16, 14), 19);
            DrawLine(start, bend, fracture, 4, false);
            DrawLine(bend, end, fracture, 4, false);
            DrawLine(start + new Vector2(2, 0), bend + new Vector2(2, 0),
                new Color(curse.R, curse.G, curse.B, 0.33f), 1.5f, false);
        }

        // Torn litter, fallen bricks and ectoplasm smears on the foreground.
        for (int i = 0; i < 40; i++)
        {
            float x = rng.RandfRange(33, w - 40);
            float y = rng.RandfRange(h - 70, h - 22);
            Color debris = i % 6 == 0 ? new Color("b2a18e94")
                : i % 3 == 0 ? new Color(curse.R, curse.G, curse.B, 0.42f) : soot;
            DrawRect(new Rect2(x, y, rng.RandfRange(8, 24),
                rng.RandfRange(3, 8)), debris);
        }

        // A pair of dim but visible curse seals in the side margins.
        // Their slow breathing echoes the infected overworld buildings.
        float glow = 0.39f + 0.24f * (float)Math.Sin(_time * 2.2 + Stage);
        Rune(new Vector2(72, h * 0.58f), curse, glow);
        Rune(new Vector2(w - 76, h * 0.54f), curse, glow * 0.86f);

        // Drifting rectangular spirit fragments are intentionally sparse.
        for (int i = 0; i < 18; i++)
        {
            float x = 45 + i * (w - 90) / 18f;
            float y = (float)((i * 43 + _time * (13 + i % 5 * 4)) % (h - 85));
            Color dust = new(curse.R, curse.G, curse.B, i % 3 == 0 ? 0.39f : 0.19f);
            DrawRect(new Rect2(x + (float)Math.Sin(_time + i) * 8,
                h - 38 - y, i % 3 == 0 ? 6 : 3, 4), dust);
        }
    }

    private void Rune(Vector2 center, Color hue, float alpha)
    {
        Color line = new(hue.R, hue.G, hue.B, alpha);
        DrawRect(new Rect2(center - new Vector2(24, 24), new Vector2(48, 48)),
            new Color(hue.R, hue.G, hue.B, alpha * 0.25f));
        DrawLine(center + new Vector2(0, -21), center + new Vector2(18, 0), line, 3, false);
        DrawLine(center + new Vector2(18, 0), center + new Vector2(0, 21), line, 3, false);
        DrawLine(center + new Vector2(0, 21), center + new Vector2(-18, 0), line, 3, false);
        DrawLine(center + new Vector2(-18, 0), center + new Vector2(0, -21), line, 3, false);
        DrawLine(center + new Vector2(0, -12), center + new Vector2(0, 12), line, 3, false);
        DrawLine(center + new Vector2(-8, 1), center + new Vector2(8, 1), line, 3, false);
    }
}
