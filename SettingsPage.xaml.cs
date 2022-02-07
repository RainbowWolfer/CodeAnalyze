using CodeAnalyze.Services;
using CodeAnalyze.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CodeAnalyze {
	public sealed partial class SettingsPage: Page {
		public Button IgnoreFoldersAddButton { get; set; }
		public string Version => "Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();

		public SettingsPage() {
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Required;
			InitializeIgnoreFolders();
			SkipEmptyLinesToggle.IsOn = Local.LocalSettings?.SkipEmptyLine ?? false;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
		}

		private async void InitializeIgnoreFolders() {
			IgnoreFoldersLoadingRing.IsActive = true;
			await Local.ReadIgnoreFolders();
			IgnoreFoldersListView.Items.Clear();
			for(int i = 0; i < Local.IgnoreFolders.Count; i++) {
				string item = Local.IgnoreFolders[i];
				IgnoreFoldersListView.Items.Add(new IgnoreFolderInput(this, i, false, item, OnInputChanged()));
			}
			IgnoreFoldersListView.Items.Add(new IgnoreFolderInput(this, 0, true));
			IgnoreFoldersLoadingRing.IsActive = false;
		}

		public async Task RemoveIgnoreFolder(IgnoreFolderInput item) {
			IgnoreFoldersListView.Items.Remove(item);
			await Local.UpdateIgnoreFolders(GetCurrentIgnoreFolders());
		}

		public void AddIgnoreFolder() {
			IgnoreFoldersListView.Items.Insert(IgnoreFoldersListView.Items.Count - 1, new IgnoreFolderInput(this, 0, false, "", OnInputChanged()));
		}

		public string[] GetCurrentIgnoreFolders() {
			return IgnoreFoldersListView.Items.Cast<IgnoreFolderInput>().Select(i => i.Input).Where(i => !string.IsNullOrWhiteSpace(i)).ToArray();
		}

		private Action<string> OnInputChanged() {
			return async newInput => {
				await Local.UpdateIgnoreFolders(GetCurrentIgnoreFolders());
			};
		}

		private async void SkipEmptyLinesToggle_Toggled(object sender, RoutedEventArgs e) {
			Local.LocalSettings.SkipEmptyLine = (sender as ToggleSwitch).IsOn;
			await Local.SaveLocalSettings();
		}

		private async void EmailButton_Tapped(object sender, TappedRoutedEventArgs e) {
			await ComposeEmail("[Code Analyzer] Subject Here", "Your Feedback Here");
		}
		private async Task ComposeEmail(string subject, string messageBody) {
			var emailMessage = new EmailMessage {
				Subject = subject,
				Body = messageBody,
			};
			emailMessage.To.Add(new EmailRecipient("RainbowWolfer@Outlook.com", "RainbowWolfer"));
			await EmailManager.ShowComposeNewEmailAsync(emailMessage);
		}
	}
}
