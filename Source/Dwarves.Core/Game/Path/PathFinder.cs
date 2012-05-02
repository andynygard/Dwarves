﻿// ----------------------------------------------------------------------------
// <copyright file="PathFinder.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves.Game.Path
{
    using System;
    using System.Collections.Generic;
    using Dwarves.Common;
    using Dwarves.Component.Game;
    using Dwarves.Game.Terrain;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Responsible for determining paths between points in the game terrain using the A* algorithm.
    /// </summary>
    public class PathFinder
    {
        /// <summary>
        /// The cost for a normal up/down/left/right step.
        /// </summary>
        private const int SquareStepCost = 10;

        /// <summary>
        /// The cost for a normal diagonal step.
        /// </summary>
        private const int DiagonalStepCost = 14;

        /// <summary>
        /// The ordered open list.
        /// </summary>
        private PriorityQueue<AStarNode> openSetOrdered;

        /// <summary>
        /// The open list mapped by point coordinates.
        /// </summary>
        private Dictionary<Point, AStarNode> openSet;

        /// <summary>
        /// The closed list mapped by point coordinates.
        /// </summary>
        private Dictionary<Point, AStarNode> closedSet;

        /// <summary>
        /// The goal node.
        /// </summary>
        private PathNode goal;

        /// <summary>
        /// Initializes a new instance of the PathFinder class.
        /// </summary>
        /// <param name="terrain">The terrain to be traversed.</param>
        public PathFinder(TerrainComponent terrain)
        {
            this.Terrain = terrain;
        }

        /// <summary>
        /// Gets or sets the terrain to be traversed.
        /// </summary>
        public TerrainComponent Terrain { get; set; }

        /// <summary>
        /// Find the path between the two given points.
        /// </summary>
        /// <param name="start">The start point of the path to search from.</param>
        /// <param name="goal">The goal point of the path.</param>
        /// <param name="nodeWidth">The width of each node along the path in terrain units.</param>
        /// <param name="nodeHeight">The height in of each node along the path in terrain units.</param>
        /// <param name="path">The array of points.</param>
        /// <returns>True if a path was established.</returns>
        public bool FindPath(Point start, Point goal, int nodeWidth, int nodeHeight, out Point[] path)
        {
            // Reset open/closed lists
            this.openSetOrdered = new PriorityQueue<AStarNode>(Comparer<AStarNode>.Default);
            this.openSet = new Dictionary<Point, AStarNode>();
            this.closedSet = new Dictionary<Point, AStarNode>();

            // Get the path node for the start and goal points. Also test that the start and goal points have valid
            // rectangles
            PathNode startNode;
            if (!this.Terrain.PathNodes.TryGetValue(start, out startNode) ||
                !this.Terrain.PathNodes.TryGetValue(goal, out this.goal) ||
                !IsOpenSpace(this.GetNodeRectangle(start, nodeWidth, nodeHeight)) ||
                !IsOpenSpace(this.GetNodeRectangle(goal, nodeWidth, nodeHeight)))
            {
                path = new Point[0];
                return false;
            }

            // Add the start node to the open list
            this.AddOpenNode(new AStarNode(startNode, this.CalculateH(start), 0));

            // Search for the path to the goal node
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            AStarNode goalNode = this.FindGoalNode(nodeWidth, nodeHeight);
            watch.Stop();
            System.Console.Write("Test path took " + watch.ElapsedMilliseconds + "ms.");

            // If the goal node was found, iterate backwards through the parent nodes for the path
            if (goalNode != null)
            {
                var pathList = new List<Point>();

                AStarNode current = goalNode;
                while (current != null)
                {
                    pathList.Add(current.PathNode.Point);
                    current = current.Parent;
                }

                path = pathList.ToArray();
                return true;
            }
            else
            {
                path = new Point[0];
                return false;
            }
        }

        /// <summary>
        /// Find the goal node from the existing open set.
        /// </summary>
        /// <param name="nodeWidth">The width of each node along the path in terrain units.</param>
        /// <param name="nodeHeight">The height in of each node along the path in terrain units.</param>
        /// <returns>The goal node.</returns>
        private AStarNode FindGoalNode(int nodeWidth, int nodeHeight)
        {
            AStarNode goalNode = null;

            // Keep searching the open list
            while (this.openSetOrdered.Count > 0)
            {
                // Take the node from the open list with the lowest cost and move to the closed list
                AStarNode current = this.openSetOrdered.Pop();
                this.openSet.Remove(current.PathNode.Point);
                this.closedSet.Add(current.PathNode.Point, current);

                // The goal node was just added to the closed list so the search is complete
                if (current.PathNode.Point.Equals(this.goal.Point))
                {
                    goalNode = current;
                    break;
                }

                // Add the adjacent nodes to the open list
                foreach (PathNode adjacentNode in current.PathNode.AdjacentNodes)
                {
                    // Add the left node (if it isn't already in the closed list or blocked)
                    if (!this.closedSet.ContainsKey(adjacentNode.Point))
                    {
                        if (this.IsOpenSpace(this.GetNodeRectangle(adjacentNode.Point, nodeWidth, nodeHeight)))
                        {
                            // Calculate the H and G value for the adjacent node
                            int g = current.G + this.CalculateGIncrement(
                                adjacentNode.Point.X - current.PathNode.Point.X,
                                adjacentNode.Point.Y - current.PathNode.Point.Y);
                            int h = this.CalculateH(adjacentNode.Point);

                            // Add the adjacent node to the open list
                            this.AddOpenNode(new AStarNode(adjacentNode, h, g, current));
                        }
                    }
                }
            }

            return goalNode;
        }

        /// <summary>
        /// Add the node to the open list; if the point already exists in the open list and this node has a better G
        /// cost, update the parent and G cost of the existing node.
        /// </summary>
        /// <param name="node">The node.</param>
        private void AddOpenNode(AStarNode node)
        {
            // Check if this node exists
            AStarNode existing;
            if (this.openSet.TryGetValue(node.PathNode.Point, out existing))
            {
                // If this node has a better cost than the existing node, update the existing node's parent and G score
                if (node.G < existing.G)
                {
                    existing.Parent = node;
                    existing.G = node.G;
                }
            }
            else
            {
                // If the node doesn't already exist, add it to the open list
                this.openSetOrdered.Push(node);
                this.openSet.Add(node.PathNode.Point, node);
            }
        }

        /// <summary>
        /// Calculate the heuristic distance estimate from the given point to the end point.
        /// </summary>
        /// <param name="point">The point for which to calculate H.</param>
        /// <returns>The heuristic distance estimate from the given point to the end point.</returns>
        private int CalculateH(Point point)
        {
            // Multiply by 10 so as to use the same scale as G values
            return (Math.Abs(point.X - this.goal.Point.X) + Math.Abs(point.Y - this.goal.Point.Y)) * 10;
        }

        /// <summary>
        /// Calculate the amount to increment G for a single step with the given X and Y offsets.
        /// </summary>
        /// <param name="offsetX">The distance that the node is offset from the adjacent node by X.</param>
        /// <param name="offsetY">The distance that the node is offset from the adjacent node by Y.</param>
        /// <returns>The amount to increment G.</returns>
        private int CalculateGIncrement(int offsetX, int offsetY)
        {
            // Multiply by 10 such that the first sqrt decimal isn't rounded off
            offsetX *= 10;
            offsetY *= 10;
            return (int)Math.Sqrt((offsetX * offsetX) + (offsetY * offsetY));
        }

        /// <summary>
        /// Gets a rectangle for the given node with the node point at the middle of the bottom edge.
        /// </summary>
        /// <param name="point">The position of the node.</param>
        /// <param name="width">The width of the node's bounds.</param>
        /// <param name="height">The height of the node's bounds.</param>
        /// <returns>The node rectangle.</returns>
        private Rectangle GetNodeRectangle(Point point, int width, int height)
        {
            return new Rectangle(point.X - (width / 2), point.Y - height, width, height);
        }

        /// <summary>
        /// Determine whether the the given rectangle is free of obstructing terrain.
        /// </summary>
        /// <param name="rect">The rectangle to test.</param>
        /// <returns>True if the rectangle doesn't contain any terrain; False if the rectangle contains obstructing
        /// terrain or the rectangle is outside the bounds of the terrain quad tree.</returns>
        private bool IsOpenSpace(Rectangle rect)
        {
            QuadTreeData<TerrainType>[] data;
            if (this.Terrain.QuadTree.GetData(rect, out data))
            {
                foreach (QuadTreeData<TerrainType> terrainType in data)
                {
                    if (terrainType.Data != TerrainType.None)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Represents a node in the A-star algorithm.
        /// </summary>
        public class AStarNode : IIndexedObject, IComparable<AStarNode>
        {
            /// <summary>
            /// Initializes a new instance of the AStarNode class.
            /// </summary>
            /// <param name="node">The path node.</param>
            /// <param name="h">The heuristic estimate of the distance to the goal from this node.</param>
            /// <param name="g">The cost from the starting node to the this node.</param>
            public AStarNode(PathNode node, int h, int g)
                : this(node, h, g, null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the AStarNode class.
            /// </summary>
            /// <param name="node">The path node.</param>
            /// <param name="h">The heuristic estimate of the distance to the goal from this node.</param>
            /// <param name="g">The cost from the starting node to the this node.</param>
            /// <param name="parent">The parent node in the direction of the start node.</param>
            public AStarNode(PathNode node, int h, int g, AStarNode parent)
            {
                this.PathNode = node;
                this.H = h;
                this.G = g;
                this.Parent = parent;
            }

            /// <summary>
            /// Gets the path node.
            /// </summary>
            public PathNode PathNode { get; private set; }

            /// <summary>
            /// Gets or sets the parent node in the direction of the start node.
            /// </summary>
            public AStarNode Parent { get; set; }

            /// <summary>
            /// Gets the estimated total cost from the starting node to the goal through of the node.
            /// </summary>
            public int F
            {
                get
                {
                    return this.G + this.H;
                }
            }

            /// <summary>
            /// Gets the heuristic estimate of the distance to the goal from this node.
            /// </summary>
            public int H { get; private set; }

            /// <summary>
            /// Gets or sets the cost from the starting node to the this node.
            /// </summary>
            public int G { get; set; }

            /// <summary>
            /// Gets or sets the index.
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// Compare the F value of this node to another node.
            /// </summary>
            /// <param name="other">The other node.</param>
            /// <returns>The relative comparative value.</returns>
            public int CompareTo(AStarNode other)
            {
                if (this.F < other.F)
                {
                    return -1;
                }
                else if (this.F > other.F)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}