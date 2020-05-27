using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S4GFX.FileReader
{
	class JilFileReader : FileReaderBase
	{
		Int32[] offsetTable;

		public int ReverseLookupOffset(int dirOffset) {
			int offset = dirOffset * 4*2 + HeaderSize;
			
			for (int i = 0; i < offsetTable.Length; i++) {
				if (offsetTable[i] == offset) {
					Console.WriteLine($"JIL {dirOffset} --> {i}");
					return i;
				}
			}

			Console.WriteLine($"Unable to find offset dirOffset: {dirOffset}");
			return 0;
		}

		public JilFileReader(BinaryReader reader) {
			ReadResource(reader);

			int imageCount = GetImageCount(reader);
			Console.WriteLine($"Image count: {imageCount}");

			offsetTable = new Int32[imageCount];

			for (int i = 0; i < imageCount; i++) {
				offsetTable[i] = reader.ReadInt32();
			}
		}
	}
}
