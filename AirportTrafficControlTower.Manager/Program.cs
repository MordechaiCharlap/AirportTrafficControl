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
    options.UseSqlServer(builder.Configuration.GetConnectionString("AirportDataConnectionString"));
}, ServiceLifetime.Transient);

builder.Services.AddControllers();
builder.Services.AddCors(options => {
    options.AddPolicy("myPolicy",
                      policy => {
                          policy
                            .WithOrigins("https://localhost:7237")
                            //.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                      });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<ILiveUpdateService, LiveUpdateService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddTransient<IRepository<LiveUpdate>, LiveUpdateRepositry>();
builder.Services.AddTransient<IRepository<Station>, StationRepository>();
builder.Services.AddTransient<IRepository<Flight>, FlightRepository>();
builder.Services.AddTransient<IRepository<AirportTrafficControlTower.Data.Model.Route>, RouteRepository>();
builder.Services.AddRouting();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseCors("myPolicy");
app.UseAuthentication();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.MapControllers();

app.Run();
