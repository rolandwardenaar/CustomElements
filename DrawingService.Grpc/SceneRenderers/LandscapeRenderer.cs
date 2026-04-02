using DrawingService.Grpc.Protos;

namespace DrawingService.Grpc.SceneRenderers;

/// <summary>
/// Renders a complete landscape combining sky, grass, sun, clouds, house, tree, and car.
/// </summary>
internal sealed class LandscapeRenderer : ISceneRenderer
{
    public string SceneName => "landschap";

    public IReadOnlyList<DrawCommand> Render(double canvasWidth, double canvasHeight)
    {
        var groundY = canvasHeight * 0.75;
        var roadH = canvasHeight * 0.10;

        var commands = new List<DrawCommand>
        {
            Draw.Clear(canvasWidth, canvasHeight),

            // Sky
            Draw.FillColor("#87CEEB"),
            Draw.Rect(0, 0, canvasWidth, groundY),

            // Grass
            Draw.FillColor("#228B22"),
            Draw.Rect(0, groundY, canvasWidth, canvasHeight - groundY - roadH),

            // Road
            Draw.FillColor("#555555"),
            Draw.Rect(0, canvasHeight - roadH, canvasWidth, roadH),

            // Road centre line (dashed effect with segments)
            Draw.StrokeColor("#FFFFFF"),
            Draw.LineWidth(3),
        };

        // Dashed road line
        var dashW = canvasWidth * 0.04;
        var gapW = canvasWidth * 0.03;
        var roadCentreY = canvasHeight - roadH / 2;
        for (double dx = 0; dx < canvasWidth; dx += dashW + gapW)
        {
            commands.Add(Draw.Begin());
            commands.Add(Draw.Move(dx, roadCentreY));
            commands.Add(Draw.Line(Math.Min(dx + dashW, canvasWidth), roadCentreY));
            commands.Add(Draw.StrokePath());
        }

        // Sun (top right)
        commands.AddRange(SunRenderer.RenderSunAt(
            canvasWidth * 0.85, canvasHeight * 0.12, canvasWidth * 0.06));

        // Clouds
        var cloudR = canvasWidth * 0.045;
        commands.AddRange(CloudRenderer.RenderCloudAt(canvasWidth * 0.20, canvasHeight * 0.15, cloudR));
        commands.AddRange(CloudRenderer.RenderCloudAt(canvasWidth * 0.55, canvasHeight * 0.10, cloudR * 1.2));
        commands.AddRange(CloudRenderer.RenderCloudAt(canvasWidth * 0.70, canvasHeight * 0.22, cloudR * 0.9));

        // House (centre)
        var houseW = canvasWidth * 0.25;
        var houseH = canvasHeight * 0.22;
        var houseX = canvasWidth * 0.38;
        var houseY = groundY - houseH;
        var roofPeak = houseY - houseH * 0.4;

        commands.AddRange(
        [
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
            Draw.Rect(houseX + houseW * 0.7, roofPeak + (houseY - roofPeak) * 0.2,
                       houseW * 0.08, (houseY - roofPeak) * 0.5),

            // Door
            Draw.FillColor("#8B4513"),
            Draw.Rect(houseX + houseW * 0.42, groundY - houseH * 0.4,
                       houseW * 0.16, houseH * 0.4),

            // Door knob
            Draw.FillColor("#FFD700"),
            Draw.Begin(),
            Draw.Circle(houseX + houseW * 0.42 + houseW * 0.16 * 0.75,
                         groundY - houseH * 0.4 + houseH * 0.4 * 0.55,
                         houseW * 0.012),
            Draw.FillPath(),

            // Left window
            Draw.FillColor("#87CEEB"),
            Draw.Rect(houseX + houseW * 0.1, houseY + houseH * 0.25,
                       houseW * 0.15, houseH * 0.2),

            // Right window
            Draw.FillColor("#87CEEB"),
            Draw.Rect(houseX + houseW * 0.75, houseY + houseH * 0.25,
                       houseW * 0.15, houseH * 0.2),
        ]);

        // Tree (left of house)
        var trunkW = canvasWidth * 0.03;
        var trunkH = canvasHeight * 0.15;
        var treeCx = canvasWidth * 0.20;
        var crownR = canvasWidth * 0.07;

        commands.AddRange(
        [
            // Trunk
            Draw.FillColor("#8B4513"),
            Draw.Rect(treeCx - trunkW / 2, groundY - trunkH, trunkW, trunkH),

            // Crown layers
            Draw.FillColor("#2E8B57"),
            Draw.Begin(),
            Draw.Circle(treeCx - crownR * 0.4, groundY - trunkH - crownR * 0.2, crownR * 0.6),
            Draw.FillPath(),

            Draw.Begin(),
            Draw.Circle(treeCx + crownR * 0.4, groundY - trunkH - crownR * 0.2, crownR * 0.6),
            Draw.FillPath(),

            Draw.FillColor("#228B22"),
            Draw.Begin(),
            Draw.Circle(treeCx, groundY - trunkH - crownR * 0.5, crownR * 0.75),
            Draw.FillPath(),

            Draw.FillColor("#32CD32"),
            Draw.Begin(),
            Draw.Circle(treeCx, groundY - trunkH - crownR * 0.85, crownR * 0.4),
            Draw.FillPath(),
        ]);

        // Car (on the road)
        var carW = canvasWidth * 0.18;
        var carH = canvasHeight * 0.055;
        var carX = canvasWidth * 0.65;
        var carY = canvasHeight - roadH - carH + roadH * 0.15;
        commands.AddRange(CarRenderer.RenderCarAt(carX, carY, carW, carH));

        return commands;
    }
}
