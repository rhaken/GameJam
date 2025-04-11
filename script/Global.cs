using Godot;
using System.Collections.Generic;
using System;	
public partial class Global : Node
{
	public static Queue<PackedScene> SceneQueue = new Queue<PackedScene>();
	
	public static double StartTime { get; private set; } = 0;
	public static bool IsStarted { get; private set; } = false;
	public static double ElapsedTime => IsStarted ? Time.GetTicksMsec() / 1000.0 - StartTime : 0;
	public static double CpuProcess = 0;
	
	private double _lastPrintTime = 0;
	public static Random Rnd = new Random();
	
	// Method to start the time counting
	public static void StartTimer()
	{
		if (!IsStarted)
		{
			StartTime = Time.GetTicksMsec() / 1000.0;
			IsStarted = true;
			GD.Print("Timer started at: " + StartTime);
		}
	}
	
	// Method to stop the time counting
	public static void StopTimer()
	{
		IsStarted = false;
		GD.Print("Timer stopped. Total elapsed time: " + (Time.GetTicksMsec() / 1000.0 - StartTime));
	}
	
	// Method to reset the timer
	public static void ResetTimer()
	{
		StartTime = Time.GetTicksMsec() / 1000.0;
		if (IsStarted)
		{
			GD.Print("Timer reset at: " + StartTime);
		}
	}
	
	// Current active files tracking for each category
	public static List<FileData> CurrentFiles { get; private set; } = new List<FileData>();
	public static string CurrentCategory { get; private set; } = "";
	public static int CurrentFakeCount { get; private set; } = 0;
	public static int TotalFakeFiles { get; private set; } = 0;
	
	// Individual tracking for specific file categories
	public static List<FileData> CurrentSystemFiles { get; private set; } = new List<FileData>();
	public static List<FileData> CurrentDownloadFiles { get; private set; } = new List<FileData>();
	public static List<FileData> CurrentVideoFiles { get; private set; } = new List<FileData>();
	public static List<FileData> CurrentPictureFiles { get; private set; } = new List<FileData>();
	public static List<FileData> CurrentMusicFiles { get; private set; } = new List<FileData>();
	
	// Global icon path that can be used as default
	public const string DefaultIconPath = "res://assets/icon/download.png";
	
	// System files - genuine
	public static List<FileData> SystemFiles => new List<FileData>
	{
		new FileData("kernel32.dll", "System library", DefaultIconPath),
		new FileData("winload.exe", "Windows boot loader", DefaultIconPath),
		new FileData("svchost.exe", "Service host process", DefaultIconPath),
		new FileData("explorer.exe", "Windows explorer", DefaultIconPath),
		new FileData("taskmgr.exe", "Task manager", DefaultIconPath),
		new FileData("conhost.exe", "Console host", DefaultIconPath),
		new FileData("logonui.exe", "Logon user interface", DefaultIconPath),
		new FileData("spoolsv.exe", "Spooler service", DefaultIconPath),
		new FileData("services.exe", "Services manager", DefaultIconPath),
		new FileData("lsass.exe", "Security service", DefaultIconPath),
		new FileData("rundll32.exe", "Run DLL utility", DefaultIconPath),
		new FileData("regedit.exe", "Registry editor", DefaultIconPath),
		new FileData("msconfig.exe", "System configuration", DefaultIconPath),
		new FileData("dxdiag.exe", "DirectX diagnostic", DefaultIconPath),
		new FileData("smss.exe", "Session manager", DefaultIconPath),
		new FileData("system.ini", "System configuration", DefaultIconPath),
		new FileData("setup.ini", "Setup configuration", DefaultIconPath),
		new FileData("drivers.sys", "System drivers", DefaultIconPath),
		new FileData("bootlog.txt", "Boot log file", DefaultIconPath),
		new FileData("config.sys", "System configuration", DefaultIconPath),
		new FileData("ntdll.dll", "NT layer DLL", DefaultIconPath),
		new FileData("storport.sys", "Storage port driver", DefaultIconPath),
		new FileData("hal.dll", "Hardware abstraction layer", DefaultIconPath),
		new FileData("readme.txt", "Read me file", DefaultIconPath),
		new FileData("debug.log", "Debug log file", DefaultIconPath),
		new FileData("crashdump.txt", "Crash dump information", DefaultIconPath),
		new FileData("bcdedit.ini", "Boot configuration", DefaultIconPath),
		new FileData("update.dll", "Update library", DefaultIconPath),
		new FileData("security.sys", "Security system file", DefaultIconPath),
		new FileData("info.txt", "Information file", DefaultIconPath),
		new FileData("taskhost.exe", "Task host process", DefaultIconPath),
		new FileData("note.txt", "Notes file", DefaultIconPath),
		new FileData("kernel.dll", "Kernel library", DefaultIconPath)
	};
	
