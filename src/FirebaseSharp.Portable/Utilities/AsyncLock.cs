using System;
using System.Threading;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Utilities
{
    // http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx
    public sealed class AsyncLock : IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly Task<IDisposable> _releaser;

        public AsyncLock()
        {
            _releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        public Task<IDisposable> LockAsync()
        {
            var wait = _semaphore.WaitAsync();
            return wait.IsCompleted ?
                _releaser :
                wait.ContinueWith((_, state) => (IDisposable)state,
                    _releaser.Result, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock _toRelease;
            internal Releaser(AsyncLock toRelease) { _toRelease = toRelease; }
            public void Dispose() { _toRelease._semaphore.Release(); }
        }

        public void Dispose()
        {
            using (_semaphore) { }
        }
    }
}