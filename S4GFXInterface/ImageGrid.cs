using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfAutoGrid;
using S4GFXLibrary.GFX;
using S4GFXInterface.Controls;

namespace S4GFXInterface
{
	class ImageGrid {
		AutoGrid grid;

		List<IGfxImage> images = new List<IGfxImage>();

		public ImageGrid(AutoGrid grid) {
			this.grid = grid;
		}

		public void AddImage(IGfxImage img) {
			images.Add(img);
			ExportedBitmap element = null;
			grid.Dispatcher.Invoke((Action) delegate {
				element = new ExportedBitmap();
				grid.Children.Add(element);
				grid.RowCount = images.Count / 3 + 3;
			},System.Windows.Threading.DispatcherPriority.ApplicationIdle);

			element.SetImage(img);
		}

		public void Clear() {
			images.Clear();
			grid.Children.Clear();
		}
	}
}
