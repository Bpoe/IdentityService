namespace Identity
{
    using Identity.Infrastructure;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
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
            services
                .AddControllers()
                .AddNewtonsoftJson();

            services
                .AddOptions()
                .Configure<ConfidentialClientApplicationOptions>(this.Configuration.GetSection("Identity"))
                .Configure<CertificateOptions>(this.Configuration.GetSection("Certificate"));

            services.AddSingleton(s =>
            {
                var options = s.GetService<IOptions<ConfidentialClientApplicationOptions>>();

                var appBuilder = ConfidentialClientApplicationBuilder
                    .CreateWithApplicationOptions(options.Value);

                var certificateOptions = s.GetService<IOptions<CertificateOptions>>();
                if (string.IsNullOrEmpty(options.Value.ClientSecret) && !string.IsNullOrEmpty(certificateOptions.Value?.Criteria))
                {
                    var certificateProvider = new CertificateProvider(certificateOptions.Value.StoreName, certificateOptions.Value.StoreLocation);
                    var certificate = certificateProvider.FindCertificate(certificateOptions.Value.Criteria);
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
}
