using DrawingService.Grpc.Protos;
using DrawingService.Grpc.SceneRenderers;
using Grpc.Core;

namespace DrawingService.Grpc.Services;

/// <summary>
/// gRPC service that streams Canvas 2D draw commands for named scenes.
/// </summary>
internal sealed class DrawingCanvasService(
    IEnumerable<ISceneRenderer> renderers,
    ILogger<DrawingCanvasService> logger) : DrawingCanvas.DrawingCanvasBase
{
    private readonly Dictionary<string, ISceneRenderer> _renderers =
        renderers.ToDictionary(r => r.SceneName, StringComparer.OrdinalIgnoreCase);

    public override async Task StreamScene(
        SceneRequest request,
        IServerStreamWriter<DrawCommand> responseStream,
        ServerCallContext context)
    {
        if (!_renderers.TryGetValue(request.SceneName, out var renderer))
        {
            logger.LogWarning("Unknown scene requested: {SceneName}", request.SceneName);
            throw new RpcException(new Status(StatusCode.NotFound,
                $"Scene '{request.SceneName}' not found. Use GetAvailableScenes to list available scenes."));
        }

        logger.LogInformation("Streaming scene: {SceneName}", request.SceneName);

        var commands = renderer.Render(800, 600);

        foreach (var command in commands)
        {
            if (context.CancellationToken.IsCancellationRequested)
                break;

            await responseStream.WriteAsync(command, context.CancellationToken);

            // Small delay between commands for a visible drawing effect
            await Task.Delay(15, context.CancellationToken);
        }
    }

    public override Task<SceneList> GetAvailableScenes(Empty request, ServerCallContext context)
    {
        var list = new SceneList();
        list.SceneNames.AddRange(_renderers.Keys.Order());
        return Task.FromResult(list);
    }
}
