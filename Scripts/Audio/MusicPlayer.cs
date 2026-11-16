using Godot;
using System;

// Shared music player. Music settings are saved outside individual game slots.
public partial class MusicPlayer : AudioStreamPlayer
{
	private const string PreferencesPath = "user://audio_settings.cfg";
	private const string WorldTrackPath = "res://Music/CORTIS (코르티스) REDRED Instrumental.ogg";
	private const string BattleTrackPath = "res://Music/BattleTheme.ogg";
	private const string OpeningTrackPath = "res://Music/WhisperPromise.ogg";
	private bool _enabled = true;
	private int _volumePercent = 20;
	private AudioStream _worldTrack;
	private AudioStream _battleTrack;
	private AudioStream _openingTrack;
	private Tween _musicFade;

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
		_openingTrack = GD.Load<AudioStream>(OpeningTrackPath);
		if (_openingTrack is AudioStreamOggVorbis openingLoop)
			openingLoop.Loop = true;
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

	private void OnSceneChanged()
	{
		// Godot's C# SceneChanged event is Action (no arguments).
		// Get the active scene from the tree after it changes.
		string path = GetTree().CurrentScene?.SceneFilePath ?? "";
		bool isBattle = path.EndsWith("BattleScene.tscn", StringComparison.OrdinalIgnoreCase);
		if (path.EndsWith("OpeningCutscene.tscn", StringComparison.OrdinalIgnoreCase))
		{
			PlayOpeningTheme();
			return;
		}
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
			VolumeDb = TargetVolumeDb;
			if (!Playing && Stream != null)
				Play();
		}
		else
			Stop();
	}

	public void SetVolumePercent(int percent)
	{
		_volumePercent = Mathf.Clamp(percent, 0, 100);
		if (GodotObject.IsInstanceValid(_musicFade))
			_musicFade.Kill();
		VolumeDb = TargetVolumeDb;
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

	private float TargetVolumeDb => Mathf.LinearToDb(Mathf.Max(_volumePercent / 100f, 0.0001f));

	public void PlayOpeningTheme() => PlayTrack(_openingTrack);

	public void PlayTrack(AudioStream track)
	{
		if (track == null || (track == Stream && Playing))
			return;
		if (GodotObject.IsInstanceValid(_musicFade))
			_musicFade.Kill();
		if (!_enabled)
		{
			Stop();
			Stream = track;
			return;
		}
		if (!Playing)
		{
			Stream = track;
			VolumeDb = TargetVolumeDb;
			Play();
			return;
		}

		// Fade down and up instead of cutting audio at every scene boundary.
		_musicFade = CreateTween().SetTrans(Tween.TransitionType.Sine);
		_musicFade.TweenProperty(this, "volume_db", -42f, 0.30);
		_musicFade.Finished += () =>
		{
			Stop();
			Stream = track;
			VolumeDb = -42f;
			if (!_enabled) return;
			Play();
			_musicFade = CreateTween().SetTrans(Tween.TransitionType.Sine);
			_musicFade.TweenProperty(this, "volume_db", TargetVolumeDb, 0.50);
		};
	}
}
