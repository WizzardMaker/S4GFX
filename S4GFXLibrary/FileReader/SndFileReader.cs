using System;
using System.IO;

namespace S4GFXLibrary.FileReader
{
    class SndFileReader : FileReaderBase
    {
        byte[][] sounds; // first index is sound ID, second index is byte of sound file (type is wave)

        SilFileReader offsetTable;

        public int GetSoundCount()
        {
            return sounds != null ? sounds.Length : 0;
        }

        public byte[] GetSound(int index)
        {
            // note that sound with ID 0 is NO_SOUND
            if (index <= 0 || index >= sounds.Length)
            {
                return null;
            }

            return sounds[index];
        }

        public void ChangeSoundData(int index, byte[] newData)
        {
            throw new NotImplementedException("sorry, this has not yet been implemented");
        }

        public SndFileReader(BinaryReader reader,
            SilFileReader offsetTable)
        {

            this.offsetTable = offsetTable;

            ReadResource(reader);

            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            byte[] buffer = reader.ReadBytes((int)reader.BaseStream.Length);

            reader.BaseStream.Seek(HeaderSize, SeekOrigin.Begin);

            int count = offsetTable.GetSoundCount();
            sounds = new byte[count][];

            sounds[0] = new byte[0]; // sound ID 0 is NO_SOUND

            for (int i = 1; i < count; i++)
            {
                int gfxOffset = offsetTable.GetSoundOffset(i);

                int jobIndex = i;

                sounds[i] = ReadSound(reader, gfxOffset, buffer);
            }
        }

        byte[] ReadSound(BinaryReader reader, int offset, byte[] buffer)
        {
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);

            int sndHeadType = reader.ReadInt32(); // always 1 ?
            int sndSize = reader.ReadInt32();

            return reader.ReadBytes(sndSize);
        }

        public override byte[] GetData()
        {
            throw new NotImplementedException();
        }
    }
}
