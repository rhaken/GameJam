using Godot;
using System;
using System.Collections.Generic;

public partial class HoverableButton : TextureButton
{
	[Export] public NodePath HoverIndicatorPath;
	[Export] public NodePath SelectedIndicatorPath;
	[Export] public int BanyakClick = 2;
	[Export(PropertyHint.File, "*.tscn")] public string TargetScenePath;
	[Export] public NodePath TargetParentNodePath;
	
	// MainFolder reference
	[Export] public NodePath MainFolderPath;
	
	// NodePath untuk tombol yang akan diambil namanya
	[Export] public NodePath ButtonNodePath;
	
	// Bisa dibuat sebagai export arrays jika ingin mengkonfigurasi dari Inspector
	[Export] public string FileIconPath = "res://assets/icon/download.png";
	
	// Tambahkan enum untuk jenis button
	[Export] public ButtonType Type = ButtonType.Default;
	
	private ColorRect hoverIndicator;
	private ColorRect selectedIndicator;
	private Color originalColor;
	private static HoverableButton selectedButton;
	private double lastClickTime = 0;
	private const double doubleClickThreshold = 0.3;
	
	private MainFolder mainFolder;
	
	// Enum untuk jenis button
	public enum ButtonType
	{
		Default,
		Video,
		Image,
		Download,
		Pictures,
		System
	}
	
	public override void _Ready()
	{
		if (HoverIndicatorPath != null && HasNode(HoverIndicatorPath) && SelectedIndicatorPath != null && HasNode(SelectedIndicatorPath))
		{
			hoverIndicator = GetNode<ColorRect>(HoverIndicatorPath);
			selectedIndicator = GetNode<ColorRect>(SelectedIndicatorPath);
			originalColor = hoverIndicator.Color;
			hoverIndicator.Visible = false;
			selectedIndicator.Visible = false;
		}
		
		// Dapatkan referensi ke MainFolder
		if (MainFolderPath != null && HasNode(MainFolderPath))
		{
			mainFolder = GetNode<MainFolder>(MainFolderPath);
		}
		else
		{
			// Coba temukan MainFolder dengan path relatif ke Videos/MainSection
			// Sesuaikan path ini dengan struktur hierarki scene Anda
			var videosNode = GetTree().Root.GetNodeOrNull("YourMainScene/Videos");
			if (videosNode != null)
			{
				mainFolder = videosNode.GetNodeOrNull<MainFolder>("MainSection");
			}
		}
		
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				if (selectedButton != null && selectedButton != this)
				{
					// Check if the selected button is still valid
					if (IsInstanceValid(selectedButton) && !selectedButton.IsQueuedForDeletion())
					{
						selectedButton.selectedIndicator.Visible = false;
						selectedButton = null;
					}
				}
			}
		}
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
				HandleSelection();
				
				// Mengatur isi MainFolder sesuai dengan tombol yang ditekan
				UpdateMainFolder();
				
				if ((currentTime - lastClickTime < doubleClickThreshold) || BanyakClick == 1)
				{
					if (!string.IsNullOrEmpty(TargetScenePath))
					{
						var scene = GD.Load<PackedScene>(TargetScenePath);
						if (scene != null)
						{
							var instance = scene.Instantiate();
							
							if (TargetParentNodePath != null && HasNode(TargetParentNodePath))
							{
								GetNode(TargetParentNodePath).AddChild(instance);
							}
							else
							{
								GetTree().Root.AddChild(instance);
							}
						}
						else
						{
							GD.PrintErr("Failed to load scene: " + TargetScenePath);
						}
					}
				}
				lastClickTime = currentTime;
			}
		}
	}
	
	private void HandleSelection()
	{
		if (selectedButton != null && selectedButton != this)
		{
			// Check if the previous selected button is still valid before using it
			if (IsInstanceValid(selectedButton) && !selectedButton.IsQueuedForDeletion())
			{
				selectedButton.selectedIndicator.Visible = false;
			}
		}
		selectedButton = this;
		if (selectedIndicator != null)
		{
			selectedIndicator.Visible = true;
		}
	}
	
	private void UpdateMainFolder()
	{
		if (mainFolder == null)
		{
			GD.PrintErr("MainFolder tidak ditemukan. Pastikan MainFolderPath dikonfigurasi dengan benar.");
			return;
		}
		
		// Ambil nama dari node yang ditentukan di ButtonNodePath atau gunakan nama node ini sendiri
		string buttonName = Name;
		
		// Jika ButtonNodePath diatur, gunakan nama node tersebut
		if (ButtonNodePath != null && !ButtonNodePath.IsEmpty && HasNode(ButtonNodePath))
		{
			Node buttonNode = GetNode(ButtonNodePath);
			buttonName = buttonNode.Name;
		}
		
		// Debug info
		GD.Print($"[HoverableButton] UpdateMainFolder called with button name: {buttonName}, Type: {Type}");
		
		// Cek berdasarkan nama button atau Type dan gunakan CurrentFiles yang sesuai dari Global
		if (buttonName == "Downloads" || Type == ButtonType.Download)
		{
			GD.Print($"[HoverableButton] Setting files to Download category");
			mainFolder.SetFiles(Global.CurrentDownloadFiles);
		}
		else if (buttonName == "Videos" || Type == ButtonType.Video)
		{
			GD.Print($"[HoverableButton] Setting files to Video category");
			mainFolder.SetFiles(Global.CurrentVideoFiles);
		}
		else if (buttonName == "Pictures" || buttonName == "Images" || Type == ButtonType.Image || Type == ButtonType.Pictures)
		{
			GD.Print($"[HoverableButton] Setting files to Picture category");
			mainFolder.SetFiles(Global.CurrentPictureFiles);
		}
		else if (buttonName == "Music")
		{
			GD.Print($"[HoverableButton] Setting files to Music category");
			mainFolder.SetFiles(Global.CurrentMusicFiles);
		}
		else if (buttonName == "System" || buttonName == "ThisPc" || buttonName == "This PC" || Type == ButtonType.System)
		{
			GD.Print(Global.CurrentSystemFiles);
			GD.Print($"[HoverableButton] Setting files to System category");
			mainFolder.SetFiles(Global.CurrentSystemFiles);
		}
		else
		{
			GD.Print($"[HoverableButton] No category matched, using current files");
			// Default fallback - use current files dari kategori yang aktif saat ini
			mainFolder.SetFiles(Global.GetCurrentFiles());
		}
	}
}
