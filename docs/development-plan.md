# Ontwikkelplan: gRPC Custom Element Clients

> **Project:** CustomElements  
> **Repository:** https://github.com/rolandwardenaar/CustomElements  
> **Doelframework:** .NET 10  
> **Datum:** Juni 2025

---

## 1. Projectoverzicht

Dit project breidt de bestaande CustomElements-oplossing uit met twee herbruikbare **Web Custom Elements** die via **gRPC-Web** communiceren met een backend service. De clients ontvangen tekencommando's en renderen deze op een HTML5 Canvas.

### 1.1 Bestaande situatie

| Project | Beschrijving |
|---|---|
| `CalcApp.Server` | Blazor Server host-applicatie met `<calc-ulator>` custom element |
| `Calculator.Wasm` | Blazor WebAssembly project dat de calculator als custom element registreert |

### 1.2 Gewenste situatie

| Component | Type | Custom Element Tag | Beschrijving |
|---|---|---|---|
| **DrawingService.Grpc** | gRPC Server | — | ASP.NET Core gRPC service die tekencommando's streamt |
| **DrawingClient.Wasm** | Blazor WASM | `<dotnet-grpc-client>` | .NET Blazor WebAssembly gRPC client met canvas |
| **DrawingClient.Vue** | Vue.js App | `<vuejs-grpc-client>` | Vue 3 gRPC-Web client met canvas |
| **DemoApp.Server** | Blazor Server | — | Voorbeeldapplicatie die beide clients integreert |

---

## 2. Architectuur

```
┌──────────────────────────────────────────────────────────────┐
│                      Browser (Host Page)                      │
│                                                                │
│  ┌─────────────────────────┐  ┌─────────────────────────────┐ │
│  │  <dotnet-grpc-client>   │  │  <vuejs-grpc-client>        │ │
│  │  Blazor WASM + Canvas   │  │  Vue 3 + Canvas             │ │
│  │  gRPC-Web (Protobuf)    │  │  gRPC-Web (Protobuf)        │ │
│  └───────────┬─────────────┘  └───────────┬─────────────────┘ │
│              │                             │                   │
└──────────────┼─────────────────────────────┼───────────────────┘
               │         gRPC-Web            │
               └──────────┬──────────────────┘
                          │
               ┌──────────▼──────────┐
               │  DrawingService.Grpc │
               │  ASP.NET Core gRPC   │
               │  Server Streaming    │
               └──────────────────────┘
```

### 2.1 Communicatieprotocol

- **gRPC-Web** (niet standaard gRPC/HTTP2) omdat browsers geen native HTTP/2 gRPC ondersteunen
- **Server Streaming** — de server stuurt een stroom van tekencommando's naar de client
- **Unary calls** — client kan een tekening aanvragen (bijv. "teken een huis")

### 2.2 Protobuf Schema (voorstel)

```protobuf
syntax = "proto3";

package drawing;

option csharp_namespace = "DrawingService.Grpc.Protos";

// Tekenservice
service DrawingCanvas {
  // Vraag een volledige scène op (server streaming)
  rpc StreamScene (SceneRequest) returns (stream DrawCommand);
  
  // Vraag beschikbare scènes op
  rpc GetAvailableScenes (Empty) returns (SceneList);
}

message Empty {}

message SceneRequest {
  string scene_name = 1;  // "boom", "huis", "wolk", "zon", "auto", "landschap"
}

message SceneList {
  repeated string scene_names = 1;
}

// Individueel tekencommando
message DrawCommand {
  oneof command {
    SetColor set_color = 1;
    MoveTo move_to = 2;
    LineTo line_to = 3;
    FillRect fill_rect = 4;
    Arc arc = 5;
    BeginPath begin_path = 6;
    ClosePath close_path = 7;
    Fill fill = 8;
    Stroke stroke = 9;
    SetLineWidth set_line_width = 10;
    ClearCanvas clear_canvas = 11;
    QuadraticCurveTo quadratic_curve_to = 12;
    BezierCurveTo bezier_curve_to = 13;
  }
}

message SetColor {
  string color = 1;  // CSS kleur, bijv. "#228B22", "rgba(135,206,235,0.8)"
}

message MoveTo {
  double x = 1;
  double y = 2;
}

message LineTo {
  double x = 1;
  double y = 2;
}

message FillRect {
  double x = 1;
  double y = 2;
  double width = 3;
  double height = 4;
}

message Arc {
  double x = 1;
  double y = 2;
  double radius = 3;
  double start_angle = 4;
  double end_angle = 5;
}

message BeginPath {}
message ClosePath {}
message Fill {}
message Stroke {}

message SetLineWidth {
  double width = 1;
}

message ClearCanvas {
  double width = 1;
  double height = 2;
}

message QuadraticCurveTo {
  double cpx = 1;
  double cpy = 2;
  double x = 3;
  double y = 4;
}

message BezierCurveTo {
  double cp1x = 1;
  double cp1y = 2;
  double cp2x = 3;
  double cp2y = 4;
  double x = 5;
  double y = 6;
}
```

