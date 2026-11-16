using Godot;
using System;

// Corruption is painted over the original building sprite; no source art,
// collision bodies, triggers or navigation shapes are modified.
public partial class DisturbedBuildingEffect : Node2D
{
    public Vector2 BuildingSize { get; set; } = new(200, 160);
    public int Variant { get; set; } = 1;
    public bool Restored { get; set; }

    private double _time;
    private double _redrawTimer;

    private static readonly Color[] Curses =
    {
        new("8f607b"), new("936288"), new("b06e51"),
        new("b85c48"), new("b3545a"), new("a75175"), new("8a9a53")
    };

    public override void _Process(double delta)
    {
        _time += delta;
        _redrawTimer += delta;
        if (_redrawTimer < 1.0 / 12.0) return;
        _redrawTimer = 0;
        QueueRedraw();
    }

    public override void _Draw()
    {
        float w = BuildingSize.X;
        float h = BuildingSize.Y;
        if (w < 32 || h < 32) return;
        if (Restored)
        {
            DrawRestored(w, h);
            return;
        }

        Color cursed = Curses[Math.Clamp(Variant - 1, 0, Curses.Length - 1)];
        Color blot = new("14171cc4");
        Color soot = new("242024bc");
        Color ash = new("a8a18f");
        Color wood = new("4c3428");
        var rng = new RandomNumberGenerator { Seed = (ulong)(Variant * 918273 + 117) };
        Vector2 top = new(-w / 2, -h / 2);

        // Deep soot pools under the eaves, beside the door and around broken
        // windows; irregular overlapping blocks keep the artwork pixelated.
        for (int i = 0; i < 13; i++)
        {
            float x = rng.RandfRange(7, w - 29);
            float y = rng.RandfRange(5, h * 0.67f);
            float blockW = rng.RandfRange(11, 31);
            float blockH = rng.RandfRange(5, 15);
            DrawRect(new Rect2(top + new Vector2(x, y), new Vector2(blockW, blockH)),
                i % 3 == 0 ? new Color(0.22f, 0.14f, 0.16f, 0.55f) : blot);
            DrawRect(new Rect2(top + new Vector2(x + 4, y + blockH),
                new Vector2(blockW * 0.35f, 4)), soot);
        }

        // Broken, boarded openings. The beams intentionally sit on the upper
        // building façade rather than across the sidewalk or collision area.
        for (int i = 0; i < 2; i++)
        {
            float x = (i == 0 ? -0.29f : 0.13f) * w;
            float y = -h * (i == 0 ? 0.19f : 0.28f);
            DrawRect(new Rect2(x, y, w * 0.19f, h * 0.16f), new Color("15191fc4"));
            DrawLine(new Vector2(x - 4, y + 3),
                new Vector2(x + w * 0.19f + 4, y + h * 0.16f), wood, 7, false);
            DrawLine(new Vector2(x - 4, y + 4),
                new Vector2(x + w * 0.19f + 4, y + h * 0.16f + 1),
                new Color("907258"), 2, false);
            DrawRect(new Rect2(x + 5, y + 5, 4, 4), new Color("c09e67"));
        }

        // Jagged curse fractures, dark inside with colored edges; unlike
        // straight generic cracks these branch and spread between openings.
        for (int i = 0; i < 5; i++)
        {
            Vector2 a = top + new Vector2(rng.RandfRange(17, w - 22),
                rng.RandfRange(17, h * 0.64f));
            Vector2 b = a + new Vector2(rng.RandfRange(-7, 6), rng.RandfRange(10, 21));
            Vector2 c = b + new Vector2(rng.RandfRange(-12, 11), rng.RandfRange(8, 17));
            DrawLine(a, b, new Color("090d14d9"), 4, false);
            DrawLine(b, c, new Color("090d14d9"), 4, false);
            DrawLine(a + new Vector2(2, 0), b + new Vector2(2, 0),
                cursed.Darkened(0.12f), 1.5f, false);
            DrawLine(b, b + new Vector2(rng.RandfRange(7, 14), 4),
                cursed.Darkened(0.23f), 2, false);
        }

        // A luminous, scratched Yokai seal tells the player at a glance that
        // this particular place has NOT been cleansed yet.
        Vector2 sigil = new(w * 0.30f, -h * 0.15f);
        float pulse = 0.65f + 0.25f * (float)Math.Sin(_time * 2.1 + Variant);
        Color aura = new(cursed.R, cursed.G, cursed.B, pulse * 0.34f);
        DrawRect(new Rect2(sigil - new Vector2(23, 23), new Vector2(46, 46)), aura);
        Color line = new(cursed.R, cursed.G, cursed.B, pulse);
        Vector2 topPoint = sigil + new Vector2(0, -19);
        Vector2 rightPoint = sigil + new Vector2(16, 0);
        Vector2 bottomPoint = sigil + new Vector2(0, 19);
        Vector2 leftPoint = sigil + new Vector2(-16, 0);
        DrawLine(topPoint, rightPoint, line, 3, false);
        DrawLine(rightPoint, bottomPoint, line, 3, false);
        DrawLine(bottomPoint, leftPoint, line, 3, false);
        DrawLine(leftPoint, topPoint, line, 3, false);
        DrawLine(sigil + new Vector2(0, -11), sigil + new Vector2(0, 10), line, 3, false);
        DrawLine(sigil + new Vector2(-7, 2), sigil + new Vector2(7, 2), line, 3, false);

        // Inky ectoplasm leaking into the road, fallen roof tiles, scraps of
        // paper, ash and shattered plaster at the entrance.
        DrawRect(new Rect2(-w * 0.45f, h * 0.40f, w * 0.9f, h * 0.085f),
            new Color("1019198c"));
        for (int i = 0; i < 29; i++)
        {
            float x = rng.RandfRange(-w * 0.57f, w * 0.56f);
            float y = rng.RandfRange(h * 0.36f, h * 0.66f);
            Vector2 size = new(rng.RandfRange(5, 16), rng.RandfRange(3, 8));
            Color debris = i % 5 == 0 ? cursed.Darkened(0.45f)
                : i % 4 == 0 ? ash : i % 3 == 0 ? wood : soot;
            DrawRect(new Rect2(x, y, size.X, size.Y), debris);
        }

        // Small rising spirits and shadow wisps make the infection feel alive
        // without a shader or additional textures and without obscuring text.
        for (int i = 0; i < 6; i++)
        {
            float x = -w * 0.38f + i * w * 0.145f;
            float rise = (float)((_time * (10 + i % 3 * 5) + i * 19) % (h * 0.55f));
            float y = h * 0.43f - rise;
            float sway = (float)Math.Sin(_time * 1.35f + i * 1.7f) * 5;
            Color vapor = new(cursed.R, cursed.G, cursed.B,
                0.22f + 0.13f * (float)Math.Sin(_time * 1.6f + i));
            DrawRect(new Rect2(x + sway, y, 10, 6), vapor);
            DrawRect(new Rect2(x + sway + 3, y - 5, 5, 6), vapor);
        }
    }

    private void DrawRestored(float w, float h)
    {
        float pulse = 0.58f + 0.24f * (float)Math.Sin(_time * 1.8f + Variant);
        Color sparkle = new(0.55f, 0.88f, 0.69f, pulse);
        // A quiet, clean lantern and three healing motes. No soot, boards,
        // curse symbol or road trash once the building's battle is won.
        DrawRect(new Rect2(-w * 0.03f, h * 0.28f, 7, 7),
            new Color(0.65f, 0.95f, 0.71f, pulse * 0.4f));
        DrawRect(new Rect2(-w * 0.02f, h * 0.29f, 3, 3), sparkle);
        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * w * 0.29f;
            float y = -h * 0.19f + (float)Math.Sin(_time * 1.8f + i) * 3;
            DrawRect(new Rect2(x, y, 4, 8), sparkle);
            DrawRect(new Rect2(x - 2, y + 2, 8, 4), sparkle);
        }
    }
}
