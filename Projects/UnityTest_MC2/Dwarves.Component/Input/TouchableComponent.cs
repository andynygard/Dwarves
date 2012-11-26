﻿// ----------------------------------------------------------------------------
// <copyright file="TouchableComponent.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves.Component.Input
{
    using UnityEngine;

    /// <summary>
    /// A component that responds to being touched.
    /// </summary>
    public abstract class TouchableComponent : MonoBehaviour
    {
        /// <summary>
        /// Handles the on-touch behaviour for the component.
        /// </summary>
        /// <param name="hitPoint">The point at which the component was touched in world coordinates.</param>
        public abstract void ProcessTouch(Vector3 hitPoint);
    }
}