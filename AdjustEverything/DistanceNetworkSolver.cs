namespace AdjustEverything;

internal sealed class DistanceNetworkModel : INonlinearObservationModel
{
    private readonly List<SurveyPoint> _points;
    private readonly List<DistanceObservation> _observations;
    private readonly Dictionary<SurveyPoint, int> _index;

    public int ParameterCount => _index.Count * 2;

    public int ObservationCount => _observations.Count;

    public DistanceNetworkModel(
        List<SurveyPoint> points,
        List<DistanceObservation> observations,
        Dictionary<SurveyPoint, int> index)
    {
        _points = points;
        _observations = observations;
        _index = index;
    }

    public void Evaluate(
        double[] X,
        double[] L,
        double[,] B,
        double[] w,
        double[,] P)
    {
        int n = _observations.Count;

        for (int i = 0; i < n; i++)
        {
            var obs = _observations[i];

            var from = GetCoordinate(obs.From, X);
            var to = GetCoordinate(obs.To, X);

            double dx = to.X - from.X;
            double dy = to.Y - from.Y;

            double s = Math.Sqrt(dx * dx + dy * dy);

            if (s < 1e-12)
                s = 1e-12;

            // 观测值
            L[i] = obs.Value;

            // 计算值（距离）
            double computed = s;

            // 残差（v = L - f(X)）
            w[i] = obs.Value - computed;

            // ===== B矩阵（雅可比） =====

            if (_index.TryGetValue(obs.From, out int fi))
            {
                B[i, fi] = -dx / s;
                B[i, fi + 1] = -dy / s;
            }

            if (_index.TryGetValue(obs.To, out int ti))
            {
                B[i, ti] = dx / s;
                B[i, ti + 1] = dy / s;
            }

            // ===== 权阵 =====
            double p = obs.Sigma > 0
                ? 1.0 / (obs.Sigma * obs.Sigma)
                : 1.0;

            P[i, i] = p;
        }
    }

    private PointD GetCoordinate(SurveyPoint p, double[] X)
    {
        if (_index.TryGetValue(p, out int i))
            return new PointD(X[i], X[i + 1]);

        return new PointD(p.X!.Value, p.Y!.Value);
    }


        report.AppendLine($"迭代次数：{iterations}");

        report.AppendLine();

        report.AppendLine("平差后坐标");

        foreach (var point in project.Points)
        {
            PointD coordinate;

            if (point.IsCoordinateFixed)
            {
                coordinate =
                    new PointD(point.X!.Value, point.Y!.Value);
            }
            else
            {
                coordinate = adjustedCoordinates[point];
            }

            report.AppendLine(
                $"{point.Name,-4} " +
                $"X={coordinate.X,12:F4} " +
                $"Y={coordinate.Y,12:F4}");
        }

        report.AppendLine();
        report.AppendLine("观测改正数 v(mm)");

        for (int i = 0; i < n; i++)
        {
            var obs = observations[i];

            report.AppendLine(
                $"{obs.Name,-4}  v={1000 * lastLs.V[i],6:F1}");
        }

        var result = new AdjustmentResultBase
        {
            Success = true,

            Report = report.ToString(),

            n = n,

            t = t,

            r = r,

            B = lastB,

            P = lastP,

            X0 = [],

            xHat = lastLs!.xHat,

            XHat = (double[])X0.Clone(),

            L = lastL,

            LHat = LHat,

            V = lastLs.V,

            N = lastLs.N
        };

        foreach (var item in adjustedCoordinates)
        {
            result.AdjustedCoordinates[item.Key] = item.Value;
        }

        return result;
    }

    private static PointD GetCoordinate(SurveyPoint point, double[] X0,
        Dictionary<SurveyPoint, int> parameterIndex)
    {
        if (
            parameterIndex.TryGetValue(point, out int index))
        {
            return new PointD(X0[index], X0[index + 1]);
        }

        return new PointD(point.X!.Value, point.Y!.Value);
    }

    private static PointD GetFinalCoordinate(
        SurveyPoint point,
        Dictionary<SurveyPoint, PointD> adjustedCoordinates)
    {
        if (
            adjustedCoordinates.TryGetValue(point, out var coordinate))
        {
            return coordinate;
        }

        return new PointD(point.X!.Value, point.Y!.Value);
    }

    private static AdjustmentResultBase Fail(string message)
    {
        return new AdjustmentResultBase
        {
            Success = false,

            Report = "测边网平差失败\r\n\r\n" + message
        };
    }
}