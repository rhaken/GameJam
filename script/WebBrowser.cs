using Godot;
using System;

public partial class WebBrowser : TextureButton
{
	private ColorRect hoverIndicator;

	public override void _Ready()
	{
		hoverIndicator = GetNode<ColorRect>("ColorRect");
		hoverIndicator.Visible = false;
	}

	private void OnMouseEntered()
	{
		hoverIndicator.Visible = true;
	}

	private void OnMouseExited()
	{
		hoverIndicator.Visible = false;
	}
}
