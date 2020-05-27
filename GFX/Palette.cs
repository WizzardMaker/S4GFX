using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S4GFX.GFX
{
	class Palette
	{
		UInt32[] palette;

		public Palette(int count = 256) {
			palette = new UInt32[count];
		}

		public void SetRGB(int index, int r, int g, int b) {
			palette[index] = (uint)(r | (g << 8) | (b << 16) | (255 << 24));
		}

		public UInt32 GetColor(int index) {
			return palette[index];
		}

		public int Read3BytePalette(Byte[] buffer, int pos) {
			for(int i = 0; i < palette.Length; i++) {
				int r = buffer[pos++];
				int g = buffer[pos++];
				int b = buffer[pos++];

				SetRGB(i, r, g, b);
			}

			return pos;
		}

		public int Read16BitPalette(BinaryReader buffer, int pos = 0) {
			buffer.BaseStream.Seek(pos, SeekOrigin.Begin);

			for (int i = 0; i < palette.Length; i++) {
				int value1 = buffer.ReadByte();
				int value2 = buffer.ReadByte();

				int r = value2 & 0xF8;
				int g = (value1 >> 3) | (value2 << 5) & 0xFC;
				int b = (value1 << 3) & 0xF8;

				SetRGB(i, r, g, b);
			}

			return pos;
		}
	}
}
