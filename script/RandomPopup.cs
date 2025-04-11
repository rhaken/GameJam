using Godot;
using System;

public partial class RandomPopup : TextureRect
{
	public override void _Ready()
	{
		string[] imageFiles = {
			"res://assets/icon/adds/1.png",
			"res://assets/icon/adds/2.png",
			"res://assets/icon/adds/3.png"
		};

		Random random = new Random();
		int index = random.Next(imageFiles.Length);

		Texture2D randomTexture = GD.Load<Texture2D>(imageFiles[index]);
		Texture = randomTexture;
	}
}
