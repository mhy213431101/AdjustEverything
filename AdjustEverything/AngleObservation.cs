namespace AdjustEverything;

/// <summary>
/// 角度观测
/// ∠ABC
/// B为测站点(Vertex)
/// </summary>
internal sealed class AngleObservation
{
    /// <summary>
    /// 观测名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 后视点A
    /// </summary>
    public required SurveyPoint From { get; init; }

    /// <summary>
    /// 测站点B
    /// </summary>
    public required SurveyPoint Vertex { get; init; }

    /// <summary>
    /// 前视点C
    /// </summary>
    public required SurveyPoint To { get; init; }

    /// <summary>
    /// 角度值(度)
    /// </summary>
    public double Value
    {
        get
        {
            var bax = From.CanvasLocation.X - Vertex.CanvasLocation.X;
            var bay = From.CanvasLocation.Y - Vertex.CanvasLocation.Y;

            var bcx = To.CanvasLocation.X - Vertex.CanvasLocation.X;
            var bcy = To.CanvasLocation.Y - Vertex.CanvasLocation.Y;

            var dot = bax * bcx + bay * bcy;

            var len1 = Math.Sqrt(
                bax * bax +
                bay * bay);

            var len2 = Math.Sqrt(
                bcx * bcx +
                bcy * bcy);

            if (len1 < 1e-6 || len2 < 1e-6)
                return 0.0;

            var cos =
                dot /
                (len1 * len2);

            cos = Math.Max(
                -1.0,
                Math.Min(
                    1.0,
                    cos));

            return
                Math.Acos(cos)
                * 180.0
                / Math.PI;
        }
    }
    public double Sigma { get; set; }

    /// <summary>
    /// 弧度值
    /// </summary>
    public double ValueRad =>
        Value * Math.PI / 180.0;

    public override string ToString()
    {
        return $"角度 {Name} = {Value:F6}°";
    }
}