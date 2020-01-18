﻿using Godot;
using System.Collections;
using System.Collections.Generic;
using WOLF3DGame.Model;

namespace WOLF3DGame
{
    public class ARVRPlayer : KinematicBody
    {
        public bool Roomscale { get; set; } = true;
        public ARVROrigin ARVROrigin { get; set; }
        public ARVRCamera ARVRCamera { get; set; }
        public ARVRController LeftController { get; set; }
        public ARVRController RightController { get; set; }

        public override void _Ready()
        {
            AddChild(ARVROrigin = new ARVROrigin());
            ARVROrigin.AddChild(LeftController = new ARVRController()
            {
                ControllerId = 1,
            });
            ARVROrigin.AddChild(RightController = new ARVRController()
            {
                ControllerId = 2,
            });
            ARVROrigin.AddChild(ARVRCamera = new ARVRCamera()
            {
                Current = true,
            });

            /*
            ARVROrigin.AddChild(new MeshInstance()
            {
                Mesh = new CubeMesh()
                {
                    Size = new Vector3(Assets.HeadXZ, Assets.HeadXZ, Assets.HeadXZ),
                },
                MaterialOverride = new SpatialMaterial()
                {
                    AlbedoColor = Color.Color8(255, 165, 0, 255), // Orange
                    FlagsUnshaded = true,
                    FlagsDoNotReceiveShadows = true,
                    FlagsDisableAmbientLight = true,
                    FlagsTransparent = false,
                    ParamsCullMode = SpatialMaterial.CullMode.Disabled,
                    ParamsSpecularMode = SpatialMaterial.SpecularMode.Disabled,
                },
            });
            AddChild(new MeshInstance()
            {
                Mesh = new CubeMesh()
                {
                    Size = new Vector3(Assets.HeadXZ, Assets.HeadXZ, Assets.HeadXZ),
                },
                MaterialOverride = new SpatialMaterial()
                {
                    AlbedoColor = Color.Color8(255, 0, 255, 255), // Purple
                    FlagsUnshaded = true,
                    FlagsDoNotReceiveShadows = true,
                    FlagsDisableAmbientLight = true,
                    FlagsTransparent = false,
                    ParamsCullMode = SpatialMaterial.CullMode.Disabled,
                    ParamsSpecularMode = SpatialMaterial.SpecularMode.Disabled,
                },
            });
            */
        }

        public static float Strength(float input) =>
            Mathf.Abs(input) < Assets.DeadZone ? 0f
            : (Mathf.Abs(input) - Assets.DeadZone) / (1f - Assets.DeadZone) * Mathf.Sign(input);

        public override void _PhysicsProcess(float delta)
        {
            Vector2 there = ARVRCameraPosition, // where we're going
                forward = ARVRCameraDirection, // which way we're facing
                movement = Vector2.Zero; // movement vector from joystick and keyboard input
            bool keyPressed = false; // if true then we go max speed and ignore what the joysticks say.
            if (!(Input.IsKeyPressed((int)KeyList.Up) || Input.IsKeyPressed((int)KeyList.W)) || !(Input.IsKeyPressed((int)KeyList.Down) || Input.IsKeyPressed((int)KeyList.S)))
            { // Don't want to move this way if both keys are pressed at once.
                if (Input.IsKeyPressed((int)KeyList.Up) || Input.IsKeyPressed((int)KeyList.W))
                {
                    movement += forward;
                    keyPressed = true;
                }
                if (Input.IsKeyPressed((int)KeyList.Down) || Input.IsKeyPressed((int)KeyList.S))
                {
                    movement += forward.Rotated(Mathf.Pi);
                    keyPressed = true;
                }
            }
            if (!Input.IsKeyPressed((int)KeyList.A) || !Input.IsKeyPressed((int)KeyList.D))
            { // Don't want to move this way if both keys are pressed at once.
                if (Input.IsKeyPressed((int)KeyList.A))
                {
                    movement += forward.Rotated(Mathf.Pi / -2f);
                    keyPressed = true;
                }
                if (Input.IsKeyPressed((int)KeyList.D))
                {
                    movement += forward.Rotated(Mathf.Pi / 2f);
                    keyPressed = true;
                }
            }
            if (keyPressed)
                movement = movement.Normalized();
            else
            {
                Vector2 joystick = new Vector2(LeftController.GetJoystickAxis(1), LeftController.GetJoystickAxis(0));
                float strength = Strength(joystick.Length());
                if (Mathf.Abs(strength) > 1)
                    strength = Mathf.Sign(strength);
                if (Mathf.Abs(strength) > float.Epsilon)
                    movement += (joystick.Normalized() * strength).Rotated(forward.Angle());
            }

            there += movement * delta * (Shift ? Assets.WalkSpeed : Assets.RunSpeed);

            if (CanReallyWalk(there))
                PlayerPosition = there;

            // Move ARVROrigin so that camera global position matches player global position
            ARVROrigin.Transform = new Transform(
                ARVROrigin.Transform.basis.Orthonormalized(),
                new Vector3(
                    -ARVRCamera.Transform.origin.x,
                    Height,
                    -ARVRCamera.Transform.origin.z
                )
            );

            // Joystick and keyboard rotation
            float axis0 = -Strength(RightController.GetJoystickAxis(0));
            if (Input.IsKeyPressed((int)KeyList.Left))
                axis0 += 1;
            if (Input.IsKeyPressed((int)KeyList.Right))
                axis0 -= 1;
            if (Mathf.Abs(axis0) > float.Epsilon)
                Rotate(Godot.Vector3.Up, Mathf.Pi * delta * axis0);
            /*
            {
                Vector3 origHeadPos = ARVRCamera.GlobalTransform.origin;
                ARVROrigin.Rotate(Godot.Vector3.Up, Mathf.Pi * delta * (axis0 > 0f ? -1f : 1f));
                ARVROrigin.GlobalTransform = new Transform(ARVROrigin.GlobalTransform.basis, ARVROrigin.GlobalTransform.origin + origHeadPos - ARVRCamera.GlobalTransform.origin).Orthonormalized();
            }
            */
        }

        public static bool Shift => Input.IsKeyPressed((int)KeyList.Shift);

        public float Height => Roomscale ?
            0f
            : (float)Assets.HalfWallHeight - ARVRCamera.Transform.origin.y;

        public Vector2 ARVROriginPosition => Assets.Vector2(ARVROrigin.GlobalTransform.origin);
        public Vector2 ARVRCameraPosition => Assets.Vector2(ARVRCamera.GlobalTransform.origin);
        public Vector2 ARVRCameraDirection => -Assets.Vector2(ARVRCamera.GlobalTransform.basis.z).Normalized();
        public Vector2 ARVRCameraMovement => ARVRCameraPosition - Assets.Vector2(GlobalTransform.origin);

        public delegate bool CanWalkDelegate(Vector2 there);
        public CanWalkDelegate CanWalk { get; set; } = (Vector2 there) => true;

        public bool CanReallyWalk(Vector2 there)
        {
            foreach (Direction8 direction in Direction8.Diagonals)
                if (!CanWalk(there + direction.Vector2 * Assets.HeadDiagonal))
                    return false;
            return CanWalk(there);
        }

        public Vector2 PlayerPosition
        {
            get => Assets.Vector2(GlobalTransform.origin);
            set => GlobalTransform = new Transform(
                    GlobalTransform.basis,
                    new Vector3(
                        value.x,
                        0f,
                        value.y
                    )
                );
        }
    }
}
