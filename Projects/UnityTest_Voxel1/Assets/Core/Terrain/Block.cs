﻿/// <summary>
/// Represents a terrain block.
/// </summary>
public struct Block
{
    /// <summary>
    /// An unknown block.
    /// </summary>
    public static readonly Block Unknown = new Block(BlockType.Unknown);

    /// <summary>
    /// Initializes a new instance of the Block struct.
    /// </summary>
    /// <param name="blockType">The block type.</param>
    public Block(BlockType blockType)
        : this()
    {
        this.BlockType = blockType;
    }

    /// <summary>
    /// Gets or sets the block type.
    /// </summary>
    public BlockType BlockType { get; set; }
}