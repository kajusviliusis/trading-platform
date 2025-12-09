using Microsoft.EntityFrameworkCore;
using trading_platform.Data;
using trading_platform.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<OrderService>();
builder.Services.AddHttpClient<FinnhubService>();
if(builder.Configuration.GetValue<bool>("Finnhub:EnablePriceUpdater"))
{
    builder.Services.AddHostedService<StockPriceUpdater>();
}
builder.Services.AddDbContext<TradingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