---

## 3. Projectstructuur (doel)

```
CustomElements/
├── docs/
│   ├── development-plan.md          ← dit document
│   └── tutorial.md                  ← integratiehandleiding
│
├── protos/
│   └── drawing.proto                ← gedeeld Protobuf schema
│
├── DrawingService.Grpc/             ← gRPC Server
│   ├── Protos/
│   │   └── drawing.proto            ← (link naar /protos/)
│   ├── Services/
│   │   └── DrawingCanvasService.cs  ← gRPC service implementatie
│   ├── SceneRenderers/
│   │   ├── ISceneRenderer.cs
│   │   ├── TreeRenderer.cs          ← boom
│   │   ├── HouseRenderer.cs         ← huis
│   │   ├── CloudRenderer.cs         ← wolk
│   │   ├── SunRenderer.cs           ← zon
│   │   ├── CarRenderer.cs           ← auto
│   │   └── LandscapeRenderer.cs     ← volledige scène
│   ├── Program.cs
│   └── DrawingService.Grpc.csproj
│
├── DrawingClient.Wasm/              ← Blazor WASM Custom Element
│   ├── Components/
│   │   └── DrawingCanvas.razor      ← Blazor component met canvas
│   ├── wwwroot/
│   │   └── js/
│   │       └── canvas-interop.js    ← JS interop voor canvas API
│   ├── Program.cs                   ← RegisterCustomElement<DrawingCanvas>("dotnet-grpc-client")
│   └── DrawingClient.Wasm.csproj
│
├── drawing-client-vue/              ← Vue.js Custom Element
│   ├── src/
│   │   ├── components/
│   │   │   └── DrawingCanvas.ce.vue ← Vue component (Custom Element mode)
│   │   ├── grpc/
│   │   │   ├── drawing_pb.js        ← gegenereerd uit proto
│   │   │   └── drawing_grpc_web_pb.js
│   │   ├── canvas/
│   │   │   └── renderer.ts          ← canvas tekenlogica
│   │   └── main.ts                  ← defineCustomElement registratie
│   ├── package.json
│   ├── vite.config.ts
│   └── tsconfig.json
│
├── DemoApp.Server/                  ← Voorbeeld host-applicatie
│   ├── Components/
│   │   ├── App.razor
│   │   └── Pages/
│   │       ├── Home.razor            ← demo pagina met beide clients
│   │       ├── DotnetClientDemo.razor
│   │       ├── VueClientDemo.razor
│   │       └── Tutorial.razor        ← interactieve tutorial
│   ├── Program.cs
│   └── DemoApp.Server.csproj
│
├── CalcApp.Server/                  ← (bestaand)
├── Calculator.Wasm/                 ← (bestaand)
├── requirements.md
└── CustomElements.slnx              ← uitgebreide solution
```

---

## 4. Ontwikkelfasen

### Fase 1: Fundament — gRPC Service (Week 1-2)

| Stap | Taak | Details |
|---:|---|---|
| 1.1 | **Protobuf schema ontwerpen** | `drawing.proto` met alle tekencommando's (zie §2.2) |
| 1.2 | **gRPC Server project aanmaken** | `DrawingService.Grpc` — ASP.NET Core met gRPC + gRPC-Web |
| 1.3 | **Scene renderers implementeren** | Aparte klasse per tekening: boom, huis, wolk, zon, auto |
| 1.4 | **Landschapsscène samenstellen** | Combineert alle elementen tot één volledige scène |
| 1.5 | **gRPC-Web middleware configureren** | `Grpc.AspNetCore.Web` package, CORS-configuratie |
| 1.6 | **Server testen** | Unit tests voor renderers, integratitest met `grpcurl` |

**Deliverables:**
- Werkende gRPC server op `https://localhost:5001`
- Streaming endpoint dat tekencommando's verstuurt
- 5 individuele scènes + 1 samengestelde landschapsscène

---

