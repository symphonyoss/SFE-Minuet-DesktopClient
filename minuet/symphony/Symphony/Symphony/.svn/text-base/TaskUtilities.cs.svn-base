using System;
using System.Threading;
using System.Threading.Tasks;

namespace Symphony
{
    public class TaskUtilities
    {
        public static DelayedTask Delay(TimeSpan timeSpan)
        {
            return new DelayedTask(timeSpan);
        }

        public static DelayedTask Delay(int milliseconds)
        {
            return new DelayedTask(TimeSpan.FromMilliseconds(milliseconds));
        }

        public class DelayedTask
        {
            private readonly TimeSpan timeSpan;

            internal DelayedTask(TimeSpan timeSpan)
            {
                this.timeSpan = timeSpan;
            }

            public void ContinueWith(Action action, TaskScheduler taskScheduler)
            {
                Timer timer = null;
                timer = new Timer(ignore =>
                {
                    timer.Dispose();
                    Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, taskScheduler);
                }, null, this.timeSpan, TimeSpan.FromMilliseconds(-1));
            }
        }
    }
}