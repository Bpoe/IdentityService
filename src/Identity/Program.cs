using Azure.Core;
using Azure.Identity;
using Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication
    .CreateBuilder(args);

builder.Services
    .AddOptions()
    .AddWindowsService()
    .AddTransient<TokenService>()
    //.AddSingleton<TokenCredential>(TokenCredentialFactory.GetTokenCredential(
    //    builder.Configuration.Get<IdentityOptions>() ?? new IdentityOptions()))
    .AddSingleton(builder.Configuration.Get<IdentityOptions>() ?? new IdentityOptions())
    .AddSingleton<TokenCredential>(new DefaultAzureCredential(new DefaultAzureCredentialOptions { ExcludeManagedIdentityCredential = true }))
    .AddDistributedMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.MapGet(
    "metadata/identity/oauth2/token",
    async ([FromQuery(Name = "resource")] string resource, [FromHeader(Name = "Authorization")] string? authorizationKey, [FromServices] TokenService applicationTokenService)
        => await applicationTokenService.GetTokenAsync(resource, authorizationKey));

await app.RunAsync();
