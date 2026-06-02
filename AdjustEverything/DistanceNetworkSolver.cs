using System.Text;

namespace AdjustEverything;

internal sealed class DistanceAdjustmentResult
{
    public bool Success { get; init; }
    public required string Report { get; init; }
    public Dictionary<SurveyPoint, PointD> AdjustedCoordinates { get; } = [];
}

internal readonly record struct PointD(double X, double Y);

internal static class DistanceNetworkSolver
{
    private const int MaxIterations = 30;
    private const double Tolerance = 1e-8;

    // 测边网是非线性模型，需要用当前近似坐标线性化并迭代改正。
    public static DistanceAdjustmentResult Solve(AdjustmentProject project)
    {
        var report = new StringBuilder();
        report.AppendLine("测边网坐标平差");
        report.AppendLine();

        var unknownPoints = project.Points.Where(point => !point.IsCoordinateFixed).ToList();
        var fixedPoints = project.Points.Where(point => point.IsCoordinateFixed && point.X.HasValue && point.Y.HasValue).ToList();
        var observations = project.DistanceObservations.ToList();
        var parameterIndex = new Dictionary<SurveyPoint, int>();
        var coordinates = new Dictionary<SurveyPoint, PointD>();

        for (var i = 0; i < unknownPoints.Count; i++)
        {
            parameterIndex[unknownPoints[i]] = i * 2;
        }

        foreach (var point in project.Points)
        {
            // 初值优先取属性面板中的 X/Y；如果没有填写，就退回画板坐标。
            coordinates[point] = new PointD(
                point.X ?? point.CanvasLocation.X,
                point.Y ?? point.CanvasLocation.Y);
        }

        var parameterCount = unknownPoints.Count * 2;
        var iterations = 0;
        var converged = false;

        for (iterations = 1; iterations <= MaxIterations; iterations++)
        {
            // 每轮迭代都要基于最新坐标重新计算 A、l 和法方程。
            var normal = new double[parameterCount, parameterCount];
            var rhs = new double[parameterCount];

            foreach (var obs in observations)
            {
                var row = BuildDistanceRow(obs, coordinates, parameterIndex, parameterCount);
                Accumulate(normal, rhs, row.Coefficients, row.RightSide, row.Weight);
            }

            if (!TrySolveLinearSystem(normal, rhs, out var correction, out var error))
            {
                return Fail($"法方程无法求解：{error}。请检查测边网基准、连通性和观测数量。");
            }

            var maxCorrection = 0.0;
            foreach (var point in unknownPoints)
            {
                // dx/dy 是本轮坐标改正数，不是观测残差。
                var index = parameterIndex[point];
                var current = coordinates[point];
                var dx = correction[index];
                var dy = correction[index + 1];
                coordinates[point] = new PointD(current.X + dx, current.Y + dy);
                maxCorrection = Math.Max(maxCorrection, Math.Max(Math.Abs(dx), Math.Abs(dy)));
            }

            if (maxCorrection < Tolerance)
            {
                converged = true;
                break;
            }
        }

        if (!converged)
        {
            return Fail($"迭代 {MaxIterations} 次后仍未收敛。请检查初始图形位置和观测值是否矛盾过大。");
        }

        var result = new DistanceAdjustmentResult { Success = true, Report = "" };
        foreach (var point in unknownPoints)
        {
            result.AdjustedCoordinates[point] = coordinates[point];
        }

        var weightedResidualSquareSum = 0.0;
        var residuals = new List<(DistanceObservation Observation, double Computed, double Residual)>();
        foreach (var obs in observations)
        {
            var computed = ComputeDistance(obs, coordinates);
            var residual = computed - obs.Value;
            var weight = obs.Sigma <= 0 ? 1.0 : 1.0 / (obs.Sigma * obs.Sigma);
            residuals.Add((obs, computed, residual));
            weightedResidualSquareSum += weight * residual * residual;
        }

        var redundancy = observations.Count - parameterCount;
        var sigma0 = redundancy > 0 ? Math.Sqrt(weightedResidualSquareSum / redundancy) : double.NaN;

        report.AppendLine($"已知平面点数：{fixedPoints.Count}");
        report.AppendLine($"未知点数：{unknownPoints.Count}");
        report.AppendLine($"距离观测数：{observations.Count}");
        report.AppendLine($"未知参数数：{parameterCount}");
        report.AppendLine($"多余观测数：{redundancy}");
        report.AppendLine($"迭代次数：{iterations}");
        report.AppendLine(double.IsNaN(sigma0) ? "单位权中误差：无法计算" : $"单位权中误差：{sigma0:F6}");
        report.AppendLine();
        report.AppendLine("平差后坐标");

        foreach (var point in project.Points)
        {
            var coordinate = coordinates[point];
            var tag = point.IsCoordinateFixed ? "已知" : "未知";
            report.AppendLine($"{point.Name,-4} X={coordinate.X,12:F4}  Y={coordinate.Y,12:F4}  {tag}");
        }

        report.AppendLine();
        report.AppendLine("距离改正数 v");
        foreach (var item in residuals)
        {
            report.AppendLine($"{item.Observation.Name,-4} {item.Observation.From.Name}-{item.Observation.To.Name}  S0={item.Computed,12:F4}  v={item.Residual,10:F6}");
        }

        return new DistanceAdjustmentResult
        {
            Success = true,
            Report = report.ToString(),
        }.WithAdjustedCoordinates(result.AdjustedCoordinates);

        static DistanceAdjustmentResult Fail(string message)
        {
            return new DistanceAdjustmentResult
            {
                Success = false,
                Report = $"无法平差\r\n\r\n{message}",
            };
        }
    }

