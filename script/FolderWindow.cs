using Godot;
using System;

public partial class FolderWindow : PanelContainer
{
	private bool dragging = false;
	private Vector2 dragOffset;
	private bool isFullscreen = false;
	private Vector2 originalSize;
	private Vector2 originalPosition;

	public override void _Ready()
	{
		originalSize = Size;
		originalPosition = Position;

		GetNode<Button>("TitleBar/CloseButton").Pressed += () => Hide();
		GetNode<Button>("TitleBar/MaximizeButton").Pressed += ToggleFullscreen;
		GetNode<Button>("TitleBar/MinimizeButton").Pressed += () => Visible = false;
		GetNode<Control>("TitleBar").GuiInput += OnTitleBarInput;
	}

	private void OnTitleBarInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				dragging = true;
				dragOffset = GetGlobalMousePosition() - GlobalPosition;
			}
			else if (!mouseEvent.Pressed)
			{
				dragging = false;
			}
		}
		else if (@event is InputEventMouseMotion motion && dragging)
		{
			GlobalPosition = GetGlobalMousePosition() - dragOffset;
		}
	}

	private void ToggleFullscreen()
	{
		if (isFullscreen)
		{
			Size = originalSize;
			Position = originalPosition;
			isFullscreen = false;
		}
		else
		{
			originalSize = Size;
			originalPosition = Position;
			Size = GetViewportRect().Size;
			Position = Vector2.Zero;
			isFullscreen = true;
		}
	}
}
