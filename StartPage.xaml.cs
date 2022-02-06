using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace CodeAnalyze {
	public sealed partial class StartPage: Page {
		public StartPage() {
			this.InitializeComponent();
		}

		private async void Button_Tapped(object sender, TappedRoutedEventArgs e) {
			FolderPicker pick = new FolderPicker() {
				FileTypeFilter = { "*" },
			};
			StorageFolder result = await pick.PickSingleFolderAsync();
			if(result != null) {
				(Window.Current.Content as Frame).Navigate(typeof(MainPage), result, new DrillInNavigationTransitionInfo());
			}
		}
	}
}
