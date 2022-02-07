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
	public static class Local {
		public static StorageFolder LocalFolder => ApplicationData.Current.LocalFolder;

		public static StorageFile File_Set { get; private set; }
		public static StorageFile File_IgnoreFolders { get; private set; }
		public static StorageFile File_Settings { get; private set; }

		private const string FILENAME_SETS = "SetsList.json";
		private const string FILENAME_IGNORES = "IgnoreFolders.json";
		private const string FILENAME_SETTINGS = "Settings.json";

		public static List<Set> SetsList { get; private set; }
		public static List<string> IgnoreFolders { get; private set; }
		public static LocalSettings LocalSettings { get; private set; }

		public async static void Initialize() {
			await ReadLocalSettings();
			await ReadIgnoreFolders();
			await ReadSets();
		}

		public async static Task ReadLocalSettings() {
			if(File_Settings == null) {
				File_Settings = await LocalFolder.CreateFileAsync(FILENAME_SETTINGS, CreationCollisionOption.OpenIfExists);
			}
			using(Stream stream = await File_Settings.OpenStreamForReadAsync()) {
				using(StreamReader reader = new StreamReader(stream)) {
					LocalSettings = JsonConvert.DeserializeObject<LocalSettings>(await reader.ReadToEndAsync()) ?? LocalSettings.GetDefault();
				}
			}
		}

		public async static Task ReadIgnoreFolders() {
			if(File_IgnoreFolders == null) {
				File_IgnoreFolders = await LocalFolder.CreateFileAsync(FILENAME_IGNORES, CreationCollisionOption.OpenIfExists);
			}
			using(Stream stream = await File_IgnoreFolders.OpenStreamForReadAsync()) {
				using(StreamReader reader = new StreamReader(stream)) {
					IgnoreFolders = JsonConvert.DeserializeObject<List<string>>(await reader.ReadToEndAsync()) ?? GetDefaultIgnoreFoldersList();
				}
			}
		}

		public async static Task ReadSets() {
			if(File_Set == null) {
				File_Set = await LocalFolder.CreateFileAsync(FILENAME_SETS, CreationCollisionOption.OpenIfExists);
			}
			using(Stream stream = await File_Set.OpenStreamForReadAsync()) {
				using(StreamReader reader = new StreamReader(stream)) {
					SetsList = JsonConvert.DeserializeObject<List<Set>>(await reader.ReadToEndAsync()) ?? GetDefaultSetList();
				}
			}
		}

		public static bool CheckIgnoreFolderDuplicate(string name) {
			return IgnoreFolders.Any(i => i == name);
		}

		public static bool CheckSetNameDuplicate(string name) {
			return SetsList.Any(i => i.Name == name);
		}

		public static async Task UpdateIgnoreFolders(string[] folders) {
			IgnoreFolders = folders.ToList();
			await SaveIgnoreFolders();
		}

		public static async Task UpdateSet(Set set) {
			if(set == null) {
				return;
			}
			var found = SetsList.Find(s => s.Name == set.Name);
			if(found != null) {
				found.Files = set.Files;
				await SaveSets();
			}
		}

		public static async Task<bool> AddSet(Set ignores) {
			if(CheckSetNameDuplicate(ignores.Name)) {
				return false;
			}
			SetsList.Add(ignores);
			await SaveSets();
			return true;
		}

		public static async Task<int> RemoveSet(string name) {
			int result = SetsList.RemoveAll(i => i.Name == name);
			await SaveSets();
			return result;
		}

		public async static Task SaveLocalSettings() {
			await FileIO.WriteTextAsync(File_Settings, JsonConvert.SerializeObject(LocalSettings));
		}

		public async static Task SaveIgnoreFolders() {
			await FileIO.WriteTextAsync(File_IgnoreFolders, JsonConvert.SerializeObject(IgnoreFolders));
		}

		public async static Task SaveSets() {
			await FileIO.WriteTextAsync(File_Set, JsonConvert.SerializeObject(SetsList));
		}

		public static List<string> GetDefaultIgnoreFoldersList() {
			return new List<string>() {
				"node_modules", "bin", "obj",
			};
		}

		public static List<Set> GetDefaultSetList() {
			return new List<Set>() {
				new Set("C#"){
					Files = new List<string> {
						".cs", ".xaml"
					}
				},
				new Set("Java"){
					Files = new List<string> {
						".java", ".xml"
					}
				},
				new Set("TypeScript"){
					Files = new List<string> {
						".ts", ".tsx", ".js", ".xml"
					}
				},
				new Set("C++"){
					Files = new List<string> {
						".cpp",
					}
				},
			};
		}
	}
}
