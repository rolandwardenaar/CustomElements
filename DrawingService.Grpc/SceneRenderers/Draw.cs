using DrawingService.Grpc.Protos;

namespace DrawingService.Grpc.SceneRenderers;

/// <summary>
/// Factory methods for creating <see cref="DrawCommand"/> messages,
/// keeping renderer code concise and readable.
/// </summary>
internal static class Draw
{
    internal static DrawCommand Clear(double w, double h) => new() { ClearCanvas = new ClearCanvas { Width = w, Height = h } };
    internal static DrawCommand FillColor(string c) => new() { SetColor = new SetColor { Color = c } };
    internal static DrawCommand StrokeColor(string c) => new() { SetStrokeColor = new SetStrokeColor { Color = c } };
    internal static DrawCommand LineWidth(double w) => new() { SetLineWidth = new SetLineWidth { Width = w } };
    internal static DrawCommand Rect(double x, double y, double w, double h) => new() { FillRect = new FillRect { X = x, Y = y, Width = w, Height = h } };
    internal static DrawCommand Begin() => new() { BeginPath = new BeginPath() };
    internal static DrawCommand Close() => new() { ClosePath = new ClosePath() };
    internal static DrawCommand Move(double x, double y) => new() { MoveTo = new MoveTo { X = x, Y = y } };
    internal static DrawCommand Line(double x, double y) => new() { LineTo = new LineTo { X = x, Y = y } };
    internal static DrawCommand FillPath() => new() { Fill = new Fill() };
    internal static DrawCommand StrokePath() => new() { Stroke = new Stroke() };
    internal static DrawCommand Circle(double x, double y, double r) => new() { Arc = new Arc { X = x, Y = y, Radius = r, StartAngle = 0, EndAngle = 2 * Math.PI } };
    internal static DrawCommand ArcCmd(double x, double y, double r, double startAngle, double endAngle) => new() { Arc = new Arc { X = x, Y = y, Radius = r, StartAngle = startAngle, EndAngle = endAngle } };
    internal static DrawCommand QuadCurve(double cpx, double cpy, double x, double y) => new() { QuadraticCurveTo = new QuadraticCurveTo { Cpx = cpx, Cpy = cpy, X = x, Y = y } };
    internal static DrawCommand BezCurve(double cp1X, double cp1Y, double cp2X, double cp2Y, double x, double y) => new() { BezierCurveTo = new BezierCurveTo { Cp1X = cp1X, Cp1Y = cp1Y, Cp2X = cp2X, Cp2Y = cp2Y, X = x, Y = y } };
}
