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

		public int Index { get; set; }

		public int GroupIndex { get; set; }

		public int rowCount;

		bool convertTo24Bit;

		public byte[] buffer;

		public GfxImage16Bit(byte[] buffer, int width, int rowCount, bool convertTo24Bit = true) {
			this.buffer = buffer;
			this.rowCount = rowCount;
			this.convertTo24Bit = convertTo24Bit;
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

			while (j < length) {
				int value1 = buffer[pos];
				pos++;

				int value2 = buffer[pos];
				pos++;

				int RGB565 = value2 << 8 | value1;

				int byte_r = (RGB565 & 0xF800) >> 11;
				int byte_g = (RGB565 & 0x07E0) >> 5;
				int byte_b = RGB565 & 0x1F;

				int ogR = byte_r;
				int ogG = byte_g;
				int ogB = byte_b;

				if (convertTo24Bit) {
					byte_r = (byte_r * 527 + 31) >> 5;
					byte_g = (byte_g * 259 + 33) >> 5;
					byte_b = (byte_b * 527 + 31) >> 6;
				} else {
					byte_r = (int)Math.Round(byte_r / 31f * 255f);
					byte_g = (int)Math.Round(byte_g / 63f * 255f);
					byte_b = (int)Math.Round(byte_b / 31f * 255f);
				}

				int r = (int)Math.Round((byte_r << 5) / 527d); // r
				int g = (int)Math.Round((byte_g << 5) / 259d); // g
				int b = (int)Math.Round((byte_b << 6) / 527d); // b

				//if (r - ogR + g - ogG + b - ogB != 0)
				//	Console.WriteLine("Difference!");

				imgData[j++] = (byte)(byte_r); // r
				imgData[j++] = (byte)(byte_g); // g
				imgData[j++] = (byte)(byte_b); // b
				imgData[j++] = (byte)((255)); // alpha
			}
		}

		public ImageData GetImageData() {
			ImageData img = new ImageData(Height, Width);

			int length = Width * Height * 4;
			int pos = DataOffset;

			byte[] imgData = new byte[length];

			GetImageData16Bit(buffer, imgData, pos, length);

			img.data = imgData;
			//Buffer.BlockCopy(imgData, 0, img.data, 0, length);

			return img;
		}
	}
}
