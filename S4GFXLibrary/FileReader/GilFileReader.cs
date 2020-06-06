using System.IO;

namespace S4GFXLibrary.FileReader
{
    public class GilFileReader : FileReaderBase
    {
        int[] offsetTable;


        public int GetImageCount()
        {
            return offsetTable != null ? offsetTable.Length : 0;
        }

        public int GetImageOffset(int index)
        {
            if (index < 0 || index >= offsetTable.Length)
            {
                return -1;
            }

            return offsetTable[index];
        }

        /// <summary>
        /// Moves all offsets by offsetToAdd, starting from index startIndex
        /// </summary>
        /// <param name="startIndex">the index to start from. INCLUDING</param>
        /// <param name="offsetToAdd">the amount to add to the offset</param>
        public void AddOffsetToFollowing(int startIndex, int offsetToAdd)
        {
            for (int i = startIndex; i < offsetTable.Length; i++)
            {
                offsetTable[i] += offsetToAdd;
            }
        }

        public GilFileReader(BinaryReader resourceReader)
        {
            ReadResource(resourceReader);

            int imageCount = GetImageCount(resourceReader);

            offsetTable = new int[imageCount];

            for (int i = 0; i < imageCount; i++)
            {
                offsetTable[i] = resourceReader.ReadInt32();
            }
        }

        override public byte[] GetData()
        {
            byte[] data = new byte[(int)baseStream.Length];

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
