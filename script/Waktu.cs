using Godot;
using System;

public partial class Waktu : Label
{
	public override void _Ready()
	{
		// Initialize the label with the current time at startup
		UpdateTimeDisplay();
	}
	
	public override void _Process(double delta)
	{
		// Update the time display every frame
		UpdateTimeDisplay();
	}
	
	private void UpdateTimeDisplay()
	{
		// Get the elapsed time from Global
		double elapsedTime = Global.ElapsedTime;
		
		// Convert to TimeSpan
		TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedTime);
		
		// Format the time manually without using custom format strings
		string minutes = timeSpan.Minutes.ToString("00");
		string seconds = timeSpan.Seconds.ToString("00");
		
		// Create the time text
		string timeText = $"{minutes}:{seconds}";
		
		// Update the label text
		Text = timeText;
	}
}
