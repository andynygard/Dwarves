﻿/// <summary>
/// Represents a 2D region of terrain blocks.
/// </summary>
public class Chunk
{
    #region Constants

    /// <summary>
    /// The power-of-2 size of the chunk for quickly determining chunk index.
    /// </summary>
    public const byte LogSizeX = 5;

    /// <summary>
    /// The power-of-2 size of the chunk for quickly determining chunk index.
    /// </summary>
    public const byte LogSizeY = 5;

    /// <summary>
    /// The block width of a chunk.
    /// </summary>
    public const int SizeX = 1 << LogSizeX;

    /// <summary>
    /// The block height of a chunk.
    /// </summary>
    public const int SizeY = 1 << LogSizeY;

    /// <summary>
    /// Mask for performing bitwise modulus operations.
    /// </summary>
    public const int MaskX = SizeX - 1;

    /// <summary>
    /// Mask for performing bitwise modulus operations.
    /// </summary>
    public const int MaskY = SizeY - 1;

    #endregion

    /// <summary>
    /// Initializes a new instance of the Chunk class.
    /// </summary>
    public Chunk()
    {
        this.Blocks = new Block[SizeX * SizeY];
    }

    /// <summary>
    /// Gets or sets the block at the given position in chunk-space.
    /// </summary>
    /// <param name="chunkX">The x position.</param>
    /// <param name="chunkY">The y position.</param>
    /// <returns>The block.</returns>
    public Block this[int chunkX, int chunkY]
    {
        get
        {
            return this.Blocks[Chunk.GetBlockIndex(chunkX, chunkY)];
        }

        set
        {
            this.Blocks[Chunk.GetBlockIndex(chunkX, chunkY)] = value;
        }
    }

    /// <summary>
    /// Gets the blocks in this chunk. This is a flattened 2D array.
    /// </summary>
    public Block[] Blocks { get; private set; }

    /// <summary>
    /// Gets the array index for the block at the given x and y position.
    /// </summary>
    /// <param name="x">The x position.</param>
    /// <param name="y">The y position.</param>
    /// <returns>The index.</returns>
    public static int GetBlockIndex(int x, int y)
    {
        return x + y * SizeX;
    }

    /// <summary>
    /// Try to get the block at the given position in chunk-space.
    /// </summary>
    /// <param name="chunkX">The x position.</param>
    /// <param name="chunkY">The y position.</param>
    /// <param name="block">The block.</param>
    /// <returns>True if the block was retrieved.</returns>
    public bool TryGetBlock(int chunkX, int chunkY, out Block block)
    {
        int blockIndex = Chunk.GetBlockIndex(chunkX, chunkY);
        if (blockIndex >= 0 && blockIndex < this.Blocks.Length)
        {
            block = this.Blocks[blockIndex];
            return true;
        }
        else
        {
            block = null;
            return false;
        }
    }

    /// <summary>
    /// Provides constants for navigating the chunk's block array.
    /// </summary>
    public static class Navigation
    {
        /// <summary>
        /// Move the index to the block above.
        /// </summary>
        public const short Up = -Chunk.SizeX;

        /// <summary>
        /// Move the index to the block below.
        /// </summary>
        public const short Down = Chunk.SizeX;

        /// <summary>
        /// Move the index to the block on the left.
        /// </summary>
        public const short Left = -1;

        /// <summary>
        /// Move the index to the block on the right.
        /// </summary>
        public const short Right = 1;
    }
}