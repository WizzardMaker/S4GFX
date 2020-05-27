using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S4GFX.FileReader
{
	class GilFileReader : FileReaderBase
	{
		Int32[] offsetTable;


		public int GetImageCount() {
			return offsetTable != null ? offsetTable.Length : 0;
		}

		public Int32 GetImagOffset(int index) {
			if ((index < 0) || (index >= offsetTable.Length)) {
				return -1;
			}

			return offsetTable[index];
		}

		public GilFileReader(BinaryReader resourceReader) {
			ReadResource(resourceReader);

			int imageCount = GetImageCount(resourceReader);

			offsetTable = new Int32[imageCount];

			for(int i = 0; i < imageCount; i++) {
				offsetTable[i] = resourceReader.ReadInt32();
			}
		}
	}
}
