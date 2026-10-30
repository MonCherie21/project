using Godot;
using System;
using System.Threading.Tasks;

public partial class BattleScene : Node2D
{
	// =========================================
	// 1. HP SETTINGS
	// =========================================

	[Export] public int PlayerMaxHP = 100;
	[Export] public int YokaiMaxHP = 100;
	[Export] public int EnemyMaxHP = 100;

	private int playerHP;
	private int yokaiHP;
	private int enemyHP;


	// =========================================
	// 2. DAMAGE AND HEALING
	// =========================================

	[Export] public int SlapDamage = 5;
	[Export] public int GhostPunchDamage = 10;

	[Export] public int EnemyDamage = 8;
	[Export] public int FeedHealing = 20;


	// =========================================
	// 3. FRIENDSHIP DIFFICULTY
	// =========================================

	[Export] public int FriendshipLevel = 0;


	// =========================================
	// 4. BATTLE STATE
	// =========================================

	private bool battleFinished = false;
	private bool playerTurn = true;

	private readonly Random random = new Random();


	// =========================================
	// 5. SPRITES
	// =========================================

	private Sprite2D playerSprite;
	private Sprite2D friendlyYokaiSprite;
	private Sprite2D enemySprite;

	private Color playerOriginalModulate = Colors.White;
	private Color friendlyOriginalModulate = Colors.White;
	private Color enemyOriginalModulate = Colors.White;


	// =========================================
	// 6. BUTTONS
	// =========================================

	private TextureButton slapButton;
	private TextureButton ghostPunchButton;

	private BaseButton attackButton;

	private Button feedButton;
	private Button runButton;


	// =========================================
	// 7. HP BARS
	// =========================================

	private Godot.Range playerHPBar;
	private Godot.Range yokaiHPBar;
	private Godot.Range enemyHPBar;


	// =========================================
	// 8. HP LABELS
	// =========================================

	private Label playerHPText;
	private Label yokaiHPText;
	private Label enemyHPText;


	// =========================================
	// 9. CANVAS LAYER
	// =========================================

	private CanvasLayer canvasLayer;


	// =========================================
	// 10. FIND NODES RECURSIVELY
	// =========================================

	private Node FindNodeRecursive(Node root, string targetName)
	{
		if (root == null)
			return null;

		if (root.Name.ToString() == targetName)
			return root;

		foreach (Node child in root.GetChildren())
		{
			Node result = FindNodeRecursive(child, targetName);

			if (result != null)
				return result;
		}

		return null;
	}


	// =========================================
	// 11. INITIALIZE BATTLE
	// =========================================

