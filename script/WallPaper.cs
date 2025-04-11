using Godot;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;

public partial class WallPaper : TextureRect
{
	private bool _triggered = false;
	private double _lastSpawnTime = 0;
	private const double SpawnInterval = 3.0;
	
	// Interval untuk update file (dalam detik)
	private const double FileUpdateInterval = 60.0; // 1 menit
	private double _lastFileUpdateTime = 0;
	private int _currentFakeFiles = 0;
	private const int InitialDelayMinutes = 1; // 5 menit pertama file asli semua
	private const int FakeFilesIncreaseRate = 2; // Penambahan 2 file palsu setiap interval
	private double _lastCpuCheckTime = 0;
	private const double CpuCheckInterval = 20.0; // 20 detik
	private int _previousFakeFilesCount = 0;
	private const int CpuLoadPerFakeFile = 10;
	
	// Win screen parameters
	private double _lastWinScreenCheckTime = 0;
	private const double WinScreenCheckInterval = 1.0; // Check every 1 second
	private const double WinTimeMinutes = 15.0; // Show win screen after 15 minutes
	private bool _winScreenShown = false;
	
	// Blue Screen check parameters
	private double _lastBlueScreenCheckTime = 0;
	private const double BlueScreenCheckInterval = 5.0; // Check every 5 seconds
	private const int BlueScreenThreshold = 50; // Trigger at 50% CPU
	private const int BlueScreenChance = 40; // 40% chance of triggering
	private const int HighCpuThreshold = 80; // High CPU threshold
	private const int HighCpuBlueScreenChance = 50; // 50% chance when CPU is high
	private const int CriticalCpuThreshold = 100; // Critical CPU threshold for forced crash
	
	// Kategori file yang akan diupdate (sesuai dengan kategori di Global.cs)
	private readonly string[] _fileCategories = { "system", "download", "video", "picture", "music" };
	private const string DefaultCategory = "system"; // Kategori default untuk tampilan
	private string _currentCategory = DefaultCategory;
	
	public override void _Ready()
	{
		string wallpaperPath = GetWallpaperPath();
		if (wallpaperPath != null && Godot.FileAccess.FileExists(wallpaperPath)) // Gunakan Godot.FileAccess
		{
			var image = new Image();
			var err = image.Load(wallpaperPath);
			image.Resize(512, 512);
			if (err == Error.Ok)
			{
				var tex = ImageTexture.CreateFromImage(image);
				Texture = tex;
			}
		}
		
		// Inisialisasi file dengan semua file asli (0 palsu)
		UpdateAllCategoriesFiles();
		
		// Print debug info awal
		GD.Print("[FileSystem] Initial file setup completed");
		PrintCurrentFilesDebug();
		
		// Load and add the StartScreen scene using call_deferred
		var startScreenScene = GD.Load<PackedScene>("res://scene/StartScreen.tscn");
		if (startScreenScene != null)
		{
			var startScreenInstance = startScreenScene.Instantiate();
			// Use CallDeferred to add the child after the current operations are complete
			GetTree().Root.CallDeferred("add_child", startScreenInstance);
			GD.Print("[StartScreen] Scheduled to be added to the scene tree (using CallDeferred)");
		}
		else
		{
			GD.Print("[ERROR] Failed to load StartScreen.tscn");
		}
	}
	
