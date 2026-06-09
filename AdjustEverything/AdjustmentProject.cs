using System.ComponentModel.DataAnnotations;

namespace AdjustEverything;

internal sealed class AdjustmentProject
{
    private int _pointSerial = 1;
    private int _heightSerial = 1;
    private int _distanceSerial = 1;
    private int _angleSerial = 1;
    public List<SurveyPoint> Points { get; } = [];
    public List<HeightObservation> HeightObservations { get; } = [];
    public List<DistanceObservation> DistanceObservations { get; } = [];
    public List<AngleObservation> AngleObservations { get; } = [];
    public List<BoardLine> Lines { get; } = [];

    public SurveyPoint AddPoint(string? name, PointF canvasLocation)
    {
        var point = new SurveyPoint
        {
            Id = Guid.NewGuid(),
            Name = string.IsNullOrWhiteSpace(name) ? NextPointName() : name.Trim(),
            CanvasLocation = canvasLocation,
            X = canvasLocation.X,
            Y = canvasLocation.Y,
        };
        Points.Add(point);
        return point;
    }

    public BoardLine AddLine(SurveyPoint from, SurveyPoint to)
    {
        var existing = Lines.FirstOrDefault(line => line.Connects(from, to));
        if (existing is not null)
        {
            return existing;
        }

        var line = new BoardLine(from, to);
        Lines.Add(line);
        return line;
    }

    public HeightObservation AddHeightObservation(SurveyPoint from, SurveyPoint to, double value, double length)
    {
        AddLine(from, to);
        var observation = new HeightObservation
        {
            Name = $"h{_heightSerial++}",
            From = from,
            To = to,
            Value = value,
            Length = length,
            Sigma = 1.0,
        };
        HeightObservations.Add(observation);
        return observation;
    }

    public DistanceObservation AddDistanceObservation(SurveyPoint from, SurveyPoint to, double value)
    {
        AddLine(from, to);
        var observation = new DistanceObservation
        {
            Name = $"s{_distanceSerial++}",
            From = from,
            To = to,
            Value = value,
            Sigma = 1.0,
        };
        DistanceObservations.Add(observation);
        return observation;
    }
    public AngleObservation AddAngleObservation(
    SurveyPoint from,
    SurveyPoint vertex,
    SurveyPoint to)
    {
        AddLine(from, vertex);
        AddLine(vertex, to);

        var observation =
            new AngleObservation
            {
                Name = $"a{_angleSerial++}",

                From = from,

                Vertex = vertex,

                To = to,


                Sigma = 1.0
            };

        AngleObservations.Add(observation);

        return observation;
    }
    public void RemovePoint(SurveyPoint point)
    {
        HeightObservations.RemoveAll(obs => ReferenceEquals(obs.From, point) || ReferenceEquals(obs.To, point));
        DistanceObservations.RemoveAll(obs => ReferenceEquals(obs.From, point) || ReferenceEquals(obs.To, point));
        AngleObservations.RemoveAll(obs => ReferenceEquals(obs.From, point) || ReferenceEquals(obs.Vertex, point) || ReferenceEquals(obs.To, point));
        Lines.RemoveAll(line => ReferenceEquals(line.From, point) || ReferenceEquals(line.To, point));
        Points.Remove(point);
    }

    public void RemoveHeightObservation(HeightObservation observation)
    {
        HeightObservations.Remove(observation);
        RemoveLineIfUnused(observation.From, observation.To);
    }

    public void RemoveDistanceObservation(DistanceObservation observation)
    {
        DistanceObservations.Remove(observation);
        RemoveLineIfUnused(observation.From, observation.To);
    }
    public void RemoveAngleObservation(
    AngleObservation observation)
    {
        AngleObservations.Remove(observation);
    }
    public void RemoveObject(object? value)
    {
        switch (value)
        {
            case SurveyPoint point:
                RemovePoint(point);
                break;
            case HeightObservation observation:
                RemoveHeightObservation(observation);
                break;
            case DistanceObservation observation:
                RemoveDistanceObservation(observation);
                break;
            case AngleObservation observation:
                RemoveAngleObservation(observation);
                break;
        }
    }

    public void Clear()
    {
        Points.Clear();
        Lines.Clear();
        HeightObservations.Clear();
        DistanceObservations.Clear();
        AngleObservations.Clear();
        _pointSerial = 1;
        _heightSerial = 1;
        _distanceSerial = 1;
        _angleSerial = 1;
    }

    private void RemoveLineIfUnused(SurveyPoint from, SurveyPoint to)
    {
        var hasObservation = HeightObservations.Any(obs => Connects(obs.From, obs.To, from, to))
            || DistanceObservations.Any(obs => Connects(obs.From, obs.To, from, to));

        if (!hasObservation)
        {
            Lines.RemoveAll(line => line.Connects(from, to));
        }
    }

