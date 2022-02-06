using CodeAnalyze.Models;
using CodeAnalyze.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Controls;
using NavigationViewItemInvokedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewItem = Windows.UI.Xaml.Controls.NavigationViewItem;
using Windows.Storage.Pickers;

namespace CodeAnalyze {
	public sealed partial class MainPage: Page {
		public static MainPage Instance;
		public MainPage() {
			Instance = this;
			this.InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter is StorageFolder folder) {
				RootText.Text = folder.Path;
				MainFrame.Navigate(typeof(ManagePage), folder, new EntranceNavigationTransitionInfo());
			}
		}

		private async void SelectButton_Tapped(object sender, TappedRoutedEventArgs e) {
			FolderPicker pick = new FolderPicker() { FileTypeFilter = { "*" } };
			StorageFolder result = await pick.PickSingleFolderAsync();
			if(result != null) {
				RootText.Text = result.Path;
				MainFrame.Navigate(typeof(ManagePage), result, new EntranceNavigationTransitionInfo());
			}
		}

		private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
			string tag = (string)args.InvokedItemContainer.Tag;
			if(tag == "Settings") {
				NavigateTo(PageType.Settings);
			} else {
				NavigateTo((PageType)int.Parse(tag));
			}
		}

		public void NavigateTo(PageType type, object parameter = null) {
			MainNavigationView.SelectedItem = MainNavigationView.MenuItems
				.Concat(MainNavigationView.FooterMenuItems)
				.Append(MainNavigationView.SettingsItem)
				.ToArray()[(int)type];
			switch(type) {
				case PageType.Manage:
					MainFrame.Navigate(typeof(ManagePage), parameter, new EntranceNavigationTransitionInfo());
					break;
				case PageType.Result:
					MainFrame.Navigate(typeof(ResultPage), parameter, new EntranceNavigationTransitionInfo());
					break;
				case PageType.Help:
					MainFrame.Navigate(typeof(HelpPage), parameter, new EntranceNavigationTransitionInfo());
					break;
				case PageType.Settings:
					MainFrame.Navigate(typeof(SettingsPage), parameter, new EntranceNavigationTransitionInfo());
					break;
				default:
					throw new Exception($"({type}) is not defined.");
			}
		}
	}

	public enum PageType {
		Manage = 0, Result = 1, Help = 2, Settings = 3
	}
}