	public override void _Process(double delta)
	{
		// Always get the elapsed time, even if game hasn't started
		double totalTime = Global.ElapsedTime;
		
		// Only process game mechanics if the game has been started
		if (!Global.IsStarted)
		{
			// Game not started yet, still initialize files but only genuine ones
			if (_currentFakeFiles != 0)
			{
				_currentFakeFiles = 0;
				UpdateAllCategoriesFiles();
				GD.Print("[FileSystem] Game not started: Ensuring all files are genuine (0 fake files)");
			}
			return; // Don't run virus spawning or other threatening mechanics
		}
		
		// Check for critical CPU levels that should trigger a game crash
		if (Global.CpuProcess >= CriticalCpuThreshold)
		{
			GD.Print($"[CRITICAL] CPU has reached {Global.CpuProcess}%, triggering fatal blue screen crash");
			SpawnFatalBlueScreen();
			return; // Stop processing after triggering the fatal crash
		}
		
		// Check if it's time to show the win screen (15 minutes have passed)
		CheckForWinScreen(totalTime);
		
		// Update file sesuai dengan waktu yang berlalu
		UpdateFilesBasedOnTime(totalTime);
		
		// Hitung chance berdasarkan total time untuk popup virus
		int minutesPassed = (int)(totalTime / 180); // 180 detik = 3 menit
		int spawnChance = Mathf.Min(20 + (minutesPassed * 15), 100); // Naik 15% per 3 menit, max 100%
		
		// Cek apakah waktunya spawn popup virus
		if (totalTime - _lastSpawnTime >= SpawnInterval)
		{
			_lastSpawnTime = totalTime;
			// Random check terhadap chance
			Random rnd = Global.Rnd;
			float roll = (float)rnd.NextDouble() * 100;
			if (roll <= spawnChance)
			{
				SpawnPopupVirus();
			}
		}
		
		UpdateCpuLoadBasedOnFakeFiles(totalTime);
		
		// Check for Blue Screen condition
		CheckForBlueScreenTrigger(totalTime);
	}
	
	// Method to check if Win Screen should be shown
	private void CheckForWinScreen(double totalTime)
	{
		// Don't check if win screen has already been shown
		if (_winScreenShown) return;
		
		// Check every WinScreenCheckInterval seconds
		if (totalTime - _lastWinScreenCheckTime >= WinScreenCheckInterval)
		{
			_lastWinScreenCheckTime = totalTime;
			
			// Check if 15 minutes have passed
			if (totalTime >= WinTimeMinutes * 60)
			{
				GD.Print($"[WinScreen] 15 minutes reached! Showing win screen at time: {totalTime}");
				ShowWinScreen();
			}
		}
	}
	
	// Method to show the Win Screen
	private void ShowWinScreen()
	{
		var winScreenScene = GD.Load<PackedScene>("res://scene/WinScreen.tscn");
		if (winScreenScene != null)
		{
			var winScreenInstance = winScreenScene.Instantiate();
			GetTree().Root.AddChild(winScreenInstance);
			
			// Set win screen to foreground
			if (winScreenInstance is Control winScreenControl)
			{
				winScreenControl.ZIndex = 1000;
			}
			
			GD.Print("[WinScreen] Win screen shown!");
			
			// Set _winScreenShown to true so we don't show it again
			_winScreenShown = true;
			
			// Set Global.IsStarted to false
			Global.StopTimer();
		}
		else
		{
			GD.Print("[ERROR] Failed to load WinScreen.tscn");
		}
	}
	
	// Method to spawn a fatal blue screen and quit the game
	private void SpawnFatalBlueScreen()
	{
		var blueScreenScene = GD.Load<PackedScene>("res://scene/BlueScreen.tscn");
		var blueScreenInstance = blueScreenScene.Instantiate();
		
		// Hide all other windows/controls
		foreach (var child in GetTree().Root.GetChildren())
		{
			// Skip self
			if (child == this)
				continue;
				
			// Hide all other nodes
			if (child is Control childControl)
			{
				childControl.Visible = false;
			}
			else if (child is Window childWindow)
			{
				childWindow.Visible = false;
			}
		}
		
		// Add the blue screen to the root
		GetTree().Root.AddChild(blueScreenInstance);
		
		// Set extreme z-index to ensure it's on top
		if (blueScreenInstance is CanvasItem canvasItem)
		{
			canvasItem.ZIndex = 1000;
			canvasItem.ShowBehindParent = false;
		}
		
		// Make it full screen
		if (blueScreenInstance is Control blueScreenControl)
		{
			blueScreenControl.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			blueScreenControl.SetDeferred("size", GetViewport().GetVisibleRect().Size);
			blueScreenControl.MouseFilter = Control.MouseFilterEnum.Stop;
		}
		
		// Make it always on top
		if (blueScreenInstance is Window blueScreenWindow)
		{
			blueScreenWindow.AlwaysOnTop = true;
		}
		
		GD.Print("[FATAL] Blue Screen of Death triggered - Game will exit in 3 seconds");
		
		// Create a timer to quit the game after showing blue screen for 3 seconds
		var timer = GetTree().CreateTimer(3.0);
		timer.Timeout += () => 
		{
			GD.Print("[FATAL] Exiting game due to critical CPU overload");
			GetTree().Quit();
		};
	}
	
