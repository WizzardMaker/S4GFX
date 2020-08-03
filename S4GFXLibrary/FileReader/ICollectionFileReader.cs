using S4GFXLibrary.GFX;
using System.Collections.Generic;
using System.IO;

namespace S4GFXLibrary.FileReader
{
	public interface ICollectionFileReader
	{
		bool HasDIL { get; }

		IGfxImage GetImage(int index);
		int GetImageCount();
		void ChangeImageData(string choiceGroup, int image, ImageData[] data);
		DataBufferCollection GetDataBufferCollection(string groupID);

		void Close();
	}

	public struct DataBufferCollection
	{
		string fileID;
		Dictionary<string, byte[]> buffer;

		//byte[] gfxDataBuffer, pilDataBuffer, paletteDataBuffer, offsetTableDataBuffer;

		public DataBufferCollection(string fileID) {
			this.fileID = fileID;

			buffer = new Dictionary<string, byte[]>();
		}

		public DataBufferCollection AddBuffer(byte[] b, string fileExtension, params string[] extraExtensions) {
			buffer.Add(fileExtension, b);
			foreach(string ext in extraExtensions) {
				buffer.Add(ext, b);
			}

			return this;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">Must be an "/" escaped string!</param>
		public void WriteToFiles(string path) {
			path = path + fileID;

			foreach(var file in buffer) {
				File.WriteAllBytes(path + $".{file.Key}", file.Value);
			}
			//File.WriteAllBytes(path + ".pi4", pilDataBuffer);
			//File.WriteAllBytes(path + ".p46", paletteDataBuffer);
			//File.WriteAllBytes(path + ".pi2", pilDataBuffer);
			//File.WriteAllBytes(path + ".p26", paletteDataBuffer);
			//File.WriteAllBytes(path + ".gil", offsetTableDataBuffer);
		}
	}
}