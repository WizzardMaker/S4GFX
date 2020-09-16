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

				int r = (int)(color & 0b0000_0000_0000_0000_1111_1111);
				int g = (int)(color >> 8 & 0b0000_0000_0000_0000_1111_1111);
				int b = (int)(color >> 16 & 0b0000_0000_0000_0000_1111_1111);

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
