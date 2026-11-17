using Godot;

public class CharacterData
{
	public string Name { get; set; }
	public int Hp { get; set; }
	public int Attack { get; set; }
	public int Level { get; set; }
	public int RsLevel { get; set; }
	public Texture2D Portrait { get; set; }
	public bool IsUnlocked { get; set; }
}
