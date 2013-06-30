﻿// ----------------------------------------------------------------------------
// <copyright file="ChunkJobQueueState.cs" company="Dematic">
//     Copyright © Dematic 2009-2013. All rights reserved
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves.Core.Jobs
{
    using System.Collections;
    using System.Collections.Generic;
    using Dwarves.Core.Math;

    /// <summary>
    /// The state of a chunk job queue.
    /// </summary>
    public class ChunkJobQueueState
    {
        /// <summary>
        /// Indicates whether the a rebuild mesh job needs to be queued due to changes to point data.
        /// </summary>
        private RequiredWork rebuildMeshState;

        /// <summary>
        /// Indicates whether the a mesh filter update job needs to be queued due to changes to mesh data.
        /// </summary>
        private RequiredWork updateMeshFilterState;

        /// <summary>
        /// Indicates whether a job is queued to load the points.
        /// </summary>
        private bool loadPointsInProgress;

        /// <summary>
        /// Indicates whether a job is queued to rebuild the chunk mesh.
        /// </summary>
        private bool rebuildMeshInProgress;

        /// <summary>
        /// Indicates whether a job is queued to update the mesh filter.
        /// </summary>
        private bool updateMeshFilterInProgress;

        /// <summary>
        /// The dictionary tracking the current dig circle jobs.
        /// </summary>
        private Dictionary<Vector2I, int> digCircleInProgress;

        /// <summary>
        /// Initialises a new instance of the ChunkJobQueueState class.
        /// </summary>
        /// <param name="chunk">The chunk.</param>
        public ChunkJobQueueState(Vector2I chunk)
        {
            this.Chunk = chunk;
            this.rebuildMeshState = new RequiredWork();
            this.updateMeshFilterState = new RequiredWork();
            this.digCircleInProgress = new Dictionary<Vector2I, int>();
        }

        /// <summary>
        /// Gets the chunk.
        /// </summary>
        public Vector2I Chunk { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a load points job has completed.
        /// </summary>
        public bool LoadPointsCompleted { get; private set; }

        #region Load Points

        /// <summary>
        /// Check whether a LoadPoints job can execute.
        /// </summary>
        /// <param name="chunk">The chunk being loaded.</param>
        /// <returns>True if the job can be enqueued.</returns>
        public bool CanLoadPoints(Vector2I chunk)
        {
            return this.Chunk != chunk || !this.loadPointsInProgress;
        }

        /// <summary>
        /// Reserves a LoadPoints job.
        /// </summary>
        /// <param name="chunk">The chunk being loaded.</param>
        public void ReserveLoadPoints(Vector2I chunk)
        {
            if (this.Chunk == chunk)
            {
                this.loadPointsInProgress = true;
            }

            this.rebuildMeshState.IsUpdateRequired = true;
        }

        /// <summary>
        /// Un-reserves a LoadPoints job.
        /// </summary>
        /// <param name="chunk">The chunk being loaded.</param>
        public void UnreserveLoadPoints(Vector2I chunk)
        {
            if (this.Chunk == chunk)
            {
                this.loadPointsInProgress = false;
                this.LoadPointsCompleted = true;
            }
        }

        #endregion

        #region Rebuild Mesh

        /// <summary>
        /// Check whether a RebuildMesh job can execute.
        /// </summary>
        /// <param name="chunk">The chunk being rebuilt.</param>
        /// <returns>True if the job can be enqueued.</returns>
        public bool CanRebuildMesh(Vector2I chunk)
        {
            return this.Chunk != chunk || (!this.rebuildMeshInProgress && this.rebuildMeshState.IsUpdateRequired);
        }

        /// <summary>
        /// Reserves a RebuildMesh job.
        /// </summary>
        /// <param name="chunk">The chunk being rebuilt.</param>
        public void ReserveRebuildMesh(Vector2I chunk)
        {
            if (this.Chunk == chunk)
            {
                this.rebuildMeshInProgress = true;

                // Indicate that a mesh filter update is required and add any chunks requiring a synchronised update
                this.updateMeshFilterState.IsUpdateRequired = true;
                this.updateMeshFilterState.AddChunksToSynchronise(this.rebuildMeshState);

                // Reset the rebuild mesh required state
                this.rebuildMeshState.IsUpdateRequired = false;
                this.rebuildMeshState.ResetChunksToSynchronise();
            }
        }

        /// <summary>
        /// Un-reserves a RebuildMesh job.
        /// </summary>
        /// <param name="chunk">The chunk being rebuilt.</param>
        public void UnreserveRebuildMesh(Vector2I chunk)
        {
            if (this.Chunk == chunk)
            {
                this.rebuildMeshInProgress = false;
            }
        }

        #endregion

        #region Update Mesh Filter

        /// <summary>
        /// Check whether a UpdateMeshFilter job can execute.
        /// </summary>
        /// <returns>True if the job can be enqueued.</returns>
        public bool CanUpdateMeshFilter()
        {
            return !this.updateMeshFilterInProgress && this.updateMeshFilterState.IsUpdateRequired;
        }

        /// <summary>
        /// Reserves a UpdateMeshFilter job.
        /// </summary>
        public void ReserveUpdateMeshFilter()
        {
            this.updateMeshFilterInProgress = true;
            this.updateMeshFilterState.IsUpdateRequired = false;
        }

        /// <summary>
        /// Un-reserves a UpdateMeshFilter job.
        /// </summary>
        public void UnreserveUpdateMeshFilter()
        {
            this.updateMeshFilterInProgress = false;
        }

        #endregion

        #region Dig Circle

        /// <summary>
        /// Check whether a DigCircle job can execute.
        /// </summary>
        /// <param name="chunk">The chunk in which the origin lies.</param>
        /// <param name="origin">The circle origin.</param>
        /// <param name="radius">The circle radius.</param>
        /// <returns>True if the job can be enqueued.</returns>
        public bool CanDigCircle(Vector2I chunk, Vector2I origin, int radius)
        {
            if (chunk == this.Chunk)
            {
                bool exists;
                int existing;
                lock ((this.digCircleInProgress as ICollection).SyncRoot)
                {
                    exists = this.digCircleInProgress.TryGetValue(origin, out existing);
                }

                return !exists || radius > existing;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Reserves a DigCircle job.
        /// </summary>
        /// <param name="chunk">The chunk in which the origin lies.</param>
        /// <param name="origin">The circle origin.</param>
        /// <param name="radius">The circle radius.</param>
        /// <param name="chunksToSync">The chunks requiring synchronisation such that their mesh filters are updated in
        /// the same frame.</param>
        public void ReserveDigCircle(Vector2I chunk, Vector2I origin, int radius, ChunkSync chunksToSync)
        {
            if (chunk == this.Chunk)
            {
                lock ((this.digCircleInProgress as ICollection).SyncRoot)
                {
                    if (this.digCircleInProgress.ContainsKey(origin))
                    {
                        this.digCircleInProgress[origin] = radius;
                    }
                    else
                    {
                        this.digCircleInProgress.Add(origin, radius);
                    }
                }
            }

            this.rebuildMeshState.IsUpdateRequired = true;
            if (chunksToSync != null)
            {
                rebuildMeshState.AddChunksToSynchronise(chunksToSync);
            }
        }

        /// <summary>
        /// Un-reserves a DigCircle job.
        /// </summary>
        /// <param name="chunk">The chunk in which the origin lies.</param>
        /// <param name="origin">The circle origin.</param>
        /// <param name="radius">The circle radius.</param>
        public void UnreserveDigCircle(Vector2I chunk, Vector2I origin, int radius)
        {
            if (chunk == this.Chunk)
            {
                lock ((this.digCircleInProgress as ICollection).SyncRoot)
                {
                    if (this.digCircleInProgress[origin] == radius)
                    {
                        this.digCircleInProgress.Remove(origin);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Indicates the type of work that is required for a stage in the chunk pipeline. This class is not thread
        /// safe.
        /// </summary>
        private class RequiredWork
        {
            /// <summary>
            /// The chunks that require synchronisation for this and future stages. This will ultimately result in the
            /// synchronised chunks being updated in the same frame.
            /// </summary>
            private List<ChunkSync> chunksToSync;

            /// <summary>
            /// Initialises a new instance of the RequiredWork class.
            /// </summary>
            public RequiredWork()
            {
                this.chunksToSync = new List<ChunkSync>();
            }

            /// <summary>
            /// Gets or sets a value indicating whether work is required to update this aspect.
            /// </summary>
            public bool IsUpdateRequired { get; set; }

            /// <summary>
            /// Add a set of chunks to synchronise.
            /// </summary>
            /// <param name="chunksToSync">The chunks requiring synchronisation to ensure that the mesh filter is updated
            /// in the one frame for all the chunks.</param>
            public void AddChunksToSynchronise(ChunkSync chunksToSync)
            {
                // Don't do anything if these chunks already exist
                foreach (ChunkSync existing in this.chunksToSync)
                {
                    if (existing.Contains(chunksToSync))
                    {
                        return;
                    }
                }

                this.chunksToSync.Add(chunksToSync);
            }

            /// <summary>
            /// Add the chunks to synchronise from the other instance.
            /// </summary>
            /// <param name="other">The other instance.</param>
            public void AddChunksToSynchronise(RequiredWork other)
            {
                foreach (ChunkSync otherChunks in other.chunksToSync)
                {
                    this.AddChunksToSynchronise(otherChunks);
                }
            }

            /// <summary>
            /// Reset the chunk synchronise.
            /// </summary>
            public void ResetChunksToSynchronise()
            {
                this.chunksToSync.Clear();
            }
        }
    }
}