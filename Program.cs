using SupportWebApp.Components;
using SupportWebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Cosmos DB-service (læser fra user secrets)
builder.Services.AddSingleton<CosmosSupportService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new CosmosSupportService(
        config["CosmosDb:ConnectionString"]!,
        config["CosmosDb:DatabaseName"]!,
        config["CosmosDb:ContainerName"]!);
});

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