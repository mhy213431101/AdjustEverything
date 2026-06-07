//最小二乘运算核心
namespace AdjustEverything;

internal static class LeastSquaresSolver
{
    public static LeastSquaresResult Solve(
        double[,] B,
        double[] l,
        double[,] P)
    {
        int n =
            B.GetLength(0);

        int t =
            B.GetLength(1);

        int r =
            n - t;

        var Bt =
            MatrixUtility.Transpose(B);

        var N =
            MatrixUtility.Multiply(
                MatrixUtility.Multiply(
                    Bt,
                    P),
                B);


        var U =
            MatrixUtility.Multiply(
                MatrixUtility.Multiply(
                    Bt,
                    P),
                l);


        var xHat =
            MatrixUtility.Solve(
                N,
                U);


        var V =
            MatrixUtility.Subtract(
                MatrixUtility.Multiply(
                    B,
                    xHat),
                l);

        return new LeastSquaresResult
        {
            n = n,

            t = t,

            r = r,

            N = N,

            U = U,

            xHat = xHat,

            V = V
        };
    }
}
