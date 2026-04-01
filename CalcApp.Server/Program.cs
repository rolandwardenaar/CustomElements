using CalcApp.Server.Components;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

// Serve the published Calculator.Wasm static files from wwwroot/calculator/.
// The Blazor WASM runtime downloads several file types that ASP.NET Core's static
// file middleware does not recognise by default. We extend the MIME type map so
// the browser can load them:
//   .dat  — ICU globalization data (e.g. icudt_no_CJK.dat)
//   .wasm — WebAssembly modules (.NET assemblies compiled to Webcil/WASM)
//   .blat — Blazor lazy-load asset table
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".dat"]  = "application/octet-stream";
contentTypeProvider.Mappings[".wasm"] = "application/wasm";
contentTypeProvider.Mappings[".blat"] = "application/octet-stream";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
