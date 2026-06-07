/// <summary>
/// 间接平差线性化接口
/// </summary>
internal interface ILinearizable
{
    /// <param name="X">
    /// 未知参数近似值向量 X⁰
    /// </param>
    /// <param name="B">
    /// 误差方程系数矩阵 B
    /// </param>
    /// <param name="W">
    /// 误差方程常数项向量 l (或称自由项)
    /// </param>
    /// <param name="P">
    /// 观测值权阵 P
    /// </param>
    /// <param name="L">
    /// 观测值向量 L
    /// </param>
    void Linearize(
        double[] X,
        double[,] B,
        double[] W,
        double[,] P,
        double[] L);
}