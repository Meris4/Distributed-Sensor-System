using IngestionService.Services;
using IngestionService.Services.Impl;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ISensorMessageValidator, SensorMessageValidator>();
builder.Services.AddSingleton<SensorActivityTracker>();
builder.Services.AddScoped<ISensorReadingWriter, SensorReadingWriter>();
builder.Services.AddScoped<IAlarmNotifier, AlarmNotifier>();


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
