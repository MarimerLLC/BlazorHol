using AppServer;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IAuthorizationHandler, BearerAuthnHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BearerAuthn", policy =>
    {
        policy.Requirements.Add(new BearerAuthnRequirement());
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("AllowAllOrigins");

app.MapControllers().RequireAuthorization("BearerAuthn");

app.Run();
