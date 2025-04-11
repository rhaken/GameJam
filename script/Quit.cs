using Godot;
using System;
public partial class Quit : Button
{
	public override void _Ready()
	{
		// Connect the pressed signal to our handler
		Pressed += OnQuitButtonPressed;
	}

	private void OnQuitButtonPressed()	
	{
		// Print debug message
		GD.Print("[DEBUG] Quit button pressed - Exiting game");
		
		// Stop the timer if it's running
		if (Global.IsStarted)
		{
			Global.StopTimer();
			GD.Print("[DEBUG] Game timer stopped before exit");
		}
		
		// Quit the application completely
		GetTree().Quit();
	}
}
