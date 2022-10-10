namespace Identity;

using Identity.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;

public class Startup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        this.Configuration = configuration;
        this.Environment = env;
    }

    public IConfiguration Configuration { get; }

    public IWebHostEnvironment Environment { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        var certificateOptions = this.Configuration.Get<IdentityOptions>();
        var options = this.Configuration.GetSection("Identity").Get<ConfidentialClientApplicationOptions>();

        services
            .AddOptions()
            .AddControllers()
            .AddNewtonsoftJson();

        services.AddSingleton(s =>
        {
            var appBuilder = ConfidentialClientApplicationBuilder
                .CreateWithApplicationOptions(options);

            if (string.IsNullOrEmpty(options.ClientSecret)
                && !string.IsNullOrEmpty(certificateOptions.CertificateSubject))
            {
                var certificate = new CertificateProvider(
                        certificateOptions.CertificateStoreName,
                        certificateOptions.CertificateStoreLocation)
                    .FindCertificate(certificateOptions.CertificateSubject);
                appBuilder.WithCertificate(certificate);
            }

            return appBuilder.Build();
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app)
    {
        if (this.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        //app.UseHttpsRedirection();
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
