using Godot;
using System;
using System.Threading.Tasks;

// Retro square-pixel effects for all seven encounters. No sprite-sheet
// dependencies: the same effects work with every existing Yokai PNG.
public partial class BattlePixelEffects : Node2D
{
    private static readonly Color[] StageColors =
    {
        new("80b8ff"),  // Whisper: spirit blue
        new("b5a2ff"),  // Tattletell: violet
        new("ffdc81"),  // bright electric
        new("ff9163"),  // Draggie: orange fire
        new("ff6684"),  // crimson streaks
        new("5be2d5"),  // Wobblewok: deep aqua
        new("b0ee86")   // Orcanos: earth green
    };

    private readonly RandomNumberGenerator _rng = new();
    private Color _palette;
    private int _stage = 1;

    public void SetStage(int stage)
    {
        _stage = Math.Clamp(stage, 1, StageColors.Length);
        _palette = StageColors[_stage - 1];
        _rng.Randomize();
    }

    private Polygon2D Pixel(Vector2 at, Color tint, float size)
    {
        var pixel = new Polygon2D
        {
            Position = at,
            Color = tint,
            Polygon = new[]
            {
                new Vector2(-size, -size), new Vector2(size, -size),
                new Vector2(size, size), new Vector2(-size, size)
            }
        };
        AddChild(pixel);
        return pixel;
    }

    private void Drift(Polygon2D pixel, Vector2 destination, float seconds)
    {
        Tween tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(pixel, "position", destination, seconds);
        tween.Parallel().TweenProperty(pixel, "modulate:a", 0.0f, seconds);
        tween.Finished += pixel.QueueFree;
    }

    public void Burst(Vector2 center, bool ultimate = false, bool hostile = false)
    {
        Color color = hostile ? _palette.Lerp(new Color("ff7e92"), 0.35f) : _palette;
        int count = ultimate ? 22 : 9;
        float radius = ultimate ? 66 : 34;
        for (int i = 0; i < count; i++)
        {
            float angle = Mathf.Tau * i / count + _rng.RandfRange(-0.2f, 0.2f);
            float travel = _rng.RandfRange(radius * 0.55f, radius);
            Vector2 target = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * travel;
            var pixel = Pixel(center, i % 4 == 0 ? Colors.White : color,
                _rng.RandfRange(2.5f, ultimate ? 7.5f : 5.0f));
            Drift(pixel, target, _rng.RandfRange(0.22f, ultimate ? 0.58f : 0.36f));
        }
    }

    public async Task Charge(Vector2 center)
    {
        // The charging ring compresses toward the attacker and brightens.
        const int count = 12;
        for (int i = 0; i < count; i++)
        {
            float angle = Mathf.Tau * i / count;
            Vector2 offset = new(Mathf.Cos(angle), Mathf.Sin(angle));
            var pixel = Pixel(center + offset * 57, i % 3 == 0 ? Colors.White : _palette, 5);
            Drift(pixel, center + offset * 13, 0.36f);
        }
        await ToSignal(GetTree().CreateTimer(0.29), SceneTreeTimer.SignalName.Timeout);
    }

    public async Task Projectile(Vector2 start, Vector2 end, bool ultimate)
    {
        // The seven Yokai use the same reliable tween pipeline, but produce
        // distinctly shaped attacks: spirit, peck, spark, fire, crimson,
        // wave, and heavy earth chunks, respectively.
        int count = ultimate ? 5 : 2;
        for (int i = 0; i < count; i++)
        {
            float spread = i - (count - 1) / 2f;
            Vector2 sideways = _stage switch
            {
                1 => new Vector2(0, Mathf.Sin(i * 1.1f) * 11),
                2 => new Vector2(spread * 5, spread * -10),
                3 => new Vector2(0, spread * 13),
                4 => new Vector2(-i * 6, spread * 6),
                5 => new Vector2(-i * 4, (i % 2 == 0 ? -1 : 1) * 10),
                6 => new Vector2(spread * 4, Mathf.Sin(i * 2f) * 16),
                _ => new Vector2(-i * 8, spread * 5)
            };
            var pixel = Pixel(start + sideways, i % 2 == 0 ? _palette : Colors.White,
                _stage == 7 ? 11f : ultimate ? 10f - i : 5f);
            Drift(pixel, end + sideways, ultimate ? .23f : .14f);
        }
        await ToSignal(GetTree().CreateTimer(ultimate ? 0.22 : 0.13),
            SceneTreeTimer.SignalName.Timeout);
        Burst(end, ultimate);
    }
}
