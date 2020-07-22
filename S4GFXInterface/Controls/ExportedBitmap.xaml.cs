using System;
using System.Collections.Generic;
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
	public partial class ExportedBitmap : UserControl
	{
		IGfxImage image;
		private bool removeAlpha;
		private bool onlyShadows;
		private bool removeShadows;

		public ExportedBitmap() {
			InitializeComponent();
		}

		public void SetImage(IGfxImage newImage) {
			image = newImage;
			UpdateGraphic();
		}

		public void UpdateGraphic() {
			Task.Run(() => {
				var s = SaveToBitmap();
				Dispatcher.Invoke((Action)delegate { MainImage.Source = s.ToBitmapSource(); ImageText.Content = $"ID:{image.Index}\n{image.Width}x:{image.Height}px"; });
				});
		}

		private Bitmap SaveToBitmap() {
			BitmapSource i;

			int width = image.Width;
			int height = image.Height;
			DirectBitmap b = new DirectBitmap(image.Width, image.Height);
				ImageData data = image.GetImageData();

				int index = 0;
				for (int y = 0; y < height; y++) {
					for (int x = 0; x < width; x++) {
						int alpha = 255;

						if (index >= data.data.Length)
							break;


						byte red = data.data[index + 0];
						byte green = data.data[index + 1];
						byte blue = data.data[index + 2];

						if (red == 255 && green + blue == 0) {
							alpha = removeAlpha ? 0 : alpha;
						}
						if (green == 255 && red + blue == 0) {
							alpha = removeShadows ? 0 : alpha;
						} else if (onlyShadows) {
							red = 0;
							green = 0;
							blue = 0;
							alpha = 0;
						}

						b.SetPixel(x, y, System.Drawing.Color.FromArgb(alpha, red, green, blue));

						index += 4;
					
				}
			};

			return b.Bitmap;
		}
	}
}
