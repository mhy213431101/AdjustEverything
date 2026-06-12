using AdjustEverything;
using System;
using System.Windows.Forms;

namespace MeasurementAdjustment
{
    public partial class FormMatrixCalculator : Form
    {
        public FormMatrixCalculator()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            BindEventHandlers();
            SetDefaultParamConditionData();
        }

        private void BindEventHandlers()
        {
            btnIndirectCalc.Click += BtnIndirectCalc_Click;
            btnConditionCalc.Click += BtnConditionCalc_Click;
            btnParamConditionCalc.Click += BtnParamConditionCalc_Click;
            btnConstrainedIndirectCalc.Click += BtnConstrainedIndirectCalc_Click;
        }

        // ==================== 间接平差计算 ====================
        private void BtnIndirectCalc_Click(object? sender, EventArgs e)
        {
            try
            {
                double[,] B = MatrixUtility.ParseMatrix(txtIndirectB.Text);
                double[,] L = MatrixUtility.ParseMatrix(txtIndirectL.Text);
                double[,] l = MatrixUtility.ParseMatrix(txtIndirectConst.Text);
                double[,] P = string.IsNullOrWhiteSpace(txtIndirectP.Text)
                    ? MatrixUtility.Identity(B.GetLength(0))
                    : MatrixUtility.ParseMatrix(txtIndirectP.Text);

                int n = B.GetLength(0);
                ValidateIndirectInput(L, l, P, n);

                double[,] Bt = MatrixUtility.Transpose(B);
                double[,] Nbb = MatrixUtility.Multiply(MatrixUtility.Multiply(Bt, P), B);
                double[,] W = MatrixUtility.Multiply(MatrixUtility.Multiply(Bt, P), l);
                double[,] x = MatrixUtility.SolveLinear(Nbb, W);
                double[,] V = MatrixUtility.AddMatrix(
                    MatrixUtility.Multiply(B, x),
                    MatrixUtility.MultiplyScalar(l, -1));
                double[,] L_hat = MatrixUtility.AddMatrix(L, V);

                txtIndirectResult.Text = FormatIndirectResult(W, Nbb, x, V, L_hat);
            }
            catch (Exception ex)
            {
                MessageBox.Show("间接平差计算错误: " + ex.Message);
            }
        }

        private void ValidateIndirectInput(double[,] L, double[,] l, double[,] P, int n)
        {
            if (L.GetLength(0) != n || L.GetLength(1) != 1)
                throw new Exception("L维度必须为 n×1");
            if (l.GetLength(0) != n || l.GetLength(1) != 1)
                throw new Exception("l维度必须为 n×1");
            if (P.GetLength(0) != n || P.GetLength(1) != n)
                throw new Exception("权阵P必须是 n×n");
        }

        private string FormatIndirectResult(double[,] W, double[,] Nbb, double[,] x, double[,] V, double[,] L_hat)
        {
            return "=== 间接平差中间量 ===\r\n"
                + "W (常数项矩阵 B^TPl):\r\n" + MatrixUtility.MatrixToString(W) + "\r\n\r\n"
                + "N (法方程系数阵 Nbb):\r\n" + MatrixUtility.MatrixToString(Nbb) + "\r\n\r\n"
                + "X (参数改正数 x):\r\n" + MatrixUtility.MatrixToString(x) + "\r\n\r\n"
                + "V (观测值改正数):\r\n" + MatrixUtility.MatrixToString(V) + "\r\n\r\n"
                + "平差值 L̂:\r\n" + MatrixUtility.MatrixToString(L_hat);
        }

