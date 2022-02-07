using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json.Serialization;
using static Orleans.Contrib.UniversalSilo.Configuration;
using static Orleans.Contrib.UniversalSilo.HealthChecks;
using Orleans.Contrib.UniversalSilo;

namespace GeneratedProjectName.WebApiDirectClient
{
    /// <summary>
    /// Override methods in this class to take over how the web-api host is configured
    /// </summary>
    static class WebApiConfigurator
    {
        private static readonly OpenApiInfo apiInfo =
            new OpenApiInfo
            {
                Version = "v1",                                                                // revision this appropriately
                Title = "My Orleans API",                                                      // title this application
                Description = "An application with an Orleans backend and a WebAPI interface", // describe this application
                TermsOfService = new Uri("http://127.0.0.1"),                                  // replace with <Your TOS Uri>
                Contact = new OpenApiContact()
                {
                    Name = "A Sagacious Developer",                                            // replace with <Your Name>
                    Email = "<Your Email>",                                                    // replace with <Your Email>
                    Url = new Uri("http://127.0.0.1"),                                         // replace with <Your Uri>
                },
                License = new OpenApiLicense
                {
                    Name = "A generous license",                                               // replace with <Your License Name>
                    Url = new Uri("http://127.0.01"),                                          // replace with <Your License Uri>
                },
            };

        private static bool useHttpsRedirection = false;

        private static Dictionary<HealthStatus, int> healthResultStatusCodes = new Dictionary<HealthStatus, int>()
        {
            [HealthStatus.Healthy  ] = StatusCodes.Status200OK,
            [HealthStatus.Degraded ] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        };

