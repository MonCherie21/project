using Godot;

// Builds physics walls from the existing world.tscn sprite positions.
// If the team moves a building in Godot, its collision follows automatically.
public partial class WorldCollisionGenerator : Node2D
{
    public override void _Ready()
    {
        var map = GetParent().GetNode<Sprite2D>("MapBackground");
        BuildMapEdges(map);

        var buildings = GetParent().GetNode<Node2D>("MapBackground/Buildings");
        foreach (Node child in buildings.GetChildren())
        {
            if (child is Sprite2D sprite && sprite.Texture != null && sprite.RegionEnabled)
            {
                // These two buildings have editable StaticBody2D hitboxes in
                // world.tscn. Do not generate a second obstacle on top.
                string name = sprite.Name.ToString();
                if (name == "SariSariStore" || name == "GasStation")
                    continue;

                BuildBuildingObstacle(sprite);
            }
        }
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

    private void BuildBuildingObstacle(Sprite2D building)
    {
        Vector2 regionSize = building.RegionRect.Size;
        Vector2 scale = AbsScale(building.GlobalScale);

        // All remaining buildings retain the original automatically sized wall.
        const float widthRatio = 0.92f;
        const float heightRatio = 0.90f;
        Vector2 colliderSize = new Vector2(
            Mathf.Max(20, regionSize.X * scale.X * widthRatio),
            Mathf.Max(20, regionSize.Y * scale.Y * heightRatio));

        Vector2 center = building.GlobalPosition;
        AddWall(building.Name.ToString() + "_Solid", center, colliderSize);
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
