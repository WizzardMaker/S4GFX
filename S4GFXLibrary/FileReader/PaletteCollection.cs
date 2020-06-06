using S4GFXLibrary.GFX;
using System.IO;

namespace S4GFXLibrary.FileReader
{
    class PaletteCollection : FileReaderBase
    {
        Palette palette;
        PilFileReader pilFile;
        int paletteFileOffset;

        public Palette GetPalette()
        {
            return palette;
        }
        public void SetPalette(Palette p)
        {
            palette = p;
        }

        public PilFileReader GetPilFile()
        {
            return pilFile;
        }

        public int GetOffset(int gfxImageIndex)
        {
            return (pilFile.GetOffset(gfxImageIndex) - paletteFileOffset) / 2; //Offset to the palette info for a image. ( / 2, because a palette color is 2 byte long)
        }

        public PaletteCollection(BinaryReader pa6File, PilFileReader pilFile)
        {
            this.pilFile = pilFile;

            ReadResource(pa6File);
            paletteFileOffset = HeaderSize;

            palette = new Palette(((int)pa6File.BaseStream.Length - HeaderSize) / 2);

            palette.Read16BitPalette(pa6File, HeaderSize);
        }


        override public byte[] GetData()
        {
            byte[] data = new byte[palette.palette.Length * 2 + HeaderSize];

            using (BinaryWriter writer = new BinaryWriter(new MemoryStream(data)))
            {
                writer.Write(GetHeaderData());

                palette.Write16BitPalette(writer, HeaderSize);
            }

            return data;
        }
    }
}
