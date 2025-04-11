using Godot;
using System;
public partial class PopupAdd : Window
{
	// Path to the sound effect file
	private const string SfxPath = "res://assets/sound/notification-sound-7062.mp3"; 
	
	// AudioStreamPlayer node for playing sounds
	private AudioStreamPlayer _audioPlayer;
	
	public override void _Ready()
	{
		Global.CpuProcess += 5;
		
		// Create an AudioStreamPlayer node
		_audioPlayer = new AudioStreamPlayer();
		AddChild(_audioPlayer);
		
		// Load and play the sound effect
		PlaySfx(SfxPath);
	}
	
	private void _on_close_requested()
	{
		Global.CpuProcess -= 5;
		QueueFree();
	}
	
	// Method to play sound effects
	private void PlaySfx(string path)
	{
		// Check if the file exists
		if (ResourceLoader.Exists(path))
		{
			// Load the audio file
			var audioStream = ResourceLoader.Load<AudioStream>(path);
			if (audioStream != null)
			{
				// Set the stream and play it
				_audioPlayer.Stream = audioStream;
				_audioPlayer.Play();
				GD.Print($"[Sound] Playing sound: {path}");
			}
			else
			{
				GD.Print($"[Sound] Error: Could not load audio stream from {path}");
			}
		}
		else
		{
			GD.Print($"[Sound] Error: Audio file not found at {path}");
		}
	}
}
