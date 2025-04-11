using Godot;
using System;

public partial class Retry : Button
{
	public override void _Ready()
	{
		// Connect the pressed signal to our handler
		Pressed += OnRetryPressed;
	}

	private void OnRetryPressed()
	{
		// Reset the game timer
		Global.ResetTimer();
		
		// Set the game to started state
		Global.StartTimer();
		
		GD.Print("Retry button pressed - Game timer reset and restarted!");
		
		// Find the parent Window (looking at your scene tree)
		Node currentNode = this;
		
		// Keep moving up the tree until we find the Window node
		while (currentNode != null && !(currentNode is Window))
		{
			currentNode = currentNode.GetParent();
		}
		
		// If we found the Window node, close it
		if (currentNode is Window windowNode)
		{
			GD.Print("Closing WinScreen window");
			windowNode.Hide(); // Hide the window
			
			// You can also use QueueFree to completely remove it if needed
			// windowNode.QueueFree();
		}
		else
		{
			// This should not happen based on your scene structure, but good for debugging
			GD.Print("Warning: Could not find Window parent node");
			
			// Print the node tree for debugging
			Node parent = GetParent();
			string path = GetPath();
			GD.Print($"Current node path: {path}");
			while (parent != null)
			{
				GD.Print($"Parent: {parent.Name} (Type: {parent.GetType()})");
				parent = parent.GetParent();
			}
		}
	}
}
