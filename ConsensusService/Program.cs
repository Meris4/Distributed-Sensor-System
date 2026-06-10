using ConsensusService.Data;
using ConsensusService.Services;
using ConsensusService.Services.Impl;
using ConsensusService.Workers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SensorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IConsensusCalculationService, ConsensusCalculationService>();
builder.Services.AddScoped<IMaliciousSensorDetector, MaliciousSensorDetector>();

builder.Services.AddHostedService<ConsensusWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();