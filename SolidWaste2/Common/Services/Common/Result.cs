using System.Net;

namespace Common.Services.Common;

public class Result
{
    public bool Successful { get; set; }
    public string Message { get; set; }
}

public class Result<T> : Result
{
    public T Value { get; set; } 
}

public class HttpResult : Result
{
    public HttpStatusCode StatusCode { get; set; }
    public string ReasonPhrase { get; set; }
}

public class HttpResult<T> : Result<T>
{
    public HttpStatusCode StatusCode { get; set; }
    public string ReasonPhrase { get; set; }
}
