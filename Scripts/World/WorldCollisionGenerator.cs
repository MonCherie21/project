using Godot;

// Keeps the player within the map boundaries only.
// Building collisions are editable StaticBody2D nodes in world.tscn.
public partial class WorldCollisionGenerator : Node2D
{
    public override void _Ready()
    {
        var map = GetParent().GetNode<Sprite2D>("MapBackground");
        BuildMapEdges(map);
    }

    private void BuildMapEdges(Sprite2D map)
    {
        Vector2 textureSize = map.Texture.GetSize();
        Vector2 scale = AbsScale(map.GlobalScale);
        Vector2 extent = new Vector2(textureSize.X * scale.X, textureSize.Y * scale.Y);
        Vector2 origin = map.GlobalPosition;
        const float wall = 36.0f;

        AddWall("MapEdgeLeft", origin + new Vector2(-extent.X / 2 - wall / 2, 0),
            new Vector2(wall, extent.Y + wall * 2));
        AddWall("MapEdgeRight", origin + new Vector2(extent.X / 2 + wall / 2, 0),
            new Vector2(wall, extent.Y + wall * 2));
        AddWall("MapEdgeTop", origin + new Vector2(0, -extent.Y / 2 - wall / 2),
            new Vector2(extent.X + wall * 2, wall));
        AddWall("MapEdgeBottom", origin + new Vector2(0, extent.Y / 2 + wall / 2),
            new Vector2(extent.X + wall * 2, wall));
    }

    private static Vector2 AbsScale(Vector2 scale) =>
        new Vector2(Mathf.Abs(scale.X), Mathf.Abs(scale.Y));

    private void AddWall(string wallName, Vector2 worldPosition, Vector2 size)
    {
        var body = new StaticBody2D
        {
            Name = wallName,
            Position = ToLocal(worldPosition),
            CollisionLayer = 1,
            CollisionMask = 0,
            InputPickable = false // Keep building mouse-click dialogue working.
        };

        body.AddChild(new CollisionShape2D
        {
            Shape = new RectangleShape2D { Size = size }
        });
        AddChild(body);
    }
}
