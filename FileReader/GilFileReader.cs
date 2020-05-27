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

		public Int32 GetImageOffset(int index) {
			if ((index < 0) || (index >= offsetTable.Length)) {
				return -1;
			}

			return offsetTable[index];
		}

		public void AddOffsetToFollowing(int startIndex, int offsetToAdd) {
			for(int i = startIndex; i < offsetTable.Length; i++) {
				offsetTable[i] += offsetToAdd;
			}
		}

		public GilFileReader(BinaryReader resourceReader) {
			ReadResource(resourceReader);

			int imageCount = GetImageCount(resourceReader);

			offsetTable = new Int32[imageCount];

			for(int i = 0; i < imageCount; i++) {
				offsetTable[i] = resourceReader.ReadInt32();
			}
		}

		public void WriteToFile(BinaryWriter writer) {
			writer.Seek(HeaderSize, SeekOrigin.Begin);

			for (int i = 0; i < offsetTable.Length; i++) {
				writer.Write((Int32)offsetTable[i]);
			}
		}
	}
}
