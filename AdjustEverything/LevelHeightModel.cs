using System.Text;

namespace AdjustEverything;

internal sealed class LevelHeightModel :
    IAdjustmentModel,
    ILinearizable
{
    private readonly List<SurveyPoint> _unknownPoints;

    private readonly List<HeightObservation> _observations;

    private readonly Dictionary<SurveyPoint, int> _index;

    private readonly StringBuilder _report = new();

    public int n => _observations.Count;

    public int t => _unknownPoints.Count;

    public int separate => 0;

    public double[] X0 { get; }

    public LevelHeightModel(
        List<SurveyPoint> unknownPoints,
        List<HeightObservation> observations,
        Dictionary<SurveyPoint, int> index,
        double[] x0)
    {
        _unknownPoints = unknownPoints;

        _observations = observations;

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
        _report.Clear();

        var S = _observations.Select(o => o.Length).ToArray();

        for (int k = 0; k < n; k++)
        {
            var obs = _observations[k];

            double known = 0.0;
            double approx = 0.0;

            Add(obs.To, +1);
            Add(obs.From, -1);

            L[k] = obs.Value;

            W[k] = (obs.Value - known - approx);

            double p = obs.Sigma > 0
                ? 1.0 / (obs.Sigma * obs.Sigma * S[k])
                : 1.0 * S[k];

            P[k, k] = p;

            _report.AppendLine(
                $"{obs.Name,-6} L={obs.Value:F3} f={(known + approx):F3} v={W[k]:F3}");

            void Add(SurveyPoint pt, double c)
            {
                if (_index.TryGetValue(pt, out int j))
                {
                    B[k, j] += c;
                    approx += c * X[j];
                    return;
                }

                if (pt.Height.HasValue)
                {
                    known += c * pt.Height.Value;
                }
            }
        }
    }
}