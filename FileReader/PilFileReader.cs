using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S4GFX.FileReader
{
	class PilFileReader : FileReaderBase
	{
		Int32[] offsetTable;

		public int GetOffset(int gfxImageOffset) {
			return offsetTable[gfxImageOffset];
		}

		public PilFileReader(BinaryReader reader) {
			ReadResource(reader);

			int imageCount = GetImageCount(reader);

			offsetTable = new Int32[imageCount];

			for(int i = 0; i < imageCount; i++) {
				offsetTable[i] = reader.ReadInt32();
			}
		}
	}
}
