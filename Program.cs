using SupportWebApp.Components;
using SupportWebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Tilføjer services til container.
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

// Konfigurer HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();