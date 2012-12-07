﻿// ----------------------------------------------------------------------------
// <copyright file="TerrainMeshBuilder.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves.Core.Terrain.Geometry
{
    using Dwarves.Core.Geometry;
    using Dwarves.Core.Math;

    /// <summary>
    /// Builds meshes for the terrain.
    /// </summary>
    public class TerrainMeshBuilder
    {
        /// <summary>
        /// Creates a mesh for the given chunk.
        /// </summary>
        /// <param name="terrain">The terrain.</param>
        /// <param name="chunk">The chunk index.</param>
        /// <returns>The mesh.</returns>
        public MeshData CreateMesh(VoxelTerrain terrain, Vector2I chunk)
        {
            var mesh = new MeshData();

            var chunkOrigin = terrain.GetChunkOrigin(chunk);
            for (int x = chunkOrigin.X; x < chunkOrigin.X + terrain.ChunkWidth; x++)
            {
                for (int y = chunkOrigin.Y; y < chunkOrigin.Y + terrain.ChunkHeight; y++)
                {
                    this.CreateMeshCell(terrain, chunk, new Vector2I(x, y), mesh);
                }
            }

            return mesh;
        }

        /// <summary>
        /// Create the cell at the given position.
        /// </summary>
        /// <param name="terrain">The terrain.</param>
        /// <param name="chunk">The chunk index.</param>
        /// <param name="position">The position.</param>
        /// <param name="mesh">The mesh data.</param>
        private void CreateMeshCell(VoxelTerrain terrain, Vector2I chunk, Vector2I position, MeshData mesh)
        {
            // Get the corner points in chunk coordinates
            var corner = terrain.WorldToChunk(position.X, position.Y);
            var cornerUp = terrain.WorldToChunk(position.X, position.Y + 1);
            var cornerRight = terrain.WorldToChunk(position.X + 1, position.Y);
            var cornerUpRight = terrain.WorldToChunk(position.X + 1, position.Y + 1);

            // Get the chunk indices for the corner points
            Vector2I chunkUp = chunk;
            Vector2I chunkRight = chunk;
            Vector2I chunkUpRight = chunk;
            if (cornerUp.Y == terrain.ChunkHeight - 1)
            {
                chunkUp.Y++;
                chunkUpRight.Y++;
            }

            if (cornerRight.X == 0)
            {
                chunkRight.X++;
                chunkUpRight.X++;
            }

            // Get the voxels for the corner points
            //Voxel voxel = terrain.Voxels[chunk][TerrainConst.VoxelIndex(corner)];
            //Voxel voxelUp = terrain.Voxels[chunkUp][TerrainConst.VoxelIndex(cornerUp)];
            //Voxel voxelRight = terrain.Voxels[chunkRight][TerrainConst.VoxelIndex(cornerRight)];
            //Voxel voxelUpRight = terrain.Voxels[chunkUpRight][TerrainConst.VoxelIndex(cornerUpRight)];

            // TODO
        }
    }
}