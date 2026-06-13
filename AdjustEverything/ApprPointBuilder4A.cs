/// <summary>
/// 测角网近似坐标计算
/// 环形近似构建初始网型，近似值X0精确程度较差，后续通过高斯—牛顿迭代收敛得到平差值
/// </summar

namespace AdjustEverything;

internal static class ApprPointBuilder4A
{
    public static Dictionary<SurveyPoint, PointD> Build(
        AdjustmentProject project)
    {
        var coordinates =
            new Dictionary<SurveyPoint, PointD>();


        // 已知点
        var knownPoints =
            project.Points
                .Where(
                    p =>
                        p.X.HasValue &&
                        p.Y.HasValue)
                .ToList();

        if (knownPoints.Count == 0)
        {
            throw new InvalidOperationException(
                "测角网不存在已知坐标点。");
        }

        foreach (var point in knownPoints)
        {
            coordinates[point] =
                new PointD(
                    point.X!.Value,
                    point.Y!.Value);
        }


        // 已知点重心
        double centerX =
            knownPoints.Average(
                p => p.X!.Value);

        double centerY =
            knownPoints.Average(
                p => p.Y!.Value);


        // 估计网尺度
        double radius = 0.0;

        foreach (var point in knownPoints)
        {
            double dx =
                point.X!.Value - centerX;

            double dy =
                point.Y!.Value - centerY;

            radius =
                Math.Max(
                    radius,
                    Math.Sqrt(
                        dx * dx +
                        dy * dy));
        }

        if (radius < 1.0)
        {
            radius = 100.0;
        }


        // 未知点
        var unknownPoints =
            project.Points
                .Where(
                    p =>
                        !coordinates.ContainsKey(p))
                .ToList();

        int count =
            unknownPoints.Count;

        if (count == 0)
        {
            return coordinates;
        }


        // 环形布设
        double layoutRadius =
            radius * 0.6;

        if (layoutRadius < 20.0)
        {
            layoutRadius = 20.0;
        }

        for (int i = 0; i < count; i++)
        {
            double theta =
                2.0 *
                Math.PI *
                i /
                count;

            double x =
                centerX +
                layoutRadius *
                Math.Cos(theta);

            double y =
                centerY +
                layoutRadius *
                Math.Sin(theta);

            coordinates[
                unknownPoints[i]]
                =
                new PointD(
                    x,
                    y);
        }

        return coordinates;
    }
    internal readonly record struct PointD(double X, double Y);
}