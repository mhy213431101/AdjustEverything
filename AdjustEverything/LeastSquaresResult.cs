/// <summary>
/// 结果对象
/// 单次最小二乘运算结果
/// </summary>
namespace AdjustEverything;

internal sealed class LeastSquaresResult
{
    public int n { get; init; }

    public int t { get; init; }

    public int r { get; init; }

    public double[,] N { get; init; } = new double[0, 0];

    public double[] U { get; init; } = [];

    public double[] xHat { get; init; } = [];

    public double[] V { get; init; } = [];

    public double[,] B { get; init; } = new double [0, 0];

    public double[,] P { get; init; } = new double[0, 0];

    public double[] W { get; init; } = [];
}