using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using S4GFXLibrary.GFX;

namespace S4GFXInterface.Controls
{
	/// <summary>
	/// Interaktionslogik für ExportedBitmap.xaml
	/// </summary>
	public partial class ExportedBitmap : UserControl, INotifyPropertyChanged {
		IGfxImage image;
		private bool isGroupView;

		public event PropertyChangedEventHandler PropertyChanged;

		public bool IsGroupView {
			get => isGroupView;
			set {
				isGroupView = value;
				UpdateText();
			}
		}

		public ExportedBitmap() {
			InitializeComponent();
			DataContext = this;
		}

		public void SetImage(IGfxImage newImage, Bitmap b) {
			image = newImage;
			UpdateGraphic(b);
		}

		public void UpdateGraphic(Bitmap b) {
			Dispatcher.BeginInvoke((Action)delegate {
				MainImage.Source = b?.ToBitmapSource();
				UpdateText();
			}, System.Windows.Threading.DispatcherPriority.Input);
		}

		private void UpdateText() {
			ImageText.Content = isGroupView ? $"Group ID:{image.GroupIndex}" : $"ID:{image.Index}\n{image.Width}x:{image.Height}px";
		}
	}
}
