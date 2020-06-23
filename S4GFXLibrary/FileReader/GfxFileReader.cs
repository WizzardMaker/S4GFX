using S4GFXLibrary.GFX;
using System;
using System.Collections.Generic;
using System.IO;

namespace S4GFXLibrary.FileReader
{
	public class GfxFileReader : FileReaderBase, ICollectionFileReader
	{
		GfxImage[] images;
		bool isWordHeader;

		GilFileReader offsetTable;
		JilFileReader jobIndexList;
		DilFileReader directionIndexList;
		PaletteCollection paletteCollection;

		public bool HasDIL => directionIndexList != null;

		public int GetImageCount() {
			return images != null ? images.Length : 0;
		}

		public IGfxImage GetImage(int index) {
			if (index < 0 || index >= images.Length) {
				return null;
			}

			return images[index];
		}

		public void ChangeImageData(string groupID, int index, ImageData[] newDatas) {
			if (newDatas[0].GetUsedColors().Length > 255) {
				throw new Exception("Imported images cannot have more than 255 colors!");
			}

			//Prepare the new palette
			Palette p = paletteCollection.GetPalette();
			int start = paletteCollection.GetOffset((GetImage(index) as GfxImage).jobIndex);

			int palettePosition = 0;
			for (int j = 0; j < newDatas.Length; j++) {
				GfxImage i = images[index + j];

				i.Width = newDatas[j].width;
				i.Height = newDatas[j].height;
				//i.DataOffset = offsetTable.GetImageOffset(index + j) + 12;

				palettePosition += RecreatePaletteAt(start, palettePosition, newDatas[j]);

				i.buffer = i.CreateImageData(newDatas[j]);

				int nextImageStartOffset = offsetTable.GetImageOffset(index + j + 1); //Get the current offset of the next image
				int offset = Math.Max(0, i.DataOffset + i.buffer.Length - nextImageStartOffset - 1); //Our data could be larger, calculate how much larger

				if (offset != 0) { //We want to move all images that follow our changed image, if our new image is bigger
					offsetTable.AddOffsetToFollowing(index + j + 1, offset);
				}

				i.DataOffset = 0; //Hack, the data is still at the same offset, but the way the we handle the reading forces us to set it to 0, as we write the new image data to buffer[0...] instead of buffer[DataOffset...]
			}

			byte[] gfxDataBuffer = GetData();

			byte[] pilDataBuffer = paletteCollection.GetPilFile().GetData();
			byte[] paletteDataBuffer = paletteCollection.GetData();

			string fileId = groupID;

			File.WriteAllBytes(fileId + ".gfx", gfxDataBuffer);
			File.WriteAllBytes(fileId + ".pi4", pilDataBuffer);
			File.WriteAllBytes(fileId + ".p46", paletteDataBuffer);
			File.WriteAllBytes(fileId + ".pi2", pilDataBuffer);
			File.WriteAllBytes(fileId + ".p26", paletteDataBuffer);
			File.WriteAllBytes(fileId + ".gil", offsetTable.GetData());
		}

		public GfxFileReader(BinaryReader reader,
			GilFileReader offsetTable, JilFileReader jobIndexList, DilFileReader directionIndexList, PaletteCollection paletteCollection) {

			this.offsetTable = offsetTable;
			this.jobIndexList = jobIndexList;
			this.directionIndexList = directionIndexList;
			this.paletteCollection = paletteCollection;

			ReadResource(reader);

			reader.BaseStream.Seek(0, SeekOrigin.Begin);
			byte[] buffer = reader.ReadBytes((int)reader.BaseStream.Length);

			reader.BaseStream.Seek(HeaderSize, SeekOrigin.Begin);

			int count = offsetTable.GetImageCount();
			images = new GfxImage[count];

			for (int i = 0; i < count; i++) {
				int gfxOffset = offsetTable.GetImageOffset(i);

				int jobIndex = GetPaletteOffsetIndex(i);

				//Console.WriteLine($"JIL Offset: {jobIndex} == {reference}");


				images[i] = ReadImage(reader, gfxOffset, paletteCollection.GetPalette(), paletteCollection.GetOffset(jobIndex), buffer);
				images[i].jobIndex = jobIndex;
			}

			//ChangeImageData(2, images[2].GetImageData());
		}


