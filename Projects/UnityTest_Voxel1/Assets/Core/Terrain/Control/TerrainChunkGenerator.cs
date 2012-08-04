// ----------------------------------------------------------------------------
// <copyright file="TerrainChunkGenerator.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Responsible for dynamically generating terrain chunks.
/// </summary>
public class TerrainChunkGenerator
{
    /// <summary>
    /// Generate a chunk and return it. The terrain object remains unmodified.
    /// </summary>
    /// <param name="terrain">The terrain.</param>
    /// <param name="chunkIndex">The chunk index.</param>
    /// <returns>The chunk.</returns>
    public Chunk GenerateChunk(Terrain terrain, Vector2I chunkIndex)
    {
        Chunk chunk = new Chunk();

        // TODO: Implement Perlin Noise algorithm for generating terrain blocks. For now, just hardcode some junk
        for (int index = Chunk.Navigation.Start; index <= Chunk.Navigation.End; index++)
        {
            if (Random.value >= 0.1f)
            {
                chunk[index] = new Block(BlockType.Dirt);
            }
            else
            {
                chunk[index] = new Block(BlockType.None);
            }
        }

        return chunk;
    }
}