using Bomberos.Core.Application;
using Bomberos.WebApp.Infrastructure;
using Bomberos.WebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

// Nombre de la ventana de consola, para distinguirla al ejecutar los cuatro proyectos a la vez.
try { Console.Title = "Bomberos - WebApp"; } catch (IOException) { /* sin consola */ }

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add<ApiExceptionFilter>();
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// AutoMapper con los perfiles de Application (ViewModels ⇄ DTOs).
builder.Services.AddApplicationMappings(builder.Configuration["AutoMapper:LicenseKey"]);

// La WebApp consume únicamente Integration.Api, enviando el JWT del usuario en cada llamada.
var integracionUrl = builder.Configuration["Integracion:BaseUrl"]
    ?? throw new InvalidOperationException("Falta Integracion:BaseUrl en la configuración.");
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BearerTokenHandler>();
builder.Services.AddHttpClient(IntegracionApi.Cliente, c =>
{
    c.BaseAddress = new Uri(integracionUrl);
    c.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddScoped<IntegracionApi>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/AccesoDenegado";
        o.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        o.Cookie.Name = "Bomberos.Sesion";
        o.Cookie.HttpOnly = true;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
