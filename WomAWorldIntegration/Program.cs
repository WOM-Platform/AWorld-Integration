using Microsoft.AspNetCore.DataProtection;
using Microsoft.Net.Http.Headers;
using MongoDB.Driver;
using WomAWorldIntegration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSingleton<HttpClient>(provider =>
{
    var client = new HttpClient();
    client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "WOMConnector/1.0");
    return client;
});
builder.Services.AddDataProtection()
    .PersistKeysToGoogleCloudStorage(Environment.GetEnvironmentVariable("GCLOUD_STORAGE_KEY_BUCKET"), "AWorld-DataProtectionKeys.xml");
builder.Services.AddSingleton(provider =>
{
    return new MongoClient(Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING"));
});
builder.Services.AddTransient<MongoDatabase>();
builder.Services.AddSingleton<WomPlatform.Connector.Client>(provider =>
{
    string domain = Environment.GetEnvironmentVariable("WOM_DOMAIN") ?? "wom.domain";

    return new WomPlatform.Connector.Client(domain, provider.GetRequiredService<ILoggerFactory>());
});
builder.Services.AddScoped<WomPlatform.Connector.Instrument>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var confCredentials = configuration.GetRequiredSection("Credentials");
    string keyPath = confCredentials["KeyPath"];

    using var keyStream = new FileStream(keyPath, FileMode.Open, FileAccess.Read);

    var client = provider.GetRequiredService<WomPlatform.Connector.Client>();
    return client.CreateInstrument(confCredentials["Id"], keyStream);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
