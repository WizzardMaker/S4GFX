using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S4GFXLibrary.FileReader
{
    abstract class FileReaderBase
    {
        int magic;
        int flag1;
        int flag2;
        int flag3;
        int flag4;

        public Stream baseStream;

        protected int GetImageCount(BinaryReader reader)
        {
            return ((int)reader.BaseStream.Length - HeaderSize) / 4;
        }

        public static int HeaderSize => 20; //Size in bytes

        /// <summary>
        /// Reads the head of the resource
        /// </summary>
        /// <param name="startReader">The file reader</param>
        /// <returns>The file reader starting offset from the header</returns>
        protected void ReadResource(BinaryReader startReader)
        {
            if (startReader.BaseStream.Length < HeaderSize)
            {
                Console.WriteLine($"Wrong file size! Wanted: {HeaderSize}, got {startReader.BaseStream.Length}");
                return;
            }

            magic = startReader.ReadInt32();
            flag1 = startReader.ReadInt32();
            flag2 = startReader.ReadInt32();
            flag3 = startReader.ReadInt32();
            flag4 = startReader.ReadInt32();

            baseStream = startReader.BaseStream;
        }

        override public string ToString()
        {
            return $"Header: {magic}; {flag1}, {flag2}, {flag3}, {flag4},";
        }

        protected byte[] GetHeaderData()
        {
            byte[] data = new byte[HeaderSize];

            using (BinaryWriter writer = new BinaryWriter(new MemoryStream(data)))
            {
                writer.Write(magic);
                writer.Write(flag1);
                writer.Write(flag2);
                writer.Write(flag3);
                writer.Write(flag4);
            }

            return data;
        }

        public abstract byte[] GetData();
    }
}
