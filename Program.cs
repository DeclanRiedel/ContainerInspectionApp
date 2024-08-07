using MudBlazor.Services;
using ContainerInspectionApp.Components;
using ContainerInspectionApp.Services;
using FluentValidation;
using ContainerInspectionApp.Validators;
using ContainerInspectionApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add ContainerTableOperations as a scoped service
builder.Services.AddScoped<ContainerTableOperations>();

// Add Fluent Validation
builder.Services.AddScoped<IValidator<string>, ConnectionStringValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();