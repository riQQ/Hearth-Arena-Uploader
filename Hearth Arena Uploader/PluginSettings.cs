using Hearthstone_Deck_Tracker;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace HearthArenaUploader
{
	public class PluginSettings
	{
		private static PluginSettings _pluginSettings;		 
		private static readonly string configFileName = "config.xml";

		private PluginSettings()
		{
			
		}

		public static PluginSettings Instance
		{
			get
			{
				if (_pluginSettings == null)
				{
					_pluginSettings = new PluginSettings();
				}

				return _pluginSettings;
			}
		}

		public static void Load()
		{
			string configDir = Path.Combine(Config.Instance.DataDir, "HearthArenaUploader");
			bool noConfig = false;
			if (!Directory.Exists(configDir))	
			{
				noConfig = true;
				Directory.CreateDirectory(configDir);
			}

			if (!noConfig)
			{
				try
				{
					string configPath = Path.Combine(configDir, configFileName);
					if (File.Exists(configPath))
					{
						_pluginSettings = XmlManager<PluginSettings>.Load(configPath);
					}

				}
				catch (Exception e)
				{
					MessageBox.Show(
									e.Message + "\n\n" + e.InnerException + "\n\n If you don't know how to fix this, please delete "
									+ configDir, "Error loading config.xml");
				}
			}
		}

		public static void Save()
		{
			string configDir = Path.Combine(Config.Instance.DataDir, "HearthArenaUploader");
			XmlManager<PluginSettings>.Save(Path.Combine(configDir, configFileName), Instance);
		}

		#region Settings
		public string AccountName = string.Empty;

		private string password;

		[XmlIgnore]
		public SecureString Password
		{ 
			get
			{
				return Encryption.DecryptString(password);
			}
			set
			{
				password = Encryption.EncryptString(value);
			}
		}
		
		public string EncryptedPassword
		{ 
			get
			{
				return password;
			}
			set
			{
				password = value;
			}
		}

		public HashSet<Guid> UploadedDecks = new HashSet<Guid>();
		#endregion
	}
}
