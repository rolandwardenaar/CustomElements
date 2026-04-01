using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

// Alias to avoid ambiguity: the class 'Calculator' lives in namespace
// 'Calculator.Wasm.Components', but C# also sees 'Calculator' as a
// namespace prefix (from 'Calculator.Wasm'). Using an alias is the
// cleanest way to resolve this without renaming either the class or the project.
using CalculatorComponent = Calculator.Wasm.Components.Calculator;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register the Calculator Razor component as a Web Custom Element.
// This makes <calc-ulator> available as a native HTML custom element
// that can be used in any web application—including Blazor Server.
builder.RootComponents.RegisterCustomElement<CalculatorComponent>("calc-ulator");

await builder.Build().RunAsync();
