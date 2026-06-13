namespace AdjustEverything;

internal static class ApproximateCoordinateBuilder
{
    public static Dictionary<SurveyPoint, List<PointD>> Build(
        AdjustmentProject project)
    {
        var coordinates =
            new Dictionary<SurveyPoint, List<PointD>>();

        //--------------------------------------
        // 已知点（单解）
        //--------------------------------------

        foreach (var point in project.Points)
        {
            if (!point.IsCoordinateFixed)
                continue;

            if (!point.X.HasValue || !point.Y.HasValue)
                continue;

            coordinates[point] =
                new List<PointD>
                {
                    new PointD(point.X.Value, point.Y.Value)
                };
        }

        //--------------------------------------
        // 逐步传播多解
        //--------------------------------------

        bool changed;

        do
        {
            changed = false;

            foreach (var point in project.Points)
            {
                if (coordinates.ContainsKey(point))
                    continue;

                if (TryDistanceIntersectionMulti(
                        point,
                        coordinates,
                        project.DistanceObservations,
                        out var solutions))
                {
                    coordinates[point] = solutions;
                    changed = true;
                }
            }

        } while (changed);

        return coordinates;
    }

    /// <summary>
    /// 多解距离交会（关键升级点）
    /// </summary>
    private static bool TryDistanceIntersectionMulti(
        SurveyPoint target,
        Dictionary<SurveyPoint, List<PointD>> coordinates,
        List<DistanceObservation> observations,
        out List<PointD> results)
    {
        results = new List<PointD>();

        var circles =
            new List<(PointD Center, double Radius)>();

        //--------------------------------------
        // 收集所有可用观测圆
        //--------------------------------------

        foreach (var obs in observations)
        {
            SurveyPoint? other = null;

            if (ReferenceEquals(obs.From, target))
                other = obs.To;
            else if (ReferenceEquals(obs.To, target))
                other = obs.From;

            if (other is null)
                continue;

            if (!coordinates.TryGetValue(other, out var list))
                continue;

            // 传播所有候选中心
            foreach (var c in list)
            {
                circles.Add((c, obs.Value));
            }
        }

        if (circles.Count < 2)
            return false;

        //--------------------------------------
        // 任意两圆组合生成所有候选解
        //--------------------------------------

        for (int i = 0; i < circles.Count; i++)
        {
            for (int j = i + 1; j < circles.Count; j++)
            {
                var c1 = circles[i];
                var c2 = circles[j];

                if (!CircleIntersection(
                        c1.Center,
                        c1.Radius,
                        c2.Center,
                        c2.Radius,
                        out var p1,
                        out var p2))
                {
                    continue;
                }

                results.Add(p1);
                results.Add(p2);
            }
        }

        if (results.Count == 0)
            return false;

        //--------------------------------------
        // 去重
        //--------------------------------------

        results =
            results
                .Distinct(new PointComparer())
                .ToList();

        return results.Count > 0;
    }

    /// <summary>
    /// 两圆相交
    /// </summary>
    private static bool CircleIntersection(
        PointD center1,
        double radius1,
        PointD center2,
        double radius2,
        out PointD point1,
        out PointD point2)
    {
        point1 = default;
        point2 = default;

        double dx = center2.X - center1.X;
        double dy = center2.Y - center1.Y;

        double d = Math.Sqrt(dx * dx + dy * dy);

        if (d < 1E-10)
            return false;

        if (d > radius1 + radius2)
            return false;

        if (d < Math.Abs(radius1 - radius2))
            return false;

        double a =
            (radius1 * radius1 -
             radius2 * radius2 +
             d * d) / (2.0 * d);

        double h2 =
            radius1 * radius1 - a * a;

        if (h2 < 0)
            return false;

        double h = Math.Sqrt(h2);

        double xm = center1.X + a * dx / d;
        double ym = center1.Y + a * dy / d;

        point1 =
            new PointD(
                xm - h * (-dy) / d,
                ym - h * dx / d);

        point2 =
            new PointD(
                xm + h * (-dy) / d,
                ym + h * dx / d);

        return true;
    }

    /// <summary>
    /// 去重
    /// </summary>
    private sealed class PointComparer : IEqualityComparer<PointD>
    {
        public bool Equals(PointD a, PointD b)
        {
            return Math.Abs(a.X - b.X) < 1e-8 &&
                   Math.Abs(a.Y - b.Y) < 1e-8;
        }

        public int GetHashCode(PointD p)
        {
            return HashCode.Combine(
                (int)(p.X * 1e6),
                (int)(p.Y * 1e6));
        }
    }
    public static List<double[]> BuildInitialSolutions(
        AdjustmentProject project,
        List<SurveyPoint> unknownPoints,
        Dictionary<SurveyPoint, List<PointD>> candidatePoints)
    {
        // candidatePoints:
        // 每个点 → 多个几何候选解

        var perPointCandidates =
            new List<List<PointD>>();

        foreach (var p in unknownPoints)
        {
            if (!candidatePoints.TryGetValue(p, out var list) ||
                list.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Point {p} has no candidate solution.");
            }

            perPointCandidates.Add(list);
        }

        // 生成全组合初值
        var solutions = GenerateInitialSolutions(perPointCandidates);

        return solutions;
    }

    /// <summary>
    /// 生成所有组合初值（核心）
    /// </summary>
    private static List<double[]> GenerateInitialSolutions(
        List<List<PointD>> perPointCandidates)
    {
        IEnumerable<List<PointD>> result = new List<List<PointD>> { new List<PointD>() };

        foreach (var candidates in perPointCandidates)
        {
            result =
                from acc in result
                from item in candidates
                select new List<PointD>(acc) { item };
        }

        var output =
            new List<double[]>();

        foreach (var combo in result)
        {
            var x0 = new double[combo.Count * 2];

            for (int i = 0; i < combo.Count; i++)
            {
                x0[i * 2] = combo[i].X;
                x0[i * 2 + 1] = combo[i].Y;
            }

            output.Add(x0);
        }

        return output;
    }
    internal readonly record struct PointD(double X, double Y);
}