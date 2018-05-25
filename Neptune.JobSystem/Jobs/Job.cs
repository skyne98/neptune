using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Neptune.JobSystem.Jobs
{
    public abstract class Job
    {
        private JobManager _jobManager;
        private List<Job> _continuations;

        public Job(JobManager jobManager)
        {
            _jobManager = jobManager;
            _continuations = new List<Job>();
        }

        public abstract void Execute();
        public abstract List<Job> GetDependencies();

        public void Reset()
        {
            State = JobState.NotScheduled;
            _continuations.Clear();
        }

        public void Schedule()
        {
            var dependencies = GetDependencies();
            var allDependenciesDone = true;

            if (dependencies != null && dependencies.Count > 0)
            {
                foreach (var dependency in dependencies)
                {
                    if (dependency.State == JobState.NotScheduled)
                    {
                        dependency.Schedule();
                    }

                    dependency._continuations.Add(this);

                    if (dependency.State != JobState.Done)
                    {
                        allDependenciesDone = false;
                    }
                }   
            }

            if (allDependenciesDone)
            {
                _jobManager.Push(this);
                State = JobState.Scheduled;
            }
        }

        public JobState State { get; internal set; } = JobState.NotScheduled;

        public List<Job> Continuations => _continuations;
    }

    public enum JobState
    {
        NotScheduled,
        Scheduled,
        Done
    }
}