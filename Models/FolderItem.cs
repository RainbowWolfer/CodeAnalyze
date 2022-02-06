using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CodeAnalyze.Models {
	public class FolderItem: INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

		public StorageFolder Folder { get; private set; }
		public List<FolderItem> Children { get; private set; }
		public Dictionary<string, long> FilesCount { get; private set; }
		public string Name => Folder?.DisplayName ?? "NULL";

		private bool isSelected;
		public bool IsSelected {
			get => isSelected;
			set {
				if(isSelected == value) {
					return;
				}
				isSelected = value;
				NotifyPropertyChanged("IsSelected");
			}
		}

		private bool isExpanded;
		public bool IsExpanded {
			get => isExpanded;
			set {
				if(isExpanded == value) {
					return;
				}
				isExpanded = value;
				NotifyPropertyChanged("IsExpanded");
			}
		}


		public FolderItem(StorageFolder folder) {
			Folder = folder;
			Children = new List<FolderItem>();
			FilesCount = new Dictionary<string, long>();
		}

		public FolderItem(StorageFolder folder, IEnumerable<FolderItem> children) {
			Folder = folder;
			Children = new List<FolderItem>(children);
			FilesCount = new Dictionary<string, long>();
		}

		private void NotifyPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public override string ToString() {
			return $"{Folder.Path} ({Children.Count})";
		}
	}
}
