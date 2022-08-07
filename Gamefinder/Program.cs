using Fumbbl.Api;
using Fumbbl.Gamefinder.Model;
using Fumbbl.Gamefinder.Model.Cache;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting Gamefinder");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/gamefinder-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 5
        )
        .ReadFrom.Configuration(context.Configuration)
    ;
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFumbbl();
builder.Services.AddSingleton<EventQueue>();
builder.Services.AddSingleton<GamefinderModel>();
builder.Services.AddSingleton<BlackboxModel>();
builder.Services.AddSingleton<CoachCache>();
builder.Services.AddSingleton<TeamCache>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();
app.Run();
