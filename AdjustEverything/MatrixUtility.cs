/// <summary>
/// 矩阵计算工具
/// </summary>
namespace AdjustEverything;

internal static class MatrixUtility
{
    #region 基本信息

    public static int RowCount(double[,] matrix)
    {
        return matrix.GetLength(0);
    }

    public static int ColumnCount(double[,] matrix)
    {
        return matrix.GetLength(1);
    }

    #endregion

    #region 转置

    public static double[,] Transpose(double[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        var result = new double[cols, rows];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[j, i] = matrix[i, j];
            }
        }

        return result;
    }

    #endregion

    #region 矩阵乘矩阵

    public static double[,] Multiply(
        double[,] left,
        double[,] right)
    {
        int m = left.GetLength(0);
        int n = left.GetLength(1);
        int p = right.GetLength(1);

        if (n != right.GetLength(0))
        {
            throw new InvalidOperationException(
                "矩阵维数不匹配。");
        }

        var result = new double[m, p];

        for (int i = 0; i < m; i++)
        {
            for (int k = 0; k < p; k++)
            {
                double sum = 0.0;

                for (int j = 0; j < n; j++)
                {
                    sum += left[i, j] * right[j, k];
                }

                result[i, k] = sum;
            }
        }

        return result;
    }

    #endregion

    #region 一维数组转对角矩阵
    public static double[,] ArrayToDiagMatrix(double[] array)
    {
        int n = array.Length;
        double[,] matrix = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            matrix[i, i] = array[i];
        }

        return matrix;
    }

    #endregion

    #region 矩阵乘向量

    public static double[] Multiply(
        double[,] matrix,
        double[] vector)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        if (cols != vector.Length)
        {
            throw new InvalidOperationException(
                "矩阵与向量维数不匹配。");
        }

        var result = new double[rows];

        for (int i = 0; i < rows; i++)
        {
            double sum = 0.0;

            for (int j = 0; j < cols; j++)
            {
                sum += matrix[i, j] * vector[j];
            }

            result[i] = sum;
        }

        return result;
    }

    #endregion

    #region 向量内积

    public static double Dot(
        double[] left,
        double[] right)
    {
        if (left.Length != right.Length)
        {
            throw new InvalidOperationException(
                "向量维数不匹配。");
        }

        double sum = 0.0;

        for (int i = 0; i < left.Length; i++)
        {
            sum += left[i] * right[i];
        }

        return sum;
    }

    #endregion

    #region 向量加法

    public static double[] Add(
        double[] left,
        double[] right)
    {
        if (left.Length != right.Length)
        {
            throw new InvalidOperationException(
                "向量维数不匹配。");
        }

        var result = new double[left.Length];

        for (int i = 0; i < left.Length; i++)
        {
            result[i] = left[i] + right[i];
        }

        return result;
    }

    #endregion

    #region 向量减法

    public static double[] Subtract(
        double[] left,
        double[] right)
    {
        if (left.Length != right.Length)
        {
            throw new InvalidOperationException(
                "向量维数不匹配。");
        }

        var result = new double[left.Length];

        for (int i = 0; i < left.Length; i++)
        {
            result[i] = left[i] - right[i];
        }

        return result;
    }

    #endregion

    #region 矩阵减法

    public static double[,] SubtractMatrix(
        double[,] left,
        double[,] right)
    {
        int rows = left.GetLength(0);
        int cols = left.GetLength(1);

        if (rows != right.GetLength(0) || cols != right.GetLength(1))
        {
            throw new InvalidOperationException(
                "矩阵维数不匹配。");
        }

        var result = new double[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = left[i, j] - right[i, j];
            }
        }

        return result;
    }

    #endregion

    #region 数乘向量

    public static double[] ScaMultiplyVec(
        double[] vector,
        double scalar)
    {
        int length = vector.Length;

        var result = new double[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = scalar * vector[i];
        }

        return result;
    }

    #endregion

    #region 数乘矩阵

    public static double[,] MultiplyScalar(
        double[,] matrix,
        double scalar)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        var result = new double[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = scalar * matrix[i, j];
            }
        }

        return result;
    }

    #endregion

    #region 单位矩阵

    public static double[,] Identity(int size)
    {
        var result = new double[size, size];

        for (int i = 0; i < size; i++)
        {
            result[i, i] = 1.0;
        }

        return result;
    }

    #endregion

    #region 矩阵求逆

    public static double[,] Inverse(
        double[,] matrix)
    {
        int n = matrix.GetLength(0);

        if (n != matrix.GetLength(1))
        {
            throw new InvalidOperationException(
                "只有方阵才能求逆。");
        }

        var a = (double[,])matrix.Clone();
        var inverse = Identity(n);

        for (int pivot = 0; pivot < n; pivot++)
        {
            int maxRow = pivot;
            double maxValue = Math.Abs(a[pivot, pivot]);

            for (int row = pivot + 1; row < n; row++)
            {
                double value = Math.Abs(a[row, pivot]);

                if (value > maxValue)
                {
                    maxValue = value;
                    maxRow = row;
                }
            }

            if (maxValue < 1e-15)
            {
                throw new InvalidOperationException(
                    "矩阵奇异，无法求逆。");
            }

            if (maxRow != pivot)
            {
                SwapRows(a, pivot, maxRow);
                SwapRows(inverse, pivot, maxRow);
            }

            double diagonal = a[pivot, pivot];

            for (int j = 0; j < n; j++)
            {
                a[pivot, j] /= diagonal;
                inverse[pivot, j] /= diagonal;
            }

            for (int row = 0; row < n; row++)
            {
                if (row == pivot)
                {
                    continue;
                }

                double factor = a[row, pivot];

                for (int col = 0; col < n; col++)
                {
                    a[row, col] -= factor * a[pivot, col];
                    inverse[row, col] -= factor * inverse[pivot, col];
                }
            }
        }

        return inverse;
    }

    #endregion

    #region 法方程求解

    public static double[] Solve(
        double[,] N,
        double[] U)
    {
        int n = U.Length;

        var a = (double[,])N.Clone();
        var b = (double[])U.Clone();

        for (int pivot = 0; pivot < n; pivot++)
        {
            int maxRow = pivot;
            double maxValue = Math.Abs(a[pivot, pivot]);

            for (int row = pivot + 1; row < n; row++)
            {
                double value = Math.Abs(a[row, pivot]);

                if (value > maxValue)
                {
                    maxValue = value;
                    maxRow = row;
                }
            }

            if (maxValue < 1e-15)
            {
                throw new InvalidOperationException(
                    "法矩阵奇异，请检查网型！");
            }

            if (maxRow != pivot)
            {
                SwapRows(a, b, pivot, maxRow);
            }

            double diagonal = a[pivot, pivot];

            for (int j = pivot; j < n; j++)
            {
                a[pivot, j] /= diagonal;
            }

            b[pivot] /= diagonal;

            for (int row = 0; row < n; row++)
            {
                if (row == pivot)
                {
                    continue;
                }

                double factor = a[row, pivot];

                for (int col = pivot; col < n; col++)
                {
                    a[row, col] -= factor * a[pivot, col];
                }

                b[row] -= factor * b[pivot];
            }
        }

        return b;
    }

    #endregion

    #region VPV

    public static double ComputeVPV(
        double[] V,
        double[,] P)
    {
        double value = 0.0;

        for (int i = 0; i < V.Length; i++)
        {
            value +=
                V[i]
                * P[i, i]
                * V[i];
        }

        return value;
    }

    #endregion

    #region 中误差传播

    public static double[,] CovarianceMatrix(
        double sigma0_sq,
        double[,] Qxx)
    {
        return MultiplyScalar(
            Qxx,
            sigma0_sq);
    }

    #endregion

    #region 私有函数

    private static void SwapRows(
        double[,] matrix,
        int first,
        int second)
    {
        int cols = matrix.GetLength(1);

        for (int i = 0; i < cols; i++)
        {
            (matrix[first, i], matrix[second, i]) =
                (matrix[second, i], matrix[first, i]);
        }
    }

    private static void SwapRows(
        double[,] matrix,
        double[] rhs,
        int first,
        int second)
    {
        int cols = matrix.GetLength(1);

        for (int i = 0; i < cols; i++)
        {
            (matrix[first, i], matrix[second, i]) =
                (matrix[second, i], matrix[first, i]);
        }

        (rhs[first], rhs[second]) =
            (rhs[second], rhs[first]);
    }

    #endregion

    #region 矩阵解析与格式化

    /// <summary>
    /// 从字符串解析矩阵
    /// 格式：行内元素用逗号分隔，行之间用分号分隔
    /// 示例："1,2,3;4,5,6" 表示 2×3 矩阵
    /// </summary>
    public static double[,] ParseMatrix(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new double[0, 0];

        string[] rows = text.Trim().Split(';');
        int rowCount = rows.Length;
        string[] firstRow = rows[0].Split(',');
        int colCount = firstRow.Length;

        double[,] result = new double[rowCount, colCount];

        for (int i = 0; i < rowCount; i++)
        {
            string[] cols = rows[i].Split(',');

            if (cols.Length != colCount)
                throw new InvalidOperationException($"第{i + 1}行列数({cols.Length})与首行列数({colCount})不一致");

            for (int j = 0; j < colCount; j++)
                result[i, j] = double.Parse(cols[j].Trim());
        }

        return result;
    }

    /// <summary>
    /// 将矩阵格式化为字符串
    /// 输出格式：行内元素逗号分隔，行之间分号分隔，每个元素占12位并保留6位小数
    /// </summary>
    public static string MatrixToString(double[,] matrix)
    {
        if (matrix == null || matrix.GetLength(0) == 0)
            return "空矩阵";

        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        var sb = new System.Text.StringBuilder();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                sb.Append(matrix[i, j].ToString("F6").PadLeft(12));
                if (j < cols - 1)
                    sb.Append(", ");
            }
            if (i < rows - 1)
                sb.Append(";\n");
        }

        return sb.ToString();
    }

    #endregion

    #region 矩阵加法

    /// <summary>
    /// 矩阵加法
    /// </summary>
    public static double[,] AddMatrix(double[,] left, double[,] right)
    {
        int rows = left.GetLength(0);
        int cols = left.GetLength(1);

        if (rows != right.GetLength(0) || cols != right.GetLength(1))
            throw new InvalidOperationException("矩阵加法维度不匹配");

        var result = new double[rows, cols];

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[i, j] = left[i, j] + right[i, j];

        return result;
    }

    #endregion

    #region 列向量与数组互转（私有辅助）

    private static double[] ColumnToArray(double[,] column)
    {
        int n = column.GetLength(0);
        double[] result = new double[n];
        for (int i = 0; i < n; i++)
            result[i] = column[i, 0];
        return result;
    }

    private static double[,] ArrayToColumn(double[] array)
    {
        int n = array.Length;
        double[,] result = new double[n, 1];
        for (int i = 0; i < n; i++)
            result[i, 0] = array[i];
        return result;
    }

    #endregion

    #region 线性方程组求解（矩阵接口）

    /// <summary>
    /// 求解线性方程组 A * x = b
    /// 其中 b 以矩阵（列向量）形式传入
    /// </summary>
    public static double[,] SolveLinear(double[,] A, double[,] b)
    {
        double[] bVec = ColumnToArray(b);
        double[] xVec = Solve(A, bVec);
        return ArrayToColumn(xVec);
    }

    #endregion
}