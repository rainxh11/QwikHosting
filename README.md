# Qwik + Deno + ASP\.NET CORE

run Deno hosted Qwik app alongside ASP\.NET application through [YARP](https://github.com/microsoft/reverse-proxy) reverse proxy.

| Version                                                                                                                    | Downloads                                                          |
| -------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| [![Latest version](https://img.shields.io/nuget/v/QwikHosting.Deno.svg)](https://www.nuget.org/packages/QwikHosting.Deno/) | ![Downloads](https://img.shields.io/nuget/dt/QwikHosting.Deno.svg) |

## Example Usage:

```csharp
using QwikHosting.Deno;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddReverseProxy(); // Register YARP
builder.Services.AddQwikHosting(config =>
                                {
                                    // qwik application path
                                    config.BaseDirectory = AppContext.BaseDirectory + "qwik-app";

                                    // port to run deno at, passed as 'PORT' environment variable
                                    config.Port = 9800;

                                    // if no 'deno' binary was found in path, will auto-download the latest version from github
                                    config.BinaryPick = DenoBinaryTypePriority.TryPathThenDownloaded;
                                });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/weather", () => "weather")
   .WithName("GetWeatherForecast")
   .WithOpenApi();

app.UseQwikDenoReverseProxy(); // map qwik deno endpoints to yarp

app.Run();
```
