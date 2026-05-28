using BlazorApp.Components;
using MyBlazorApp.Hubs;
using MyBlazorApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();

builder.Services.AddScoped<GameInputService>();
builder.Services.AddScoped<GameSkillService>();
builder.Services.AddScoped<GameAiService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapHub<GameHub>("/gamehub");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();