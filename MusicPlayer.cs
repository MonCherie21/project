using Godot;
using System;

// Shared music player. Music settings are saved outside individual game slots.
public partial class MusicPlayer : AudioStreamPlayer
{
	private const string PreferencesPath = "user://audio_settings.cfg";
	private const string WorldTrackPath = "res://Music/CORTIS (코르티스) REDRED Instrumental.ogg";
	private const string BattleTrackPath = "res://Music/BattleTheme.ogg";
	private bool _enabled = true;
	private int _volumePercent = 20;
	private AudioStream _worldTrack;
	private AudioStream _battleTrack;

	public bool Enabled => _enabled;
	public int VolumePercent => _volumePercent;

	public override void _Ready()
	{
		var settings = new ConfigFile();
		if (settings.Load(PreferencesPath) == Error.Ok)
		{
			_enabled = (bool)settings.GetValue("audio", "music_enabled", true);
			_volumePercent = Mathf.Clamp((int)settings.GetValue("audio", "volume_percent", 20), 0, 100);
		}
		VolumeDb = Mathf.LinearToDb(Mathf.Max(_volumePercent / 100f, 0.0001f));
		_worldTrack = GD.Load<AudioStream>(WorldTrackPath) ?? Stream;
		_battleTrack = GD.Load<AudioStream>(BattleTrackPath);
		GetTree().SceneChanged += OnSceneChanged;

		if (!_enabled)
			Stop();
		else if (!Playing && Stream != null)
			Play();
	}

	public override void _ExitTree()
	{
		GetTree().SceneChanged -= OnSceneChanged;
	}

	private void OnSceneChanged(Node scene)
	{
		string path = scene?.SceneFilePath ?? "";
		bool isBattle = path.EndsWith("BattleScene.tscn", StringComparison.OrdinalIgnoreCase);
		for (int i = 2; i <= 7 && !isBattle; i++)
			isBattle = path.EndsWith($"BattleScene{i}.tscn", StringComparison.OrdinalIgnoreCase);
		PlayTrack(isBattle ? _battleTrack : _worldTrack);
	}

	public void SetEnabled(bool enabled)
	{
		_enabled = enabled;
		SavePreferences();

		if (_enabled)
		{
			if (!Playing && Stream != null)
				Play();
		}
		else
			Stop();
	}

	public void SetVolumePercent(int percent)
	{
		_volumePercent = Mathf.Clamp(percent, 0, 100);
		VolumeDb = Mathf.LinearToDb(Mathf.Max(_volumePercent / 100f, 0.0001f));
		SavePreferences();
	}

	private void SavePreferences()
	{
		var settings = new ConfigFile();
		settings.SetValue("audio", "music_enabled", _enabled);
		settings.SetValue("audio", "volume_percent", _volumePercent);
		Error result = settings.Save(PreferencesPath);
		if (result != Error.Ok)
			GD.PushWarning("Could not save music setting: " + result);
	}

	public void PlayTrack(AudioStream track)
	{
		if (track == Stream && Playing)
			return;
		Stop();
		Stream = track;
		if (_enabled && Stream != null)
			Play();
	}
}
