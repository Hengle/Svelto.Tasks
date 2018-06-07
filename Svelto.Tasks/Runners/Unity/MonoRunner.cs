#if UNITY_5 || UNITY_5_3_OR_NEWER

using Svelto.DataStructures;
using Svelto.Tasks.Internal;
using UnityEngine;

#if TASKS_PROFILER_ENABLED
using Svelto.Tasks.Profiler;
#endif

namespace Svelto.Tasks
{
    public abstract class MonoRunner : IRunner
    {
        public bool paused { set; get; }
        public bool isStopping { get { return flushingOperation.stopped; } }
        
        public int  numberOfRunningTasks { get { return info.count; } }

        protected abstract UnityCoroutineRunner.RunningTasksInfo info { get; }
        protected abstract ThreadSafeQueue<IPausableTask> newTaskRoutines { get; }
        protected abstract UnityCoroutineRunner.FlushingOperation flushingOperation { get; }
        protected GameObject _go;
        
        /// <summary>
        /// TaskRunner doesn't stop executing tasks between scenes
        /// it's the final user responsibility to stop the tasks if needed
        /// </summary>
        public virtual void StopAllCoroutines()
        {
            paused = false;

            UnityCoroutineRunner.StopRoutines(flushingOperation);

            newTaskRoutines.Clear();
        }

        public virtual void StartCoroutine(IPausableTask task)
        {
            paused = false;

            if (task == null) return;

            newTaskRoutines.Enqueue(task); //careful this could run on another thread!
        }

        public void Dispose()
        {
            StopAllCoroutines();
            
            GameObject.DestroyImmediate(_go);
        }
    }
}
#endif