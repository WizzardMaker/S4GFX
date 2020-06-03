using System.IO;

namespace S4GFXFramework.FileReader
{
    public class DilFileReader : FileReaderBase
    {
        int[] offsetTable;

        public int ReverseLookupOffset(int gilIndex)
        {
            int offset = gilIndex * 4 + HeaderSize;

            int lastGood = 0;

            for (int i = 0; i < offsetTable.Length; i++)
            {
                if (offsetTable[i] == 0)
                    continue;

                if (offsetTable[i] > offset)
                {
                    //Console.WriteLine($"DIL {gilIndex} --> {lastGood}");
                    return lastGood;
                }

                lastGood = i;
            }

            //Console.WriteLine($"Unable to find offset gilIndex: {gilIndex}");
            return lastGood;
        }

        public DilFileReader(BinaryReader reader)
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
