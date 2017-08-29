namespace Microsoft.OpenPublishing.Build.Common
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Async lock
    /// </summary>
    /// <example>
    /// AsyncLock locker = new AsyncLock();
    /// using (await locker.LockAsync()){
    ///    // Call code required async lock
    /// }
    /// </example>
    public class AsyncLock
    {
        /// <summary>
        /// SemaphoreSlim will use unmanaged resouce only on accessing the property AvailableWaitHandle
        /// we don't use that propery here, so remove IDisposable
        /// </summary>
        private readonly SemaphoreSlim _semaphore;

        public AsyncLock()
        {
            _semaphore = new SemaphoreSlim(1);
        }

        public async Task<IDisposable> LockAsync()
        {
            await _semaphore.WaitAsync();
            return new Locker(_semaphore);
        }

        private class Locker : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            public Locker(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                _semaphore.Release();
            }
        }
    }
}
