﻿// ----------------------------------------------------------------------------
// <copyright file="Chunk.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves.Core
{
    /// <summary>
    /// A 2D chunk of voxels.
    /// </summary>
    public class Chunk
    {
        /// <summary>
        /// The power-of-2 size of the chunk for quickly determining chunk index.
        /// </summary>
        public const byte LogSizeX = 4;

        /// <summary>
        /// The power-of-2 size of the chunk for quickly determining chunk index.
        /// </summary>
        public const byte LogSizeY = 4;

        /// <summary>
        /// The width of a chunk.
        /// </summary>
        public const int Width = 1 << LogSizeX;

        /// <summary>
        /// The height of a chunk.
        /// </summary>
        public const int Height = 1 << LogSizeY;

        /// <summary>
        /// Mask for converting from world coordinates to chunk coordinates.
        /// </summary>
        public const int MaskX = Width - 1;

        /// <summary>
        /// Mask for converting from world coordinates to chunk coordinates.
        /// </summary>
        public const int MaskY = Height - 1;

        /// <summary>
        /// Initializes a new instance of the Chunk class.
        /// </summary>
        public Chunk()
        {
            this.Voxels = new Voxel[Width * Height];
        }

        /// <summary>
        /// Gets the voxels in this chunk.
        /// </summary>
        public Voxel[] Voxels { get; private set; }

        /// <summary>
        /// Gets the array index for the voxel at the given chunk coordinates.
        /// </summary>
        /// <param name="chunkX">The x position.</param>
        /// <param name="chunkY">The y position.</param>
        /// <returns>The index.</returns>
        public static int GetVoxelIndex(int chunkX, int chunkY)
        {
            return chunkX + (chunkY * Width);
        }
    }
}