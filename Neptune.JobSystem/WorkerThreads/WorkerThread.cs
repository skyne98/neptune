using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Neptune.JobSystem.Jobs;

namespace Neptune.JobSystem.WorkerThreads
{
    public class WorkerThread
    {
        private JobManager _jobManager;
        private Thread _thread;
        private WorkerThreadStatus _status = WorkerThreadStatus.Idle;
        private ReaderWriterLockSlim _statusMutex;

        public WorkerThreadStatus Status
        {
            get
            {
                _statusMutex.EnterReadLock();
                var value = _status;
                _statusMutex.ExitReadLock();
                return value;
            }
            private set
            {
                _statusMutex.EnterWriteLock();
                _status = value;
                _statusMutex.ExitWriteLock();
            }
        }

        public WorkerThread(JobManager jobManager)
        {
            _jobManager = jobManager;

            _statusMutex = new ReaderWriterLockSlim();

            _thread = new Thread(Run);
            _thread.Name = "Job Worker Thread";
            _thread.Priority = ThreadPriority.Normal;
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void Run()
        {
            while (true)
            {
                if (_jobManager.Paused)
                {
                    Thread.Sleep(1);
                }
                else
                {
                    var job = _jobManager.Steal();
                    if (job != null)
                    {
                        // We have got a job
                        Status = WorkerThreadStatus.Working;
                        job.Execute();
                        job.Status = JobStatus.Done;
                        Status = WorkerThreadStatus.Idle;

                        // Job was executed, try to schedule it's dependents
                        foreach (var jobDependant in job.Dependants)
                        {
                            jobDependant.Schedule(_jobManager);
                        }
                    }
                }
            }
        }
    }

    public enum WorkerThreadStatus
    {
        Idle,
        Working
    }
}