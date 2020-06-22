using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S4GFXLibrary.GFX
{
	class GfxImage16Bit : IGfxImage {
		public int DataOffset { get; set; }
		public int Height { get; set; }
		public int Width { get; set; }
		public int Flag1 { get; set; }
		public int Flag2 { get; set; }

		public int rowCount;
		public byte[] buffer;

		public GfxImage16Bit(byte[] buffer, int width, int rowCount) {
			this.buffer = buffer;
			this.rowCount = rowCount;
			Width = width;
			Height = rowCount * width;
		}

		public byte[] GetData() {
			throw new NotImplementedException();
		}

		public int GetDataSize() {
			return Width * Height * 2;
		}

		public void GetImageData16Bit(byte[] buffer, byte[] imgData, int pos, int length) {
			int j = 0;

			while(j < length) {
				int value1 = buffer[pos];
				pos++;

				int value2 = buffer[pos];
				pos++;

				imgData[j++] =(byte)( value2 & 0xF8); // r
				imgData[j++] = (byte)((value1 >> 3) | (value2 << 5) & 0xFC); // g
				imgData[j++] = (byte)((value1 << 3) & 0xF8); // b
				imgData[j++] = (byte)(255); // alpha

			}
		}

		public ImageData GetImageData() {
			ImageData img = new ImageData(Height, Width);

			int length = Width * Height * 2;
			int pos = DataOffset;

			byte[] imgData = new byte[length];

			GetImageData16Bit(buffer, imgData, pos, length);

			img.data = imgData;
			//Buffer.BlockCopy(imgData, 0, img.data, 0, length);

			return img;
		}
	}
}
