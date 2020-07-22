﻿using S4GFXInterface.Controls;
using S4GFXLibrary.FileReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace S4GFXInterface
{
	/// <summary>
	/// Interaktionslogik für MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		HeaderManager header;
		private static ICollectionFileReader gfxFile;

		ImageGrid exportImageGrid;

		public string PathAddress {
			get {
				return App.GamePath;
			}
		}

		public MainWindow() {
			InitializeComponent();
			Import.Visibility = Visibility.Collapsed;
			Export.Visibility = Visibility.Collapsed;
			Settings.Visibility = Visibility.Collapsed;

			header = new HeaderManager(MainControl);

			header.AddButton(ImportB, Import);
			header.AddButton(ExportB, Export);
			header.AddButton(SettingsB, Settings);

			exportImageGrid = new ImageGrid(ExportGrid);

			GetAllGroups();

			Load(App.GamePath + "/GFX/0");
			SaveAllBitmaps("");
			//ExportGrid.Children.Add(new ExportedBitmap());
		}

		void GetAllGroups() {
			List<string> ids = new List<string>();

			foreach(string f in Directory.GetFiles(App.GamePath+"/GFX/", "*.gfx")) {
				ids.Add(System.IO.Path.GetFileNameWithoutExtension(f));
			}


			ids.Distinct();
			int maxlen = ids.Max(x => x.Length);
			var result = ids.OrderBy(x => x.PadLeft(maxlen, '0')).ToList();
			ExportGroupIDs.ItemsSource = result;
		}

		protected override void OnActivated(EventArgs e) {
			base.OnActivated(e);
			header.RefreshAll();
		}

		private void Button_Click(object sender, RoutedEventArgs e) {

		}

		private void ChangePath_Click(object sender, RoutedEventArgs e) {
			PathSplashscreen s = new PathSplashscreen { isDialogue = true };

			s.ShowDialog();
			//OnPropertyChanged("PathAddress");
		}

		static public ICollectionFileReader Load(string fileId) {

			bool gfx = File.Exists(fileId + ".gfx");
			bool gh = File.Exists(fileId + ".gh6");

			if (gh) {
				return DoLoadGH(fileId);
			}

			if (gfx == false) {
				return null;
			}

			bool pil = File.Exists(fileId + ".pil");
			bool jil = File.Exists(fileId + ".jil");


			return DoLoad(fileId, pil, jil);
		}

		static public ICollectionFileReader DoLoadGH(string fileId) {
			var gh = new BinaryReader(File.Open(fileId + ".gh5", FileMode.Open), Encoding.Default, true);
			//var gl = new BinaryReader(File.Open(fileId + ".gl5", FileMode.Open), Encoding.Default, true);

			gfxFile = new GhFileReader(gh);

			gh.Close();
			//gl.Close();

			return gfxFile;
		}

		static public ICollectionFileReader DoLoad(string fileId, bool usePli, bool useJil) {
			//Console.WriteLine($"Using .jil={useJil}");

			var gfx = new BinaryReader(File.Open(fileId + ".gfx", FileMode.Open), Encoding.Default, true);
			var gil = new BinaryReader(File.Open(fileId + ".gil", FileMode.Open), Encoding.Default, true);

			BinaryReader paletteIndex, palette, directionIndex = null, jobIndex = null;

			if (usePli) {
				paletteIndex = new BinaryReader(File.Open(fileId + ".pil", FileMode.Open), Encoding.Default, true);
				palette = new BinaryReader(File.Open(fileId + ".pa6", FileMode.Open), Encoding.Default, true);
			} else {
				paletteIndex = new BinaryReader(File.Open(fileId + ".pi4", FileMode.Open), Encoding.Default, true);
				palette = new BinaryReader(File.Open(fileId + ".p46", FileMode.Open), Encoding.Default, true);
			}

			if (useJil) {
				directionIndex = new BinaryReader(File.Open(fileId + ".dil", FileMode.Open), Encoding.Default, true);
				jobIndex = new BinaryReader(File.Open(fileId + ".jil", FileMode.Open), Encoding.Default, true);
			}



			var gfxIndexList = new GilFileReader(gil);
			var paletteIndexList = new PilFileReader(paletteIndex);
			var paletteCollection = new PaletteCollection(palette, paletteIndexList);

			DilFileReader directionIndexList = null;
			JilFileReader jobIndexList = null;

			if (useJil) {
				directionIndexList = new DilFileReader(directionIndex);
				jobIndexList = new JilFileReader(jobIndex);
			}

			gfxFile = new GfxFileReader(gfx, gfxIndexList, jobIndexList, directionIndexList, paletteCollection);

			gfx?.Close();
			gil?.Close();
			paletteIndex?.Close();
			palette?.Close();
			directionIndex?.Close();
			jobIndex?.Close();

			return gfxFile;
		}

		CancellationTokenSource source = new CancellationTokenSource();

		private void SaveAllBitmaps(string path, ICollectionFileReader file = null) {
			Console.WriteLine($"Start saving: " + path);
			//Parallel.For(0, gfxFile.GetImageCount(), (i) => { SaveToBitmap(path, i); });
			file = file ?? gfxFile;

			source.Cancel();
			source = new CancellationTokenSource();
			CancellationToken token = source.Token;

			Task.Run(() => {
				int count = file.GetImageCount();
				for (int i = 0; i < count; i++) {
					try {
						token.ThrowIfCancellationRequested();
						exportImageGrid.AddImage(file.GetImage(i));

						Dispatcher.Invoke((Action)delegate { ExportLoadCount.Text = $"Loaded: {i+1}/{count}"; });
						//SaveToBitmap(path, i, file);
					} catch (Exception e) {
						Console.WriteLine(e.Message);
						Console.WriteLine(e.StackTrace);
					}
				}
			}, token);
			

			Console.WriteLine($"Saved: " + path);
		}

		class HeaderManager
		{
			TabControl parentControl;

			public List<HeaderButton> buttons = new List<HeaderButton>();

			public HeaderManager(TabControl parentControl) {
				this.parentControl = parentControl;
			}

			public void RefreshAll() {
				foreach (var but in buttons) {
					but.Refresh();
				}
			}

			public void AddButton(HeaderButton b, object indexObject) {
				b.control = parentControl;
				b.indexItem = indexObject;
				b.OnClick = RefreshAll;

				buttons.Add(b);
			}
		}

		private void ExportGroupIDs_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			exportImageGrid.Clear();
			Load(App.GamePath + "/GFX/" + ExportGroupIDs.SelectedItem);
			SaveAllBitmaps("");
		}
	}
}