using Godot;
using System;
using System.Collections.Generic;

public partial class MainFolder : VBoxContainer
{
	[Export] public PackedScene FileItemScene; // Scene untuk IsiFolder
	
	private List<FileData> files = new List<FileData>();
	private PopupMenu contextMenu;
	private Node currentRightClickedItem;
	private FileData currentRightClickedFileData; // Tambahkan referensi ke FileData yang diklik
	private Control hoveredItem; // Variable to track the currently hovered item
	private ScrollContainer scrollContainer; // ScrollContainer for scrollable content
	private VBoxContainer contentContainer; // Container to hold actual file items
	
	public override void _Ready()
	{
		// Setup scroll container
		scrollContainer = new ScrollContainer();
		scrollContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		scrollContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
		scrollContainer.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled; // Disable horizontal scrolling
		scrollContainer.VerticalScrollMode = ScrollContainer.ScrollMode.Auto; // Enable vertical scrolling
		
		// Setup content container
		contentContainer = new VBoxContainer();
		contentContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		contentContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
		
		// Add content container to scroll container
		scrollContainer.AddChild(contentContainer);
		
		// Add scroll container to main container (this)
		AddChild(scrollContainer);
		
		// Buat popup menu
		contextMenu = new PopupMenu();
		AddChild(contextMenu);
		
		// Tambahkan item "Delete" ke popup menu
		contextMenu.AddItem("Delete", 0);
		
		// Connect signal
		contextMenu.IdPressed += OnContextMenuItemPressed;
	}
	
	// Metode untuk mengatur isi file
	public void SetFiles(List<FileData> fileList)
	{
		GD.Print($"[MainFolder] SetFiles called with {fileList.Count} files");
		files = new List<FileData>(fileList); // Buat salinan list untuk menghindari referensi ke list yang sama
		RefreshFiles();
	}
	
	// Metode untuk memperbarui tampilan file
	private void RefreshFiles()
	{
		// Hapus semua node anak dari content container
		foreach (var child in contentContainer.GetChildren())
		{
			child.QueueFree();
		}
		
		// Reset hover status
		hoveredItem = null;
		
		// Debug info
		GD.Print($"[MainFolder] RefreshFiles: Menampilkan {files.Count} files");
		
		// Tambahkan file-file ke content container
		foreach (var file in files)
		{
			if (FileItemScene != null)
			{
				var fileItem = FileItemScene.Instantiate() as Control;
				if (fileItem is IsiFolder isiFolder)
				{
					// Add hover indicator (ColorRect)
					var hoverIndicator = new ColorRect();
					hoverIndicator.Color = new Color(0.2f, 0.4f, 1.0f, 0.5f); // Brighter blue with more opacity
					hoverIndicator.MouseFilter = Control.MouseFilterEnum.Ignore; // Ignore mouse events
					hoverIndicator.Visible = false; // Hide by default
					hoverIndicator.SetAnchorsPreset(Control.LayoutPreset.FullRect); // Fill the entire control
					hoverIndicator.SizeFlagsHorizontal = SizeFlags.Fill;
					hoverIndicator.SizeFlagsVertical = SizeFlags.Fill;
					
					// Add the hover indicator as first child (will be behind other elements)
					fileItem.AddChild(hoverIndicator);
					// Move the indicator to be the first child so it appears behind other elements
					fileItem.MoveChild(hoverIndicator, 0);
					
					// Store reference to the hover indicator
					fileItem.SetMeta("hover_indicator", hoverIndicator);
					
					// Store reference to the file name only (tidak bisa menyimpan objek FileData di Meta)
					fileItem.SetMeta("file_name", file.Name);
					
					isiFolder.SetFile(file.Name, file.IconPath);
					contentContainer.AddChild(fileItem);
					
					// Connect signal GuiInput untuk menangkap klik kanan dan mouse hover
					fileItem.GuiInput += (InputEvent @event) => OnFileItemInput(@event, fileItem, file);
					
					// Connect mouse entered and exited signals
					fileItem.MouseEntered += () => OnMouseEntered(fileItem);
					fileItem.MouseExited += () => OnMouseExited(fileItem);
				}
			}
			else
			{
				// Buat container untuk file
				var hbox = new HBoxContainer();
				
				// Add hover indicator (ColorRect)
				var hoverIndicator = new ColorRect();
				hoverIndicator.Color = new Color(0.2f, 0.4f, 1.0f, 0.5f); // Brighter blue with more opacity
				hoverIndicator.MouseFilter = Control.MouseFilterEnum.Ignore; // Ignore mouse events
				hoverIndicator.Visible = false; // Hide by default
				hoverIndicator.SetAnchorsPreset(Control.LayoutPreset.FullRect); // Fill the entire control
				hoverIndicator.SizeFlagsHorizontal = SizeFlags.Fill;
				hoverIndicator.SizeFlagsVertical = SizeFlags.Fill;
				
				// Add the hover indicator as first child
				hbox.AddChild(hoverIndicator);
				
				// Store reference to the hover indicator
				hbox.SetMeta("hover_indicator", hoverIndicator);
				
				// Buat icon file
				var iconRect = new TextureRect();
				if (ResourceLoader.Exists(file.IconPath))
				{
					iconRect.Texture = GD.Load<Texture2D>(file.IconPath);
				}
				iconRect.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
				iconRect.CustomMinimumSize = new Vector2(24, 24);
				
				// Buat label file
				var nameLabel = new Label();
				nameLabel.Text = file.Name;
				nameLabel.AddThemeColorOverride("font_color", Colors.Black);
				
				// Tambahkan ke HBox
				hbox.AddChild(iconRect);
				hbox.AddChild(nameLabel);
				
				// Tambahkan ke content container
				contentContainer.AddChild(hbox);
				
				// Simpan referensi nama file di hbox
				hbox.SetMeta("file_name", file.Name);
				
				// Menyimpan nama file saja karena objek FileData tidak bisa disimpan di Meta
				// Nama file sudah ditambahkan di baris sebelumnya
				
				// Connect signal GuiInput untuk menangkap klik kanan
				hbox.GuiInput += (InputEvent @event) => OnFileItemInput(@event, hbox, file);
				
				// Connect mouse entered and exited signals
				hbox.MouseEntered += () => OnMouseEntered(hbox);
				hbox.MouseExited += () => OnMouseExited(hbox);
			}
		}
	}
	
