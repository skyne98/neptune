using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Neptune.JobSystem.Jobs;

namespace Neptune.JobSystem.WorkerThreads
{
    public class WorkerThread
    {
        private readonly JobManager _jobManager;
        private Thread _thread;
        private List<Job> _jobs;
        private Mutex _jobsMutex;
        private bool _executingJob = false;

        public WorkerThread(JobManager jobManager)
        {
            _jobManager = jobManager;
            _jobsMutex = new Mutex();
            _jobs = new List<Job>();
            _thread = new Thread(Run);
            _thread.IsBackground = true;
            _thread.Priority = ThreadPriority.AboveNormal;
            _thread.Start();
        }

        private void Run()
        {
            while (true)
            {
                var job = Steal();

                if (job == null)
                {
                    job = _jobManager.Steal(this);
                }

                if (job != null)
                {
                    _executingJob = true;
                    job.Execute();
                    job.State = JobState.Done;

                    foreach (var continuation in job.Continuations)
                    {
                        continuation.Schedule();
                    }

                    _executingJob = false;
                }
            }
        }

        public bool IsIdle()
        {
            var result = true;
            
            _jobsMutex.WaitOne();
            if (_jobs.Count > 0 || _executingJob)
                result = false;
            _jobsMutex.ReleaseMutex();

            return result;
        }
        
        public void Push(Job job)
        {
            _jobsMutex.WaitOne();
            _jobs.Add(job);
            _jobsMutex.ReleaseMutex();
        }

        public Job Steal()
        {
            _jobsMutex.WaitOne();
            if (_thread == null)
                return null;
            
            var job = _jobs.FirstOrDefault();
            _jobs.Remove(job);
            _jobsMutex.ReleaseMutex();

            return job;
        }
    }
}