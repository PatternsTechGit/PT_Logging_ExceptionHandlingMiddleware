using BBBankAPI.Middlewares;
using Infrastructure;
using Services;
using Services.Contracts;

var builder = WebApplication.CreateBuilder(args);

//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Allow CORS request

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder.WithOrigins("http://localhost:4200" , "https://bbanktest.z13.web.core.windows.net")
                          .AllowAnyHeader()
                                                  .AllowAnyMethod();
                      });
});

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddSingleton<BBBankContext>();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

// Registering cutom middleware in pipeline
app.UseCustomLogginMiddleware();
//app.UseCustomExceptionHandlingMiddleware();
app.MapControllers();

app.Run();




