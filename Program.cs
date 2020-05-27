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

		static bool removeAlpha, removeShadows;

		static void Main(string[] args) {
			Console.WriteLine("Export all: (1), Export one group: (2), Export one individual: (3)");
			string choice = Console.ReadLine();

			switch (choice) {
				case "1":
					AskRemoveShadowsAndAlpha();

				//Parallel.For(0, 45, (i) => {
				//	var gfx = Load("GFX/" + i);
				//	if (gfx == null)
				//		return;

				//	SaveAllBitmaps("GFX/" + i, gfx);
				//});
				for (int i = 0; i < 45; i++) {
					Load("GFX/" + i);
					SaveAllBitmaps("GFX/" + i);
				}
				break;

				default:
				case "2": { 
					REPEAT:
					Console.WriteLine("What group:");
					string choiceGroup = Console.ReadLine();

					string path = "GFX/" + choiceGroup;
					if (!File.Exists(path + ".gfx")) {
							Console.WriteLine($"Group {path} does not exist!");
							goto REPEAT;
					}

					AskRemoveShadowsAndAlpha();
					Load(path);
					SaveAllBitmaps(path);
				}
				break;

				case "3": {
					REPEAT_SINGLE_GROUP:
					Console.WriteLine("What group:");
					string choiceGroup = Console.ReadLine();

					string path = "GFX/" + choiceGroup;
					if (!File.Exists(path + ".gfx")) {
						Console.WriteLine($"Group {path} does not exist!");
						goto REPEAT_SINGLE_GROUP;
					}
					Load(path);

					REPEAT_SINGLE_IMAGE:
					Console.WriteLine($"What image?: {gfxFile.GetImageCount()} images");
					string choiceImage = Console.ReadLine();

					int image = int.Parse(choiceImage);
					if (image > gfxFile.GetImageCount()) {
						Console.WriteLine($"There is no image nr. {image}!");
						goto REPEAT_SINGLE_IMAGE;
					}

					AskRemoveShadowsAndAlpha();
					SaveToBitmap(path, image, gfxFile);
				}
				break;
			}

			gfxFile = null;
			GC.Collect();
			//Console.ReadKey();
		}

		public static void AskRemoveShadowsAndAlpha() {
			Console.WriteLine("Remove nothing (1), only Alpha (2), Shadows (3) or both (4)?");
			string choice = Console.ReadLine();
			switch (choice) {
				case "2":
				removeAlpha = true;
				break;
				case "3":
				removeShadows = true;
				break;
				case "4":
				removeAlpha = removeShadows = true;
				break;
			}
		}

		static public GfxFileReader Load(string fileId) {

			bool gfx = File.Exists(fileId + ".gfx");
			if(gfx == false) {
				return null;
			}

			bool pil = File.Exists(fileId + ".pil");
			bool jil = File.Exists(fileId + ".jil");


			return DoLoad(fileId, pil, jil);
		}

		static public GfxFileReader DoLoad(string fileId, bool usePli, bool useJil) {
			//Console.WriteLine($"Using .jil={useJil}");

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
			return gfxFile;
		}

		static void SaveAllBitmaps(string path, GfxFileReader file = null) {
			Console.WriteLine($"Start saving: " + path);
			//Parallel.For(0, gfxFile.GetImageCount(), (i) => { SaveToBitmap(path, i); });
			file = file ?? gfxFile;

			for (int i = 0; i < file.GetImageCount(); i++) {
				try {
					SaveToBitmap(path, i, file);
				}catch(Exception e) {
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
			}

			Console.WriteLine($"Saved: " + path);
		}

		private static void SaveToBitmap(string path, int i, GfxFileReader file) {
			GfxImage image = file.GetImage(i);
			int width = image.Width;
			int height = image.Height;
			using (DirectBitmap b = new DirectBitmap(image.Width, image.Height)) {
				ImageData data = image.GetImageData();

				int index = 0;
				for (int y = 0; y < height; y++) {
					for (int x = 0; x < width; x++) {
						int alpha = 255;
						byte red = data.data[index + 0];
						byte green = data.data[index + 1];
						byte blue = data.data[index + 2];

						if (red == 255 && green + blue == 0) {
							alpha = removeAlpha ? 0 : alpha;
						}
						if (green == 255 && red + blue == 0) {
							alpha = removeShadows ? 0 : alpha;
						}

						b.SetPixel(x, y, Color.FromArgb(alpha, red, green, blue));

						index += 4;
					}
				}

				Directory.CreateDirectory(path);
				b.Bitmap.Save($"{path}/{i}.png", System.Drawing.Imaging.ImageFormat.Png);
				//if( i % 50 == 0)
					//Console.WriteLine($"Saved {i}/{gfxFile.GetImageCount()}");
			}
		}
	}
}
