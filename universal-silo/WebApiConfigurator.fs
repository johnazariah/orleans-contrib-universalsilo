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

[<AbstractClass>]
type WebApiConfigurator (apiInfo : OpenApiInfo) = class
    member val Configuration : IConfiguration = null with get, set

    member this.SetConfigurationObject (hostBuilderContext : HostBuilderContext) =
        this.Configuration <- hostBuilderContext.Configuration

    abstract AddSwaggerGen : IServiceCollection -> IServiceCollection
    default __.AddSwaggerGen services =
        let xmlFile = sprintf "%s.xml" (Assembly.GetEntryAssembly().GetName().Name)
        let xmlPath = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, xmlFile)
        services.AddSwaggerGen(fun options ->
            options.SwaggerDoc (apiInfo.Version, apiInfo)
            options.EnableAnnotations();
            options.IncludeXmlComments(xmlPath))

    abstract AddResponseCompression : IServiceCollection -> IServiceCollection
    default __.AddResponseCompression services =
        services.AddResponseCompression()
            .Configure(fun (options : BrotliCompressionProviderOptions) ->
                options.Level <- CompressionLevel.Optimal)
            .Configure(fun (options : GzipCompressionProviderOptions) ->
                options.Level <- CompressionLevel.Optimal);

    abstract ConfigureServices : HostBuilderContext -> IServiceCollection -> IServiceCollection
    default this.ConfigureServices hostBuilderContext services =
        services
        |> (fun sc -> sc.AddControllers())
        |> (fun mvc -> mvc.AddJsonOptions (fun options -> options.JsonSerializerOptions.Converters.Add (JsonStringEnumConverter()) ))
        |> ignore

        services
        |> this.AddSwaggerGen
        |> this.AddResponseCompression

    abstract Configure : IApplicationBuilder -> IHostEnvironment -> unit
    default __.Configure appBuilder hostEnv =
        appBuilder
        |> (fun app -> if (not <| hostEnv.IsDevelopment ()) then app else app.UseDeveloperExceptionPage() |> ignore; app)
        |> (fun app -> app.UseDefaultFiles ())
        |> (fun app -> app.UseStaticFiles  ())
        |> (fun app -> app.UseHttpsRedirection    ())
        |> (fun app -> app.UseSwagger             ())
        |> (fun app -> app.UseSwaggerUI (fun options ->
            options.SwaggerEndpoint ("/swagger/v1/swagger.json", (sprintf "%s %s" apiInfo.Title apiInfo.Version))))
        |> (fun app -> app.UseResponseCompression ())
        |> (fun app -> app.UseRouting             ())
        |> (fun app -> app.UseAuthorization       ())
        |> (fun app -> app.UseEndpoints (fun endpoints -> endpoints.MapControllers() |> ignore))
        |> ignore

    abstract ConfigureWebHost : IWebHostBuilder -> IWebHostBuilder
    default this.ConfigureWebHost builder =
        let applicationName = Assembly.GetEntryAssembly().GetName().Name

        builder
        |> (fun b -> b.Configure (fun ctx app -> this.Configure app (ctx.HostingEnvironment)))
        |> (fun b -> b.UseKestrel(fun options -> options.Limits.MaxRequestBodySize <- System.Nullable ()))
        |> (fun b -> b.UseSetting (WebHostDefaults.ApplicationKey, applicationName))

    abstract ConfigureWebApiHost : IHostBuilder -> IHostBuilder
    default this.ConfigureWebApiHost builder =
        builder
        |> (fun b -> b.ConfigureWebHostDefaults(fun _b -> this.ConfigureWebHost _b |> ignore))
        |> (fun b -> b.ConfigureServices(fun hostBuilderContext _ -> this.SetConfigurationObject hostBuilderContext |> ignore))
        |> (fun b -> b.ConfigureServices(fun hostBuilderContext services -> this.ConfigureServices hostBuilderContext services |> ignore))
end

