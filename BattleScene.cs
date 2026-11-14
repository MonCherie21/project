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
	[Export] public int EnemyMaxHP = 70;

	private int playerHP;
	private int yokaiHP;
	private int enemyHP;


	// =========================================
	// 2. DAMAGE AND HEALING
	// =========================================

	[Export] public int SlapDamage = 8;
	[Export] public int GhostPunchDamage = 16;

	[Export] public int EnemyDamage = 6;
	[Export] public int FeedHealing = 20;


	// =========================================
	// 3. FRIENDSHIP DIFFICULTY
	// =========================================

	[Export] public int FriendshipLevel = 0;


	// =========================================
	// 4. BATTLE STATE & COOLDOWNS
	// =========================================

	private bool battleFinished = false;
	private bool playerTurn = true;
	private int ghostPunchCooldown = 0;

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
	private Tween _friendlyIdleTween;
	private Tween _enemyIdleTween;


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
		ghostPunchCooldown = 0;

		// The occupied locations look damaged until the Yokai falls. The
		// world scene restores the clean building art after that victory.
		var battleBackground = GetNodeOrNull<TextureRect>("level1_bg");
		if (battleBackground != null)
		{
			var damage = new DisturbedBattleBackdrop { Name = "YokaiDamage" };
			AddChild(damage);
			MoveChild(damage, battleBackground.GetIndex() + 1);
		}

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
		StartIdleAnimations();

		GD.Print("Battle initialized!");
		GD.Print("Friendship Level: " + FriendshipLevel);
		GD.Print("Enemy damage: " + EnemyDamage);

		if (GameProgress.LevelFromBattleScene(GetTree().CurrentScene?.SceneFilePath ?? SceneFilePath) == 1
			&& !GameProgress.HasSeenBattleTutorial)
		{
			GameProgress.MarkBattleTutorialSeen();
			TutorialOverlay.Show(this, "YOUR FIRST YO-KAI BATTLE",
				"It is your turn first! Choose an attack below.\n\n" +
				"SLAP is a reliable quick hit. GHOST PUNCH does more damage, " +
				"but needs time to recharge.\n\n" +
				"FEED heals your team. RUN leaves the battle. " +
				"Watch the HP bars and wait for your turn!");
		}
	}


	// =========================================
	// 12. FRIENDSHIP DIFFICULTY
	// =========================================

	private void ApplyFriendshipDifficulty()
	{
		if (FriendshipLevel <= 0)
		{
			EnemyDamage = 6;
		}
		else if (FriendshipLevel == 1)
		{
			EnemyDamage = 5;
		}
		else if (FriendshipLevel == 2)
		{
			EnemyDamage = 4;
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
		}


		// GHOST PUNCH

		if (ghostPunchButton != null)
		{
			ghostPunchButton.Visible = true;
			ghostPunchButton.Show();
			ghostPunchButton.ZIndex = 100;
			ghostPunchButton.MoveToFront();

			// Grey out and disable the skill while it is on cooldown.
			ghostPunchButton.Disabled = ghostPunchCooldown > 0;
			if (ghostPunchCooldown > 0)
			{
				ghostPunchButton.Modulate = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				GD.Print($"Ghost Punch is on cooldown. Turns remaining: {ghostPunchCooldown}");
			}
			else
			{
				ghostPunchButton.Modulate = Colors.White;
			}
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

		// Reduce Ghost Punch cooldown when using regular attack
		if (ghostPunchCooldown > 0)
		{
			ghostPunchCooldown--;
		}

		GD.Print("Used Slap!");

		await PlayFriendlyAttack(new Color(0.65f, 0.90f, 1.0f),
			43.0f, SlapDamage, 4.0f);

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

		// Prevent using Ghost Punch if it's still cooling down
		if (ghostPunchCooldown > 0)
		{
			GD.Print($"Ghost Punch is on cooldown! ({ghostPunchCooldown} turns left)");
			return;
		}

		playerTurn = false;

		HideAttackChoices();

		// Set cooldown to 2 turns (can adjust this number as needed)
		ghostPunchCooldown = 2;

		GD.Print("Used Ghost Punch!");

		await PlayFriendlyAttack(new Color(0.15f, 0.65f, 1.0f),
			72.0f, GhostPunchDamage, 12.0f);

		if (battleFinished)
			return;

		await WaitSeconds(0.7);

		if (!battleFinished)
		{
			await EnemyTurn();
		}
	}


	// =========================================
	// 19. RED FLASH AND SHAKE WHEN TAKING DAMAGE
	// =========================================

	private async Task FlashRed(Sprite2D character)
	{
		if (character == null)
			return;

		Color originalColor = character.Modulate;
		Vector2 start = character.Position;
		Tween tween = CreateTween();
		tween.TweenProperty(character, "modulate", new Color(1.0f, 0.22f, 0.22f), 0.06);
		tween.Parallel().TweenProperty(character, "position", start + new Vector2(-7, 0), 0.06);
		tween.TweenProperty(character, "position", start + new Vector2(7, 0), 0.07);
		tween.TweenProperty(character, "position", start - new Vector2(4, 0), 0.06);
		tween.TweenProperty(character, "position", start, 0.10);
		tween.Parallel().TweenProperty(character, "modulate", originalColor, 0.10);
		await ToSignal(tween, Tween.SignalName.Finished);
	}


	// =========================================
	// 20. FRIENDLY YO-KAI LUNGE AND GLOW
	// =========================================

	private async Task PlayFriendlyAttack(Color glowColor, float distance,
		int damage, float shakeAmount)
	{
		if (friendlyYokaiSprite == null)
		{
			await DamageEnemy(damage, shakeAmount);
			return;
		}

		Vector2 start = friendlyYokaiSprite.Position;
		float startRotation = friendlyYokaiSprite.Rotation;
		Tween tween = CreateTween();
		tween.SetTrans(Tween.TransitionType.Cubic);
		tween.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(friendlyYokaiSprite, "position", start + new Vector2(distance, -8), 0.18);
		tween.Parallel().TweenProperty(friendlyYokaiSprite, "modulate", glowColor, 0.18);
		tween.Parallel().TweenProperty(friendlyYokaiSprite, "rotation", startRotation - 0.07f, 0.18);
		await ToSignal(tween, Tween.SignalName.Finished);

		// Damage happens at the forward impact, while the attacker recoils.
		Task impact = DamageEnemy(damage, shakeAmount);
		Tween recoil = CreateTween();
		recoil.SetTrans(Tween.TransitionType.Sine);
		recoil.SetEase(Tween.EaseType.Out);
		recoil.TweenProperty(friendlyYokaiSprite, "position", start, 0.24);
		recoil.Parallel().TweenProperty(friendlyYokaiSprite, "modulate", friendlyOriginalModulate, 0.24);
		recoil.Parallel().TweenProperty(friendlyYokaiSprite, "rotation", startRotation, 0.24);
		await ToSignal(recoil, Tween.SignalName.Finished);
		await impact;
	}

	// Gentle looping movement makes the static Yo-kai sprites feel alive.
	// Offset is animated separately from Position, so it doesn't fight lunges.
	private void StartIdleAnimations()
	{
		_friendlyIdleTween = CreateIdleBob(friendlyYokaiSprite, 3.0f, 0.76);
		_enemyIdleTween = CreateIdleBob(enemySprite, 4.0f, 0.95);
	}

	private Tween CreateIdleBob(Sprite2D sprite, float height, double halfCycle)
	{
		if (sprite == null)
			return null;

		Vector2 baseOffset = sprite.Offset;
		Tween tween = CreateTween();
		tween.SetTrans(Tween.TransitionType.Sine);
		tween.SetEase(Tween.EaseType.InOut);
		tween.SetLoops();
		tween.TweenProperty(sprite, "offset", baseOffset + new Vector2(0, -height), halfCycle);
		tween.TweenProperty(sprite, "offset", baseOffset, halfCycle);
		return tween;
	}

	private void StopIdleAnimations()
	{
		if (GodotObject.IsInstanceValid(_friendlyIdleTween))
			_friendlyIdleTween.Kill();
		if (GodotObject.IsInstanceValid(_enemyIdleTween))
			_enemyIdleTween.Kill();
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
		Tween tween = CreateTween();
		tween.SetTrans(Tween.TransitionType.Sine);
		tween.TweenProperty(enemySprite, "modulate", new Color(1.0f, 0.16f, 0.16f), 0.06);
		tween.Parallel().TweenProperty(enemySprite, "position",
			originalPosition + new Vector2(-shakeAmount, 0), 0.06);
		tween.TweenProperty(enemySprite, "position",
			originalPosition + new Vector2(shakeAmount, 0), 0.07);
		tween.TweenProperty(enemySprite, "position",
			originalPosition + new Vector2(-shakeAmount / 2, 0), 0.07);
		tween.TweenProperty(enemySprite, "position", originalPosition, 0.10);
		tween.Parallel().TweenProperty(enemySprite, "modulate", enemyOriginalModulate, 0.10);
		await ToSignal(tween, Tween.SignalName.Finished);
	}


	// =========================================
	// 23. ENEMY TURN
	// =========================================
	private async Task PlayEnemyLunge(Action onImpact)
	{
		if (enemySprite == null)
		{
			onImpact();
			return;
		}

		Vector2 start = enemySprite.Position;
		float startRotation = enemySprite.Rotation;
		Tween tween = CreateTween();
		tween.SetTrans(Tween.TransitionType.Cubic);
		tween.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(enemySprite, "position", start + new Vector2(-54, -7), 0.18);
		tween.Parallel().TweenProperty(enemySprite, "rotation", startRotation + 0.07f, 0.18);
		await ToSignal(tween, Tween.SignalName.Finished);

		// The HP hit and target flash begin when the enemy reaches the apex.
		onImpact();
		Tween recoil = CreateTween();
		recoil.SetTrans(Tween.TransitionType.Sine);
		recoil.SetEase(Tween.EaseType.Out);
		recoil.TweenProperty(enemySprite, "position", start, 0.23);
		recoil.Parallel().TweenProperty(enemySprite, "rotation", startRotation, 0.23);
		recoil.Parallel().TweenProperty(enemySprite, "modulate", enemyOriginalModulate, 0.23);
		await ToSignal(recoil, Tween.SignalName.Finished);
	}

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

		await WaitSeconds(0.35);

		if (battleFinished)
			return;

		// One enemy action hits ONE target. Previously it damaged both characters
		// every round, making the opening fights disproportionately punishing.
		bool hitPlayer = playerHP > 0 &&
			(yokaiHP <= 0 || random.NextDouble() < 0.4);
		Sprite2D target = hitPlayer ? playerSprite : friendlyYokaiSprite;
		Task hitReaction = Task.CompletedTask;
		await PlayEnemyLunge(() =>
		{
			if (hitPlayer)
			{
				playerHP = Math.Max(0, playerHP - EnemyDamage);
				GD.Print("Enemy hit the player for " + EnemyDamage + ".");
			}
			else
			{
				yokaiHP = Math.Max(0, yokaiHP - EnemyDamage);
				GD.Print("Enemy hit the friendly Yo-kai for " + EnemyDamage + ".");
			}
			UpdateAllHP();
			hitReaction = FlashRed(target);
		});
		await hitReaction;
		if (battleFinished)
			return;

		GD.Print(
			$"Player HP: {playerHP}/{PlayerMaxHP}"
		);

		GD.Print(
			$"Friendly Yo-kai HP: {yokaiHP}/{YokaiMaxHP}"
		);


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

		// Reduce Ghost Punch cooldown at the end of the full round
		if (ghostPunchCooldown > 0)
		{
			ghostPunchCooldown--;
		}

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

		// Reduce cooldown if feeding counts as an action turn
		if (ghostPunchCooldown > 0)
		{
			ghostPunchCooldown--;
		}

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
			ReturnToWorld();

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
		StopIdleAnimations();

		HideAttackChoices();

		if (attackButton != null)
			attackButton.Disabled = true;

		if (feedButton != null)
			feedButton.Disabled = true;

		if (runButton != null)
			runButton.Disabled = true;

		GD.Print("VICTORY!");
		GD.Print("You defeated the enemy Yo-kai!");

		string path = GetTree().CurrentScene?.SceneFilePath ?? SceneFilePath;
		int level = GameProgress.LevelFromBattleScene(path);
		bool advanced = GameProgress.CompleteLevel(level);
		string message = advanced
			? level == GameProgress.TotalLevels
				? "You cleared all seven levels! Wawa is safe. Return to the map."
				: $"Level {level} cleared! Next: {GameProgress.NextBuilding}. Return to Wawa."
			: "Enemy defeated! Return to Wawa.";
		ShowResult("VICTORY!", message);
	}


	// =========================================
	// 28. DEFEAT
	// =========================================

	private void LoseBattle()
	{
		battleFinished = true;
		StopIdleAnimations();

		HideAttackChoices();

		if (attackButton != null)
			attackButton.Disabled = true;

		if (feedButton != null)
			feedButton.Disabled = true;

		if (runButton != null)
			runButton.Disabled = true;

		GD.Print("DEFEAT!");
		GD.Print("Your team can no longer fight.");
		ShowResult("DEFEAT", "Your team can no longer fight. Return to Wawa and try again.");
	}

	private void ShowResult(string title, string message)
	{
		var dialog = new AcceptDialog { Title = title, DialogText = message };
		AddChild(dialog);
		dialog.Confirmed += ReturnToWorld;
		dialog.CloseRequested += ReturnToWorld;
		dialog.PopupCentered();
	}

	private void ReturnToWorld()
	{
		// Preserve the last map position after a battle or successful escape.
		PlayerData.HasLoadedPosition = true;
		GetTree().Paused = false;
		Error error = GetTree().ChangeSceneToFile("res://world.tscn");
		if (error != Error.Ok)
			GD.PushError("Could not return to Wawa: " + error);
	}


	// =========================================
	// 29. TIMER
	// =========================================

	private async Task WaitSeconds(double seconds)
	{
		await ToSignal(
			GetTree().CreateTimer(seconds, processAlways: false),
			SceneTreeTimer.SignalName.Timeout
		);
	}
}
