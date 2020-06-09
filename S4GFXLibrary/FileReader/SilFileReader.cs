using System.IO;

namespace S4GFXLibrary.FileReader
{
    public class SilFileReader : GilFileReader
    {   // Sil File is basically just handled like a GilFile. ID 0 is NO_SOUND
        public SilFileReader(BinaryReader reader) : base(reader) { }

        public int GetSoundCount()
        {
            return GetImageCount();
        }

        public int GetSoundOffset(int index)
        {
            return GetImageOffset(index);
        }
    }
}
