# Blazor Custom Elements — Demo Solution

A working reference implementation showing how to build **Web Custom Elements** using two different approaches — **Blazor WebAssembly** and **Vue.js** — and embed them inside **Blazor Server** applications. All in a single .NET 10 solution orchestrated by **.NET Aspire**.

The solution contains two demos:

1. **Calculator** — A Windows 11–style calculator rendered by a `<calc-ulator>` tag, with the component logic running entirely in the browser via Blazor WebAssembly.
2. **gRPC Drawing Canvas** — A side-by-side comparison of a **Blazor WASM** (`<dotnet-grpc-client>`) and a **Vue.js** (`<vuejs-grpc-client>`) custom element, both streaming drawing commands from a shared **gRPC server** and rendering them on an HTML5 Canvas in real time.

---

## Projects

| Project | Type | Purpose |
|---------|------|---------|
| `Calculator.Wasm` | Blazor WebAssembly | The calculator component and custom element registration |
| `CalcApp.Server` | Blazor Server (ASP.NET Core) | Host application that embeds the calculator custom element |
| `DrawingClient.Wasm` | Blazor WebAssembly | Drawing canvas component exposed as `<dotnet-grpc-client>` custom element |
| `drawing-client-vue` | Vue.js 3 + TypeScript | Drawing canvas component exposed as `<vuejs-grpc-client>` custom element |
| `DrawingService.Grpc` | ASP.NET Core gRPC | gRPC server that streams Canvas 2D draw commands for named scenes |
| `DemoApp.Server` | Blazor Server (ASP.NET Core) | Host application that embeds both drawing custom elements side-by-side |
| `CustomElements.AppHost` | .NET Aspire AppHost | Orchestrates all services for local development |
| `CustomElements.ServiceDefaults` | Shared library | Common Aspire service defaults (health checks, telemetry) |

---

## Quick Start

### Calculator demo (standalone)

```bash
git clone https://github.com/rolandwardenaar/CustomElements.git
cd CustomElements
dotnet run --project CalcApp.Server
```

Then open `http://localhost:5142`.

### Drawing demo (via Aspire)

```bash
dotnet run --project CustomElements.AppHost
```

The Aspire dashboard opens automatically and shows all services. The demo app is available at the `demo-app` endpoint. The gRPC drawing service and the demo host start together — no manual setup needed.

