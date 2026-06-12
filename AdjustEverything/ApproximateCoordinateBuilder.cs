namespace AdjustEverything;

internal static class ApproximateCoordinateBuilder
{
    public static Dictionary<SurveyPoint, PointD> Build(
        AdjustmentProject project)
    {
        var coordinates =
            new Dictionary<SurveyPoint, PointD>();

        //--------------------------------------
        // 已知点
        //--------------------------------------

        foreach (var point in project.Points)
        {
            if (!point.IsCoordinateFixed)
            {
                continue;
            }

            if (!point.X.HasValue ||
                !point.Y.HasValue)
            {
                continue;
            }

            coordinates[point] =
                new PointD(
                    point.X.Value,
                    point.Y.Value);
        }

        //--------------------------------------
        // 逐步交会未知点
        //--------------------------------------

        bool changed;

        do
        {
            changed = false;

            foreach (var point in project.Points)
            {
                if (coordinates.ContainsKey(point))
                {
                    continue;
                }

                if (TryDistanceIntersection(
                        point,
                        coordinates,
                        project.DistanceObservations,
                        out var coordinate))
                {
                    coordinates[point] = coordinate;
                    changed = true;
                }
            }

        } while (changed);

        return coordinates;
    }

    /// <summary>
    /// 距离交会
    /// </summary>
    private static bool TryDistanceIntersection(
        SurveyPoint target,
        Dictionary<SurveyPoint, PointD> coordinates,
        List<DistanceObservation> observations,
        out PointD result)
    {
        result = default;

        var circles =
            new List<(PointD Center, double Radius)>();

        foreach (var obs in observations)
        {
            SurveyPoint? other = null;

            if (ReferenceEquals(obs.From, target))
            {
                other = obs.To;
            }
            else if (ReferenceEquals(obs.To, target))
            {
                other = obs.From;
            }

            if (other is null)
            {
                continue;
            }

            if (!coordinates.TryGetValue(
                    other,
                    out var center))
            {
                continue;
            }

            circles.Add(
                (
                    center,
                    obs.Value
                ));
        }

        if (circles.Count < 2)
        {
            return false;
        }

        //--------------------------------------
        // 前两个已知圆交会
        //--------------------------------------

        var c1 = circles[0];
        var c2 = circles[1];

        if (!CircleIntersection(
                c1.Center,
                c1.Radius,
                c2.Center,
                c2.Radius,
                out var p1,
                out var p2))
        {
            return false;
        }

        //--------------------------------------
        // 利用画板位置判定双解
        //--------------------------------------

        double canvasX =
            SurveyCoordinateMapper.XFromCanvas(
                target.CanvasLocation);

        double canvasY =
            SurveyCoordinateMapper.YFromCanvas(
                target.CanvasLocation);

        double d1 =
            Math.Sqrt(
                (p1.X - canvasX) * (p1.X - canvasX)
                +
                (p1.Y - canvasY) * (p1.Y - canvasY));

        double d2 =
            Math.Sqrt(
                (p2.X - canvasX) * (p2.X - canvasX)
                +
                (p2.Y - canvasY) * (p2.Y - canvasY));

        result =
            d1 <= d2
            ? p1
            : p2;

        return true;
    }

    /// <summary>
    /// 两圆交会
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

        double dx =
            center2.X - center1.X;

        double dy =
            center2.Y - center1.Y;

        double d =
            Math.Sqrt(
                dx * dx +
                dy * dy);

        //--------------------------------------
        // 无解情况
        //--------------------------------------

        if (d < 1E-10)
        {
            return false;
        }

        if (d > radius1 + radius2)
        {
            return false;
        }

        if (d < Math.Abs(
                radius1 - radius2))
        {
            return false;
        }

        //--------------------------------------
        // 中间点
        //--------------------------------------

        double a =
            (
                radius1 * radius1
                -
                radius2 * radius2
                +
                d * d
            )
            /
            (
                2.0 * d
            );

        double h2 =
            radius1 * radius1
            -
            a * a;

        if (h2 < 0)
        {
            return false;
        }

        double h =
            Math.Sqrt(h2);

        double xm =
            center1.X
            +
            a * dx / d;

        double ym =
            center1.Y
            +
            a * dy / d;

        //--------------------------------------
        // 两个交点
        //--------------------------------------

        point1 =
            new PointD(
                xm + h * (-dy) / d,
                ym + h * dx / d);

        point2 =
            new PointD(
                xm - h * (-dy) / d,
                ym - h * dx / d);

        return true;
    }
    internal readonly record struct PointD(double X, double Y);

}