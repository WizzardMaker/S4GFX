using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S4GFX.GFX
{
	class ImageData
	{
		public Byte[] data;
		List<UInt32> usedColors;

		public int height, width;

		public ImageData(int height, int width) {
			this.height = height;
			this.width = width;
		}

		public UInt32[] GetUsedColors(bool forceUpdate = false) {
			if(usedColors != null && !forceUpdate) {
				return usedColors.ToArray();
			}

			usedColors = new List<UInt32>();

			for(int c = 0; c < data.Length; c += 4) {
				UInt32 color = Palette.RGBToPalette(data[c], data[c + 1], data[c + 2]);

				if (!usedColors.Contains(color)) {
					usedColors.Add(color);
					usedColors.Sort();
				}
			}

			return usedColors.ToArray();
		}
	}
}