> **Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download), [Node.js](https://nodejs.org/) (for the Vue.js client build)

---

## Building Custom Elements (PowerShell)

For scenarios where you want to build the WebAssembly custom elements **without triggering a full MSBuild**, use the `Build-CustomElements.ps1` script. This is useful for:

- CI/CD pipelines that need explicit build steps
- Development workflows where you want to rebuild only the WASM components
- Debugging build issues in isolation

### Usage

```powershell
# Build all custom elements (Calculator + DrawingClient)
.\Build-CustomElements.ps1

# Build only the Calculator custom element
.\Build-CustomElements.ps1 -Project Calculator

# Build only the DrawingClient custom element
.\Build-CustomElements.ps1 -Project DrawingClient

# Build with verbose output to see all file operations
.\Build-CustomElements.ps1 -Verbose

# Build in Debug configuration (default is Release)
.\Build-CustomElements.ps1 -Configuration Debug
```

### What the script does

The script performs the same steps as the MSBuild targets in the `.csproj` files:

1. **Publishes** the WASM project (`dotnet publish -c Release`)
2. **Cleans** stale framework files from previous builds
3. **Copies** static files to the server's `wwwroot/` directory (excluding `.br`/`.gz` pre-compressed files)
4. **Creates stable-name copies** of fingerprinted JS files:
   - `blazor.webassembly.HASH.js` → `blazor.webassembly.js`
   - `dotnet.HASH.js` → `dotnet.js`
   - `dotnet.native.HASH.js` → `dotnet.native.js`
   - `dotnet.runtime.HASH.js` → `dotnet.runtime.js`
5. **Copies** `_content/` files to the server's wwwroot root for WASM runtime resolution

### Output locations

| Project | Published to |
|---------|--------------|
| `Calculator.Wasm` | `CalcApp.Server/wwwroot/calculator/` |
| `DrawingClient.Wasm` | `DemoApp.Server/wwwroot/drawing-client/` |

---

## How It Works — Calculator (`<calc-ulator>`)

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

## How It Works — Drawing Canvas (`<dotnet-grpc-client>` & `<vuejs-grpc-client>`)

The drawing demo shows how the same **gRPC server** can feed two completely different front-end technologies — **Blazor WebAssembly** and **Vue.js** — via gRPC-Web server streaming. Both clients receive identical draw commands and render them on an HTML5 Canvas in real time.

### Architecture

```
┌──────────────────────┐
│  DrawingService.Grpc │   ASP.NET Core gRPC server
│  (gRPC-Web enabled)  │   Streams Canvas 2D draw commands
└──────────┬───────────┘
           │  gRPC-Web (HTTP/1.1)
     ┌─────┴─────┐
     ▼           ▼
┌──────────┐ ┌──────────┐
│  Blazor  │ │  Vue.js  │   Both run in the browser
│   WASM   │ │  (IIFE)  │   as Web Custom Elements
└──────────┘ └──────────┘
```

### gRPC service & proto contract

A shared `protos/drawing.proto` defines the service contract:

```protobuf
service DrawingCanvas {
  rpc StreamScene (SceneRequest) returns (stream DrawCommand);
  rpc GetAvailableScenes (Empty) returns (SceneList);
}
```

Each `DrawCommand` is a `oneof` containing a single Canvas 2D operation (`setColor`, `fillRect`, `arc`, `lineTo`, etc.). The server has pluggable **scene renderers** (tree, house, cloud, sun, car, landscape) that each produce a sequence of draw commands.

### Blazor WASM client (`DrawingClient.Wasm`)

Registered as `<dotnet-grpc-client>` via:

```csharp
builder.RootComponents.RegisterCustomElement<DrawingCanvasComponent>("dotnet-grpc-client");
```

Uses `Grpc.Net.Client.Web` (gRPC-Web handler) for browser-compatible gRPC calls. Draw commands are streamed with `ReadAllAsync()`, batched for performance, and forwarded to the Canvas via JS Interop.

### Vue.js client (`drawing-client-vue`)

Registered as `<vuejs-grpc-client>` using Vue 3's `defineCustomElement`:

```typescript
import { defineCustomElement } from 'vue';
import DrawingCanvas from './components/DrawingCanvas.ce.vue';

const DrawingCanvasElement = defineCustomElement(DrawingCanvas);
customElements.define('vuejs-grpc-client', DrawingCanvasElement);
```

Uses a custom **minimal gRPC-Web transport** built on the Fetch API (`src/grpc/transport.ts`) — no gRPC framework dependency needed. Protobuf encoding/decoding is handled by `protobufjs/light` with a hand-written schema (`src/proto/schema.ts`) matching the `.proto` file.

Vite builds the entire app as a **single self-contained IIFE** bundle (`vue-drawing-client.js`) — no external dependencies at runtime.

### Embedding in the host (`DemoApp.Server`)

The `DemoApp.Server.csproj` contains two MSBuild targets that run before `Build`:

1. **`PublishDrawingClientWasm`** — publishes `DrawingClient.Wasm` in Release mode and copies the output to `wwwroot/drawing-client/` (same approach as the calculator integration).
2. **`BuildVueDrawingClient`** — runs `npm install` + `npm run build:only` in the `drawing-client-vue` directory and copies `dist/vue-drawing-client.js` to `wwwroot/vue-client/`.

Both scripts are loaded in `App.razor`:

```html
<script src="drawing-client/_framework/blazor.webassembly.js"></script>
<script src="vue-client/vue-drawing-client.js"></script>
```

Then used side-by-side on the home page:

```html
<dotnet-grpc-client service-url="https://localhost:5001"
                    scene="landschap" width="800" height="600">
</dotnet-grpc-client>

<vuejs-grpc-client service-url="https://localhost:5001"
                   scene="landschap" width="800" height="600">
</vuejs-grpc-client>
```

Both elements accept the same attributes: `service-url`, `scene`, `width`, `height`, and `auto-play`.

### Available scenes

| Scene | Description |
|-------|-------------|
| 🌳 `boom` | A tree with trunk and crown |
| 🏠 `huis` | A house with roof, windows and door |
| ☁️ `wolk` | A fluffy cloud |
| ☀️ `zon` | A sun with rays |
| 🚗 `auto` | A car on the road |
| 🏞️ `landschap` | All elements combined |

---

## .NET Aspire Orchestration

The `CustomElements.AppHost` project orchestrates all services for local development:

```csharp
var drawingService = builder.AddProject<Projects.DrawingService_Grpc>("drawing-service");

var demoApp = builder.AddProject<Projects.DemoApp_Server>("demo-app")
    .WithExternalHttpEndpoints()
    .WaitFor(drawingService);

builder.AddProject<Projects.CalcApp_Server>("calc-app")
    .WithExternalHttpEndpoints();
```

This ensures the gRPC drawing service is running before the demo app starts, and provides the Aspire dashboard for monitoring all services.

---

## Solution Structure

```
TestApp1.slnx
│
├── CustomElements.AppHost/          # .NET Aspire orchestrator
│   └── Program.cs                   # Wires up all services
│
├── CustomElements.ServiceDefaults/  # Shared Aspire service defaults
│   └── Extensions.cs
│
├── protos/
│   └── drawing.proto                # Shared gRPC contract
│
├── DrawingService.Grpc/             # gRPC server
│   ├── Program.cs                   # gRPC + CORS + gRPC-Web setup
│   ├── Services/
│   │   └── DrawingCanvasService.cs  # StreamScene + GetAvailableScenes
│   └── SceneRenderers/
│       ├── ISceneRenderer.cs        # Pluggable renderer interface
│       ├── LandscapeRenderer.cs     # Composite scene
│       ├── TreeRenderer.cs
│       ├── HouseRenderer.cs
│       ├── CloudRenderer.cs
│       ├── SunRenderer.cs
│       ├── CarRenderer.cs
│       └── Draw.cs                  # Helper for building DrawCommand messages
│
├── DrawingClient.Wasm/              # Blazor WASM custom element
│   ├── Program.cs                   # RegisterCustomElement<...>("dotnet-grpc-client")
│   └── Components/
│       ├── DrawingCanvas.razor      # gRPC streaming + Canvas rendering
│       └── DrawingCanvas.razor.css  # Scoped styles
│
├── drawing-client-vue/              # Vue.js custom element (TypeScript)
│   ├── package.json                 # Vue 3 + protobufjs + Vite
│   ├── vite.config.ts               # Builds single IIFE bundle
│   └── src/
│       ├── main.ts                  # defineCustomElement("vuejs-grpc-client")
│       ├── components/
│       │   └── DrawingCanvas.ce.vue # Vue component (custom element mode)
│       ├── canvas/
│       │   └── renderer.ts          # Canvas 2D command executor
│       ├── grpc/
│       │   ├── client.ts            # Type-safe gRPC client wrapper
│       │   └── transport.ts         # Minimal Fetch-based gRPC-Web transport
│       └── proto/
│           └── schema.ts            # Protobuf schema (protobufjs/light)
│
├── DemoApp.Server/                  # Blazor Server host for drawing demo
│   ├── DemoApp.Server.csproj        # MSBuild targets: publish WASM + build Vue
│   ├── Program.cs                   # MIME types, static files, Blazor Server
│   └── Components/
│       ├── App.razor                # Loads both client scripts
│       ├── Layout/
│       │   └── MainLayout.razor
│       └── Pages/
│           ├── Home.razor           # Side-by-side <dotnet-grpc-client> + <vuejs-grpc-client>
│           └── Tutorial.razor       # Integration tutorial page
│
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
            └── Integration.razor    # Integration guide, rendered as a page
```

---

## Integration Guide

A detailed in-app guide is available at `/integration` (CalcApp) and `/tutorial` (DemoApp) when the apps are running.  
They cover all steps, pitfalls, and code examples — generated from the actual implementation in this repo.

---

## Technology

| | |
|---|---|
| **Runtime** | .NET 10 |
| **Framework** | ASP.NET Core 10 / Blazor |
| **Orchestration** | .NET Aspire |
| **Rendering** | Blazor Server (hosts) + Blazor WebAssembly + Vue.js 3 (components) |
| **Custom Elements API** | `Microsoft.AspNetCore.Components.CustomElements` / Vue `defineCustomElement` |
| **Communication** | gRPC-Web (server streaming) via `Grpc.Net.Client.Web` and custom Fetch transport |
| **Protobuf** | `Google.Protobuf` + `Grpc.Tools` (.NET) / `protobufjs/light` (Vue.js) |
| **Build** | MSBuild (WASM publish) + Vite (Vue IIFE bundle) |
| **Styling** | Windows 11 Fluent Design (Calculator, CSS scoped) |
