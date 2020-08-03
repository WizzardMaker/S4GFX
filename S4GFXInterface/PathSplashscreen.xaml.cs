using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace S4GFXInterface {

	/// <summary>
	/// Window to select the "Settler IV" game folder
	/// </summary>
	public partial class PathSplashscreen : Window {
		public bool isDialogue;


		public PathSplashscreen() {
			InitializeComponent();
			Path.Text = Properties.Settings.Default.GamePath;

			if (!isDialogue && CheckPath()) {
				App.GamePath = Path.Text;
				Properties.Settings.Default.GamePath = Path.Text;
				Properties.Settings.Default.Save();

				MainWindow m = new MainWindow();
				m.Show();
				this.Close();
			}
		}
		protected override void OnActivated(EventArgs e) {
			base.OnActivated(e);
			Cancel.Visibility = isDialogue ? Visibility.Visible : Visibility.Collapsed;


		}

		private void FolderDialog_Click(object sender, RoutedEventArgs e) {
			CommonOpenFileDialog dialog = new CommonOpenFileDialog();
			dialog.InitialDirectory = "\\";
			dialog.IsFolderPicker = true;
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
				Path.Text = dialog.FileName;
			}
		}

		private void Select_Click(object sender, RoutedEventArgs e) {
			if(CheckPath() == false) {
				MessageBox.Show("Wrong path provided! Can't find any .gfx or .sfx files in the game folder!", "Wrong path!", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			App.GamePath = Path.Text;
			Properties.Settings.Default.GamePath = Path.Text;
			Properties.Settings.Default.Save();

			if (isDialogue == false) {
				MainWindow m = new MainWindow();
				m.Show();
			} else {
				DialogResult = true;
			}
			this.Close();
		}

		private bool CheckPath() {
			return System.IO.File.Exists(System.IO.Path.Combine(Path.Text, "gfx/0.gfx"));
		}

		private void Path_TextChanged(object sender, TextChangedEventArgs e) {
		}

		private void Cancel_Click(object sender, RoutedEventArgs e) {
			DialogResult = false;
			this.Close();
		}
	}
}
