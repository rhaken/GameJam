using Godot;
using System;

public partial class CpuGui : Label
{
	public override void _Process(double delta)
	{
		// Ambil nilai CPU process dari Global
		float cpuUsage = (float)Global.CpuProcess;

		// Update teks label
		Text = $"CPU: {cpuUsage:0.0}%";

		// Ubah warna berdasarkan nilai CPU
		if (cpuUsage > 85)
		{
			AddThemeColorOverride("font_color", new Color(1, 0, 0)); // Merah
		}
		else if (cpuUsage > 60)
		{
			AddThemeColorOverride("font_color", new Color(1, 1, 0)); // Kuning
		}
		else
		{
			AddThemeColorOverride("font_color", new Color(1, 1, 1)); // Putih
		}
	}
}