	public override void _Ready()
	{
		battleFinished = false;
		playerTurn = true;

		// Initialize HP.
		playerHP = PlayerMaxHP;
		yokaiHP = YokaiMaxHP;
		enemyHP = EnemyMaxHP;

		ApplyFriendshipDifficulty();


		// -------------------------------------
		// FIND SPRITES
		// -------------------------------------

		playerSprite =
			FindNodeRecursive(this, "P1Whole") as Sprite2D;

		friendlyYokaiSprite =
			FindNodeRecursive(this, "m1_py2") as Sprite2D;

		enemySprite =
			FindNodeRecursive(this, "m1_m") as Sprite2D;


		// Save original sprite colors.
		if (playerSprite != null)
		{
			playerOriginalModulate = playerSprite.Modulate;
		}
		else
		{
			GD.PushWarning(
				"Player Sprite2D not found. Check the name and node type of P1Whole."
			);
		}

		if (friendlyYokaiSprite != null)
		{
			friendlyOriginalModulate = friendlyYokaiSprite.Modulate;
		}
		else
		{
			GD.PushWarning(
				"Friendly Yo-kai Sprite2D not found: m1_py2."
			);
		}

		if (enemySprite != null)
		{
			enemyOriginalModulate = enemySprite.Modulate;
		}
		else
		{
			GD.PushWarning(
				"Enemy Sprite2D not found: m1_m."
			);
		}


		// -------------------------------------
		// FIND SKILL BUTTONS
		// -------------------------------------

		slapButton =
			FindNodeRecursive(this, "TextureButton") as TextureButton;

		ghostPunchButton =
			FindNodeRecursive(this, "TextureButton2") as TextureButton;


		// -------------------------------------
		// FIND ATTACK MENU BUTTON
		// -------------------------------------

		attackButton =
			FindNodeRecursive(this, "btn-attack") as BaseButton;


		// -------------------------------------
		// FIND CANVAS LAYER
		// -------------------------------------

		canvasLayer =
			FindNodeRecursive(this, "CanvasLayer") as CanvasLayer;


		// -------------------------------------
		// FIND HP BARS
		// -------------------------------------

		playerHPBar =
			FindNodeRecursive(this, "hp_p") as Godot.Range;

		yokaiHPBar =
			FindNodeRecursive(this, "hp_py") as Godot.Range;

		enemyHPBar =
			FindNodeRecursive(this, "hp_m") as Godot.Range;


		// -------------------------------------
		// FIND HP LABELS
		// -------------------------------------

		playerHPText =
			FindNodeRecursive(this, "hptext_p") as Label;

		yokaiHPText =
			FindNodeRecursive(this, "hptext_py") as Label;

		enemyHPText =
			FindNodeRecursive(this, "hptext_m") as Label;


		// -------------------------------------
		// CONNECT SLAP BUTTON
		// -------------------------------------

		if (slapButton != null)
		{
			slapButton.Pressed += OnSlapPressed;

			GD.Print(
				"Slap button found at: " + slapButton.GetPath()
			);
		}
		else
		{
			GD.PushWarning(
				"SLAP BUTTON NOT FOUND. Expected node name: TextureButton"
			);
		}


		// -------------------------------------
		// CONNECT GHOST PUNCH BUTTON
		// -------------------------------------

		if (ghostPunchButton != null)
		{
			ghostPunchButton.Pressed += OnGhostPunchPressed;

			GD.Print(
				"Ghost Punch button found at: " +
				ghostPunchButton.GetPath()
			);
		}
		else
		{
			GD.PushWarning(
				"GHOST PUNCH BUTTON NOT FOUND. Expected node name: TextureButton2"
			);
		}


		// -------------------------------------
		// CONNECT ATTACK MENU BUTTON
		// -------------------------------------

		if (attackButton != null)
		{
			attackButton.Pressed += OnAttackMenuPressed;

			GD.Print(
				"Attack menu found at: " + attackButton.GetPath()
			);
		}
		else
		{
			GD.PushWarning(
				"ATTACK MENU BUTTON NOT FOUND. Expected node name: btn-attack"
			);
		}


		// -------------------------------------
		// CREATE FEED AND RUN BUTTONS
		// -------------------------------------

		CreateActionButtons();


		// -------------------------------------
		// INITIAL BUTTON VISIBILITY
		// -------------------------------------

		ShowAttackChoices();


		// -------------------------------------
		// INITIAL HP DISPLAY
		// -------------------------------------

		UpdateAllHP();

		GD.Print("Battle initialized!");
		GD.Print("Friendship Level: " + FriendshipLevel);
		GD.Print("Enemy damage: " + EnemyDamage);
	}


	// =========================================
	// 12. FRIENDSHIP DIFFICULTY
	// =========================================

	private void ApplyFriendshipDifficulty()
	{
		if (FriendshipLevel <= 0)
		{
			EnemyDamage = 12;
		}
		else if (FriendshipLevel == 1)
		{
			EnemyDamage = 8;
		}
		else if (FriendshipLevel == 2)
		{
			EnemyDamage = 5;
		}
		else
		{
			EnemyDamage = 3;
		}
	}


	// =========================================
	// 13. CREATE FEED AND RUN BUTTONS
	// =========================================

