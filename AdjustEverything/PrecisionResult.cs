namespace AdjustEverything;

internal sealed class PrecisionResult
{
    public required string Report { get; init; }

    #region 单位方差中误差

    public double Sigma0_Sq { get; init; }

    public double Sigma0 { get; init; }

    #endregion

    #region 协因数阵

    public double[,] Qxx { get; init; } = new double[0, 0];

    public double[,] Qvv { get; init; } = new double[0, 0];

    public double[,] Qll { get; init; } = new double[0, 0];

    public double[,] Qxl { get; init; } = new double[0, 0];

    #endregion

    #region 协方差阵

    public double[,] Dxx { get; init; } = new double[0, 0];

    public double[,] Dvv { get; init; } = new double[0, 0];

    public double[,] Dll { get; init; } = new double[0, 0];

    public double[,] Dxl { get; init; } = new double[0, 0];

    #endregion

}
