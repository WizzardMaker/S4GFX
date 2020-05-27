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

		public GfxFileReader(BinaryReader reader,
			GilFileReader offsetTable, JilFileReader jobIndexList, DilFileReader directionIndexList, PaletteCollection paletteCollection)  {

			ReadResource(reader);

			int count = offsetTable.GetImageCount();
			images = new GfxImage[count];

			for(int i = 0; i < count; i++) {
				int gfxOffset = offsetTable.GetImagOffset(i);

				int jobIndex = i;

				if(directionIndexList != null) {
					int dirOffset = directionIndexList.ReverseLookupOffset(i);
					jobIndex = jobIndexList.ReverseLookupOffset(dirOffset);
				}

				images[i] = ReadImage(reader, gfxOffset, paletteCollection.GetPalette(), paletteCollection.GetOffset(jobIndex));
			}
		}

		GfxImage ReadImage(BinaryReader reader, int offset, Palette palette, int paletteOffset) {
			reader.BaseStream.Seek(offset, SeekOrigin.Begin);

			int imgHeadType = reader.ReadInt16();

			reader.BaseStream.Seek(offset, SeekOrigin.Begin);

			GfxImage newImg = new GfxImage(reader, palette, paletteOffset);

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
