using S4GFX.GFX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S4GFX.FileReader{
	class PaletteCollection : FileReaderBase {
		Palette palette;
		PilFileReader pilFile;
		int paletteFileOffset;

		public Palette GetPalette() {
			return palette;
		}

		public int GetOffset(int gfxImageIndex) {
			return (pilFile.GetOffset(gfxImageIndex) - paletteFileOffset) / 2; //Offset to the palette info for a image. ( / 2, because a palette color is 2 byte long)
		}

		public PaletteCollection(BinaryReader pa6File, PilFileReader pilFile) {
			this.pilFile = pilFile;

			ReadResource(pa6File);
			paletteFileOffset = HeaderSize;

			palette = new Palette(((int)pa6File.BaseStream.Length - HeaderSize) / 2);

			palette.Read16BitPalette(pa6File, HeaderSize);
		}
	}
}
