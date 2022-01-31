using CodeAnalyze.Models;
using CodeAnalyze.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
	public sealed partial class ManagePage: Page {
		public readonly ObservableCollection<Set> sets = new ObservableCollection<Set>();
		public ManagePage() {
			this.InitializeComponent();
			Initialize();
		}
		private async void Initialize() {
			LoadingRing.IsActive = true;
			await SetsManager.ReadLocal();
			ReloadFromSets();
			LoadingRing.IsActive = false;
		}

		private void ReloadFromSets() {
			sets.Clear();
			foreach(Set item in SetsManager.SetsList) {
				sets.Add(item);
			}
		}

		private void SetsListViewItemPanel_RightTapped(object sender, RightTappedRoutedEventArgs e) {
			FrameworkElement element = sender as FrameworkElement;
			MenuFlyout flyout = new MenuFlyout() {
				Placement = FlyoutPlacementMode.Bottom,
			};
			MenuFlyoutItem item_delete = new MenuFlyoutItem() {
				Icon = new FontIcon() { Glyph = "\uE74D" },
				Text = "Delete",
			};
			item_delete.Click += async (s, args) => {
				string name = element.Tag as string;
				await SetsManager.Remove(name);
				ReloadFromSets();
			};
			flyout.Items.Add(item_delete);
			flyout.ShowAt(element, e.GetPosition(element));
		}
	}
}
