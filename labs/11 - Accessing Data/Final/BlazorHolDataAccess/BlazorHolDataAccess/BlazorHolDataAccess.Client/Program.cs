using BlazorHolDataAccess.Data;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IPersonDal, BlazorHolDataAccess.Client.Data.PersonDal>();

await builder.Build().RunAsync();
