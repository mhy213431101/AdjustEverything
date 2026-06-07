using System.Text;

namespace AdjustEverything;

internal static class HeightNetworkSolver
{

    public static AdjustmentResultBase Solve(AdjustmentProject project)
    {

        var report = new StringBuilder();

        report.AppendLine("高程网间接平差");
        report.AppendLine();

        var fixedPoints = project.Points
            .Where(p => p.IsHeightFixed && p.Height.HasValue)
            .ToList();

        var unknownPoints = project.Points
            .Where(p => !p.IsHeightFixed)
            .ToList();

        var observations = project.HeightObservations.ToList();

        int t = unknownPoints.Count;
        int n = observations.Count;
        int r = n - t;

        if (fixedPoints.Count == 0)
        {
            return Fail("没有已知高程点");
        }

        if (t == 0)
        {
            return Fail("没有未知高程点");
        }

        if (n < t)
        {
            return Fail($"观测不足：n={n}, t={t}");
        }

        var index = unknownPoints
            .Select((p, i) => (p, i))
            .ToDictionary(x => x.p, x => x.i);

        var X0 = new double[t];

        for (int i = 0; i < t; i++)
        {
            var p = unknownPoints[i];
            X0[i] = p.Height ?? 0.0;
        }

        var L = observations
            .Select(o => o.Value)
            .ToArray();

        var B = new double[n, t];
        var l = new double[n];
        var P = new double[n, n];

        //权模型：P=1/(σ²·S)
        var S = new double[n];
        S = observations.Select(o => o.Length).ToArray();

        for (int k = 0; k < n; k++)
        {
            var obs = observations[k];

            double known = 0.0;
            double approx = 0.0;

            Add(obs.To, +1);
            Add(obs.From, -1);

            l[k] = obs.Value - known - approx;

            double p = obs.Sigma > 0
                ? 1.0 / (obs.Sigma * obs.Sigma * S[k])
                : 1.0 * S[k];

            P[k, k] = p;

            void Add(SurveyPoint pt, double c)
            {
                if (index.TryGetValue(pt, out int j))
                {
                    B[k, j] += c;
                    approx += c * X0[j];
                    return;
                }

                if (pt.Height.HasValue)
                {
                    known += c * pt.Height.Value;
                }
            }
        }

        var model = new AdjustmentModel
        {
            ObservationCount = n,
            ParameterCount = t,
            B = B,
            l = l,
            L = L,
            X0 = X0,
            P = P
        };

        var result = LeastSquaresAdjustment.Solve(model);

        for (int i = 0; i < t; i++)
        {
            var pt = unknownPoints[i];
            result.AdjustedHeights[pt] = result.XHat[i];
        }

        report.AppendLine($"n = {n}, t = {t}, r = {r}");
        report.AppendLine();

        report.AppendLine("未知点高程 X̂(m)");

        for (int i = 0; i < t; i++)
        {
            var pt = unknownPoints[i];

            report.AppendLine(
                $"{pt.Name,-4}  {result.XHat[i],10:F4}");
        }

        report.AppendLine();
        report.AppendLine("观测改正数 v(mm)");

        for (int i = 0; i < n; i++)
        {
            var obs = observations[i];

            report.AppendLine(
                $"{obs.Name,-4}  v={1000 * result.V[i],6:F1}");
        }

        return new AdjustmentResultBase
        {
            Success = true,
            Report = report.ToString(),

            n = n,
            t = t,
            r = r,
            P = P,

            B = B,
            X0 = result.X0,
            xHat = result.xHat,
            XHat = result.XHat,

            L = result.L,
            LHat = result.LHat,
            V = result.V,

            N = result.N
        };
    }

    private static AdjustmentResultBase Fail(string msg)
    {
        return new AdjustmentResultBase
        {
            Success = false,
            Report = "高程网平差失败：\r\n" + msg
        };
    }
}