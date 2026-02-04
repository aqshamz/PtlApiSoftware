using Ptl.Agent.Application;
using Ptl.Api.Infrastructure.Repositories;
using Ptl.Core.Application;
using Ptl.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 🔹 Fake DB (Phase 5)
builder.Services.AddSingleton<ITransactionSource, MySqlTransactionSource>(); //db
// 🔹 No-op infra
builder.Services.AddSingleton<IPtlActionSink, NullActionSink>();
builder.Services.AddSingleton<ICoreNotifier, NullNotifier>();

// 🔹 Hardware proxy (FIXED)
builder.Services.AddHttpClient<IPtlDisplay, HardwarePtlDisplayProxy>(c =>
{
    c.BaseAddress = new Uri("http://localhost:6001"); //connect to hardware
});

// 🔹 Core
builder.Services.AddSingleton<TransactionRunner>();//start tx
builder.Services.AddScoped<TagCommandHandler>();//receive message

builder.Services.AddSingleton<PtlHardwareRepository>();

builder.Services.AddHostedService<RecoveryService>(); //recovery tx
builder.Services.AddHostedService<TransactionLoader>(); //get next tx

builder.Services.AddSingleton<IPickEventStore, JsonPickEventStore>();
builder.Services.AddHostedService<PickEventRetryWorker>();


var app = builder.Build();

app.MapControllers();
app.Run();
