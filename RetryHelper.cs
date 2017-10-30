namespace Common
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public static class RetryHelper
    {
        public static TResult Retry<TResult>(
            Func<TResult> func,
            Func<Exception, bool>[] errorHandlers,
            TimeSpan[] retryIntervals,
            [CallerMemberName] string methodName = null,
            [CallerFilePath] string filePath = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            Guard.ArgumentNotNull(func, nameof(func));
            ValidateRetryParameters(errorHandlers, retryIntervals);

            int retryCount = 0;
            while (true)
            {
                if (retryCount > 0)
                {
                    Thread.Sleep(retryIntervals[retryCount - 1]);
                }
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    if (retryCount++ < retryIntervals.Length)
                    {
                        if (errorHandlers.Any(handler => handler(ex)))
                        {
                            TraceEx.TraceWarning($"Error occurred at method {methodName} in {filePath} line {lineNumber}, current retry count: {retryCount}", ex);
                            continue;
                        }
                    }
                    throw;
                }
            }
        }

        public static void Retry(
            Action action,
            Func<Exception, bool>[] errorHandlers,
            TimeSpan[] retryIntervals,
            [CallerMemberName] string methodName = null,
            [CallerFilePath] string filePath = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            Guard.ArgumentNotNull(action, nameof(action));
            ValidateRetryParameters(errorHandlers, retryIntervals);

            int retryCount = 0;
            while (true)
            {
                if (retryCount > 0)
                {
                    Thread.Sleep(retryIntervals[retryCount - 1]);
                }
                try
                {
                    action();
                    break;
                }
                catch (Exception ex)
                {
                    if (retryCount++ < retryIntervals.Length)
                    {
                        if (errorHandlers.Any(handler => handler(ex)))
                        {
                            TraceEx.TraceWarning($"Error occurred at method {methodName} in {filePath} line {lineNumber}, current retry count: {retryCount}", ex);
                            continue;
                        }
                    }
                    throw;
                }
            }
        }

        public static async Task<TResult> RetryAsync<TResult>(
            Func<CancellationToken, Task<TResult>> func,
            Func<Exception, bool>[] errorHandlers,
            TimeSpan[] timeouts,
            CancellationToken cancellationToken = default(CancellationToken),
            [CallerMemberName] string methodName = null,
            [CallerFilePath] string filePath = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            Guard.ArgumentNotNull(func, nameof(func));
            ValidateRetryParameters(errorHandlers, timeouts);

            int retryCount = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (retryCount > 0)
                {
                    await Task.Delay(timeouts[retryCount - 1], cancellationToken);
                }
                try
                {
                    return await func(cancellationToken);
                }
                catch (Exception ex)
                {
                    if (retryCount++ < timeouts.Length)
                    {
                        if (errorHandlers.Any(handler => handler(ex)))
                        {
                            TraceEx.TraceWarning($"Error occurred at method {methodName} in {filePath} line {lineNumber}, current retry count: {retryCount}", ex);
                            continue;
                        }
                    }
                    throw;
                }
            }
        }

        public static async Task RetryAsync(
            Func<CancellationToken, Task> func,
            Func<Exception, bool>[] errorHandlers,
            TimeSpan[] timeouts,
            CancellationToken cancellationToken = default(CancellationToken),
            [CallerMemberName] string methodName = null,
            [CallerFilePath] string filePath = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            Guard.ArgumentNotNull(func, nameof(func));
            ValidateRetryParameters(errorHandlers, timeouts);

            int retryCount = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (retryCount > 0)
                {
                    await Task.Delay(timeouts[retryCount - 1], cancellationToken);
                }
                try
                {
                    await func(cancellationToken);
                    break;
                }
                catch (Exception ex)
                {
                    if (retryCount++ < timeouts.Length)
                    {
                        if (errorHandlers.Any(handler => handler(ex)))
                        {
                            TraceEx.TraceWarning($"Error occurred at method {methodName} in {filePath} line {lineNumber}, current retry count: {retryCount}", ex);
                            continue;
                        }
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Check whether the exception is certain WebException in HttpRequestException, see https://msdn.microsoft.com/en-us/library/dn589788.aspx
        /// Seems we are not retrying in certain cases, update to retry on all kinds of WebException
        /// </summary>
        /// <param name="exception">Catched exception</param>
        /// <returns>Return true if retryable, otherwise false</returns>
        public static bool IsRetryableWebException(Exception exception)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));

            var httpRequestException = exception as HttpRequestException;
            return httpRequestException?.InnerException is WebException;
        }

        #region Private Methods

        private static void ValidateRetryParameters(Func<Exception, bool>[] errorHandlers, TimeSpan[] retryIntervals)
        {
            Guard.ArgumentNotNull(errorHandlers, nameof(errorHandlers));
            Guard.Argument(() => errorHandlers.All(errorHandler => errorHandler != null), nameof(errorHandlers), $"{nameof(errorHandlers)} cannot contain null element.");
            Guard.ArgumentNotNull(retryIntervals, nameof(retryIntervals));
            Guard.Argument(
                () => retryIntervals.All(timeout => timeout.CompareTo(TimeSpan.Zero) >= 0),
                nameof(retryIntervals),
                $"{nameof(retryIntervals)} should only contain non-negative timeout value.");
        }

        #endregion
    }
}