	// Mouse entered event handler
	private void OnMouseEntered(Control item)
	{
		// Show the hover indicator for this item
		if (item.HasMeta("hover_indicator"))
		{
			var indicatorVariant = item.GetMeta("hover_indicator");
			var indicator = indicatorVariant.As<ColorRect>();
			if (indicator != null)
			{
				indicator.Visible = true;
				
				// Make sure the indicator is properly sized
				indicator.SetAnchorsPreset(Control.LayoutPreset.FullRect);
				indicator.Position = new Vector2(0, 0);
				indicator.Size = item.Size;
			}
		}
		
		// Update the currently hovered item
		hoveredItem = item;
	}
	
	// Mouse exited event handler
	private void OnMouseExited(Control item)
	{
		// Hide the hover indicator for this item
		if (item.HasMeta("hover_indicator"))
		{
			var indicatorVariant = item.GetMeta("hover_indicator");
			var indicator = indicatorVariant.As<ColorRect>();
			if (indicator != null)
			{
				indicator.Visible = false;
			}
		}
		
		// Clear the hovered item reference
		if (hoveredItem == item)
		{
			hoveredItem = null;
		}
	}
	
	// Metode untuk menangani input pada item file
	private void OnFileItemInput(InputEvent @event, Control fileItem, FileData fileData)
	{
		// Cek apakah event adalah mouse button dan klik kanan
		if (@event is InputEventMouseButton mouseEvent)
		{
			GD.Print($"Mouse button {mouseEvent.ButtonIndex} detected on file {fileData.Name}");
			
			if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
			{
				GD.Print($"Right click detected on {fileData.Name}!");
				
				// Simpan item yang diklik kanan
				currentRightClickedItem = fileItem;
				currentRightClickedFileData = fileData; // Simpan referensi ke fileData
				
				// Dapatkan posisi mouse global
				Vector2I mousePos = DisplayServer.MouseGetPosition();
				
				// Konversi posisi mouse global ke posisi lokal relatif terhadap window
				Vector2I windowPosition = DisplayServer.WindowGetPosition();
				Vector2I localMousePos = mousePos - windowPosition;
				
				// Tambahkan offset jika diperlukan (posisi relatif ScrollContainer)
				// Vector2 scrollOffset = scrollContainer.GetScrollOffset();
				// localMousePos += new Vector2I((int)scrollOffset.X, (int)scrollOffset.Y);
				
				// Buat atau Reset popup menu
				if (contextMenu == null)
				{
					contextMenu = new PopupMenu();
					AddChild(contextMenu);
					contextMenu.AddItem("Delete", 0);
					contextMenu.IdPressed += OnContextMenuItemPressed;
				}
				
				// Set posisi dan tampilkan
				contextMenu.Position = localMousePos;
				// PopupMenu tidak memiliki method Reset() di Godot
				contextMenu.Popup();
				
				// Debug
				GD.Print($"Menampilkan popup menu untuk file: {fileData.Name}");
				GD.Print($"Window position: {windowPosition}, Mouse position: {mousePos}, Local mouse: {localMousePos}");
				
				// Consume event untuk mencegah propagasi
				GetViewport().SetInputAsHandled();
			}
		}
	}
	
