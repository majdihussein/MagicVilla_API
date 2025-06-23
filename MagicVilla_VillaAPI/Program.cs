
using MagicVilla_VillaAPI.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// (majdi)1.1 - For Serilog - then install Serilog.AspNetCore&Serilog.Sinks.File NuGet package
//Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
//    .WriteTo.File("log/villalogs.txt", rollingInterval: RollingInterval.Day).CreateLogger();

//builder.Host.UseSerilog();
// (majdi)1.1 - For SeriLog -

builder.Services.AddControllers(option =>
{
    //option.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();

// add for swagger
//builder.Services.AddEndpointsApiExplorer();

// add for logging
builder.Services.AddSingleton<ILogging, LoggingV2>();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // for swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