        // ==================== 条件平差计算 ====================
        private void BtnConditionCalc_Click(object? sender, EventArgs e)
        {
            try
            {
                double[,] A = MatrixUtility.ParseMatrix(txtConditionA.Text);
                double[,] L = MatrixUtility.ParseMatrix(txtConditionL.Text);
                double[,] W0 = MatrixUtility.ParseMatrix(txtConditionW0.Text);
                double[,] P = string.IsNullOrWhiteSpace(txtConditionP.Text)
                    ? MatrixUtility.Identity(L.GetLength(0))
                    : MatrixUtility.ParseMatrix(txtConditionP.Text);

                int n = L.GetLength(0);
                int r = A.GetLength(0);
                ValidateConditionInput(A, W0, P, n, r);

                double[,] Q = MatrixUtility.Inverse(P);
                double[,] AQ = MatrixUtility.Multiply(A, Q);
                double[,] N = MatrixUtility.Multiply(AQ, MatrixUtility.Transpose(A));
                double[,] W = MatrixUtility.AddMatrix(MatrixUtility.Multiply(A, L), W0);
                double[,] K = MatrixUtility.SolveLinear(N, MatrixUtility.MultiplyScalar(W, -1));
                double[,] V = MatrixUtility.Multiply(MatrixUtility.Multiply(Q, MatrixUtility.Transpose(A)), K);
                double[,] L_hat = MatrixUtility.AddMatrix(L, V);

                txtConditionResult.Text = FormatConditionResult(W, N, K, V, L_hat);
            }
            catch (Exception ex)
            {
                MessageBox.Show("条件平差错误: " + ex.Message);
            }
        }

        private void ValidateConditionInput(double[,] A, double[,] W0, double[,] P, int n, int r)
        {
            if (A.GetLength(1) != n)
                throw new Exception("A列数必须等于观测值个数");
            if (W0.GetLength(0) != r || W0.GetLength(1) != 1)
                throw new Exception("W0维度r×1");
            if (P.GetLength(0) != n || P.GetLength(1) != n)
                throw new Exception("权阵P维度n×n");
        }

        private string FormatConditionResult(double[,] W, double[,] N, double[,] K, double[,] V, double[,] L_hat)
        {
            return "=== 条件平差中间量 ===\r\n"
                + "W (闭合差 AL+W0):\r\n" + MatrixUtility.MatrixToString(W) + "\r\n\r\n"
                + "N (法方程系数 A Q A^T):\r\n" + MatrixUtility.MatrixToString(N) + "\r\n\r\n"
                + "K (联系数向量):\r\n" + MatrixUtility.MatrixToString(K) + "\r\n\r\n"
                + "V (观测值改正数):\r\n" + MatrixUtility.MatrixToString(V) + "\r\n\r\n"
                + "平差值 L̂:\r\n" + MatrixUtility.MatrixToString(L_hat);
        }

