﻿namespace S4GFXLibrary.GFX
{
    public interface IGfxImage
    {
        ImageData GetImageData();
        int GetDataSize();

        int DataOffset { get; set; }

        int Height { get; set; }
        int Width { get; set; }

        int Flag1 { get; set; }
        int Flag2 { get; set; }

		int Index { get; set; }
		int GroupIndex { get; set; }

		byte[] GetData();
    }
}
