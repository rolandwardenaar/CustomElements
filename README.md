# Blazor Custom Elements — Demo Solution

A working reference implementation showing how to build a **Blazor WebAssembly component**, expose it as a native HTML **Custom Element**, and embed it inside a **Blazor Server** application — all in a single .NET 10 solution.

The result: a Windows 11–style calculator rendered by a `<calc-ulator>` tag on a Blazor Server page, with the component logic running entirely in the browser via WebAssembly.

---

## Projects

| Project | Type | Purpose |
|---------|------|---------|
| `Calculator.Wasm` | Blazor WebAssembly | The calculator component and custom element registration |
| `CalcApp.Server` | Blazor Server (ASP.NET Core) | Host application that embeds the custom element |

---

## Quick Start

```bash
git clone https://github.com/rolandwardenaar/CustomElements.git
cd CustomElements
dotnet run --project CalcApp.Server
```

Then open `http://localhost:5142`.

> **Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

---

## How It Works

### 1. Register the component as a Custom Element

In `Calculator.Wasm/Program.cs`, the component is registered under the HTML tag name `calc-ulator` instead of being mounted as a root component:

```csharp
builder.RootComponents.RegisterCustomElement<CalculatorComponent>("calc-ulator");
```

Custom element names must contain a hyphen (browser requirement).

### 2. Publish the WASM bundle into the host

An incremental MSBuild target in `CalcApp.Server.csproj` publishes `Calculator.Wasm` in Release mode and copies the output to `wwwroot/calculator/`. Key decisions:

- **Always Release** — Debug builds include a hot-reload module with a fingerprinted filename that the bootstrap cannot locate without an import map.
- **Exclude `.br`/`.gz`** — `MapStaticAssets()` auto-discovers compressed siblings; copying wrong-metadata variants causes `ERR_CONTENT_DECODING_FAILED`.
- **Clean before publish** — prevents stale fingerprinted files from a previous SDK version accumulating alongside new ones (which breaks MSBuild's 1-to-1 `Copy` task).
- **Stable-name JS copies** — `blazor.webassembly.js` imports `dotnet.js` by its stable name. The WASM project's `index.html` normally provides an `<importmap>` for this, but a host page can only have one import map. The workaround: copy `dotnet.HASH.js → dotnet.js` so the browser resolves the import directly.

### 3. Configure static file MIME types

ASP.NET Core returns **404** — not 415 — for unknown file extensions, even if the file exists. The WASM runtime needs:

```csharp
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".dat"]  = "application/octet-stream"; // ICU globalization data
provider.Mappings[".wasm"] = "application/wasm";         // .NET assemblies
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });
```

### 4. Load runtime and scoped CSS in the host

```html
<!-- Blazor scoped CSS (Calculator.razor.css → this file) -->
<link rel="stylesheet" href="calculator/Calculator.Wasm.styles.css" />

<!-- Bootstrap the WASM runtime once per page -->
<script src="calculator/_framework/blazor.webassembly.js"></script>
```

The `styles.css` link is easy to forget — without it, all component-scoped styles are silently ignored.

### 5. Use the tag anywhere

```html
<calc-ulator></calc-ulator>
```

Any page that includes the script above can use this tag. The WASM runtime starts once and hydrates every instance on the page.

### Important: custom elements are inline by default

Browsers treat unknown HTML elements as `display: inline`. Add this to your global stylesheet:

```css
calc-ulator { display: block; }
```

Without it, fixed widths and CSS grid layouts inside the component don't work correctly.

---

## Solution Structure

```
TestApp1.slnx
├── Calculator.Wasm/
│   ├── Calculator.Wasm.csproj       # Blazor WASM project
│   ├── Program.cs                   # RegisterCustomElement<Calculator>("calc-ulator")
│   └── Components/
│       ├── Calculator.razor         # Component template + logic
│       └── Calculator.razor.css     # Scoped styles (Windows 11 Fluent Design)
│
└── CalcApp.Server/
    ├── CalcApp.Server.csproj        # MSBuild target: publish WASM → wwwroot/calculator/
    ├── Program.cs                   # MIME types, static files, Blazor Server
    └── Components/
        ├── App.razor                # Loads WASM styles + script
        ├── Layout/
        │   ├── MainLayout.razor
        │   └── NavMenu.razor
        └── Pages/
            ├── Home.razor           # <calc-ulator> usage
            └── Integration.razor    # This guide, rendered as a page
```

---

## Integration Guide

A detailed in-app guide is available at `/integration` when the app is running.  
It covers all steps, pitfalls, and code examples — generated from the actual implementation in this repo.

---

## Technology

| | |
|---|---|
| **Runtime** | .NET 10 |
| **Framework** | ASP.NET Core 10 / Blazor |
| **Rendering** | Blazor Server (host) + Blazor WebAssembly (component) |
| **Custom Elements API** | `Microsoft.AspNetCore.Components.CustomElements` |
| **Styling** | Windows 11 Fluent Design (CSS, scoped to component) |
