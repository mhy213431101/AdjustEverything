using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AdjustEverything
{
    internal class AngleModel : IAdjustmentModel, ILinearizable
    {
        private readonly List<AngleObservation> _observations;
        private readonly List<SurveyPoint> _unknownPoints;
        private readonly Dictionary<SurveyPoint, int> _paramIndex;

        public AngleModel(
            List<SurveyPoint> unknownPoints,
            List<AngleObservation> observations,
            Dictionary<SurveyPoint, int> paramIndex,
            double[] x0)
        {
            _unknownPoints = unknownPoints;
            _observations = observations;
            _paramIndex = paramIndex;
            X0 = x0;
        }

        public int n => _observations.Count;
        public int t => _unknownPoints.Count * 2;
        public int separate => 0;

        public double[] X0 { get; }

        /// <summary>
        /// 误差方程线性化
        /// </summary>

        private static double NormalizeAngle(double angle)
        {
            while (angle > Math.PI)
                angle -= 2 * Math.PI;

            while (angle < -Math.PI)
                angle += 2 * Math.PI;

            return angle;
        }
        public void Linearize(double[] X, double[,] B, double[] W, double[,] P, double[] L)
        {
            for (int i = 0; i < _observations.Count; i++)
            {
                var obs = _observations[i];

                var from = GetPoint(obs.From, X);
                var vertex = GetPoint(obs.Vertex, X);
                var to = GetPoint(obs.To, X);

                // 计算观测角（弧度）
                double computedAngleRad = ComputeAngle(from, vertex, to);
                          
                // 误差方程

                L[i] = obs.ValueRad;                 // 观测值弧度
                W[i] =(NormalizeAngle(obs.ValueRad - computedAngleRad));
                P[i, i] = 1.0 /(obs.Sigma * obs.Sigma);

                // 雅可比矩阵
                ApplyJacobian(B, i, obs, X);
            }
        }

        private (double X, double Y) GetPoint(SurveyPoint p, double[] X)
        {
            if (_paramIndex.TryGetValue(p, out int idx))
                return (X[idx], X[idx + 1]);

            if (!p.X.HasValue || !p.Y.HasValue)
            {
                throw new InvalidOperationException($"点 {p.Name} 缺少 X/Y 坐标。");
            }

            return (p.X.Value, p.Y.Value);
        }

        private double ComputeAngle((double X, double Y) a,
                                    (double X, double Y) b,
                                    (double X, double Y) c)
        {
            double ang1 = Math.Atan2(a.Y - b.Y, a.X - b.X);
            double ang2 = Math.Atan2(c.Y - b.Y, c.X - b.X);
            double angle = ang2 - ang1;
            if (angle < 0) angle += 2 * Math.PI;
            return angle; // 返回弧度
        }

        private void ApplyJacobian(double[,] B, int row, AngleObservation obs, double[] X)
        {
            double eps = 1e-5;
            for (int k = 0; k < X.Length; k++)
            {
                double[] X1 = (double[])X.Clone();
                double[] X2 = (double[])X.Clone();

                X1[k] += eps;
                X2[k] -= eps;

                var a1 = ComputeAngle(GetPoint(obs.From, X1), GetPoint(obs.Vertex, X1), GetPoint(obs.To, X1));
                var a2 = ComputeAngle(GetPoint(obs.From, X2), GetPoint(obs.Vertex, X2), GetPoint(obs.To, X2));

                B[row, k] = ((a1 - a2) / (2 * eps)); // 雅可比单位为弧度
            }
        }
    }
}

