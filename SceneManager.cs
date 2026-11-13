using Godot;

public partial class SceneManager : Node
{
	// Dito natin ise-save ang path ng huling scene bago mag-pause
	public string LastScenePath { get; private set; } = "";

	public void LoadScene(string path)
	{
		LastScenePath = GetTree().CurrentScene.SceneFilePath;
		GetTree().ChangeSceneToFile(path);
	}

	public void GoBack()
	{
		if (!string.IsNullOrEmpty(LastScenePath))
		{
			GetTree().ChangeSceneToFile(LastScenePath);
		}
		else
		{
			// Fallback kung sakaling walang naka-save (hal. nag-start directly sa Pause Menu)
			GetTree().ChangeSceneToFile("res://main.tscn"); 
		}
	}
}
