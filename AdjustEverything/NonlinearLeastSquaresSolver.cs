using AdjustEverything;
using System.Text;
using static DistanceModel;

internal static class NonlinearLeastSquaresSolver
{
    private const int MaxIterations = 30;
    private const double Tol = 1e-8;

    public static NonlinearResult Solve(
        IAdjustmentModel model,
        ILinearizable linearizer)
    {
        int n = model.n;
        int t = model.t;

        double[] X = (double[])model.X0.Clone();

        LeastSquaresResult last = null!;

        var report = new StringBuilder();


        for (int iter = 0; iter < MaxIterations; iter++)
        {
            var B = new double[n, t];
            var W = new double[n];
            var L = new double[n];
            var P = new double[n, n];

            linearizer.Linearize(
                X,
                B,
                W,
                P,
                L);

            last =
                LeastSquaresSolver.Solve(
                    B,
                    W,
                    P);

            double max = 0;

            for (int i = 0; i < t; i++)
            {
                X[i] += last.xHat[i];

                max = Math.Max(
                    max,
                    Math.Abs(last.xHat[i]));
            }

            if (max < Tol)
            {
                report.AppendLine("收敛成功");

                return new NonlinearResult
                {
                    Success = true,
                    XHat = X,
                    Iterations = iter + 1,
                    LS = last,
                    Report = report.ToString()
                };
            }
        }

        report.AppendLine($"未在 {MaxIterations} 次迭代内收敛");

        return new NonlinearResult
        {
            Success = false,
            XHat = X,
            Iterations = MaxIterations,
            LS = last,
            Report = report.ToString()
        };
    }
}