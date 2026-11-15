using Godot;

// Non-destructive, pixel-style damage over each battle location. World.cs
// removes the related building's damage after the Yokai has been defeated.
public partial class DisturbedBattleBackdrop : Control
{
    public override void _Ready()
    {
        Size = GetViewportRect().Size;
        MouseFilter = MouseFilterEnum.Ignore;
        QueueRedraw();
    }

    public override void _Draw()
    {
        float w = Size.X;
        float h = Size.Y;
        var dust = new Color("16132266");
        var crack = new Color("27212b99");
        var paper = new Color("b9b4af99");
        var rng = new RandomNumberGenerator { Seed = 438012 };

        // Frame the sides so creatures and the attack UI stay readable.
        DrawRect(new Rect2(0, 0, w, 28), dust);
        DrawRect(new Rect2(0, h - 44, w, 44), dust);
        DrawRect(new Rect2(0, 0, 24, h), dust);
        DrawRect(new Rect2(w - 24, 0, 24, h), dust);
        for (int i = 0; i < 23; i++)
        {
            bool left = i % 2 == 0;
            float x = left ? rng.RandfRange(20, w * 0.27f)
                           : rng.RandfRange(w * 0.73f, w - 22);
            float y = rng.RandfRange(36, h - 64);
            DrawLine(new Vector2(x, y), new Vector2(x + 7, y + 19), crack, 3);
            DrawLine(new Vector2(x + 7, y + 19),
                new Vector2(x - 3, y + 27), crack, 2);
        }
        for (int i = 0; i < 28; i++)
        {
            float x = rng.RandfRange(22, w - 25);
            float y = rng.RandfRange(h - 57, h - 14);
            DrawRect(new Rect2(x, y, rng.RandiRange(7, 17),
                rng.RandiRange(3, 6)), i % 4 == 0 ? paper : dust);
        }
    }
}
