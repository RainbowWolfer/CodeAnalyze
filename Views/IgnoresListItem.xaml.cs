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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace CodeAnalyze.Views {
	public sealed partial class IgnoresListItem: UserControl {
		private string title;

		public string Title {
			get => title;
			set {
				title = value;
				TitleText.Text = value;
			}
		}

		public IgnoresListItem() {
			this.InitializeComponent();
		}

		private void DeleteButton_Tapped(object sender, TappedRoutedEventArgs e) {
			SwitchAnimation.Children[0].SetValue(DoubleAnimation.FromProperty, DeleteButton.ActualWidth);
			SwitchAnimation.Children[0].SetValue(DoubleAnimation.ToProperty, 0);

			SwitchAnimation.Children[1].SetValue(DoubleAnimation.FromProperty, AcceptButton.ActualWidth);
			SwitchAnimation.Children[1].SetValue(DoubleAnimation.ToProperty, 40);

			SwitchAnimation.Children[2].SetValue(DoubleAnimation.FromProperty, CancelButton.ActualWidth);
			SwitchAnimation.Children[2].SetValue(DoubleAnimation.ToProperty, 40);

			SwitchAnimation.Begin();
		}

		private void AcceptButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}

		private void CancelButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}
	}
}
