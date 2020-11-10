# Customizing the SiloConfigurator

In the project's `Program.cs`, you will find the following class:

```csharp
    /// <summary>
    /// Override methods in this class to take over how the silo is configured
    /// </summary>
    class SiloConfigurator : Configuration.SiloConfigurator
    {
        public override SiloConfiguration SiloConfiguration =>
            base.SiloConfiguration
            .With(_c => _c.ServiceId = "Cornflake");

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
```

This class is the main customization point for the silo configuration boilerplate that comes with the `Orleans.UniversalSilo` library. 

The extension points supported are functions and properties that can be overriden from the base class to adjust and extend the behaviour of the default configuration. Given the familiar `IConfiguration` and `ISiloBuilder` objects, as well as the `UniversalSiloConfiguration` object in some cases, you can extend the functionality by returning an appropriately configured `ISiloBuilder` object. 