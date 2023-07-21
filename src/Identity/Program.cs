using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Identity;

var builder = WebApplication
    .CreateBuilder(args);

builder.Services
    .AddOptions()
    .AddWindowsService()
    .AddTransient<TokenService>()
    .AddSingleton(builder.Configuration.Get<TokenCredentialOptions>() ?? new TokenCredentialOptions())
    .AddHttpClient<TenantIdResolver>();

// Add appropriate cache
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.MapGet(
    "metadata/identity/oauth2/token",
    async ([FromQuery(Name = "resource")] string resource, [FromQuery(Name = "challengeResource")] string? challengeResource, [FromServices] TokenService applicationTokenService)
        => await applicationTokenService.GetTokenAsync(resource, challengeResource));

app.MapGet(
    "oauth2/token",
    async ([FromQuery(Name = "resource")] string resource, [FromQuery(Name = "challengeResource")] string? challengeResource, [FromServices] TokenService applicationTokenService)
        => await applicationTokenService.GetTokenAsync(resource, challengeResource));

await app.RunAsync();
