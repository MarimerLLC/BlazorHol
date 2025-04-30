using BlazorHolState;
using BlazorHolState.Client.Pages;
using BlazorHolState.Components;
using BlazorHolState.Server;
using Marimer.Blazor.RenderMode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddRenderModeDetection();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton(typeof(ISessionManager), typeof(SessionManager));
builder.Services.AddTransient(typeof(SessionIdManager), typeof(SessionIdManager));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorHolState.Client._Imports).Assembly);

app.Run();