	private void CreateActionButtons()
	{
		if (canvasLayer == null)
		{
			GD.PushWarning(
				"CanvasLayer not found. Feed and Run buttons were not created."
			);

			return;
		}


		// FEED BUTTON

		feedButton = new Button();

		feedButton.Name = "RuntimeFeedButton";
		feedButton.Text = "FEED";
		feedButton.Position = new Vector2(20, 460);
		feedButton.Size = new Vector2(110, 42);

		canvasLayer.AddChild(feedButton);

		feedButton.Pressed += OnFeedPressed;


		// RUN BUTTON

		runButton = new Button();

		runButton.Name = "RuntimeRunButton";
		runButton.Text = "RUN";
		runButton.Position = new Vector2(145, 460);
		runButton.Size = new Vector2(110, 42);

		canvasLayer.AddChild(runButton);

		runButton.Pressed += OnRunPressed;
	}


	// =========================================
	// 14. ATTACK MENU
	// =========================================

	private void OnAttackMenuPressed()
	{
		GD.Print("ATTACK MENU PRESSED!");

		if (battleFinished)
		{
			GD.Print("Battle has already finished.");
			return;
		}

		if (!playerTurn)
		{
			GD.Print("It is not your turn.");
			return;
		}

		ShowAttackChoices();
	}


	// =========================================
	// 15. SHOW ATTACK SKILLS
	// =========================================

	private void ShowAttackChoices()
	{
		if (battleFinished || !playerTurn)
		{
			GD.Print("Cannot show skills right now.");
			return;
		}


		// SLAP

		if (slapButton != null)
		{
			slapButton.Visible = true;
			slapButton.Show();
			slapButton.ZIndex = 100;
			slapButton.MoveToFront();

			GD.Print(
				"Slap visibility: " + slapButton.Visible
			);
		}
		else
		{
			GD.PushWarning(
				"Cannot show Slap: the button reference is null."
			);
		}


		// GHOST PUNCH

		if (ghostPunchButton != null)
		{
			ghostPunchButton.Visible = true;
			ghostPunchButton.Show();
			ghostPunchButton.ZIndex = 100;
			ghostPunchButton.MoveToFront();

			GD.Print(
				"Ghost Punch visibility: " +
				ghostPunchButton.Visible
			);
		}
		else
		{
			GD.PushWarning(
				"Cannot show Ghost Punch: the button reference is null."
			);
		}


		// Report whether the buttons are visible
		// together with their parent containers.

		if (slapButton != null)
		{
			GD.Print(
				"Slap visible in tree: " +
				slapButton.IsVisibleInTree()
			);
		}

		if (ghostPunchButton != null)
		{
			GD.Print(
				"Ghost Punch visible in tree: " +
				ghostPunchButton.IsVisibleInTree()
			);
		}
	}


	// =========================================
	// 16. HIDE ATTACK SKILLS
	// =========================================

	private void HideAttackChoices()
	{
		if (slapButton != null)
		{
			slapButton.Hide();
		}

		if (ghostPunchButton != null)
		{
			ghostPunchButton.Hide();
		}

		GD.Print("Attack skills hidden.");
	}


	// =========================================
	// 17. SLAP
	// =========================================

	private async void OnSlapPressed()
	{
		if (battleFinished || !playerTurn)
			return;

		playerTurn = false;

		HideAttackChoices();

		GD.Print("Used Slap!");

		await PlayYokaiGlow(
			new Color(0.65f, 0.90f, 1.0f)
		);

		if (battleFinished)
			return;

		await DamageEnemy(SlapDamage, 4.0f);

		if (battleFinished)
			return;

		await WaitSeconds(0.7);

		if (!battleFinished)
		{
			await EnemyTurn();
		}
	}


	// =========================================
	// 18. GHOST PUNCH
	// =========================================

	private async void OnGhostPunchPressed()
	{
		if (battleFinished || !playerTurn)
			return;

		playerTurn = false;

		HideAttackChoices();

		GD.Print("Used Ghost Punch!");

		await PlayYokaiGlow(
			new Color(0.15f, 0.45f, 1.0f)
		);

		if (battleFinished)
			return;

		await DamageEnemy(GhostPunchDamage, 12.0f);

		if (battleFinished)
			return;

		await WaitSeconds(0.7);

		if (!battleFinished)
		{
			await EnemyTurn();
		}
	}