		int lastGoodIndex, lastGoodJob;
		public int GetPaletteOffsetIndex(int imageIndex, bool useLastGood = true) {
			int lastGood = useLastGood ? lastGoodJob : 0;
			int jobIndex = 0;

			int start = useLastGood ? lastGoodIndex : 0;
			for (int i = start; i <= imageIndex; i++) {
				int gfxOffset = offsetTable.GetImageOffset(i);

				jobIndex = i;

				if (directionIndexList != null) {
					int dirOffset = directionIndexList.ReverseLookupOffset(i);

					jobIndex = jobIndexList.ReverseLookupOffset(dirOffset);
					jobIndex = jobIndex == -1 ? lastGood : jobIndex;
					lastGood = jobIndex;
				}
			}

			Console.WriteLine($"JIL Search for:{imageIndex}, got: {jobIndex}");
			lastGoodIndex = imageIndex;
			lastGoodJob = jobIndex;
			return jobIndex;
		}

		/// <summary>
		/// Connected images in the sense of, images that share a palette
		/// </summary>
		/// <param name="jil"></param>
		/// <returns></returns>
		public int[] GetConnectedImages(int jil) {
			List<int> imgs = new List<int>();

			for (int i = 0; i < images.Length; i++) {
				if (GetPaletteOffsetIndex(i, false) == jil) {
					imgs.Add(i);
				}
			}

			return imgs.ToArray();
		}


		/// <summary>
		/// Temporary function to remove the DIL and JIL "feature" - turns out, the game uses it for more than palette informations
		/// </summary>
		public void RemoveDILDependence() {
			//Prepare the colors for the palette generation
			ImageData[] colorData = new ImageData[images.Length];
			for (int j = 0; j < images.Length; j++) {
				colorData[j] = images[j].GetImageData();
				uint[] colorsToBeAdded = colorData[j].GetUsedColors();
			}

			PilFileReader pil = paletteCollection.GetPilFile();
			pil.Resize(images.Length);

			paletteCollection.SetPalette(new Palette((256 * images.Length + 20) * 2)); //maximum size for a palette. We could shrink it to size, but we don't have to

			for (int j = 0; j < images.Length; j++) {
				GfxImage i = images[j];
				int newPaletteOffset = 255 * j + 20; //Set according to JIL/normal palette offset.
				i.paletteOffset = newPaletteOffset;
				i.palette = paletteCollection.GetPalette();

				//RecreatePalette(j, 255 * j + 20, colorData[j]); //Skip repeating JIL entries. They share the same palette
				i.buffer = i.CreateImageData(colorData[j]);
				i.DataOffset = 0; //Hack, the data is still at the same offset, but the way the we handle the reading forces us to set it to 0, as we write the new image data to buffer[0...] instead of buffer[DataOffset...]
			}
			byte[] gfxDataBuffer = GetData();

			byte[] pilDataBuffer = paletteCollection.GetPilFile().GetData();
			byte[] paletteDataBuffer = paletteCollection.GetData();

			//directionIndexList.FakeLookupOffset(images.Length, jobIndexList); //Make every DIL entry point to itself. Doesnt work though in game
			//jobIndexList.FakeLookupOffset(images.Length);

			File.WriteAllBytes("3.gfx", gfxDataBuffer); //TODO! Change file name!
			File.WriteAllBytes("3.pi4", pilDataBuffer); //TODO! Change file name!
			File.WriteAllBytes("3.p46", paletteDataBuffer); //TODO! Change file name!
			File.WriteAllBytes("3.pi2", pilDataBuffer); //TODO! Change file name!
			File.WriteAllBytes("3.p26", paletteDataBuffer); //TODO! Change file name!
			File.WriteAllBytes("3.dil", directionIndexList.GetData()); //TODO! Change file name!
			File.WriteAllBytes("3.jil", jobIndexList.GetData()); //TODO! Change file name!
		}


