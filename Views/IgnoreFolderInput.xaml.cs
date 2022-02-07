using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CodeAnalyze.Views {
	public sealed partial class IgnoreFolderInput: UserControl {
		public event Action<string> OnInputChanged;
		private bool isInputing;
		private readonly SettingsPage parent;
		private CoreCursor cursorBeforePointerEntered = null;

		public bool IsAdd { get; set; }
		public string Input { get; set; }
		public int Index { get; private set; }

		public Visibility InputVisibility => IsAdd ? Visibility.Collapsed : Visibility.Visible;

		public Visibility AddButtonVisibility => !IsAdd ? Visibility.Collapsed : Visibility.Visible;

		public bool IsInputing {
			get => isInputing;
			set {
				isInputing = value;
				Block.Visibility = !value ? Visibility.Visible : Visibility.Collapsed;
				Box.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
				if(value) {
					Box.Focus(FocusState.Keyboard);
					Box.Text = Input;
					Box.SelectionStart = Input.Length;
				} else {
					string newInput = Box.Text.ToLower().Trim();
					if(newInput != Input) {
						Input = newInput;
						OnInputChanged?.Invoke(newInput);
					}
					Block.Text = newInput;
					if(string.IsNullOrWhiteSpace(Input)) {
						MainGrid.BorderThickness = new Thickness(1.5);
					} else {
						MainGrid.BorderThickness = new Thickness(0);
					}
				}
			}
		}

		public IgnoreFolderInput(SettingsPage parent, int index, bool isAdd, string text = "", Action<string> onInputChanged = null) {
			this.InitializeComponent();
			this.parent = parent;
			this.IsAdd = isAdd;
			this.Index = index;
			this.Input = text;
			Box.Text = text;
			Block.Text = text;
			IsInputing = false;
			OnInputChanged = onInputChanged;
			if(isAdd) {
				parent.IgnoreFoldersAddButton = AddButton;
			}
		}

		private void MainGrid_PointerEntered(object sender, PointerRoutedEventArgs e) {
			cursorBeforePointerEntered = Window.Current.CoreWindow.PointerCursor;
			Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Hand, 0);
			FolderIcon.Glyph = "\uED44";
		}

		private void MainGrid_PointerExited(object sender, PointerRoutedEventArgs e) {
			Window.Current.CoreWindow.PointerCursor = cursorBeforePointerEntered;
			FolderIcon.Glyph = "\uED43";
		}

		private void MainGrid_Tapped(object sender, TappedRoutedEventArgs e) {
			if(!IsInputing) {
				IsInputing = true;
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
				await parent.RemoveIgnoreFolder(this);
			};
			flyout.Items.Add(item_delete);
			flyout.ShowAt(sender as Grid);
		}

		private void Box_LostFocus(object sender, RoutedEventArgs e) {
			IsInputing = false;
		}

		private void Box_PreviewKeyDown(object sender, KeyRoutedEventArgs e) {
			if(e.Key == VirtualKey.Enter) {
				IsInputing = false;
				parent.IgnoreFoldersAddButton?.Focus(FocusState.Keyboard);
			}
		}

		private void AddButton_Tapped(object sender, TappedRoutedEventArgs e) {
			parent.AddIgnoreFolder();
		}
	}
}
