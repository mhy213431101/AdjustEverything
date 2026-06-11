using AdjustEverything;

internal sealed class AngleDistanceModel : IAdjustmentModel, ILinearizable
{
    private const double RHO = 180.0 / Math.PI * 3600.0;

    private readonly List<SurveyPoint> _points;
    private readonly List<DistanceObservation> _distances;
    private readonly List<AngleObservation> _angles;
    private readonly Dictionary<SurveyPoint, int> _index;

    public int n =>
        _distances.Count +
        _angles.Count;

    public int t =>
        _index.Count * 2;

    public double[] X0 { get; init; } = [];

    public AngleDistanceModel(
        List<SurveyPoint> unknownPoints,
        List<DistanceObservation> distances,
        List<AngleObservation> angles,
        Dictionary<SurveyPoint, int> index,
        double[] x0)
    {
        _points = unknownPoints;
        _distances = distances;
        _angles = angles;
        _index = index;
        X0 = x0;
    }

    public void Linearize(
        double[] X,
        double[,] B,
        double[] W,
        double[,] P,
        double[] L)
    {
        int row = 0;

        //-------------------------------------------------
        // 距离观测
        //-------------------------------------------------

        foreach (var o in _distances)
        {
            var from = Get(o.From, X);
            var to = Get(o.To, X);

            double dx = to.X - from.X;
            double dy = to.Y - from.Y;

            double s =
                Math.Sqrt(
                    dx * dx +
                    dy * dy);

            if (s < 1e-12)
                s = 1e-12;

            L[row] = o.Value;

            W[row] = (o.Value - s) * 1000;

            if (_index.TryGetValue(
                o.From,
                out int fi))
            {
                B[row, fi] = (-dx / s) * 1000;
                B[row, fi + 1] = (-dy / s) * 1000;
            }

            if (_index.TryGetValue(
                o.To,
                out int ti))
            {
                B[row, ti] = (dx / s) * 1000 ;
                B[row, ti + 1] = (dy / s) * 1000;
            }

            P[row, row] =
                o.Sigma > 0
                    ? 1.0 /
                      (o.Sigma * o.Sigma)
                    : 1.0;

            row++;
        }

        //-------------------------------------------------
        // 角度观测
        //-------------------------------------------------

        foreach (var o in _angles)
        {
           
            var A = Get(o.From, X);
            var Bp = Get(o.Vertex, X);
            var C = Get(o.To, X);

            double alpha =
                ComputeAngle(
                    A,
                    Bp,
                    C);

            double observed = o.ValueRad;

            L[row] = observed;

            // 角度残差归化
            double w = observed - alpha;

            while (w > Math.PI)
            {
                w -= 2.0 * Math.PI;
            }

            while (w < -Math.PI)
            {
                w += 2.0 * Math.PI;
            }

            W[row] = w * RHO;

            double h = 1e-6;
            FillDerivative(
                row,
                o,
                o.From,
                0,
                X,
                h,
                B);
            FillDerivative(
                row,
                o,
                o.From,
                1,
                X,
                h,
                B);

            FillDerivative(
                row,
                o,
                o.Vertex,
                0,
                X,
                h,
                B);

            FillDerivative(
                row,
                o,
                o.Vertex,
                1,
                X,
                h,
                B);

            FillDerivative(
                row,
                o,
                o.To,
                0,
                X,
                h,
                B);

            FillDerivative(
                row,
                o,
                o.To,
                1,
                X,
                h,
                B);

            P[row, row] =
                o.Sigma > 0
                ? 1.0 /
                  (o.Sigma * o.Sigma)
                : 1.0;

            row++;
            
        }
    }

    private void FillDerivative(
    int row,
    AngleObservation obs,
    SurveyPoint point,
    int coord,
    double[] X,
    double h,
    double[,] B)
    {
        if (!_index.TryGetValue(point, out int i))
        {
            return;
        }

        int k = i + coord;

        double original = X[k];

        X[k] = original + h;
        double f1 = AngleAtCurrentState(obs, X);

        X[k] = original - h;
        double f2 = AngleAtCurrentState(obs, X);

        X[k] = original;

        double diff = f1 - f2;

        // 防止跨越0°造成导数爆炸
        if (diff > Math.PI)
        {
            diff -= 2.0 * Math.PI;
        }

        if (diff < -Math.PI)
        {
            diff += 2.0 * Math.PI;
        }

        B[row, k] = (diff / (2.0 * h)) * RHO;
    }

    private double AngleAtCurrentState(
     AngleObservation obs,
     double[] X)
    {
        var A =
            Get(obs.From, X);

        var B =
            Get(obs.Vertex, X);

        var C =
            Get(obs.To, X);

        return ComputeAngle(
            A,
            B,
            C);
    }

    private PointD Get(
        SurveyPoint p,
        double[] X)
    {
        if (_index.TryGetValue(
            p,
            out int i))
        {
            return new PointD(
                X[i],
                X[i + 1]);
        }

        return new PointD(
            p.X!.Value,
            p.Y!.Value);
    }

    private static double ComputeAngle(
        PointD A,
        PointD B,
        PointD C)
    {
        double dx1 = A.X - B.X;
        double dy1 = A.Y - B.Y;

        double dx2 = C.X - B.X;
        double dy2 = C.Y - B.Y;

        double len1 =
            Math.Sqrt(dx1 * dx1 + dy1 * dy1);

        double len2 =
            Math.Sqrt(dx2 * dx2 + dy2 * dy2);

        if (len1 < 1e-12 ||
            len2 < 1e-12)
        {
            return 0.0;
        }

        double a1 =
            Math.Atan2(dy1, dx1);

        double a2 =
            Math.Atan2(dy2, dx2);

        double angle =
            a2 - a1;

        while (angle < 0)
        {
            angle += 2.0 * Math.PI;
        }

        while (angle >= 2.0 * Math.PI)
        {
            angle -= 2.0 * Math.PI;
        }

        return angle;
    }
    internal readonly record struct PointD(
        double X,
        double Y);
}
