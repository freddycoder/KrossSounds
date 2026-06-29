using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KrossSounds.Components;
using KrossSounds.Components.Account;
using KrossSounds.Data;
using KrossSounds.Services;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using KrossSounds.Middleware;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Environment: " + builder.Configuration["ASPNETCORE_ENVIRONMENT"]);

// ── 1. HTTPS & HSTS ────────────────────────────────────────────────────────── 
// Force le navigateur à utiliser HTTPS pendant 1 an, y compris les sous-domaines. 
if (builder.Configuration.UseHsts())
{
    builder.Services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(365); // 31 536 000 secondes 
    });
}

// ── 2. POLITIQUE CORS ──────────────────────────────────────────────────────── 
// Politique restrictive : autorise uniquement le domaine KrossSounds. 
var corsOrigins = builder.Configuration["CORS_ORIGIN"];
if (!string.IsNullOrWhiteSpace(corsOrigins))
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("KrossSoundsPolicy", policy =>
        {
            policy.WithOrigins(corsOrigins.Split(','))
                .AllowAnyMethod()
                .AllowCredentials()
                .AllowAnyHeader();
        });
    });
}

// ── 3. COOKIE POLICY ───────────────────────────────────────────────────────── 
// Voir section 2.2 pour la configuration détaillée. 
if (builder.Configuration.UseCookiePolicy())
{
    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        options.Secure = CookieSecurePolicy.Always; // ← Corrige le flag Secure manquant 
        options.HttpOnly = HttpOnlyPolicy.Always;
        options.MinimumSameSitePolicy = SameSiteMode.Strict;
    });
}

// ── 4. ANTIFORGERY ─────────────────────────────────────────────────────────── 
if (builder.Configuration.UseAntiforgery())
{
    builder.Services.AddAntiforgery(options =>
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.SuppressXFrameOptionsHeader = true; // Géré par SecurityHeadersMiddleware 
    });
}

// ── 5. SESSION ─────────────────────────────────────────────────────────────── 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// ── 6. RATE LIMITING (.NET 7+) ─────────────────────────────────────────────── 
// Voir section 2.5 pour la configuration détaillée. 
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // ── Politique pour Login : 5 tentatives par minute par IP ──────────────── 
    options.AddFixedWindowLimiter("LoginPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    // ── Politique pour Register : 3 créations de compte par heure par IP ───── 
    options.AddFixedWindowLimiter("RegisterPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 3;
        limiterOptions.Window = TimeSpan.FromHours(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    // ── Politique globale : 100 requêtes par minute (protection DDoS légère) ── 
    options.AddFixedWindowLimiter("GlobalPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddScoped<ISoundStorageService, LocalSoundStorageService>();
builder.Services.AddSingleton<SecurityHeadersMiddleware>();

var app = builder.Build();

// Migrer la base de données
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var log = app.Services.GetRequiredService<ILogger<Program>>();
    log.LogInformation("Database connection string: {ConnectionString}", app.Configuration["ConnectionStrings:DefaultConnection"]);
    await context.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

if (builder.Configuration.UseHsts())
{
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Headers de ③sécurité personnalisés 
app.UseMiddleware<SecurityHeadersMiddleware>();

// Cookie Policy 
if (builder.Configuration.UseCookiePolicy())
{
    app.UseCookiePolicy();
}

if (builder.Configuration.UseAntiforgery())
{
    app.UseAntiforgery();
}

app.UseCors("KrossSoundsPolicy");

app.UseSession();

app.UseRateLimiter();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireRateLimiting("GlobalPolicy");

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapControllers();

app.Run();
