﻿// ----------------------------------------------------------------------------
// <copyright file="TerrainComponent.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves.Component.Terrain
{
    using System;
    using System.Collections.Generic;
    using Dwarves.Component.Bounds;
    using Dwarves.Core.Math;
    using Dwarves.Core.Math.Noise;
    using Dwarves.Core.Terrain;
    using Dwarves.Core.Terrain.Generation;
    using Dwarves.Core.Terrain.Mutation;
    using Dwarves.Core.Terrain.Serialization;
    using UnityEngine;

    /// <summary>
    /// Terrain component.
    /// </summary>
    public class TerrainComponent : MonoBehaviour
    {
        /// <summary>
        /// The distance from the mean surface height that the terrain oscillates.
        /// </summary>
        public int SurfaceAmplitude;

        /// <summary>
        /// The seed value used by the terrain generator.
        /// </summary>
        public int Seed;

        /// <summary>
        /// The number of octaves of noise used by the terrain generator.
        /// </summary>
        public int Octaves;

        /// <summary>
        /// The base frequency which is the frequency of the lowest octave used by the terrain generator.
        /// </summary>
        public float BaseFrequency;

        /// <summary>
        /// The persistence value, which determines the amplitude for each octave used by the terrain generator.
        /// </summary>
        public float Persistence;

        /// <summary>
        /// Bitwise values indicating chunk border.
        /// </summary>
        [Flags]
        private enum ChunkNeighbour
        {
            /// <summary>
            /// The neighbour to the top.
            /// </summary>
            Top = 1,

            /// <summary>
            /// The neighbour to the right.
            /// </summary>
            Right = 2
        }

        /// <summary>
        /// Gets the terrain instance.
        /// </summary>
        public VoxelTerrain Terrain { get; private set; }

        /// <summary>
        /// Gets the terrain serialiser.
        /// </summary>
        public TerrainSerialiser TerrainSerialiser { get; private set; }

        /// <summary>
        /// Gets the terrain generator.
        /// </summary>
        public TerrainGenerator TerrainGenerator { get; private set; }

        /// <summary>
        /// Gets the terrain mutator.
        /// </summary>
        public TerrainMutator TerrainMutator { get; private set; }

        /// <summary>
        /// Initialises the component.
        /// </summary>
        public void Start()
        {
            this.Terrain = new VoxelTerrain();

            // Initialise the serialiser
            this.TerrainSerialiser = new TerrainSerialiser();

            // Initialise the terrain generator
            var simplexGenerator = new SimplexNoiseGenerator();
            var noiseGenerator = new CompoundNoiseGenerator(
                simplexGenerator, this.Seed, (byte)this.Octaves, this.BaseFrequency, this.Persistence);
            this.TerrainGenerator = new TerrainGenerator(noiseGenerator, this.SurfaceAmplitude);

            // Initialise the mutator
            this.TerrainMutator = new TerrainMutator();
        }

        /// <summary>
        /// Called once per frame.
        /// </summary>
        public void Update()
        {
            this.LoadUnloadActorChunks();
        }

        /// <summary>
        /// Check the bounds of each actor in the game world and load/unload the chunks that are new/no longer required.
        /// </summary>
        private void LoadUnloadActorChunks()
        {
            // Determine which chunks are currently active
            var activeChunks = new HashSet<Vector2I>();
            foreach (ActorComponent actor in GameObject.FindObjectsOfType(typeof(ActorComponent)))
            {
                // Get the chunk-bounds of the actor
                RectangleI bounds = actor.GetChunkBounds();

                // Step through each chunk index in the actor bounds
                for (int x = bounds.X; x < bounds.Right; x++)
                {
                    for (int y = bounds.Y; y > bounds.Bottom; y--)
                    {
                        activeChunks.Add(new Vector2I(x, y));
                    }
                }
            }

            // Check if any chunks are now off screen with no actors within and will need to be removed
            var toRemove = new HashSet<Vector2I>();
            foreach (Vector2I chunk in this.Terrain.Voxels.Keys)
            {
                if (!activeChunks.Contains(chunk))
                {
                    toRemove.Add(chunk);
                }
            }

            // Remove data for chunks that are no longer in active
            foreach (Vector2I chunk in toRemove)
            {
                // Remove the data
                this.Terrain.RemoveChunkData(chunk);

                // Remove the chunk game object
                Transform chunkTransform = this.transform.FindChild(TerrainChunkComponent.GetLabel(chunk));
                if (chunkTransform != null)
                {
                    GameObject.Destroy(chunkTransform.gameObject);
                }
            }

            // Load the new chunk data
            foreach (Vector2I chunk in activeChunks)
            {
                if (!this.Terrain.Voxels.ContainsKey(chunk))
                {
                    // Attempt to deserialise the chunk
                    if (!this.TerrainSerialiser.TryDeserialise(this.Terrain, chunk))
                    {
                        // The chunk doesn't exist to be serialised, so generate it from scratch
                        this.TerrainGenerator.Generate(this.Terrain, chunk);
                    }

                    // Remove any neighbouring meshes that depend on this so that they can be rebuilt
                    this.Terrain.Meshes.Remove(new Vector2I(chunk.X, chunk.Y - 1));
                    this.Terrain.Meshes.Remove(new Vector2I(chunk.X - 1, chunk.Y));

                    // Create the chunk game object
                    var chunkObject = new GameObject(TerrainChunkComponent.GetLabel(chunk));
                    chunkObject.transform.parent = this.transform;
                    TerrainChunkComponent chunkComponent = chunkObject.AddComponent<TerrainChunkComponent>();
                    chunkComponent.Terrain = this.Terrain;
                    chunkComponent.Chunk = chunk;
                }
            }
        }
    }
}