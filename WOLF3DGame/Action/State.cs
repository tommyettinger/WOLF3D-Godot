﻿using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WOLF3D.WOLF3DGame.Action
{
    public class State
    {
        public string Name { get; set; } = null;
        public XElement XML { get; set; } = null;
        public bool Rotate { get; set; } = false;
        public short Shape { get; set; } = -1;
        public ushort TicTime { get; set; } = 0;
        public delegate void StateDelegate(Actor actor);
        public StateDelegate Think { get; set; } = null;
        public StateDelegate Act { get; set; } = null;
        public State Next { get; set; } = null;

        public State(XElement xml)
        {
            XML = xml;
            if (xml?.Attribute("Name")?.Value is string name)
                Name = name;
            Rotate = xml.IsTrue("Rotate");
            if (short.TryParse(xml?.Attribute("Shape")?.Value, out short shape))
                Shape = shape;
            if (ushort.TryParse(xml?.Attribute("TicTime")?.Value, out ushort ticTime))
                TicTime = ticTime;
        }
    }
}