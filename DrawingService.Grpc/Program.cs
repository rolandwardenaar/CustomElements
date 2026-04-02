using DrawingService.Grpc.SceneRenderers;
using DrawingService.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpc();

// Register all scene renderers
builder.Services.AddSingleton<ISceneRenderer, TreeRenderer>();
builder.Services.AddSingleton<ISceneRenderer, HouseRenderer>();
builder.Services.AddSingleton<ISceneRenderer, CloudRenderer>();
builder.Services.AddSingleton<ISceneRenderer, SunRenderer>();
builder.Services.AddSingleton<ISceneRenderer, CarRenderer>();
builder.Services.AddSingleton<ISceneRenderer, LandscapeRenderer>();

// CORS policy for gRPC-Web — allows browser clients on different origins
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
    });
});

var app = builder.Build();

app.UseCors();

// Enable gRPC-Web for all services so browser clients can connect via HTTP/1.1
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

app.MapGrpcService<DrawingCanvasService>();

app.MapDefaultEndpoints();

app.MapGet("/", () => "DrawingService gRPC server is running. Use a gRPC client to connect.");

app.Run();
