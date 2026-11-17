using Godot;
using System;
using System.Threading.Tasks;

public partial class BattleScene : Node2D
{
	// =========================================
	// 1. HP SETTINGS
	// =========================================

	[Export] public int YokaiMaxHP = 100;
	[Export] public int EnemyMaxHP = 70;

	private int yokaiHP;
	private int enemyHP;


	// =========================================
	// 2. DAMAGE
	// =========================================

	[Export] public int SlapDamage = 8;
	[Export] public int GhostPunchDamage = 16;

	[Export] public int EnemyDamage = 6;


	// =========================================
	// 3. FRIENDSHIP POWER
	// =========================================

	[Export] public int FriendshipLevel = 0;
	private int _battleStage = 1;
	private int _activePartnerIndex;


	// =========================================
	// 4. BATTLE STATE & COOLDOWNS
	// =========================================

	private bool battleFinished = false;
	private bool _victoryAdvanced;
	private bool playerTurn = true;
	private const int UltimateChargeRequired = 2;
	private const double FriendlyCriticalChance = 0.16;
	private const double EnemyCriticalChance = 0.28;
	private int ultimateCharge = 0;
	private int ghostPunchCooldown = 0;

	private readonly Random random = new Random();


	// =========================================
	// 5. SPRITES
	// =========================================

	private Sprite2D friendlyYokaiSprite;
	private Sprite2D enemySprite;
	private Sprite2D trainerSprite;

	private Color friendlyOriginalModulate = Colors.White;
	private Color enemyOriginalModulate = Colors.White;
	private Tween _friendlyIdleTween;
	private Tween _enemyIdleTween;
	private Tween _friendlyBreathTween;
	private Tween _enemyBreathTween;
	private Tween _trainerIdleTween;
	private BattlePixelEffects _pixelEffects;


	// =========================================
	// 6. BUTTONS
	// =========================================

	private TextureButton slapButton;
	private TextureButton ghostPunchButton;

	private BaseButton attackButton;

	private Button runButton;
	private ProgressBar ultimateMeter;
	private Label ultimateText;


	// =========================================
	// 7. HP BARS
	// =========================================

	private Godot.Range yokaiHPBar;
	private Godot.Range enemyHPBar;


	// =========================================
	// 8. HP LABELS
	// =========================================

	private Label yokaiHPText;
	private Label enemyHPText;
	private Label _friendlyPercent;
	private Label _enemyPercent;


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
		_victoryAdvanced = false;
		playerTurn = true;
		ghostPunchCooldown = 0;
		ultimateCharge = 0;

		// The occupied locations look damaged until the Yokai falls. The
		// world scene restores the clean building art after that victory.
		var battleBackground = GetNodeOrNull<TextureRect>("level1_bg");
		if (battleBackground != null)
		{
			var damage = new DisturbedBattleBackdrop
			{
				Name = "YokaiDamage",
				Stage = Math.Clamp(GameProgress.LevelFromBattleScene(
					GetTree().CurrentScene?.SceneFilePath ?? SceneFilePath), 1, GameProgress.TotalLevels)
			};
			AddChild(damage);
			MoveChild(damage, battleBackground.GetIndex() + 1);
		}

		// The story stage determines the opponent; the Party menu determines
		// which unlocked companion fights and which friendship stat applies.
		_battleStage = Math.Clamp(GameProgress.LevelFromBattleScene(
			GetTree().CurrentScene?.SceneFilePath ?? SceneFilePath), 1, GameProgress.TotalLevels);
		_activePartnerIndex = CandyInventory.ActivePartnerIndex;
		SlapDamage = YokaiRoster.NormalBaseDamage(_activePartnerIndex);
		GhostPunchDamage = YokaiRoster.UltimateBaseDamage(_activePartnerIndex);
		FriendshipLevel = CandyInventory.FriendshipLevel(_activePartnerIndex);
		ApplyFriendshipPower();
		yokaiHP = YokaiMaxHP;
		enemyHP = EnemyMaxHP;


		// -------------------------------------
		// FIND SPRITES
		// -------------------------------------

		friendlyYokaiSprite =
			FindNodeRecursive(this, "m1_py2") as Sprite2D;

		enemySprite =
			FindNodeRecursive(this, "m1_m") as Sprite2D;
		trainerSprite = FindNodeRecursive(this, "P1Whole") as Sprite2D;
		ConfigureTrainerAppearance();


