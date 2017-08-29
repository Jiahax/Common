namespace Microsoft.OpenPublishing.Build.Common
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class HttpAutoRedirectClientHandler : HttpClientHandler
    {
        public HttpAutoRedirectClientHandler()
        {
            AllowAutoRedirect = false;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (IsRedirectResponse(response.StatusCode))
            {
                // by default HttpClientHandler will redirect without keeping original authorization information in request headers
                // so here do redirect by ourselves
                request.RequestUri = response.Headers.Location;
                if (request.RequestUri != null)
                {
                    response = await base.SendAsync(request, cancellationToken);
                }
            }
            return response;
        }

        protected virtual bool IsRedirectResponse(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.MultipleChoices
                   || statusCode == HttpStatusCode.MovedPermanently
                   || statusCode == HttpStatusCode.Found
                   || statusCode == HttpStatusCode.SeeOther
                   || statusCode == HttpStatusCode.TemporaryRedirect;
        }
    }
}