	// Method to check if Blue Screen should be triggered
	private void CheckForBlueScreenTrigger(double totalTime)
	{
		// Check every BlueScreenCheckInterval seconds
		if (totalTime - _lastBlueScreenCheckTime >= BlueScreenCheckInterval)
		{
			_lastBlueScreenCheckTime = totalTime;
			
			// Check if CPU is above basic threshold
			if (Global.CpuProcess > BlueScreenThreshold)
			{
				// Calculate chance based on CPU level
				int currentChance = BlueScreenChance; // Default 40%
				
				// If CPU is above high threshold, increase chance to 50%
				if (Global.CpuProcess > HighCpuThreshold)
				{
					currentChance = HighCpuBlueScreenChance; // 50%
					GD.Print($"[BlueScreen] High CPU detected ({Global.CpuProcess} > {HighCpuThreshold}), increasing chance to {HighCpuBlueScreenChance}%");
				}
				
				// Calculate roll
				Random rnd = Global.Rnd;
				float roll = (float)rnd.NextDouble() * 100;
				
				if (roll <= currentChance)
				{
					GD.Print($"[BlueScreen] Triggering! CPU at {Global.CpuProcess}, rolled {roll} (threshold: {currentChance})");
					SpawnBlueScreen();
				}
				else
				{
					GD.Print($"[BlueScreen] Not triggered. CPU at {Global.CpuProcess}, rolled {roll} (threshold: {currentChance})");
				}
			}
		}
	}
	
	// Method to spawn the Blue Screen scene for 2 seconds
	private void SpawnBlueScreen()
	{
		var blueScreenScene = GD.Load<PackedScene>("res://scene/BlueScreen.tscn");
		var blueScreenInstance = blueScreenScene.Instantiate();
		
		// Store all currently visible windows/popups to hide them
		var visibleNodes = new List<(Node node, bool wasVisible)>();
		
		// First, make all existing windows invisible
		foreach (var child in GetTree().Root.GetChildren())
		{
			// Skip self
			if (child == this)
				continue;
				
			// If it's a visible control, hide it
			if (child is Control childControl)
			{
				visibleNodes.Add((child, childControl.Visible));
				childControl.Visible = false;
				GD.Print($"[BlueScreen] Hiding control: {child.Name}");
			}
			// If it's a visible window, hide it
			else if (child is Window childWindow)
			{
				visibleNodes.Add((child, childWindow.Visible));
				childWindow.Visible = false;
				GD.Print($"[BlueScreen] Hiding window: {child.Name}");
			}
		}
		
		// Add the blue screen to the root
		GetTree().Root.AddChild(blueScreenInstance);
		
		// Set extreme z-index to ensure it's on top
		if (blueScreenInstance is CanvasItem canvasItem)
		{
			canvasItem.ZIndex = 1000;
			canvasItem.ShowBehindParent = false;
		}
		
		// Make it full screen
		if (blueScreenInstance is Control blueScreenControl)
		{
			blueScreenControl.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			// Use SetDeferred instead of directly setting the size
			blueScreenControl.SetDeferred("size", GetViewport().GetVisibleRect().Size);
			blueScreenControl.MouseFilter = Control.MouseFilterEnum.Stop;
		}
		
		// Make it always on top
		if (blueScreenInstance is Window blueScreenWindow)
		{
			blueScreenWindow.AlwaysOnTop = true;
		}
		
		GD.Print("[BlueScreen] Blue Screen spawned for 2 seconds!");
		
		// Create a timer to remove the blue screen after 2 seconds
		var timer = GetTree().CreateTimer(2.0);
		timer.Timeout += () => 
		{
			if (blueScreenInstance != null && blueScreenInstance.IsInsideTree())
			{
				// Remove the blue screen
				blueScreenInstance.QueueFree();
				
				// Restore visibility of previously visible nodes
				foreach (var (node, wasVisible) in visibleNodes)
				{
					if (node != null && node.IsInsideTree())
					{
						if (node is Control storedControl)
						{
							storedControl.Visible = wasVisible;
						}
						else if (node is Window storedWindow)
						{
							storedWindow.Visible = wasVisible;
						}
						
						GD.Print($"[BlueScreen] Restoring visibility for: {node.Name}");
					}
				}
				
				GD.Print("[BlueScreen] Blue Screen removed after 2 seconds");
			}
		};
	}
	
