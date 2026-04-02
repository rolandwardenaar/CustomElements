using DrawingService.Grpc.Protos;

namespace DrawingService.Grpc.SceneRenderers;

/// <summary>Renders a sun with a yellow disc, orange rays, and a soft glow.</summary>
internal sealed class SunRenderer : ISceneRenderer
{
    public string SceneName => "zon";

    public IReadOnlyList<DrawCommand> Render(double canvasWidth, double canvasHeight)
    {
        var cx = canvasWidth * 0.80;
        var cy = canvasHeight * 0.18;
        var discR = canvasWidth * 0.08;

        return
        [
            Draw.Clear(canvasWidth, canvasHeight),

            // Sky
            Draw.FillColor("#87CEEB"),
            Draw.Rect(0, 0, canvasWidth, canvasHeight),

            .. RenderSunAt(cx, cy, discR),
        ];
    }

    /// <summary>Renders a sun centred at (<paramref name="cx"/>, <paramref name="cy"/>).</summary>
    internal static List<DrawCommand> RenderSunAt(double cx, double cy, double discR)
    {
        var commands = new List<DrawCommand>
        {
            // Glow
            Draw.FillColor("rgba(255,255,224,0.35)"),
            Draw.Begin(),
            Draw.Circle(cx, cy, discR * 1.8),
            Draw.FillPath(),

            // Rays
            Draw.StrokeColor("#FFA500"),
            Draw.LineWidth(3),
        };

        const int rayCount = 12;
        var rayInner = discR * 1.15;
        var rayOuter = discR * 1.7;
        for (var i = 0; i < rayCount; i++)
        {
            var angle = 2 * Math.PI / rayCount * i;
            commands.Add(Draw.Begin());
            commands.Add(Draw.Move(cx + Math.Cos(angle) * rayInner, cy + Math.Sin(angle) * rayInner));
            commands.Add(Draw.Line(cx + Math.Cos(angle) * rayOuter, cy + Math.Sin(angle) * rayOuter));
            commands.Add(Draw.StrokePath());
        }

        // Disc
        commands.Add(Draw.FillColor("#FFD700"));
        commands.Add(Draw.Begin());
        commands.Add(Draw.Circle(cx, cy, discR));
        commands.Add(Draw.FillPath());

        return commands;
    }
}
