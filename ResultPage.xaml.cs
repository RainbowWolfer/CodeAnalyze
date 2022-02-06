﻿using CodeAnalyze.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CodeAnalyze {
	public sealed partial class ResultPage: Page {
		private readonly List<FolderInfo> allFolders = new List<FolderInfo>();
		private readonly List<FileTypeInfo> infos = new List<FileTypeInfo>();
		private readonly Dictionary<string, LineOfCodeInfo> lineOfCodeByTypes = new Dictionary<string, LineOfCodeInfo>();

		public ObservableCollection<LineOfCodeListViewItem> Items { get; } = new ObservableCollection<LineOfCodeListViewItem>();

		public ResultPage() {
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Required;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			if(e.Parameter is AnalyzePamameters pamameters) {
				SetName.Text = $"Set: {pamameters.Set.Name}";
				LoadingPanel.Visibility = Visibility.Visible;
				await Analyze(pamameters);
				LoadingPanel.Visibility = Visibility.Collapsed;
			}
		}

		private async Task Analyze(AnalyzePamameters pamameters) {
			Items.Clear();
			allFolders.Clear();
			infos.Clear();
			lineOfCodeByTypes.Clear();
			ReadAllItems(pamameters.Root, pamameters.Folders);
			LoadingText.Text = "Start Loading";
			foreach(FolderInfo folder in allFolders) {
				foreach(StorageFile file in await folder.Folder.GetFilesAsync()) {
					string name = null;
					if(pamameters.Set.Files.Any(i => {
						if(file.Name.EndsWith(i)) {
							name = i;
							return true;
						} else {
							return false;
						}
					})) {
						LoadingText.Text = $"Reading Lines of {file.Path}";
						IList<string> lines = await FileIO.ReadLinesAsync(file);
						infos.Add(new FileTypeInfo(name, file, folder.Folder, lines.Count));
						folder.SubmitFile(name, lines.Count);
					} else {
						continue;
					}
				}
			}
			FilesCountText.Text = $"Files Scanned: {infos.Count}";

			LoadingText.Text = $"Calculating {infos.Count} Files";
			foreach(FileTypeInfo item in infos) {
				LineOfCodeInfo info;
				if(lineOfCodeByTypes.ContainsKey(item.Name)) {
					info = lineOfCodeByTypes[item.Name];
					info.Lines += item.Lines;
				} else {
					info = new LineOfCodeInfo(item.Name, item.Lines);
					lineOfCodeByTypes.Add(item.Name, info);
				}
				if(info.MostFileLine < item.Lines) {
					info.MostFileLine = item.Lines;
					info.MostFile = item.File;
				}
			}

			LoadingText.Text = $"Finalizing";

			LineOfCodeInfo[] array = lineOfCodeByTypes.Select(s => s.Value).ToArray();
			long lines_sum = lineOfCodeByTypes.Select(s => s.Value.Lines).Sum();
			for(int i = 0; i < array.Length; i++) {
				LineOfCodeInfo item = array[i];
				Items.Add(new LineOfCodeListViewItem() {
					Index = i,
					Name = item.Name,
					LineOfCode = item.Lines,
					MostFile = $"{item.MostFile.Name} - {item.MostFileLine}",
					MostFolder = MostFolderString(item.Name),
					Percentage = (double)item.Lines / lines_sum,
				});
			}
		}

		private void ReadAllItems(FolderItem root, List<FolderItem> folders) {
			foreach(FolderItem item in folders) {
				allFolders.Add(new FolderInfo(item.Folder));
				if(item != root && item.Children != null && item.Children.Count != 0) {
					ReadAllItems(root, item.Children);
				}
			}
		}

		private string MostFolderString(string name) {
			var folder = FindMostFolder(name, out long lines);
			return $"\\{folder.Name}\\ - {lines}";
		}

		private StorageFolder FindMostFolder(string name, out long lines) {
			StorageFolder most = null;
			lines = 0;
			foreach(FolderInfo item in allFolders) {
				if(item.LineOfCodeByTypes.ContainsKey(name)) {
					long l = item.LineOfCodeByTypes[name];
					if(l > lines) {
						lines = l;
						most = item.Folder;
					}
				}
			}
			return most;
		}
	}

	public class FolderInfo {
		public StorageFolder Folder { get; private set; }
		public Dictionary<string, long> LineOfCodeByTypes { get; } = new Dictionary<string, long>();

		public FolderInfo(StorageFolder folder) {
			Folder = folder;
		}

		public void SubmitFile(string name, long lines) {
			if(LineOfCodeByTypes.ContainsKey(name)) {
				LineOfCodeByTypes[name] += lines;
			} else {
				LineOfCodeByTypes.Add(name, lines);
			}
		}

		public override string ToString() {
			string result = $"{Folder.Name}: ";
			foreach(KeyValuePair<string, long> item in LineOfCodeByTypes) {
				result += $"({item.Key} - {item.Value}) ";
			}
			return result;
		}
	}

	public class LineOfCodeListViewItem {
		public int Index { get; set; }// 1
		public int IndexDisplay => Index + 1;
		public string Name { get; set; }// .cs
		public double Percentage { get; set; }
		public string PercentageString => $"{(int)(Percentage * 100)}%";
		public long LineOfCode { get; set; }
		public string MostFile { get; set; }// Test.cs - 820
		public string MostFolder { get; set; }// /Views/ - 200
	}

	public class LineOfCodeInfo {
		public string Name { get; private set; }
		public long Lines { get; set; }

		public StorageFile MostFile { get; set; } = null;
		public long MostFileLine { get; set; } = 0;

		public LineOfCodeInfo(string name, long count) {
			Name = name;
			Lines = count;
		}
	}

	public class FileTypeInfo {
		public string Name { get; private set; }
		public StorageFile File { get; private set; }
		public StorageFolder Folder { get; private set; }
		public long Lines { get; private set; }
		public FileTypeInfo(string name, StorageFile file, StorageFolder folder, long lines) {
			Name = name;
			File = file;
			Folder = folder;
			Lines = lines;
		}

		public override string ToString() {
			return $"({Name} - {Lines} - [{Folder.Name}/{File.Name}])";
		}
	}


	public class AnalyzePamameters {
		public FolderItem Root { get; private set; }
		public Set Set { get; private set; }
		public List<FolderItem> Folders { get; private set; }
		public AnalyzePamameters(FolderItem root, Set set, List<FolderItem> folders) {
			Root = root;
			Set = set;
			Folders = folders;
		}
	}
}