		// Save original sprite colors.
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
		_pixelEffects = new BattlePixelEffects { Name = "PixelBattleFX", ZIndex = 85 };
		AddChild(_pixelEffects);
		_pixelEffects.SetStage(_battleStage);


		// -------------------------------------
		// FIND SKILL BUTTONS
		// -------------------------------------

		slapButton =
			FindNodeRecursive(this, "TextureButton") as TextureButton;

		ghostPunchButton =
			FindNodeRecursive(this, "TextureButton2") as TextureButton;
		ConfigureActiveYokai();


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

		yokaiHPBar =
			FindNodeRecursive(this, "hp_py") as Godot.Range;

		enemyHPBar =
			FindNodeRecursive(this, "hp_m") as Godot.Range;


		// -------------------------------------
		// FIND HP LABELS
		// -------------------------------------

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
		// CREATE RUN BUTTON AND ULTIMATE METER
		// -------------------------------------

		CreateActionButtons();
		UseSingleYokaiHpBar();
		StyleHPDisplays();


		// -------------------------------------
		// INITIAL BUTTON VISIBILITY
		// -------------------------------------

		ShowAttackChoices();


		// -------------------------------------
		// INITIAL HP DISPLAY
		// -------------------------------------

		UpdateAllHP();
		StartIdleAnimations();
		if (_battleStage == GameProgress.TotalLevels)
			BossTitleCard.Show(this);

		GD.Print("Battle initialized!");
		GD.Print("Friendship Level: " + FriendshipLevel);
		GD.Print("Enemy damage (stage-scaled): " + EnemyDamage);
		GD.Print("Yokai HP: " + YokaiMaxHP + ", normal damage: " + NormalAttackDamage());

