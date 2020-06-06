namespace S4GFXLibrary.GFX
{
    interface IGfxImage
    {
        ImageData GetImageData();
        int GetDataSize();

        int DataOffset { get; set; }

        int Height { get; set; }
        int Width { get; set; }

        int Flag1 { get; set; }
        int Flag2 { get; set; }

        byte[] GetData();
    }
}
