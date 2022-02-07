using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyze.Models {
	public class LocalSettings {
		public bool SkipEmptyLine { get; set; }

		public static LocalSettings GetDefault() {
			return new LocalSettings() {
				SkipEmptyLine = true,
			};
		}
	}
}