	// System files - fake versions
	public static List<FileData> FakeSystemFiles => new List<FileData>
	{
		new FileData("kernell32.dll", "Fake system library", DefaultIconPath),
		new FileData("winIoad.exe", "Fake Windows boot loader", DefaultIconPath), // Using capital I
		new FileData("svchosst.exe", "Fake service host process", DefaultIconPath),
		new FileData("exploorer.exe", "Fake Windows explorer", DefaultIconPath),
		new FileData("taskmgrr.exe", "Fake task manager", DefaultIconPath),
		new FileData("conhostt.exe", "Fake console host", DefaultIconPath),
		new FileData("logonuii.exe", "Fake logon user interface", DefaultIconPath),
		new FileData("spooIsv.exe", "Fake spooler service", DefaultIconPath), // Using capital I
		new FileData("services.ex3", "Fake services manager", DefaultIconPath),
		new FileData("lsaas.exe", "Fake security service", DefaultIconPath),
		new FileData("rundI32.exe", "Fake run DLL utility", DefaultIconPath), // Using capital I
		new FileData("regedit_.exe", "Fake registry editor", DefaultIconPath),
		new FileData("msconfigg.exe", "Fake system configuration", DefaultIconPath),
		new FileData("dxdiagx.exe", "Fake DirectX diagnostic", DefaultIconPath),
		new FileData("smss.syy", "Fake session manager", DefaultIconPath),
		new FileData("sytem.ini", "Fake system configuration", DefaultIconPath),
		new FileData("setup.ini_", "Fake setup configuration", DefaultIconPath),
		new FileData("driverss.sys", "Fake system drivers", DefaultIconPath),
		new FileData("bootIog.txt", "Fake boot log file", DefaultIconPath), // Using capital I
		new FileData("config_.sys", "Fake system configuration", DefaultIconPath),
		new FileData("ntdI.dll", "Fake NT layer DLL", DefaultIconPath), // Using capital I
		new FileData("storport.sys_", "Fake storage port driver", DefaultIconPath),
		new FileData("hal.ddl", "Fake hardware abstraction layer", DefaultIconPath),
		new FileData("readme_.txt", "Fake read me file", DefaultIconPath),
		new FileData("debuglog.log", "Fake debug log file", DefaultIconPath),
		new FileData("crashrpt.txt", "Fake crash dump information", DefaultIconPath),
		new FileData("bcdedit_.ini", "Fake boot configuration", DefaultIconPath),
		new FileData("update.d11", "Fake update library", DefaultIconPath), // Using 1s instead of ls
		new FileData("security.sy_", "Fake security system file", DefaultIconPath),
		new FileData("info.txt.exe", "Fake information file", DefaultIconPath),
		new FileData("taskhost.exe.exe", "Fake task host process", DefaultIconPath),
		new FileData("note.txt.bat", "Fake notes file", DefaultIconPath),
		new FileData("kernel.dll_.exe", "Fake kernel library", DefaultIconPath)
	};
	
	// Download files - genuine
	public static List<FileData> DownloadFiles => new List<FileData>
	{
		new FileData("File1.zip", "Downloaded file 1", DefaultIconPath),
		new FileData("File2.zip", "Downloaded file 2", DefaultIconPath),
		new FileData("Software.exe", "Downloaded software", DefaultIconPath),
		new FileData("Document.pdf", "Downloaded document", DefaultIconPath),
		new FileData("Archive.rar", "Downloaded archive", DefaultIconPath)
	};
	
	// Download files - fake versions
	public static List<FileData> FakeDownloadFiles => new List<FileData>
	{
		new FileData("FileI.zip", "Fake downloaded file", DefaultIconPath), // Using capital I
		new FileData("File2.zip.exe", "Malicious file disguised as ZIP", DefaultIconPath),
		new FileData("Software_.exe", "Fake software", DefaultIconPath),
		new FileData("Document.pdf.js", "Fake PDF document", DefaultIconPath),
		new FileData("Archive.rarr", "Fake archive", DefaultIconPath)
	};
	
