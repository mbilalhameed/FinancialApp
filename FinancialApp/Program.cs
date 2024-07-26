using FinancialApp.Factories;
using FinancialApp.Hubs;
using FinancialApp.Observers;
using FinancialApp.Services;
using FinancialApp.Strategy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Register services
builder.Services.AddSingleton<FinancialDataObserver>();
builder.Services.AddSingleton<TiingoService>();
builder.Services.AddSingleton<BinanceService>();

builder.Services.AddSingleton<IDataProviderFactory, DataProviderFactory>(provider =>
{
    var tiingoService = provider.GetRequiredService<TiingoService>();
    var binanceService = provider.GetRequiredService<BinanceService>();
    var dataProviders = new Dictionary<string, IDataProvider>
    {
        { "forex", tiingoService },
        { "crypto", binanceService }
    };
    var logger = provider.GetRequiredService<ILogger<DataProviderFactory>>();
    return new DataProviderFactory(dataProviders, logger);
});

builder.Services.AddHttpClient<TiingoService>();
builder.Services.AddHttpClient<BinanceService>();
builder.Services.AddSingleton<IDataProviderStrategy, DataProviderStrategy>();

builder.Logging.ClearProviders();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<FinancialDataHub>("/financialDataHub");
});

app.Run();
