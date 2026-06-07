using System.Text;

namespace AdjustEverything;

internal readonly record struct PointD(double X, double Y);

internal static class DistanceNetworkSolver
{
    private const int MaxIterations = 30;

    private const double Tolerance = 1E-8;

    public static AdjustmentResultBase Solve(AdjustmentProject project)
    {
        var report = new StringBuilder();

        report.AppendLine("测边网坐标间接平差");
        report.AppendLine();

        var fixedPoints =
            project.Points
                .Where(p =>
                    p.IsCoordinateFixed &&
                    p.X.HasValue &&
                    p.Y.HasValue)
                .ToList();

        var unknownPoints =
            project.Points
                .Where(p => !p.IsCoordinateFixed)
                .ToList();

        var observations =
            project.DistanceObservations
                .ToList();

        int n = observations.Count;

        int t = unknownPoints.Count * 2;

        int r = n - t;

        if (fixedPoints.Count == 0)
        {
            return Fail("没有已知坐标点");
        }

        if (unknownPoints.Count == 0)
        {
            return Fail("没有未知坐标点");
        }

        if (n < t)
        {
            return Fail($"观测不足：n={n}, t={t}");
        }

        var parameterIndex =
            new Dictionary<SurveyPoint, int>();

        for (int i = 0; i < unknownPoints.Count; i++)
        {
            parameterIndex[unknownPoints[i]] = 2 * i;
        }

        var X0 =
            new double[t];

        for (int i = 0; i < unknownPoints.Count; i++)
        {
            var point = unknownPoints[i];

            X0[2 * i]
                = point.X
                ?? point.CanvasLocation.X;

            X0[2 * i + 1]
                = point.Y
                ?? point.CanvasLocation.Y;
        }

        // 保存最终迭代结果
        LeastSquaresResult? lastLs = null;

        double[,] lastB = new double[0, 0];

        double[,] lastP = new double[0, 0];

        double[] lastL = [];

        int iterations = 0;

        bool converged = false;

        // 迭代
        for (
            iterations = 1;
            iterations <= MaxIterations;
            iterations++)
        {
            var B = new double[n, t];

            var l = new double[n];

            var L = new double[n];

            var P = new double[n, n];

            for (int row = 0; row < n; row++)
            {
                var obs = observations[row];

                L[row] = obs.Value;

                var from =
                    GetCoordinate(
                        obs.From,
                        X0,
                        parameterIndex);

                var to =
                    GetCoordinate(
                        obs.To,
                        X0,
                        parameterIndex);

                double dx = to.X - from.X;

                double dy = to.Y - from.Y;

                double s = Math.Sqrt(dx * dx + dy * dy);

                if (s < 1E-12)
                {
                    s = 1E-12;
                }

                l[row] = obs.Value - s;

                if (
                    parameterIndex.TryGetValue(
                        obs.From,
                        out int fromIndex))
                {
                    B[row, fromIndex] = -dx / s;
                    B[row, fromIndex + 1] = -dy / s;
                }

                if (
                    parameterIndex.TryGetValue(
                        obs.To,
                        out int toIndex))
                {
                    B[row, toIndex] = dx / s;
                    B[row, toIndex + 1] = dy / s;
                }

                double weight =
                    obs.Sigma > 0
                    ? 1.0 /
                      (obs.Sigma * obs.Sigma)
                    : 1.0;

                P[row, row] = weight;
            }

            var ls = LeastSquaresSolver.Solve(B, l, P);

            lastLs = ls;

            lastB = B;

            lastP = P;

            lastL = L;

            double maxCorrection = 0;

            for (int i = 0; i < t; i++)
            {
                X0[i] += ls.xHat[i];

                maxCorrection =
                    Math.Max(
                        maxCorrection,
                        Math.Abs(
                            ls.xHat[i]));
            }

            if (maxCorrection < Tolerance)
            {
                converged = true;
                break;
            }
        }

        if (!converged)
        {
            return Fail(
                $"迭代 {MaxIterations} 次后未收敛");
        }

        // 最终坐标
        var adjustedCoordinates =
            new Dictionary<SurveyPoint, PointD>();

        for (int i = 0; i < unknownPoints.Count; i++)
        {
            adjustedCoordinates[unknownPoints[i]] =
                new PointD(
                    X0[2 * i],
                    X0[2 * i + 1]);
        }

        var LHat = new double[n];

        for (int i = 0; i < n; i++)
        {
            var obs = observations[i];

            PointD from =
                GetFinalCoordinate(
                    obs.From,
                    adjustedCoordinates);

            PointD to =
                GetFinalCoordinate(
                    obs.To,
                    adjustedCoordinates);

            LHat[i] =
                Math.Sqrt(
                    Math.Pow(to.X - from.X, 2) +
                    Math.Pow(to.Y - from.Y, 2));
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