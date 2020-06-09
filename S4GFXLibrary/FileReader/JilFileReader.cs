using System.IO;

namespace S4GFXLibrary.FileReader
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
                    //Console.WriteLine($"JIL index:{dirOffset} --> jil:{i} at offset:{offset}");
                    //Console.WriteLine($"JIL dil:{dirOffset} --> jil{i}");
                    return i;
                }
            }

            //Console.WriteLine($"Unable to find offset dirOffset: {dirOffset}");
            return -1;
        }

        public void FakeLookupOffset(int length)
        {
            offsetTable = new int[length];

            for (int i = 0; i < offsetTable.Length; i++)
            {
                offsetTable[i] = i * 4 + HeaderSize;
            }
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
                    writer.Write(offsetTable[i]);
                }
            }

            return data;
        }
    }
}