	private void UpdateFilesBasedOnTime(double totalTime)
	{
		// Cek apakah sudah waktunya update file
		if (totalTime - _lastFileUpdateTime >= FileUpdateInterval)
		{
			_lastFileUpdateTime = totalTime;
			
			int minutesPassed = (int)(totalTime / 60.0); // Konversi total waktu ke menit
			
			// 5 menit pertama file asli semua
			if (minutesPassed < InitialDelayMinutes)
			{
				// Pastikan semua file asli (0 palsu) selama 5 menit pertama
				if (_currentFakeFiles != 0)
				{
					_currentFakeFiles = 0;
					UpdateAllCategoriesFiles();
					GD.Print($"[FileSystem] Initial phase: All genuine files (0 fake files)");
				}
			}
			else
			{
				// Setelah 5 menit, mulai tambahkan file palsu
				int elapsedMinutesAfterDelay = minutesPassed - InitialDelayMinutes;
				int targetFakeFiles = elapsedMinutesAfterDelay * FakeFilesIncreaseRate;
				
				// Update hanya jika jumlah file palsu berubah
				if (targetFakeFiles != _currentFakeFiles)
				{
					_currentFakeFiles = targetFakeFiles;
					UpdateAllCategoriesFiles();
					GD.Print($"[FileSystem] Minute {minutesPassed}: Updated to {_currentFakeFiles} fake files for all categories");
					
					// Debug: Print file listing untuk kategori yang sedang aktif
					PrintCurrentFilesDebug();
				}
			}
		}
	}
	
	// Method untuk mengupdate semua kategori file
	private void UpdateAllCategoriesFiles()
	{
		// Jika total fake files = 0, update semua kategori dengan 0 fake file
		if (_currentFakeFiles <= 0)
		{
			foreach (string category in _fileCategories)
			{
				Global.GetMixedFilesByCategory(category, 0);
			}
			return;
		}
		
		// Jika ada fake files, distribusikan secara acak
		int remainingFakeFiles = _currentFakeFiles;
		int categoriesLeft = _fileCategories.Length;
		
		// Distribusikan fake files secara acak di antara semua kategori
		foreach (string category in _fileCategories)
		{
			// Untuk kategori terakhir, gunakan sisa fake files
			if (categoriesLeft == 1)
			{
				Global.GetMixedFilesByCategory(category, remainingFakeFiles);
				GD.Print($"[FileSystem] Category '{category}': {remainingFakeFiles} fake files (last category)");
			}
			else
			{
				// Untuk kategori lain, pilih jumlah acak dari sisa file palsu
				// dengan memastikan masih ada file untuk kategori lain
				int maxFilesForThisCategory = remainingFakeFiles - (categoriesLeft - 1);
				maxFilesForThisCategory = Math.Max(0, maxFilesForThisCategory); // Pastikan tidak negatif
				
				// Pilih jumlah acak antara 0 dan maksimum yang diperbolehkan
				int fakesForThisCategory = 0;
				if (maxFilesForThisCategory > 0)
				{
					fakesForThisCategory = Global.Rnd.Next(maxFilesForThisCategory + 1);
				}
				
				Global.GetMixedFilesByCategory(category, fakesForThisCategory);
				remainingFakeFiles -= fakesForThisCategory;
				GD.Print($"[FileSystem] Category '{category}': {fakesForThisCategory} fake files");
			}
			
			categoriesLeft--;
		}
		
		// Debug: Print jumlah total fake files setelah distribusi
		int totalDistributedFakes = 0;
		
		foreach (string category in _fileCategories)
		{
			var files = Global.GetCurrentFilesByCategory(category);
			int fakeCount = 0;
			
			foreach (var file in files)
			{
				if (Global.IsFileFake(file.Name))
				{
					fakeCount++;
				}
			}
			
			totalDistributedFakes += fakeCount;
		}
		
		GD.Print($"[FileSystem] Total fake files distributed: {totalDistributedFakes} (target: {_currentFakeFiles})");
		
		// Debug: Print file listing untuk kategori yang sedang aktif
		PrintCurrentFilesDebug();
	}
	
