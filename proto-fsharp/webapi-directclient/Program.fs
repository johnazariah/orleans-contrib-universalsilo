namespace GeneratedProjectName.WebApiDirectClient

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.ResponseCompression
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.OpenApi.Models
open System
open System.IO
open System.IO.Compression
open System.Reflection
open System.Text.Json.Serialization
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Diagnostics.HealthChecks
open Microsoft.Extensions.Diagnostics.HealthChecks
open Microsoft.AspNetCore.Http
open Orleans.Contrib.UniversalSilo

[<AutoOpen>]
module WebApiConfigurator =
    let private healthResultStatusCodes =
        [
            (HealthStatus.Healthy,   StatusCodes.Status200OK);
            (HealthStatus.Degraded,  StatusCodes.Status200OK);
            (HealthStatus.Unhealthy, StatusCodes.Status503ServiceUnavailable)
        ]
        |> dict

    let private Version = "v1"                                                                    // revision this appropriately
    let private Title = "My Orleans API"                                                          // title this application
    let private Description = "An application with an Orleans backend and a WebAPI interface"     // describe this application
    let private TermsOfService = new Uri("http://127.0.0.1")                                      // replace with <Your TOS Uri>
    let private Contact_Name = "A Sagacious Developer"                                            // replace with <Your Name>
    let private Contact_Email = "<Your Email>"                                                    // replace with <Your Email>
    let private Contact_Url = new Uri("http://127.0.0.1")                                         // replace with <Your Uri>
    let private License_Name = "A generous license"                                               // replace with <Your License Name>
    let private License_Url = new Uri("http://127.0.01")                                          // replace with <Your License Uri>
    let private Contact = new OpenApiContact(Name = Contact_Name, Email = Contact_Email, Url = Contact_Url)
    let private License = new OpenApiLicense(Name = License_Name, Url = License_Url)

    let apiInfo =
        OpenApiInfo(
            Version = Version,
            Title = Title,
            Description = Description,
            TermsOfService = TermsOfService,
            Contact = Contact,
            License = License)

    let useHttpsRedirection = false

    type IHostBuilder with
        /// Configure the WebApi Host
        member this.ConfigureWebApi() : IHostBuilder =
            let configureApp (webHostBuilderContext : WebHostBuilderContext) (applicationBuilder : IApplicationBuilder) =
                let hostEnv = webHostBuilderContext.HostingEnvironment
                let swaggerUri  = "/swagger/v1/swagger.json"
                let swaggerName = sprintf "%s %s" apiInfo.Title apiInfo.Version

                let builder =
                    match hostEnv.IsDevelopment() with
                    | false -> applicationBuilder
                    | true  -> applicationBuilder.UseDeveloperExceptionPage()

                if useHttpsRedirection then
                    ignore <| builder.UseHttpsRedirection()

                builder
                    .UseDefaultFiles()
                    .UseStaticFiles()
                    .UseSwagger()
                    .UseSwaggerUI(fun options -> options.SwaggerEndpoint(swaggerUri, swaggerName))
                    .UseResponseCompression()
                    .UseRouting()
                    .UseAuthorization()
                    .UseEndpoints(fun endpoints ->
                        endpoints.MapControllers()
                        |> ignore

                        endpoints.MapHealthChecks(
                            "/health",
                            new HealthCheckOptions(
                                AllowCachingResponses = false,
                                ResultStatusCodes = healthResultStatusCodes
                            ))
                        |> ignore)
                |> ignore

            let configureServices (hostBuilderContext : HostBuilderContext) (services : IServiceCollection) =
                services
                    .AddControllers()
                    .AddJsonOptions(fun options -> options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
                |> ignore

                let xmlFile = sprintf "%s.xml" (Assembly.GetEntryAssembly().GetName().Name)
                let xmlPath = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, xmlFile)
                services.AddSwaggerGen(fun options ->
                    options.SwaggerDoc (apiInfo.Version, apiInfo)
                    options.EnableAnnotations()
                    options.IncludeXmlComments(xmlPath))
                |> ignore

                services
                    .AddHealthChecks()
                    .AddCheck<ClusterHealthCheck>(nameof(ClusterHealthCheck))
                    .AddCheck<LocalHealthCheck>(nameof(LocalHealthCheck))
                    .AddCheck<StorageHealthCheck>(nameof(StorageHealthCheck))
                    .AddCheck<SiloHealthCheck>(nameof(SiloHealthCheck))
                |> ignore

                services
                    .AddResponseCompression()
                    .Configure(fun (options : BrotliCompressionProviderOptions) ->
                        options.Level <- CompressionLevel.Optimal)
                    .Configure(fun (options : GzipCompressionProviderOptions) ->
                        options.Level <- CompressionLevel.Optimal)
                |> ignore

            this
                .ConfigureWebHostDefaults(fun webHostBuilder ->
                    webHostBuilder
                        .Configure(configureApp)
                        .UseSetting(WebHostDefaults.ApplicationKey, Assembly.GetEntryAssembly().GetName().Name)
                        |> ignore)
                .ConfigureServices(configureServices)



/// Override methods in this class to take over how the silo is configured
type SiloConfigurator () = class
    inherit Orleans.Contrib.UniversalSilo.Configuration.SiloConfigurator()

    override __.SiloConfiguration =
        base.SiloConfiguration.ServiceId <- "GeneratedProjectName"
        base.SiloConfiguration


    // override __.ClusteringConfiguration =
    //    base.ClusteringConfiguration

    // override __.StorageProviderConfiguration =
    //    base.StorageProviderConfiguration

    // override __.TelemetryConfiguration =
    //    base.TelemetryConfiguration

    // override __.ConfigureServices(configuration, siloSettings, siloBuilder) =
    //    base.ConfigureServices(configuration, siloSettings, siloBuilder)

    // override __.ConfigureClustering(configuration, siloSettings, ISiloBuilder siloBuilder) =
    //    base.ConfigureClustering(configuration, siloSettings, siloBuilder)

    // override __.ConfigureStorageProvider(configuration, siloSettings, ISiloBuilder siloBuilder) =
    //    base.ConfigureStorageProvider(configuration, siloSettings, siloBuilder)

    // override __.ConfigureReminderService(configuration, siloSettings, siloBuilder) =
    //    base.ConfigureReminderService(configuration, siloSettings, siloBuilder)

    // override __.ConfigureApplicationInsights(configuration, siloSettings, siloBuilder) =
    //    base.ConfigureApplicationInsights(configuration, siloSettings, siloBuilder)

    // override __.ConfigureDashboard(configuration, siloSettings, siloBuilder) =
    //    base.ConfigureDashboard(configuration, siloSettings, siloBuilder)

    // override __.ConfigureApplicationParts(siloBuilder) =
    //    base.ConfigureApplicationParts(siloBuilder)
end

module Program =
    /// This is the entry point to the silo.
    [<EntryPoint>]
    let Main args =
        (Host.CreateDefaultBuilder args)
            .ConfigureHostConfiguration(fun builder -> ignore <| builder.SetBasePath(Directory.GetCurrentDirectory()))
            .UseOrleans((new SiloConfigurator()).ConfigurationFunc)
            .ConfigureWebApi()
            .Build()
            .Run()
        0
