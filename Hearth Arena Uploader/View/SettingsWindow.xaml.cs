using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HearthArenaUploader
{
	/// <summary>
	/// Interaktionslogik für SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : MetroWindow
	{
		public SettingsWindow()
		{
			InitializeComponent();
			textboxAccountName.Text = PluginSettings.Instance.AccountName ?? string.Empty;
			passwordbox.Password = Encryption.ToInsecureString(PluginSettings.Instance.Password);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			PluginSettings.Instance.AccountName = textboxAccountName.Text;
			PluginSettings.Instance.Password = Encryption.ToSecureString(passwordbox.Password);
			PluginSettings.Save();
		}
	}
}
