using CodeAnalyze.Models;
using CodeAnalyze.Services;
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
	public sealed partial class IgnoresManageDialog: UserControl {
		public bool Confirm { get; private set; }
		public IgnoresManageDialog() {
			this.InitializeComponent();
			Initialize();
		}

		private async void Initialize() {
			LoadingRing.IsActive = true;
			await SetsManager.ReadLocal();
			foreach(Set item in SetsManager.SetsList) {
				IgnoresListView.Items.Add(new IgnoresListItem() {
					Title = item.Name,
				});
			}
			LoadingRing.IsActive = false;
		}
	}
}
