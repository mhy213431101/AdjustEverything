namespace AdjustEverything;

internal sealed class LeastSquaresResult
{
    public int n { get; init; }

    public int t { get; init; }

    public int r { get; init; }

    public double[,] N { get; init; }
        = new double[0, 0];

    public double[] U { get; init; }
        = [];

    public double[] xHat { get; init; }
        = [];

    public double[] V { get; init; }
        = [];
}