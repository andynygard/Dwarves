﻿// ----------------------------------------------------------------------------
// <copyright file="TerrainGenerator.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves.Core.Terrain.Generation
{
    using Dwarves.Core.Math;
    using Dwarves.Core.Math.Noise;
    using Dwarves.Core.Terrain.Engine;
    using UnityEngine;

    /// <summary>
    /// Generates voxel terrain.
    /// </summary>
    public class TerrainGenerator
    {
        /// <summary>
        /// Initialises a new instance of the TerrainGenerator class.
        /// </summary>
        /// <param name="terrain">The terrain.</param>
        /// <param name="noiseGenerator">The noise generator.</param>
        /// <param name="surfaceAmplitude">The distance from the mean surface height that the terrain oscillates.
        /// </param>
        public TerrainGenerator(VoxelTerrain terrain, INoiseGenerator noiseGenerator, int surfaceAmplitude)
        {
            this.Terrain = terrain;
            this.NoiseGenerator = noiseGenerator;
            this.SurfaceAmplitude = surfaceAmplitude;
        }

        /// <summary>
        /// Gets the terrain.
        /// </summary>
        public VoxelTerrain Terrain { get; private set; }

        /// <summary>
        /// Gets or sets the noise generator.
        /// </summary>
        public INoiseGenerator NoiseGenerator { get; set; }

        /// <summary>
        /// Gets or sets the distance from the mean surface height that the terrain oscillates.
        /// </summary>
        public int SurfaceAmplitude { get; set; }

        /// <summary>
        /// Generates the voxel terrain for the given chunk.
        /// </summary>
        /// <param name="chunk">The chunk index.</param>
        public void Generate(Vector2I chunk)
        {
            // Create the voxel chunk if it doesn't yet exist
            if (!this.Terrain.ContainsChunk(chunk))
            {
                this.Terrain.NewChunk(chunk);
            }

            // Generate the surface heights for this chunk if they don't exist
            if (!this.Terrain.SurfaceHeights.ContainsKey(chunk.X))
            {
                this.Terrain.SurfaceHeights.Add(chunk.X, this.GenerateSurfaceHeights(chunk.X));
            }

            // Fill the terrain
            this.FillTerrain(chunk);

            // Flag the chunk as requiring a rebuild
            this.Terrain.GetChunk(chunk).RebuildRequired = true;
        }

        /// <summary>
        /// Generate the heights for each x-coordinate for the given chunk.
        /// </summary>
        /// <param name="chunkIndexX">The chunk index's x component.</param>
        /// <returns>The surface heights.</returns>
        private float[] GenerateSurfaceHeights(int chunkIndexX)
        {
            var heights = new float[this.Terrain.ChunkWidth];

            int originX = chunkIndexX * this.Terrain.ChunkWidth;
            for (int x = 0; x < this.Terrain.ChunkWidth; x++)
            {
                // Generate the noise value at this x position
                float noise = this.NoiseGenerator.Generate(originX + x);

                // Obtain the height by scaling the noise with the surface amplitude
                heights[x] = noise * this.SurfaceAmplitude;
            }

            return heights;
        }

        /// <summary>
        /// Fill the terrain.
        /// </summary>
        /// <param name="chunk">The chunk index.</param>
        private void FillTerrain(Vector2I chunk)
        {
            // Get the voxels and surface heights
            IVoxels voxels = this.Terrain.GetChunk(chunk);
            float[] surfaceHeights = this.Terrain.SurfaceHeights[chunk.X];

            // Fill the voxel array
            int originY = chunk.Y * this.Terrain.ChunkHeight;
            for (int x = 0; x < this.Terrain.ChunkWidth; x++)
            {
                // Determine where the surface lies for this x-value
                float surfaceHeightF = surfaceHeights[x];
                int surfaceHeightI = (int)System.Math.Floor(surfaceHeightF);
                float deltaHeight = surfaceHeightF - surfaceHeightI;

                for (int y = 0; y < this.Terrain.ChunkHeight; y++)
                {
                    int height = originY + y;

                    // Create the voxel at this point
                    Voxel voxel;
                    if (height > surfaceHeightI)
                    {
                        // This voxel lies above the surface
                        voxel = Voxel.Air;
                    }
                    else
                    {
                        // Determine the material
                        var material = TerrainMaterial.Dirt;

                        if (height == surfaceHeightI)
                        {
                            // This voxel lies on the surface, so scale the density by the noise value
                            byte density = (byte)(Voxel.DensityMax - (Voxel.DensityMax * deltaHeight));
                            voxel = new Voxel(material, density);
                        }
                        else
                        {
                            // The voxel lies under the surface
                            voxel = new Voxel(material, Voxel.DensityMin);
                        }
                    }

                    // Set the voxel at each depth point
                    for (int z = 0; z < this.Terrain.ChunkDepth; z++)
                    {
                        voxels[x, y, z] = z > 0 && z < this.Terrain.ChunkDepth - 1 ? voxel : Voxel.Air;
                    }
                }
            }
        }
    }
}