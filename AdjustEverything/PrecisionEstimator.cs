//最小二乘精度评定
using System.Text;

namespace AdjustEverything;

internal static class PrecisionEstimator
{
    public static PrecisionResult Estimate(LeastSquaresResult result)
    {       
        if (result.r <= 0)
        {
            throw new ArgumentException(
                "多余观测数必须大于0。", nameof(result.r));
        }

        //统一单位为mm
        double[] Vmm = MatrixUtility.ScaMultiplyVec(result.V,1000);

        // VᵀPV
        var VPV = MatrixUtility.ComputeVPV(Vmm, result.P);

        // 单位权方差 σ₀² = VᵀPV / r
        double sigma0_Sq = VPV / result.r;

        // 单位权中误差 σ₀
        double sigma0 = Math.Sqrt(sigma0_Sq);

        //协因数阵
        // QXX = N⁻¹
        var Qxx = MatrixUtility.Inverse(result.N);

        // QLL = B · Qxx · Bᵀ
        double[,] Qll = ComputeQll(result.B, Qxx);
        
        // QVV = P⁻¹ - QLL
        double[,] Qvv = ComputeQvv(result.P, Qll);

        // QXL = Qxx · Bᵀ
        double[,] Qxl = ComputeQxl(Qxx, result.B);

        // 协方差阵
        double[,] Dxx = ComputeCovarianceMatrix(sigma0_Sq, Qxx);
        double[,] Dvv = ComputeCovarianceMatrix(sigma0_Sq, Qvv);
        double[,] Dll = ComputeCovarianceMatrix(sigma0_Sq, Qll);
        double[,] Dxl = ComputeCovarianceMatrix(sigma0_Sq, Qxl);

        var report = new StringBuilder();

        report.AppendLine();
        report.AppendLine("平差精度评定：");

        report.AppendLine();
        report.AppendLine($"单位权方差 σ0² = {sigma0_Sq:F6} mm²");
        report.AppendLine($"单位权中误差 σ0 = {sigma0:F6} mm");

        report.AppendLine();
        report.AppendLine("观测值残差协方差阵 DVV（mm²）：");
        for (int i = 0; i < result.n; i++)
        {
            var row = new System.Text.StringBuilder();
            for (int j = 0; j < result.n; j++)
            {
                row.Append($"{Dvv[i, j],8:F4} ");
            }
            report.AppendLine(row.ToString());
        }

        report.AppendLine();
        report.AppendLine("观测值平差值协方差阵 DLL（mm²）：");
        for (int i = 0; i < result.n; i++)
        {
            var row = new System.Text.StringBuilder();
            for (int j = 0; j < result.n; j++)
            {
                row.Append($"{Dll[i, j],8:F4} ");
            }
            report.AppendLine(row.ToString());
        }

        report.AppendLine();
        report.AppendLine("参数协方差阵 DXX（mm²）：");
        for (int i = 0; i < result.t; i++)
        {
            var row = new System.Text.StringBuilder();
            for (int j = 0; j < result.t; j++)
            {
                row.Append($"{Dxx[i, j],8:F4} ");
            }
            report.AppendLine(row.ToString());
        }

        report.AppendLine();
        report.AppendLine("参数与观测值平差值协方差阵 DXL（mm²）：");
        for (int i = 0; i < result.t; i++)
        {
            var row = new System.Text.StringBuilder();
            for (int j = 0; j < result.n; j++)
            {
                row.Append($"{Dxl[i, j],8:F4} ");
            }
            report.AppendLine(row.ToString());
        }

        return new PrecisionResult
        {
            Report = report.ToString(),
            Sigma0_Sq = sigma0_Sq,
            Sigma0 = sigma0,            
            Qvv = Qvv,
            Qll = Qll,
            Qxx = Qxx,
            Qxl = Qxl,           
            Dvv = Dvv,
            Dll = Dll,
            Dxx = Dxx,
            Dxl = Dxl,
        };
    }

    #region 协因数阵计算

    private static double[,] ComputeQll(
        double[,] B,
        double[,] Qxx)
    {

        double[,] BQxx = MatrixUtility.Multiply(B, Qxx);
        double[,] Bt = MatrixUtility.Transpose(B);

        // QLL = B · Qxx · Bᵀ
        return MatrixUtility.Multiply(BQxx, Bt);
    }

    private static double[,] ComputeQvv(
        double[,] P,
        double[,] Qll)
    {
        // P⁻¹
        double[,] PInv = MatrixUtility.Inverse(P);

        // QVV = P⁻¹ - QLL
        return MatrixUtility.SubtractMatrix(PInv, Qll);
    }

    private static double[,] ComputeQxl(
        double[,] Qxx,
        double[,] B)
    {
        // Bᵀ
        double[,] Bt = MatrixUtility.Transpose(B);

        // QXL = Qxx · Bᵀ
        return MatrixUtility.Multiply(Qxx, Bt);
    }

    #endregion

    #region 协方差阵计算

    private static double[,] ComputeCovarianceMatrix(
        double sigma0_Sq,
        double[,] Q)
    {
        return MatrixUtility.MultiplyScalar(Q, sigma0_Sq);
    }

    #endregion
}