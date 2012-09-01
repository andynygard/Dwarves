﻿// ----------------------------------------------------------------------------
// <copyright file="ChunkLoader.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves.Core.Terrain.Load
{
    using Dwarves.Core.Terrain.Generation;
    using UnityEngine;

    /// <summary>
    /// Responsible for loading and unloading terrain chunks.
    /// </summary>
    public class ChunkLoader
    {
        /// <summary>
        /// Serializes and deserializes terrain chunks.
        /// </summary>
        private ChunkSerializer serializer;

        /// <summary>
        /// Dynamically generates chunk voxels.
        /// </summary>
        private ChunkVoxelGenerator generator;

        /// <summary>
        /// Initializes a new instance of the ChunkLoader class.
        /// </summary>
        /// <param name="seed">The seed value for generated chunks.</param>
        public ChunkLoader(float seed)
        {
            this.serializer = new ChunkSerializer();
            this.generator = new ChunkVoxelGenerator(seed);
        }

        /// <summary>
        /// Gets the seed value for generated chunks.
        /// </summary>
        public float Seed
        {
            get
            {
                return this.generator.Seed;
            }
        }

        /// <summary>
        /// Load a chunk into the terrain object.
        /// </summary>
        /// <param name="terrain">The terrain.</param>
        /// <param name="chunkIndex">The chunk index.</param>
        /// <returns>The chunk that was loaded.</returns>
        public Chunk LoadChunk(VoxelTerrain terrain, Position chunkIndex)
        {
            // Deserialize or generate the chunk
            Chunk chunk;
            if (!this.serializer.TryDeserializeChunk(chunkIndex, out chunk))
            {
                // The chunk doesn't yet exist, so generate a new one
                chunk = new Chunk();
                this.generator.Generate(chunk.Voxels, chunkIndex);

                // Serialize the generated chunk
                this.serializer.SerializeChunk(chunk, chunkIndex);
            }

            // Add the chunk from the Terrain object
            terrain.AddChunk(chunk, chunkIndex);

            return chunk;
        }

        /// <summary>
        /// Unload a chunk from the terrain object.
        /// </summary>
        /// <param name="terrain">The terrain.</param>
        /// <param name="chunkIndex">The chunk index.</param>
        /// <returns>The chunk that was unloaded; Null if the chunk was never loaded.</returns>
        public Chunk UnloadChunk(VoxelTerrain terrain, Position chunkIndex)
        {
            // Get the chunk
            Chunk chunk;
            if (!terrain.TryGetChunk(chunkIndex, out chunk))
            {
                // The chunk isn't loaded so do nothing
                return null;
            }

            // Serialize the chunk
            this.serializer.SerializeChunk(chunk, chunkIndex);

            // Remove the chunk from the Terrain object
            terrain.RemoveChunk(chunkIndex);

            return chunk;
        }
    }
}