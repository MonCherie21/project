using Godot;

// Shared background music player. Audio preferences are independent of save slots.
// Battle music can switch tracks through PlayTrack during the combat overhaul.
public partial class MusicPlayer : AudioStreamPlayer
{
	private const string PreferencesPath = "user://audio_settings.cfg";
	private bool _enabled = true;

	public bool Enabled => _enabled;

	public override void _Ready()
	{
		var settings = new ConfigFile();
		if (settings.Load(PreferencesPath) == Error.Ok)
			_enabled = (bool)settings.GetValue("audio", "music_enabled", true);

		if (!_enabled)
			Stop();
		else if (!Playing && Stream != null)
			Play();
	}

	public void SetEnabled(bool enabled)
	{
		_enabled = enabled;
		var settings = new ConfigFile();
		settings.SetValue("audio", "music_enabled", _enabled);
		Error result = settings.Save(PreferencesPath);
		if (result != Error.Ok)
			GD.PushWarning("Could not save music setting: " + result);

		if (_enabled)
		{
			if (!Playing && Stream != null)
				Play();
		}
		else
			Stop();
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
