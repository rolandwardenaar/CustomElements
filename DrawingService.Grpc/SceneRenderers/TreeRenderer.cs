using DrawingService.Grpc.Protos;

namespace DrawingService.Grpc.SceneRenderers;

/// <summary>Renders a tree with a brown trunk and layered green crown.</summary>
internal sealed class TreeRenderer : ISceneRenderer
{
    public string SceneName => "boom";

    public IReadOnlyList<DrawCommand> Render(double canvasWidth, double canvasHeight)
    {
        var cx = canvasWidth / 2;
        var groundY = canvasHeight * 0.85;
        var trunkW = canvasWidth * 0.06;
        var trunkH = canvasHeight * 0.25;
        var crownR = canvasWidth * 0.15;

        return
        [
            Draw.Clear(canvasWidth, canvasHeight),

            // Sky background
            Draw.FillColor("#87CEEB"),
            Draw.Rect(0, 0, canvasWidth, canvasHeight),

            // Grass
            Draw.FillColor("#228B22"),
            Draw.Rect(0, groundY, canvasWidth, canvasHeight - groundY),

            // Trunk
            Draw.FillColor("#8B4513"),
            Draw.Rect(cx - trunkW / 2, groundY - trunkH, trunkW, trunkH),

            // Crown — layered circles for a natural look
            Draw.FillColor("#2E8B57"),
            Draw.Begin(),
            Draw.Circle(cx - crownR * 0.5, groundY - trunkH - crownR * 0.3, crownR * 0.7),
            Draw.FillPath(),

            Draw.Begin(),
            Draw.Circle(cx + crownR * 0.5, groundY - trunkH - crownR * 0.3, crownR * 0.7),
            Draw.FillPath(),

            Draw.FillColor("#228B22"),
            Draw.Begin(),
            Draw.Circle(cx, groundY - trunkH - crownR * 0.6, crownR * 0.85),
            Draw.FillPath(),

            // Top highlight
            Draw.FillColor("#32CD32"),
            Draw.Begin(),
            Draw.Circle(cx, groundY - trunkH - crownR, crownR * 0.5),
            Draw.FillPath(),
        ];
    }
}
