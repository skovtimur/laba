using System.Net;
using System.Net.Http.Headers;
using Laba.Shared.Domain.Models;

namespace Laba.Shared.Exceptions;

public class InvalidPropertyException(
    string message,
    HttpStatusCode statusCode,
    HttpResponseHeaders? headers,
    ErrorOfProperty? content) : BaseHttpException<ErrorOfProperty>(message, statusCode, headers, content)
{
}
