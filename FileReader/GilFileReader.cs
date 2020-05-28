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

		/// <summary>
		/// Moves all offsets by offsetToAdd, starting from index startIndex
		/// </summary>
		/// <param name="startIndex">the index to start from. INCLUDING</param>
		/// <param name="offsetToAdd">the amount to add to the offset</param>
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



		public byte[] GetData() {
			Byte[] data = new Byte[(int)baseStream.Length];

			using (BinaryWriter writer = new BinaryWriter(new MemoryStream(data))) {
				writer.Write(GetHeaderData());
				//magic = startReader.ReadInt32();
				//flag1 = startReader.ReadInt32();
				//flag2 = startReader.ReadInt32();
				//flag3 = startReader.ReadInt32();
				//flag4 = startReader.ReadInt32();

				writer.Seek(HeaderSize, SeekOrigin.Begin);

				for (int i = 0; i < offsetTable.Length; i++) {
					writer.Write((Int32)offsetTable[i]);
				}
			}

			return data;
		}
	}
}
