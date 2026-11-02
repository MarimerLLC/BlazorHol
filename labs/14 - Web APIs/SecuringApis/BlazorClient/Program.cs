using BlazorClient.Components;
using BlazorClient.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Typed HttpClients for calling the ApiServer
var apiServer = new Uri(builder.Configuration["ApiServer"]!);
builder.Services.AddHttpClient<JwtApiClient>(client => client.BaseAddress = apiServer);
builder.Services.AddHttpClient<OAuthApiClient>(client => client.BaseAddress = apiServer);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
