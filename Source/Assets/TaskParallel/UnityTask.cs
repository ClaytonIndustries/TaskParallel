using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using CI.TaskParallel.Core;

namespace CI.TaskParallel
{
    public class UnityTask
    {
        protected Thread _thread;
        protected ThreadStart _threadStart;
        protected UnityTask _continuation;

        private static UnityDispatcher _dispatcher;

        public UnityTask(Action action)
        {
            Initialise(action);
        }

        protected UnityTask()
        {
        }

        protected void Initialise(Action action)
        {
            _threadStart = new ThreadStart(action);

            _threadStart += () =>
            {
                if (_continuation != null)
                {
                    _continuation.Start();
                }
            };

            _thread = new Thread(_threadStart);
        }

        public void Start()
        {
            _thread.Start();
        }

        public void Abort()
        {
            _thread.Abort();
        }

        public void Wait()
        {
            _thread.Join();
        }

        public UnityTask ContinueWith(Action<UnityTask> action)
        {
            Action wrapper = () =>
            {
                action(this);
            };

            UnityTask continuation = new UnityTask(wrapper);
            _continuation = continuation;

            return continuation;
        }

        public void ContinueOnUIThread(Action<UnityTask> action)
        {
            Action wrapper = () =>
            {
                action(this);
            };

            RunOnUIThread(wrapper);
        }

        public static UnityTask Run(Action action)
        {
            UnityTask unityTask = new UnityTask(action);
            unityTask.Start();

            return unityTask;
        }

        public static UnityTask<TResult> Run<TResult>(Func<TResult> action)
        {
            UnityTask<TResult> unityTask = new UnityTask<TResult>(action);
            unityTask.Start();

            return unityTask;
        }

        public static void RunOnUIThread(Action action)
        {
            _dispatcher.Enqueue(action);
        }

        public static void InitialiseDispatcher()
        {
            if (_dispatcher == null)
            {
                _dispatcher = new GameObject("UIDispatcher").AddComponent<UnityDispatcher>();
            }
        }

        public static void WaitAll(params UnityTask[] unityTasks)
        {
            foreach(UnityTask unityTask in unityTasks)
            {
                unityTask.Wait();
            }
        }

        public static void WaitAll(IEnumerable<UnityTask> unityTasks)
        {
            foreach (UnityTask unityTask in unityTasks)
            {
                unityTask.Wait();
            }
        }
    }

    public class UnityTask<TResult> : UnityTask
    {
        public TResult Result
        {
            get; set;
        }

        public UnityTask(Func<TResult> action)
        {
            Action wrapperAction = () =>
            {
                Result = action();
            };

            Initialise(wrapperAction);
        }

        public UnityTask ContinueWith(Action<UnityTask<TResult>> action)
        {
            Action wrapper = () =>
            {
                action(this);
            };

            UnityTask continuation = new UnityTask(wrapper);
            _continuation = continuation;

            return continuation;
        }

        public UnityTask<NewTResult> ContinueWith<NewTResult>(Func<UnityTask<TResult>, NewTResult> action)
        {
            Func<NewTResult> wrapperAction = () =>
            {
                return action(this);
            };

            UnityTask<NewTResult> continuation = new UnityTask<NewTResult>(wrapperAction);
            _continuation = continuation;

            return continuation;
        }

        public void ContinueOnUIThread(Action<UnityTask<TResult>> action)
        {
            Action wrapper = () =>
            {
                action(this);
            };

            RunOnUIThread(wrapper);
        }
    }
}