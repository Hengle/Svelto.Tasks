#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Diagnostics;
using Svelto.Tasks.Unity.Internal;

namespace Svelto.Tasks.Unity
{
    /// <summary>
    //TimeBoundMonoRunner ensures that the tasks running won't take more than maxMilliseconds per iteration
    //Several tasks must run on this runner to make sense. TaskCollections are considered
    //single tasks, so they don't count (may change in future)
    /// </summary>
    public class TimeBoundMonoRunner : MonoRunner
    {
        public float maxMilliseconds
        {
            set
            {
                _info.maxMilliseconds = (long) (value * 10000);
            }
        }

        // Greedy means that the runner will try to occupy the whole maxMilliseconds interval, by looping among all tasks until all are completed or maxMilliseconds passed
        public TimeBoundMonoRunner(string name, float maxMilliseconds, bool mustSurvive = false)
        {
            _flushingOperation = new UnityCoroutineRunner.FlushingOperation();

            UnityCoroutineRunner.InitializeGameObject(name, ref _go, mustSurvive);

            var runnerBehaviour = _go.AddComponent<RunnerBehaviourUpdate>();
            var runnerBehaviourForUnityCoroutine = _go.AddComponent<RunnerBehaviour>();

            _info = new TimeBoundRunningInfo(maxMilliseconds) { runnerName = name };

            runnerBehaviour.StartUpdateCoroutine(new UnityCoroutineRunner.Process
                (_newTaskRoutines, _coroutines, _flushingOperation, _info,
                 UnityCoroutineRunner.StandardTasksFlushing,
                 runnerBehaviourForUnityCoroutine, StartCoroutine));
        }

        class TimeBoundRunningInfo : UnityCoroutineRunner.RunningTasksInfo
        {
            public long maxMilliseconds;

            public TimeBoundRunningInfo(float maxMilliseconds)
            {
                this.maxMilliseconds = (long) (maxMilliseconds * 10000);
            }
            
            public override bool CanMoveNext(ref int index, int count, object current)
            {
                if (_stopWatch.ElapsedTicks > maxMilliseconds)
                    return false;
                 
                return true;
            }

            public override void Reset()
            {
                _stopWatch.Reset();
                _stopWatch.Start();
            }

            readonly Stopwatch _stopWatch = new Stopwatch();

        }

        readonly TimeBoundRunningInfo _info;
    }
}
#endif