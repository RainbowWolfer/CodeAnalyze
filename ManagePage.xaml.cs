using CodeAnalyze.Models;
using CodeAnalyze.Services;
using CodeAnalyze.Views;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using TreeViewItem = Microsoft.UI.Xaml.Controls.TreeViewItem;

namespace CodeAnalyze {
	public sealed partial class ManagePage: Page {
		public static ManagePage Instance { get; private set; }
		public ObservableCollection<Set> Sets { get; } = new ObservableCollection<Set>();
		public ObservableCollection<FolderItem> Items { get; } = new ObservableCollection<FolderItem>();
		public Dictionary<string, long> FilesTypeCount { get; private set; } = new Dictionary<string, long>();
		public FolderItem Root { get; private set; }
		public static Set CurrentSet { get; private set; }
		public static Button FileTypeInputAddButton { get; private set; }

		public ManagePage() {
			Instance = this;
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Required;
			this.DataContextChanged += (s, e) => Bindings.Update();
			Initialize();
		}

		private async void Initialize() {
			SetsLoadingRing.IsActive = true;
			await Local.ReadSets();
			ReloadFromSets();
			SetsLoadingRing.IsActive = false;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter is StorageFolder folder) {
				FilesStatisticsLoadingRing.IsActive = true;
				FoldersLoadingPanel.Visibility = Visibility.Visible;
				FilesTypeCount.Clear();
				Items.Clear();
				FolderTreeView.ItemsSource = null;
				this.Root = new FolderItem(folder) {
					IsExpanded = true,
				};
				Items.Add(Root);
				await ReadSubFolders(Root, folder);
				FolderTreeView.ItemsSource = Items;
				FoldersLoadingPanel.Visibility = Visibility.Collapsed;
				UpdateFilesTypeCount();
				FilesStatisticsLoadingRing.IsActive = false;
			}
		}

		public void UpdateFilesTypeCount() {
			FilesStatisticsPanel.Children.Clear();
			foreach(var item in FilesTypeCount.OrderByDescending(o => o.Value).Take(10)) {
				Grid grid = new Grid();
				grid.ColumnDefinitions.Add(new ColumnDefinition());
				grid.ColumnDefinitions.Add(new ColumnDefinition());
				TextBlock key = new TextBlock() {
					Text = item.Key,
					TextAlignment = TextAlignment.Right,
					Margin = new Thickness(0, 0, 10, 0),
				};
				TextBlock value = new TextBlock() {
					Text = $"{item.Value}",
					Margin = new Thickness(20, 0, 0, 0),
				};
				Grid.SetColumn(value, 1);
				grid.Children.Add(key);
				grid.Children.Add(value);
				FilesStatisticsPanel.Children.Add(grid);
			}
		}

		public void UpdateFileTypeInputs(Set set) {
			FileTypeInputsListView.Items.Clear();
			foreach(string item in set.Files) {
				FileTypeInputsListView.Items.Add(new FileTypeInput(this, new FileTypeInputModel(false, set, item), OnInputChanged()));
			}
			var add = new FileTypeInput(this, new FileTypeInputModel(true));
			FileTypeInputsListView.Items.Add(add);
			FileTypeInputAddButton = add.AddButtonExternal;
		}

		public void AddEmptyFileTypeInput() {
			FileTypeInputsListView.Items.Insert(FileTypeInputsListView.Items.Count - 1, new FileTypeInput(this, new FileTypeInputModel(false, CurrentSet), OnInputChanged()));
		}

		private Action<string> OnInputChanged() {
			return async newInput => {
				CurrentSet.Files = GetCurrentSetInputs();
				await Local.UpdateSet(CurrentSet);
			};
		}

		public async Task RemoveFileTypeInput(FileTypeInput input) {
			FileTypeInputsListView.Items.Remove(input);
			CurrentSet.Files = GetCurrentSetInputs();
			await Local.UpdateSet(CurrentSet);
		}

		public List<string> GetCurrentSetInputs() {
			return FileTypeInputsListView.Items.Cast<FileTypeInput>().Where(i => !i.model.IsAdd).Select(i => i.model.Input).ToList();
		}

		public static List<FolderItem> GetSelected() {
			return Instance.FolderTreeView.SelectedItems.Cast<FolderItem>().Append(Instance.Root).ToList();
		}

