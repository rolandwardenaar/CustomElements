using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DrawingCanvasComponent = DrawingClient.Wasm.Components.DrawingCanvas;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register the DrawingCanvas Razor component as a Web Custom Element.
// This makes <dotnet-grpc-client> available as a native HTML custom element
// that can be used in any web application.
builder.RootComponents.RegisterCustomElement<DrawingCanvasComponent>("dotnet-grpc-client");

await builder.Build().RunAsync();