	// Video files - genuine
	public static List<FileData> VideoFiles => new List<FileData>
	{
		new FileData("Tutorial Video.mp4", "Video tutorial", DefaultIconPath),
		new FileData("Demo Video.mp4", "Video demo", DefaultIconPath),
		new FileData("Basic Tutorial.mp4", "Basic tutorial", DefaultIconPath),
		new FileData("Advanced Tutorial.mp4", "Advanced tutorial", DefaultIconPath),
		new FileData("Product Demo.mp4", "Product demo", DefaultIconPath),
		new FileData("Feature Demo.mp4", "Feature demo", DefaultIconPath)
	};
	
	// Video files - fake versions
	public static List<FileData> FakeVideoFiles => new List<FileData>
	{
		new FileData("Tutorial Video.mp4.exe", "Fake video file", DefaultIconPath),
		new FileData("Demo Vidieo.mp4", "Fake video demo", DefaultIconPath),
		new FileData("Basic_Tutorial.exe", "Malicious tutorial file", DefaultIconPath),
		new FileData("Advanced TutoriaI.mp4", "Fake advanced tutorial", DefaultIconPath), // Using capital I
		new FileData("Product Demo.mp4.bat", "Fake product demo", DefaultIconPath),
		new FileData("Feature-Demo.scr", "Malicious screensaver", DefaultIconPath)
	};
	
	// Picture/Image files - genuine
	public static List<FileData> PictureFiles => new List<FileData>
	{
		new FileData("Portrait.jpg", "Portrait image", DefaultIconPath),
		new FileData("Landscape.jpg", "Landscape image", DefaultIconPath),
		new FileData("Beach.jpg", "Beach photo", DefaultIconPath),
		new FileData("Mountain.jpg", "Mountain photo", DefaultIconPath),
		new FileData("Office.jpg", "Office photo", DefaultIconPath),
		new FileData("Team.jpg", "Team photo", DefaultIconPath)
	};
	
	// Picture/Image files - fake versions
	public static List<FileData> FakePictureFiles => new List<FileData>
	{
		new FileData("Portrait.jpg.exe", "Fake portrait image", DefaultIconPath),
		new FileData("Landscapee.jpg", "Fake landscape image", DefaultIconPath),
		new FileData("Beach.jpg.vbs", "Malicious beach photo", DefaultIconPath),
		new FileData("Mountain.jpeg.bat", "Fake mountain photo", DefaultIconPath),
		new FileData("Office_.jpg", "Fake office photo", DefaultIconPath),
		new FileData("Team.jpg.scr", "Malicious team photo", DefaultIconPath)
	};
	
	// Music files - genuine
	public static List<FileData> MusicFiles => new List<FileData>
	{
		new FileData("Song1.mp3", "Music track 1", DefaultIconPath),
		new FileData("Song2.mp3", "Music track 2", DefaultIconPath),
		new FileData("Album.mp3", "Album track", DefaultIconPath)
	};
	
	// Music files - fake versions
	public static List<FileData> FakeMusicFiles => new List<FileData>
	{
		new FileData("SongI.mp3", "Fake music track", DefaultIconPath), // Using capital I
		new FileData("Song2.mp3.exe", "Malicious music file", DefaultIconPath),
		new FileData("AIbum.mp3", "Fake album track", DefaultIconPath) // Using capital I
	};
	
	// Default files untuk fallback
	public static List<FileData> DefaultFiles => SystemFiles;
	
