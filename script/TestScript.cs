using Godot;
using System;

public partial class TestScript : TextureButton
{
	public override void _Ready() {
		
	}
	
	private void _on_mouse_entered() {
		GD.Print("lewat");
	}
}
