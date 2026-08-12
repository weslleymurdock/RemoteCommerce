using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using RemoteCommerce.Components;
using RemoteCommerce.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddMudServices();

builder.Services.AddDbContextFactory<CommerceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Commerce")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
