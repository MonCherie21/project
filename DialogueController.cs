using Godot;

public partial class DialogueController : Node
{
	private int currentDialogue = 1;
	private const int maxDialogue = 7;

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_accept") || @event.IsActionPressed("ui_right"))
		{
			if (currentDialogue < maxDialogue)
			{
				currentDialogue++;
				LoadDialogueScene(currentDialogue);
			}
		}
		else if (@event.IsActionPressed("ui_left"))
		{
			if (currentDialogue > 1)
			{
				currentDialogue--;
				LoadDialogueScene(currentDialogue);
			}
		}
	}

	private void LoadDialogueScene(int dialogueNumber)
	{
		string scenePath = $"res://Dialogue{dialogueNumber}.tscn";
		
		GetTree().ChangeSceneToFile(scenePath);
	}
}
