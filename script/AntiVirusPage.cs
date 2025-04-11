using Godot;
using System;
using System.Collections.Generic;

public partial class AntiVirusPage : Control
{
	[Export] public PackedScene VirusCardScene;

	private VBoxContainer virusList;

	private List<VirusData> viruses = new()
	{
		new VirusData(
			"PopUpPest",
			"Virus ringan yang membanjiri layar dengan pop-up acak berisi meme, iklan palsu, atau pesan aneh.\n" +
			"Pop-up ini akan terus muncul secara berkala dan mengganggu aktivitas pengguna.\n" +
			"Jika tidak segera ditutup, setiap pop-up akan memakan sekitar 5% CPU,\n" +
			"Cara mengatasi:\n" +
			"- Tutup semua pop-up yang muncul sesegera mungkin untuk mencegah konsumsi CPU berlebih.",
			"res://assets/icon/browser.png"
		),
		new VirusData(
			"SilentClone",
			"Virus ini menyamar sebagai file sistem bernama 'sys_cache.tmp' dan terlihat tidak mencurigakan.\n" +
			"Jika tidak segera dihapus, SilentClone akan diam-diam menggunakan 10% CPU secara terus-menerus,\n" +
			"Cara mengatasi:\n" +
			"- Jalankan Scan dengan AVX Antivirus untuk mendeteksi keberadaan SilentClone.\n" +
			"- Setelah terdeteksi, hapus file secara manual melalui pengelola file.\n",
			"res://assets/icon/browser.png"
		),
	};

	public override void _Ready()
	{
		virusList = GetNode<VBoxContainer>("VBoxContainer/MarginContainer/ScrollContainer/VirusList");


		foreach (var virus in viruses)
		{
			var card = (Control)VirusCardScene.Instantiate();
			if (card is ListVirusPage virusCard)
			{
				virusCard.SetVirus(virus);
				virusList.AddChild(virusCard);
			}
		}
	}


}

public class VirusData
{
	public string Name;
	public string Description;
	public string ImagePath;

	public VirusData(string name, string desc, string img)
	{
		Name = name;
		Description = desc;
		ImagePath = img;
	}
}
