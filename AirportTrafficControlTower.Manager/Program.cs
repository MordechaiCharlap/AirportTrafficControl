using AirportTrafficControlTower.Data.Contexts;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using AirportTrafficControlTower.Service;
using AirportTrafficControlTower.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AirPortTrafficControlContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AirPortDataConnectionString"));
}, ServiceLifetime.Transient);

builder.Services.AddControllers();
//.AddNewtonsoftJson(jsonOptions =>
// {
//     jsonOptions.SerializerSettings.Converters.Add(new StringEnumConverter());
//     jsonOptions.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
// });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<ILiveUpdateService, LiveUpdateService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<IRepository<LiveUpdate>, LiveUpdateRepositry>();
builder.Services.AddScoped<IRepository<Station>, StationRepository>();
builder.Services.AddScoped<IRepository<Flight>, FlightRepository>();
builder.Services.AddScoped<IRepository<AirportTrafficControlTower.Data.Model.Route>, RouteRepository>();
builder.Services.AddRouting();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
var connectionString = builder.Configuration.GetConnectionString("AirPortDataConnectionString");
builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));
builder.Services.AddHangfireServer();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHangfireDashboard();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
