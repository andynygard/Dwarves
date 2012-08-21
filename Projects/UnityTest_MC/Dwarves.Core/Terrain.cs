﻿// ----------------------------------------------------------------------------
// <copyright file="Terrain.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the terrain.
    /// </summary>
    public class Terrain
    {
        /// <summary>
        /// Initializes a new instance of the Terrain class.
        /// </summary>
        public Terrain()
        {
            this.Chunks = new Dictionary<Position, Chunk>();
        }

        /// <summary>
        /// Gets the currently active chunks.
        /// </summary>
        public Dictionary<Position, Chunk> Chunks { get; private set; }

        /// <summary>
        /// Get the index of the chunk at the given world coordinates.
        /// </summary>
        /// <param name="worldX">The x position.</param>
        /// <param name="worldY">The y position.</param>
        /// <returns>The chunk index.</returns>
        public static Position GetChunkIndex(int worldX, int worldY)
        {
            return new Position(worldX >> Chunk.LogSizeX, worldY >> Chunk.LogSizeY);
        }

        /// <summary>
        /// Get the voxel at the given world coorinates.
        /// </summary>
        /// <param name="worldX">The x position.</param>
        /// <param name="worldY">The y position.</param>
        /// <returns>The voxel.</returns>
        public Voxel GetVoxel(int worldX, int worldY)
        {
            Chunk chunk = this.Chunks[Terrain.GetChunkIndex(worldX, worldY)];
            return chunk.Voxels[Chunk.GetVoxelIndex(worldX & Chunk.MaskX, worldY & Chunk.MaskY)];
        }
    }
}