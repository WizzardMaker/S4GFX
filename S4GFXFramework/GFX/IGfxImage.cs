namespace S4GFXFramework.GFX
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
    }
}
