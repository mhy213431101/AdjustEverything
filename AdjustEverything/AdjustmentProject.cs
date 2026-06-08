using System.ComponentModel.DataAnnotations;

namespace AdjustEverything;

internal sealed class AdjustmentProject
{
    private int _pointSerial = 1;
    private int _heightSerial = 1;
    private int _distanceSerial = 1;

    public List<SurveyPoint> Points { get; } = [];
    public List<HeightObservation> HeightObservations { get; } = [];
    public List<DistanceObservation> DistanceObservations { get; } = [];
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

    public void RemovePoint(SurveyPoint point)
    {
        HeightObservations.RemoveAll(obs => ReferenceEquals(obs.From, point) || ReferenceEquals(obs.To, point));
        DistanceObservations.RemoveAll(obs => ReferenceEquals(obs.From, point) || ReferenceEquals(obs.To, point));
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
        }
    }

    public void Clear()
    {
        Points.Clear();
        Lines.Clear();
        HeightObservations.Clear();
        DistanceObservations.Clear();
        _pointSerial = 1;
        _heightSerial = 1;
        _distanceSerial = 1;
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