### Fase 2: .NET Blazor WASM Client (Week 2-3)

| Stap | Taak | Details |
|---:|---|---|
| 2.1 | **Blazor WASM project aanmaken** | `DrawingClient.Wasm` met `CustomElements` package |
| 2.2 | **Canvas JS Interop schrijven** | JavaScript module voor Canvas 2D API aanroepen vanuit C# |
| 2.3 | **gRPC-Web client configureren** | `Grpc.Net.Client.Web` package, `GrpcWebHandler` |
| 2.4 | **DrawingCanvas component bouwen** | Blazor component dat gRPC stream ontvangt en op canvas tekent |
| 2.5 | **Custom Element registreren** | `RegisterCustomElement<DrawingCanvas>("dotnet-grpc-client")` |
| 2.6 | **Attributen toevoegen** | `service-url`, `scene`, `width`, `height` als HTML attributen |
| 2.7 | **Publicatie MSBuild target** | Zoals bestaande `PublishCalculatorWasm` target |

**Custom Element API:**
```html
<dotnet-grpc-client 
    service-url="https://localhost:5001" 
    scene="landschap"
    width="800" 
    height="600">
</dotnet-grpc-client>
```

**Technische keuzes:**
- `Grpc.Net.Client.Web` voor gRPC-Web ondersteuning in WASM
- `IJSRuntime` voor canvas interop (geen 3rd party canvas library)
- Server streaming voor real-time tekenen (commando voor commando)

---

### Fase 3: Vue.js Client (Week 3-4)

| Stap | Taak | Details |
|---:|---|---|
| 3.1 | **Vue 3 project opzetten** | Vite + TypeScript + Vue 3 |
| 3.2 | **Protobuf code genereren** | `protoc` met `grpc-web` plugin → JS/TS client code |
| 3.3 | **Canvas renderer schrijven** | TypeScript klasse die `DrawCommand` → Canvas 2D vertaalt |
| 3.4 | **Vue component bouwen** | `DrawingCanvas.ce.vue` met props voor configuratie |
| 3.5 | **Custom Element registreren** | `defineCustomElement()` → `customElements.define('vuejs-grpc-client', ...)` |
| 3.6 | **Build configureren** | Vite library mode, output als single JS bundle |

**Custom Element API:**
```html
<vuejs-grpc-client 
    service-url="https://localhost:5001" 
    scene="landschap"
    width="800" 
    height="600">
</vuejs-grpc-client>
```

**Technische keuzes:**
- `grpc-web` npm package (officiële Google gRPC-Web client)
- `protoc-gen-grpc-web` voor TypeScript codegeneratie
- Vue 3 `defineCustomElement` API voor Web Component registratie
- Vite `lib` mode voor een compacte, zelfstandige JavaScript bundle

---

### Fase 4: Demo Applicatie & Integratie (Week 4-5)

| Stap | Taak | Details |
|---:|---|---|
| 4.1 | **DemoApp.Server project aanmaken** | Blazor Server host die beide clients toont |
| 4.2 | **WASM bundle integreren** | Publicatie-target + script tag (zoals bestaande calculator) |
| 4.3 | **Vue bundle integreren** | `<script src="vue-drawing-client.js">` + custom element tag |
| 4.4 | **Demo pagina's bouwen** | Side-by-side vergelijking, individuele demo's |
| 4.5 | **gRPC server proxy/CORS** | Reverse proxy of CORS configuratie voor cross-origin gRPC-Web |

---

### Fase 5: Tutorial & Documentatie (Week 5-6)

| Stap | Taak | Details |
|---:|---|---|
| 5.1 | **Integratiehandleiding schrijven** | `docs/tutorial.md` — stap-voor-stap gids |
| 5.2 | **Tutorial secties** | Zie §5 hieronder |
| 5.3 | **Interactieve tutorial pagina** | Blazor Server pagina met code voorbeelden en live demo |
| 5.4 | **README bijwerken** | Project overzicht, quick start, links naar tutorial |

---

## 5. Tutorial Inhoud (docs/tutorial.md)

De tutorial bevat de volgende secties:

### 5.1 Introductie
- Wat zijn Web Custom Elements?
- Wat is gRPC-Web en waarom gebruiken we het?
- Overzicht van de architectuur

### 5.2 Vereisten
- .NET 10 SDK
- Node.js 20+ (voor Vue client)
- Protobuf compiler (`protoc`)
- IDE: Visual Studio 2026 / VS Code

