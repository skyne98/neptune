using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Neptune.JobSystem.Jobs;
using Neptune.JobSystem.WorkerThreads;

namespace Neptune.JobSystem
{
    public class JobManager
    {
        private ConcurrentQueue<RecurrentJob> _jobsQueue;
        private List<WorkerThread> _workerThreads;
        internal bool _paused = false;
        internal ReaderWriterLockSlim _pausedLock;

        public bool Paused
        {
            get {
                _pausedLock.EnterReadLock();
                var value = _paused;
                _pausedLock.ExitReadLock();
                return value;
            }
            private set
            {
                _pausedLock.EnterWriteLock();
                _paused = value;
                _pausedLock.ExitWriteLock();
            }
        }

        public JobManager()
        {
            _jobsQueue = new ConcurrentQueue<RecurrentJob>();
            _workerThreads = new List<WorkerThread>();
            _pausedLock = new ReaderWriterLockSlim();

            var count = Environment.ProcessorCount - 1;
            if (count < 1)
                count = 1;

            for (int i = 0; i < count; i++)
            {
                var workerThread = new WorkerThread(this);
                _workerThreads.Add(workerThread);
            }
        }

        public void WaitAll()
        {
            while (true)
            {
                var canBreak = _jobsQueue.Count == 0;
                foreach (var workerThread in _workerThreads)
                {
                    if (workerThread.Status == WorkerThreadStatus.Working)
                    {
                        canBreak = false;
                    }
                }

                if (canBreak)
                    break;
            }
        }

        internal RecurrentJob Steal()
        {
            RecurrentJob job = null;
            _jobsQueue.TryDequeue(out job);

            return job;
        }

        public void Reset()
        {
            PauseAll();
            while (_jobsQueue.Count > 0)
            {
                RecurrentJob job = null;
                while (!_jobsQueue.TryDequeue(out job))
                {
                    // Just wait for the job to finally dequeue
                }
            }
            StartAll();
        }

        public void PauseAll()
        {
            Paused = true;
        }

        public void StartAll()
        {
            Paused = false;
        }

        public void Schedule(RecurrentJob job)
        {
            // If the job was already scheduled somewhere, dont't schedule
            if (job.Status != JobStatus.NotScheduled)
            {
                return;
            }

            // If the job has any unfinished dependencies, make it wait for them
            var dependencies = job.GetDependencies();
            var someNotFinished = dependencies.Any(d => d.Status != JobStatus.Done);
            if (someNotFinished)
            {
                foreach (var dependencyJob in dependencies.Where(d => d.Status != JobStatus.Done))
                {
                    dependencyJob.AddDependant(job);
                }
                return;
            }

            // If the job is free to schedule, do it
            job.Status = JobStatus.Scheduled;
            _jobsQueue.Enqueue(job);
        }
    }
}