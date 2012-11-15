﻿// ----------------------------------------------------------------------------
// <copyright file="VoxelTerrain.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves.Core.Terrain
{
    using System.Collections.Generic;
    using Dwarves.Core.Geometry;
    using Dwarves.Core.Math;

    /// <summary>
    /// Represents the terrain.
    /// </summary>
    public class VoxelTerrain
    {
        /// <summary>
        /// Initialises a new instance of the VoxelTerrain class.
        /// </summary>
        public VoxelTerrain()
        {
            this.Voxels = new Dictionary<Vector2I, Voxel[]>();
            this.Meshes = new Dictionary<Vector2I, Mesh>();
        }

        /// <summary>
        /// Gets the voxel data organised by chunk.
        /// </summary>
        public Dictionary<Vector2I, Voxel[]> Voxels { get; private set; }

        /// <summary>
        /// Gets the mesh data organised by chunk.
        /// </summary>
        public Dictionary<Vector2I, Mesh> Meshes { get; private set; }
    }
}