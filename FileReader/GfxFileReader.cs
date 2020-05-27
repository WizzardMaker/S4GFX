using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S4GFX.GFX;

namespace S4GFX.FileReader
{
	class GfxFileReader : FileReaderBase
	{
		GfxImage[] images;
		bool isWordHeader;

		public int GetImageCount() {
			return images != null ? images.Length : 0;
		}

		public GfxImage GetImage(int index) {
			if((index < 0) || (index >= images.Length)) {
				return null;
			}

			return images[index];
		}

		public void ResizeImage(int index, int width, int height) {
			GfxImage image = images[index];

			int oldSize = image.Width * image.Height * 4;
			int offsetToAdd = width * height * 4 - oldSize;

			offsetTable.AddOffsetToFollowing(index+1, offsetToAdd);
		}

		GilFileReader offsetTable;
		JilFileReader jobIndexList;
		DilFileReader directionIndexList;
		PaletteCollection paletteCollection;
		public GfxFileReader(BinaryReader reader,
			GilFileReader offsetTable, JilFileReader jobIndexList, DilFileReader directionIndexList, PaletteCollection paletteCollection)  {

			this.offsetTable = offsetTable;
			this.jobIndexList = jobIndexList;
			this.directionIndexList = directionIndexList;
			this.paletteCollection = paletteCollection;

			ReadResource(reader);

			reader.BaseStream.Seek(0, SeekOrigin.Begin);
			Byte[] buffer = reader.ReadBytes((int)reader.BaseStream.Length);

			reader.BaseStream.Seek(HeaderSize, SeekOrigin.Begin);

			int count = offsetTable.GetImageCount();
			images = new GfxImage[count];

			int lastGood = 0;
			for(int i = 0; i < count; i++) {
				int gfxOffset = offsetTable.GetImageOffset(i);

				int jobIndex = i;

				if(directionIndexList != null) {
					int dirOffset = directionIndexList.ReverseLookupOffset(i);

					jobIndex = jobIndexList.ReverseLookupOffset(dirOffset);
					jobIndex = jobIndex == -1 ? lastGood : jobIndex;
					lastGood = jobIndex;
				}

				//Console.WriteLine($"JIL Offset: {jobIndex} in image {i}");

				images[i] = ReadImage(reader, gfxOffset, paletteCollection.GetPalette(), paletteCollection.GetOffset(jobIndex), buffer);
			}
		}

		GfxImage ReadImage(BinaryReader reader, int offset, Palette palette, int paletteOffset, Byte[] buffer) {
			reader.BaseStream.Seek(offset, SeekOrigin.Begin);

			int imgHeadType = (UInt16)reader.ReadInt16();

			GfxImage newImg = new GfxImage(buffer, palette, paletteOffset);

			reader.BaseStream.Seek(offset, SeekOrigin.Begin);


			if(imgHeadType > 860) {
				isWordHeader = true;

				newImg.headType = true;
				newImg.Width = reader.ReadByte();
				newImg.Height = reader.ReadByte();
				newImg.left = reader.ReadByte();
				newImg.top = reader.ReadByte();

				newImg.imgType = 0;

				newImg.Flag1 = reader.ReadByte();
				newImg.Flag2 = reader.ReadByte();

				newImg.DataOffset = offset + 8;
			} else {
				isWordHeader = false;

				newImg.headType = false;
				newImg.Width = reader.ReadInt16();
				newImg.Height = reader.ReadInt16();
				newImg.left = reader.ReadInt16();
				newImg.top = reader.ReadInt16();

				newImg.imgType = reader.ReadByte();

				newImg.Flag1 = reader.ReadByte();
				reader.BaseStream.Seek(2, SeekOrigin.Begin);
				newImg.Flag2 = reader.ReadInt32();

				newImg.DataOffset = offset + 12;
			}

			return newImg;
		}
	}
}
