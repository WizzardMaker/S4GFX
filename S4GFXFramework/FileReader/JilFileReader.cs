using System.IO;

namespace S4GFXFramework.FileReader
{
    public class JilFileReader : FileReaderBase
    {
        int[] offsetTable;

        public int ReverseLookupOffset(int dirOffset)
        {
            int offset = dirOffset * 4 + HeaderSize;

            for (int i = 0; i < offsetTable.Length; i++)
            {
                if (offsetTable[i] == offset)
                {
                    //Console.WriteLine($"JIL {dirOffset} --> {i}");
                    return i;
                }
            }

            //Console.WriteLine($"Unable to find offset dirOffset: {dirOffset}");
            return -1;
        }

        public JilFileReader(BinaryReader reader)
        {
            ReadResource(reader);

            int imageCount = GetImageCount(reader);
            //Console.WriteLine($"Image count: {imageCount}");

            offsetTable = new int[imageCount];

            for (int i = 0; i < imageCount; i++)
            {
                offsetTable[i] = reader.ReadInt32();
            }
        }
    }
}
