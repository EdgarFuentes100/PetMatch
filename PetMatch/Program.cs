using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PetMatch.Context;
using PetMatch.Models;
using PetMatch.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
///CONFIGURACION NECESARIA PARA LLAMAR LAS CLAVES DE appsettingsLocal, si se llama solo appsettings no es necesario
builder.Configuration
    .AddJsonFile("appsettingsLocal.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// CLAVES DE GOOGLE CONSOLE
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

// AUTENTICACIÓN DE GOOGLE 
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(opts =>
{
    opts.LoginPath = "/login"; // ← Muestra vista personalizada
    opts.AccessDeniedPath = "/acceso-denegado";

    //DEFINE EL TIEMPO PARA CERRAR LA SESION DE LA APP
    opts.ExpireTimeSpan = TimeSpan.FromHours(1);
    opts.SlidingExpiration = true;
    opts.Cookie.HttpOnly = true;
    opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opts.Cookie.SameSite = SameSiteMode.Lax;

    opts.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = ctx =>
        {
            var returnUrl = ctx.Request.Path + ctx.Request.QueryString;
            var loginUri = $"{opts.LoginPath}?expired=true&returnUrl={Uri.EscapeDataString(returnUrl)}";
            ctx.Response.Redirect(loginUri);
            return Task.CompletedTask;
        }
    };
})
.AddGoogle("Google", options =>
{
    //TREA LOS DATOS DE GOOGLE PARA INYECTAR UN SERVICIO
    options.ClientId = googleClientId;
    options.ClientSecret = googleClientSecret;
    options.CallbackPath = "/signin-google";
    options.Events = new OAuthEvents
    {
        OnRedirectToAuthorizationEndpoint = ctx =>
        {
            ctx.Response.Redirect(ctx.RedirectUri + "&prompt=select_account");
            return Task.CompletedTask;
        },
        OnRemoteFailure = ctx =>
        {
            ctx.Response.Redirect("/login?error=external");
            ctx.HandleResponse();
            return Task.CompletedTask;
        },
        OnCreatingTicket = ctx =>
        {
            if (ctx.TokenResponse.Response.RootElement.TryGetProperty("id_token", out var idProp))
            {
                var idToken = idProp.GetString();
                // También puedes guardarlo en las propiedades de autenticación
                ctx.Properties.StoreTokens(new[]
                {
                    new AuthenticationToken { Name = "id_token", Value = idToken }
                });
            }
            return Task.CompletedTask;
        }
    };
});

// AUTORIZACIÓN GLOBAL
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

//SE CREAN POLITICAS PARA LOS ROLES
builder.Services.AddAuthorization(options =>
{
    // Solo ADMIN
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // Solo FUNDACION (otro rol de ejemplo)
    options.AddPolicy("UserOnly", policy =>
        policy.RequireRole("Usuario"));

    options.AddPolicy("All", policy =>
    policy.RequireRole("Usuario", "Admin"));

    // Rol más un claim adicional (ejemplo)
    /*options.AddPolicy("AdminConEmailCorporativo", policy =>
        policy.RequireRole("Admin")
              .RequireClaim(ClaimTypes.Email, email => email.EndsWith("@miempresa.com")));*/
});

// Obtener el puerto que Render asigna, si no existe usa 5000 por defecto para desarrollo local
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";

// Configurar Kestrel para que escuche en el puerto asignado
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

// SERVICIOS PARA CONSUMO DE API-OPENAI-AZURE
builder.Services.Configure<AzureOpenAIConfig>(builder.Configuration.GetSection("AzureOpenAI"));
builder.Services.AddSingleton<AzureOpenAIClient>(sp =>
{
    var config = sp.GetRequiredService<IOptions<AzureOpenAIConfig>>().Value;
    return new AzureOpenAIClient(new Uri(config.Endpoint), new AzureKeyCredential(config.ApiKey));
});

//SERVICOS GLOBALES CREADOS EN CLASES
builder.Services.AddScoped<ServiceTransactionHelper>();
builder.Services.AddScoped<ServiceIA>();
builder.Services.AddScoped<ServiceGoogleValidator>();

// CONEXION A BASE DE DATOS SQL SERVER 
var connection = builder.Configuration.GetConnectionString("Connection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));

// Middleware
var app = builder.Build();
app.UseDeveloperExceptionPage();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
