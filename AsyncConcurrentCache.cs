namespace Microsoft.OpenPublishing.Build.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// An asynchronous wrapper over concurrent dictionary
    /// </summary>
    /// <typeparam name="TKey">Type of keys</typeparam>
    /// <typeparam name="TValue">Type of values</typeparam>
    /// <remarks>Implementation copied from http://msdn.microsoft.com/en-us/library/hh873173(v=vs.110).aspx, AsyncCache</remarks>
    public class AsyncConcurrentCache<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<Task<TValue>>> _cache;

        public AsyncConcurrentCache(IEqualityComparer<TKey> comparer = null)
        {
            _cache = comparer == null
                 ? new ConcurrentDictionary<TKey, Lazy<Task<TValue>>>()
                 : new ConcurrentDictionary<TKey, Lazy<Task<TValue>>>(comparer);
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="AsyncConcurrentCache{TKey, TValue}"/> by using the specified function, if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to get a task to generate value for the key</param>
        /// <param name="removeKeyOnFaulted">A flag indicating whether to remove the key from cache on faulted.</param>
        /// <returns>The task to generate value for the key</returns>
        public Task<TValue> GetOrAdd(TKey key, Func<TKey, Task<TValue>> valueFactory, bool removeKeyOnFaulted = true)
        {
            Guard.ArgumentNotNull(key, nameof(key));
            Guard.ArgumentNotNull(valueFactory, nameof(valueFactory));

            return _cache.GetOrAdd(
                key,
                k => new Lazy<Task<TValue>>(
                         () =>
                             {
                                 Task<TValue> task = Task.Run(() => valueFactory(k));

                                 if (removeKeyOnFaulted)
                                 {
                                     task.ContinueWith(
                                         _ =>
                                             {
                                                 Lazy<Task<TValue>> useless;
                                                 _cache.TryRemove(key, out useless);
                                             },
                                         TaskContinuationOptions.OnlyOnFaulted);
                                 }

                                 return task;
                             })).Value;
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key from the ConcurrentDictionary<TKey, Lazy<Task<TValue>>>.
        /// </summary>
        /// <param name="key">The key of the element</param>
        /// <param name="value">The task to generate value for the key</param>
        /// <returns>true if the key was found in the ConcurrentDictionary<TKey, Lazy<Task<TValue>>>; otherwise, false.</returns>
        public bool TryGetValue(TKey key, out Task<TValue> value)
        {
            Guard.ArgumentNotNull(key, nameof(key));

            Lazy<Task<TValue>> lazyValue;
            var result = _cache.TryGetValue(key, out lazyValue);

            value = null;
            if (lazyValue != null)
            {
                value = lazyValue.Value;
            }

            return result;
        }

        /// <summary>
        /// Gets a List containing the values in the ConcurrentDictionary<TKey, Lazy<Task<TValue>>>.
        /// </summary>
        public List<Task<TValue>> Values
        {
            get
            {
                return _cache.Values.Select(x => x.Value).ToList();
            }
        }
    }
}
