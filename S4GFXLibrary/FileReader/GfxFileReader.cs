using S4GFXLibrary.GFX;
using System;
using System.Collections.Generic;
using System.IO;

namespace S4GFXLibrary.FileReader
{
    public class GfxFileReader : FileReaderBase
    {
        GfxImage[] images;
        bool isWordHeader;

        GilFileReader offsetTable;
        JilFileReader jobIndexList;
        DilFileReader directionIndexList;
        PaletteCollection paletteCollection;

        public int GetImageCount()
        {
            return images != null ? images.Length : 0;
        }

        public GfxImage GetImage(int index)
        {
            if (index < 0 || index >= images.Length)
            {
                return null;
            }

            return images[index];
        }

        public void ChangeImageData(int index, ImageData newData)
        {
            if (newData.GetUsedColors().Length > 255)
            {
                throw new Exception("Imported images cannot have more than 255 colors!");
            }

            //Read the old data
            BinaryReader reader = new BinaryReader(baseStream);
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            byte[] oldData = reader.ReadBytes((int)baseStream.Length);

            //Get old image we want to change and create the corresponding new file data bytes from the new image
            GfxImage image = images[index];
            byte[] imageDataToWrite = image.CreateImageData(newData); //The new bytes which are to be written to the gfx file

            int nextImageStartOffset = offsetTable.GetImageOffset(index + 1); //Get the current offset of the next image
            int offset = Math.Max(0, images[index].DataOffset + imageDataToWrite.Length - nextImageStartOffset - 1); //Our data could be larger, calculate how much larger

            byte[] newDataBuffer = new byte[(int)baseStream.Length + offset];//Our new data
            Buffer.BlockCopy(oldData, 0, newDataBuffer, 0, image.DataOffset);//Fill it with the old data, up until our new image. We don't need to rewrite it, it still works

            //write new data

            Buffer.BlockCopy(imageDataToWrite, 0, newDataBuffer, image.DataOffset, imageDataToWrite.Length); //Our new image gets written to the new gfx file

            if (offset != 0)
            { //We want to move all images that follow our changed image, if our new image is bigger
                offsetTable.AddOffsetToFollowing(index + 1, offset);

                File.WriteAllBytes("14TEST.gil", offsetTable.GetData()); //TODO! Change file name!
            }

            //Write new width and height
            int newImageOffset = offsetTable.GetImageOffset(index);
            if (image.headType)
            { //All sizes are 8Bit when true
                newDataBuffer[newImageOffset] = (byte)newData.width;//width
                newDataBuffer[newImageOffset + 1] = (byte)newData.height;//height
            }
            else
            { //16bit values
                newDataBuffer[newImageOffset] = (byte)newData.width; //width 1. Byte
                newDataBuffer[newImageOffset + 1] = (byte)(newData.width >> 8); //width 2. Byte

                newDataBuffer[newImageOffset + 2] = (byte)newData.height; //height 1. Byte
                newDataBuffer[newImageOffset + 3] = (byte)(newData.height >> 8); //height 2. Byte
            }

            //All files end with 0 and 1! This marks the end of the file
            newDataBuffer[nextImageStartOffset + offset - 1] = 0;
            newDataBuffer[nextImageStartOffset + offset - 2] = 1;

            //UpdatePalette(index, image, newData);

            Buffer.BlockCopy(oldData, nextImageStartOffset, newDataBuffer, offsetTable.GetImageOffset(index + 1), oldData.Length - nextImageStartOffset);

            File.WriteAllBytes("14TEST.gfx", newDataBuffer); //TODO! Change file name!
        }

        public GfxFileReader(BinaryReader reader,
            GilFileReader offsetTable, JilFileReader jobIndexList, DilFileReader directionIndexList, PaletteCollection paletteCollection)
        {

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

            int lastGood = 0;
            for (int i = 0; i < count; i++)
            {
                int gfxOffset = offsetTable.GetImageOffset(i);

                int jobIndex = GetPaletteOffsetIndex(i);

                //Console.WriteLine($"JIL Offset: {jobIndex} == {reference}");


                images[i] = ReadImage(reader, gfxOffset, paletteCollection.GetPalette(), paletteCollection.GetOffset(jobIndex), buffer);
                images[i].jobIndex = jobIndex;
            }

            //ChangeImageData(2, images[2].GetImageData());
        }

