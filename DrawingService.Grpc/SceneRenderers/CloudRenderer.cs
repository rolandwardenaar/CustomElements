using DrawingService.Grpc.Protos;

namespace DrawingService.Grpc.SceneRenderers;

/// <summary>Renders a fluffy cloud from overlapping white circles.</summary>
internal sealed class CloudRenderer : ISceneRenderer
{
    public string SceneName => "wolk";

    public IReadOnlyList<DrawCommand> Render(double canvasWidth, double canvasHeight)
    {
        var cx = canvasWidth / 2;
        var cy = canvasHeight * 0.30;
        var baseR = canvasWidth * 0.07;

        return
        [
            Draw.Clear(canvasWidth, canvasHeight),

            // Sky
            Draw.FillColor("#87CEEB"),
            Draw.Rect(0, 0, canvasWidth, canvasHeight),

            .. RenderCloudAt(cx, cy, baseR),
        ];
    }

    /// <summary>Renders a single cloud centred at (<paramref name="cx"/>, <paramref name="cy"/>).</summary>
    internal static List<DrawCommand> RenderCloudAt(double cx, double cy, double baseR)
    {
        return
        [
            // Shadow
            Draw.FillColor("#D3D3D3"),
            Draw.Begin(),
            Draw.Circle(cx, cy + baseR * 0.25, baseR * 1.1),
            Draw.FillPath(),

            // Main body — overlapping white circles
            Draw.FillColor("#FFFFFF"),
            Draw.Begin(),
            Draw.Circle(cx - baseR * 1.0, cy, baseR * 0.75),
            Draw.FillPath(),

            Draw.Begin(),
            Draw.Circle(cx + baseR * 1.0, cy, baseR * 0.75),
            Draw.FillPath(),

            Draw.Begin(),
            Draw.Circle(cx - baseR * 0.4, cy - baseR * 0.4, baseR * 0.85),
            Draw.FillPath(),

            Draw.Begin(),
            Draw.Circle(cx + baseR * 0.4, cy - baseR * 0.4, baseR * 0.85),
            Draw.FillPath(),

            Draw.Begin(),
            Draw.Circle(cx, cy, baseR),
            Draw.FillPath(),
        ];
    }
}
