using System.Net;
using System.Net.Http.Headers;
using Laba.Shared.Domain.Models;

namespace Laba.Shared.Exceptions;

public class BadRequestException(
    string message,
    HttpStatusCode statusCode,
    HttpResponseHeaders? headers,
    string? content) : BaseHttpException<string>(message, statusCode, headers, content)
{
}
