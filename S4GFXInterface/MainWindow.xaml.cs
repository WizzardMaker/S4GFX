using Microsoft.WindowsAPICodePack.Dialogs;
using S4GFXInterface.Controls;
using S4GFXLibrary.FileReader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		HeaderManager header;
		private static ICollectionFileReader gfxFile;

		ImageGrid exportImageGrid;

		bool isNotLoading;
		public bool IsNotLoading {
			get {
				return isNotLoading;
			}
			set {
				isNotLoading = value;
				RaisePropertyChanged(nameof(IsNotLoading));
			}
		}

		public bool containerHasGroups;

		bool _canExportAll, _canExportGroup, _canExportSingle;
		public bool CanExport {
			set {
				if (value == false) {
					CanExportAll = false;
					CanExportGroup = false;
					CanExportSingle = false;
				}
			}
			get {
				return CanExportAll || CanExportGroup || CanExportSingle;
			}
		}

		public bool CanExportAll {
			get {
				return _canExportAll;
			}
			set {
				_canExportAll = value;
				RaisePropertyChanged(nameof(CanExportAll));
			}
		}
		public bool CanExportGroup {
			get {
				return _canExportGroup;
			}
			set {
				_canExportGroup = value;
				RaisePropertyChanged(nameof(CanExportGroup));
			}
		}
		public bool CanExportSingle {
			get {
				return _canExportSingle;
			}
			set {
				_canExportSingle = value;
				RaisePropertyChanged(nameof(CanExportSingle));
			}
		}

		private int loadingIconCounter = 1;

		public string PathAddress {
			get {
				return App.GamePath;
			}
		}

		public bool ExportToContainer {
			get {
				return Properties.Settings.Default.ExportToContainer;
			}

			set {
				Properties.Settings.Default.ExportToContainer = value;
				Properties.Settings.Default.Save();
				RaisePropertyChanged(nameof(ExportToContainer));
			}
		}
		public bool ExportToGroup {
			get {
				return Properties.Settings.Default.ExportToGroup;
			}

			set {
				Properties.Settings.Default.ExportToGroup = value;
				Properties.Settings.Default.Save();
				RaisePropertyChanged(nameof(ExportToGroup));
			}
		}

		private string exportPathAddress;
		public string ExportPathAddress {
			get {
				return exportPathAddress;
			}
			set {
				exportPathAddress = value;
			}
		}

		public MainWindow() {
			InitializeComponent();

			ExportPathAddress = Properties.Settings.Default.ExportPath;

			DataContext = this;

			Import.Visibility = Visibility.Collapsed;
			Export.Visibility = Visibility.Collapsed;
			Settings.Visibility = Visibility.Collapsed;

			header = new HeaderManager(MainControl);

			header.AddButton(ImportB, Import);
			header.AddButton(ExportB, Export);
			header.AddButton(SettingsB, Settings);

			exportImageGrid = new ImageGrid(ExportGrid);
			exportImageGrid.SetGroupView();

			GetAllGroups();

			exportImageGrid.ViewModeChanged += (mode) => {
				switch (mode) {
					case ImageGrid.ViewMode.Group:
					ExportAllView();
					break;
					case ImageGrid.ViewMode.ImageGroup:
					ExportImageGroupView();
					break;
					case ImageGrid.ViewMode.All:
					ExportAllView();
					break;
				}

			};
			exportImageGrid.ViewModeChanged += CacheExportScrollPosition;
			exportImageGrid.SelectionChanged += ExportSelectionChanged;

			CanExport = false;
			IsNotLoading = true;
			ReturnToGroupBut.Visibility = Visibility.Collapsed;

			LoadingIcon.Visibility = Visibility.Hidden;


			ImageGrid.ImageView.removeAlpha = TransparencyCheck.IsChecked == false;
			ImageGrid.ImageView.removeShadows = ShadowCheck.IsChecked == false;

			//ExportGroupIDs.SelectedIndex = 0;
			//ExportGrid.Children.Add(new ExportedBitmap());
		}

		private void ExportSelectionChanged(ImageGrid.ImageView view) {
			CanExportSingle = true;
		}

		//INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		protected void RaisePropertyChanged(string name) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		double exportScrollPosition = 0.0d;
		private void CacheExportScrollPosition(ImageGrid.ViewMode mode) {
			if (mode == ImageGrid.ViewMode.Group) {
				ExportScrollView.ScrollToVerticalOffset(exportScrollPosition);
			} else {
				exportScrollPosition = ExportScrollView.VerticalOffset;
				ExportScrollView.ScrollToTop();
			}

		}

		private void ExportImageGroupView() {
			ReturnToGroupBut.Visibility = Visibility.Visible;
			CanExportGroup = true;
			CanExportAll = false;
			CanExportSingle = false;
		}

		private void ExportAllView() {
			ReturnToGroupBut.Visibility = Visibility.Collapsed;
			CanExportGroup = false;
			CanExportAll = true;
			CanExportSingle = false;
		}

		void GetAllGroups() {
			List<string> ids = new List<string>();

			foreach (string f in Directory.GetFiles(App.GamePath + "/GFX/").Where(f => f.EndsWith(".gfx") || f.EndsWith(".gh5"))) {
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

		private void ChangeExportPath_Click(object sender, RoutedEventArgs e) {
			CommonOpenFileDialog dialog = new CommonOpenFileDialog();
			dialog.InitialDirectory = "\\";
			dialog.IsFolderPicker = true;
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
				ExportPathAddress = dialog.FileName;
				RaisePropertyChanged("ExportPathAddress");
				Properties.Settings.Default.ExportPath = ExportPathAddress;
				Properties.Settings.Default.Save();
			}
		}

		private void ChangePath_Click(object sender, RoutedEventArgs e) {
			PathSplashscreen s = new PathSplashscreen { isDialogue = true };

			s.ShowDialog();
			//OnPropertyChanged("PathAddress");
		}

		public ICollectionFileReader Load(string fileId) {

			bool gfx = File.Exists(fileId + ".gfx");
			bool gh = File.Exists(fileId + ".gh6");


			gfxFile?.Close();
			if (gh) {
				return DoLoadGH(fileId);
			}

			if (gfx == false) {
				return null;
			}

			bool pil = File.Exists(fileId + ".pil");
			bool jil = File.Exists(fileId + ".jil");

			containerHasGroups = jil;

			if (jil) {
				exportImageGrid.SetGroupView();
			} else {
				exportImageGrid.ShowAll();
			}


			return DoLoad(fileId, pil, jil);
		}

		public ICollectionFileReader DoLoadGH(string fileId) {
			var gh = new BinaryReader(File.Open(fileId + ".gh5", FileMode.Open), Encoding.Default, true);
			//var gl = new BinaryReader(File.Open(fileId + ".gl5", FileMode.Open), Encoding.Default, true);

			gfxFile = new GhFileReader(gh, true);

			gh.Close();
			//gl.Close();

			return gfxFile;
		}

		public ICollectionFileReader DoLoad(string fileId, bool usePli, bool useJil) {
			//Console.WriteLine($"Using .jil={useJil}");

			var gfx = new BinaryReader(File.Open(fileId + ".gfx", FileMode.Open), Encoding.Default, true);
			var gil = new BinaryReader(File.Open(fileId + ".gil", FileMode.Open), Encoding.Default, true);

			BinaryReader paletteIndex, palette, directionIndex = null, jobIndex = null;

			if (usePli) {
				paletteIndex = new BinaryReader(File.Open(fileId + ".pil", FileMode.Open), Encoding.Default, true);
				palette = new BinaryReader(File.Open(fileId + ".pa6", FileMode.Open), Encoding.Default, true);
			} else {
				paletteIndex = new BinaryReader(File.Open(fileId + ".pi2", FileMode.Open), Encoding.Default, true);
				palette = new BinaryReader(File.Open(fileId + ".p26", FileMode.Open), Encoding.Default, true);
			}

			if (useJil) {
				directionIndex = new BinaryReader(File.Open(fileId + ".dil", FileMode.Open), Encoding.Default, true);
				jobIndex = new BinaryReader(File.Open(fileId + ".jil", FileMode.Open), Encoding.Default, true);
			}



			var gfxIndexList = new GilFileReader(gil);
			var paletteIndexList = new PilFileReader(paletteIndex);
			var paletteCollection = new PaletteCollection(palette, paletteIndexList);

			switch (fileId.Substring(fileId.Length-2)) {
				case ("20"):
				case ("21"):
				case ("22"):
				case ("24"):
					paletteCollection.GetPalette().replaceTeamColors = true;
					paletteCollection.GetPalette().team = TeamGFXSetting.SelectedIndex;
				break;
			}

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
			gfxFile?.Close();

			return gfxFile;
		}

		CancellationTokenSource source = new CancellationTokenSource();

		private void SaveAllBitmaps(string path, ICollectionFileReader file = null) {
			Console.WriteLine($"Start saving: " + path);
			//Parallel.For(0, gfxFile.GetImageCount(), (i) => { SaveToBitmap(path, i); });
			file = file ?? gfxFile;

			IsNotLoading = false;
			CanExport = false;

			source.Cancel();
			source = new CancellationTokenSource();
			CancellationToken token = source.Token;

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Task.Run(() => {
				int loaded = 1;
				int count = file.GetImageCount();

				Parallel.For(0, count, (i, state) => {
					//for (int i = 0; i < count; i++) {
					if (token.IsCancellationRequested) {
						state.Break();
					}

					try {
						exportImageGrid.AddImage(file.GetImage(i));

						if (token.IsCancellationRequested) {
							state.Break();
						}

						Dispatcher.Invoke((Action)delegate {
							try {
								ExportLoadCount.Text = $"Loaded: {loaded++}/{count}";
								ExportLoadCount.Visibility = Visibility.Visible;
								LoadingIcon.Visibility = Visibility.Visible;
							} catch (Exception e) {
								Console.WriteLine(e.Message);
								Console.WriteLine(e.StackTrace);
							}
						});

						//SaveToBitmap(path, i, file);
					} catch (Exception e) {
						Console.WriteLine(e.Message);
						Console.WriteLine(e.StackTrace);
					}
				});//}

				Dispatcher.Invoke((Action)delegate {
					exportImageGrid.UpdateView();
				});

				Dispatcher.Invoke((Action)delegate {
					LoadingIcon.Visibility = Visibility.Hidden;
				});

				CanExportAll = true;
				IsNotLoading = true;
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
			LoadGroup();
		}

		private void LoadGroup() {
			if (ExportScrollView == null)
				return;
			ExportScrollView.ScrollToTop();

			exportImageGrid?.Clear();
			source.Cancel();

			exportImageGrid.SetContainer(int.Parse((string)ExportGroupIDs.SelectedItem));
			Load(App.GamePath + "/GFX/" + ExportGroupIDs.SelectedItem);

			SaveAllBitmaps("");
		}

		private void UpdateLoadingSpinner() {
			string filename = "Resources/Icons/icons8-ladeanimation-bild-" + loadingIconCounter + "-50.png";
			BitmapImage image = new BitmapImage();
			image.BeginInit();
			image.UriSource = new Uri(filename, UriKind.Relative);
			image.EndInit();
			LoadingIcon.Source = image;
			loadingIconCounter++;
			if (loadingIconCounter > 8) {
				loadingIconCounter = 1;    // Display first image after the last image
			}
		}

		public string GetFullPath(int group = -1) {
			string basePath = ExportPathAddress;

			if (Properties.Settings.Default.ExportToContainer) {
				basePath += $"/{ExportGroupIDs.SelectedItem}";

				if (!Directory.Exists(basePath)) {
					Directory.CreateDirectory(basePath);
				}
			}

			if (Properties.Settings.Default.ExportToGroup && containerHasGroups) {
				basePath += $"/{group}";
				if (!Directory.Exists(basePath)) {
					Directory.CreateDirectory(basePath);
				}
			}

			return basePath;
		}

		private void LoadingAnim_Completed(object sender, EventArgs e) {
			UpdateLoadingSpinner();
			loadingAnim.BeginAnimation(Image.WidthProperty, loadingAnim);
		}

		private void ReturnToGroupBut_Click(object sender, RoutedEventArgs e) {
			exportImageGrid.SetGroupView();
		}

		private void OpenPath_Click(object sender, RoutedEventArgs e) {
			Process.Start(PathAddress);
		}

		private void OpenExportPath_Click(object sender, RoutedEventArgs e) {
			Process.Start(ExportPathAddress);
		}

		private void ExportSaveToDiskSingle_Click(object sender, RoutedEventArgs e) {
			exportImageGrid.selectedItem.SaveBitmapToFile(GetFullPath(exportImageGrid.selectedItem.Group));
		}

		private void ReloadBut_Click(object sender, RoutedEventArgs e) {
			LoadGroup();
		}

		private void TransparencyCheck_Checked(object sender, RoutedEventArgs e) {
			ImageGrid.ImageView.removeAlpha = TransparencyCheck.IsChecked == false;
		}

		private void ShadowCheck_Checked(object sender, RoutedEventArgs e) {
			ImageGrid.ImageView.removeShadows = ShadowCheck.IsChecked == false;
		}

		private void RemoveGarbageCheck_Checked(object sender, RoutedEventArgs e) {
			ImageGrid.ImageView.removeGarbage = ShadowCheck.IsChecked == false;
		}

		private void ExportSaveToDiskGroup_Click(object sender, RoutedEventArgs e) {
			foreach (var i in exportImageGrid.GetImagesInCurrentGroup()) {
				i.SaveBitmapToFile(GetFullPath(i.Group));
			}
		}

		private void ExportSaveToDiskAll_Click(object sender, RoutedEventArgs e) {
			foreach (var i in exportImageGrid.GetAllImages()) {
				i.SaveBitmapToFile(GetFullPath(i.Group));
			}
		}
	}
}
