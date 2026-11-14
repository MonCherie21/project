
using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<Button>("NewGame").Pressed += OnNewGamePressed;
		GetNode<Button>("Exit").Pressed += OnExitPressed;
		GetNode<Button>("Collections").Pressed += OnCollectionsPressed;
		GetNode<Button>("Continue").Pressed += OnContinuePressed;
		var continueButton = GetNode<Button>("Continue");
		bool hasAccounts = PlayerData.HasLocalSession;
		continueButton.Disabled = !hasAccounts;
		if (!hasAccounts)
		{
			// The menu artwork has the word CONTINUE painted on it; cover it
			// until at least one profile exists instead of leaving a dead click.
			var lockShade = new ColorRect
			{
				Color = new Color("101725ba"),
				MouseFilter = MouseFilterEnum.Ignore
			};
			lockShade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
			continueButton.AddChild(lockShade);
			var locked = new Label
			{
				Text = "START NEW GAME FIRST",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				MouseFilter = MouseFilterEnum.Ignore
			};
			locked.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
			locked.AddThemeColorOverride("font_color", new Color("f6d490"));
			locked.AddThemeFontSizeOverride("font_size", 17);
			continueButton.AddChild(locked);
		}
	}

	private void OnNewGamePressed()
	{
		if (PlayerData.AccountRegistryError)
		{
			TutorialOverlay.Show(this, "COULD NOT OPEN SAVES",
				"Your player accounts could not be read. Please back up your " +
				"save files and check them before creating another game.");
			return;
		}
		int freeSlot = PlayerData.FirstEmptyAccountSlot();
		if (freeSlot == 0)
		{
			TutorialOverlay.Show(this, "ALL THREE SLOTS ARE FULL",
				"You can keep up to three adventures. Open CONTINUE and delete " +
				"an old account if you want to make a new one.");
			return;
		}
		TutorialOverlay.Confirm(this, "START A NEW GAME?",
			"Create a new adventure in Account Slot " + freeSlot + "?\n\n" +
			"Your other saved adventures will not be removed.",
			() =>
			{
				PlayerData.PendingNewAccountSlot = freeSlot;
				GetTree().ChangeSceneToFile("res://character_creation.tscn");
			}, "CREATE GAME");
	}
	
	private void OnExitPressed()
	{
		GetTree().Quit();
	}
	
	private void OnCollectionsPressed()
	{
		GetTree().ChangeSceneToFile("res://Collection.tscn");
	}
	
	private void OnContinuePressed()
	{
		GetTree().ChangeSceneToFile("res://Continue.tscn");
	}
}
