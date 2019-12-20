namespace Identity
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Infrastructure;
    using Microsoft.Identity.Client;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<ConfidentialClientApplicationOptions>(this.Configuration.GetSection("Identity"));
            services.Configure<CertificateOptions>(this.Configuration.GetSection("Certificate"));

            var provider = services.BuildServiceProvider();
            var options = provider.GetService<IOptions<ConfidentialClientApplicationOptions>>();
            var certificateOptions = provider.GetService<IOptions<CertificateOptions>>();

            var certificateProvider = new CertificateProvider(certificateOptions.Value.StoreName, certificateOptions.Value.StoreLocation);
            var certificate = certificateProvider.FindCertificate(certificateOptions.Value.Thumbprint);

            var app = ConfidentialClientApplicationBuilder
                .CreateWithApplicationOptions(options.Value)
                .WithCertificate(certificate)
                .Build();

            services.AddSingleton(app);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
