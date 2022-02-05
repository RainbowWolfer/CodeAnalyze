using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyze.Models {
	public class Set {
		public string Name { get; set; }
		public List<string> Files { get; set; } = new List<string>();

		public Set(string name) {
			Name = name;
		}
	}
}
