using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

public static class HttpRequestExtensions
{
    public static string GetRawUrl(this HttpRequest request)
    {
        var httpContext = request.HttpContext;
        var requestFeature = httpContext.Features.Get<IHttpRequestFeature>();
        return requestFeature.RawTarget;
    }
}