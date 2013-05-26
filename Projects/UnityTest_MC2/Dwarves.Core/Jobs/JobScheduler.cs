﻿// ----------------------------------------------------------------------------
// <copyright file="JobScheduler.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------
namespace Dwarves.Core.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Dwarves.Core.Math;

    /// <summary>
    /// Schedules jobs to be executed in parallel. A dependency tree is maintained for the scheduled jobs such that
    /// jobs are executed in the correct logical order and that no resource conflicts arise.
    /// </summary>
    public class JobScheduler : IDisposable
    {
        /// <summary>
        /// The root jobs, which are jobs with no dependents (no other jobs depend on them).
        /// </summary>
        private List<Job> rootJobs;

        /// <summary>
        /// All of the jobs, mirrored in this set for convenience.
        /// </summary>
        private HashSet<Job> allJobs;

        /// <summary>
        /// The pool of jobs to be executed.
        /// </summary>
        private JobPool jobPool;

        /// <summary>
        /// The job comparer.
        /// </summary>
        private JobComparer comparer;

        /// <summary>
        /// The job lock.
        /// </summary>
        private SpinLock jobsLock;

        /// <summary>
        /// Indicates whether the instance has been disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Initialises a new instance of the JobScheduler class.
        /// </summary>
        public JobScheduler()
            : this(Environment.ProcessorCount)
        {
        }

        /// <summary>
        /// Initialises a new instance of the JobScheduler class.
        /// </summary>
        /// <param name="threadCount">The number of threads to spawn.</param>
        public JobScheduler(int threadCount)
        {
            this.rootJobs = new List<Job>(50);
            this.allJobs = new HashSet<Job>();
            this.jobPool = new JobPool(threadCount);
            this.comparer = new JobComparer();
            this.jobsLock = new SpinLock(5);
        }

        /// <summary>
        /// Gets or sets the priority chunks.
        /// </summary>
        public Vector2I[] PriorityChunks
        {
            get { return this.jobPool.PriorityChunks; }
            set { this.jobPool.PriorityChunks = value; }
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
        /// Schedules a job to be executed.
        /// </summary>
        /// <param name="work">The work to be executed.</param>
        /// <param name="parameter">The work parameter.</param>
        /// <param name="info">The job information and access requirements.</param>
        /// <param name="reuse">Indicates the job reuse behaviour.</param>
        /// <returns>The job.</returns>
        public Job Run(JobWorkAction work, object parameter, JobInfo info, JobReuse reuse = JobReuse.None)
        {
            // Create the job
            var job = new Job(work, parameter, info);

            this.jobsLock.Enter();
            try
            {
                if (this.allJobs.Count > 0)
                {
                    // Attempt to reuse an existing job
                    if (reuse != JobReuse.None)
                    {
                        foreach (Job existing in this.allJobs)
                        {
                            if (this.CanReuse(existing, job, reuse))
                            {
                                return existing;
                            }
                        }
                    }

                    // Add the full set of dependents and dependencies (including ancestors)
                    foreach (Job existing in this.allJobs)
                    {
                        int comparison = this.comparer.Compare(job, existing);
                        if (comparison > 0)
                        {
                            job.Dependents.Add(existing);
                        }
                        else if (comparison < 0)
                        {
                            job.Dependencies.Add(existing);
                        }
                    }

                    // Trim the non-direct dependencies
                    for (int i = 0; i < job.Dependencies.Count; i++)
                    {
                        Job dependency = job.Dependencies[i];
                        if (job.HasSubDependency(dependency))
                        {
                            job.Dependencies.RemoveAt(i--);
                        }
                        else
                        {
                            dependency.Dependents.Add(job);

                            // Remove this job from the root list (if it is there) as it now has a dependent
                            this.rootJobs.Remove(dependency);
                        }
                    }

                    // Trim the non-direct dependents
                    for (int i = 0; i < job.Dependents.Count; i++)
                    {
                        Job dependent = job.Dependents[i];
                        if (job.HasSubDependent(dependent))
                        {
                            job.Dependents.RemoveAt(i--);
                        }
                        else
                        {
                            // This is a direct dependent. Check if this dependent is already queued for execution
                            if (!dependent.IsQueuedForExecution)
                            {
                                dependent.Dependencies.Add(job);
                            }
                            else
                            {
                                // Remove dependent from the execution queue
                                if (this.jobPool.Remove(dependent))
                                {
                                    dependent.IsQueuedForExecution = false;
                                    dependent.Dependencies.Add(job);
                                }
                                else
                                {
                                    // The dependent couldn't be unqueued, probably because it is already executing
                                    // We will make our job depend on this, since they can't run at the same time
                                    job.Dependents.RemoveAt(i--);
                                    job.Dependencies.Add(dependent);
                                    dependent.Dependents.Add(job);

                                    // Remove this job from the root list (if it is there) as it now has a dependent
                                    this.rootJobs.Remove(dependent);
                                }
                            }
                        }
                    }

                    // Remove any relations that this job bridges
                    if (job.Dependencies.Count > 0 && job.Dependents.Count > 0)
                    {
                        foreach (Job dependency in job.Dependencies)
                        {
                            foreach (Job dependent in job.Dependents)
                            {
                                if (dependent.Dependencies.Contains(dependency))
                                {
                                    dependent.Dependencies.Remove(dependency);
                                    dependency.Dependents.Remove(dependent);
                                }
                            }
                        }
                    }
                }

                // Register the completion handler
                job.Completed += this.Job_Completed;

                // If this job has no dependents, add it as a root node
                if (job.Dependents.Count == 0)
                {
                    this.rootJobs.Add(job);
                }

                // If this job has no dependencies, enqueue it for execution
                if (job.Dependencies.Count == 0)
                {
                    this.jobPool.Enqueue(job);
                    job.IsQueuedForExecution = true;
                }

                // Add the job to the 'all' set
                this.allJobs.Add(job);
            }
            finally
            {
                this.jobsLock.Exit();
            }

            return job;
        }

        /// <summary>
        /// Cancel any jobs that meet the criteria of the selector.
        /// </summary>
        /// <param name="selector">The chunks.</param>
        public void CancelJobsWhere(Predicate<Job> selector)
        {
            this.jobsLock.Enter();
            try
            {
                foreach (Job job in this.allJobs)
                {
                    if (selector(job))
                    {
                        job.Cancel();
                    }
                }
            }
            finally
            {
                this.jobsLock.Exit();
            }
        }

        /// <summary>
        /// Removes a job that has completed.
        /// </summary>
        /// <param name="job">The job.</param>
        private void RemoveCompletedJob(Job job)
        {
            this.jobsLock.Enter();
            try
            {
                // Remove this job from dependencies
                foreach (Job dependency in job.Dependencies)
                {
                    dependency.Dependents.Remove(job);

                    // All dependents are removed from the given job so add it as a root job now
                    if (dependency.Dependents.Count == 0)
                    {
                        this.rootJobs.Add(dependency);
                    }
                }

                if (job.Dependents.Count > 0)
                {
                    foreach (Job dependent in job.Dependents)
                    {
                        // Remove this job from the dependent
                        dependent.Dependencies.Remove(job);

                        // Bridge the gap between dependents and dependencies
                        foreach (Job dependency in job.Dependencies)
                        {
                            dependent.Dependencies.Add(dependency);
                            dependency.Dependents.Add(dependent);

                            // Remove this job from the root list (if it is there) as it now has a dependent
                            this.rootJobs.Remove(dependency);
                        }

                        // If this dependent has no dependencies, enqueue it for execution
                        if (dependent.Dependencies.Count == 0)
                        {
                            this.jobPool.Enqueue(dependent);
                            dependent.IsQueuedForExecution = true;
                        }
                    }
                }
                else
                {
                    // The job has no dependents so remove this root node
                    this.rootJobs.Remove(job);
                }

                // Remove the job from the 'all' set
                this.allJobs.Remove(job);
            }
            finally
            {
                this.jobsLock.Exit();
            }
        }

        /// <summary>
        /// Determine whether the an duplicate can be used as a substitute for a new job.
        /// </summary>
        /// <param name="existingJob">The existing job.</param>
        /// <param name="newJob">The new job.</param>
        /// <param name="action">The action to be taken for duplicate jobs.</param>
        /// <returns>True if the existing duplicate can be used in place of the new job.</returns>
        private bool CanReuse(Job existingJob, Job newJob, JobReuse action)
        {
            if (!existingJob.IsSubstitute(newJob))
            {
                return false;
            }

            switch (action)
            {
                case JobReuse.None:
                    return false;

                case JobReuse.ReuseAny:
                    return true;

                case JobReuse.ReusePending:
                    return !existingJob.IsCommenced;

                case JobReuse.MergePending:
                    return existingJob.TryMerge(newJob);

                default:
                    throw new InvalidOperationException("Unexpected duplicate action: " + action);
            }
        }

        /// <summary>
        /// Handles the completion of a job.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="job">The job that completed.</param>
        private void Job_Completed(object sender, Job job)
        {
            this.RemoveCompletedJob(job);
        }

        /// <summary>
        /// A fast yet CPU-intensive locking mechanism.
        /// </summary>
        private class SpinLock
        {
            /// <summary>
            /// Indicates whether the lock is currently held.
            /// </summary>
            private int isLockHeld;

            /// <summary>
            /// The iterations to spin between each check on the lock status.
            /// </summary>
            private int spinIterations;

            /// <summary>
            /// Initialises a new instance of the SpinLock class.
            /// </summary>
            /// <param name="spinIterations">The iterations to spin between each check on the lock status.</param>
            public SpinLock(int spinIterations)
            {
                this.spinIterations = spinIterations;
            }

            /// <summary>
            /// Enter the lock.
            /// </summary>
            public void Enter()
            {
                while (Interlocked.CompareExchange(ref this.isLockHeld, 1, 0) != 0)
                {
                    Thread.SpinWait(this.spinIterations);
                }
            }

            /// <summary>
            /// Release the lock.
            /// </summary>
            public void Exit()
            {
                Interlocked.Exchange(ref this.isLockHeld, 0);
            }
        }
    }
}