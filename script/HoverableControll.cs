using Godot;
using System;

public partial class HoverableControl : Control
{
	[Export] public NodePath HoverIndicatorPath;
	[Export(PropertyHint.File, "*.tscn")] public string TargetScenePath;

	private ColorRect hoverIndicator;
	private Color originalColor;

	private double lastClickTime = 0;
	private const double doubleClickThreshold = 0.3;

	public override void _Ready()
	{
		if (HoverIndicatorPath != null && HasNode(HoverIndicatorPath))
		{
			hoverIndicator = GetNode<ColorRect>(HoverIndicatorPath);
			originalColor = hoverIndicator.Color;
			hoverIndicator.Visible = false;
		}

		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
	}

	private void OnMouseEntered()
	{
		if (hoverIndicator != null)
			hoverIndicator.Visible = true;
	}

	private void OnMouseExited()
	{
		if (hoverIndicator != null)
			hoverIndicator.Visible = false;
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				double currentTime = Time.GetTicksMsec() / 1000.0;

				if (hoverIndicator != null)
					hoverIndicator.Color = originalColor.Darkened(0.2f);

				if (currentTime - lastClickTime < doubleClickThreshold)
				{
					if (!string.IsNullOrEmpty(TargetScenePath))
					{
						var scene = GD.Load<PackedScene>(TargetScenePath);
						if (scene != null)
						{
							var instance = scene.Instantiate();
							GetTree().Root.AddChild(instance);
						}
						else
						{
							GD.PrintErr("Failed to load scene: " + TargetScenePath);
						}
					}
				}

				lastClickTime = currentTime;
			}
			else if (!mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				if (hoverIndicator != null)
					hoverIndicator.Color = originalColor;
			}
		}
	}
}
