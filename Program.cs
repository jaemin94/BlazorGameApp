using BlazorApp.Components;
using MyBlazorApp.Hubs;
using MyBlazorApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Blazor Server 렌더링
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// SignalR
builder.Services.AddSignalR();

// 게임 서비스
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

// 게임 허브 주소
app.MapHub<GameHub>("/gamehub");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