        // ==================== 附有参数的条件平差计算 ====================
        private void BtnParamConditionCalc_Click(object? sender, EventArgs e)
        {
        try
        {
                // 读取输入
                double[,] A = MatrixUtility.ParseMatrix(txtParamConditionA.Text);   // r×n
                double[,] B = MatrixUtility.ParseMatrix(txtParamConditionB.Text);   // r×u
                double[,] W = MatrixUtility.ParseMatrix(txtParamConditionW.Text);   // r×1
                double[,] L = MatrixUtility.ParseMatrix(txtParamConditionL.Text);   // n×1
                double[,] P = string.IsNullOrWhiteSpace(txtParamConditionP.Text)
                    ? MatrixUtility.Identity(L.GetLength(0))
                    : MatrixUtility.ParseMatrix(txtParamConditionP.Text);           // n×n

                int n = L.GetLength(0);   // 观测值个数
                int r = A.GetLength(0);   // 条件方程个数
                int u = B.GetLength(1);   // 参数个数

                // 输入合法性检查
                if (A.GetLength(1) != n) throw new Exception("A的列数必须等于观测值个数 n");
                if (B.GetLength(0) != r) throw new Exception("B的行数必须等于条件方程个数 r");
                if (W.GetLength(0) != r || W.GetLength(1) != 1) throw new Exception("W必须是 r×1 列向量");
                if (L.GetLength(1) != 1) throw new Exception("L必须是 n×1 列向量");
                if (P.GetLength(0) != n || P.GetLength(1) != n) throw new Exception("P必须是 n×n 方阵");

                // 权逆阵 Q = P⁻¹
                double[,] Q = MatrixUtility.Inverse(P);

                // 计算 Nbb = A Q Aᵀ
                double[,] AQ = MatrixUtility.Multiply(A, Q);
                double[,] Nbb = MatrixUtility.Multiply(AQ, MatrixUtility.Transpose(A));  // r×r

                // 构建法方程系数矩阵 M = [Nbb, B; Bᵀ, 0]
                double[,] M = new double[r + u, r + u];
                for (int i = 0; i < r; i++)
                    for (int j = 0; j < r; j++)
                        M[i, j] = Nbb[i, j];
                for (int i = 0; i < r; i++)
                    for (int j = 0; j < u; j++)
                        M[i, r + j] = B[i, j];
                for (int i = 0; i < u; i++)
                    for (int j = 0; j < r; j++)
                        M[r + i, j] = B[j, i];

                // 构建右端项 RHS = [-W; 0]
                double[,] RHS = new double[r + u, 1];
                for (int i = 0; i < r; i++)
                    RHS[i, 0] = -W[i, 0];

                // 解算 [K; x]
                double[,] sol = MatrixUtility.SolveLinear(M, RHS);
                double[,] K = ExtractSubMatrix(sol, 0, r);      // r×1
                double[,] x = ExtractSubMatrix(sol, r, u);      // u×1

                // 计算改正数 V = -Q * Aᵀ * K
                double[,] AT = MatrixUtility.Transpose(A);      // n×r
                double[,] ATK = MatrixUtility.Multiply(AT, K);  // n×1
                double[,] Q_ATK = MatrixUtility.Multiply(Q, ATK); // n×1
                double[,] V = MatrixUtility.MultiplyScalar(Q_ATK, -1.0);

                // 平差值 L̂ = L + V
                double[,] L_hat = MatrixUtility.AddMatrix(L, V);

                // 
                txtParamConditionResult.Text = FormatParamConditionResult(W, M, K, x, V, L_hat);
            }
            catch (Exception ex)
            {
                MessageBox.Show("计算错误: " + ex.Message);
            }

        }
        private string FormatParamConditionResult(double[,] W, double[,] M, double[,] K, double[,] x, double[,] V, double[,] L_hat)
        {
        return "=== 附有参数的条件平差结果 ===\r\n"
        + "W (闭合差):\r\n" + MatrixUtility.MatrixToString(W) + "\r\n\r\n"
        + "法方程系数矩阵:\r\n" + MatrixUtility.MatrixToString(M) + "\r\n\r\n"
        + "K (联系数):\r\n" + MatrixUtility.MatrixToString(K) + "\r\n"
        + "x (参数改正数):\r\n" + MatrixUtility.MatrixToString(x) + "\r\n\r\n"
        + "V (观测值改正数):\r\n" + MatrixUtility.MatrixToString(V) + "\r\n\r\n"
        + "平差值 L̂:\r\n" + MatrixUtility.MatrixToString(L_hat);
        }
        private void SetDefaultParamConditionData()
        {
        // 确保控件存在（名称与设计器中的一致）
        txtParamConditionA.Text = "1,-1";
        txtParamConditionB.Text = "1";
        txtParamConditionW.Text = "0";
        txtParamConditionL.Text = "0.001;0.002";
        txtParamConditionP.Text = "";   // 留空表示使用单位权
        }
        // ==================== 附有限制条件的间接平差计算 ====================
        private void BtnConstrainedIndirectCalc_Click(object? sender, EventArgs e)
        {
            try
            {
                double[,] B = MatrixUtility.ParseMatrix(txtConstrainedIndirectB.Text);
                double[,] L = MatrixUtility.ParseMatrix(txtConstrainedIndirectL.Text);
                double[,] l = MatrixUtility.ParseMatrix(txtConstrainedIndirectConst.Text);
                double[,] C = MatrixUtility.ParseMatrix(txtConstrainedIndirectC.Text);
                double[,] Wx = MatrixUtility.ParseMatrix(txtConstrainedIndirectWx.Text);
                double[,] P = string.IsNullOrWhiteSpace(txtConstrainedIndirectP.Text)
                    ? MatrixUtility.Identity(L.GetLength(0))
                    : MatrixUtility.ParseMatrix(txtConstrainedIndirectP.Text);

                int n = B.GetLength(0), t = B.GetLength(1), c = C.GetLength(0);
                ValidateConstrainedIndirectInput(L, l, C, n, t);

                double[,] Bt = MatrixUtility.Transpose(B);
                double[,] Nbb = MatrixUtility.Multiply(MatrixUtility.Multiply(Bt, P), B);
                double[,] W = MatrixUtility.Multiply(MatrixUtility.Multiply(Bt, P), l);

                int total = t + c;
                double[,] M = BuildConstrainedIndirectMatrix(Nbb, C, t, c);
                double[,] RHS = BuildConstrainedIndirectRHS(W, Wx, t, c);
                double[,] sol = MatrixUtility.SolveLinear(M, RHS);

                double[,] x = ExtractSubMatrix(sol, 0, t);
                double[,] Kc = ExtractSubMatrix(sol, t, c);

                double[,] V = MatrixUtility.AddMatrix(
                    MatrixUtility.Multiply(B, x),
                    MatrixUtility.MultiplyScalar(l, -1));
                double[,] L_hat = MatrixUtility.AddMatrix(L, V);

                txtConstrainedIndirectResult.Text = FormatConstrainedIndirectResult(W, M, Kc, x, V, L_hat);
            }
            catch (Exception ex)
            {
                MessageBox.Show("计算错误: " + ex.Message);
            }
        }

