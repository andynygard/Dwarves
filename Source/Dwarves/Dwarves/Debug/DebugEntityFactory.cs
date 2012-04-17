﻿// ----------------------------------------------------------------------------
// <copyright file="DebugEntityFactory.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------
#if DEBUG
namespace Dwarves.Debug
{
    using Dwarves.Assembler.Body;
    using Dwarves.Component.Screen;
    using Dwarves.Component.Spatial;
    using EntitySystem;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Factory for creating commonly used entities.
    /// </summary>
    public class DebugEntityFactory
    {
        #region Public Methods

        /// <summary>
        /// Create a camera entity.
        /// </summary>
        /// <param name="world">The world context.</param>
        /// <param name="centerX">The X point for the center of the camera.</param>
        /// <param name="centerY">The Y point for the center of the camera.</param>
        /// <param name="zoom">The zoom ratio of the camera.</param>
        /// <returns>The entity.</returns>
        public Entity CreateCamera(WorldContext world, float centerX, float centerY, float zoom)
        {
            Entity entity = world.EntityManager.CreateEntity();

            // Create the components
            var camera = new CameraComponent();
            var position = new PositionComponent(new Vector2(centerX, centerY));
            var scale = new ScaleComponent(zoom);

            // Add the components
            world.EntityManager.AddComponent(entity, camera);
            world.EntityManager.AddComponent(entity, position);
            world.EntityManager.AddComponent(entity, scale);

            return entity;
        }

        /// <summary>
        /// Create a dwarf entity.
        /// </summary>
        /// <param name="world">The world context.</param>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <returns>The entity.</returns>
        public Entity CreateDwarf(WorldContext world, float x, float y)
        {
            Entity entity = world.EntityManager.CreateEntity();

            var neckJoint = new HumanoidAssemblerArgs.RevoluteJoint(
                new Vector2(0.0f, 11.0f) * DwarfConst.PixelsToMeters,
                true,
                -MathHelper.Pi / 4,
                MathHelper.Pi / 4,
                false,
                0.0f);
            var shoulderJoint = new HumanoidAssemblerArgs.RevoluteJoint(
                new Vector2(-2.5f, 8.0f) * DwarfConst.PixelsToMeters,
                true,
                -MathHelper.Pi / 4,
                MathHelper.Pi / 4,
                false,
                0.0f);
            var hipJoint = new HumanoidAssemblerArgs.RevoluteJoint(
                new Vector2(-2.0f, 3.0f) * DwarfConst.PixelsToMeters,
                true,
                -MathHelper.Pi / 4,
                MathHelper.Pi / 4,
                false,
                0.0f);

            var args = new HumanoidAssemblerArgs(
                "dwarf",
                DwarfConst.CollisionGroupDwarf,
                new Vector2(x, y),                                      // Body position
                new Vector2(4.0f, 0.0f) * DwarfConst.PixelsToMeters,    // Right-left offset
                new Vector2(-5.0f, 11.0f) * DwarfConst.PixelsToMeters,  // Torso
                new Vector2(-2.0f, 19.0f) * DwarfConst.PixelsToMeters,  // Head
                new Vector2(-3.0f, 9.0f) * DwarfConst.PixelsToMeters,   // Arm
                new Vector2(-2.0f, 3.0f) * DwarfConst.PixelsToMeters,   // Leg
                new Vector2(-3.0f, 20.0f) * DwarfConst.PixelsToMeters,  // Beard
                neckJoint,                                              // Neck joint
                shoulderJoint,                                          // Shoulder joint
                hipJoint);                                              // Hip joint

            var bodyAssembler = new HumanoidAssembler(world);
            bodyAssembler.AssembleBody(entity, args);

            // TODO
            return entity;
        }

        #endregion
    }
}
#endif