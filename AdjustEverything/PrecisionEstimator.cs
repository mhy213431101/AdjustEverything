/// <summary>
/// 平差结果精度评定
/// 计算单位权方差以及协方差阵等，生成精度评定报告
/// </summary>
using System.Net.NetworkInformation;
using System.Text;

namespace AdjustEverything;

internal static class PrecisionEstimator
{
    private const double RHO = 180.0 / Math.PI * 3600.0;

    public enum NetType
    {
        LevelHeight,
        Distance,
        Angle,
        AngleDistance
    }

    public static PrecisionResult Estimate(NonlinearResult result, NetType type)
    {
        if (result.LS.r <= 0)
        {
            throw new ArgumentException(
                "多余观测数必须大于0。", nameof(result.LS.r));
        }

        // 缩放残差和设计矩阵
        var scaled = ScaleResiduals(result, type);

        // VᵀPV
        var VPV = MatrixUtility.ComputeVPV(scaled.V, result.LS.P);

        // 单位权方差 σ₀² = VᵀPV / r
        double sigma0_Sq = VPV / result.LS.r;

        // 单位权中误差 σ₀
        double sigma0 = Math.Sqrt(sigma0_Sq);

        //协因数阵
        // Qxx = Bᵀ ·P·B
        double[,] Qxx = ComputeQxx(scaled.B, result.LS.P);

        // QLL = B · Qxx · Bᵀ
        double[,] Qll = ComputeQll(scaled.B, Qxx);

        // QVV = P⁻¹ - QLL
        double[,] Qvv = ComputeQvv(result.LS.P, Qll);

        // QXL = Qxx · Bᵀ
        double[,] Qxl = ComputeQxl(Qxx, scaled.B);

        // 协方差阵
        double[,] Dxx = ComputeCovarianceMatrix(sigma0_Sq, Qxx);
        double[,] Dvv = ComputeCovarianceMatrix(sigma0_Sq, Qvv);
        double[,] Dll = ComputeCovarianceMatrix(sigma0_Sq, Qll);
        double[,] Dxl = ComputeCovarianceMatrix(sigma0_Sq, Qxl);

        var report = new StringBuilder();

        report.AppendLine();
        report.AppendLine("平差精度评定【长度单位:毫米（mm），角度单位：秒（″）】：");

        report.AppendLine();
        report.AppendLine($"单位权方差 σ0² = {sigma0_Sq:F6}");
        report.AppendLine($"单位权中误差 σ0 = {sigma0:F6}");

        report.AppendLine();
        report.AppendLine("观测值残差协方差阵 DVV：");
        for (int i = 0; i < result.LS.n; i++)
        {
            var row = new StringBuilder();
            for (int j = 0; j < result.LS.n; j++)
            {
                row.Append($"{Dvv[i, j],8:F4} ");
            }
            report.AppendLine(row.ToString());
        }

        report.AppendLine();
        report.AppendLine("观测值平差值协方差阵 DLL：");
        for (int i = 0; i < result.LS.n; i++)
        {
            var row = new StringBuilder();
            for (int j = 0; j < result.LS.n; j++)
            {
                row.Append($"{Dll[i, j],8:F4} ");
            }
            report.AppendLine(row.ToString());
        }

        report.AppendLine();
        report.AppendLine("参数协方差阵 DXX：");
        for (int i = 0; i < result.LS.t; i++)
        {
            var row = new StringBuilder();
            for (int j = 0; j < result.LS.t; j++)
            {
                row.Append($"{Dxx[i, j],8:F4} ");
            }
            report.AppendLine(row.ToString());
        }

        report.AppendLine();
        report.AppendLine("参数与观测值平差值协方差阵 DXL：");
        for (int i = 0; i < result.LS.t; i++)
        {
            var row = new StringBuilder();
            for (int j = 0; j < result.LS.n; j++)
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

    #region 缩放
    private static ScaledResiduals ScaleResiduals(NonlinearResult result, NetType type)
    {
        int n = result.LS.n;
        int t = result.LS.t;

        var scaled = new ScaledResiduals
        {
            V = new double[n],
            B = new double[n, t]
        };

        if (type == NetType.LevelHeight || type == NetType.Distance)
        {
            scaled.V = MatrixUtility.ScaMultiplyVec(result.LS.V, 1000);
            scaled.B = result.LS.B;
        }
        else if (type == NetType.Angle)
        {
            scaled.V = MatrixUtility.ScaMultiplyVec(result.LS.V, RHO);
            scaled.B = MatrixUtility.MultiplyScalar(result.LS.B, Math.Sqrt(RHO));
        }
        else if (type == NetType.AngleDistance)
        {
            for (int i = 0; i < result.Separate; i++)
            {
                scaled.V[i] = result.LS.V[i] * 1000;
                double scaledValue = result.LS.V[i] * 1000;
                for (int j = 0; j < t; j++)
                {
                    scaled.B[i, j] = result.LS.B[i, j] * 1;
                }
            }

            for (int i = result.Separate; i < n; i++)
            {
                scaled.V[i] = result.LS.V[i] * RHO;
                double scaledValue = result.LS.V[i] * RHO;
                for (int j = 0; j < t; j++)
                {
                    scaled.B[i, j] = result.LS.B[i, j] * 1;
                }
            }
        }

        return scaled;
    }
    #endregion

    #region 协因数阵计算
    private static double[,] ComputeQxx(
        double[,] B, double[,] P)
    {
        double[,] Bt = MatrixUtility.Transpose(B);
        double[,] BtP = MatrixUtility.Multiply(Bt, P);

        // N = Bᵀ · P · B
        double[,] N = MatrixUtility.Multiply(BtP, B);

        // Qxx = N⁻¹
        return MatrixUtility.Inverse(N);
    }

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
public class ScaledResiduals
{
    public required double[] V { get; set; }
    public required double[,] B { get; set; }
}