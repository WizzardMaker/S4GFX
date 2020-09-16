using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;

namespace S4GFXLibrary.GFX
{
    public class Palette
    {
        #region Team Colors
        static ushort[] redTeam = { 0x0000, 0x0000, 0x0000, 0x0800, 0x1000, 0x1800, 0x2000, 0x2800, 0x3000, 0x3800, 0x4000, 0x4800, 0x5000, 0x5800, 0x6000, 0x6800, 0x7000, 0x7800, 0x8000, 0x8800, 0x9000, 0x9800, 0xA000, 0xA800, 0xB000, 0xB800, 0xC000, 0xC800, 0xD000, 0xD820, 0xE061, 0xE8A2, };
        static ushort[] blueTeam = { 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0001, 0x0002, 0x0003, 0x0004, 0x0005, 0x0006, 0x0007, 0x0008, 0x0008, 0x0009, 0x000A, 0x000B, 0x000C, 0x000D, 0x000E, 0x000F, 0x0010, 0x0011, 0x0832, 0x1073, 0x18B4, 0x20F5, 0x2936, 0x3177, 0x39B8, 0x41F9, };
        static ushort[] greenTeam = { 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0020, 0x0060, 0x00A0, 0x00E0, 0x0120, 0x0160, 0x01A0, 0x01E0, 0x0220, 0x0260, 0x02A0, 0x02E0, 0x0320, 0x0360, 0x03A0, 0x03E0, 0x0420, 0x0460, 0x04A0, 0x04E1, 0x0522, 0x0563, 0x05A4, 0x05E5, 0x0626, };
        static ushort[] yellowTeam = { 0x0000, 0x0800, 0x1020, 0x1860, 0x20A0, 0x28E0, 0x3120, 0x3960, 0x41A0, 0x41E0, 0x4A20, 0x5260, 0x5AA0, 0x62E0, 0x6B20, 0x7360, 0x7BA0, 0x83E0, 0x8C20, 0x9460, 0x9CA0, 0xA4E0, 0xAD20, 0xB540, 0xBD80, 0xC5C0, 0xCE00, 0xD640, 0xDE80, 0xE6C0, 0xEF00, 0xF740, };
        static ushort[] purpleTeam = { 0x0000, 0x0001, 0x0002, 0x0003, 0x0004, 0x0005, 0x0006, 0x0007, 0x0008, 0x0809, 0x100A, 0x180B, 0x200C, 0x280D, 0x300E, 0x380F, 0x4010, 0x4811, 0x5012, 0x5813, 0x6014, 0x6815, 0x7016, 0x7857, 0x8098, 0x88D9, 0x911A, 0x995B, 0xA19C, 0xA9DD, 0xB21E, 0xBA5F, };
        static ushort[] orangeTeam = { 0x0000, 0x0800, 0x1000, 0x1800, 0x2000, 0x2800, 0x3000, 0x3800, 0x4000, 0x4800, 0x5000, 0x5800, 0x6020, 0x6860, 0x70A0, 0x78E0, 0x8120, 0x8960, 0x91A0, 0x99E0, 0xA220, 0xAA60, 0xB2A0, 0xB2E0, 0xBB20, 0xC360, 0xCBA0, 0xD3E0, 0xDC21, 0xE462, 0xECA3, 0xF4E4, };
        static ushort[] cyanTeam = { 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0020, 0x0060, 0x00A0, 0x00E1, 0x0122, 0x0163, 0x01A4, 0x01E5, 0x0226, 0x0267, 0x02A8, 0x02C9, 0x030A, 0x034B, 0x038C, 0x03CD, 0x040E, 0x044F, 0x0490, 0x04D1, 0x0512, 0x0D53, 0x1594, 0x1DD5, 0x2616, };
        static ushort[] whiteTeam = { 0x0020, 0x0061, 0x00A2, 0x00E3, 0x0924, 0x1165, 0x19A6, 0x21E7, 0x2A28, 0x3269, 0x3AAA, 0x42EB, 0x4B2C, 0x536D, 0x5BAE, 0x63EF, 0x6C30, 0x7471, 0x7CB2, 0x84F3, 0x8D34, 0x9575, 0x9DB6, 0xA5F7, 0xAE38, 0xB679, 0xBEBA, 0xC6FB, 0xCF3C, 0xD77D, 0xD7BE, 0xDFFF, };

        static List<ushort[]> teams = new List<ushort[]> { redTeam, blueTeam, greenTeam, yellowTeam, purpleTeam, orangeTeam, cyanTeam, whiteTeam };
        #endregion

        public uint[] palette;

        public bool replaceTeamColors;
        public int team;

        public Palette(int count = 256)
        {
            palette = new uint[count];
        }

        public Palette(Palette copy)
        {
            palette = (uint[])copy.palette.Clone();
        }

		public void AddEntry() {
			Array.Resize(ref palette, palette.Length + 256);
		}

        public void SetRGB(int index, int r, int g, int b)
        {
            palette[index] = (uint)(r | g << 8 | b << 16 | 255 << 24);
        }
        public void SetColor(int index, uint color)
        {
            palette[index] = color;
        }

        public static uint RGBToPalette(int r, int g, int b, int a = 255)
        {
            return (uint)(r | g << 8 | b << 16 | a << 24);
        }

        public static uint RGB565ToPalette(ushort r565) {
            int value2 = (r565 >> 8) & 0x00FF;
            int value1 = r565 & 0x00FF;

            int r = value2 & 0b1111_1000;
            int g = value1 >> 3 | value2 << 5 & 0b1111_1100;
            int b = value1 << 3 & 0b1111_1000;

            return RGBToPalette(r, g, b);
        }

        public uint GetColor(int index, int offset)
        {
            //return RGBToPalette(offset, 0, 0);


            if (replaceTeamColors && offset > (224-32) && offset <(224)) {
				int i = 224 - offset;
                /*  32 - i = 32
                 *  32 - i = 0
                 */
				return RGB565ToPalette(teams[team][32-i]);
			}

            int offsetToFile = index - offset;

            if (offset >= 224 && replaceTeamColors) {
                offsetToFile += 256 + 32 * team;
                index = offsetToFile + (offset - 224);
            }

            return GetColor(index);
        }
        public uint GetColor(int index) {
            if (index < 0)
                return 0;
            return palette[index];
        }

        public int GetIndex(int offset, uint color)
        {
            return Array.IndexOf(palette, color, Math.Max(2, offset),256);
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

                int r = value2 & 0b1111_1000;
                int g = value1 >> 3 | value2 << 5 & 0b1111_1100;
                int b = value1 << 3 & 0b1111_1000;

                SetRGB(i, r, g, b);
            }

            return pos;
        }

        public void Write16BitPalette(BinaryWriter buffer, int pos = 0)
        {
            buffer.BaseStream.Seek(pos, SeekOrigin.Begin);

            for (int i = 0; i < palette.Length; i++)
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
