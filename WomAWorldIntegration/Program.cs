using Microsoft.AspNetCore.DataProtection;
using Microsoft.Net.Http.Headers;

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
