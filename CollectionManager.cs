using Godot;

public partial class CollectionManager : Control
{
	private Control[] _characters;
	private int _currentIndex = 0;
	
	// UI Elements
	private Label _counterLabel;
	private Label _nameLabel;
	private Label _hpLabel;
	private Label _attackLabel;
	private Label _levelLabel;
	private Label _rsLevelLabel;

	// Mga data para sa 7 characters (maaari mong baguhin sa Inspector)
	[Export] private string[] _characterNames = new string[] 
	{ 
		"Character 1", "Character 2", "Character 3", "Character 4", 
		"Character 5", "Character 6", "Character 7" 
	};
	
	[Export] private int[] _hpValues = new int[] { 100, 120, 90, 150, 110, 130, 200 };
	[Export] private int[] _attackValues = new int[] { 20, 25, 30, 15, 22, 28, 35 };
	[Export] private int[] _levelValues = new int[] { 1, 1, 1, 1, 1, 1, 1 };
	[Export] private int[] _rsLevelValues = new int[] { 1, 2, 1, 3, 2, 1, 4 };

	public override void _Ready()
	{
		// 1. Kunin ang 7 character nodes mula sa scene
		_characters = new Control[]
		{
			GetNode<Control>("Character 1"),
			GetNode<Control>("Character 2"),
			GetNode<Control>("Character 3"),
			GetNode<Control>("Character 4"),
			GetNode<Control>("Character 5"),
			GetNode<Control>("Character 6"),
			GetNode<Control>("Character 7")
		};

		// 2. Kunin ang mga Label nodes mula sa scene
		_counterLabel = GetNodeOrNull<Label>("CounterLabel");
		_nameLabel = GetNodeOrNull<Label>("NameLabel");
		_hpLabel = GetNodeOrNull<Label>("HpLabel");
		_attackLabel = GetNodeOrNull<Label>("AttackLabel");
		_levelLabel = GetNodeOrNull<Label>("LevelLabel");
		_rsLevelLabel = GetNodeOrNull<Label>("RsLevelLabel");

		// 3. Ikonekta ang mga button (Siguraduhing 'Button1' ang pangalan ng kaliwang button sa Scene tree)
		var leftBtn = GetNodeOrNull<Button>("Button2"); 
		var rightBtn = GetNodeOrNull<Button>("Button");

		if (leftBtn != null) leftBtn.Pressed += OnLeftButtonPressed;
		if (rightBtn != null) rightBtn.Pressed += OnRightButtonPressed;

		UpdateDisplay();
	}

	private void UpdateDisplay()
	{
		// I-update ang visibility at alpha modulation ng mga character
		for (int i = 0; i < _characters.Length; i++)
		{
			_characters[i].Visible = true;
			
			Color c = _characters[i].Modulate;
			c.A = (i == _currentIndex) ? 1.0f : 0.0f;
			_characters[i].Modulate = c;
		}

		// I-update ang Counter (1/7, 2/7...)
		if (_counterLabel != null)
		{
			_counterLabel.Text = $"{_currentIndex + 1} / {_characters.Length}";
		}

		// I-update ang Character Name at Stats batay sa kasalukuyang index
		if (_nameLabel != null && _currentIndex < _characterNames.Length)
			_nameLabel.Text = _characterNames[_currentIndex];

		if (_hpLabel != null && _currentIndex < _hpValues.Length)
			_hpLabel.Text = $"HP: {_hpValues[_currentIndex]}";

		if (_attackLabel != null && _currentIndex < _attackValues.Length)
			_attackLabel.Text = $"Attack: {_attackValues[_currentIndex]}";

		if (_levelLabel != null && _currentIndex < _levelValues.Length)
			_levelLabel.Text = $"Level: {_levelValues[_currentIndex]}";

		if (_rsLevelLabel != null && _currentIndex < _rsLevelValues.Length)
			_rsLevelLabel.Text = $"RS Level: {_rsLevelValues[_currentIndex]}";
	}

	private void OnLeftButtonPressed()
	{
		_currentIndex--;
		if (_currentIndex < 0)
		{
			_currentIndex = _characters.Length - 1;
		}
		UpdateDisplay();
	}

	private void OnRightButtonPressed()
	{
		_currentIndex++;
		if (_currentIndex >= _characters.Length)
		{
			_currentIndex = 0;
		}
		UpdateDisplay();
	}
}
