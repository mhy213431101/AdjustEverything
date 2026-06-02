using System.Text;

namespace AdjustEverything;

internal sealed class HeightAdjustmentResult
{
    public bool Success { get; init; }
    public required string Report { get; init; }
    public Dictionary<SurveyPoint, double> AdjustedHeights { get; } = [];
}

internal static class HeightNetworkSolver
{
    // 高程网是线性模型，不需要迭代。未知量是所有非已知点的 H。
    public static HeightAdjustmentResult Solve(AdjustmentProject project)
    {
        var report = new StringBuilder();
        report.AppendLine("高程网间接平差");
        report.AppendLine();

        var fixedPoints = project.Points
            .Where(point => point.IsHeightFixed && point.Height.HasValue)
            .ToList();
        var unknownPoints = project.Points
            .Where(point => !point.IsHeightFixed)
            .ToList();
        var observations = project.HeightObservations.ToList();

        if (fixedPoints.Count == 0)
        {
            return Fail("当前高程网没有已知高程点。请先使用“已知点”工具设置至少一个已知高程。");
        }

        if (unknownPoints.Count == 0)
        {
            return Fail("当前没有未知高程点需要平差。");
        }

        if (observations.Count < unknownPoints.Count)
        {
            return Fail($"观测数量不足：未知高程 {unknownPoints.Count} 个，高差观测 {observations.Count} 条。");
        }

        var index = unknownPoints
            .Select((point, i) => new { point, i })
            .ToDictionary(item => item.point, item => item.i);

        // 直接组装法方程 N x = W。这里的 x 实际就是未知点高程值。
        var normal = new double[unknownPoints.Count, unknownPoints.Count];
        var rhs = new double[unknownPoints.Count];
        var rows = new List<EquationRow>();

        foreach (var obs in observations)
        {
            var coefficients = new double[unknownPoints.Count];
            var knownContribution = 0.0;

            // 高差方程：H_To - H_From = Δh。已知点移到常数项，未知点进入系数矩阵。
            AddCoefficient(obs.To, 1.0);
            AddCoefficient(obs.From, -1.0);

            var l = obs.Value - knownContribution;
            var weight = obs.Sigma <= 0 ? 1.0 : 1.0 / (obs.Sigma * obs.Sigma);
            rows.Add(new EquationRow(obs, coefficients, l, weight));

            for (var i = 0; i < unknownPoints.Count; i++)
            {
                rhs[i] += weight * coefficients[i] * l;
                for (var j = 0; j < unknownPoints.Count; j++)
                {
                    normal[i, j] += weight * coefficients[i] * coefficients[j];
                }
            }

            void AddCoefficient(SurveyPoint point, double coefficient)
            {
                if (index.TryGetValue(point, out var parameterIndex))
                {
                    coefficients[parameterIndex] += coefficient;
                    return;
                }

                if (!point.Height.HasValue)
                {
                    return;
                }

                knownContribution += coefficient * point.Height.Value;
            }
        }

        if (!TrySolveLinearSystem(normal, rhs, out var solved, out var error))
        {
            return Fail($"法方程无法求解：{error}。请检查网是否连通、是否有足够基准。");
        }

        var result = new HeightAdjustmentResult { Success = true, Report = "" };
        for (var i = 0; i < unknownPoints.Count; i++)
        {
            result.AdjustedHeights[unknownPoints[i]] = solved[i];
        }

        var weightedResidualSquareSum = 0.0;
        var residuals = new List<(HeightObservation Observation, double Residual)>();
        foreach (var row in rows)
        {
            // 改正数 v = 平差后计算值 - 观测值。
            var computed = 0.0;
            for (var i = 0; i < unknownPoints.Count; i++)
            {
                computed += row.Coefficients[i] * solved[i];
            }

            var residual = computed - row.RightSide;
            residuals.Add((row.Observation, residual));
            weightedResidualSquareSum += row.Weight * residual * residual;
        }

        var redundancy = observations.Count - unknownPoints.Count;
        var sigma0 = redundancy > 0 ? Math.Sqrt(weightedResidualSquareSum / redundancy) : double.NaN;

        report.AppendLine($"已知点数：{fixedPoints.Count}");
        report.AppendLine($"未知点数：{unknownPoints.Count}");
        report.AppendLine($"观测数：{observations.Count}");
        report.AppendLine($"多余观测数：{redundancy}");
        report.AppendLine(double.IsNaN(sigma0) ? "单位权中误差：无法计算" : $"单位权中误差：{sigma0:F6}");
        report.AppendLine();
        report.AppendLine("平差后高程");

        foreach (var point in project.Points)
        {
            var height = point.IsHeightFixed
                ? point.Height!.Value
                : result.AdjustedHeights.GetValueOrDefault(point);
            var tag = point.IsHeightFixed ? "已知" : "未知";
            report.AppendLine($"{point.Name,-4} {height,12:F4}  {tag}");
        }

        report.AppendLine();
        report.AppendLine("高差改正数 v");
        foreach (var item in residuals)
        {
            report.AppendLine($"{item.Observation.Name,-4} {item.Observation.From.Name}->{item.Observation.To.Name}  v={item.Residual,10:F6}");
        }

        return new HeightAdjustmentResult
        {
            Success = true,
            Report = report.ToString(),
            AdjustedHeights =
            {
                // Filled below by collection initializer is not expressive enough for this case.
            }
        }.WithAdjustedHeights(result.AdjustedHeights);

        static HeightAdjustmentResult Fail(string message)
        {
            return new HeightAdjustmentResult
            {
                Success = false,
                Report = $"无法平差\r\n\r\n{message}",
            };
        }
    }

    private static HeightAdjustmentResult WithAdjustedHeights(
        this HeightAdjustmentResult target,
        Dictionary<SurveyPoint, double> heights)
    {
        foreach (var item in heights)
        {
            target.AdjustedHeights[item.Key] = item.Value;
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

    private sealed record EquationRow(
        HeightObservation Observation,
        double[] Coefficients,
        double RightSide,
        double Weight);
}
