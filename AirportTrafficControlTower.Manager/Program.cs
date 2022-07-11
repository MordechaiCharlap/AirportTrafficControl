using AirportTrafficControlTower.Data.Contexts;
using AirportTrafficControlTower.Data.Model;
using AirportTrafficControlTower.Data.Repositories;
using AirportTrafficControlTower.Data.Repositories.Interfaces;
using AirportTrafficControlTower.Service;
using AirportTrafficControlTower.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AirPortTrafficControlContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AirPortDataConnectionString"));
}
    , ServiceLifetime.Transient);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<ILiveUpdateService, LiveUpdateService>();
builder.Services.AddScoped<IRouteService, RouteService>();
IServiceCollection serviceCollection = builder.Services.AddScoped<IRepository<Flight>, FlightRepository>();
builder.Services.AddScoped<IRepository<LiveUpdate>, LiveUpdateRepositry>();
builder.Services.AddScoped<IRepository<Station>, StationRepository>();
builder.Services.AddRouting();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