    private static bool Connects(SurveyPoint a, SurveyPoint b, SurveyPoint from, SurveyPoint to)
    {
        return ReferenceEquals(a, from) && ReferenceEquals(b, to)
            || ReferenceEquals(a, to) && ReferenceEquals(b, from);
    }

    private string NextPointName()
    {
        var index = _pointSerial++;
        var letter = (char)('A' + (index - 1) % 26);
        var round = (index - 1) / 26;
        return round == 0 ? letter.ToString() : $"{letter}{round + 1}";
    }
}

public enum DisplayMode
{
    Height,
    Coordinate
}

internal sealed class SurveyPoint
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }
    public required PointF CanvasLocation { get; set; }
    public bool IsHeightFixed { get; set; }
    public double? Height { get; set; }
    public bool IsCoordinateFixed { get; set; }
    public double? X { get; set; }
    public double? Y { get; set; }


    public DisplayMode CurrentDisplayMode { get; set; }

    public override string ToString()
    {
        if ((CurrentDisplayMode == DisplayMode.Height) && IsHeightFixed)
        {
            return $"点 {Name}  已知H={Height:F3} m";
        }
        else if ((CurrentDisplayMode == DisplayMode.Coordinate) && IsCoordinateFixed)
        {
            return $"点 {Name}  已知XY=({X:F3},{Y:F3}) m";
        }
        else
        {
            return $"点 {Name}";
        }
    }
}

internal sealed class BoardLine(SurveyPoint from, SurveyPoint to)
{
    public SurveyPoint From { get; } = from;
    public SurveyPoint To { get; } = to;

    public bool Connects(SurveyPoint a, SurveyPoint b)
    {
        return ReferenceEquals(From, a) && ReferenceEquals(To, b)
            || ReferenceEquals(From, b) && ReferenceEquals(To, a);
    }
}

internal sealed class HeightObservation
{
    public required string Name { get; set; }
    public required SurveyPoint From { get; init; }
    public required SurveyPoint To { get; init; }
    public required double Value { get; set; }
    public required double Length { get; set; }
    public required double Sigma { get; set; }

    public override string ToString()
    {
        return $"{Name}: {From.Name}->{To.Name}  Δh={Value:F3} m";
    }
}

internal sealed class DistanceObservation
{
    public required string Name { get; set; }
    public required SurveyPoint From { get; init; }
    public required SurveyPoint To { get; init; }
    public required double Value { get; set; }
    public required double Sigma { get; set; }

    public override string ToString()
    {
        return $"{Name}: {From.Name}-{To.Name}  S={Value:F3} m";
    }
}


/// <summary>
/// 角度观测
/// ∠ABC
/// B为测站点(Vertex)
/// </summary>
internal sealed class AngleObservation
{
    private bool _isManual = false;
    private double _manualValue;
    /// <summary>
    /// 观测名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 后视点A
    /// </summary>
    public required SurveyPoint From { get; init; }

    /// <summary>
    /// 测站点B
    /// </summary>
    public required SurveyPoint Vertex { get; init; }

    /// <summary>
    /// 前视点C
    /// </summary>
    public required SurveyPoint To { get; init; }

    /// <summary>
    /// 角度值(度)
    /// </summary>
    public double Value
    {
        get
        {
            if (_isManual)
                return _manualValue;

            var bax = From.CanvasLocation.X - Vertex.CanvasLocation.X;
            var bay = From.CanvasLocation.Y - Vertex.CanvasLocation.Y;

            var bcx = To.CanvasLocation.X - Vertex.CanvasLocation.X;
            var bcy = To.CanvasLocation.Y - Vertex.CanvasLocation.Y;

            var dot = bax * bcx + bay * bcy;

            var len1 = Math.Sqrt(
                bax * bax +
                bay * bay);

            var len2 = Math.Sqrt(
                bcx * bcx +
                bcy * bcy);

            if (len1 < 1e-6 || len2 < 1e-6)
                return 0.0;

            var cos =
                dot /
                (len1 * len2);

            cos = Math.Max(
                -1.0,
                Math.Min(
                    1.0,
                    cos));

            return
                Math.Acos(cos)
                * 180.0
                / Math.PI;
        }
    }
    public double Sigma { get; set; }

    /// <summary>
    /// 弧度值
    /// </summary>
    public double ValueRad =>
        Value * Math.PI / 180.0;

    public void SetManualValue(double value)
    {
        _manualValue = value;
        _isManual = true;
    }

    public override string ToString()
    {
        return $"角度 {Name} = {Value:F6}°";
    }

}
