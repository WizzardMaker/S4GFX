using System.IO;

namespace S4GFXFramework.FileReader
{

    /// <summary>
    /// The offset of each image in a gfx to their palette in the palette file
    /// </summary>
    public class PilFileReader : FileReaderBase
    {
        int[] offsetTable;

        public int GetOffset(int gfxImageOffset)
        {
            return offsetTable[gfxImageOffset];
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
    }
}
