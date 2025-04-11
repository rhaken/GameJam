using Godot;
using System;

public partial class ButtonWeb : TextureButton
{
	[Export] private PackedScene sceneToLoad;
	[Export] private Node rootNode;

	public override void _Ready()
	{
		Pressed += OnButtonPressed;
	}

	private void OnButtonPressed()
	{
		// Ambil root (biasanya "WebBrowser")
		Node root = rootNode ?? GetTree().Root.GetChild(0);
		Control controlNode = root.GetNode<Control>("Control");

		// Buat node kosong untuk menyimpan tampilan sekarang (kecuali BackButton)
		Node tempRoot = new Node();

		foreach (Node child in controlNode.GetChildren())
		{
			if (child.Name == "BackButton")
				continue;

			controlNode.RemoveChild(child);
			tempRoot.AddChild(child);
		}

		// Simpan tampilan sebelumnya ke queue
		PackedScene packed = new PackedScene();
		packed.Pack(tempRoot);
		Global.SceneQueue.Enqueue(packed);
		GD.Print("Scene saved to queue. Queue size: " + Global.SceneQueue.Count);

		// Load halaman baru dari sceneToLoad saja
		LoadSceneToRoot();
	}

	private void LoadSceneToRoot()
	{
		Node root = rootNode ?? GetTree().Root.GetChild(0);
		Control controlNode = root.GetNode<Control>("Control");
		Node backButton = controlNode.GetNodeOrNull("BackButton");

		// Hapus semua kecuali BackButton
		foreach (Node child in controlNode.GetChildren())
		{
			if (child.Name == "BackButton")
				continue;

			controlNode.RemoveChild(child);
			child.QueueFree();
		}

		// Pakai hanya sceneToLoad
		if (sceneToLoad != null)
		{
			Node newPage = sceneToLoad.Instantiate();
			controlNode.AddChild(newPage);
			GD.Print("Loaded scene from sceneToLoad.");
		}
		else
		{
			GD.PrintErr("sceneToLoad is null. Cannot load new scene.");
		}

		// Pastikan BackButton tetap paling atas
		if (backButton != null)
		{
			controlNode.MoveChild(backButton, controlNode.GetChildCount() - 1);
		}
	}
}
