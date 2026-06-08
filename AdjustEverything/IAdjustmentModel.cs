///<summary>
///接口
///间接平差输入数据
/// </summary>
namespace AdjustEverything;

internal interface IAdjustmentModel
{
    /// <param name="n">
    /// 观测值个数 n
    /// </param>
    /// <param name="t">
    /// 未知参数个数 t
    /// </param>
    /// <param name="X0">
    /// 未知参数近似值向量 X⁰
    /// </param>
    int n { get; }

    int t { get; }

    double[] X0 { get; }
}