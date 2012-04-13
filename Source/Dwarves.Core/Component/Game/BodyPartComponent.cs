﻿// ----------------------------------------------------------------------------
// <copyright file="BodyPartComponent.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves.Component.Game
{
    using System.Collections.Generic;
    using EntitySystem;
    using EntitySystem.Component;
    using FarseerPhysics.Dynamics.Joints;

    /// <summary>
    /// The body part types.
    /// </summary>
    public enum BodyPart
    {
        /// <summary>
        /// Body part.
        /// </summary>
        Head,

        /// <summary>
        /// Body part.
        /// </summary>
        Torso,

        /// <summary>
        /// Body part.
        /// </summary>
        Arm,

        /// <summary>
        /// Body part.
        /// </summary>
        Leg,

        /// <summary>
        /// Body part.
        /// </summary>
        Beard
    }

    /// <summary>
    /// Represents a body part belonging to a parent entity.
    /// </summary>
    public class BodyPartComponent : IComponent
    {
        /// <summary>
        /// Initializes a new instance of the BodyPartComponent class.
        /// </summary>
        /// <param name="parentEntity">The parent entity.</param>
        /// <param name="bodyPart">The body part type.</param>
        public BodyPartComponent(Entity parentEntity, BodyPart bodyPart)
        {
            this.ParentEntity = parentEntity;
            this.BodyPart = bodyPart;
            this.Joints = new List<Joint>();
        }

        /// <summary>
        /// Gets the parent entity.
        /// </summary>
        public Entity ParentEntity { get; private set; }

        /// <summary>
        /// Gets the body part type.
        /// </summary>
        public BodyPart BodyPart { get; private set; }

        /// <summary>
        /// Gets the body part joints.
        /// </summary>
        public List<Joint> Joints { get; private set; }
    }
}