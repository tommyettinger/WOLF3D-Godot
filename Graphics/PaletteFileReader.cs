﻿using System.IO;

namespace WOLF3D.Graphics
{
    public class PaletteFileReader
    {
        private static readonly int COLORS = 256;

        public static uint[] ColorModelFromPAL(string file)
        {
            uint[] result = new uint[COLORS];
            using (StreamReader input = new StreamReader(file))
            {
                if (!input.ReadLine().Equals("JASC-PAL") || !input.ReadLine().Equals("0100"))
                    throw new InvalidDataException("Palette \"" + file + "\" is an incorrectly formatted JASC palette.");
                if (!int.TryParse(input.ReadLine(), out int numColors)
                 || numColors != COLORS)
                    throw new InvalidDataException("Palette \"" + file + "\" does not contain exactly " + COLORS + " colors.");
                for (int x = 0; x < numColors; x++)
                {
                    string[] tokens = input.ReadLine()?.Split(' ');
                    if (tokens == null || tokens.Length != 3
                        || !byte.TryParse(tokens[0], out byte r)
                        || !byte.TryParse(tokens[1], out byte g)
                        || !byte.TryParse(tokens[2], out byte b))
                        throw new InvalidDataException("Palette \"" + file + "\" is an incorrectly formatted JASC palette.");
                    result[x] = (uint)(r << 24) 
                        + (uint)(g << 16) 
                        + (uint)(b << 8) 
                        + (uint)(x == 255 ? 0 : 255);
                }
            }
            return result;
        }
    }
}
