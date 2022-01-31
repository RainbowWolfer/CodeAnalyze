using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CodeAnalyze.Models {
	public class FolderItem {
		public StorageFolder Folder { get; private set; }
		public List<FolderItem> Children { get; private set; }

		public FolderItem(StorageFolder folder) {
			Folder = folder;
			Children = new List<FolderItem>();
		}

		public FolderItem(StorageFolder folder, IEnumerable<FolderItem> children) {
			Folder = folder;
			Children = new List<FolderItem>(children);
		}
	}
}