	// Function to get all genuine files plus a specified number of fake files from a category
	// fakeCount: number of fake files to include
	// Updates the current files for the specified category and returns the mixed files
	public static List<FileData> GetMixedFilesByCategory(string category, int fakeCount = 3)
	{
		List<FileData> genuineFiles;
		List<FileData> fakeFiles;
		
		switch (category.ToLower())
		{
			case "system":
				genuineFiles = SystemFiles;
				fakeFiles = FakeSystemFiles;
				break;
			case "download":
				genuineFiles = DownloadFiles;
				fakeFiles = FakeDownloadFiles;
				break;
			case "video":
				genuineFiles = VideoFiles;
				fakeFiles = FakeVideoFiles;
				break;
			case "picture":
				genuineFiles = PictureFiles;
				fakeFiles = FakePictureFiles;
				break;
			case "music":
				genuineFiles = MusicFiles;
				fakeFiles = FakeMusicFiles;
				break;
			default:
				genuineFiles = SystemFiles;
				fakeFiles = FakeSystemFiles;
				break;
		}
		
		List<FileData> mixedFiles = new List<FileData>();
		
		// Add all genuine files
		mixedFiles.AddRange(genuineFiles);
		
		// Add random fake files (limit by available files)
		fakeCount = Math.Min(fakeCount, fakeFiles.Count);
		List<int> fakeIndices = GetRandomIndices(fakeFiles.Count, fakeCount);
		foreach (int index in fakeIndices)
		{
			mixedFiles.Add(fakeFiles[index]);
		}
		
		// Shuffle the mixed files
		ShuffleList(mixedFiles);
		
		// Update the current global variables
		CurrentFiles = mixedFiles;
		CurrentCategory = category.ToLower();
		CurrentFakeCount = fakeCount;
		
		// Update category-specific current files
		switch (category.ToLower())
		{
			case "system":
				CurrentSystemFiles = mixedFiles;
				break;
			case "download":
				CurrentDownloadFiles = mixedFiles;
				break;
			case "video":
				CurrentVideoFiles = mixedFiles;
				break;
			case "picture":
				CurrentPictureFiles = mixedFiles;
				break;
			case "music":
				CurrentMusicFiles = mixedFiles;
				break;
		}
		
		return mixedFiles;
	}
	
	// Get the currently active files from the most recently loaded category
	public static List<FileData> GetCurrentFiles()
	{
		return CurrentFiles;
	}
	
	// Get the currently active files for a specific category
	public static List<FileData> GetCurrentFilesByCategory(string category)
	{
		switch (category.ToLower())
		{
			case "system":
				return CurrentSystemFiles;
			case "download":
				return CurrentDownloadFiles;
			case "video":
				return CurrentVideoFiles;
			case "picture":
				return CurrentPictureFiles;
			case "music":
				return CurrentMusicFiles;
			default:
				return CurrentFiles;
		}
	}
	
	// Helper method to get random indices
	private static List<int> GetRandomIndices(int maxValue, int count)
	{
		List<int> indices = new List<int>();
		for (int i = 0; i < maxValue; i++)
		{
			indices.Add(i);
		}
		
		ShuffleList(indices);
		
		return indices.GetRange(0, Math.Min(count, indices.Count));
	}
	
	// Helper method to shuffle a list
	private static void ShuffleList<T>(List<T> list)
	{
		for (int i = list.Count - 1; i > 0; i--)
		{
			int j = Rnd.Next(i + 1);
			T temp = list[i];
			list[i] = list[j];
			list[j] = temp;
		}
	}
	
	public override void _Ready()
	{
		GD.Print("Global script loaded. Start time: " + StartTime);
		GD.Print("Global script loaded.");
		
		// Tidak melakukan inisialisasi otomatis
		// Pengguna akan memanggil GetMixedFilesByCategory sendiri saat dibutuhkan
	}
	
	// Check if a file is fake (used for game logic)
	public static bool IsFileFake(string fileName)
	{
		// Check in all fake file categories
		foreach (var file in FakeSystemFiles)
		{
			if (file.Name == fileName) return true;  // Changed from FileName to Name
		}
		
		foreach (var file in FakeDownloadFiles)
		{
			if (file.Name == fileName) return true;  // Changed from FileName to Name
		}
		
		foreach (var file in FakeVideoFiles)
		{
			if (file.Name == fileName) return true;  // Changed from FileName to Name
		}
		
		foreach (var file in FakePictureFiles)
		{
			if (file.Name == fileName) return true;  // Changed from FileName to Name
		}
		
		foreach (var file in FakeMusicFiles)
		{
			if (file.Name == fileName) return true;  // Changed from FileName to Name
		}
		
		return false;
	}
	
	public static int CountTotalFakeFiles()
	{
		int count = 0;
		
		// Hitung fake files di masing-masing kategori
		foreach (var file in CurrentSystemFiles)
		{
			if (IsFileFake(file.Name))
				count++;
		}
		
		foreach (var file in CurrentDownloadFiles)
		{
			if (IsFileFake(file.Name))
				count++;
		}
		
		foreach (var file in CurrentVideoFiles)
		{
			if (IsFileFake(file.Name))
				count++;
		}
		
		foreach (var file in CurrentPictureFiles)
		{
			if (IsFileFake(file.Name))
				count++;
		}
		
		foreach (var file in CurrentMusicFiles)
		{
			if (IsFileFake(file.Name))
				count++;
		}
		
		// Simpan hasil di variabel global
		TotalFakeFiles = count;
		
		return count;
	}
}
