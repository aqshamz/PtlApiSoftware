using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Ptl.Contracts.Dtos.Hardware;

namespace Ptl.Hardware;

public static class HardwareApiHost
{
    public static void Start(HardwarePtlDisplay display)
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseUrls("http://localhost:6001");

        var app = builder.Build();

        app.MapPost("/ptl/execute", (PtlTxCommandDto dto) =>
        {
            display.Execute(dto);
            return Results.Ok();
        });

        app.Start();
    }
}
