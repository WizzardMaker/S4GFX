using S4GFXLibrary.GFX;

namespace S4GFXLibrary.FileReader
{
	public interface ICollectionFileReader
	{
		bool HasDIL { get; }

		IGfxImage GetImage(int index);
		int GetImageCount();
		void ChangeImageData(string choiceGroup, int image, ImageData[] data);
	}
}