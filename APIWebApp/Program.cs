using APIWebApp.Hubs;
using APIWebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// user service (JSON file storage)
builder.Services.AddSingleton<IUserService, FileUserService>();
// inventory service
builder.Services.AddSingleton<IInventoryService, FileInventoryService>();

// Authentication: cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
    });

// Authorization policies for minimum permission levels
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MinPermission1", policy => policy.RequireAssertion(ctx =>
        int.TryParse(ctx.User.FindFirst("PermissionLevel")?.Value, out var lvl) && lvl >= 1));
    options.AddPolicy("MinPermission2", policy => policy.RequireAssertion(ctx =>
        int.TryParse(ctx.User.FindFirst("PermissionLevel")?.Value, out var lvl) && lvl >= 2));
    options.AddPolicy("MinPermission3", policy => policy.RequireAssertion(ctx =>
        int.TryParse(ctx.User.FindFirst("PermissionLevel")?.Value, out var lvl) && lvl >= 3));
});

var app = builder.Build();

// Ensure users file initialized
using (var scope = app.Services.CreateScope())
{
    var userSvc = scope.ServiceProvider.GetRequiredService<IUserService>();
    await userSvc.EnsureInitializedAsync();
    var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
    await inv.EnsureInitializedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map the SignalR hub endpoint
app.MapHub<LineMonitorHub>("/lineMonitorHub");

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
