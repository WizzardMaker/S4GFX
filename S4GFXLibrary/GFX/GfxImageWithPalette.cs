using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S4GFXLibrary.GFX
{
	class GfxImageWithPalette : IGfxImage
	{
		public int DataOffset { get; set; }
		public int Height { get; set; }
		public int Width { get; set; }
		public int Flag1 { get; set; }
		public int Flag2 { get; set; }

		public int Index { get; set; }

		public int GroupIndex { get; set; }

		public int rowCount;
		public byte[] buffer;

		Palette p = new Palette();
		private int chunkHeight;

		public GfxImageWithPalette(byte[] buffer, int width, int chunkCount) {
			this.buffer = buffer;
			chunkHeight = width;
			Width = width;
			Height = width * chunkCount;
		}

		public byte[] GetData() {
			throw new NotImplementedException();
		}

		public int GetDataSize() {
			return Width * Height * 2;
		}

		public void GetImageDataWithPalette(byte[] buffer, UInt32[] imgData, int pos, int length) {
			int j = 0;

			int chunklength = chunkHeight * Width;
			int c = 0;

			pos -= 256 * 3;

			while (j < length) {
				if (c <= 0) {
					pos += 256 * 3;

					p.Read3BytePalette(buffer, pos + chunklength);
					c = chunklength;
				}
				c--;

				int index = buffer[pos++];

				imgData[j++] = p.GetColor(index);
			}
		}

		public ImageData GetImageData() {
			ImageData img = new ImageData(Height, Width);

			int length = Width * Height;
			int pos = DataOffset;

			UInt32[] imgData = new UInt32[length];

			GetImageDataWithPalette(buffer, imgData, pos, length);

			img.data = new byte[length * 4];
			Buffer.BlockCopy(imgData, 0, img.data, 0, length * 4);

			return img;
		}
	}
}
