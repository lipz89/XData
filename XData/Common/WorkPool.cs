using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace XData.Common
{
    internal class WorkPool<T> : IDisposable
    {
        private readonly EventWaitHandle wh = new AutoResetEvent(false);
        private ConcurrentQueue<Lazy<T>> free = new ConcurrentQueue<Lazy<T>>();

        public WorkPool(Func<T> creator, int maxCount)
        {
            if (creator == null)
            {
                throw Error.ArgumentNullException(nameof(creator));
            }
            if (maxCount < 2)
            {
                maxCount = 2;
            }
            for (int i = 0; i < maxCount; i++)
            {
                free.Enqueue(new Lazy<T>(creator, true));
            }
        }

        public void Do(Action<T> work)
        {
            while (true)
            {
                Lazy<T> item;
                if (free.TryDequeue(out item))
                {
                    work(item.Value);
                    free.Enqueue(item);
                    wh.Set();
                    break;
                }
                wh.WaitOne();
            }
        }

        public R Do<R>(Func<T, R> work)
        {
            while (true)
            {
                Lazy<T> item;
                if (free.TryDequeue(out item))
                {
                    var r = work(item.Value);
                    free.Enqueue(item);
                    wh.Set();
                    return r;
                }
                wh.WaitOne();
            }
        }

        public virtual void Dispose()
        {
            foreach (var local in free.Where(x => x.IsValueCreated))
            {
                (local.Value as IDisposable)?.Dispose();
            }
            free = null;
        }
    }
}