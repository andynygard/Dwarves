﻿// ----------------------------------------------------------------------------
// <copyright file="CachedRows.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves.Core.Terrain.Geometry
{
    using Dwarves.Core.Geometry;
    using Dwarves.Core.Math;

    /// <summary>
    /// Reference to the vertices of the cells in the current and previous rows.
    /// </summary>
    public class CachedRows
    {
        /// <summary>
        /// Initialises a new instance of the CachedRows class.
        /// </summary>
        public CachedRows()
        {
            this.Cells = new CachedCell[2][];
            this.Cells[0] = new CachedCell[TerrainConst.ChunkWidth];
            this.Cells[1] = new CachedCell[TerrainConst.ChunkWidth];
        }

        /// <summary>
        /// Gets the cached cells. 1st dimension indicates row index; 2nd indicates x position of cell.
        /// </summary>
        public CachedCell[][] Cells { get; private set; }

        /// <summary>
        /// Gets the cached cell at the position in the direction relative to the given position.
        /// </summary>
        /// <param name="x">The current x position.</param>
        /// <param name="y">The current y position.</param>
        /// <param name="direction">The bitmask indicating the directional of the cached cell.</param>
        /// <returns>The cached cell.</returns>
        public CachedCell GetCachedCell(int x, int y, byte direction)
        {
            // Get the directional offset
            int rx = direction & 0x01;
            int ry = (direction >> 1) & 0x01;

            // Offset the reused cell from the given position
            int dx = x - rx;
            int dy = y - ry;

            return this.Cells[dy & 1][dx];
        }

        /// <summary>
        /// Set the cached cell at the given position.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="cell">The cached cell.</param>
        public void SetCachedCell(int x, int y, CachedCell cell)
        {
            this.Cells[y & 1][x] = cell;
        }
    }
}