        private void ValidateConstrainedIndirectInput(double[,] L, double[,] l, double[,] C, int n, int t)
        {
            if (L.GetLength(0) != n || L.GetLength(1) != 1) throw new Exception("L维度n×1");
            if (l.GetLength(0) != n || l.GetLength(1) != 1) throw new Exception("l维度n×1");
            if (C.GetLength(1) != t) throw new Exception("C列数应为t");
        }

        private double[,] BuildConstrainedIndirectMatrix(double[,] Nbb, double[,] C, int t, int c)
        {
            int total = t + c;
            double[,] M = new double[total, total];
            for (int i = 0; i < t; i++)
                for (int j = 0; j < t; j++)
                    M[i, j] = Nbb[i, j];

            double[,] Ct = MatrixUtility.Transpose(C);
            for (int i = 0; i < t; i++)
                for (int j = 0; j < c; j++)
                    M[i, t + j] = Ct[i, j];
            for (int i = 0; i < c; i++)
                for (int j = 0; j < t; j++)
                    M[t + i, j] = C[i, j];
            return M;
        }

        private double[,] BuildConstrainedIndirectRHS(double[,] W, double[,] Wx, int t, int c)
        {
            double[,] RHS = new double[t + c, 1];
            for (int i = 0; i < t; i++) RHS[i, 0] = W[i, 0];
            for (int i = 0; i < c; i++) RHS[t + i, 0] = -Wx[i, 0];
            return RHS;
        }

        private string FormatConstrainedIndirectResult(double[,] W, double[,] M, double[,] Kc, double[,] x, double[,] V, double[,] L_hat)
        {
            return "=== 附有限制条件的间接平差中间量 ===\r\n"
                + "W (B^TPl):\r\n" + MatrixUtility.MatrixToString(W) + "\r\n\r\n"
                + "N (法方程系数矩阵):\r\n" + MatrixUtility.MatrixToString(M) + "\r\n\r\n"
                + "K (联系数Kc):\r\n" + MatrixUtility.MatrixToString(Kc) + "\r\n"
                + "x (参数改正数):\r\n" + MatrixUtility.MatrixToString(x) + "\r\n\r\n"
                + "V (观测值改正数):\r\n" + MatrixUtility.MatrixToString(V) + "\r\n\r\n"
                + "平差值 L̂:\r\n" + MatrixUtility.MatrixToString(L_hat);
        }

        // ==================== 通用工具方法 ====================
        private double[,] ExtractSubMatrix(double[,] source, int startRow, int count)
        {
            double[,] result = new double[count, 1];
            for (int i = 0; i < count; i++)
                result[i, 0] = source[startRow + i, 0];
            return result;
        }
    }
}
