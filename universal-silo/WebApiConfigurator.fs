namespace Orleans.Contrib.UniversalSilo

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

[<AbstractClass>]
type WebApiConfigurator (apiInfo : OpenApiInfo) = class
    let mutable configuration : IConfiguration = null
    member val public Configuration = configuration

    member __.SetConfigurationObject (hostBuilderContext : HostBuilderContext) =
        configuration <- hostBuilderContext.Configuration

    abstract AddSwaggerGen : IServiceCollection -> IServiceCollection
    default __.AddSwaggerGen services =
        let xmlFile = sprintf "%s.xml" (Assembly.GetEntryAssembly().GetName().Name)
        let xmlPath = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, xmlFile)
        services.AddSwaggerGen(fun options ->
            options.SwaggerDoc (apiInfo.Version, apiInfo)
            options.EnableAnnotations()
            options.IncludeXmlComments(xmlPath))

    abstract AddResponseCompression : IServiceCollection -> IServiceCollection
    default __.AddResponseCompression services =
        services
            .AddResponseCompression()
            .Configure(fun (options : BrotliCompressionProviderOptions) ->
                options.Level <- CompressionLevel.Optimal)
            .Configure(fun (options : GzipCompressionProviderOptions) ->
                options.Level <- CompressionLevel.Optimal)

    abstract ConfigureHealthChecks : HostBuilderContext -> IServiceCollection -> IServiceCollection
    default __.ConfigureHealthChecks hostBuilderContext services =
        services
            .AddHealthChecks()
            .AddCheck<ClusterHealthCheck>(nameof(ClusterHealthCheck))
            .AddCheck<LocalHealthCheck>(nameof(LocalHealthCheck))
            .AddCheck<StorageHealthCheck>(nameof(StorageHealthCheck))
            .AddCheck<SiloHealthCheck>(nameof(SiloHealthCheck))
        |> ignore

        services

    abstract ConfigureServices : HostBuilderContext -> IServiceCollection -> IServiceCollection
    default this.ConfigureServices hostBuilderContext services =
        services
            .AddControllers()
            .AddJsonOptions(fun options -> options.JsonSerializerOptions.Converters.Add(JsonStringEnumConverter()))
        |> ignore

        services
        |> this.AddSwaggerGen
        |> this.AddResponseCompression

    abstract Configure : IApplicationBuilder -> IHostEnvironment -> unit
    default __.Configure appBuilder hostEnv =
        let swaggerUri  = "/swagger/v1/swagger.json"
        let swaggerName = sprintf "%s %s" apiInfo.Title apiInfo.Version

        let builder =
            match hostEnv.IsDevelopment() with
            | false -> appBuilder
            | true  -> appBuilder.UseDeveloperExceptionPage()

        let healthResultStatusCodes =
            [
                (HealthStatus.Healthy,   StatusCodes.Status200OK);
                (HealthStatus.Degraded,  StatusCodes.Status200OK);
                (HealthStatus.Unhealthy, StatusCodes.Status503ServiceUnavailable)
            ]
            |> dict

        builder
            .UseDefaultFiles()
            .UseStaticFiles()
            .UseHttpsRedirection()
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

    abstract ConfigureWebHost : IWebHostBuilder -> IWebHostBuilder
    default this.ConfigureWebHost builder =
        builder
        |> (fun b -> b.Configure(fun ctx app -> this.Configure app ctx.HostingEnvironment))
        |> (fun b -> b.UseSetting(WebHostDefaults.ApplicationKey, Assembly.GetEntryAssembly().GetName().Name))

    abstract ConfigureWebApiHost : IHostBuilder -> IHostBuilder
    default this.ConfigureWebApiHost builder =
        builder
            .ConfigureWebHostDefaults(fun _b -> this.ConfigureWebHost _b |> ignore)
            .ConfigureServices(fun hostBuilderContext _        -> this.SetConfigurationObject hostBuilderContext          |> ignore)
            .ConfigureServices(fun hostBuilderContext services -> this.ConfigureHealthChecks  hostBuilderContext services |> ignore)
            .ConfigureServices(fun hostBuilderContext services -> this.ConfigureServices      hostBuilderContext services |> ignore)
end

