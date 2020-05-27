using S4GFX.FileReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S4GFX.GFX
{
	class GfxImage : IGfxImage {
		public int DataOffset { get; set; }
		public int Height { get; set; }
		public int Width { get; set; }
		public int Flag1 { get; set; }
		public int Flag2 { get; set; }

		public bool headType;
		public int imgType;

		//x and y offset of the image
		public int left, top;

		Byte[] buffer;

		Palette palette;
		int paletteOffset;

		public GfxImage(Byte[] buffer, Palette palette, int paletteOffset) {
			this.buffer = buffer;
			this.palette = palette;
			this.paletteOffset = paletteOffset;
		}

		public int GetDataSize() {
			return 0;
		}

		void GetImageDataWithRunLengthEncoding(Byte[] buffer, UInt32[] imgData, int pos, int length) {
			int j = 0;

			while (j < length) {
				int value = pos >= buffer.Length ? 0 : buffer[pos];
				pos++;

				UInt32 color;
				int count = 1;

				if (value <= 1) {
					count = pos >= buffer.Length ? 1 : buffer[pos];
					pos++;

					if (value == 0) {
						color = 0xFF0000FF;
					} else {
						color = 0xFF00FF00;
					}
				} else {
					color = palette.GetColor(paletteOffset + value);
				}

				for (int i = 0; (i < count) && (j < length); i++) {
					imgData[j++] = color;
				}
			}
		}

		public void GetImageDataWithNoEncoding(Byte[] buffer, UInt32[] imgData, int pos, int length) {
			int j = 0;
			while (j < length) {
				int value = pos >= buffer.Length ? 1 : buffer[pos];
				pos++;

				imgData[j++] = palette.GetColor(paletteOffset + value);
			}
		}

		//public void WriteImageData() {
		//	ImageData img = new ImageData(Width, Height);

		//	int length = Width * Height * 4;
		//	int pos = DataOffset;

		//	UInt32[] imgData = new UInt32[length];

		//	if (imgType != 32) {
		//		GetImageDataWithRunLengthEncoding(buffer, imgData, pos, length);
		//	} else {
		//		GetImageDataWithNoEncoding(buffer, imgData, pos, length);
		//	}
		//	img.data = new Byte[length];

		//	Buffer.BlockCopy(imgData, 0, img.data, 0, img.data.Length);


		//	return img;
		//}

		public ImageData GetImageData() {
			ImageData img = new ImageData(Width, Height);

			int length = Width * Height * 4;
			int pos = DataOffset;

			UInt32[] imgData = new UInt32[length];

			if (imgType != 32) {
				GetImageDataWithRunLengthEncoding(buffer, imgData, pos, length);
			} else {
				GetImageDataWithNoEncoding(buffer, imgData, pos, length);
			}
			img.data = new Byte[length];

			Buffer.BlockCopy(imgData, 0, img.data, 0, img.data.Length);


			return img;
		}
	}
}
