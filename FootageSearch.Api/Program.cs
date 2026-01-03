using FootageSearch.Core.Interfaces;
using FootageSearch.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IJobStatusService, FileJobStatusService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/health", () => "OK");

app.MapGet("/status", async (IJobStatusService statusService) => 
{
    return await statusService.GetStatusAsync();
});

app.Run();