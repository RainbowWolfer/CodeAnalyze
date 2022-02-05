using CodeAnalyze.Models;
using CodeAnalyze.Services;
using CodeAnalyze.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CodeAnalyze {
	public sealed partial class ManagePage: Page {
		public ObservableCollection<Set> Sets { get; } = new ObservableCollection<Set>();
		public ObservableCollection<FolderItem> Items { get; } = new ObservableCollection<FolderItem>();
		public FolderItem Root { get; private set; }
		public static Set CurrentSet { get; private set; }
		public static Button FileTypeInputAddButton { get; private set; }

		public ManagePage() {
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Required;
			Initialize();
		}

		private async void Initialize() {
			LoadingRing.IsActive = true;
			await SetsManager.ReadLocal();
			ReloadFromSets();
			LoadingRing.IsActive = false;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter is StorageFolder folder) {
				this.Root = new FolderItem(folder);
				Items.Clear();
				await ReadSubFolders(null, folder);
				FolderTreeView.ItemsSource = Items;
			}
		}

		public void UpdateFileTypeInputs(Set set) {
			FileTypeInputsListView.Items.Clear();
			foreach(string item in set.Files) {
				FileTypeInputsListView.Items.Add(new FileTypeInput(this, new FileTypeInputModel(false, set, item), async newInput => {
					CurrentSet.Files = GetCurrentSetInputs();
					await SetsManager.UpdateSet(CurrentSet);
				}));
			}
			var add = new FileTypeInput(this, new FileTypeInputModel(true));
			FileTypeInputsListView.Items.Add(add);
			FileTypeInputAddButton = add.AddButtonExternal;
		}

		public void AddEmptyFileTypeInput() {
			FileTypeInputsListView.Items.Insert(FileTypeInputsListView.Items.Count - 1, new FileTypeInput(this, new FileTypeInputModel(false, CurrentSet), async newInput => {
				CurrentSet.Files = GetCurrentSetInputs();
				await SetsManager.UpdateSet(CurrentSet);
			}));
		}

		public async Task RemoveFileTypeInput(FileTypeInput input) {
			FileTypeInputsListView.Items.Remove(input);
			CurrentSet.Files = GetCurrentSetInputs();
			await SetsManager.UpdateSet(CurrentSet);
		}

		public List<string> GetCurrentSetInputs() {
			return FileTypeInputsListView.Items.Cast<FileTypeInput>().Where(i => !i.model.IsAdd).Select(i => i.model.Input).ToList();
		}

		public List<FolderItem> GetSelected() {
			return FolderTreeView.SelectedItems.Cast<FolderItem>().Append(Root).ToList();
		}

		private async Task ReadSubFolders(FolderItem parent, StorageFolder folder) {
			foreach(StorageFolder item in await folder.GetFoldersAsync()) {
				FolderItem sub = new FolderItem(item);
				if(parent == null) {
					Items.Add(sub);
					this.Root.Children.Add(sub);
				} else {
					parent.Children.Add(sub);
				}
				await ReadSubFolders(sub, item);
				FolderTreeView.UpdateLayout();
			}
		}

		private void ReloadFromSets() {
			Sets.Clear();
			foreach(Set item in SetsManager.SetsList) {
				Sets.Add(item);
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
				Sets.Remove(Sets.Where(set => set.Name == name).FirstOrDefault());
				await SetsManager.Remove(name);
			};
			flyout.Items.Add(item_delete);
			flyout.ShowAt(element, e.GetPosition(element));
		}

		private void SetsListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(e.AddedItems.FirstOrDefault() is Set set) {
				CurrentSet = set;
				UpdateFileTypeInputs(set);
			}
		}

		private async void AddSetButton_Tapped(object sender, TappedRoutedEventArgs e) {
			AddSetButton.IsEnabled = false;
			ContentDialog dialog = new ContentDialog() {
				Title = "Add New Set"
			};
			InputTextDialog content = new InputTextDialog(dialog, Sets.Select(s => s.Name));
			dialog.Content = content;
			await dialog.ShowAsync();
			if(content.Accept) {
				var newSet = new Set(content.InputText);
				Sets.Add(newSet);
				await SetsManager.Add(newSet);
			}
			AddSetButton.IsEnabled = true;
		}
	}
}
