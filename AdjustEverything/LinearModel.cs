/// <summary>
/// 线性网间接平差模型
/// </summary>
using AdjustEverything;

internal sealed class LinearModel : IAdjustmentModel, ILinearizable
{
    public required int n { get; init; }

    public required int t { get; init; }

    public required double[,] B { get; init; }

    public required double[] L { get; init; }

    public required double[,] P { get; init; }

    public required double[] X0 { get; init; }

    public string BuildReport() => "";

    public void Linearize(
        double[] X,
        double[,] BOut,
        double[] W,
        double[,] POut,
        double[] LOut)
    {

        for (int i = 0; i < n; i++)
        {
            LOut[i] = L[i];
            W[i] = L[i];

            for (int j = 0; j < t; j++)
            {
                BOut[i, j] = B[i, j];
            }

            for (int j = 0; j < n; j++)
            {
                POut[i, j] = P[i, j];
            }
        }
    }
}