		public int RecreatePaletteAt(int paletteOffset, int additionOffset, ImageData orig) {
			Palette p = paletteCollection.GetPalette();

			if (additionOffset == 0) {
				for (int i = 2; i < 256; i++) {
					p.SetColor(paletteOffset + i, 9999);
				}
			}

			List<uint> colorsToBeChecked = new List<uint>(orig.GetUsedColors());
			List<uint> colorsToBeAdded = new List<uint>();

			for (int i = paletteOffset; i < colorsToBeChecked.Count + paletteOffset; i++) {
				if (colorsToBeChecked[i - paletteOffset] == Palette.RGBToPalette(255, 0, 0) || colorsToBeChecked[i - paletteOffset] == Palette.RGBToPalette(0, 255, 0))
					continue;

				if (p.GetIndex(paletteOffset, colorsToBeChecked[i - paletteOffset]) == -1) {
					colorsToBeAdded.Add(colorsToBeChecked[i - paletteOffset]);
				}
			}


			for (int i = paletteOffset + additionOffset; i < colorsToBeAdded.Count + paletteOffset + additionOffset; i++) {
				p.SetColor(i, colorsToBeAdded[i - paletteOffset - additionOffset]);
			}

			return colorsToBeAdded.Count;

		}

		GfxImage ReadImage(BinaryReader reader, int offset, Palette palette, int paletteOffset, byte[] buffer) {
			reader.BaseStream.Seek(offset, SeekOrigin.Begin);

			int imgHeadType = (ushort)reader.ReadInt16();

			GfxImage newImg = new GfxImage(buffer, palette, paletteOffset);

			reader.BaseStream.Seek(offset, SeekOrigin.Begin);

			if (imgHeadType > 860) {
				isWordHeader = false;

				newImg.headType = true;
				newImg.Width = reader.ReadByte();
				newImg.Height = reader.ReadByte();
				newImg.left = reader.ReadByte();
				newImg.top = reader.ReadByte();

				newImg.imgType = 0;

				newImg.Flag1 = reader.ReadByte();
				newImg.Flag2 = reader.ReadByte();

				newImg.DataOffset = offset + 8;
			} else {
				isWordHeader = true;

				newImg.headType = false;
				newImg.Width = reader.ReadInt16();
				newImg.Height = reader.ReadInt16();
				newImg.left = reader.ReadInt16();
				newImg.top = reader.ReadInt16();

				newImg.imgType = reader.ReadByte();

				newImg.Flag1 = reader.ReadByte();
				reader.BaseStream.Seek(2, SeekOrigin.Begin);
				newImg.Flag2 = reader.ReadInt32();

				newImg.DataOffset = offset + 12;
			}

			return newImg;
		}

		override public byte[] GetData() {
			byte[] header = new byte[HeaderSize];

			using (BinaryWriter writer = new BinaryWriter(new MemoryStream(header))) {
				writer.Write(GetHeaderData());

				writer.Seek(HeaderSize, SeekOrigin.Begin);
			}

			byte[] gfxDataBuffer = new byte[(int)baseStream.Length];
			Buffer.BlockCopy(header, 0, gfxDataBuffer, 0, header.Length);

			for (int j = 0; j < images.Length; j++) {
				int offset = offsetTable.GetImageOffset(j);
				byte[] data = images[j].GetData();

				if (gfxDataBuffer.Length < offsetTable.GetImageOffset(j) + data.Length) {
					Array.Resize(ref gfxDataBuffer, offsetTable.GetImageOffset(j) + data.Length);
				}

				int size = data.Length;

				if (data.Length + offset > gfxDataBuffer.Length) {
					size = gfxDataBuffer.Length - offset;
				}

				Buffer.BlockCopy(data, 0, gfxDataBuffer, offset, size);

				if (j + 1 < images.Length) {
					if (gfxDataBuffer.Length < offsetTable.GetImageOffset(j + 1)) {
						Array.Resize(ref gfxDataBuffer, offsetTable.GetImageOffset(j + 1));
					}

					gfxDataBuffer[offsetTable.GetImageOffset(j + 1) - 1] = 0;
					gfxDataBuffer[offsetTable.GetImageOffset(j + 1) - 2] = 1;
					//gfxDataBuffer[offsetTable.GetImageOffset(j + 1) - 3] = 1;
				}
			}

			return gfxDataBuffer;
		}
	}
}
