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

namespace CodeAnalyze {
	public sealed partial class MainPage: Page {
		public MainPage() {
			this.InitializeComponent();
		}
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter is StorageFolder folder) {
				RootText.Text = folder.Path;
				MainFrame.Navigate(typeof(ManagePage), folder, new EntranceNavigationTransitionInfo());
			}
		}

		private void ManageButton_Tapped(object sender, TappedRoutedEventArgs e) {
			MainFrame.Navigate(typeof(ManagePage), null, new EntranceNavigationTransitionInfo());
		}

		private void SelectButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}

		private void StartButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}
	}
}
