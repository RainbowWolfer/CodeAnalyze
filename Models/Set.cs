using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyze.Models {
	public class Set: INotifyPropertyChanged {
		private string name;

		public string Name {
			get => name;
			set {
				if(name == value) {
					return;
				}
				name = value;
				NotifyPropertyChanged();
			}
		}
		public List<string> Files { get; set; } = new List<string>();

		public Set(string name) {
			Name = name;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public override string ToString() {
			return $"Set: {Name} - {Files.Count}";
		}
	}
}
