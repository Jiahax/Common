namespace Microsoft.OpenPublishing.Build.Common
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;

    public static class HttpHelper
    {
        /// <summary>
        /// Get a header's value from a <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="name">The header name.</param>
        /// <returns>The header's value. Null if the header doesn't exist.</returns>
        public static string GetHeaderValue(HttpRequestMessage request, string name)
        {
            Guard.ArgumentNotNull(request, nameof(request));
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));

            return GetHeaderValue(request.Headers, name);
        }

        /// <summary>
        /// Get a header's value from a <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="name">The header name.</param>
        /// <returns>The header's value. Null if the header doesn't exist.</returns>
        public static string GetHeaderValue(HttpResponseMessage response, string name)
        {
            Guard.ArgumentNotNull(response, nameof(response));
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));

            return GetHeaderValue(response.Headers, name);
        }

        private static string GetHeaderValue(HttpHeaders headers, string name)
        {
            IEnumerable<string> values;
            return headers.TryGetValues(name, out values) ? values.FirstOrDefault() : null;
        }
    }
}
