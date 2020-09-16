using System;
using System.Collections.Generic;
using System.IO;

namespace S4GFXLibrary.GFX
{
    public class GfxImage : IGfxImage
    {
        public int DataOffset { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Flag1 { get; set; }
        public int Flag2 { get; set; }

        public bool headType;
        public int imgType;

        public int jobIndex;

        //x and y offset of the image
        public int left, top;

        public byte[] buffer;

        public Palette palette;
        public int paletteOffset;

        public int HeaderSize => headType ? 8 : 12;

		public int Index { get; set; }

		public int GroupIndex { get; set; }

		int[] usedPaletteEntries;

        public int[] GetUsedPaletteEntries()
        {
            if (usedPaletteEntries == null)
            {
                GetImageData();
            }

            return usedPaletteEntries;
        }

        public GfxImage(byte[] buffer, Palette palette, int paletteOffset)
        {
            this.buffer = buffer;
            this.palette = palette;
            this.paletteOffset = paletteOffset;
        }

        public int GetDataSize()
        {
            return 0;
        }


        //Run length encoding
        //value is not a color?
        //value is a shadow(1) or transparent(0)
        //then the next value will have the info on how many following pixel are that value
        void GetImageDataWithRunLengthEncoding(byte[] buffer, uint[] imgData, int pos, int length)
        {
            List<int> paletteEntries = new List<int>();

            int j = 0;

            while (j < length)
            {
                int value = pos >= buffer.Length ? 0 : buffer[pos];
                pos++;

                uint color;
                int count = 1;

                if (value <= 1)
                {
                    count = pos >= buffer.Length ? 1 : buffer[pos];
                    pos++;

                    if (value == 0)
                    {
                        color = 0xFF0000FF;
                    }
                    else
                    {
                        color = 0xFF00FF00;
                    }
                }
                else
                {
                    color = palette.GetColor(paletteOffset + value, value);
				}

                for (int i = 0; i < count && j < length; i++)
                {
                    imgData[j++] = color;
                }
            }

            usedPaletteEntries = paletteEntries.ToArray();
            //Console.WriteLine(count);
        }

        public void GetImageDataWithNoEncoding(byte[] buffer, uint[] imgData, int pos, int length)
        {
            List<int> paletteEntries = new List<int>();

            int j = 0;
            while (j < length)
            {
                int value = pos >= buffer.Length ? 1 : buffer[pos];
                pos++;

                imgData[j++] = palette.GetColor(paletteOffset + value, value);
                if (!paletteEntries.Contains(paletteOffset + value))
                    paletteEntries.Add(paletteOffset + value);
            }

            usedPaletteEntries = paletteEntries.ToArray();
        }

        public byte[] CreateImageData(ImageData newImage)
        {
            int length = Width * Height * 4;
            int pos = DataOffset;

            byte[] data;

            if (imgType != 32)
            {
                data = CreateImageDataWithRunLengthEncoding(newImage.data, length);
            }
            else
            {
                data = WriteImageDataWithNoEncoding(newImage.data, pos, length);
            }
			return data;
        }



        /// <summary>
        /// Counts the times a value was repeated. Used in the RunLengthEncoding as it replaces duplicates by [value,repeats]
        /// </summary>
        /// <param name="data">the color data</param>
        /// <param name="start">start offset in the color data array</param>
        /// <param name="valueToFind">value we want to count</param>
        /// <param name="offset">how far we moved in the color array while counting</param>
        /// <returns></returns>
        private int GetSameValueCount(ref byte[] data, int start, int valueToFind, ref int offset)
        {
            int count = 0;
            offset = start;
            for (int i = start; i < data.Length; i += 4)
            { // limit by image width!
                int x = i / 4 % Width;
                int y = i / 4 / Width;

                byte red = data[i + 0];
                byte green = data[i + 1];
                byte blue = data[i + 2];
                byte alpha = data[i + 3];


                int value = -1;
                if (red == 255 && green + blue == 0)
                {
                    value = 0;//transparent
                }
                else if (green == 255 && red + blue == 0)
                {
                    value = 1;//shadow
                }

                if (value == valueToFind)
                    count++;
                else
                    break;

                //Console.WriteLine(x);

                offset = i;
                if (count >= Width)
                    break;
                if (count >= 255)
                    break;

            }
            return count;
        }

		private byte[] WriteImageDataWithNoEncoding(byte[] data, int pos, int length) {
			List<byte> newData = new List<byte>();

			for (int i = 0; i < length; i += 4) {
				int value;

				byte red = data[i + 0];
				byte green = data[i + 1];
				byte blue = data[i + 2];
				byte alpha = data[i + 3];

				value = palette.GetIndex(paletteOffset, Palette.RGBToPalette(red, green, blue));
				if (value == -1) {
					Console.WriteLine($"Invalid color - not found in the palette! Color: R:{red}, G:{green}, B:{blue}");
				}

				value = value - paletteOffset;

				if (value >= 252) {
					Console.WriteLine("Invalid color - not found in the palette!");
				}

				newData.Add((byte)value);
			}

			return newData.ToArray();
		}

		private byte[] CreateImageDataWithRunLengthEncoding(byte[] data, int length)
        {
            List<byte> newData = new List<byte>();

            int dataLength = 0;
            for (int i = 0; i < length; i += 4)
            {
                int value = 0;

                byte red = data[i + 0];
                byte green = data[i + 1];
                byte blue = data[i + 2];
                byte alpha = data[i + 3];

                bool valueIsOperator = false;

                if (red == 255 && green + blue == 0)
                {
                    value = 0;//transparent
                    valueIsOperator = true;
                }
                else if (green == 255 && red + blue == 0)
                {
                    value = 1;//shadow
                    valueIsOperator = true;
                }
                else
                {
                    value = palette.GetIndex(paletteOffset, Palette.RGBToPalette(red, green, blue));
					if(value == -1) {
						Console.WriteLine($"Invalid color - not found in the palette! Color: R:{red}, G:{green}, B:{blue}");
					}

                    value = value - paletteOffset;

					if (value >= 252) {
						Console.WriteLine("Invalid color - not found in the palette!");
					}

					value = Math.Max((byte)2, (byte)value);
                }
                int count = 1;

                newData.Add((byte)value);
                dataLength += 1;

                if (valueIsOperator == true)
                {
                    int offset = 0;
                    count = GetSameValueCount(ref data, i, value, ref offset);
                    newData.Add((byte)count);

                    i = offset;
                    dataLength += 1;
                }
			}

			return newData.ToArray();
        }

        public ImageData GetImageData()
        {
            ImageData img = new ImageData(Height, Width);

            int length = Width * Height * 4;
            int pos = DataOffset;

            uint[] imgData = new uint[length];

            if (imgType != 32)
            {
                GetImageDataWithRunLengthEncoding(buffer, imgData, pos, length);
            }
            else
            {
                GetImageDataWithNoEncoding(buffer, imgData, pos, length);
            }
            img.data = new byte[length];
            Buffer.BlockCopy(imgData, 0, img.data, 0, length);

            return img;
        }

        public byte[] GetHeaderData()
        {
            byte[] data = new byte[HeaderSize];

            using (BinaryWriter writer = new BinaryWriter(new MemoryStream(data)))
            {
                if (headType)
                {
                    writer.Write((byte)Width);
                    writer.Write((byte)Height);
                    writer.Write((byte)left);
                    writer.Write((byte)top);


                    writer.Write((byte)Flag1);
                    writer.Write((byte)Flag2);
                }
                else
                {
                    writer.Write((short)Width);
                    writer.Write((short)Height);
                    writer.Write((short)left);
                    writer.Write((short)top);

                    writer.Write((byte)imgType);


                    writer.Write((byte)Flag1);
                    //writer.Write((Byte)Flag2);
                }

            }

            return data;
        }

        public byte[] GetData()
        {
			int length = Width * Height * 4;
			byte[] data = new byte[length + HeaderSize];

            using (BinaryWriter writer = new BinaryWriter(new MemoryStream(data)))
            {
                writer.Write(GetHeaderData());

                writer.Seek(HeaderSize, SeekOrigin.Begin);

				for (int i = DataOffset; i < length + DataOffset; i++)
                {
                    if (i >= buffer.Length)
                        break;

                    writer.Write(buffer[i]);
                }
			}

            return data;
        }
    }
}