    private static EquationRow BuildDistanceRow(
        DistanceObservation obs,
        Dictionary<SurveyPoint, PointD> coordinates,
        Dictionary<SurveyPoint, int> parameterIndex,
        int parameterCount)
    {
        var from = coordinates[obs.From];
        var to = coordinates[obs.To];
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        if (distance < 1e-12)
        {
            distance = 1e-12;
        }

        var coefficients = new double[parameterCount];
        if (parameterIndex.TryGetValue(obs.From, out var fromIndex))
        {
            // S 对起点坐标的偏导：[-dx/S, -dy/S]。
            coefficients[fromIndex] = -dx / distance;
            coefficients[fromIndex + 1] = -dy / distance;
        }

        if (parameterIndex.TryGetValue(obs.To, out var toIndex))
        {
            // S 对终点坐标的偏导：[dx/S, dy/S]。
            coefficients[toIndex] = dx / distance;
            coefficients[toIndex + 1] = dy / distance;
        }

        // 右端项 l = 观测距离 - 当前近似距离。
        var rightSide = obs.Value - distance;
        var weight = obs.Sigma <= 0 ? 1.0 : 1.0 / (obs.Sigma * obs.Sigma);
        return new EquationRow(coefficients, rightSide, weight);
    }

    private static double ComputeDistance(DistanceObservation obs, Dictionary<SurveyPoint, PointD> coordinates)
    {
        var from = coordinates[obs.From];
        var to = coordinates[obs.To];
        return Math.Sqrt(Math.Pow(to.X - from.X, 2) + Math.Pow(to.Y - from.Y, 2));
    }

    private static void Accumulate(double[,] normal, double[] rhs, double[] coefficients, double rightSide, double weight)
    {
        // 累加 N=A'PA 和 W=A'Pl。当前版本只支持独立观测，所以 P 是对角权。
        for (var i = 0; i < rhs.Length; i++)
        {
            rhs[i] += weight * coefficients[i] * rightSide;
            for (var j = 0; j < rhs.Length; j++)
            {
                normal[i, j] += weight * coefficients[i] * coefficients[j];
            }
        }
    }

    private static DistanceAdjustmentResult WithAdjustedCoordinates(
        this DistanceAdjustmentResult target,
        Dictionary<SurveyPoint, PointD> coordinates)
    {
        foreach (var item in coordinates)
        {
            target.AdjustedCoordinates[item.Key] = item.Value;
        }

        return target;
    }

    private static bool TrySolveLinearSystem(double[,] matrix, double[] rhs, out double[] solution, out string error)
    {
        var n = rhs.Length;
        var a = (double[,])matrix.Clone();
        var b = (double[])rhs.Clone();

        for (var pivot = 0; pivot < n; pivot++)
        {
            var maxRow = pivot;
            var maxValue = Math.Abs(a[pivot, pivot]);
            for (var row = pivot + 1; row < n; row++)
            {
                var value = Math.Abs(a[row, pivot]);
                if (value > maxValue)
                {
                    maxValue = value;
                    maxRow = row;
                }
            }

            if (maxValue < 1e-12)
            {
                solution = [];
                error = "矩阵奇异";
                return false;
            }

            if (maxRow != pivot)
            {
                SwapRows(a, b, pivot, maxRow);
            }

            var pivotValue = a[pivot, pivot];
            for (var column = pivot; column < n; column++)
            {
                a[pivot, column] /= pivotValue;
            }
            b[pivot] /= pivotValue;

            for (var row = 0; row < n; row++)
            {
                if (row == pivot)
                {
                    continue;
                }

                var factor = a[row, pivot];
                if (Math.Abs(factor) < 1e-15)
                {
                    continue;
                }

                for (var column = pivot; column < n; column++)
                {
                    a[row, column] -= factor * a[pivot, column];
                }
                b[row] -= factor * b[pivot];
            }
        }

        solution = b;
        error = "";
        return true;
    }

    private static void SwapRows(double[,] matrix, double[] rhs, int first, int second)
    {
        for (var column = 0; column < rhs.Length; column++)
        {
            (matrix[first, column], matrix[second, column]) = (matrix[second, column], matrix[first, column]);
        }

        (rhs[first], rhs[second]) = (rhs[second], rhs[first]);
    }

    private sealed record EquationRow(double[] Coefficients, double RightSide, double Weight);
}
