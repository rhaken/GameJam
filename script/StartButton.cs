using Godot;
using System;

public partial class StartButton : Button
{
	public override void _Ready()
	{
		// Connect the pressed signal to our handler
		Pressed += OnButtonPressed;
	}
	
	private void OnButtonPressed()
	{
		// Start the game timer when button is pressed
		Global.StartTimer();
		GD.Print("Start button pressed - Game timer started!");
		
		// Hide the parent window popup
		if (GetParent() is Window parentWindow)
		{
			// If parent is a Window, hide it
			parentWindow.Hide();
			// Alternatively, you can close it completely
			// parentWindow.QueueFree();
		}
		else
		{
			// Try to find the window in the parent hierarchy
			Node current = this;
			while (current != null && !(current is Window))
			{
				current = current.GetParent();
			}
			
			if (current is Window window)
			{
				window.QueueFree();
			}
		}
	}
}
