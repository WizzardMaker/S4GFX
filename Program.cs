using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S4GFX.FileReader;
using S4GFX.GFX;
using System.Drawing;

namespace S4GFX
{
	class Program
	{
		static GfxFileReader gfxFile;

		static void Main(string[] args) {
			Load("GFX/14");

			SaveBitmap();
			Console.ReadKey();
		}

		static public void Load(string fileId) {
			bool pil = File.Exists(fileId + ".pil");
			bool jil = File.Exists(fileId + ".jil");

			DoLoad(fileId, pil, jil);
		}

		static public void DoLoad(string fileId, bool usePli, bool useJil) {
			Console.WriteLine($"Using .jil={useJil}");

			var gfx = new BinaryReader(File.Open(fileId + ".gfx", FileMode.Open));
			var gil = new BinaryReader(File.Open(fileId + ".gil", FileMode.Open));

			BinaryReader paletteIndex, palette, directionIndex = null, jobIndex = null;

			if (usePli) {
				paletteIndex = new BinaryReader(File.Open(fileId + ".pil", FileMode.Open));
				palette = new BinaryReader(File.Open(fileId + ".pa6", FileMode.Open));
			} else {
				paletteIndex = new BinaryReader(File.Open(fileId + ".pi4", FileMode.Open));
				palette = new BinaryReader(File.Open(fileId + ".p46", FileMode.Open));
			}

			if (useJil) {
				directionIndex = new BinaryReader(File.Open(fileId + ".dil", FileMode.Open));
				jobIndex = new BinaryReader(File.Open(fileId + ".jil", FileMode.Open));
			}

			

			var gfxIndexList = new GilFileReader(gil);
			var paletteIndexList = new PilFileReader(paletteIndex);
			var paletteCollection = new PaletteCollection(palette, paletteIndexList);

			DilFileReader directionIndexList = null;
			JilFileReader jobIndexList = null;

			if (useJil) {
				directionIndexList = new DilFileReader(directionIndex);
				jobIndexList = new JilFileReader(jobIndex);
			}

			gfxFile = new GfxFileReader(gfx, gfxIndexList, jobIndexList, directionIndexList, paletteCollection);
		}

		static void SaveBitmap() {
			GfxImage image = gfxFile.GetImage(3);
			Bitmap b = new Bitmap(image.Width, image.Height);
			
			ImageData data = image.GetImageData();

			int index = 0;
			for (int y = 0; y < b.Height;y ++) {
				for(int x = 0; x < b.Width; x++) {
					b.SetPixel(x, y, Color.FromArgb(data.data[index + 0] == 255 && data.data[index + 1]+ data.data[index + 2] == 0 ? 0:255, data.data[index + 0], data.data[index + 1], data.data[index + 2]));
					index += 4;
				}
			}

			b.Save("test.png",System.Drawing.Imaging.ImageFormat.Png);
		}
	}
}
