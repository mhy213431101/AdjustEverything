using AdjustEverything;

internal static class ApproximateHeightBuilder
{
    /// <summary>
    /// 根据已知高程点和高差观测推算未知点近似高程
    /// </summary>
    public static Dictionary<SurveyPoint, double> Build(
        List<SurveyPoint> points,
        List<HeightObservation> observations)
    {
        var heights =
            new Dictionary<SurveyPoint, double>();

        var queue =
            new Queue<SurveyPoint>();

        // 初始化已知高程点
        foreach (var point in points)
        {
            if (point.Height.HasValue)
            {
                heights[point] =
                    point.Height.Value;

                queue.Enqueue(point);
            }
        }

        if (queue.Count == 0)
        {
            throw new InvalidOperationException(
                "水准网中不存在已知高程点。");
        }

        // 广度优先传播高程
        while (queue.Count > 0)
        {
            var current =
                queue.Dequeue();

            foreach (var obs in observations)
            {
                // current 为 From
                if (obs.From == current)
                {
                    if (!heights.ContainsKey(obs.To))
                    {
                        heights[obs.To] =
                            heights[current] +
                            obs.Value;

                        queue.Enqueue(
                            obs.To);
                    }
                }

                // current 为 To
                if (obs.To == current)
                {
                    if (!heights.ContainsKey(obs.From))
                    {
                        heights[obs.From] =
                            heights[current] -
                            obs.Value;

                        queue.Enqueue(
                            obs.From);
                    }
                }
            }
        }

        return heights;
    }

    /// <summary>
    /// 按未知点顺序生成 X0
    /// </summary>
    public static double[] BuildX0(
        List<SurveyPoint> points,
        List<SurveyPoint> unknownPoints,
        List<HeightObservation> observations)
    {
        var heights =
            Build(
                points,
                observations);

        var X0 =
            new double[
                unknownPoints.Count];

        for (int i = 0;
             i < unknownPoints.Count;
             i++)
        {
            if (!heights.TryGetValue(
                    unknownPoints[i],
                    out double h))
            {
                throw new InvalidOperationException(
                    $"点 {unknownPoints[i].Name} 无法通过高差路线推算近似高程，请检查网的连通性。");
            }

            X0[i] = h;
        }

        return X0;
    }
}