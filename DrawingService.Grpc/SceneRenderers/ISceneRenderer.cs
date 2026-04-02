using DrawingService.Grpc.Protos;

namespace DrawingService.Grpc.SceneRenderers;

/// <summary>
/// Produces a sequence of <see cref="DrawCommand"/> messages that together
/// render a specific scene on an HTML5 Canvas.
/// </summary>
internal interface ISceneRenderer
{
    /// <summary>Scene identifier used in <see cref="SceneRequest.SceneName"/>.</summary>
    string SceneName { get; }

    /// <summary>Returns all draw commands for this scene.</summary>
    IReadOnlyList<DrawCommand> Render(double canvasWidth, double canvasHeight);
}
