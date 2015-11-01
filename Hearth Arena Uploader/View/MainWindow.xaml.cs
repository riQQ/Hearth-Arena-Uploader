using HearthArenaUploader.Data;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace HearthArenaUploader.View
{
	/// <summary>
	/// 
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		private bool hideUploaded = false;
		ICollectionView collectionView;

		public MainWindow()
		{
			InitializeComponent();

			hideUploaded = false;
			collectionView = CollectionViewSource.GetDefaultView(DeckList.Instance.Decks);
			collectionView.Filter = (deckObj) =>
			{
				Deck tempDeck = deckObj as Deck;
				HashSet<Guid> uploadedDecks = PluginSettings.Instance.UploadedDecks;
				return tempDeck != null && tempDeck.IsArenaDeck && (!hideUploaded || !uploadedDecks.Contains(tempDeck.DeckId));
			};
			this.dataGrid.ItemsSource = collectionView;
			this.dataGrid.Items.SortDescriptions.Add(new SortDescription(this.lastPlayedColumn.SortMemberPath, ListSortDirection.Descending));
			this.lastPlayedColumn.SortDirection = ListSortDirection.Descending;
		}

		private async void ButtonUploadSelectedRuns_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			await UploadSelectedRuns();
		}

		private void ButtonOpenSettings_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			OpenSettings();
		}

		private void HideAlreadyUploaded_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			HideUploaded = checkBoxHideAlreadyUploaded.IsChecked.HasValue && checkBoxHideAlreadyUploaded.IsChecked.Value;
		}

		public async Task UploadSelectedRuns()
		{
			MetroWindow pluginWindow = Window.GetWindow(this) as MetroWindow;

			if (string.IsNullOrEmpty(PluginSettings.Instance.AccountName) || string.IsNullOrEmpty(PluginSettings.Instance.EncryptedPassword))
			{
				MessageDialogResult messageDialogResult = await pluginWindow.ShowMessageAsync("Error", "Empty account name and/or password. Do you want to open the settings?", MessageDialogStyle.AffirmativeAndNegative);
				if (messageDialogResult == MessageDialogResult.Affirmative)
				{
					OpenSettings();
				}
				return;
			}
			HearthArenaUploaderLogic controller = new HearthArenaUploaderLogic(PluginSettings.Instance.AccountName, PluginSettings.Instance.Password);
			ProgressDialogController progressDialogController = await pluginWindow.ShowProgressAsync("Upload to Hearth Arena", "Uploading arena runs to Hearth Arena");

			Result<UploadResults> result = await controller.LoginAndSubmitArenaRuns(SelectedDecks, progressDialogController.SetProgress);
			collectionView.Refresh();
			await progressDialogController.CloseAsync();
			if (result.Outcome != UploadResults.Success)
			{
				await pluginWindow.ShowMessageAsync("Error", "Error uploading arena runs: " + result.ErrorMessage);
			}
		}

		private async void MenuItem_Click_UploadToHearthArena(object sender, RoutedEventArgs e)
		{
			await UploadSelectedRuns();
		}

		public List<Deck> SelectedDecks
		{
			get
			{
				return this.dataGrid.SelectedItems.Cast<Deck>().ToList();
			}
		}

		public void OpenSettings()
		{
			SettingsWindow settingsWindow = new SettingsWindow();
			settingsWindow.Show();
		}

		public bool HideUploaded
		{
			get
			{
				return hideUploaded;
			}
			set
			{
				hideUploaded = value;
				collectionView.Refresh();
			}
		}
	}
}