	// =========================================
	// 19. RED FLASH WHEN TAKING DAMAGE
	// =========================================

	private async Task FlashRed(Sprite2D character)
	{
		if (character == null)
			return;

		Color originalColor = character.Modulate;

		character.Modulate =
			new Color(1.0f, 0.0f, 0.0f, 1.0f);

		await WaitSeconds(0.2);

		if (GodotObject.IsInstanceValid(character))
		{
			character.Modulate = originalColor;
		}
	}


	// =========================================
	// 20. FRIENDLY YO-KAI GLOW
	// =========================================

	private async Task PlayYokaiGlow(Color glowColor)
	{
		if (friendlyYokaiSprite == null)
			return;

		Tween tween = CreateTween();

		tween.TweenProperty(
			friendlyYokaiSprite,
			"modulate",
			glowColor,
			0.15
		);

		tween.TweenInterval(0.25);

		tween.TweenProperty(
			friendlyYokaiSprite,
			"modulate",
			friendlyOriginalModulate,
			0.20
		);

		await ToSignal(
			tween,
			Tween.SignalName.Finished
		);
	}


	// =========================================
	// 21. DAMAGE ENEMY
	// =========================================

	private async Task DamageEnemy(
		int damage,
		float shakeAmount
	)
	{
		if (battleFinished)
			return;

		enemyHP = Math.Max(
			0,
			enemyHP - damage
		);

		UpdateAllHP();

		GD.Print(
			$"Enemy HP: {enemyHP}/{EnemyMaxHP}"
		);

		await PlayEnemyHit(shakeAmount);

		if (enemyHP <= 0)
		{
			WinBattle();
		}
	}


	// =========================================
	// 22. ENEMY HIT REACTION
	// =========================================

	private async Task PlayEnemyHit(float shakeAmount)
	{
		if (enemySprite == null)
			return;

		Vector2 originalPosition = enemySprite.Position;

		enemySprite.Modulate =
			new Color(1.0f, 0.4f, 0.4f, 1.0f);

		enemySprite.Position =
			originalPosition + new Vector2(-shakeAmount, 0);

		await WaitSeconds(0.06);

		enemySprite.Position =
			originalPosition + new Vector2(shakeAmount, 0);

		await WaitSeconds(0.06);

		enemySprite.Position = originalPosition;

		enemySprite.Modulate = enemyOriginalModulate;
	}


	// =========================================
	// 23. ENEMY TURN
	// =========================================

	private async Task EnemyTurn()
	{
		if (battleFinished)
			return;

		playerTurn = false;

		HideAttackChoices();

		GD.Print("Enemy's turn!");


		// Enemy turns blue before attacking.

		if (enemySprite != null)
		{
			enemySprite.Modulate =
				new Color(0.35f, 0.65f, 1.0f, 1.0f);
		}

		await WaitSeconds(0.6);

		if (battleFinished)
			return;


		// ENEMY ATTACKS PLAYER

		playerHP = Math.Max(
			0,
			playerHP - EnemyDamage
		);

		_ = FlashRed(playerSprite);


		// ENEMY ATTACKS FRIENDLY YO-KAI

		yokaiHP = Math.Max(
			0,
			yokaiHP - EnemyDamage
		);

		_ = FlashRed(friendlyYokaiSprite);


		// UPDATE HP

		UpdateAllHP();

		GD.Print(
			$"Player HP: {playerHP}/{PlayerMaxHP}"
		);

		GD.Print(
			$"Friendly Yo-kai HP: {yokaiHP}/{YokaiMaxHP}"
		);


		// ENEMY ATTACK MOVEMENT

		if (enemySprite != null)
		{
			Vector2 originalPosition = enemySprite.Position;

			enemySprite.Position =
				originalPosition + new Vector2(-6, 0);

			await WaitSeconds(0.06);

			enemySprite.Position =
				originalPosition + new Vector2(6, 0);

			await WaitSeconds(0.06);

			enemySprite.Position = originalPosition;

			enemySprite.Modulate = enemyOriginalModulate;
		}


		// DEFEAT CHECK

		if (playerHP <= 0 && yokaiHP <= 0)
		{
			await WaitSeconds(0.2);

			LoseBattle();

			return;
		}


		await WaitSeconds(0.4);

		if (battleFinished)
			return;


		// -------------------------------------
		// PLAYER'S TURN STARTS AGAIN
		// -------------------------------------

		playerTurn = true;

		// Skills become available again.
		ShowAttackChoices();

		GD.Print("Your turn! Attack skills are available.");
	}


