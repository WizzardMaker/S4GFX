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

		public int height, width;

		public ImageData(int height, int width) {
			this.height = height;
			this.width = width;
		}
	}
}