		if (GameProgress.LevelFromBattleScene(GetTree().CurrentScene?.SceneFilePath ?? SceneFilePath) == 1
			&& !GameProgress.HasSeenBattleTutorial)
		{
			GameProgress.MarkBattleTutorialSeen();
			TutorialOverlay.Show(this, "YOUR FIRST YO-KAI BATTLE",
				"It is your turn first! Choose an attack below.\n\n" +
				"Normal attacks fill the ULT meter. After TWO normal attacks, " +
				"use your second skill for a powerful ultimate! It must recharge afterward.\n\n" +
				"RUN attempts to escape. Only your Yo-kai takes damage.\n\n" +
				"Open PARTY [I] in the world to choose your active Yokai and feed candy. " +
				"Friendship boosts your Yo-kai's maximum HP and attack damage; " +
				"enemies do NOT get weaker. Explore for candy before challenging them!");
		}
	}

	private void ConfigureTrainerAppearance()
	{
		if (trainerSprite == null)
			return;

		bool male = string.Equals(PlayerData.Gender, "Male", StringComparison.OrdinalIgnoreCase);
		string spritePath = male ? "res://Sprites/P2_Whole.png" : "res://Sprites/P1_Whole.png";
		Texture2D texture = GD.Load<Texture2D>(spritePath);
		if (texture == null)
		{
			GD.PushWarning("Trainer sprite missing: " + spritePath);
			return;
		}

		trainerSprite.Texture = texture;
		// Male art is 368x677; normalize it to the original 205x243
		// trainer's visible size and foot position in every battle scene.
		if (male)
			trainerSprite.Scale = new Vector2(0.83f, 0.68f);
		trainerSprite.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
	}

	private void ConfigureActiveYokai()
	{
		if (friendlyYokaiSprite != null)
		{
			Texture2D yokaiTexture = GD.Load<Texture2D>(YokaiRoster.BattleSprite(_activePartnerIndex));
			if (yokaiTexture != null)
			{
				friendlyYokaiSprite.Texture = yokaiTexture;
				// Common size / footing prevents large companions from obscuring
				// the trainer or getting cut off in different stage layouts.
				float scale = 185f / Math.Max(1, yokaiTexture.GetHeight());
				friendlyYokaiSprite.Scale = new Vector2(scale, scale);
				friendlyYokaiSprite.Position = new Vector2(366, 533);
				friendlyYokaiSprite.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
			}
			else
				GD.PushWarning("Active Yokai sprite missing for slot " + _activePartnerIndex);
		}

		if (slapButton != null)
		{
			slapButton.TextureNormal = GD.Load<Texture2D>(YokaiRoster.NormalSkill(_activePartnerIndex));
			slapButton.TooltipText = $"{YokaiRoster.NormalMoveName(_activePartnerIndex)}: " +
				$"{NormalAttackDamage()} damage with current friendship";
		}
		if (ghostPunchButton != null)
		{
			ghostPunchButton.TextureNormal = GD.Load<Texture2D>(YokaiRoster.UltimateSkill(_activePartnerIndex));
			ghostPunchButton.TooltipText = $"{YokaiRoster.UltimateMoveName(_activePartnerIndex)}: " +
				$"{UltimateAttackDamage()} damage (charges in two normal attacks)";
		}
		ArrangeSkillButtonsForActiveYokai();
		GD.Print("Active battle companion: " + YokaiRoster.Name(_activePartnerIndex));
	}

	private void ArrangeSkillButtonsForActiveYokai()
	{
		if (_battleStage < 5 || slapButton == null || ghostPunchButton == null)
			return;

		// The McDo and school scenes have wide slots for Enma and Wobblewok.
		// Earlier circular skill art must use Whisper's full-size slots instead,
		// even when that companion is taken into a later story battle.
		if (!IsCircularSkill(slapButton.TextureNormal) ||
			!IsCircularSkill(ghostPunchButton.TextureNormal))
			return; // Preserve the original wide buttons for later Yokai.

		slapButton.Position = new Vector2(452, 457);
		slapButton.Size = new Vector2(197, 199);
		ghostPunchButton.Position = new Vector2(667, 457);
		ghostPunchButton.Size = new Vector2(197, 199);
		slapButton.IgnoreTextureSize = true;
		ghostPunchButton.IgnoreTextureSize = true;
		slapButton.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
		ghostPunchButton.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
	}

	private static bool IsCircularSkill(Texture2D texture) =>
		texture != null && texture.GetWidth() <= texture.GetHeight() * 1.4f;


	// =========================================
	// 12. FRIENDSHIP POWER — BOTH SIDES SCALE WITH STORY PROGRESS
	// =========================================

	private void ApplyFriendshipPower()
	{
		FriendshipLevel = Math.Clamp(FriendshipLevel, 0, CandyInventory.MaxFriendshipLevel);
		// Opponents scale up with stage and slightly with friendship, never down.
		// Feeding always improves the friendly Yokai's advantage in a matchup.
		EnemyMaxHP = Math.Max(EnemyMaxHP, 70 + 10 * (_battleStage - 1) + 5 * FriendshipLevel);
		EnemyDamage = Math.Max(EnemyDamage, 10 + (_battleStage - 1) + FriendshipLevel / 2);
		YokaiMaxHP = Math.Max(YokaiMaxHP + 9 * (_battleStage - 1) + 8 * FriendshipLevel,
			EnemyMaxHP + 20);
	}

	private int NormalAttackDamage() => SlapDamage + (_battleStage - 1) + FriendshipLevel * 3;
	private int UltimateAttackDamage() => GhostPunchDamage + 2 * (_battleStage - 1) + FriendshipLevel * 6;


	// =========================================
	// 13. MATCHING RUN BUTTON AND ULTIMATE METER
	// =========================================

	private void CreateActionButtons()
	{
		if (canvasLayer == null)
		{
			GD.PushWarning(
				"CanvasLayer not found. Battle controls were not created."
			);

			return;
		}


		runButton = new Button
		{
			Name = "RuntimeRunButton",
			Text = "RUN",
			Position = new Vector2(72, 585),
			Size = new Vector2(190, 53),
			FocusMode = Control.FocusModeEnum.None
		};
		YokaiTheme.StyleButton(runButton);
		runButton.AddThemeFontOverride("font", GD.Load<Font>("res://Assets/PressStart2P-Regular.ttf"));
		runButton.AddThemeFontSizeOverride("font_size", 18);
		runButton.AddThemeColorOverride("font_color", YokaiTheme.Parchment);
		runButton.AddThemeColorOverride("font_hover_color", Colors.White);
		canvasLayer.AddChild(runButton);
		runButton.Pressed += OnRunPressed;

		// Fixed screen-space HUD across all seven battle scenes: it never
		// depends on a skill button's position or falls below 720p.
		var ultimatePanel = new Panel
		{
			Name = "UltimateStatusPanel",
			Position = new Vector2(492, 69),
			Size = new Vector2(285, 64),
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
		ultimatePanel.AddThemeStyleboxOverride("panel", YokaiTheme.Panel(3));
		canvasLayer.AddChild(ultimatePanel);
		ultimateMeter = new ProgressBar
		{
			Name = "UltimateChargeMeter",
			Position = new Vector2(14, 38),
			Size = new Vector2(256, 15),
			MinValue = 0,
			MaxValue = UltimateChargeRequired,
			ShowPercentage = false,
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
		ultimateMeter.AddThemeStyleboxOverride("background", YokaiTheme.Box(YokaiTheme.Shadow, YokaiTheme.FrameEdge));
		ultimateMeter.AddThemeStyleboxOverride("fill", YokaiTheme.Box(YokaiTheme.Brass, YokaiTheme.BrassBright));
		ultimatePanel.AddChild(ultimateMeter);

		ultimateText = new Label
		{
			Name = "UltimateChargeText",
			Position = new Vector2(14, 9),
			Size = new Vector2(256, 27),
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
		ultimateText.AddThemeFontOverride("font", GD.Load<Font>("res://Assets/PressStart2P-Regular.ttf"));
		ultimateText.AddThemeFontSizeOverride("font_size", 12);
		ultimateText.AddThemeColorOverride("font_color", YokaiTheme.BrassBright);
		ultimateText.AddThemeColorOverride("font_outline_color", Colors.Black);
		ultimateText.AddThemeConstantOverride("outline_size", 3);
		ultimatePanel.AddChild(ultimateText);
		UpdateUltimateUI();
	}

	private static StyleBoxFlat BattleBox(string background, string border) => new()
	{
		BgColor = new Color(background),
		BorderColor = new Color(border),
		BorderWidthLeft = 3,
		BorderWidthTop = 3,
		BorderWidthRight = 3,
		BorderWidthBottom = 3,
		CornerRadiusTopLeft = 3,
		CornerRadiusTopRight = 3,
		CornerRadiusBottomLeft = 3,
		CornerRadiusBottomRight = 3
	};

	private void UseSingleYokaiHpBar()
	{
		// Legacy battle scenes still contain trainer HP nodes. Hide these
		// at runtime and use their former top-left slot for the Yokai HP.
		Control trainerBar = FindNodeRecursive(this, "hp_p") as Control;
		Control trainerText = FindNodeRecursive(this, "hptext_p") as Control;
		Label trainerRole = FindNodeRecursive(this, "PlayerRole") as Label;
		Label yokaiRole = FindNodeRecursive(this, "YokaiRole") as Label;
		if (trainerBar != null && yokaiHPBar is Control yokaiBar)
			yokaiBar.Position = trainerBar.Position;
		if (trainerText != null && yokaiHPText != null)
			yokaiHPText.Position = trainerText.Position;
		if (trainerRole != null && yokaiRole != null)
			yokaiRole.Position = trainerRole.Position;
		if (yokaiRole != null)
		{
			yokaiRole.Text = YokaiRoster.Name(_activePartnerIndex);
			yokaiRole.AddThemeFontSizeOverride("font_size", 14);
		}
		trainerBar?.Hide();
		trainerText?.Hide();
		trainerRole?.Hide();
	}

	private void StyleHPDisplays()
	{
		// Native ProgressBar percentage text blends into bright maps.
		// Draw it explicitly with separate, higher-contrast colors.
		_friendlyPercent = CreatePercentLabel(yokaiHPBar, "YokaiPercent",
			new Color("082d43"), new Color("d3f6ff"));
		_enemyPercent = CreatePercentLabel(enemyHPBar, "EnemyPercent",
			new Color("fff1cf"), new Color("50121d"));
		StyleNumberLabel(yokaiHPText, new Color("ffed9b"));
		StyleNumberLabel(enemyHPText, new Color("ffd2d0"));
	}

	private Label CreatePercentLabel(Godot.Range bar, string name, Color text, Color outline)
	{
		if (bar is not ProgressBar progress || canvasLayer == null)
			return null;
		progress.ShowPercentage = false;
		var label = new Label
		{
			Name = name,
			Position = progress.Position + new Vector2(0, 2),
			Size = progress.Size,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
		label.AddThemeColorOverride("font_color", text);
		label.AddThemeColorOverride("font_outline_color", outline);
		label.AddThemeConstantOverride("outline_size", 3);
		label.AddThemeFontSizeOverride("font_size", 16);
		canvasLayer.AddChild(label);
		return label;
	}

	private static void StyleNumberLabel(Label label, Color color)
	{
		if (label == null)
			return;
		label.AddThemeColorOverride("font_color", color);
		label.AddThemeColorOverride("font_outline_color", new Color("14202d"));
		label.AddThemeConstantOverride("outline_size", 4);
		label.AddThemeFontSizeOverride("font_size", 19);
	}

	private void UpdateUltimateUI()
	{
		bool available = ultimateCharge >= UltimateChargeRequired && ghostPunchCooldown == 0;
		if (ghostPunchButton != null)
		{
			ghostPunchButton.Disabled = battleFinished || !playerTurn || !available;
			ghostPunchButton.Modulate = available ? Colors.White : new Color(0.48f, 0.48f, 0.48f);
		}
		if (ultimateMeter != null)
			ultimateMeter.Value = ultimateCharge;
		if (ultimateText != null)
		{
			ultimateText.Text = available ? "ULT READY!" : $"ULT {ultimateCharge}/{UltimateChargeRequired}";
			ultimateText.AddThemeColorOverride("font_color",
				available ? new Color("c9ffad") : new Color("ffe3a1"));
		}
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

			UpdateUltimateUI();
		}
		if (runButton != null)
			runButton.Disabled = false;
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
		if (runButton != null)
			runButton.Disabled = true;

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

		// Normal attacks charge the ultimate and tick its post-use cooldown.
		bool willCharge = ultimateCharge == UltimateChargeRequired - 1;
		ultimateCharge = Math.Min(UltimateChargeRequired, ultimateCharge + 1);
		if (ghostPunchCooldown > 0)
			ghostPunchCooldown--;
		UpdateUltimateUI();

		GD.Print("Used " + YokaiRoster.NormalMoveName(_activePartnerIndex) + "!");
		PlaySound("AttackNormal", -7);

		await PlayFriendlyAttack(new Color(0.65f, 0.90f, 1.0f),
			43.0f, NormalAttackDamage(), 4.0f);
		if (willCharge && !battleFinished)
			PlaySound("ChargeReady", -11);

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

		// Ultimates cannot be used until normal attacks fill the meter.
		if (ghostPunchCooldown > 0 || ultimateCharge < UltimateChargeRequired)
		{
			GD.Print($"Ultimate not ready: charge {ultimateCharge}/{UltimateChargeRequired}");
			return;
		}

		playerTurn = false;

		HideAttackChoices();

		ultimateCharge = 0;
		ghostPunchCooldown = 1;
		UpdateUltimateUI();

		GD.Print("Used " + YokaiRoster.UltimateMoveName(_activePartnerIndex) + "!");
		if (_pixelEffects != null && friendlyYokaiSprite != null)
			await _pixelEffects.Charge(friendlyYokaiSprite.Position);
		PlaySound("AttackUltimate", -5);

		await PlayFriendlyAttack(new Color(0.15f, 0.65f, 1.0f),
			72.0f, UltimateAttackDamage(), 12.0f);

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
		if (_pixelEffects != null)
			await _pixelEffects.Projectile(friendlyYokaiSprite.Position,
				enemySprite?.Position ?? new Vector2(950, 370), distance >= 65f);
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

	// Gentle looping movement makes the trainer and Yo-kai sprites feel alive.
	// Offset is animated separately from Position, so it doesn't fight lunges.
	private void StartIdleAnimations()
	{
		_friendlyIdleTween = CreateIdleBob(friendlyYokaiSprite, 4.0f, 0.76);
		_enemyIdleTween = CreateIdleBob(enemySprite, 3.0f, 0.95);
		_trainerIdleTween = CreateIdleBob(trainerSprite, 2.5f, 0.95);
		_friendlyBreathTween = CreateBreathing(friendlyYokaiSprite, 0.81);
		_enemyBreathTween = CreateBreathing(enemySprite, 1.05);
	}

	private Tween CreateBreathing(Sprite2D sprite, double halfCycle)
	{
		if (sprite == null)
			return null;
		Vector2 baseScale = sprite.Scale;
		Tween tween = CreateTween();
		tween.SetTrans(Tween.TransitionType.Sine);
		tween.SetEase(Tween.EaseType.InOut);
		tween.SetLoops();
		tween.TweenProperty(sprite, "scale", baseScale * new Vector2(1.025f, 0.98f), halfCycle);
		tween.TweenProperty(sprite, "scale", baseScale, halfCycle);
		return tween;
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
		if (GodotObject.IsInstanceValid(_friendlyBreathTween))
			_friendlyBreathTween.Kill();
		if (GodotObject.IsInstanceValid(_enemyBreathTween))
			_enemyBreathTween.Kill();
		if (GodotObject.IsInstanceValid(_trainerIdleTween))
			_trainerIdleTween.Kill();
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

		bool critical = random.NextDouble() < FriendlyCriticalChance;
		int actualDamage = critical ? (int)Math.Ceiling(damage * 1.5) : damage;
		if (critical)
			ShowCriticalHit(enemySprite, actualDamage, true);

		enemyHP = Math.Max(
			0,
			enemyHP - actualDamage
		);

		UpdateAllHP();
		PlaySound("Hit", -11);

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
		_pixelEffects?.Burst(friendlyYokaiSprite?.Position ?? new Vector2(320, 368), hostile: true);
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
		PlaySound("EnemyAttack", -9);


		// Enemy turns blue before attacking.

		if (enemySprite != null)
		{
			enemySprite.Modulate =
				new Color(0.35f, 0.65f, 1.0f, 1.0f);
		}

		await WaitSeconds(0.35);

		if (battleFinished)
			return;

		// Only the companion has HP, like a classic monster battle.
		Task hitReaction = Task.CompletedTask;
		await PlayEnemyLunge(() =>
		{
			bool critical = random.NextDouble() < EnemyCriticalChance;
			int actualDamage = critical ? EnemyDamage * 2 : EnemyDamage;
			if (critical)
				ShowCriticalHit(friendlyYokaiSprite, actualDamage, false);
			yokaiHP = Math.Max(0, yokaiHP - actualDamage);
			GD.Print("Enemy hit the friendly Yo-kai for " + actualDamage +
				(critical ? " (CRITICAL)." : "."));
			UpdateAllHP();
			hitReaction = FlashRed(friendlyYokaiSprite);
		});
		await hitReaction;
		if (battleFinished)
			return;

		GD.Print(
			$"Friendly Yo-kai HP: {yokaiHP}/{YokaiMaxHP}"
		);


		// DEFEAT CHECK

		if (yokaiHP <= 0)
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

	private void ShowCriticalHit(Sprite2D target, int damage, bool friendlyAttack)
	{
		if (canvasLayer == null)
			return;

		Vector2 targetPosition = target?.GlobalPosition ??
			(friendlyAttack ? new Vector2(1000, 400) : new Vector2(350, 430));
		var notice = new Label
		{
			Name = "CriticalHitPopup",
			Text = $"CRITICAL! {damage} DMG",
			Position = new Vector2(Math.Clamp(targetPosition.X - 120, 6, 1030),
				Math.Clamp(targetPosition.Y - 118, 160, 580)),
			Size = new Vector2(238, 34),
			HorizontalAlignment = HorizontalAlignment.Center,
			MouseFilter = Control.MouseFilterEnum.Ignore,
			ZIndex = 100
		};
		notice.AddThemeFontOverride("font", GD.Load<Font>("res://Assets/PressStart2P-Regular.ttf"));
		notice.AddThemeFontSizeOverride("font_size", 12);
		notice.AddThemeColorOverride("font_color",
			friendlyAttack ? new Color("ffde73") : new Color("ff7777"));
		notice.AddThemeColorOverride("font_outline_color", new Color("172230"));
		notice.AddThemeConstantOverride("outline_size", 5);
		canvasLayer.AddChild(notice);

		Tween floatUp = CreateTween();
		floatUp.SetTrans(Tween.TransitionType.Cubic);
		floatUp.SetEase(Tween.EaseType.Out);
		floatUp.TweenProperty(notice, "position", notice.Position + new Vector2(0, -42), 0.85);
		floatUp.Parallel().TweenProperty(notice, "modulate", new Color(1, 1, 1, 0), 0.85);
		floatUp.Finished += () =>
		{
			if (GodotObject.IsInstanceValid(notice))
				notice.QueueFree();
		};
		GD.Print((friendlyAttack ? "Your Yokai" : "Enemy Yokai") +
			" landed a critical hit for " + damage + " damage!");
	}


	// =========================================
	// 24. RUN
	// =========================================

	private async void OnRunPressed()
	{
		if (battleFinished || !playerTurn)
			return;

		playerTurn = false;

		HideAttackChoices();

		GD.Print("Attempting to escape...");
		PlaySound("Escape", -10);

		int escapeRoll = random.Next(1, 101);

		if (escapeRoll <= 70)
		{
			GD.Print("Escaped successfully!");

			battleFinished = true;
			await WaitSeconds(0.32);
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
	// 25. UPDATE YOKAI HP
	// =========================================

	private void UpdateAllHP()
	{
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
		if (_friendlyPercent != null)
			_friendlyPercent.Text = $"{(int)Math.Round(100.0 * yokaiHP / Math.Max(1, YokaiMaxHP))}%";
		if (_enemyPercent != null)
			_enemyPercent.Text = $"{(int)Math.Round(100.0 * enemyHP / Math.Max(1, EnemyMaxHP))}%";
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

		if (runButton != null)
			runButton.Disabled = true;
		UpdateUltimateUI();

		GD.Print("VICTORY!");
		PlaySound("Victory", -8);
		GD.Print("You defeated the enemy Yo-kai!");

		string path = GetTree().CurrentScene?.SceneFilePath ?? SceneFilePath;
		int level = GameProgress.LevelFromBattleScene(path);
		bool advanced = GameProgress.CompleteLevel(level);
		_victoryAdvanced = advanced;
		string message = advanced
			? level == GameProgress.TotalLevels
				? "All seven districts are clear! The town's lights are returning. Wawa is safe."
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

		if (runButton != null)
			runButton.Disabled = true;
		UpdateUltimateUI();

		GD.Print("DEFEAT!");
		PlaySound("Defeat", -8);
		GD.Print("Your team can no longer fight.");
		ShowResult("DEFEAT", "Your team can no longer fight. Return to Wawa and try again.");
	}

	private void ShowResult(string title, string message)
	{
		PackedScene resultScene = GD.Load<PackedScene>("res://Scenes/Battles/BattleResultUI.tscn");
		if (resultScene == null)
		{
			GD.PushError("Battle result overlay is missing.");
			ReturnToWorld();
			return;
		}

		var result = resultScene.Instantiate<BattleResultUI>();
		AddChild(result);
		bool ending = title == "VICTORY!" && _battleStage == GameProgress.TotalLevels;
		Action continuation = ending ? GoToEnding : ReturnToWorld;
		if (title == "VICTORY!" && _victoryAdvanced && _battleStage >= 2)
		{
			continuation = () =>
			{
				result.QueueFree();
				YokaiBondCutscene.Show(this, _battleStage - 1,
					ending ? GoToEnding : ReturnToWorld);
			};
		}
		result.Configure(title, message,
			ending ? "VIEW EPILOGUE" : "RETURN TO WAWA",
			_battleStage,
			continuation);
	}

	private void GoToEnding()
	{
		PlayerData.HasLoadedPosition = true;
		GetNode<SceneManager>("/root/SceneManager").TransitionTo("res://Scenes/Story/Ending.tscn");
	}

	private void ReturnToWorld()
	{
		// Preserve the last map position after a battle or successful escape.
		PlayerData.HasLoadedPosition = true;
		GetNode<SceneManager>("/root/SceneManager").TransitionTo("res://Scenes/World/world.tscn");
	}

	// Independent short-lived players let hit/attack sounds overlap naturally.
	// These intentionally use Master so the music slider only changes BGM.
	private void PlaySound(string fileName, float volumeDb = -9f)
	{
		AudioStream sound = GD.Load<AudioStream>($"res://Music/Sfx/{fileName}.wav");
		if (sound == null)
			return;
		var player = new AudioStreamPlayer
		{
			Stream = sound,
			VolumeDb = volumeDb,
			PitchScale = fileName == "Hit" ? 1f : 0.91f + 0.035f * (_battleStage - 1),
			ProcessMode = ProcessModeEnum.Pausable
		};
		AddChild(player);
		player.Finished += player.QueueFree;
		player.Play();
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