	// Method untuk mengupdate file pada kategori aktif saja
	private void UpdateCurrentFiles()
	{
		// Metode ini tidak boleh mengubah jumlah total file palsu
		// Kita hanya perlu mengambil file sesuai dengan kategori saat ini
		var currentCategoryFiles = Global.GetCurrentFilesByCategory(_currentCategory);
		
		// Hitung jumlah file palsu yang ada di kategori saat ini
		int currentCategoryFakeCount = 0;
		foreach (var file in currentCategoryFiles)
		{
			if (Global.IsFileFake(file.Name))
			{
				currentCategoryFakeCount++;
			}
		}
		
		// Tampilkan informasi tentang kategori yang dipilih
		int genuineCount = currentCategoryFiles.Count - currentCategoryFakeCount;
		GD.Print($"[FileSystem] Active category '{_currentCategory}': {genuineCount} genuine, {currentCategoryFakeCount} fake, total {currentCategoryFiles.Count}");
	}
	
	// Mengubah kategori file saat ini (dapat dipanggil dari luar jika perlu)
	public void SetFileCategory(string category)
	{
		// Penanganan khusus untuk "this.pc" - map ke "system"
		if (category == "this.pc")
		{
			GD.Print($"[FileSystem] 'this.pc' dideteksi, akan dikonversi ke 'system'");
			category = "system";
		}
		
		if (_currentCategory != category)
		{
			_currentCategory = category;
			// Tidak perlu mengupdate file, hanya ubah kategori yang ditampilkan
			GD.Print($"[FileSystem] Category changed to: {_currentCategory}");
			
			// Cetak debug info untuk kategori baru
			PrintCurrentFilesDebug();
		}
	}
	
	private void SpawnPopupVirus()
	{
		var popupScene = GD.Load<PackedScene>("res://scene/PopUpVirus.tscn");
		var popupInstance = popupScene.Instantiate();
		if (popupInstance is Window popupWindow)
		{
			Vector2 screenSize = GetViewport().GetVisibleRect().Size;
			Vector2 windowSize = popupWindow.Size;
			float randomX = (float)Global.Rnd.NextDouble() * (screenSize.X - windowSize.X);
			float randomY = (float)Global.Rnd.NextDouble() * (screenSize.Y - windowSize.Y);
			popupWindow.Position = new Vector2I((int)randomX, (int)randomY);
			GetTree().Root.AddChild(popupWindow);
		}
	}
	