        public int GetPaletteOffsetIndex(int imageIndex)
        {
            int lastGood = 0;
            int jobIndex = 0;

            Console.WriteLine($"JIL Search for:{imageIndex}");

            for (int i = 0; i <= imageIndex; i++)
            {
                int gfxOffset = offsetTable.GetImageOffset(i);

                jobIndex = i;

                if (directionIndexList != null)
                {
                    int dirOffset = directionIndexList.ReverseLookupOffset(i);

                    jobIndex = jobIndexList.ReverseLookupOffset(dirOffset);
                    jobIndex = jobIndex == -1 ? lastGood : jobIndex;
                    lastGood = jobIndex;
                }
            }

            return jobIndex;
        }

        /// <summary>
        /// Connected images in the sense of, images that share a palette
        /// </summary>
        /// <param name="jil"></param>
        /// <returns></returns>
        public int[] GetConnectedImages(int jil)
        {
            List<int> imgs = new List<int>();

            for (int i = 0; i < images.Length; i++)
            {
                if (GetPaletteOffsetIndex(i) == jil)
                {
                    imgs.Add(i);
                }
            }

            return imgs.ToArray();
        }


        /// <summary>
        /// Temporary function to remove the DIL and JIL "feature" - turns out, the game uses it for more than palette informations
        /// </summary>
        public void RemoveDILDependence()
        {
            //Prepare the colors for the palette generation
            ImageData[] colorData = new ImageData[images.Length];
            for (int j = 0; j < images.Length; j++)
            {
                colorData[j] = images[j].GetImageData();
                uint[] colorsToBeAdded = colorData[j].GetUsedColors();
            }

            PilFileReader pil = paletteCollection.GetPilFile();
            pil.Resize(images.Length);

            paletteCollection.SetPalette(new Palette((256 * images.Length + 20) * 2)); //maximum size for a palette. We could shrink it to size, but we don't have to

            for (int j = 0; j < images.Length; j++)
            {
                GfxImage i = images[j];
                int newPaletteOffset = 255 * j + 20; //Set according to JIL/normal palette offset.
                i.paletteOffset = newPaletteOffset;
                i.palette = paletteCollection.GetPalette();

                RecreatePalette(j, 255 * j + 20, colorData[j]); //Skip repeating JIL entries. They share the same palette
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


        public void RecreatePalette(int index, int paletteOffset, ImageData orig)
        {
            PilFileReader pil = paletteCollection.GetPilFile();
            Palette p = paletteCollection.GetPalette();

            pil.SetOffset(index, paletteOffset);
            uint[] colorsToBeAdded = orig.GetUsedColors();

            for (int i = paletteOffset; i < colorsToBeAdded.Length + paletteOffset; i++)
            {
                p.SetColor(i + 2, colorsToBeAdded[i - paletteOffset]);
            }
        }

        GfxImage ReadImage(BinaryReader reader, int offset, Palette palette, int paletteOffset, byte[] buffer)
        {
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);

            int imgHeadType = (ushort)reader.ReadInt16();

            GfxImage newImg = new GfxImage(buffer, palette, paletteOffset);

            reader.BaseStream.Seek(offset, SeekOrigin.Begin);

            if (imgHeadType > 860)
            {
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
            }
            else
            {
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

        override public byte[] GetData()
        {
            byte[] header = new byte[HeaderSize];

            using (BinaryWriter writer = new BinaryWriter(new MemoryStream(header)))
            {
                writer.Write(GetHeaderData());

                writer.Seek(HeaderSize, SeekOrigin.Begin);
            }

            byte[] gfxDataBuffer = new byte[(int)baseStream.Length];
            Buffer.BlockCopy(header, 0, gfxDataBuffer, 0, header.Length);

            for (int j = 0; j < images.Length; j++)
            {
                int offset = offsetTable.GetImageOffset(j);
                byte[] data = images[j].GetData();

                int size = data.Length;

                if (data.Length + offset > gfxDataBuffer.Length)
                {
                    size = gfxDataBuffer.Length - offset;
                }

                Buffer.BlockCopy(data, 0, gfxDataBuffer, offset, size);

                if (j + 1 < images.Length)
                {
                    gfxDataBuffer[offsetTable.GetImageOffset(j + 1) - 1] = 0;
                    gfxDataBuffer[offsetTable.GetImageOffset(j + 1) - 2] = 1;
                }
            }

            return gfxDataBuffer;
        }
    }
}
