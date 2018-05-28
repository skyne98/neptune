using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Neptune.JobSystem.Jobs
{
    public abstract class RecurrentJob
    {
        private readonly Mutex _dependantsMutex;
        private readonly List<RecurrentJob> _dependants;
        private JobStatus _status;

        public RecurrentJob()
        {
            _dependants = new List<RecurrentJob>();
            _dependantsMutex = new Mutex();

            Status = JobStatus.NotScheduled;
        }

        public List<RecurrentJob> Dependants
        {
            get
            {
                _dependantsMutex.WaitOne();
                var list = _dependants.ToList();
                _dependantsMutex.ReleaseMutex();
                return list;
            }
        }

        public JobStatus Status
        {
            get => _status;
            internal set => _status = value;
        }

        public void Schedule(JobManager jobManager)
        {
            jobManager.Schedule(this);
        }

        internal void AddDependant(RecurrentJob job)
        {
            _dependantsMutex.WaitOne();
            _dependants.Add(job);
            _dependantsMutex.ReleaseMutex();
        }

        public void Reset()
        {
            _dependantsMutex.WaitOne();
            _dependants.Clear();
            _status = JobStatus.NotScheduled;
            _dependantsMutex.ReleaseMutex();
        }

        public abstract List<RecurrentJob> GetDependencies();
        public abstract void Execute();
    }

    public enum JobStatus
    {
        NotScheduled,
        Scheduled,
        WaitingForDependencies,
        Done
    }
}