	// =========================================
	// 24. FEED
	// =========================================

	private async void OnFeedPressed()
	{
		if (battleFinished || !playerTurn)
			return;

		if (yokaiHP >= YokaiMaxHP)
		{
			GD.Print("Your Yo-kai already has full HP!");
			return;
		}

		playerTurn = false;

		HideAttackChoices();

		yokaiHP = Math.Min(
			YokaiMaxHP,
			yokaiHP + FeedHealing
		);

		UpdateAllHP();

		GD.Print("You fed your Yo-kai!");

		GD.Print(
			$"Friendly Yo-kai HP: {yokaiHP}/{YokaiMaxHP}"
		);

		await WaitSeconds(0.7);

		if (!battleFinished)
		{
			await EnemyTurn();
		}
	}


	// =========================================
	// 25. RUN
	// =========================================

	private async void OnRunPressed()
	{
		if (battleFinished || !playerTurn)
			return;

		playerTurn = false;

		HideAttackChoices();

		GD.Print("Attempting to escape...");

		int escapeRoll = random.Next(1, 101);

		if (escapeRoll <= 70)
		{
			GD.Print("Escaped successfully!");

			battleFinished = true;

			Error error = GetTree().ChangeSceneToFile(
				"res://world.tscn"
			);

			if (error != Error.Ok)
			{
				GD.PushError(
					"Could not open res://world.tscn. Check the scene filename."
				);
			}

			return;
		}

		GD.Print("Escape failed!");

		await WaitSeconds(0.7);

		if (!battleFinished)
		{
			await EnemyTurn();
		}
	}


	// =========================================
	// 26. UPDATE ALL HP
	// =========================================

	private void UpdateAllHP()
	{
		UpdateHPDisplay(
			playerHPBar,
			playerHPText,
			playerHP,
			PlayerMaxHP
		);

		UpdateHPDisplay(
			yokaiHPBar,
			yokaiHPText,
			yokaiHP,
			YokaiMaxHP
		);

		UpdateHPDisplay(
			enemyHPBar,
			enemyHPText,
			enemyHP,
			EnemyMaxHP
		);
	}


	private void UpdateHPDisplay(
		Godot.Range hpBar,
		Label hpText,
		int currentHP,
		int maximumHP
	)
	{
		if (hpBar != null)
		{
			hpBar.MaxValue = maximumHP;
			hpBar.Value = currentHP;
		}

		if (hpText != null)
		{
			hpText.Text = $"{currentHP}/{maximumHP}";
		}
	}


	// =========================================
	// 27. VICTORY
	// =========================================

	private void WinBattle()
	{
		battleFinished = true;

		HideAttackChoices();

		if (attackButton != null)
			attackButton.Disabled = true;

		if (feedButton != null)
			feedButton.Disabled = true;

		if (runButton != null)
			runButton.Disabled = true;

		GD.Print("VICTORY!");
		GD.Print("You defeated the enemy Yo-kai!");
	}


	// =========================================
	// 28. DEFEAT
	// =========================================

	private void LoseBattle()
	{
		battleFinished = true;

		HideAttackChoices();

		if (attackButton != null)
			attackButton.Disabled = true;

		if (feedButton != null)
			feedButton.Disabled = true;

		if (runButton != null)
			runButton.Disabled = true;

		GD.Print("DEFEAT!");
		GD.Print("Your team can no longer fight.");
	}


	// =========================================
	// 29. TIMER
	// =========================================

	private async Task WaitSeconds(double seconds)
	{
		await ToSignal(
			GetTree().CreateTimer(seconds),
			SceneTreeTimer.SignalName.Timeout
		);
	}
}
