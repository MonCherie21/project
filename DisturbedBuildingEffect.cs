using Godot;

// Pixel-art damage and litter layered over the ORIGINAL artwork. Clearing
// level N removes that building's effect the next time world.tscn is shown.
// This leaves the map PNG and every building hitbox completely unchanged.
public partial class DisturbedBuildingEffect : Node2D
{
    public Vector2 BuildingSize { get; set; } = new Vector2(200, 160);
    public int Variant { get; set; }

    public override void _Ready() => QueueRedraw();

    public override void _Draw()
    {
        float w = BuildingSize.X;
        float h = BuildingSize.Y;
        Vector2 topLeft = -BuildingSize / 2;
        var soot = new Color("17212ab5");
        var deep = new Color("342439d6");
        var pale = new Color("ada4a2");
        var violet = new Color("6a4b83");
        var rng = new RandomNumberGenerator { Seed = (ulong)(Variant * 192847 + 301) };

        // Damaged walls: uneven grime, hairline cracks, boarded-up windows.
        for (int i = 0; i < 8; i++)
        {
            float x = rng.RandfRange(5, w - 14);
            float y = rng.RandfRange(7, h * 0.59f);
            DrawRect(new Rect2(topLeft + new Vector2(x, y),
                new Vector2(rng.RandiRange(5, 18), rng.RandiRange(2, 7))),
                i % 2 == 0 ? soot : new Color("392f3b8a"));
        }
        for (int i = 0; i < 4; i++)
        {
            Vector2 crack = topLeft + new Vector2(rng.RandfRange(18, w - 16),
                rng.RandfRange(18, h * 0.58f));
            DrawLine(crack, crack + new Vector2(4, 10), deep, 2);
            DrawLine(crack + new Vector2(4, 10),
                crack + new Vector2(-2, 17), deep, 2);
            DrawLine(crack + new Vector2(4, 10),
                crack + new Vector2(10, 13), deep, 2);
        }

        // A few crooked planks hint that Yokai have disturbed the shops.
        float plankY = topLeft.Y + h * 0.37f;
        DrawLine(new Vector2(-w * 0.25f, plankY),
            new Vector2(w * 0.16f, plankY + 11), new Color("5b443c"), 6);
        DrawLine(new Vector2(-w * 0.25f, plankY + 2),
            new Vector2(w * 0.16f, plankY + 13), new Color("251e23"), 1);

        // Scattered paper, rubble, and fallen leaves around the doorstep.
        for (int i = 0; i < 17; i++)
        {
            float x = rng.RandfRange(-w * 0.60f, w * 0.60f);
            float y = rng.RandfRange(h * 0.35f, h * 0.60f);
            var color = i % 4 == 0 ? violet : i % 3 == 0 ? pale : soot;
            DrawRect(new Rect2(x, y, rng.RandiRange(4, 11),
                rng.RandiRange(2, 5)), color);
            if (i % 4 == 0)
                DrawLine(new Vector2(x, y), new Vector2(x + 7, y + 3), deep, 1);
        }

        // Faint supernatural traces; the clean art reappears on victory.
        DrawCircle(new Vector2(-w * 0.25f, -h * 0.23f), 12,
            new Color(0.35f, 0.22f, 0.50f, 0.16f));
        DrawCircle(new Vector2(w * 0.31f, h * 0.05f), 9,
            new Color(0.45f, 0.25f, 0.53f, 0.17f));
    }
}
