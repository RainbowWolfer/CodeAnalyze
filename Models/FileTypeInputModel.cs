using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyze.Models {
	public class FileTypeInputModel {
		public bool IsAdd { get; private set; }
		public Set Parent { get; private set; }
		public string Input { get; set; }
		public FileTypeInputModel(bool isAdd, Set parent = null, string input = "") {
			this.IsAdd = isAdd;
			this.Parent = parent;
			this.Input = input;
		}
	}
}
