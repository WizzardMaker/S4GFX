using S4GFXLibrary.GFX;
using System.Collections.Generic;
using System.IO;

namespace S4GFXLibrary.FileReader {
	public class GhFileReader : FileReaderBase, ICollectionFileReader
	{
		List<IGfxImage> images = new List<IGfxImage>();

		public bool HasDIL => false;

		public int GetImageCount() {
			return images != null ? images.Count : 0;
		}

		public IGfxImage GetImage(int index) {
			if (index < 0 || index >= images.Count) {
				return null;
			}

			return images[index];
		}

		public override byte[] GetData() {
			throw new System.NotImplementedException();
		}

		public void ChangeImageData(string choiceGroup, int image, ImageData[] data) {
			throw new System.NotImplementedException();
		}

		public void Close() {
			baseStream.Close();
		}

		public DataBufferCollection GetDataBufferCollection(string groupID) {
			throw new System.NotImplementedException();
		}

		public GhFileReader(BinaryReader reader) {
			ReadResource(reader);

			reader.BaseStream.Seek(0, SeekOrigin.Begin);
			byte[] buffer = reader.ReadBytes((int)reader.BaseStream.Length);

			reader.BaseStream.Seek(HeaderSize, SeekOrigin.Begin);


			int filePos = HeaderSize;
			int size = -1;
			int index = 0;
			while(size != 0) {
				reader.BaseStream.Seek(filePos, SeekOrigin.Begin);

				byte imageType = reader.ReadByte();
				byte flag1 = reader.ReadByte();
				byte flag2 = reader.ReadByte();
				byte rowCount = reader.ReadByte();
				size = reader.ReadInt32();

				IGfxImage img = null;

				switch (imageType) {
					case 0:
					img = new GfxImage16Bit(buffer, 128, rowCount);
					break;
					case 1:
					img = new GfxImage16Bit(buffer, 256, rowCount);
					break;
					case 2:
					img = new GfxImageWithPalette(buffer, 128, rowCount);
					break;
					case 3:
					img = new GfxImageWithPalette(buffer, 256, rowCount);
					break;
				}

				img.Flag1 = flag1;
				img.Flag1 = flag2;
				img.DataOffset = filePos + 8;

				img.Index = index;
				img.GroupIndex = index;

				images.Add(img);

				filePos += size + 8;
				index++;
			}
		}
	}
}
