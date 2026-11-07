using Godot;

public partial class EFeed1 : Node2D
{
	// =========================================
	// 1. NODE REFERENCES
	// =========================================

	private TextureButton CandyFeed;
	private Control CandyCount;

	private Range friendshipLvl_bar;
	private Control friendshipLvl;

	private CanvasItem M1Front;

	// =========================================
	// 2. VARIABLES
	// =========================================

	private int candyCount = 5;
	private int friendshipLevel = 0; // Starts at Level 0, maxes out at Level 3

	private const double FriendshipIncrease = 50.0;

	private Tween flashTween;

	// =========================================
	// 3. INITIALIZATION
	// =========================================

	public override void _Ready()
	{
		// Get nodes using their actual Scene Tree paths
		CandyFeed = GetNode<TextureButton>("CanvasLayer/CandyFeed");
		CandyCount = GetNode<Control>("CanvasLayer/CandyCount");
		friendshipLvl_bar = GetNode<Range>("friendshipLvl_bar");
		
		// friendshipLvl is a child of friendshipLvl_bar
		friendshipLvl = GetNode<Control>("friendshipLvl_bar/friendshipLvl");

		if (friendshipLvl == null)
		{
			GD.PrintErr("CRITICAL ERROR: friendshipLvl node could not be found!");
		}
		else
		{
			GD.Print("friendshipLvl node found successfully: " + friendshipLvl.Name);
		}

		M1Front = GetNode<CanvasItem>("M1Front");

		// Initialize values
		candyCount = 5;
		friendshipLevel = 0;

		friendshipLvl_bar.MinValue = 0;
		friendshipLvl_bar.MaxValue = 100;
		friendshipLvl_bar.Value = 0;

		UpdateCandyCount();
		UpdateFriendshipLevel();

		// Button settings
		CandyFeed.Disabled = false;
		CandyFeed.MouseFilter = Control.MouseFilterEnum.Stop;
		CandyCount.MouseFilter = Control.MouseFilterEnum.Ignore;

		// Prevent editing if it's left as a TextEdit
		if (CandyCount is TextEdit candyText)
		{
			candyText.Editable = false;
		}

		if (friendshipLvl is TextEdit friendshipText)
		{
			friendshipText.Editable = false;
		}

		// Connect button signal
		CandyFeed.Pressed += OnCandyFeedPressed;

		GD.Print("FEEDING SYSTEM READY!");
	}

	// =========================================
	// 4. UPDATE CANDY COUNT
	// =========================================

	private void UpdateCandyCount()
	{
		string countText = $"x{candyCount:00}";

		if (CandyCount is Label label)
		{
			label.Text = countText;
		}
		else if (CandyCount is TextEdit textEdit)
		{
			textEdit.Text = countText;
		}
	}

	// =========================================
	// 5. UPDATE FRIENDSHIP LEVEL
	// =========================================

	private void UpdateFriendshipLevel()
	{
		string levelText = $"Level {friendshipLevel:00}";

		// Supports both Label and TextEdit nodes seamlessly
		if (friendshipLvl is Label label)
		{
			label.Text = levelText;
		}
		else if (friendshipLvl is TextEdit textEdit)
		{
			textEdit.Text = levelText;
		}
	}

	// =========================================
	// 6. FEED BUTTON
	// =========================================

	private void OnCandyFeedPressed()
	{
		if (friendshipLevel >= 3)
		{
			GD.Print("FRIENDSHIP IS ALREADY AT MAX LEVEL (3)!");
			CandyFeed.Disabled = true;
			return;
		}

		if (candyCount <= 0)
		{
			GD.Print("NO CANDIES LEFT!");
			return;
		}

		candyCount--;
		UpdateCandyCount();

		FlashYokai();
		IncreaseFriendship();

		GD.Print("CANDY FED!");
		GD.Print("Remaining candies: " + candyCount);
		GD.Print("Current Level: " + friendshipLevel);
		GD.Print("Bar Progress: " + friendshipLvl_bar.Value);
	}

	// =========================================
	// 7. YOKAI GREEN EFFECT
	// =========================================

	private void FlashYokai()
	{
		if (flashTween != null && flashTween.IsRunning())
		{
			flashTween.Kill();
		}

		M1Front.Modulate = Colors.Green;

		flashTween = CreateTween();
		flashTween.TweenInterval(1.0);
		flashTween.TweenProperty(M1Front, "modulate", Colors.White, 0.15);
	}

	// =========================================
	// 8. FRIENDSHIP PROGRESS & LEVEL UPS
	// =========================================

	private void IncreaseFriendship()
	{
		if (friendshipLevel >= 3) return;

		friendshipLvl_bar.Value += FriendshipIncrease;

		if (friendshipLvl_bar.Value >= 100)
		{
			friendshipLevel++;

			if (friendshipLevel >= 3)
			{
				friendshipLevel = 3;
				friendshipLvl_bar.Value = 100;
				CandyFeed.Disabled = true; 
				GD.Print("MAX FRIENDSHIP LEVEL 3 REACHED! FEEDING DISABLED.");
			}
			else
			{
				friendshipLvl_bar.Value = 0; 
				GD.Print("FRIENDSHIP LEVEL UP! Current Level: " + friendshipLevel);
			}

			UpdateFriendshipLevel();
		}
	}
}
