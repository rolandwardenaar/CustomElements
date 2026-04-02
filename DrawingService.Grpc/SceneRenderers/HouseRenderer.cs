using DrawingService.Grpc.Protos;

namespace DrawingService.Grpc.SceneRenderers;

/// <summary>Renders a house with walls, roof, door, windows, and chimney.</summary>
internal sealed class HouseRenderer : ISceneRenderer
{
    public string SceneName => "huis";

    public IReadOnlyList<DrawCommand> Render(double canvasWidth, double canvasHeight)
    {
        var groundY = canvasHeight * 0.85;
        var houseW = canvasWidth * 0.40;
        var houseH = canvasHeight * 0.35;
        var houseX = (canvasWidth - houseW) / 2;
        var houseY = groundY - houseH;
        var roofPeak = houseY - houseH * 0.45;

        var doorW = houseW * 0.15;
        var doorH = houseH * 0.4;
        var doorX = houseX + houseW / 2 - doorW / 2;
        var doorY = groundY - doorH;

        var winW = houseW * 0.15;
        var winH = houseH * 0.2;
        var winY = houseY + houseH * 0.25;

        return
        [
            Draw.Clear(canvasWidth, canvasHeight),

            // Sky
            Draw.FillColor("#87CEEB"),
            Draw.Rect(0, 0, canvasWidth, canvasHeight),

            // Grass
            Draw.FillColor("#228B22"),
            Draw.Rect(0, groundY, canvasWidth, canvasHeight - groundY),

            // Walls
            Draw.FillColor("#F5DEB3"),
            Draw.Rect(houseX, houseY, houseW, houseH),

            // Roof
            Draw.FillColor("#B22222"),
            Draw.Begin(),
            Draw.Move(houseX - houseW * 0.05, houseY),
            Draw.Line(houseX + houseW / 2, roofPeak),
            Draw.Line(houseX + houseW + houseW * 0.05, houseY),
            Draw.Close(),
            Draw.FillPath(),

            // Chimney
            Draw.FillColor("#8B0000"),
            Draw.Rect(houseX + houseW * 0.7, roofPeak + (houseY - roofPeak) * 0.15,
                       houseW * 0.1, (houseY - roofPeak) * 0.55),

            // Door
            Draw.FillColor("#8B4513"),
            Draw.Rect(doorX, doorY, doorW, doorH),

            // Door knob
            Draw.FillColor("#FFD700"),
            Draw.Begin(),
            Draw.Circle(doorX + doorW * 0.75, doorY + doorH * 0.55, doorW * 0.08),
            Draw.FillPath(),

            // Left window
            Draw.FillColor("#87CEEB"),
            Draw.Rect(houseX + houseW * 0.12, winY, winW, winH),
            // Window cross
            Draw.StrokeColor("#FFFFFF"),
            Draw.LineWidth(2),
            Draw.Begin(),
            Draw.Move(houseX + houseW * 0.12 + winW / 2, winY),
            Draw.Line(houseX + houseW * 0.12 + winW / 2, winY + winH),
            Draw.Move(houseX + houseW * 0.12, winY + winH / 2),
            Draw.Line(houseX + houseW * 0.12 + winW, winY + winH / 2),
            Draw.StrokePath(),

            // Right window
            Draw.FillColor("#87CEEB"),
            Draw.Rect(houseX + houseW * 0.73, winY, winW, winH),
            Draw.StrokeColor("#FFFFFF"),
            Draw.LineWidth(2),
            Draw.Begin(),
            Draw.Move(houseX + houseW * 0.73 + winW / 2, winY),
            Draw.Line(houseX + houseW * 0.73 + winW / 2, winY + winH),
            Draw.Move(houseX + houseW * 0.73, winY + winH / 2),
            Draw.Line(houseX + houseW * 0.73 + winW, winY + winH / 2),
            Draw.StrokePath(),
        ];
    }
}
