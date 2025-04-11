using Godot;
using System;
public partial class BlueScreen : ColorRect
{
	// Path to the blue screen error sound effect
	private const string SfxPath = "res://assets/sound/buzzing-sound-271564.mp3"; // Update this path to your actual sound file
	
	// AudioStreamPlayer for playing the sound
	private AudioStreamPlayer _audioPlayer;
	
	public override void _Ready()
	{
		// Create an AudioStreamPlayer node
		_audioPlayer = new AudioStreamPlayer();
		AddChild(_audioPlayer);
		
		// Load and play the sound effect
		PlaySfx(SfxPath);
		
		// Optional: Make the blue screen sound a bit louder than other sounds
		_audioPlayer.VolumeDb = 2.0f;
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
				GD.Print($"[BlueScreen] Playing error sound: {path}");
			}
			else
			{
				GD.Print($"[BlueScreen] Error: Could not load audio stream from {path}");
			}
		}
		else
		{
			GD.Print($"[BlueScreen] Error: Audio file not found at {path}");
		}
	}
}
