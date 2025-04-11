using Godot;
using System;

public partial class IsiFolder : Control
{
	[Export] private TextureRect iconRect;
	[Export] private Label nameLabel;

	private Color normalColor = new Color(0f, 0f, 0f);
	private Color hoverColor = new Color(0f, 0.4f, 1f);

	public override void _Ready()
	{
		nameLabel.AddThemeColorOverride("font_color", normalColor);
		nameLabel.MouseDefaultCursorShape = CursorShape.PointingHand;

		nameLabel.MouseEntered += OnLabelMouseEntered;
		nameLabel.MouseExited += OnLabelMouseExited;
		nameLabel.GuiInput += OnLabelGuiInput;
	}

	public void SetFile(string fileName, string fileIconPath)
	{
		nameLabel.Text = fileName;

		if (ResourceLoader.Exists(fileIconPath))
		{
			iconRect.Texture = GD.Load<Texture2D>(fileIconPath);
		}
		else
		{
			GD.PrintErr($"Icon not found: {fileIconPath}");
		}
	}

	private void OnLabelMouseEntered()
	{
		nameLabel.AddThemeColorOverride("font_color", hoverColor);
	}

	private void OnLabelMouseExited()
	{
		nameLabel.AddThemeColorOverride("font_color", normalColor);
	}

	private void OnLabelGuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent &&
			mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			GD.Print($"Clicked file: {nameLabel.Text}");
			// Bisa dipanggil fungsi buka file / event lain di sini
		}
	}
}
