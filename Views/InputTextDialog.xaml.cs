using System;
using System.Collections.Generic;
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

namespace CodeAnalyze.Views {
	public sealed partial class InputTextDialog: UserControl {
		private readonly ContentDialog dialog;
		private readonly string[] exsitingNames;
		private readonly string originName;
		public bool Accept { get; private set; }

		public string InputText {
			get => InputBox.Text;
			set {
				InputBox.Text = value;
			}
		}

		public InputTextDialog(ContentDialog dialog, IEnumerable<string> exsitingNames, string originName = "") {
			this.InitializeComponent();
			this.dialog = dialog;
			this.exsitingNames = exsitingNames?.ToArray() ?? Array.Empty<string>();
			this.originName = originName;
			if(!string.IsNullOrWhiteSpace(originName)) {
				InputText = originName;
				InputBox.SelectionLength = originName.Length;
			}
		}

		private void AcceptButton_Tapped(object sender, TappedRoutedEventArgs e) {
			Accept = true;
			dialog.Hide();
		}

		private void BackButton_Tapped(object sender, TappedRoutedEventArgs e) {
			Accept = false;
			dialog.Hide();
		}

		private void InputBox_TextChanged(object sender, TextChangedEventArgs e) {
			if(string.IsNullOrWhiteSpace(InputText)) {
				AcceptButton.IsEnabled = false;
				HintPanel.Visibility = Visibility.Visible;
				HintText.Text = "Input Cannot Be Empty";
			} else if(InputText == originName) {
				AcceptButton.IsEnabled = false;
				HintPanel.Visibility = Visibility.Visible;
				HintText.Text = "Cannot Be The Same As Before";
			} else if(exsitingNames.Contains(InputText)) {
				AcceptButton.IsEnabled = false;
				HintPanel.Visibility = Visibility.Visible;
				HintText.Text = $"({InputText}) Already Existed";
			} else {
				AcceptButton.IsEnabled = true;
				HintPanel.Visibility = Visibility.Collapsed;
			}
		}
	}
}
