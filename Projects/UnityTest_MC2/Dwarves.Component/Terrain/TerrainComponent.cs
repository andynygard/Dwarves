﻿// ----------------------------------------------------------------------------
// <copyright file="TerrainComponent.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves.Component.Terrain
{
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
    }
}