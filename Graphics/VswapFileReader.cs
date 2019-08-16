﻿using System.Collections.Generic;
using System.IO;

namespace WOLF3D.Graphics
{
    public class VswapFileReader
    {
        #region Static variables
        private static readonly int NUM_DATA_OFS = 64;
        #endregion

        #region Inner classes
        public class VswapFileData
        {
            private List<byte[]> graphics;
            private int wallStartIndex, wallEndIndex;
            private int spriteStartIndex, spriteEndIndex;

            public List<byte[]> GetGraphics()
            {
                return graphics;
            }

            public void SetGraphics(List<byte[]> graphics)
            {
                this.graphics = graphics;
            }

            public int GetWallStartIndex()
            {
                return wallStartIndex;
            }

            public void SetWallStartIndex(int wallStartIndex)
            {
                this.wallStartIndex = wallStartIndex;
            }

            public int GetWallEndIndex()
            {
                return wallEndIndex;
            }

            public void SetWallEndIndex(int wallEndIndex)
            {
                this.wallEndIndex = wallEndIndex;
            }

            public int GetSpriteStartIndex()
            {
                return spriteStartIndex;
            }

            public void SetSpriteStartIndex(int spriteStartIndex)
            {
                this.spriteStartIndex = spriteStartIndex;
            }

            public int GetSpriteEndIndex()
            {
                return spriteEndIndex;
            }

            public void SetSpriteEndIndex(int spriteEndIndex)
            {
                this.spriteEndIndex = spriteEndIndex;
            }
        }
        #endregion

        #region Constructors
        private VswapFileReader()
        {
        }
        #endregion

        #region Static methods
        public static int ReadWord(FileStream file)
        {
            return (file.ReadByte() << 8) + file.ReadByte();
        }

        public static int ReadDWord(FileStream file)
        {
            return (ReadWord(file) << 16) + ReadWord(file);
        }

        public static VswapFileData Read(FileStream file, int dimension)
        {
            // parse header info
            int pageSize = dimension * dimension,
                chunks = ReadWord(file),
                spritePageOffset = ReadWord(file),
                soundPageOffset = ReadWord(file),
                graphicChunks = soundPageOffset;
            int[] pageOffsets = new int[graphicChunks];
            int dataStart = 0;

            for (int x = 0; x < graphicChunks; x++)
            {
                pageOffsets[x] = ReadDWord(file);
                if (x == 0)
                    dataStart = pageOffsets[0];
                if (pageOffsets[x] != 0 && (pageOffsets[x] < dataStart || pageOffsets[x] > file.Length))
                    throw new InvalidDataException("VSWAP file '" + file.Name + "' contains invalid page offsets.");
            }

            // parse graphic data
            List<byte[]> graphics = new List<byte[]>();
            byte[] dbuf = new byte[pageSize];
            byte[] bitMasks = new byte[] { (byte)0xFF };
            //SampleModel sampleModel = new SinglePixelPackedSampleModel(DataBuffer.TYPE_BYTE, dimension, dimension, bitMasks);
            //int[] sampleModel = new int[dimension ^ 2];
            //WritableRaster raster = Raster.createWritableRaster(sampleModel, dbuf, null);
            byte[] wall = new byte[dimension * dimension];
            int page;
            // read in walls
            for (page = 0; page < spritePageOffset; page++)
            {
                file.Seek(pageOffsets[page], 0);
                for (int col = 0; col < dimension; col++)
                    for (int row = 0; row < dimension; row++)
                        //dbuf[dimension * row + col] = (byte)file.ReadByte();
                        wall[dimension * row + col] = (byte)file.ReadByte();
                //            BufferedImage img = new BufferedImage(dimension, dimension, BufferedImage.TYPE_BYTE_INDEXED, colorModel);
                //img.setData(raster);
                graphics.Add(wall);
            }

            // read in sprites
            //        for (; page<graphicChunks; page++) {
            //            file.Seek(pageOffsets[page], 0);
            //// https://devinsmith.net/backups/bruce/wolf3d.html
            //        }

            // package results
            VswapFileData fileData = new VswapFileData();
            fileData.SetGraphics(graphics);
            fileData.SetWallStartIndex(0);
            fileData.SetWallEndIndex(spritePageOffset);
            fileData.SetSpriteStartIndex(spritePageOffset);
            fileData.SetSpriteEndIndex(soundPageOffset);
            return fileData;
        }

        /**
         * @param args 0 is VSWAP.WL1, 1 is the Wolf3D JASC palette from WDC (the Wolf Data Compiler at http://winwolf3d.dugtrio17.com ), 2 is output folder
         * @throws Exception
         */
        public static void Main(string[] args)
        {
            VswapFileData data;
            using (FileStream file = new FileStream(args[0], FileMode.Open))
                 data = VswapFileReader.Read(file, 64);

            for (int i = data.GetWallStartIndex(), x = 0; i < data.GetWallEndIndex(); i++)
            {
                bool even = i % 2 == 0;
                if (even)
                    x++;
                //ImageIO.write(data.GetGraphics()[i], "png", new File(String.format("%s/%s.png", args[2], x + (even ? "_1" : "_2"))));

            }
        }
        #endregion
    }
}
