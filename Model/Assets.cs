﻿using Godot;
using OPL;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace WOLF3D
{
    /// <summary>
    /// Assets takes the bytes extracted from VSwap and creates the corresponding Godot objects for them to be used throughout the game.
    /// </summary>
    public class Assets
    {
        //Tom Hall's Doom Bible and also tweets from John Carmack state that the walls in Wolfenstein 3D were always eight feet thick. The wall textures are 64x64 pixels, which means that the ratio is 8 pixels per foot.
        //However, VR uses the metric system, where 1 game unit is 1 meter in real space. One foot equals 0.3048 meters.
        //Now unless I am a complete failure at basic math (quite possible) this means that to scale Wolfenstein 3D correctly in VR, one pixel must equal 0.0381 in game units, and a Wolfenstein 3D wall must be 2.4384 game units thick.
        public static readonly float PixelWidth = 0.0381f;
        public static readonly float WallWidth = 2.4384f;
        public static readonly float HalfWallWidth = 1.2192f;

        // However, Wolfenstein 3D ran in SVGA screen mode 13h, which has a 320x200 resolution in a 4:3 aspect ratio.
        // This means that the pixels are not square! They have a 1.2:1 aspect ratio.
        public static readonly Vector3 Scale = new Vector3(1f, 1.2f, 1f);
        public static readonly float PixelHeight = 0.04572f;
        public static readonly double WallHeight = 2.92608;

        public static readonly Vector3 BillboardLocal = new Vector3(WallWidth / -2f, 0f, 0f);

        public Assets(string folder, string file = "game.xml")
        {
            Load(folder, file, out XElement xml, out VSwap vSwap, out GameMaps gameMaps, out AudioT audioT, out VgaGraph vgaGraph);
            XML = xml;
            VSwap = vSwap;
            GameMaps = gameMaps;
            AudioT = audioT;
        }

        public static void Load(string folder, out XElement xml, out VSwap vSwap, out GameMaps gameMaps, out AudioT audioT, out VgaGraph vgaGraph)
        {
            Load(folder, "game.xml", out xml, out vSwap, out gameMaps, out audioT, out vgaGraph);
        }

        public static void Load(string folder, string file, out XElement xml, out VSwap vSwap, out GameMaps gameMaps, out AudioT audioT, out VgaGraph vgaGraph)
        {
            using (FileStream xmlStream = new FileStream(System.IO.Path.Combine(folder, file), FileMode.Open))
                xml = XElement.Load(xmlStream);
            Load(folder, xml, out vSwap, out gameMaps, out audioT, out vgaGraph);
        }

        public static void Load(string folder, XElement xml, out VSwap vSwap, out GameMaps gameMaps, out AudioT audioT, out VgaGraph vgaGraph)
        {
            if (xml.Element("Palette") != null && xml.Element("VSwap") != null)
                using (MemoryStream palette = new MemoryStream(Encoding.ASCII.GetBytes(xml.Element("Palette").Value)))
                using (FileStream vSwapStream = new FileStream(System.IO.Path.Combine(folder, xml.Element("VSwap").Attribute("Name").Value), FileMode.Open))
                    vSwap = new VSwap(palette, vSwapStream);
            else vSwap = null;

            if (xml.Element("Maps") != null)
                using (FileStream mapHead = new FileStream(System.IO.Path.Combine(folder, xml.Element("Maps").Attribute("MapHead").Value), FileMode.Open))
                using (FileStream gameMapsStream = new FileStream(System.IO.Path.Combine(folder, xml.Element("Maps").Attribute("GameMaps").Value), FileMode.Open))
                    gameMaps = new GameMaps(mapHead, gameMapsStream);
            else gameMaps = null;

            if (xml.Element("Audio") != null)
                using (FileStream audioHead = new FileStream(System.IO.Path.Combine(folder, xml.Element("Audio").Attribute("AudioHead").Value), FileMode.Open))
                using (FileStream audioTStream = new FileStream(System.IO.Path.Combine(folder, xml.Element("Audio").Attribute("AudioT").Value), FileMode.Open))
                    audioT = new AudioT(audioHead, audioTStream, xml.Element("Audio"));
            else audioT = null;

            if (xml.Element("VgaGraph") != null)
                using (FileStream vgaDict = new FileStream(System.IO.Path.Combine(folder, xml.Element("VgaGraph").Attribute("VgaDict").Value), FileMode.Open))
                using (FileStream vgaHead = new FileStream(System.IO.Path.Combine(folder, xml.Element("VgaGraph").Attribute("VgaHead").Value), FileMode.Open))
                using (FileStream vgaGraphStream = new FileStream(System.IO.Path.Combine(folder, xml.Element("VgaGraph").Attribute("VgaGraph").Value), FileMode.Open))
                    vgaGraph = new VgaGraph(vgaDict, vgaHead, vgaGraphStream);
            else
                vgaGraph = null;
        }

        public XElement XML { get; set; }
        public GameMaps GameMaps { get; set; }

        public OplPlayer OplPlayer { get; set; }
        public AudioT AudioT { get; set; }

        public VSwap VSwap
        {
            get
            {
                return vswap;
            }
            set
            {
                vswap = value;
                Textures = new ImageTexture[VSwap.SoundPage];
                for (uint i = 0; i < Textures.Length; i++)
                    if (VSwap.Pages[i] != null)
                    {
                        Godot.Image image = new Image();
                        image.CreateFromData(64, 64, false, Image.Format.Rgba8, VSwap.Pages[i]);
                        Textures[i] = new ImageTexture();
                        Textures[i].CreateFromImage(image, (int)Texture.FlagsEnum.ConvertToLinear);
                    }
            }
        }
        private VSwap vswap;

        public VgaGraph VgaGraph { get; set; }

        public ImageTexture[] Textures;
    }
}