### 5.3 De gRPC Server opzetten
- Proto bestanden definiëren
- ASP.NET Core gRPC service implementeren
- gRPC-Web middleware configureren
- CORS instellen
- Server starten en testen

### 5.4 Blazor WASM Client (`<dotnet-grpc-client>`)
- Project aanmaken met `dotnet new blazorwasm`
- gRPC-Web client packages installeren
- Canvas component bouwen met JS Interop
- Custom element registreren
- Publiceren en integreren in een host-applicatie

```html
<!-- Voorbeeld: integratie in een willekeurige HTML pagina -->
<!DOCTYPE html>
<html>
<head>
    <title>Mijn App</title>
</head>
<body>
    <h1>Tekening via gRPC</h1>
    <dotnet-grpc-client 
        service-url="https://mijn-grpc-server.nl" 
        scene="landschap">
    </dotnet-grpc-client>

    <!-- Blazor WASM runtime laden -->
    <script src="drawing-client/_framework/blazor.webassembly.js"></script>
</body>
</html>
```

### 5.5 Vue.js Client (`<vuejs-grpc-client>`)
- Project opzetten met Vite + Vue 3
- Protobuf code genereren
- Canvas component bouwen
- Custom element registreren en bouwen
- Bundle integreren in een host-applicatie

```html
<!-- Voorbeeld: integratie in een willekeurige HTML pagina -->
<!DOCTYPE html>
<html>
<head>
    <title>Mijn App</title>
</head>
<body>
    <h1>Tekening via gRPC</h1>
    <vuejs-grpc-client 
        service-url="https://mijn-grpc-server.nl" 
        scene="boom">
    </vuejs-grpc-client>

    <!-- Vue bundle laden -->
    <script src="vue-drawing-client.js"></script>
</body>
</html>
```

### 5.6 Integratie in bestaande projecten
- **Plain HTML** — script tag + custom element tag
- **React** — wrapper component met `useRef`
- **Angular** — `CUSTOM_ELEMENTS_SCHEMA` + template tag
- **Blazor Server** — zoals de DemoApp (statische bestanden + script)
- **WordPress/CMS** — script in header, tag in content

### 5.7 Geavanceerd
- Meerdere clients op één pagina
- Dynamisch scènes wisselen via JavaScript
- Custom element attributen wijzigen via JS (`element.setAttribute('scene', 'auto')`)
- Events afvangen vanuit de custom elements

---

## 6. Tekenontwerpen (Canvas Scènes)

Elke scène wordt opgebouwd uit Canvas 2D API commando's die via gRPC worden gestreamd.

### 6.1 Boom 🌳
- **Stam:** bruin (`#8B4513`) rechthoek
- **Kroon:** groen (`#228B22`) cirkel/ovaal met meerdere lagen
- **Details:** donkergroene schaduw, kleine bladeren

### 6.2 Huis 🏠
- **Muren:** beige/crème (`#F5DEB3`) rechthoek
- **Dak:** rood (`#B22222`) driehoek (2 lijnen + fill)
- **Deur:** bruin rechthoek met gele cirkel (deurknop)
- **Ramen:** lichtblauw (`#87CEEB`) rechthoeken met kruis (roeden)
- **Schoorsteen:** donkerrood rechthoek op het dak

### 6.3 Wolk ☁️
- **Vorm:** meerdere overlappende witte (`#FFFFFF`) cirkels
- **Schaduw:** lichtgrijs (`#D3D3D3`) aan de onderkant
- **Variatie:** verschillende groottes voor natuurlijk effect

### 6.4 Zon ☀️
- **Schijf:** geel (`#FFD700`) gevulde cirkel
- **Stralen:** oranje (`#FFA500`) lijnen vanuit het centrum
- **Gloed:** lichtgele (`#FFFFE0`) semi-transparante buitencirkel

### 6.5 Auto 🚗
- **Carrosserie:** rood (`#DC143C`) afgeronde rechthoek
- **Wielen:** zwarte (`#000000`) cirkels met grijze velgen
- **Ramen:** lichtblauw (`#87CEEB`) trapeziumvorm
- **Details:** koplampen (geel), achterlichten (rood), bumper (grijs)

### 6.6 Landschap (gecombineerd) 🏞️
Volledige scène met alle elementen:
- **Achtergrond:** lichtblauwe lucht gradient → groen gras
- **Zon** rechtsboven
- **Wolken** verspreid over de lucht (2-3 stuks)
- **Huis** centraal
- **Boom** links van het huis
- **Auto** rechts op de "weg" (grijze strook onderaan)

---

