using BlazorHolDataAccess.Client.Pages;
using BlazorHolDataAccess.Components;
using BlazorHolDataAccess.Data;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddTransient<SqliteConnection>(sp =>
{
    var connection = new SqliteConnection("Data Source=BlazorHolDataAccess.db");
    connection.Open();
    return connection;
});
builder.Services.AddScoped<Database>();
builder.Services.AddScoped<IPersonDal, PersonDal>();

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
    .AddAdditionalAssemblies(typeof(BlazorHolDataAccess.Client._Imports).Assembly);

app.Run();
