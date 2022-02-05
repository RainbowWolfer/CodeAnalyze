using CodeAnalyze.Models;
using CodeAnalyze.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CodeAnalyze.Views {
	public sealed partial class FileTypeInput: UserControl {
		public event Action<string> OnInputChanged;
		public readonly FileTypeInputModel model;
		public readonly ManagePage parent;
		private bool isInputing;
		private CoreCursor cursorBeforePointerEntered = null;

		public Visibility InputVisibility => model.IsAdd ? Visibility.Collapsed : Visibility.Visible;

		public Visibility AddButtonVisibility => !model.IsAdd ? Visibility.Collapsed : Visibility.Visible;

		public bool IsInputing {
			get => isInputing;
			set {
				isInputing = value;
				Block.Visibility = !value ? Visibility.Visible : Visibility.Collapsed;
				Box.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
				if(value) {
					Box.Focus(FocusState.Keyboard);
					Box.Text = model.Input;
				} else {
					string newInput = Box.Text.ToLower().Trim();
					if(newInput != model.Input) {
						model.Input = newInput;
						OnInputChanged?.Invoke(newInput);
					}
					Block.Text = newInput;
					if(string.IsNullOrWhiteSpace(model.Input)) {
						MainGrid.BorderThickness = new Thickness(1.5);
					} else {
						MainGrid.BorderThickness = new Thickness(0);
					}
				}
			}
		}

		public Button AddButtonExternal => model.IsAdd ? AddButton : null;

		public FileTypeInput(ManagePage parent, FileTypeInputModel model, Action<string> onInputChanged = null) {
			this.InitializeComponent();
			this.parent = parent;
			this.model = model;
			Box.Text = model.Input;
			Block.Text = model.Input;
			IsInputing = false;
			OnInputChanged = onInputChanged;
		}

		private void AddButton_Tapped(object sender, TappedRoutedEventArgs e) {
			parent.AddEmptyFileTypeInput();
		}

		private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e) {
			cursorBeforePointerEntered = Window.Current.CoreWindow.PointerCursor;
			Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Hand, 0);
		}

		private void Grid_PointerExited(object sender, PointerRoutedEventArgs e) {
			Window.Current.CoreWindow.PointerCursor = cursorBeforePointerEntered;
		}

		private void Box_LostFocus(object sender, RoutedEventArgs e) {
			IsInputing = false;
		}

		private void Grid_Tapped(object sender, TappedRoutedEventArgs e) {
			if(!IsInputing) {
				IsInputing = true;
			}
		}

		private void Box_PreviewKeyDown(object sender, KeyRoutedEventArgs e) {
			if(e.Key == VirtualKey.Enter) {
				IsInputing = false;
				ManagePage.FileTypeInputAddButton.Focus(FocusState.Keyboard);
			}
		}

		private void MainGrid_RightTapped(object sender, RightTappedRoutedEventArgs e) {
			MenuFlyout flyout = new MenuFlyout() {
				Placement = FlyoutPlacementMode.Bottom,
				ShowMode = FlyoutShowMode.Auto,
			};
			MenuFlyoutItem item_delete = new MenuFlyoutItem() {
				Text = "Delete",
				Icon = new FontIcon() { Glyph = "\uE107" },
			};
			item_delete.Click += async (s, args) => {
				await parent.RemoveFileTypeInput(this);
			};
			flyout.Items.Add(item_delete);
			flyout.ShowAt(sender as Grid);
		}
	}
}