		private async Task ReadSubFolders(FolderItem parent, StorageFolder folder) {
			if(Local.IgnoreFolders.Contains(parent.Name)) {
				FoldersLoadingText.Text = $"Ignore Folder: {folder.Path}";
				return;
			}
			foreach(StorageFolder item in await folder.GetFoldersAsync()) {
				FolderItem sub = new FolderItem(item);
				FoldersLoadingText.Text = $"Loading Sub Files: {item.Path}";
				await ReadFiles(sub);
				parent.Children.Add(sub);
				FoldersLoadingText.Text = $"Loading Sub Folders: {item.Path}";
				await ReadSubFolders(sub, item);
			}
		}

		private async Task ReadFiles(FolderItem item) {
			foreach(StorageFile file in await item.Folder.GetFilesAsync()) {
				if(FilesTypeCount.ContainsKey(file.FileType)) {
					FilesTypeCount[file.FileType]++;
				} else {
					FilesTypeCount.Add(file.FileType, 1);
				}
			}
		}

		private void ReloadFromSets() {
			Sets.Clear();
			foreach(Set item in Local.SetsList) {
				Sets.Add(item);
			}
		}

		private void SetsListViewItemPanel_RightTapped(object sender, RightTappedRoutedEventArgs e) {
			FrameworkElement element = sender as FrameworkElement;
			string name = element.Tag as string;
			MenuFlyout flyout = new MenuFlyout() {
				Placement = FlyoutPlacementMode.Bottom,
			};
			MenuFlyoutItem item_delete = new MenuFlyoutItem() {
				Icon = new FontIcon() { Glyph = "\uE74D" },
				Text = "Delete",
			};
			MenuFlyoutItem item_rename = new MenuFlyoutItem() {
				Icon = new FontIcon() { Glyph = "\uE104" },
				Text = "Rename",
			};
			item_delete.Click += async (s, args) => {
				Sets.Remove(Sets.Where(set => set.Name == name).FirstOrDefault());
				await Local.RemoveSet(name);
			};
			item_rename.Click += async (s, args) => {
				ContentDialog dialog = new ContentDialog() {
					Title = $"Rename ({name})"
				};
				InputTextDialog content = new InputTextDialog(dialog, Sets.Select(set => set.Name), name);
				dialog.Content = content;
				await dialog.ShowAsync();
				if(content.Accept) {
					Sets.Where(set => set.Name == name).FirstOrDefault().Name = content.InputText;
					await Local.SaveSets();
				}
			};
			flyout.Items.Add(item_delete);
			flyout.Items.Add(item_rename);
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
				await Local.AddSet(newSet);
			}
			AddSetButton.IsEnabled = true;
		}

		private async void TextBlock_Loaded(object sender, RoutedEventArgs e) {
			var text = (TextBlock)sender;
			var folder = (StorageFolder)text.Tag;
			if(Local.IgnoreFolders.Contains(folder.Name)) {
				text.Text = "Ignored";
			} else {
				IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
				text.Text = $"{files.Count()}";
			}
		}

		private void StartButton_Tapped(object sender, TappedRoutedEventArgs e) {
			List<FolderItem> selected = GetSelected();
			selected = selected.Where(i => !Local.IgnoreFolders.Contains(i.Name)).ToList();
			Set set = CurrentSet;
			if(selected.Where(s => s != Root).Count() == 0) {
				NoFoldersSelectedTip.IsOpen = true;
				return;
			} else if(set.Files.Count == 0) {
				NoFileTypeInputTip.Target = FileTypeInputsListView.Items.Cast<FileTypeInput>().LastOrDefault();
				NoFileTypeInputTip.IsOpen = true;
				return;
			} else if(set.Files.Any(f => string.IsNullOrWhiteSpace(f))) {
				EmptyFileTypeInputTip.Target = FileTypeInputsListView.Items.Cast<FileTypeInput>().FirstOrDefault(f => string.IsNullOrWhiteSpace(f.model.Input));
				EmptyFileTypeInputTip.IsOpen = true;
				return;
			}

			bool accept = false;

			foreach(string item in FilesTypeCount.Select(s => s.Key)) {
				foreach(string input in set.Files) {
					if(input == item) {
						accept = true;
					}
				}
			}

			if(!accept) {
				UnacceptedFileTypeInputTip.IsOpen = true;
				return;
			}

			MainPage.Instance.NavigateTo(PageType.Result, new AnalyzePamameters(Root, set, selected));
		}

		private void TreeViewItem_Loaded(object sender, RoutedEventArgs e) {
			var item = sender as TreeViewItem;
			var folderName = item.Tag as string;
			if(Local.IgnoreFolders.Contains(folderName)) {
				item.IsEnabled = false;
			} else {
				item.IsEnabled = true;
			}
		}
	}
}
