﻿using Godot;
using System.Xml.Linq;

namespace WOLF3D.WOLF3DGame.Menu
{
    public class PixelRect : Node2D
    {
        public Vector2 Size
        {
            get => SEBorder.RectSize;
            set
            {
                SEBorder.RectSize = value;
                NWBorder.RectSize = new Vector2(value.x - 1, value.y - 1);
                ColorRect.RectSize = new Vector2(value.x - 2, value.y - 2);
            }
        }
        public ColorRect SEBorder { get; private set; }
        public ColorRect NWBorder { get; private set; }
        public ColorRect ColorRect { get; private set; }
        public Color SEColor
        {
            get => SEBorder.Color;
            set => SEBorder.Color = value;
        }
        public Color NWColor
        {
            get => NWBorder.Color;
            set => NWBorder.Color = value;
        }
        public Color Color
        {
            get => ColorRect.Color;
            set => ColorRect.Color = value;
        }
        public PixelRect()
        {
            AddChild(SEBorder = new ColorRect());
            AddChild(NWBorder = new ColorRect());
            AddChild(ColorRect = new ColorRect()
            {
                RectPosition = new Vector2(1, 1),
            });
        }
        public PixelRect(XElement xElement) : this()
        {
            NWBorder.Color = Assets.Palette[(uint)xElement.Attribute("BordColor")];
            SEBorder.Color = Assets.Palette[(uint)xElement.Attribute("Bord2Color")];
            Color = Assets.Palette[(uint)xElement.Attribute("Color")];
            Position = new Vector2((float)xElement.Attribute("X"), (float)xElement.Attribute("Y"));
            Size = new Vector2((float)xElement.Attribute("Width"), (float)xElement.Attribute("Height"));
        }
    }
}