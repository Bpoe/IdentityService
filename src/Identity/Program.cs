using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Identity;

var builder = WebApplication
    .CreateBuilder(args);

builder.Services
    .AddOptions()
    .AddWindowsService()
    .AddSingleton<TokenCredential>(new ClientSecretCredential(
        builder.Configuration["TenantId"],
        builder.Configuration["ClientId"],
        builder.Configuration["ClientSecret"]))
    .AddTransient<TokenService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.MapGet(
    "metadata/identity/oauth2/token",
    async ([FromQuery(Name = "resource")] string resource, [FromServices] TokenService tokenService)
        => await tokenService.Token(resource));

await app.RunAsync();
