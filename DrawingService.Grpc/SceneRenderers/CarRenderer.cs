using DrawingService.Grpc.Protos;

namespace DrawingService.Grpc.SceneRenderers;

/// <summary>Renders a car with body, wheels, windows, and details.</summary>
internal sealed class CarRenderer : ISceneRenderer
{
    public string SceneName => "auto";

    public IReadOnlyList<DrawCommand> Render(double canvasWidth, double canvasHeight)
    {
        var groundY = canvasHeight * 0.85;
        var carW = canvasWidth * 0.45;
        var carH = canvasHeight * 0.12;
        var carX = (canvasWidth - carW) / 2;
        var carY = groundY - carH - canvasHeight * 0.04;

        return
        [
            Draw.Clear(canvasWidth, canvasHeight),

            // Sky
            Draw.FillColor("#87CEEB"),
            Draw.Rect(0, 0, canvasWidth, canvasHeight),

            // Road
            Draw.FillColor("#555555"),
            Draw.Rect(0, groundY, canvasWidth, canvasHeight - groundY),

            // Road line
            Draw.StrokeColor("#FFFFFF"),
            Draw.LineWidth(3),
            Draw.Begin(),
            Draw.Move(0, groundY + (canvasHeight - groundY) / 2),
            Draw.Line(canvasWidth, groundY + (canvasHeight - groundY) / 2),
            Draw.StrokePath(),

            .. RenderCarAt(carX, carY, carW, carH),
        ];
    }

    /// <summary>Renders a car with top-left at (<paramref name="x"/>, <paramref name="y"/>).</summary>
    internal static List<DrawCommand> RenderCarAt(double x, double y, double w, double h)
    {
        var wheelR = h * 0.35;
        var cabinH = h * 0.7;

        return
        [
            // Body
            Draw.FillColor("#DC143C"),
            Draw.Rect(x, y, w, h),

            // Cabin (top part)
            Draw.FillColor("#DC143C"),
            Draw.Begin(),
            Draw.Move(x + w * 0.2, y),
            Draw.Line(x + w * 0.3, y - cabinH),
            Draw.Line(x + w * 0.75, y - cabinH),
            Draw.Line(x + w * 0.85, y),
            Draw.Close(),
            Draw.FillPath(),

            // Windows
            Draw.FillColor("#87CEEB"),
            Draw.Begin(),
            Draw.Move(x + w * 0.32, y - cabinH * 0.1),
            Draw.Line(x + w * 0.34, y - cabinH * 0.85),
            Draw.Line(x + w * 0.52, y - cabinH * 0.85),
            Draw.Line(x + w * 0.52, y - cabinH * 0.1),
            Draw.Close(),
            Draw.FillPath(),

            Draw.Begin(),
            Draw.Move(x + w * 0.55, y - cabinH * 0.1),
            Draw.Line(x + w * 0.55, y - cabinH * 0.85),
            Draw.Line(x + w * 0.72, y - cabinH * 0.85),
            Draw.Line(x + w * 0.82, y - cabinH * 0.1),
            Draw.Close(),
            Draw.FillPath(),

            // Front headlight
            Draw.FillColor("#FFD700"),
            Draw.Rect(x + w - w * 0.03, y + h * 0.2, w * 0.03, h * 0.25),

            // Rear light
            Draw.FillColor("#FF0000"),
            Draw.Rect(x, y + h * 0.2, w * 0.02, h * 0.25),

            // Front wheel
            Draw.FillColor("#000000"),
            Draw.Begin(),
            Draw.Circle(x + w * 0.25, y + h, wheelR),
            Draw.FillPath(),
            Draw.FillColor("#888888"),
            Draw.Begin(),
            Draw.Circle(x + w * 0.25, y + h, wheelR * 0.5),
            Draw.FillPath(),

            // Rear wheel
            Draw.FillColor("#000000"),
            Draw.Begin(),
            Draw.Circle(x + w * 0.75, y + h, wheelR),
            Draw.FillPath(),
            Draw.FillColor("#888888"),
            Draw.Begin(),
            Draw.Circle(x + w * 0.75, y + h, wheelR * 0.5),
            Draw.FillPath(),
        ];
    }
}
