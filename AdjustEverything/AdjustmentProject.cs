using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace AdjustEverything;

internal sealed class AdjustmentProject
{
    private int _pointSerial = 1;
    private int _baselineSerial = 1;
    private int _knownSideSerial = 1;
    private int _heightSerial = 1;
    private int _distanceSerial = 1;
    private int _angleSerial = 1;

    public List<SurveyPoint> Points { get; } = [];
    public List<HeightObservation> HeightObservations { get; } = [];
    public List<DistanceObservation> DistanceObservations { get; } = [];
    public List<AngleObservation> AngleObservations { get; } = [];
    public List<BoardLine> Lines { get; } = [];
    public List<KnownPoint> KnownPoints { get; } = [];
    public List<KnownSide> KnownSides { get; } = [];
    public List<Baseline> Baselines { get; } = [];

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

    public KnownPoint AddKnownPoint(SurveyPoint point)
    {
        var existing = KnownPoints.FirstOrDefault(p => ReferenceEquals(p.Point, point));

        if (existing != null)
        {
            return existing;
        }

        point.IsCoordinateFixed = true;

        var known = new KnownPoint
        {
            Point = point
        };

        KnownPoints.Add(known);
        return known;
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

    public KnownSide AddKnownSide(SurveyPoint from, SurveyPoint to, double length)
    {
        AddLine(from, to);
        var side = new KnownSide
        {
            Name = $"k{_knownSideSerial++}",
            From = from,
            To = to,
            Length = length
        };

        KnownSides.Add(side);
        return side;
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

    public Baseline AddBaseline(SurveyPoint from, SurveyPoint to)
    {
        bool known1 = KnownPoints.Any(
                p =>
                    ReferenceEquals(
                        p.Point,
                        from));

        bool known2 = KnownPoints.Any(
                p =>
                    ReferenceEquals(
                        p.Point,
                        to));

        if (!known1 || !known2)
        {
            throw new InvalidOperationException("基线两端必须为已知点。");
        }

        AddLine(from, to);

        var baseline = new Baseline
        {
            Name = $"b{_baselineSerial++}",
            From = from,
            To = to
        };

        Baselines.Add(baseline);
        return baseline;
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

    public void RemoveKnownPoint(KnownPoint point)
    {
        HeightObservations.RemoveAll(obs => ReferenceEquals(obs.From, point) || ReferenceEquals(obs.To, point));
        DistanceObservations.RemoveAll(obs => ReferenceEquals(obs.From, point) || ReferenceEquals(obs.To, point));
        AngleObservations.RemoveAll(obs => ReferenceEquals(obs.From, point) || ReferenceEquals(obs.Vertex, point) || ReferenceEquals(obs.To, point));
        Lines.RemoveAll(line => ReferenceEquals(line.From, point) || ReferenceEquals(line.To, point));
        Baselines.RemoveAll(baseline => ReferenceEquals(baseline.From, point) || ReferenceEquals(baseline.To, point));
        KnownPoints.Remove(point);
    }

    public void RemoveKnownSide(KnownSide side)
    {
        KnownSides.Remove(side);
        RemoveLineIfUnused(side.From, side.To);
    }

    public void RemoveBaseline(Baseline baseline)
    {
        Baselines.Remove(baseline);
        RemoveLineIfUnused(baseline.From, baseline.To);
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
            case Baseline baseline:
                RemoveBaseline(baseline);
                break;
            case KnownPoint knownPoint:
                RemoveKnownPoint(knownPoint);
                break;
            case KnownSide knownSide:
                RemoveKnownSide(knownSide);
                break;
        }
    }

    public void Clear()
    {
        Points.Clear();
        Lines.Clear();
        KnownPoints.Clear();
        KnownSides.Clear();
        Baselines.Clear();
        HeightObservations.Clear();
        DistanceObservations.Clear();
        AngleObservations.Clear();

        _pointSerial = 1;
        _baselineSerial = 1;
        _knownSideSerial = 1;
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

internal sealed class KnownPoint
{
    public required SurveyPoint Point { get; init; }

    public override string ToString()
    {
        return
            $"{Point.Name} " +
            $"({Point.X:F3},{Point.Y:F3})";
    }
}

internal sealed class KnownSide
{
    public required string Name { get; set; }

    public required SurveyPoint From { get; init; }

    public required SurveyPoint To { get; init; }

    public required double Length { get; set; }

    public override string ToString()
    {
        return
            $"{Name}: " +
            $"{From.Name}-{To.Name} " +
            $"L={Length:F3}";
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

internal sealed class Baseline()
{
    public required string Name { get; set; }

    public required SurveyPoint From { get; init; }

    public required SurveyPoint To { get; init; }

    public double Length
    {
        get
        {
            if (!From.X.HasValue ||
                !From.Y.HasValue ||
                !To.X.HasValue ||
                !To.Y.HasValue)
            {
                return 0.0;
            }

            var dx = To.X.Value - From.X.Value;
            var dy = To.Y.Value - From.Y.Value;

            return Math.Sqrt(
                dx * dx +
                dy * dy);
        }
    }

    public override string ToString()
    {
        return
            $"{Name}: {From.Name}-{To.Name}  " +
            $"L={Length:F3} m";
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

internal sealed class AngleObservation
{
    public required string Name { get; set; }

    public required SurveyPoint From { get; init; }

    public required SurveyPoint Vertex { get; init; }

    public required SurveyPoint To { get; init; }

    // 实测角
    public double Value { get; set; }

    public double Sigma { get; set; }

    public double ValueRad =>
        Value * Math.PI / 180.0;

    // 当前坐标计算角
    public double CurrentValue
    {
        get
        {
            double a1 =
                Math.Atan2(
                    From.Y!.Value - Vertex.Y!.Value,
                    From.X!.Value - Vertex.X!.Value);

            double a2 =
                Math.Atan2(
                    To.Y!.Value - Vertex.Y!.Value,
                    To.X!.Value - Vertex.X!.Value);

            double angle =
                (a2 - a1) *
                180.0 /
                Math.PI;

            while (angle < 0)
                angle += 360.0;

            while (angle >= 360)
                angle -= 360.0;

            return angle;
        }
    }

    public override string ToString()
    {
        return
            $"角度 {Name}  " +
            $"观测={Value:F4}°  " +
            $"当前={CurrentValue:F4}°";
    }
}
