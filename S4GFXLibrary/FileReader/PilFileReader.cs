using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S4GFXLibrary.FileReader
{

    /// <summary>
    /// The offset of each image in a gfx to their palette in the palette file
    /// </summary>
    class PilFileReader : FileReaderBase
    {
        int[] offsetTable;

        public int GetOffset(int gfxImageOffset)
        {
            return offsetTable[gfxImageOffset];
        }

        public void SetOffset(int gfxImageOffset, int offset)
        {
            offsetTable[gfxImageOffset] = offset;
        }

        public void Resize(int newSize)
        {
            Array.Resize(ref offsetTable, newSize);
        }

        public PilFileReader(BinaryReader reader)
        {
            ReadResource(reader);

            int imageCount = GetImageCount(reader);

            offsetTable = new int[imageCount];
            for (int i = 0; i < imageCount; i++)
            {
                offsetTable[i] = reader.ReadInt32();
            }
        }
        override public byte[] GetData()
        {
            byte[] data = new byte[offsetTable.Length * 4 + HeaderSize];

            using (BinaryWriter writer = new BinaryWriter(new MemoryStream(data)))
            {
                writer.Write(GetHeaderData());
                //magic = startReader.ReadInt32();
                //flag1 = startReader.ReadInt32();
                //flag2 = startReader.ReadInt32();
                //flag3 = startReader.ReadInt32();
                //flag4 = startReader.ReadInt32();

                writer.Seek(HeaderSize, SeekOrigin.Begin);

                for (int i = 0; i < offsetTable.Length; i++)
                {
                    writer.Write((GetOffset(i) - HeaderSize) * 2 + HeaderSize);
                }
            }

            return data;
        }
    }
}
