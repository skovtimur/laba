using System.Net;
using System.Net.Http.Headers;

namespace Laba.Shared.Exceptions;

public class TooManyRequestsException(
    string message,
    HttpStatusCode statusCode,
    HttpResponseHeaders? headers,
    string? content) : BaseHttpException<string>(message, statusCode, headers, content)
{
}