	private string GetWallpaperPath()
	{
		if (OS.GetName() == "Windows")
		{
			return GetWindowsWallpaperPath();
		}
		return null;
	}
	
	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, System.Text.StringBuilder pvParam, uint fWinIni);
	
	private string GetWindowsWallpaperPath()
	{
		const uint SPI_GETDESKWALLPAPER = 0x0073;
		const int MAX_PATH = 260;
		var sb = new System.Text.StringBuilder(MAX_PATH);
		SystemParametersInfo(SPI_GETDESKWALLPAPER, (uint)sb.Capacity, sb, 0);
		return sb.ToString();
	}
	
	// Fungsi untuk debug: mencetak semua file dalam CurrentFiles
	private void PrintCurrentFilesDebug()
	{
		// Jika kategori "this.pc", gunakan "system" untuk mengambil files
		string categoryToUse = _currentCategory == "this.pc" ? "system" : _currentCategory;
		
		var currentFiles = Global.GetCurrentFilesByCategory(categoryToUse);
		GD.Print($"[FileSystem DEBUG] ===== CURRENT FILES ({_currentCategory}) =====");
		GD.Print($"[FileSystem DEBUG] Total files: {currentFiles.Count}");
		
		int index = 0;
		int genuineCount = 0;
		int fakeCount = 0;
		
		foreach (var file in currentFiles)
		{
			bool isFake = Global.IsFileFake(file.Name);
			string fileType = isFake ? "FAKE" : "GENUINE";
			
			if (isFake) fakeCount++;
			else genuineCount++;
			
			GD.Print($"[FileSystem DEBUG] {index + 1}. {file.Name} - {file.Description} [{fileType}]");
			index++;
		}
		
		GD.Print($"[FileSystem DEBUG] Summary: {genuineCount} genuine files, {fakeCount} fake files");
		GD.Print($"[FileSystem DEBUG] ========================================");
	}
	
	private void UpdateCpuLoadBasedOnFakeFiles(double totalTime)
	{
		// Cek setiap 20 detik
		if (totalTime - _lastCpuCheckTime >= CpuCheckInterval)
		{
			_lastCpuCheckTime = totalTime;
			
			// Hitung total file fake saat ini dari semua kategori
			int currentFakeFilesCount = 0;
			
			foreach (string category in _fileCategories)
			{
				var files = Global.GetCurrentFilesByCategory(category);
				foreach (var file in files)
				{
					if (Global.IsFileFake(file.Name))
					{
						currentFakeFilesCount++;
					}
				}
			}
			GD.Print("[FakeFIleCount] " + currentFakeFilesCount);
			
			// Hanya tambahkan CPU load jika ada file fake baru
			if (currentFakeFilesCount > _previousFakeFilesCount)
			{
				int newFakeFiles = currentFakeFilesCount - _previousFakeFilesCount;
				double cpuAdjustment = newFakeFiles * CpuLoadPerFakeFile;
				
				// Update CPU load (hanya penambahan)
				Global.CpuProcess += cpuAdjustment;
				
				GD.Print($"[CPU Load] New fake files detected: {newFakeFiles}");
				GD.Print($"[CPU Load] Increasing CPU by {cpuAdjustment}. New CPU load: {Global.CpuProcess}");
				
				// Check if CPU has reached critical threshold
				if (Global.CpuProcess >= CriticalCpuThreshold)
				{
					GD.Print($"[CPU CRITICAL] CPU load has reached critical level: {Global.CpuProcess}%");
				}
			}
			else if (currentFakeFilesCount < _previousFakeFilesCount)
			{
				// Hanya log perubahan tanpa mengurangi CPU load
				int removedFakeFiles = _previousFakeFilesCount - currentFakeFilesCount;
				GD.Print($"[CPU Load] {removedFakeFiles} fake files removed, but CPU load maintained at {Global.CpuProcess}");
				GD.Print($"[CPU Load] CPU load will be reduced manually when files are deleted");
			}
			
			// Simpan jumlah file fake saat ini untuk perbandingan berikutnya
			_previousFakeFilesCount = currentFakeFilesCount;
		}
	}
	
	// Fungsi untuk debug yang bisa dipanggil secara manual
	public void DebugCurrentFiles()
	{
		PrintCurrentFilesDebug();
	}
}
