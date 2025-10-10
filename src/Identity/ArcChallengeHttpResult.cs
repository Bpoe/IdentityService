namespace Identity;

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class ArcChallengeHttpResult : IResult
{
    private readonly string keyFile;

    public ArcChallengeHttpResult(string keyFile)
    {
        this.keyFile = keyFile;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.Headers.Append("WWW-Authenticate", $"Basic realm={this.keyFile}");
        httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

        return Task.CompletedTask;
    }
}