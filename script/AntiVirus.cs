using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public partial class AntiVirus : Window
{
	[Export] public NodePath OutputPath;
	private RichTextLabel TerminalOutput;
	private StringBuilder currentInput = new();
	private Queue<string> bootLines = new();
	private bool booting = true;
	[Export] private float typingDelay = 0.05f;
	private float typingTimer = 0;
	private string terminalText = "";
	private string username = "";
	
	// Caret blinking variables
	private float caretBlinkTime = 0.5f;
	private float caretTimer = 0;
	private bool caretVisible = true;
	private string caretSymbol = "█";
	
	// Scanning animation variables
	private bool isScanning = false;
	private float scanAnimationTimer = 0;
	private float scanAnimationDelay = 0.1f;
	private int scanAnimationFrame = 0;
	private string[] scanAnimationFrames = { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
	private string scanningLine = "";
	private float scanDuration = 10.0f; // Durasi scan 10 detik
	private List<string> detectedFakeFiles = new List<string>();
	private Dictionary<string, List<string>> fakeFilesByCategory = new Dictionary<string, List<string>>();
	private int totalFakeFiles = 0;
	private float scanTimer = 0;
	private List<string> scanningSteps = new()
	{
		"Checking memory...",
		"Scanning system files...",
		"Analyzing network connections...",
		"Inspecting startup programs...",
		"Verifying file integrity..."
	};
	private int currentScanStep = 0;
	
	public override void _Ready()
	{
		TerminalOutput = GetNode<RichTextLabel>(OutputPath);
		TerminalOutput.BbcodeEnabled = true;
		TerminalOutput.Clear();
		TerminalOutput.ScrollFollowing = true; // Enable auto-scrolling
		Input.MouseMode = Input.MouseModeEnum.Visible;
		
		// Get the username from the OS
		username = OS.GetEnvironment("USERNAME"); // Windows
		if (string.IsNullOrEmpty(username))
		{
			username = OS.GetEnvironment("USER"); // macOS/Linux
		}
		if (string.IsNullOrEmpty(username))
		{
			username = "user"; // Default fallback
		}
		
		// Booting
		bootLines.Enqueue("[b][color=lime][AVX SYSTEM BOOT SEQUENCE INITIATED][/color][/b]");
		bootLines.Enqueue("> Loading modules...");
		bootLines.Enqueue("> Connecting to secure sandbox...");
		bootLines.Enqueue($"> Authenticating user: [color=yellow]{username}[/color]");
		bootLines.Enqueue("> DeepClean Protocol version 1.4.9");
		bootLines.Enqueue("> System ready. Type 'help'.");
	}
	
	public override void _Process(double delta)
	{
		if (booting)
		{
			typingTimer -= (float)delta;
			if (typingTimer <= 0 && bootLines.Count > 0)
			{
				string line = bootLines.Dequeue();
				AppendToTerminal(line + "\n");
				typingTimer = typingDelay;
			}
			else if (bootLines.Count == 0)
			{
				booting = false;
				AddPrompt();
			}
		}
		else if (isScanning)
		{
			// Handle scanning animation
			ProcessScanning((float)delta);
		}
		else
		{
			// Handle caret blinking
			caretTimer -= (float)delta;
			if (caretTimer <= 0)
			{
				caretTimer = caretBlinkTime;
				caretVisible = !caretVisible;
				UpdateTerminalWithCaret();
			}
		}
	}
	
	private void ProcessScanning(float delta)
	{
		scanTimer += delta;
		
		// Update animation frame
		scanAnimationTimer -= delta;
		if (scanAnimationTimer <= 0)
		{
			scanAnimationTimer = scanAnimationDelay;
			scanAnimationFrame = (scanAnimationFrame + 1) % scanAnimationFrames.Length;
			
			// Update scan step
			float stepDuration = scanDuration / scanningSteps.Count;
			int expectedStep = Mathf.Min((int)(scanTimer / stepDuration), scanningSteps.Count - 1);
			
			if (expectedStep > currentScanStep)
			{
				currentScanStep = expectedStep;
				// Add a checkmark to the previous step
				int lastLinePos = terminalText.LastIndexOf('\n');
				if (lastLinePos >= 0)
				{
					terminalText = terminalText.Substring(0, lastLinePos) + " [color=lime]✓[/color]\n";
				}
			}
			
			// Update scanning line
			scanningLine = $"[color=cyan]{scanningSteps[currentScanStep]} {scanAnimationFrames[scanAnimationFrame]}[/color]";
			
			// Update terminal
			string textWithoutScanLine = terminalText;
			int lastNewLinePos = textWithoutScanLine.LastIndexOf('\n');
			if (lastNewLinePos >= 0 && lastNewLinePos < textWithoutScanLine.Length - 1)
			{
				textWithoutScanLine = textWithoutScanLine.Substring(0, lastNewLinePos + 1);
			}
			
			TerminalOutput.Text = textWithoutScanLine + scanningLine;
			TerminalOutput.ScrollToLine(TerminalOutput.GetLineCount() - 1);
		}
		
		// Check if scanning is complete
		if (scanTimer >= scanDuration)
		{
			isScanning = false;
			// Add a checkmark to the last step
			terminalText += $" [color=lime]✓[/color]\n";
			
			if (totalFakeFiles > 0)
			{
				// Report detected threats by category
				AppendToTerminal($"[color=red]❗ Scan complete. Found {totalFakeFiles} suspicious files:[/color]\n");
				
				foreach (var category in fakeFilesByCategory.Keys)
				{
					var fakeFiles = fakeFilesByCategory[category];
					if (fakeFiles.Count > 0)
					{
						AppendToTerminal($"[color=yellow]Category: {category.ToUpper()}[/color]\n");
						
						// Display all fake files in this category
						foreach (var fileName in fakeFiles)
						{
							AppendToTerminal($"[color=red]- {fileName}[/color]\n");
						}
					}
				}
				
				AppendToTerminal("[color=yellow]Please manually remove these suspicious files from your system.[/color]\n");
			}
			else
			{
				// No threats found
				AppendToTerminal("[color=lime]✔ Scan complete. No suspicious files found.[/color]\n");
			}
			
			TerminalOutput.Text = terminalText;
			TerminalOutput.ScrollToLine(TerminalOutput.GetLineCount() - 1);
			
			// Add prompt after scan is complete
			AddPrompt();
		}
	}
	
	private void AppendToTerminal(string text)
	{
		terminalText += text;
		TerminalOutput.Text = terminalText;
		TerminalOutput.ScrollToLine(TerminalOutput.GetLineCount() - 1); // Scroll to bottom
	}
	
	private void UpdateTerminalWithCaret()
	{
		// Show text with or without caret based on blink state
		if (!booting && !isScanning)
		{
			string textWithoutCaret = terminalText.TrimEnd(caretSymbol[0]);
			TerminalOutput.Text = caretVisible ? textWithoutCaret + caretSymbol : textWithoutCaret;
			TerminalOutput.ScrollToLine(TerminalOutput.GetLineCount() - 1); // Scroll to bottom
		}
	}
	
	private void AddPrompt()
	{
		AppendToTerminal($"[color=yellow]{username}[/color]/avx> ");
		currentInput.Clear();
		caretTimer = caretBlinkTime; // Reset caret timer
		caretVisible = true;
		UpdateTerminalWithCaret();
	}
	
	public override void _Input(InputEvent @event)
	{
		if (booting || isScanning || !IsInsideTree()) return;
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			var scancode = keyEvent.Keycode;
			// Enter to submit
			if (scancode == Key.Enter)
			{
				string command = currentInput.ToString();
				// Make sure to remove caret before adding newline
				terminalText = terminalText.TrimEnd(caretSymbol[0]);
				// Add newline after the command
				AppendToTerminal("\n");
				ProcessCommand(command);
				if (!isScanning) // Only add prompt if not scanning
				{
					currentInput.Clear();
					AddPrompt();
				}
			}
			// Backspace
			else if (scancode == Key.Backspace && currentInput.Length > 0)
			{
				// Remove last character from input
				currentInput.Remove(currentInput.Length - 1, 1);
				// Make sure to remove caret before removing character
				terminalText = terminalText.TrimEnd(caretSymbol[0]);
				// Remove last character from terminal
				terminalText = terminalText.Substring(0, terminalText.Length - 1);
				// Reset caret blinking when typing
				caretTimer = caretBlinkTime;
				caretVisible = true;
				UpdateTerminalWithCaret();
			}
			// Characters
			else if (keyEvent.Unicode > 31 && keyEvent.Unicode < 127)
			{
				char c = (char)keyEvent.Unicode;
				currentInput.Append(c);
				// Make sure to remove caret before adding character
				terminalText = terminalText.TrimEnd(caretSymbol[0]);
				terminalText += c;
				// Reset caret blinking when typing
				caretTimer = caretBlinkTime;
				caretVisible = true;
				UpdateTerminalWithCaret();
			}
		}
	}
	
	private void ProcessCommand(string command)
	{
		switch (command.ToLower())
		{
			case "scan":
				StartScanning();
				break;
			case "help":
				AppendToTerminal("Available: [color=yellow]scan[/color], [color=yellow]clear[/color], [color=yellow]help[/color], [color=yellow]exit[/color]\n");
				AppendToTerminal("[color=yellow]scan[/color] - Detect suspicious files in your system\n");
				AppendToTerminal("[color=yellow]clear[/color] - Clear terminal display\n");
				AppendToTerminal("[color=yellow]exit[/color] - Close antivirus application\n");
				break;
			case "clear":
				terminalText = "";
				TerminalOutput.Clear();
				return; // Return early as we'll be adding prompt after this
			case "exit":
				AppendToTerminal("[color=lime]Shutting down AVX protection system...[/color]\n");
				GetTree().CreateTimer(1.0f).Connect("timeout", Callable.From(() => QueueFree()));
				return;
			default:
				AppendToTerminal($"[color=red]Unknown command:[/color] {command}\n");
				break;
		}
	}
	
	private void StartScanning()
	{
		isScanning = true;
		scanTimer = 0;
		currentScanStep = 0;
		scanAnimationFrame = 0;
		scanAnimationTimer = 0;
		detectedFakeFiles.Clear();
		fakeFilesByCategory.Clear();
		
		// Cek jumlah fake files yang ada di sistem dan catat kategorinya
		totalFakeFiles = 0;
		foreach (string category in new[] { "system", "download", "video", "picture", "music" })
		{
			var files = Global.GetCurrentFilesByCategory(category);
			fakeFilesByCategory[category] = new List<string>();
			
			foreach (var file in files)
			{
				if (Global.IsFileFake(file.Name))
				{
					totalFakeFiles++;
					detectedFakeFiles.Add(file.Name);
					fakeFilesByCategory[category].Add(file.Name);
				}
			}
		}
		
		AppendToTerminal("[color=cyan]Starting system scan...[/color]\n");
	}
	
	private void _on_close_requested(){
		QueueFree();
	}
}
