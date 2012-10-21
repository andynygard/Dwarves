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
        /// Serializes and deserialises terrain chunks.
        /// </summary>
        private ChunkSerialiser serializer;

        /// <summary>
        /// Dynamically generates chunk voxels.
        /// </summary>
        private ChunkVoxelGenerator voxelGenerator;

        /// <summary>
        /// Initialises a new instance of the ChunkLoader class.
        /// </summary>
        /// <param name="seed">The seed value for generated chunks.</param>
        public ChunkLoader(float seed)
        {
            this.serializer = new ChunkSerialiser();
            this.voxelGenerator = new ChunkVoxelGenerator(seed);
        }

        /// <summary>
        /// Gets the seed value for generated chunks.
        /// </summary>
        public float Seed
        {
            get
            {
                return this.voxelGenerator.Seed;
            }
        }

        /// <summary>
        /// Load a chunk into the terrain object.
        /// </summary>
        /// <param name="terrain">The terrain.</param>
        /// <param name="chunkIndex">The chunk index.</param>
        /// <returns>The chunk.</returns>
        public Chunk LoadChunk(VoxelTerrain terrain, Position chunkIndex)
        {
            // Deserialize or generate the chunk
            Chunk chunk;
            if (!this.serializer.TryDeserialiseChunk(chunkIndex, out chunk))
            {
                // The chunk doesn't yet exist, so generate a new one
                chunk = new Chunk();
                this.voxelGenerator.Generate(chunk.Voxels, chunkIndex);

                // Serialize the generated chunk
                this.serializer.SerialiseChunk(chunk, chunkIndex);
            }

            // Add the chunk to the terrain object
            terrain.Chunks.Add(chunkIndex, chunk);

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
            if (!terrain.Chunks.TryGetValue(chunkIndex, out chunk))
            {
                // The chunk isn't loaded so do nothing
                return null;
            }

            // Serialize the chunk
            this.serializer.SerialiseChunk(chunk, chunkIndex);

            // Remove the chunk from the Terrain object
            terrain.Chunks.Remove(chunkIndex);

            return chunk;
        }
    }
}