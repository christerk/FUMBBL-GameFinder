using Fumbbl.Api;
using Fumbbl.Gamefinder.Model;
using Fumbbl.Gamefinder.Model.Cache;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFumbbl();
builder.Services.AddSingleton<EventQueue>();
builder.Services.AddSingleton<MatchGraph>();
builder.Services.AddSingleton<GamefinderModel>();
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
