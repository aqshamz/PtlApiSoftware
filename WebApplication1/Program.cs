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

builder.Services.AddScoped<ITransactionCommandRepository, TransactionCommandRepository>();

// 🔹 No-op infra
builder.Services.AddSingleton<IPtlActionSink, NullActionSink>();
builder.Services.AddSingleton<ICoreNotifier, NullNotifier>();

// 🔹 Hardware proxy (FIXED)
builder.Services.AddHttpClient<IPtlDisplay, HardwarePtlDisplayProxy>(c =>
{
    c.BaseAddress = new Uri("http://localhost:6001"); //connect to hardware
});

// 🔹 Core
builder.Services.AddSingleton<TransactionRunner>();//process tx
builder.Services.AddScoped<TagCommandHandler>();//receive message

builder.Services.AddSingleton<PtlHardwareRepository>(); //get gateaway

builder.Services.AddHostedService<RecoveryService>(); //recovery tx
builder.Services.AddHostedService<TransactionLoader>(); //get next tx

builder.Services.AddSingleton<IPickEventStore, JsonPickEventStore>(); //store tx
builder.Services.AddHostedService<PickEventRetryWorker>(); //retry event


var app = builder.Build();

app.UseCors("AllowVueApp");
app.MapControllers();
app.Run();
