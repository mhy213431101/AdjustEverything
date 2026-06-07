//平差结果类
namespace AdjustEverything;

internal sealed class AdjustmentResultBase
{
    public bool Success { get; init; }

    public required string Report { get; init; }

    public Dictionary<SurveyPoint, double> AdjustedHeights { get; } = [];

    public Dictionary<SurveyPoint, PointD> AdjustedCoordinates { get; } = [];

    #region 基本统计量

    public int n { get; init; }

    public int t { get; init; }

    public int r { get; init; }

    public double[,] P { get; init; } = new double[0, 0];
    #endregion

    #region 参数与观测值

    public double[,] B { get; init; } = new double[0, 0];

    public double[] X0 { get; init; } = [];

    public double[] xHat { get; init; } = [];

    public double[] XHat { get; init; } = [];

    public double[] L { get; init; } = [];

    public double[] LHat { get; init; } = [];

    public double[] V { get; init; } = [];

    #endregion

    #region 法方程

    public double[,] N { get; init; } = new double[0, 0];

    #endregion
}