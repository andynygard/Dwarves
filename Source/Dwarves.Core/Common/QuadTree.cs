﻿// ----------------------------------------------------------------------------
// <copyright file="TightQuadTree.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves.Common
{
    using System.Collections.Generic;

    /// <summary>
    /// A 2D position-based quadtree which places items in the smallest possible quadrants (hence 'tight').
    /// </summary>
    /// <typeparam name="T">The type of data stored in the quad tree.</typeparam>
    public class TightQuadTree<T>
    {
        #region Private Variables

        /// <summary>
        /// The items in this quadrant.
        /// </summary>
        private List<QuadTreeItem<T>> items;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the TightQuadTree class.
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        public TightQuadTree(RectangleF rectangle)
        {
            this.Rectangle = rectangle;
            this.items = new List<QuadTreeItem<T>>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the rectangle.
        /// </summary>
        public RectangleF Rectangle { get; private set; }

        /// <summary>
        /// Gets the top-left quadrant.
        /// </summary>
        public TightQuadTree<T> TopLeft { get; private set; }

        /// <summary>
        /// Gets the top-right quadrant.
        /// </summary>
        public TightQuadTree<T> TopRight { get; private set; }

        /// <summary>
        /// Gets the bottom-left quadrant.
        /// </summary>
        public TightQuadTree<T> BottomLeft { get; private set; }

        /// <summary>
        /// Gets the bottom-right quadrant.
        /// </summary>
        public TightQuadTree<T> BottomRight { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this node is a leaf (has no child quadrants).
        /// </summary>
        public bool IsLeaf
        {
            get { return this.TopLeft == null; }
        }

        /// <summary>
        /// Gets the number of leaves contained by the node.
        /// </summary>
        public int LeafCount
        {
            get
            {
                if (this.IsLeaf)
                {
                    return 1;
                }
                else
                {
                    return
                        this.TopLeft.LeafCount +
                        this.TopRight.LeafCount +
                        this.BottomLeft.LeafCount +
                        this.BottomRight.LeafCount;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Insert an item to the quad tree into the smallest quadrant size possible for the given bounds.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <param name="bounds">The bounds of the item.</param>
        /// <returns>True if the item was inserted; False if the item does not fit into the quadrant.</returns>
        public bool Insert(T item, RectangleF bounds)
        {
            return this.Insert(new QuadTreeItem<T>(item, bounds));
        }

        /// <summary>
        /// Insert an item to the quad tree into the smallest quadrant size possible for the given bounds.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <returns>True if the item was inserted; False if the item does not fit into the quadrant.</returns>
        public bool Insert(QuadTreeItem<T> item)
        {
            if (!this.Rectangle.Contains(item.Bounds))
            {
                // Item bounds exceed this quadrant's bounds
                return false;
            }

            // Attempt to insert the item into a child quadrant
            if (!this.InsertInChild(item))
            {
                // The item doesn't fit into a child quadrant, so this is the best fit possible
                this.items.Add(item);
            }

            return true;
        }

        /// <summary>
        /// Gets the items contained within the given bounds.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        /// <returns>The items contained in the bounds.</returns>
        public QuadTreeItem<T>[] GetItems(RectangleF bounds)
        {
            if (!this.Rectangle.Intersects(bounds))
            {
                // Bounds does not intersect this rectangle
                return new QuadTreeItem<T>[0];
            }

            if (this.IsLeaf)
            {
                return this.items.ToArray();
            }
            else
            {
                QuadTreeItem<T>[] itemsQ1 = this.TopLeft.GetItems(bounds);
                QuadTreeItem<T>[] itemsQ2 = this.TopRight.GetItems(bounds);
                QuadTreeItem<T>[] itemsQ3 = this.BottomLeft.GetItems(bounds);
                QuadTreeItem<T>[] itemsQ4 = this.BottomRight.GetItems(bounds);

                var items = new QuadTreeItem<T>[itemsQ1.Length + itemsQ2.Length + itemsQ3.Length + itemsQ4.Length];
                int index = 0;
                itemsQ1.CopyTo(items, index);
                index += itemsQ1.Length;
                itemsQ2.CopyTo(items, index);
                index += itemsQ2.Length;
                itemsQ3.CopyTo(items, index);
                index += itemsQ3.Length;
                itemsQ4.CopyTo(items, index);

                return items;
            }
        }

        #endregion

        #region Insert

        /// <summary>
        /// Attempt to insert the item into a child quadrant for the given bounds.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <returns>True if the item was inserted; False if the item does not fit into any child quadrants.</returns>
        protected bool InsertInChild(QuadTreeItem<T> item)
        {
            if (!this.IsLeaf)
            {
                // This is not a leaf node so attempt to insert into each child quadrant
                return
                    this.TopLeft.Insert(item) ||
                    this.TopRight.Insert(item) ||
                    this.BottomLeft.Insert(item) ||
                    this.BottomRight.Insert(item);
            }
            else
            {
                // This is a leaf node so test if this node could be split further to more tightly accomodate the item
                if (this.Rectangle.GetTopLeftQuadrant().Contains(item.Bounds))
                {
                    this.Split();
                    return this.TopLeft.Insert(item);
                }
                else if (this.Rectangle.GetTopRightQuadrant().Contains(item.Bounds))
                {
                    this.Split();
                    return this.TopRight.Insert(item);
                }
                else if (this.Rectangle.GetBottomLeftQuadrant().Contains(item.Bounds))
                {
                    this.Split();
                    return this.BottomLeft.Insert(item);
                }
                else if (this.Rectangle.GetBottomRightQuadrant().Contains(item.Bounds))
                {
                    this.Split();
                    return this.BottomRight.Insert(item);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Split the rectangle into four even sub-quadrants.
        /// </summary>
        protected void Split()
        {
            this.TopLeft = new TightQuadTree<T>(this.Rectangle.GetTopLeftQuadrant());
            this.TopRight = new TightQuadTree<T>(this.Rectangle.GetTopRightQuadrant());
            this.BottomLeft = new TightQuadTree<T>(this.Rectangle.GetBottomLeftQuadrant());
            this.BottomRight = new TightQuadTree<T>(this.Rectangle.GetBottomRightQuadrant());
        }

        #endregion

        #region Get

        #endregion
    }
}