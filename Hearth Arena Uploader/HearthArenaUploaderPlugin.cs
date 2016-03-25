using HearthArenaUploader.View;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HearthArenaUploader
{
	public class HearthArenaUploaderPlugin : Hearthstone_Deck_Tracker.Plugins.IPlugin
	{
		public void OnLoad()
		{
			PluginSettings.Load();
			CreateMenuItem();
		}

		private void CreateMenuItem()
		{
			MenuItem = new MenuItem()
			{
				Header = "Hearth Arena Uploader"
			};

			MenuItem.Click += (sender, args) =>
			{
				ShowWindow();
			};
		}

		private void ShowWindow()
		{
			MainWindow mainWindow = new MainWindow();
			mainWindow.Show();
		}

		public void OnUnload()
		{
			PluginSettings.Save();
		}

		public void OnButtonPress()
		{
			SettingsWindow settingsWindow = new SettingsWindow();
			settingsWindow.Show();
		}

		public void OnUpdate()
		{

		}

		public string Name
		{
			get { return "Hearth Arena Uploader"; }
		}

		public string Description
		{
			get
			{
				return @"Plugin for uploading your arena runs to Hearth Arena (http://heartharena.com).
Suggestions and bug reports can be sent to https://github.com/riQQ/Hearth-Arena-Uploader.";
			}
		}

		public string ButtonText
		{
			get { return "Settings"; }
		}

		public string Author
		{
			get { return "riQQ"; }
		}

		public static readonly Version PluginVersion = new Version(0, 1, 0);

		public Version Version
		{
			get { return PluginVersion; }
		}

		public MenuItem MenuItem
		{
			get;
			private set;
		}

		internal static PluginSettings Settings { get; set; }
	}
}
