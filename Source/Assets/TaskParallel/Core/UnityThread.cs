using System;
using System.Threading;

namespace CI.TaskParallel.Core
{
    public class UnityThread
    {
        private Thread _thread;

        public UnityThread(Action action)
        {
            _thread = new Thread(new ThreadStart(action));
        }

        public void Start()
        {
            _thread.Start();
        }

        public void Abort()
        {
            _thread.Abort();
        }

        public void Join()
        {
            _thread.Join();
        } 
    }
}