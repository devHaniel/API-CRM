
using Infrastructure;
using Application;
using Microsoft.AspNetCore.Authentication.Cookies;
using Hangfire;
using Application.Interfaces;
using Hangfire.PostgreSql;
using Application.Configuration;


var builder = WebApplication.CreateBuilder(args);

// ============================================
// Validaciones de configuración al iniciar
// ============================================
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<TwilioSettings>()
    .Bind(builder.Configuration.GetSection("Twilio"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<StripeSettings>()
    .Bind(builder.Configuration.GetSection("Stripe"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ============================================
// Servicios de capas internas
// ============================================
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// ============================================
// Hangfire — AHORA vive aquí, porque API no se despliega
// ============================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta configurar ConnectionStrings:DefaultConnection");

builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(
        connectionString,
        new PostgreSqlStorageOptions
        {
            PrepareSchemaIfNecessary = true  
        }));

builder.Services.AddHangfireServer();

// ============================================
// Razor Pages y Controllers
// ============================================
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// ============================================
// Autenticación por Cookies
// ============================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ============================================
// Pipeline HTTP
// ============================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// Dashboard de Hangfire — protégelo, ver punto 3
// app.UseHangfireDashboard("/hangfire", new DashboardOptions
// {
//     Authorization = new[] { new HangfireAuthorizationFilter() }
// });

app.MapControllers();
app.MapRazorPages();

// ============================================
// Configurar trabajos recurrentes
// ============================================
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<IRecordatorioService>(
        "procesar-recordatorios-pendientes",
        service => service.ProcesarPendientesAsync(CancellationToken.None),
        "*/1 * * * *");
}

app.Run();