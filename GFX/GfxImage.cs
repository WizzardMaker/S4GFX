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

		public int jobIndex;

		//x and y offset of the image
		public int left, top;

		public Byte[] buffer;

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


		//Run length encoding
		//value is not a color?
		//value is a shadow(1) or transparent(0)
		//then the next value will have the info on how many following pixel are that value
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

			//Console.WriteLine(count);
		}

		public void GetImageDataWithNoEncoding(Byte[] buffer, UInt32[] imgData, int pos, int length) {
			int j = 0;
			while (j < length) {
				int value = pos >= buffer.Length ? 1 : buffer[pos];
				pos++;

				imgData[j++] = palette.GetColor(paletteOffset + value);
			}
		}

		public Byte[] CreateImageData(ImageData newImage) {
			int length = Width * Height * 4;
			int pos = DataOffset;

			Byte[] data;

			if (imgType != 32) {
				data = CreateImageDataWithRunLengthEncoding(newImage.data, length);
			} else {
				data = WriteImageDataWithNoEncoding(newImage.data, pos, length);
			}
			return data;
		}

		private Byte[] WriteImageDataWithNoEncoding(byte[] data, int pos, int length) {
			throw new NotImplementedException();
		}

		private int GetSameValueCount(ref Byte[] data, int start, int valueToFind, ref int offset) {
			int count = 0;
			offset = start;
			for(int i = start; i < data.Length; i+=4) { // limit by image width!
				int x = (i/4) % Width;
				int y = (i/4) / Width;

				byte red = data[i + 0];
				byte green = data[i + 1];
				byte blue = data[i + 2];
				byte alpha = data[i + 3];


				int value = -1;
				if (red == 255 && green + blue == 0) {
					value = 0;//transparent
				} else if (green == 255 && red + blue == 0) {
					value = 1;//shadow
				}

				if (value == valueToFind)
					count++;
				else
					break;

				//Console.WriteLine(x);

				offset = i;
				if (count >= Width)
					break;
				if (count >= 255)
					break;

			}
			return count;
		}

		private Byte[] CreateImageDataWithRunLengthEncoding(byte[] data, int length) {
			List<Byte> newData = new List<Byte>();

			int dataLength = 0;
			for(int i = 0; i < length; i+=4) {
				int value = 0;

				byte red = data[i + 0];
				byte green = data[i + 1];
				byte blue = data[i + 2];
				byte alpha = data[i + 3];

				bool valueIsOperator = false;

				if (red == 255 && green + blue == 0) {
					value = 0;//transparent
					valueIsOperator = true;
				} else if (green == 255 && red + blue == 0) {
					value = 1;//shadow
					valueIsOperator = true;
				} else {
					value = palette.GetIndex(paletteOffset, Palette.RGBToPalette(red, green, blue));
					value = value - paletteOffset;
					value = Math.Max((byte)2, ((byte)value));
				}
				int count = 1;

				newData.Add((byte)value);
				dataLength += 1;

				if (valueIsOperator == true) {
					int offset = 0;
					count = GetSameValueCount(ref data, i, value, ref offset);
					newData.Add((byte)(count));
					//i += count * 4;
					i = offset;
					dataLength += 1;
				}
			}

			return newData.ToArray();
			//while (j < length) {
			//	int value = pos >= buffer.Length ? 0 : buffer[pos];
			//	pos++;

			//	UInt32 color;
			//	int count = 1;

			//	if (value <= 1) {
			//		count = pos >= buffer.Length ? 1 : buffer[pos];
			//		pos++;

			//		if (value == 0) {
			//			color = 0xFF0000FF;
			//		} else {
			//			color = 0xFF00FF00;
			//		}
			//	} else {
			//		color = palette.GetColor(paletteOffset + value);
			//	}

			//	for (int i = 0; (i < count) && (j < length); i++) {
			//		imgData[j++] = color;
			//	}
			//}
		}

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
			Buffer.BlockCopy(imgData, 0, img.data, 0, length);

			//byte[] test = CreateImageDataWithRunLengthEncoding(img.data, length);
			//Buffer.BlockCopy(test, 0, buffer, pos, test.Length);

			//GetImageDataWithRunLengthEncoding(buffer, imgData, pos, length);

			//Buffer.BlockCopy(imgData, 0, img.data, 0, img.data.Length);

			return img;
		}
	}
}
