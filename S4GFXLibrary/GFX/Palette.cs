using System;
using System.IO;

namespace S4GFXLibrary.GFX
{
    class Palette
    {
        public uint[] palette;

        public Palette(int count = 256)
        {
            palette = new uint[count];
        }

        public Palette(Palette copy)
        {
            palette = (uint[])copy.palette.Clone();
        }

        public void SetRGB(int index, int r, int g, int b)
        {
            palette[index] = (uint)(r | g << 8 | b << 16 | 255 << 24);
        }
        public void SetColor(int index, uint color)
        {
            palette[index] = color;
        }

        public static uint RGBToPalette(int r, int g, int b)
        {
            return (uint)(r | g << 8 | b << 16 | 255 << 24);
        }

        public uint GetColor(int index)
        {
            return palette[index];
        }

        public int GetIndex(int offset, uint color)
        {
            return Array.IndexOf(palette, color, Math.Max(2, offset));
        }

        public int Read3BytePalette(byte[] buffer, int pos)
        {
            for (int i = 0; i < palette.Length; i++)
            {
                int r = buffer[pos++];
                int g = buffer[pos++];
                int b = buffer[pos++];

                SetRGB(i, r, g, b);
            }

            return pos;
        }

        public int Read16BitPalette(BinaryReader buffer, int pos = 0)
        {
            buffer.BaseStream.Seek(pos, SeekOrigin.Begin);

            for (int i = 0; i < palette.Length; i++)
            {
                int value1 = buffer.ReadByte();
                int value2 = buffer.ReadByte();

                int v = value1 << 8 | value2;

                int r = value2 & 0b1111_1000;
                int g = value1 >> 3 | value2 << 5 & 0b1111_1100;
                int b = value1 << 3 & 0b1111_1000;

                int valueG = ((r & 0b1111_1000) << 8) + ((g & 0b1111_1100) << 3) + (b >> 3);
                int v1 = valueG & 0b0000_0000_1111_1111;
                int v2 = valueG >> 8 & 0b0000_0000_1111_1111;

                SetRGB(i, r, g, b);
            }

            return pos;
        }

        public void Write16BitPalette(BinaryWriter buffer, int pos = 0)
        {
            buffer.BaseStream.Seek(pos, SeekOrigin.Begin);

            for (int i = pos; i < palette.Length; i++)
            {
                uint color = GetColor(i);

                int r = (int)(color & 0b0000_0000_0000_0000_1111_1111);
                int g = (int)(color >> 8 & 0b0000_0000_0000_0000_1111_1111);
                int b = (int)(color >> 16 & 0b0000_0000_0000_0000_1111_1111);

                int valueG = ((r & 0b1111_1000) << 8) + ((g & 0b1111_1100) << 3) + (b >> 3);
                byte v1 = (byte)(valueG & 0b0000_0000_1111_1111);
                byte v2 = (byte)(valueG >> 8 & 0b0000_0000_1111_1111);

                buffer.Write(v1);
                buffer.Write(v2);
            }
        }
    }
}