        public static IHostBuilder ConfigureWebApi(this IHostBuilder hostBuilder) =>
            hostBuilder
                .ConfigureWebHostDefaults(webHostBuilder =>
                    webHostBuilder
                        .Configure((webHostBuilderContext, applicationBuilder) =>
                            {
                                var hostEnv = webHostBuilderContext.HostingEnvironment;
                                var swaggerUri = "/swagger/v1/swagger.json";
                                var swaggerName = $"{apiInfo.Title} {apiInfo.Version}";

                                var builder = hostEnv.IsDevelopment() ? applicationBuilder.UseDeveloperExceptionPage() : applicationBuilder;
                                if (useHttpsRedirection) builder.UseHttpsRedirection();

                                builder
                                    .UseDefaultFiles()
                                    .UseStaticFiles()
                                    .UseSwagger()
                                    .UseSwaggerUI(options => options.SwaggerEndpoint(swaggerUri, swaggerName))
                                    .UseResponseCompression()
                                    .UseRouting()
                                    .UseAuthorization()
                                    .UseEndpoints(endpoints =>
                                        {
                                            endpoints.MapControllers();
                                            endpoints.MapHealthChecks(
                                                "/health",
                                                new HealthCheckOptions()
                                                {
                                                    AllowCachingResponses = false,
                                                    ResultStatusCodes = healthResultStatusCodes
                                                });
                                        });
                            })
                        .UseSetting(WebHostDefaults.ApplicationKey, Assembly.GetEntryAssembly().GetName().Name))
                .ConfigureServices((hostBuilderContext, services) =>
                {
                    services
                        .AddControllers()
                        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

                    var xmlFile = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, xmlFile);
                    services.AddSwaggerGen(options =>
                        {
                            options.SwaggerDoc(apiInfo.Version, apiInfo);
                            options.EnableAnnotations();
                            options.IncludeXmlComments(xmlPath);
                        });

                    services
                        .AddHealthChecks()
                        .AddCheck<ClusterHealthCheck>(nameof(ClusterHealthCheck))
                        .AddCheck<LocalHealthCheck>(nameof(LocalHealthCheck))
                        .AddCheck<StorageHealthCheck>(nameof(StorageHealthCheck))
                        .AddCheck<SiloHealthCheck>(nameof(SiloHealthCheck));

                    services
                        .AddResponseCompression()
                        .Configure((BrotliCompressionProviderOptions options) => { options.Level = CompressionLevel.Optimal; })
                        .Configure((GzipCompressionProviderOptions options) => { options.Level = CompressionLevel.Optimal; });
                });
    }

    /// <summary>
    /// Override methods in this class to take over how the silo is configured
    /// </summary>
    class SiloConfigurator : Configuration.SiloConfigurator
    {
        public override SiloConfiguration SiloConfiguration =>
            base.SiloConfiguration
            .With(_c => _c.ServiceId = "GeneratedProjectName");

        //public override ClusteringConfiguration ClusteringConfiguration =>
        //    base.ClusteringConfiguration;

        //public override StorageProviderConfiguration StorageProviderConfiguration =>
        //    base.StorageProviderConfiguration;

        //public override TelemetryConfiguration TelemetryConfiguration =>
        //    base.TelemetryConfiguration;

        //public override Orleans.Hosting.ISiloBuilder ConfigureServices(IConfiguration configuration, UniversalSiloConfiguration siloSettings, Orleans.Hosting.ISiloBuilder siloBuilder) =>
        //    base.ConfigureServices(configuration, siloSettings, siloBuilder);

        //public override Orleans.Hosting.ISiloBuilder ConfigureClustering(IConfiguration configuration, UniversalSiloConfiguration siloSettings, Orleans.Hosting.ISiloBuilder siloBuilder) =>
        //    base.ConfigureClustering(configuration, siloSettings, siloBuilder);

        //public override Orleans.Hosting.ISiloBuilder ConfigureStorageProvider(IConfiguration configuration, UniversalSiloConfiguration siloSettings, Orleans.Hosting.ISiloBuilder siloBuilder) =>
        //    base.ConfigureStorageProvider(configuration, siloSettings, siloBuilder);

        //public override Orleans.Hosting.ISiloBuilder ConfigureReminderService(IConfiguration configuration, UniversalSiloConfiguration siloSettings, Orleans.Hosting.ISiloBuilder siloBuilder) =>
        //    base.ConfigureReminderService(configuration, siloSettings, siloBuilder);

        //public override Orleans.Hosting.ISiloBuilder ConfigureApplicationInsights(IConfiguration configuration, UniversalSiloConfiguration siloSettings, Orleans.Hosting.ISiloBuilder siloBuilder) =>
        //    base.ConfigureApplicationInsights(configuration, siloSettings, siloBuilder);

        //public override Orleans.Hosting.ISiloBuilder ConfigureDashboard(IConfiguration configuration, UniversalSiloConfiguration siloSettings, Orleans.Hosting.ISiloBuilder siloBuilder) =>
        //    base.ConfigureDashboard(configuration, siloSettings, siloBuilder);

        //public override Orleans.Hosting.ISiloBuilder ConfigureApplicationParts(Orleans.Hosting.ISiloBuilder siloBuilder) =>
        //    base.ConfigureApplicationParts(siloBuilder);

        public SiloConfigurator() : base()
        { }
    }

    /// <summary>
    ///
    /// This is the entry point to the silo.
    ///
    /// No changes should normally be needed here to start up a silo and a web-api front-end co-hosted in the same executable
    ///
    /// Provide the configuration of the silo to connect by any combination of (in order of override)
    ///    * The default configuration
    ///    * Overriding in the <see cref="SiloConfigurator"/> and <see cref="WebApiConfigurator"/>classes
    ///    * Providing a section in the "appSettings.json"/> file. (If at all possible, do not use this option.)
    ///    * Setting user secrets for managing secrets and connection strings in development
    ///    * Setting environment variables
    ///
    /// </summary>
    class Program
    {
        public static void Main(string[] args) =>
            CreateHostBuilder(args)
            .Build()
            .Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
            .CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(builder => builder.SetBasePath(Directory.GetCurrentDirectory()))
            .UseOrleans(new SiloConfigurator().ConfigurationFunc)
            .ConfigureWebApi();
    }
}
