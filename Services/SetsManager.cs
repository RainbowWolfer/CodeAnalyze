using CodeAnalyze.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CodeAnalyze.Services {
	public static class SetsManager {
		public static StorageFile File { get; private set; }
		private static StorageFolder LocalFolder => ApplicationData.Current.LocalFolder;
		private const string FILENAME = "IgoresList.json";

		public static List<Set> SetsList { get; private set; }

		static SetsManager() {
			Debug.WriteLine(LocalFolder.Path);
		}

		public async static Task ReadLocal() {
			if(File == null) {
				File = await LocalFolder.CreateFileAsync(FILENAME, CreationCollisionOption.OpenIfExists);
			}
			using(Stream stream = await File.OpenStreamForReadAsync()) {
				using(StreamReader reader = new StreamReader(stream)) {
					SetsList = JsonConvert.DeserializeObject<List<Set>>(await reader.ReadToEndAsync()) ?? GetDefaultList();
				}
			}
		}

		public static bool CheckNameDuplicate(string name) {
			return SetsList.Any(i => i.Name == name);
		}

		public static async Task UpdateSet(Set set) {
			if(set == null) {
				return;
			}
			var found = SetsList.Find(s => s.Name == set.Name);
			if(found != null) {
				found.Files = set.Files;
				await Save();
			}
		}

		public static async Task<bool> Add(Set ignores) {
			if(CheckNameDuplicate(ignores.Name)) {
				return false;
			}
			SetsList.Add(ignores);
			await Save();
			return true;
		}

		public static async Task<int> Remove(string name) {
			int result = SetsList.RemoveAll(i => i.Name == name);
			await Save();
			return result;
		}

		public async static Task Save() {
			await FileIO.WriteTextAsync(File, JsonConvert.SerializeObject(SetsList));
		}


		public static List<Set> GetDefaultList() {
			return new List<Set>() {
				new Set("Visual Studio For UWP"){
					Files = new List<string> {
						".editorconfig", ".gitattributes", ".gitignore", ".csproj", ".user", ".sln", ".pfx", ".appxmanifest", ".xml"
					}
				},
				new Set("Visual Studio For UWP"){
					Files = new List<string> {
						".editorconfig", ".gitattributes", ".gitignore", ".csproj", ".user", ".sln", ".pfx", ".appxmanifest", ".xml"
					}
				},
				new Set("Visual Studio For UWP"){
					Files = new List<string> {
						".editorconfig", ".gitattributes", ".gitignore", ".csproj", ".user", ".sln", ".pfx", ".appxmanifest", ".xml"
					}
				},
				new Set("Visual Studio For UWP"){
					Files = new List<string> {
						".editorconfig", ".gitattributes", ".gitignore", ".csproj", ".user", ".sln", ".pfx", ".appxmanifest", ".xml"
					}
				},
				new Set("Visual Studio For UWP"){
					Files = new List<string> {
						".editorconfig", ".gitattributes", ".gitignore", ".csproj", ".user", ".sln", ".pfx", ".appxmanifest", ".xml"
					}
				},
			};
		}
	}
}
