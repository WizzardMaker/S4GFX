using System.Collections.Generic;

namespace S4GFXLibrary.GFX
{
    public class ImageData
    {
        public byte[] data;
        List<uint> usedColors;

        public int height, width;

        public ImageData(int height, int width)
        {
            this.height = height;
            this.width = width;
        }

        public uint[] GetUsedColors(bool forceUpdate = false)
        {
            if (usedColors != null && !forceUpdate)
            {
                return usedColors.ToArray();
            }

            usedColors = new List<uint>();

            for (int c = 0; c < data.Length; c += 4)
            {
                uint color = Palette.RGBToPalette(data[c], data[c + 1], data[c + 2]);

                if (!usedColors.Contains(color))
                {
                    usedColors.Add(color);
                    usedColors.Sort();
                }
            }

            return usedColors.ToArray();
        }
    }
}
