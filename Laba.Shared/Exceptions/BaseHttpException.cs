using System.Net;
using System.Net.Http.Headers;

namespace Laba.Shared.Exceptions;

public abstract class BaseHttpException<TContent>(
    string message,
    HttpStatusCode statusCode,
    HttpResponseHeaders? headers,
    TContent? content)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public HttpResponseHeaders? Headers { get; } = headers;
    public TContent? Content { get; } = content;


    public bool HaveSomeHeaders => Headers != null && Headers.Any();
    public bool HaveContent => Content != null;
}