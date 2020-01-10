﻿using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using WOLF3DGame.Model;

namespace WOLF3DGame
{
    public class Level : Spatial
    {
        public GameMap Map { get; private set; }
        public WorldEnvironment WorldEnvironment { get; private set; }
        public MeshInstance Floor { get; private set; }
        public MeshInstance Ceiling { get; private set; }
        public StaticBody StaticBody { get; private set; }
        public CollisionShape[][] CollisionShapes { get; private set; }

        public bool CanWalk(int x, int z)
        {
            return !(x < 0 || z < 0 || x >= Map.Width || z >= Map.Depth);
        }

        public Level(GameMap map)
        {
            AddChild(WorldEnvironment = new WorldEnvironment()
            {
                Environment = new Godot.Environment()
                {
                    BackgroundColor = Game.Assets.Palette[map.Border],
                    BackgroundMode = Godot.Environment.BGMode.Color,
                },
            });

            AddChild(Floor = new MeshInstance()
            {
                Mesh = new QuadMesh()
                {
                    Size = new Vector2(map.Width * Assets.WallWidth, map.Depth * Assets.WallWidth),
                },
                MaterialOverride = new SpatialMaterial()
                {
                    AlbedoColor = Game.Assets.Palette[map.Floor],
                    FlagsUnshaded = true,
                    FlagsDoNotReceiveShadows = true,
                    FlagsDisableAmbientLight = true,
                    FlagsTransparent = false,
                    ParamsCullMode = SpatialMaterial.CullMode.Disabled,
                    ParamsSpecularMode = SpatialMaterial.SpecularMode.Disabled,
                },
                Transform = new Transform(
                    Basis.Identity.Rotated(Vector3.Right, Mathf.Pi / 2f),
                    new Vector3(
                        map.Width * Assets.HalfWallWidth,
                        0f,
                        map.Depth * Assets.HalfWallWidth
                    )
                ),
            });

            AddChild(Ceiling = new MeshInstance()
            {
                Mesh = new QuadMesh()
                {
                    Size = new Vector2(map.Width * Assets.WallWidth, map.Depth * Assets.WallWidth),
                },
                MaterialOverride = new SpatialMaterial()
                {
                    AlbedoColor = Game.Assets.Palette[map.Ceiling],
                    FlagsUnshaded = true,
                    FlagsDoNotReceiveShadows = true,
                    FlagsDisableAmbientLight = true,
                    FlagsTransparent = false,
                    ParamsCullMode = SpatialMaterial.CullMode.Disabled,
                    ParamsSpecularMode = SpatialMaterial.SpecularMode.Disabled,
                },
                Transform = new Transform(
                    Basis.Identity.Rotated(Vector3.Right, Mathf.Pi / 2f),
                    new Vector3(
                        map.Width * Assets.HalfWallWidth,
                        (float)Assets.WallHeight,
                        map.Depth * Assets.HalfWallWidth
                    )
                ),
            });

            MapWalls mapWalls = new MapWalls(map);
            foreach (Spatial sprite in mapWalls.Walls)
                AddChild(sprite);

            Billboard[] billboards = Billboard.MakeBillboards(map);
            foreach (Billboard billboard in billboards)
                AddChild(billboard);

            StaticBody = new StaticBody();
            CollisionShapes = new CollisionShape[Map.Width][];
            for (ushort x = 0; x < Map.Width; x++)
            {
                CollisionShapes[x] = new CollisionShape[Map.Depth];
                for (ushort z = 0; z < Map.Depth; z++)
                    if (!IsWall(Map.GetMapData(x, z)) || IsByFloor(x, z))
                        StaticBody.AddChild(CollisionShapes[x][z] = new CollisionShape()
                        {
                            Name = "CollisionShape at " + x + ", " + z,
                            Shape = Assets.BoxShape,
                            Disabled = false,
                            Transform = new Transform(Basis.Identity, new Vector3(x * Assets.WallWidth + Assets.HalfWallWidth, (float)Assets.HalfWallHeight, z * Assets.WallWidth + Assets.HalfWallWidth)),
                        });
            }
        }

        public bool IsWall(ushort x, ushort z) => IsWall(Map.GetMapData(x, z));

        public static bool IsWall(uint cell) => XWall(cell).Any();

        /// <returns>if the specified map coordinate is adjacent to a floor</returns>
        public bool IsByFloor(ushort x, ushort z)
        {
            ushort startX = x < 1 ? x : x > Map.Width - 1 ? (ushort)(Map.Width - 1) : (ushort)(x - 1),
                startZ = z < 1 ? z : z > Map.Depth - 1 ? (ushort)(Map.Depth - 1) : (ushort)(z - 1),
                endX = x > Map.Width - 1 ? (ushort)(Map.Width - 1) : x,
                endZ = z > Map.Depth - 1 ? (ushort)(Map.Depth - 1) : z;
            for (ushort dx = startX; dx <= endX; dx++)
                for (ushort dz = startZ; dz <= endZ; dz++)
                    if ((dx != x || dz != z) && !IsWall(Map.GetMapData(dx, dz)))
                        return true;
            return false;
        }

        public static uint WallTexture(uint cell) =>
            (uint)XWall(cell).FirstOrDefault()?.Attribute("Page");

        /// <summary>
        /// Never underestimate the power of the Dark Side
        /// </summary>
        public static uint DarkSide(uint cell) =>
            (uint)XWall(cell).FirstOrDefault()?.Attribute("DarkSide");

        public static IEnumerable<XElement> XWall(uint cell) =>
            from e in Game.Assets?.XML?.Element("VSwap")?.Element("Walls")?.Elements("Wall") ?? Enumerable.Empty<XElement>()
            where (uint)e.Attribute("Number") == cell
            select e;

        public static IEnumerable<XElement> XDoor(uint cell) =>
            from e in Game.Assets?.XML?.Element("VSwap")?.Element("Walls")?.Elements("Door") ?? Enumerable.Empty<XElement>()
            where (uint)e.Attribute("Number") == cell
            select e;

        public static uint DoorTexture(uint cell) =>
            (uint)XDoor(cell).FirstOrDefault()?.Attribute("Page");

        public static bool IsDoor(uint cell) =>
            XDoor(cell).Any();
    }
}
