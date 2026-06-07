//通用间接平差模型 V = B * X - L
namespace AdjustEverything;

internal sealed class AdjustmentModel
{
    public required int ObservationCount { get; init; } // 观测数 n

    public required int ParameterCount { get; init; } // 未知数个数 t

    public required double[,] B { get; init; } // 参数矩阵 B

    public required double[] l { get; init; } // 常数项向量 l

    public required double[] L { get; init; } // 观测值向量 L

    public required double[] X0 { get; init; } // 未知数近似值向量 X0

    public required double[,] P { get; init; } // 权阵 P
}