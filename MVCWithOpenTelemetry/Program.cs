using Microsoft.EntityFrameworkCore;
using MVCWithOpenTelemetry.Models;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using Azure.Monitor.OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();

//to use this you need to create a sql server or Azure SQL database and put that
//connection string in appSettings.json, 
//then migrate the database changes. https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli
builder.Services.AddDbContext<MvcMovieContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("MvcMovieContext")));
string weatherAPI = builder.Configuration.GetSection("AppSettings").GetValue<string>("WeatherSvcURL");

builder.Services.AddTransient<WeatherSvc.IClient, WeatherSvc.Client>((t) =>
{
    return new WeatherSvc.Client(weatherAPI, new HttpClient());
});



//
//Configure Telemetry Sources and Exporters
//

//Constants for telemetry
const string serviceName = "Patrick.Samples.MVCWithOpenTelemetry";
const string serviceVersion = "1.0.0";

//Setup Logging - this is just normal asp.net logging,
//with the addition of an OpenTelemetry logging provider
builder.Services.AddLogging(loggingbuilder=>
{
    loggingbuilder
    .ClearProviders()
    .AddConsole()
    .AddOpenTelemetry() //Adds Open Telemetry style logging
    .AddEventLog();
});

//Setup custom meters
//These are .NET meters, which are created in our own class and used in code
InstrumentationHelper ih = new InstrumentationHelper(serviceName, serviceVersion);

//Here, we take the meters that already exist, and export them via opentelemetry
using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter(serviceName)
            .AddConsoleExporter()
            .AddAspNetCoreInstrumentation()
            .Build();

//Add meter dependency for injection into consumers
builder.Services.AddSingleton<InstrumentationHelper>(ih);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


/// <summary>
/// Exposes .NET Meters for creating custom metrics counters
/// </summary>
public class InstrumentationHelper : IDisposable
{
    private string _serviceName;
    private string _serviceVersion;
    public readonly ActivitySource MyActivitySource;
    public Meter MoviesMeter { get; init; }
    public InstrumentationHelper(string serviceName, string serviceVersion)
    {
        _serviceName = serviceName;
        _serviceVersion = serviceVersion;

        MoviesMeter = new Meter(_serviceName, _serviceVersion);
        MoviesAddedCounter = MoviesMeter.CreateCounter<long>("MoviesAdded");

        MyActivitySource = new ActivitySource($"OTel.AzureMonitor.Demo");

    }
    
    //This gets incremented whenever a movie is successfully added
    //Tracks the total count of movies added since last start
    public Counter<long> MoviesAddedCounter { get; init; }

    public void Dispose()
    {
        MoviesMeter?.Dispose();
        MyActivitySource?.Dispose();
    }
}