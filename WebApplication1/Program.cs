using Ptl.Agent.Application;
using Ptl.Api.Infrastructure.Repositories;
using Ptl.Core.Application;
using Ptl.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // URL Vite kamu
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddControllers();

// 🔹 Fake DB (Phase 5)
builder.Services.AddSingleton<ITransactionSource, MySqlTransactionSource>(); //db
builder.Services.AddSingleton<PostgresConnectionFactory>(); //postgredb

builder.Services.AddScoped<IBatchRepository, PostgresBatchRepository>();//pgrepository get data tx
builder.Services.AddScoped<ITransactionCommandRepository, TransactionCommandRepository>(); //command tx lama

// 🔹 No-op infra
builder.Services.AddSingleton<IPtlActionSink, NullActionSink>(); //skip
builder.Services.AddSingleton<ICoreNotifier, NullNotifier>(); //skip

// 🔹 Hardware proxy (FIXED)
builder.Services.AddHttpClient<IPtlDisplay, HardwarePtlDisplayProxy>(c =>
{
    c.BaseAddress = new Uri("http://localhost:6001"); //connect to hardware
});

builder.Services.AddHttpClient("hardware", c =>
{
    c.BaseAddress = new Uri("http://localhost:6001");
});

builder.Services.AddSingleton<Phase2TagRegistry>(); //command respond

builder.Services.AddScoped<BatchRxService>(); //pg service

builder.Services.AddScoped<BatchEngineService>(); //pg service
builder.Services.AddHostedService<BatchEngineBackgroundService>(); //pg service loop


builder.Services.AddSingleton<PtlHardwareRepository>(); //get gateaway
builder.Services.AddScoped<PtlPostgresHardwareRepository>(); //get gateaway pg

builder.Services.AddSingleton<ConnectedGatewayRegistry>();//store available ip pg


builder.Services.AddSingleton<RecoveryService>(); //recovery
builder.Services.AddSingleton<IRecoveryState>(sp => sp.GetRequiredService<RecoveryService>()); //recovery
builder.Services.AddHostedService(sp => sp.GetRequiredService<RecoveryService>()); //recovery

builder.Services.AddSingleton<IPickEventStore, JsonPickEventStore>(); //store tx

//builder.Services.AddScoped<BatchLoaderService>();
// 🔹 Core
//builder.Services.AddSingleton<TransactionRunner>();//process tx
//builder.Services.AddScoped<TagCommandHandler>();//receive message

//builder.Services.AddHostedService<TransactionLoader>(); //get next tx
//builder.Services.AddHostedService<PickEventRetryWorker>(); //retry event


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<PostgresConnectionFactory>();
    await factory.TestConnectionAsync();
}

app.UseCors("AllowVueApp");
app.MapControllers();
app.Run();
