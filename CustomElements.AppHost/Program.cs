var builder = DistributedApplication.CreateBuilder(args);

// gRPC Drawing Service — streams canvas draw commands to clients
var drawingService = builder.AddProject<Projects.DrawingService_Grpc>("drawing-service");

// Demo App — Blazor Server host that embeds the <dotnet-grpc-client> custom element
var demoApp = builder.AddProject<Projects.DemoApp_Server>("demo-app")
    .WithExternalHttpEndpoints()
    .WaitFor(drawingService);

// Calculator App — Blazor Server host that embeds the <calc-ulator> custom element
builder.AddProject<Projects.CalcApp_Server>("calc-app")
    .WithExternalHttpEndpoints();

builder.Build().Run();
