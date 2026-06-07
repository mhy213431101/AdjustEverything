/// <summary>
/// 非线性网平差结果
/// </summary>
using AdjustEverything;

internal sealed class NonlinearResult
{
    public bool Success { get; set; }

    public int Iterations { get; set; }

    public double[] XHat { get; set; } = [];

    public LeastSquaresResult LS { get; set; } = default!;

    public string Report { get; set; } = "";
}

  