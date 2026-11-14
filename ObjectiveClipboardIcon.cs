using Godot;

// Drawn in Godot so the objective icon stays crisp at pixel-art resolution.
public partial class ObjectiveClipboardIcon : Control
{
	public override void _Draw()
	{
		Color outline = new Color("14202b");
		Color frame = new Color("416b72");
		Color paper = new Color("f1e5bd");
		Color ink = new Color("41616d");

		DrawRect(new Rect2(7, 5, 47, 55), outline);
		DrawRect(new Rect2(10, 8, 41, 49), frame);
		DrawRect(new Rect2(15, 15, 31, 36), paper);
		DrawRect(new Rect2(23, 3, 15, 13), outline);
		DrawRect(new Rect2(26, 5, 9, 8), new Color("f0c85c"));

		DrawRect(new Rect2(20, 23, 22, 3), ink);
		DrawRect(new Rect2(20, 31, 18, 3), ink);
		DrawRect(new Rect2(20, 39, 22, 3), ink);
		DrawRect(new Rect2(20, 46, 12, 2), ink);
	}
}
