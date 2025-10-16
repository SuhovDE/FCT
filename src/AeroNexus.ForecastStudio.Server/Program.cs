using AeroNexus.ForecastStudio.Domain.Services;
using AeroNexus.ForecastStudio.Infrastructure;
using AeroNexus.ForecastStudio.Infrastructure.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=AeroNexus;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<AeroNexusDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IImportService, ImportService>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
