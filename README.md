# MVCWithOpenTelemetry
This sample app shows emitting custom telemetry, adding packages to collect automatic telemetry, and open telemetry export to Azure Monitor/Application Insights
It's dependencies are:
1. Start the MoviesAPI before you start the MVCWithOpenTelemetry app to be able to get the weather to work
2. You need a SQL database to get the movies controller to work. It can be a local sql server, sql express, or azure sql
	1. Create a sql database somewhere
	2. migrate the application's ef context to the sql database https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli
3. You need an Application Insights instance for the Azure Monitor export to work. You can create those for free in Azure (portal.azure.com).
	1. Create an appinsights resource in azure
	2. copy the connection string for that resource to your appsettings.json AppSettings.AppInsightsCN key.


## Demo points
1. Start both apps from a dotnet console to see the console output of telemetry. Do the following for each of the two applications:
   1. Open a command prompt
   2. CD to the project directory
   3. Type dotnet run
3. Looking up the weather triggers a service call to the Movies API, which generates telemetry
4. Adding a movie causes a Metric to be incremented
5. Viewing the Privacy page causes a custom log entry to be emitted
6. The OpenTelemetry libraries added as packages are also instrumenting the application themselves, producing a rich set of telemetry for distributed tracing and monitoring.
7. All the code to get that telemetry and export it is in the Program.cs. There are three key sections

### Logging
```
builder.Services.AddLogging(loggingbuilder=>
{
    loggingbuilder
    .ClearProviders()
    .AddConsole()
    .AddOpenTelemetry() //Adds Open Telemetry style logging
    .AddEventLog();
});
```

### Activity Tracing
```
builder.Services.AddOpenTelemetryTracing(tracingProviderBuilder =>
{
    tracingProviderBuilder
    .AddConsoleExporter()
    .AddSource(serviceName)
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddSqlClientInstrumentation()
    .AddAzureMonitorTraceExporter(o =>
    {
        //to use this, you will need to setup an AppInsights resource in Azure and get the connection string
        //from the azure portal, and put it into the appSettings.json file under the AppInsightsCN key.
        o.ConnectionString = builder.Configuration.GetValue<string>("AppSettings:AppInsightsCN");
    });
});
```
### Metrics
```  
using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter(serviceName)
            .AddConsoleExporter()
            .AddAspNetCoreInstrumentation()
            .Build();
```
