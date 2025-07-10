using MagicVilla_Web;
using MagicVilla_Web.Services;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// majdi add automapper
builder.Services.AddAutoMapper(typeof(MappingConfig));

// majdi add httpclient factory
builder.Services.AddHttpClient<IVillaService, VillaService>();
builder.Services.AddHttpClient<IVillaNumberService, VillaNumberService>();
builder.Services.AddHttpClient<IAuthService, AuthService>();


// majdi add service
builder.Services.AddScoped<IVillaService, VillaService>();
builder.Services.AddScoped<IVillaNumberService, VillaNumberService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// majdi add ContextAccessor
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// majdi add session 
builder.Services.AddDistributedMemoryCache();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Set cookie expiration time
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.SlidingExpiration = true; // Enable sliding expiration... 
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100); // Set session timeout
    options.Cookie.HttpOnly = true; // Make the session cookie HTTP only
    options.Cookie.IsEssential = true; // Make the session cookie essential
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
