﻿using Godot;

namespace WOLF3D.WOLF3DGame
{
    public class FadeCamera : ARVRCamera
    {
        public FadeCamera() => Cube = new MeshInstance()
        {
            Mesh = new CubeMesh()
            {
                Size = new Vector3(0.1f, 0.1f, 0.1f),
                Material = new SpatialMaterial()
                {
                    AlbedoColor = Color.Color8(0, 0, 255, 64),
                    FlagsUnshaded = true,
                    FlagsDoNotReceiveShadows = true,
                    FlagsDisableAmbientLight = true,
                    FlagsTransparent = true,
                    ParamsCullMode = SpatialMaterial.CullMode.Disabled,
                    ParamsSpecularMode = SpatialMaterial.SpecularMode.Disabled,
                },
            },

        };

        public MeshInstance Cube
        {
            get => cube;
            set
            {
                if (cube != null)
                    RemoveChild(cube);
                cube = value;
                if (cube != null)
                    AddChild(cube);
            }
        }
        private MeshInstance cube;

        public Color Color
        {
            get => ((SpatialMaterial)((CubeMesh)Cube.Mesh).Material).AlbedoColor;
            set => ((SpatialMaterial)((CubeMesh)Cube.Mesh).Material).AlbedoColor = value;
        }
    }
}