/// <summary>
/// 测边网模型建模
/// 提供 f(x)、B、P
/// </summary>

using AdjustEverything;
using System.Text;

internal sealed class DistanceModel : IAdjustmentModel, ILinearizable
{
    private readonly List<SurveyPoint> _points;
    private readonly List<DistanceObservation> _obs;
    private readonly Dictionary<SurveyPoint, int> _index;

    public int n => _obs.Count;

    public int t => _index.Count * 2;

    public double[] X0 { get; init; } = [];

    private readonly StringBuilder _report = new();

    public DistanceModel(
    List<SurveyPoint> unknownPoints,
    List<DistanceObservation> observations,
    Dictionary<SurveyPoint, int> index,
    double[] x0)
    {
        _points = unknownPoints;
        _obs = observations;
        _index = index;
        X0 = x0;
    }

    public void Linearize(
        double[] X,
        double[,] B,
        double[] W,
        double[,] P,
        double[] L)
    {
        int n = _obs.Count;

        for (int i = 0; i < n; i++)
        {
            var o = _obs[i];

            var from = Get(o.From, X);
            var to = Get(o.To, X);

            double dx = to.X - from.X;
            double dy = to.Y - from.Y;

            double s = Math.Sqrt(dx * dx + dy * dy);
            if (s < 1e-12) s = 1e-12;

            L[i] = (o.Value) * 1000;

            double f = s;

            W[i] = (o.Value - f) * 1000;


            if (_index.TryGetValue(o.From, out int fi))
            {
                B[i, fi] = (-dx / s) * 1000;
                B[i, fi + 1] = (-dy / s) * 1000;
            }

            if (_index.TryGetValue(o.To, out int ti))
            {
                B[i, ti] = (dx / s) * 1000;
                B[i, ti + 1] = (dy / s) * 1000;
            }

            double p = o.Sigma > 0 ? 1.0 / (o.Sigma * o.Sigma) : 1.0;
            P[i, i] = p;
        }
    }

    private PointD Get(SurveyPoint p, double[] X)
    {
        if (_index.TryGetValue(p, out int i))
            return new PointD(X[i], X[i + 1]);

        return new PointD(p.X!.Value, p.Y!.Value);
    }

    internal readonly record struct PointD(double X, double Y);
}