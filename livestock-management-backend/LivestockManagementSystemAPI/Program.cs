using DataAccess.AutoMapperConfig;
using DataAccess.Repository.Interfaces;
using DataAccess.Repository.Services;
using LivestockManagementSystemAPI.Extensions;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDistributedMemoryCache();


builder.Services.AddHttpClient();
builder.Services.AddAutoMapper(typeof(ConfigMapper));
builder.Services.AddSwaggerExplorer()
    .AddDBContextService(builder.Configuration)
    .AddIdentityHandlerAndStores()
    .ConfigureIdentityOptions()
    .AddAppConfig(builder.Configuration)
    .AddIdentityAuth(builder.Configuration);

// Add logging services
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    // Optionally add file logging with Serilog or other providers
});

var app = builder.Build();

// Thêm hỗ trợ file tĩnh
app.UseStaticFiles();

app.SeedRoles();
app.SeedPermissionClaims();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.ConfigureCORSMiddlewares()
    .AddSwaggerExplorerMiddleware()
    .AddIdentityAuthMiddlewares();

app.MapControllers();

app.Run();
