using System.Text;

namespace AdjustEverything;

internal enum DiagnosticSeverity
{
    Info,
    Warning,
    Error,
}

internal sealed record ProjectDiagnostic(DiagnosticSeverity Severity, string Message);

internal sealed class ProjectValidationResult
{
    public List<ProjectDiagnostic> Diagnostics { get; } = [];
    public bool HasErrors => Diagnostics.Any(item => item.Severity == DiagnosticSeverity.Error);

    public string ToReport(string title = "计算前检查")
    {
        if (Diagnostics.Count == 0)
        {
            return $"{title}\r\n\r\n[信息] 检查通过，未发现明显建模问题。";
        }

        var report = new StringBuilder();
        report.AppendLine(title);
        report.AppendLine();

        foreach (var item in Diagnostics)
        {
            var prefix = item.Severity switch
            {
                DiagnosticSeverity.Error => "[错误]",
                DiagnosticSeverity.Warning => "[提醒]",
                _ => "[信息]",
            };
            report.AppendLine($"{prefix} {item.Message}");
        }

        return report.ToString();
    }
}

internal static class ProjectDiagnostics
{
    // 诊断器只负责“能不能算、哪里有问题”，不参与真正的平差计算。
    public static ProjectValidationResult ValidateHeightNetwork(AdjustmentProject project)
    {
        var result = new ProjectValidationResult();
        var points = project.Points;
        var observations = project.HeightObservations;
        var fixedPoints = points.Where(point => point.IsHeightFixed && point.Height.HasValue).ToList();
        var unknownPoints = points.Where(point => !point.IsHeightFixed).ToList();

        if (points.Count == 0)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, "当前项目还没有点。"));
            return result;
        }

        if (observations.Count == 0)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, "当前项目还没有高差观测。请使用“添加高程”连接两个点。"));
        }

        foreach (var point in points.Where(point => point.IsHeightFixed && !point.Height.HasValue))
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, $"点 {point.Name} 被设为已知高程点，但没有填写高程 H。"));
        }

        if (fixedPoints.Count == 0)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, "水准网至少需要一个已知高程点作为基准。"));
        }

        if (unknownPoints.Count == 0)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Warning, "当前没有未知高程点需要平差。"));
        }

        if (observations.Count < unknownPoints.Count)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(
                DiagnosticSeverity.Error,
                $"观测数量不足：未知高程 {unknownPoints.Count} 个，有效高差观测 {observations.Count} 条。"));
        }

        foreach (var obs in observations)
        {
            if (obs.Sigma <= 0)
            {
                result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, $"{obs.Name} 的中误差 σ 必须大于 0。"));
            }
        }

        CheckDuplicateNames(project, result, includeDistanceObservations: false);
        CheckHeightConnectivity(project, result);

        var redundancy = observations.Count - unknownPoints.Count;
        if (redundancy == 0 && !result.HasErrors)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Info, "当前水准网没有多余观测，可以解算，但无法计算单位权中误差。"));
        }
        else if (redundancy > 0 && !result.HasErrors)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Info, $"当前多余观测数为 {redundancy}。"));
        }

        return result;
    }

    public static ProjectValidationResult ValidateDistanceNetwork(AdjustmentProject project)
    {
        var result = new ProjectValidationResult();
        var points = project.Points;
        var observations = project.DistanceObservations;
        var fixedPoints = points.Where(point => point.IsCoordinateFixed && point.X.HasValue && point.Y.HasValue).ToList();
        var unknownPoints = points.Where(point => !point.IsCoordinateFixed).ToList();
        var parameterCount = unknownPoints.Count * 2;

        if (points.Count == 0)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, "当前项目还没有点。"));
            return result;
        }

        if (observations.Count == 0)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, "当前项目还没有距离观测。请使用“添加距离”连接两个点。"));
        }

        foreach (var point in points.Where(point => point.IsCoordinateFixed && (!point.X.HasValue || !point.Y.HasValue)))
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, $"点 {point.Name} 被设为已知平面点，但没有完整填写 X/Y。"));
        }

        if (fixedPoints.Count < 2)
        {
            // 当前测边网暂不做自由网，因此先要求两个已知点固定平移、旋转和尺度基准。
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, "测边网暂时要求至少两个已知平面坐标点作为基准。"));
        }

        if (unknownPoints.Count == 0)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Warning, "当前没有未知平面点需要平差。"));
        }

        if (observations.Count < parameterCount)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(
                DiagnosticSeverity.Error,
                $"观测数量不足：未知坐标参数 {parameterCount} 个，有效距离观测 {observations.Count} 条。"));
        }

        foreach (var obs in observations)
        {
            if (obs.Value <= 0)
            {
                result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, $"{obs.Name} 的距离 S 必须大于 0。"));
            }

            if (obs.Sigma <= 0)
            {
                result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, $"{obs.Name} 的中误差 σ 必须大于 0。"));
            }
        }

        CheckDuplicateNames(project, result, includeDistanceObservations: true);
        CheckDistanceConnectivity(project, result);

        var redundancy = observations.Count - parameterCount;
        if (redundancy == 0 && !result.HasErrors)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Info, "当前测边网没有多余观测，可以解算，但无法计算单位权中误差。"));
        }
        else if (redundancy > 0 && !result.HasErrors)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Info, $"当前多余观测数为 {redundancy}。"));
        }

        return result;
    }

    private static void CheckDuplicateNames(AdjustmentProject project, ProjectValidationResult result, bool includeDistanceObservations)
    {
        foreach (var name in DuplicateNames(project.Points.Select(point => point.Name)))
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Warning, $"点名 {name} 重复，建议改成唯一点名以免报告混淆。"));
        }

        foreach (var name in DuplicateNames(project.HeightObservations.Select(obs => obs.Name)))
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Warning, $"观测名 {name} 重复，建议改成唯一名称。"));
        }

        if (!includeDistanceObservations)
        {
            return;
        }

        foreach (var name in DuplicateNames(project.DistanceObservations.Select(obs => obs.Name)))
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Warning, $"观测名 {name} 重复，建议改成唯一名称。"));
        }
    }

    private static IEnumerable<string> DuplicateNames(IEnumerable<string> names)
    {
        return names
            .GroupBy(name => name.Trim(), StringComparer.CurrentCultureIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);
    }

    private static void CheckHeightConnectivity(AdjustmentProject project, ProjectValidationResult result)
    {
        CheckConnectivity(
            project,
            result,
            project.HeightObservations.Select(obs => (obs.From, obs.To)).ToList(),
            point => point.IsHeightFixed && point.Height.HasValue,
            point => !point.IsHeightFixed,
            "水准");
    }

    private static void CheckDistanceConnectivity(AdjustmentProject project, ProjectValidationResult result)
    {
        CheckConnectivity(
            project,
            result,
            project.DistanceObservations.Select(obs => (obs.From, obs.To)).ToList(),
            point => point.IsCoordinateFixed && point.X.HasValue && point.Y.HasValue,
            point => !point.IsCoordinateFixed,
            "测边");
    }

    private static void CheckConnectivity(
        AdjustmentProject project,
        ProjectValidationResult result,
        List<(SurveyPoint From, SurveyPoint To)> edges,
        Func<SurveyPoint, bool> isFixed,
        Func<SurveyPoint, bool> isUnknown,
        string networkName)
    {
        // 连通性检查按观测类型分别做；水准网只看高差边，测边网只看距离边。
        var visited = new HashSet<SurveyPoint>();

        foreach (var start in project.Points)
        {
            if (!visited.Add(start))
            {
                continue;
            }

            var component = new List<SurveyPoint>();
            var queue = new Queue<SurveyPoint>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                component.Add(current);

                foreach (var next in AdjacentPoints(edges, current))
                {
                    if (visited.Add(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }

            var componentHasObservation = edges.Any(edge =>
                component.Contains(edge.From) || component.Contains(edge.To));
            var componentHasUnknown = component.Any(isUnknown);
            var componentHasFixed = component.Any(isFixed);

            if (!componentHasObservation)
            {
                foreach (var point in component)
                {
                    result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, $"点 {point.Name} 没有参与任何{networkName}观测。"));
                }
                continue;
            }

            if (componentHasUnknown && !componentHasFixed)
            {
                var names = string.Join(", ", component.Select(point => point.Name));
                result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, $"由 {names} 构成的{networkName}子网没有已知基准点。"));
            }
        }
    }

    private static IEnumerable<SurveyPoint> AdjacentPoints(List<(SurveyPoint From, SurveyPoint To)> edges, SurveyPoint point)
    {
        foreach (var edge in edges)
        {
            if (ReferenceEquals(edge.From, point))
            {
                yield return edge.To;
            }
            else if (ReferenceEquals(edge.To, point))
            {
                yield return edge.From;
            }
        }
    }
}
