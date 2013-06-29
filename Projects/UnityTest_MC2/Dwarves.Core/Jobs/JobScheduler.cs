﻿// ----------------------------------------------------------------------------
// <copyright file="JobScheduler.cs" company="Dematic">
//     Copyright © Dematic 2009-2013. All rights reserved
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves.Core.Jobs
{
    using System;
    using System.Collections.Generic;
    using Dwarves.Core.Math;

    /// <summary>
    /// The job scheduler.
    /// </summary>
    public class JobScheduler : IDisposable
    {
        /// <summary>
        /// The master job queue.
        /// </summary>
        private MasterJobQueue masterQueue;

        /// <summary>
        /// All the jobs in the master queue.
        /// </summary>
        private List<Job> masterQueueJobs;

        /// <summary>
        /// The job queue for each chunk.
        /// </summary>
        private Dictionary<Vector2I, ChunkJobQueue> chunkQueues;

        /// <summary>
        /// The pool of worker threads to execute jobs.
        /// </summary>
        private JobPool jobPool;

        /// <summary>
        /// The queues lock.
        /// </summary>
        private SpinLock queuesLock;

        /// <summary>
        /// Indicates whether the instance has been disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Initialises a new instance of the JobScheduler class.
        /// </summary>
        /// <param name="threadCount">The number of threads to spawn.</param>
        public JobScheduler(int threadCount)
        {
            this.masterQueue = new MasterJobQueue();
            this.masterQueueJobs = new List<Job>();
            this.chunkQueues = new Dictionary<Vector2I, ChunkJobQueue>();
            this.jobPool = new JobPool(threadCount);
            this.queuesLock = new SpinLock(10);
        }

        /// <summary>
        /// Gets the master queue state.
        /// </summary>
        public MasterJobQueueState MasterQueueState
        {
            get { return this.masterQueue.State; }
        }

        /// <summary>
        /// Dispose the instance.
        /// </summary>
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.jobPool.Dispose();
                this.isDisposed = true;
            }
        }

        /// <summary>
        /// Enqueue a master job. A master job requires exclusive access to all chunks.
        /// </summary>
        /// <param name="work">The work to be executed by the job.</param>
        /// <param name="canEnqueue">Evaluates whether the job can be enqueued. If the master queue evaluate false the
        /// job will not be enqueued.</param>
        /// <param name="reserveQueue">Reserve the master queue to indicate that this job is currently queued or
        /// executing.</param>
        /// <param name="unreserveQueue">Un-reserve the master queue to indicate that this job is no longer queued or
        /// executing.</param>
        /// <param name="canSkip">Indicates whether the job can be skipped.</param>
        /// <returns>True if the job was enqueued.</returns>
        public bool EnqueueMaster(
            Action work,
            Predicate<MasterJobQueue> canEnqueue,
            Action<MasterJobQueue> reserveQueue,
            Action<MasterJobQueue> unreserveQueue,
            bool canSkip)
        {
            // Create the job
            var job = new MasterJob(work, unreserveQueue, canSkip, this.chunkQueues.Count + 10);
            job.IsPendingChanged += this.Job_IsPendingChanged;
            job.Completed += this.Job_Completed;

            // Check whether the job can be enqueued and get the full set of queues
            ChunkJobQueue[] chunkQueues;
            this.queuesLock.Enter();
            try
            {
                // Determine whether the job can be enqueued in the master queue
                if (!canEnqueue(this.masterQueue))
                {
                    return false;
                }

                // Reserve the master queue
                reserveQueue(this.masterQueue);

                // Retain a reference to this master job
                this.masterQueueJobs.Add(job);

                // Build the set of owners
                int i = 0;
                chunkQueues = new ChunkJobQueue[this.chunkQueues.Count];
                foreach (ChunkJobQueue queue in this.chunkQueues.Values)
                {
                    chunkQueues[i++] = queue;
                }
            }
            finally
            {
                this.queuesLock.Exit();
            }

            // Add the owners and enqueue the job
            job.AddOwners(this.masterQueue);
            job.AddOwners(chunkQueues);
            this.masterQueue.Enqueue(job);
            foreach (ChunkJobQueue queue in chunkQueues)
            {
                queue.Enqueue(job);
            }

            return true;
        }

        /// <summary>
        /// Enqueue a job.
        /// </summary>
        /// <param name="work">The work to be executed by the job.</param>
        /// <param name="canEnqueue">Evaluates whether the job can be enqueued. If any queue evaluate false the job
        /// will not be enqueued.</param>
        /// <param name="reserveQueue">Reserve each queue to indicate that this job is currently queued or executing.
        /// </param>
        /// <param name="unreserveQueue">Un-reserve each queue to indicate that this job is no longer queued or
        /// executing.</param>
        /// <param name="canSkip">Indicates whether the job can be skipped.</param>
        /// <param name="chunks">The chunks to which this job requires exclusive access.</param>
        /// <returns>True if the job was enqueued.</returns>
        public bool Enqueue(
            Action work,
            Predicate<ChunkJobQueue> canEnqueue,
            Action<ChunkJobQueue> reserveQueue,
            Action<ChunkJobQueue> unreserveQueue,
            bool canSkip,
            params Vector2I[] chunks)
        {
            // Create the job
            var job = new ChunkJob(work, unreserveQueue, canSkip, chunks.Length);
            job.IsPendingChanged += this.Job_IsPendingChanged;
            job.Completed += this.Job_Completed;

            // Get the owners of the job, creating the owner queues if necessary
            ChunkJobQueue[] chunkQueues = new ChunkJobQueue[chunks.Length];
            this.queuesLock.Enter();
            try
            {
                // Build the set of owners, checking whether the job can be enqueued
                for (int i = 0; i < chunks.Length; i++)
                {
                    ChunkJobQueue queue = this.GetOrInitialiseQueue(chunks[i]);
                    if (!canEnqueue(queue))
                    {
                        return false;
                    }

                    chunkQueues[i] = queue;
                }

                // Reserve the owner queues
                foreach (ChunkJobQueue queue in chunkQueues)
                {
                    reserveQueue(queue);
                }
            }
            finally
            {
                this.queuesLock.Exit();
            }

            // Add the owners and enqueue the job
            job.AddOwners(chunkQueues);
            foreach (ChunkJobQueue queue in chunkQueues)
            {
                queue.Enqueue(job);
            }

            return true;
        }

        /// <summary>
        /// Update the chunks that are currently active.
        /// </summary>
        /// <param name="activeChunks">The currently active chunks.</param>
        public void UpdateActiveChunks(HashSet<Vector2I> activeChunks)
        {
            List<Vector2I> removeNow = null;
            this.queuesLock.Enter();
            try
            {
                foreach (ChunkJobQueue queue in this.chunkQueues.Values)
                {
                    bool remove = !activeChunks.Contains(queue.Chunk);
                    if (remove && queue.IsIdle)
                    {
                        if (removeNow == null)
                        {
                            removeNow = new List<Vector2I>();
                        }

                        removeNow.Add(queue.Chunk);
                    }
                    else
                    {
                        queue.FlaggedForRemoval = remove;
                    }
                }

                if (removeNow != null)
                {
                    foreach (Vector2I chunk in removeNow)
                    {
                        this.chunkQueues.Remove(chunk);
                    }
                }
            }
            finally
            {
                this.queuesLock.Exit();
            }
        }

        /// <summary>
        /// Remove chunks from the list that do not satisfy the selector condition.
        /// </summary>
        /// <param name="chunks">The chunks to trim from.</param>
        /// <param name="selector">The selector.</param>
        public void TrimChunks(List<Vector2I> chunks, Predicate<ChunkJobQueue> selector)
        {
            this.queuesLock.Enter();
            try
            {
                for (int i = chunks.Count - 1; i >= 0; i--)
                {
                    ChunkJobQueue queue;
                    if (this.chunkQueues.TryGetValue(chunks[i], out queue))
                    {
                        if (!selector(queue))
                        {
                            chunks.RemoveAt(i);
                        }
                    }
                }
            }
            finally
            {
                this.queuesLock.Exit();
            }
        }

        /// <summary>
        /// Gets the job queue for the given chunk, initialising the queue if one doesn't exist.
        /// </summary>
        /// <param name="chunk">The chunk.</param>
        /// <returns>The job queue.</returns>
        private ChunkJobQueue GetOrInitialiseQueue(Vector2I chunk)
        {
            ChunkJobQueue queue;
            if (!this.chunkQueues.TryGetValue(chunk, out queue))
            {
                queue = new ChunkJobQueue(chunk, this.masterQueueJobs);
                queue.Idle += this.ChunkJobs_QueueIdle;
                this.chunkQueues.Add(chunk, queue);
            }

            return queue;
        }

        /// <summary>
        /// Moves the owner queues of a job forward. This means each queue is stepped to the job after this one.
        /// </summary>
        /// <param name="job">The job.</param>
        private void MoveToNextJob(Job job)
        {
            if (job is MasterJob)
            {
                this.queuesLock.Enter();
                try
                {
                    this.masterQueueJobs.Remove(job);
                }
                finally
                {
                    this.queuesLock.Exit();
                }
            }

            foreach (JobQueue owner in job.GetOwners())
            {
                owner.MoveNext();
            }
        }

        /// <summary>
        /// Handle a job pending execution.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="job">The job.</param>
        private void Job_IsPendingChanged(object sender, Job job)
        {
            if (job.IsPending)
            {
                if (!job.IsSkipRequested)
                {
                    // Enqueue this job to be executed
                    this.jobPool.Enqueue(job);
                }
                else
                {
                    // Skip the job by moving each owner queue forward
                    this.MoveToNextJob(job);
                }
            }
        }

        /// <summary>
        /// Handle a job entering the completed state.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="job">The job.</param>
        private void Job_Completed(object sender, Job job)
        {
            this.MoveToNextJob(job);
        }

        /// <summary>
        /// Handle a chunk queue becoming idle.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="queue">The job queue.</param>
        private void ChunkJobs_QueueIdle(object sender, JobQueue queue)
        {
            this.queuesLock.Enter();
            try
            {
                if (queue.IsIdle && queue.FlaggedForRemoval)
                {
                    this.chunkQueues.Remove((queue as ChunkJobQueue).Chunk);
                }
            }
            finally
            {
                this.queuesLock.Exit();
            }
        }
    }
}