using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfAutoGrid;
using S4GFXLibrary.GFX;
using S4GFXInterface.Controls;
using System.Windows;

namespace S4GFXInterface
{
	class ImageGrid {
		AutoGrid grid;
		ViewMode currentMode;
		int currentGroup;
		int currentContainer = 0;

		public ImageView selectedItem;

		public event Action<ViewMode> ViewModeChanged;
		public event Action<ImageView> SelectionChanged;

		volatile List<ImageView> images = new List<ImageView>();

		internal ViewMode CurrentMode { get => currentMode; set { currentMode = value; ViewModeChanged?.Invoke(value); } }

		public class ImageView : IDisposable {
			public IGfxImage image;
			DirectBitmap bitmap;
			private ExportedBitmap gridElement;

			private bool removeAlpha = true;
			private bool onlyShadows;
			private bool removeShadows = true;

			ImageData data;

			public int Group => image.GroupIndex;
			public int Id => image.Index;

			public ExportedBitmap GridElement {
				get {
					gridElement = gridElement ?? GetNewGridElement();

					return gridElement;
				}

				set => gridElement = value;
			}

			public ImageView(IGfxImage image) {
				this.image = image;
				this.GridElement = null;

				data = image.GetImageData();
			}

			private void CreateBitmap() {
				int width = image.Width;
				int height = image.Height;
				DirectBitmap b = new DirectBitmap(image.Width, image.Height);
				bitmap = b;

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

				b.Done();
			}

			public void SaveBitmapToFile(string path) {
				if(bitmap == null)
					CreateBitmap();

				bitmap.Bitmap.Save(path + "/" + image.Index + ".png", System.Drawing.Imaging.ImageFormat.Png);
			}

			private ExportedBitmap GetNewGridElement() {
				ExportedBitmap element = new ExportedBitmap();
				try {
					CreateBitmap();

					element.SetImage(image, bitmap.Bitmap);
				} catch (Exception e) {
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
				return element;
			}

			public void Dispose() {
				bitmap?.Dispose();
			}
		}

		public enum ViewMode
		{
			/// <summary>
			/// Displays the images in their groups. Defaults to Image mode when there are no groups
			/// </summary>
			Group,
			/// <summary>
			/// Displays the images
			/// </summary>
			ImageGroup,

			All,
		}

		public ImageGrid(AutoGrid grid) {
			this.grid = grid;
			CurrentMode = ViewMode.ImageGroup;
		}

		public void SetContainer(int id) {
			currentContainer = id;
		}

		public void SetGroupView() {
			CurrentMode = ViewMode.Group;
			currentGroup = -1;

			images = images.OrderBy((item) => item?.Id).ToList();

			var groupItems = from i in images
							 where i != null
							 group i by i.Group into g
							 select g.First();

			UpdateView(groupItems);
		}

		public void UpdateView() {
			switch (CurrentMode) {
				case ViewMode.Group:
				SetGroupView();
				break;
				case ViewMode.ImageGroup:
				EnterImageView(currentGroup);
				break;
				case ViewMode.All:
				ShowAll();
				break;
			}
		}

		private void UpdateView(IEnumerable<ImageView> groupItems) {
			try {
				grid.Children.Clear();

				foreach (var i in groupItems) {
					if (i == null)
						continue;

					i.GridElement.IsGroupView = CurrentMode == ViewMode.Group;

					if (CurrentMode == ViewMode.Group)
						i.GridElement.MouseLeftButtonDown += (ob, args) => { EnterImageView(i.Group); };
					else
						i.GridElement.MouseLeftButtonDown += (ob, args) => { selectedItem = i; SelectionChanged?.Invoke(i); };

					grid.Children.Add(i.GridElement);
					grid.RowCount = images.Count / 3 + 3;
				}
			} catch (Exception e) {
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}

		public void EnterImageView(int group) {
			CurrentMode = ViewMode.ImageGroup;
			currentGroup = group;

			images = images.OrderBy((item) => item?.Id).ToList();
			IEnumerable<ImageView> groupItems = GetImagesInCurrentGroup();

			UpdateView(groupItems);
		}

		public IEnumerable<ImageView> GetImagesInCurrentGroup() {
			return from i in images
				   where i != null
				   where i.Group == currentGroup
				   select i;
		}
		public IEnumerable<ImageView> GetAllImages() {
			return from i in images
				   where i != null
				   select i;
		}

		public void ShowAll() {
			CurrentMode = ViewMode.All;

			images = images.OrderBy((item) => item?.Id).ToList();
			//groupItems.ToList().OrderBy((item) => item.Group);

			UpdateView(images);
		}

		public void AddImage(IGfxImage img) {
			try {
				int startedContainer = currentContainer;
				ImageView v = new ImageView(img);

				if (startedContainer == currentContainer)
					images.Add(v);
			} catch (Exception e) {
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}

		public void Clear() {
			grid.Children.Clear();
			foreach (var i in images) {
				i?.Dispose();
			}
			images.Clear();
		}

	}
}
