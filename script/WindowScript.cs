using Godot;
using System;

public partial class WindowScript : Window
{
	private TextureButton backButton;

	public override void _Ready()
	{
		// Cek kalau node BackButton ada, baru dipasang handler
		if (HasNode("BackButton"))
		{
			backButton = GetNode<TextureButton>("BackButton");
			backButton.Pressed += OnBackPressed;
		}
	}

	private void _on_close_requested()
	{
		QueueFree();
	}

	private void OnBackPressed()
	{
		if (Global.SceneQueue.Count > 0)
		{
			PackedScene previousScene = Global.SceneQueue.Dequeue();

			// Hapus semua child kecuali BackButton
			foreach (Node child in GetChildren())
			{
				if (child != backButton)
				{
					RemoveChild(child);
					child.QueueFree();
				}
			}

			// Tambahkan scene sebelumnya sebagai child
			Node previousPage = previousScene.Instantiate();
			AddChild(previousPage);
		}
		else
		{
			GD.PrintErr("Tidak ada scene sebelumnya.");
		}
	}
}
