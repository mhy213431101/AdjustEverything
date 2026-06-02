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

    public string ToReport()
    {
        if (Diagnostics.Count == 0)
        {
            return "检查通过：当前高程网未发现明显建模问题。";
        }

        var report = new StringBuilder();
        report.AppendLine("计算前检查");
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

        var fixedWithoutHeight = points
            .Where(point => point.IsHeightFixed && !point.Height.HasValue)
            .ToList();
        foreach (var point in fixedWithoutHeight)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, $"点 {point.Name} 被设为已知点，但没有填写高程 H。"));
        }

        if (fixedPoints.Count == 0)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, "高程网至少需要一个已知高程点作为基准。"));
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

            if (ReferenceEquals(obs.From, obs.To))
            {
                result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, $"{obs.Name} 的起点和终点不能相同。"));
            }
        }

        var duplicatedNames = points
            .GroupBy(point => point.Name.Trim(), StringComparer.CurrentCultureIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();
        foreach (var name in duplicatedNames)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Warning, $"点名 {name} 重复，建议改成唯一点名以免报告混淆。"));
        }

        var observationNames = observations
            .GroupBy(obs => obs.Name.Trim(), StringComparer.CurrentCultureIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();
        foreach (var name in observationNames)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Warning, $"观测名 {name} 重复，建议改成唯一名称。"));
        }

        CheckConnectivity(project, result);

        var redundancy = observations.Count - unknownPoints.Count;
        if (redundancy == 0 && !result.HasErrors)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Info, "当前高程网没有多余观测，可以解算，但无法计算单位权中误差。"));
        }
        else if (redundancy > 0 && !result.HasErrors)
        {
            result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Info, $"当前多余观测数为 {redundancy}。"));
        }

        return result;
    }

    private static void CheckConnectivity(AdjustmentProject project, ProjectValidationResult result)
    {
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

                foreach (var next in AdjacentPoints(project, current))
                {
                    if (visited.Add(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }

            var componentHasObservation = project.HeightObservations.Any(obs =>
                component.Contains(obs.From) || component.Contains(obs.To));
            var componentHasUnknown = component.Any(point => !point.IsHeightFixed);
            var componentHasFixedHeight = component.Any(point => point.IsHeightFixed && point.Height.HasValue);

            if (!componentHasObservation)
            {
                foreach (var point in component)
                {
                    result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, $"点 {point.Name} 没有参与任何高差观测。"));
                }
                continue;
            }

            if (componentHasUnknown && !componentHasFixedHeight)
            {
                var names = string.Join(", ", component.Select(point => point.Name));
                result.Diagnostics.Add(new ProjectDiagnostic(DiagnosticSeverity.Error, $"由 {names} 构成的高程子网没有已知高程基准。"));
            }
        }
    }

    private static IEnumerable<SurveyPoint> AdjacentPoints(AdjustmentProject project, SurveyPoint point)
    {
        foreach (var obs in project.HeightObservations)
        {
            if (ReferenceEquals(obs.From, point))
            {
                yield return obs.To;
            }
            else if (ReferenceEquals(obs.To, point))
            {
                yield return obs.From;
            }
        }
    }
}