	// Metode untuk menangani item yang dipilih dari popup menu
	private void OnContextMenuItemPressed(long id)
	{
		GD.Print($"Context menu item {id} pressed");
		
		if (id == 0) // Delete
		{
			if (currentRightClickedItem != null)
			{
				GD.Print($"Attempting to delete file");
				
				string fileName = null;
				
				// Coba dapatkan nama file dari IsiFolder jika menggunakan scene khusus
				if (currentRightClickedItem is IsiFolder isiFolder)
				{
					// Jika isiFolder memiliki properti untuk nama file
					if (isiFolder.HasMethod("GetFileName"))
					{
						fileName = (string)isiFolder.Call("GetFileName");
					}
					else
					{
						// Alternatif jika tidak ada GetFileName
						foreach (var child in isiFolder.GetChildren())
						{
							if (child is Label label)
							{
								fileName = label.Text;
								break;
							}
						}
					}
				}
				// Jika menggunakan HBoxContainer yang dibuat secara dinamis
				else if (currentRightClickedItem.HasMeta("file_name"))
				{
					fileName = (string)currentRightClickedItem.GetMeta("file_name");
				}
				
				// Jika berhasil mendapatkan nama file
				if (!string.IsNullOrEmpty(fileName))
				{
					GD.Print($"Deleting file: {fileName}");
					
					// Cek apakah file yang dihapus adalah file palsu
					bool isFakeFile = Global.IsFileFake(fileName);
					
					// Hapus dari list lokal
					FileData fileToDelete = files.Find(f => f.Name == fileName);
					if (fileToDelete != null)
					{
						files.Remove(fileToDelete);
					}
					
					// Hapus dari list di Global
					// Coba hapus dari semua kemungkinan list kategori
					RemoveFileFromGlobalList(fileName, Global.CurrentSystemFiles);
					RemoveFileFromGlobalList(fileName, Global.CurrentDownloadFiles);
					RemoveFileFromGlobalList(fileName, Global.CurrentVideoFiles);
					RemoveFileFromGlobalList(fileName, Global.CurrentPictureFiles);
					RemoveFileFromGlobalList(fileName, Global.CurrentMusicFiles);
					RemoveFileFromGlobalList(fileName, Global.CurrentFiles);
					
					// Jika yang dihapus adalah file palsu, kurangi CPU load
					if (isFakeFile)
					{
						// Asumsikan nilai CpuLoadPerFakeFile adalah 2 (sesuai dengan kode WallPaper)
						const int CpuLoadPerFakeFile = 2;
						
						// Kurangi CPU load
						Global.CpuProcess -= CpuLoadPerFakeFile;
						
						// Pastikan CPU load tidak negatif
						if (Global.CpuProcess < 0)
						{
							Global.CpuProcess = 0;
						}
						
						GD.Print($"[CPU Load] Fake file deleted: {fileName}");
						GD.Print($"[CPU Load] CPU reduced by {CpuLoadPerFakeFile}. New CPU load: {Global.CpuProcess}");
					}
					
					RefreshFiles();
					
					// Tampilkan notifikasi
					GD.Print($"File {fileName} has been deleted.");
				}
				else
				{
					GD.Print("Failed to get filename for deletion");
				}
				
				// Reset variabel
				currentRightClickedItem = null;
				currentRightClickedFileData = null;
			}
			else
			{
				GD.Print("No item was right-clicked");
			}
		}
	}
	
	// Helper method untuk menghapus file dari list di Global
	private void RemoveFileFromGlobalList(string fileName, List<FileData> globalList)
	{
		if (globalList == null) return;
		
		FileData fileToDelete = globalList.Find(f => f.Name == fileName);
		if (fileToDelete != null)
		{
			globalList.Remove(fileToDelete);
			GD.Print($"Removed {fileName} from a Global list");
		}
	}
	
	// Metode untuk menghapus file berdasarkan nama
	public void DeleteFile(string fileName)
	{
		FileData fileToDelete = files.Find(f => f.Name == fileName);
		if (fileToDelete != null)
		{
			files.Remove(fileToDelete);
			RefreshFiles();
		}
	}
}

// Kelas untuk data file 
public class FileData
{
	public string Name;
	public string Description;
	public string IconPath;
	
	public FileData(string name, string desc, string iconPath)
	{
		Name = name;
		Description = desc;
		IconPath = iconPath;
	}
}
