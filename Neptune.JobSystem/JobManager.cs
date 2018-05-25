using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Neptune.JobSystem.Jobs;
using Neptune.JobSystem.WorkerThreads;

namespace Neptune.JobSystem
{
    public class JobManager
    {
        private readonly int _coresCount;
        private readonly List<WorkerThread> _workerThreads;
        private readonly int _workerThreadsCount;
        private readonly Mutex _threadsMutex;
        private readonly Random _random;

        public JobManager()
        {
            _threadsMutex = new Mutex();
            _random = new Random();
            
            _coresCount = Environment.ProcessorCount;
            _workerThreadsCount = _coresCount - 1;
            if (_workerThreadsCount < 1)
                _workerThreadsCount = 1;
            
            _workerThreads = new List<WorkerThread>();
            for (var i = 0; i < _workerThreadsCount; i++)
            {
                var workerThread = new WorkerThread(this);
                _workerThreads.Add(workerThread);
            }
        }

        public void WaitAll()
        {
            while (true)
            {
                var idle = true;
                foreach (var workerThread in _workerThreads)
                {
                    var threadIdle = workerThread.IsIdle();

                    if (threadIdle == false)
                        idle = false;
                }

                if (idle)
                    break;
            }
        }

        public void Push(Job job)
        {
            int index = _random.Next(0, _workerThreadsCount);
            _workerThreads[index].Push(job);
        }

        internal Job Steal(WorkerThread exclude)
        {
            _threadsMutex.WaitOne();
            if (_workerThreadsCount == 1)
            {
                _threadsMutex.ReleaseMutex();
                return null;
            }
            
            int index = _random.Next(0, _workerThreadsCount - 1);
            var thread = _workerThreads.Except(new List<WorkerThread>() {exclude}).ElementAt(index);
            var job = thread.Steal();            
            _threadsMutex.ReleaseMutex();

            return job;
        }
    }
}