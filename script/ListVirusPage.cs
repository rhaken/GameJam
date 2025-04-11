using Godot;
using System;

public partial class ListVirusPage : Control
{
	[Export] private Label nameLabel;
	[Export] private Label descLabel;
	[Export] private TextureRect imageRect;
	
	private Color normalColor = new Color(0.2f, 0.4f, 1.0f);
	private Color hoverColor = new Color(0.1f, 0.3f, 0.8f);

	public override void _Ready()
	{
		nameLabel.AddThemeColorOverride("font_color", normalColor);
		nameLabel.AutowrapMode = TextServer.AutowrapMode.Off;
		nameLabel.Size = nameLabel.GetMinimumSize();
		nameLabel.MouseDefaultCursorShape = Control.CursorShape.PointingHand;
		
		nameLabel.MouseEntered += OnNameLabelMouseEntered;
		nameLabel.MouseExited += OnNameLabelMouseExited;
		
		nameLabel.GuiInput += OnNameLabelGuiInput;
	}

	public void SetVirus(VirusData virus)
	{
		nameLabel.Text = virus.Name;
		descLabel.Text = virus.Description;
		imageRect.Texture = GD.Load<Texture2D>(virus.ImagePath);
	}
	
	private void OnNameLabelMouseEntered()
	{
		nameLabel.AddThemeColorOverride("font_color", hoverColor);
	}

	private void OnNameLabelMouseExited()
	{
		nameLabel.AddThemeColorOverride("font_color", normalColor);
	}
	
	private void OnNameLabelGuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				GD.Print($"Klik pada virus: {nameLabel.Text}");
				// Bisa trigger event / buka detail di sini
			}
		}
	}
}