## 7. Technische Specificaties

### 7.1 NuGet Packages (Server)

| Package | Doel |
|---|---|
| `Grpc.AspNetCore` | gRPC server framework |
| `Grpc.AspNetCore.Web` | gRPC-Web middleware (HTTP/1.1 compatibiliteit) |

### 7.2 NuGet Packages (Blazor WASM Client)

| Package | Doel |
|---|---|
| `Microsoft.AspNetCore.Components.CustomElements` | Custom element registratie |
| `Microsoft.AspNetCore.Components.WebAssembly` | Blazor WASM runtime |
| `Grpc.Net.Client.Web` | gRPC-Web handler voor HttpClient |
| `Google.Protobuf` | Protobuf serialisatie |
| `Grpc.Net.Client` | gRPC client |
| `Grpc.Tools` | Proto compilatie (build-time) |

### 7.3 NPM Packages (Vue Client)

| Package | Doel |
|---|---|
| `vue` | Vue 3 framework |
| `grpc-web` | gRPC-Web client library |
| `google-protobuf` | Protobuf runtime |
| `protoc-gen-grpc-web` | Code generator (dev) |
| `vite` | Build tool |
| `typescript` | Type checking |

### 7.4 Canvas Custom Element Attributen

Beide custom elements ondersteunen dezelfde HTML attributen:

| Attribuut | Type | Default | Beschrijving |
|---|---|---|---|
| `service-url` | string | `""` | URL van de gRPC server |
| `scene` | string | `"landschap"` | Naam van de te tekenen scène |
| `width` | number | `800` | Canvas breedte in pixels |
| `height` | number | `600` | Canvas hoogte in pixels |
| `auto-play` | boolean | `true` | Start tekenen bij laden |

---

## 8. Tijdlijn

```
Week 1  ████████░░░░░░░░░░░░  Proto schema + gRPC server basis
Week 2  ░░░░████████░░░░░░░░  Scene renderers + Blazor WASM client start
Week 3  ░░░░░░░░████████░░░░  Blazor WASM client + Vue.js client start
Week 4  ░░░░░░░░░░░░████████  Vue.js client + Demo applicatie
Week 5  ░░░░░░░░░░░░░░░░████  Integratie, tutorial, documentatie
Week 6  ░░░░░░░░░░░░░░░░░░██  Afronding, testen, review
```

### Mijlpalen

| Mijlpaal | Week | Criterium |
|---|---|---|
| **M1: gRPC Server draait** | Week 1 | Streaming tekencommando's via grpcurl |
| **M2: .NET Client tekent** | Week 3 | `<dotnet-grpc-client>` toont landschap |
| **M3: Vue Client tekent** | Week 4 | `<vuejs-grpc-client>` toont landschap |
| **M4: Demo App compleet** | Week 5 | Beide clients naast elkaar werkend |
| **M5: Tutorial klaar** | Week 6 | Volledige documentatie + tutorial pagina |

---

## 9. Risico's & Aandachtspunten

| Risico | Impact | Mitigatie |
|---|---|---|
| **gRPC-Web beperking**: geen bidirectionele streaming | Gemiddeld | Gebruik server streaming + unary calls |
| **Blazor WASM bestandsgrootte** (~15-20 MB) | Gemiddeld | AOT compilatie, tree shaking, lazy loading |
| **CORS configuratie** | Laag | Duidelijke documentatie, development proxy |
| **Canvas rendering performance** | Laag | Batch commando's, requestAnimationFrame |
| **Proto schema wijzigingen** | Gemiddeld | Proto schema als shared project/directory |
| **Vue custom element Shadow DOM** | Laag | Gebruik `defineCustomElement` met style injection |

---

## 10. Volgorde van implementatie (aanbevolen)

```
1. Proto schema              → gedeeld contract
2. gRPC Server               → backend gereed
3. Blazor WASM Client        → volgt bestaand patroon (Calculator.Wasm)
4. Demo integratie (.NET)     → proof of concept
5. Vue.js Client             → tweede implementatie
6. Demo integratie (Vue)      → volledig werkend
7. Tutorial & documentatie    → kennisborging
```

Deze volgorde is gekozen omdat:
- Het **proto schema** het gedeelde contract is waar alles op bouwt
- De **gRPC server** moet draaien voordat clients getest kunnen worden
- De **Blazor WASM client** volgt het bestaande patroon van `Calculator.Wasm`
- De **Vue.js client** kan parallel ontwikkeld worden zodra het proto schema stabiel is
