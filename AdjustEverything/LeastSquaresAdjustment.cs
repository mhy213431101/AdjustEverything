//最小二乘包装层 AdjustmentModel → AdjustmentResultBase
//对于线性网型只调用一次，非线性网型迭代到收敛为止
namespace AdjustEverything;

internal static class LeastSquaresAdjustment
{
    public static AdjustmentResultBase Solve(
        AdjustmentModel model)
    {
        var ls =
            LeastSquaresSolver.Solve(
                model.B,
                model.l,
                model.P);

        return new AdjustmentResultBase
        {
            Success = true,

            Report = "",

            n = ls.n,

            t = ls.t,

            r = ls.r,

            B = model.B,

            P = model.P,

            X0 = model.X0,

            xHat = ls.xHat,

            XHat =
                MatrixUtility.Add(
                    model.X0,
                    ls.xHat),

            L = model.L,

            LHat =
                MatrixUtility.Add(
                    model.L,
                    ls.V),

            V = ls.V,

            N = ls.N
        };
    }
}