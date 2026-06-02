namespace AdjustEverything;

internal sealed class AdjustmentProject
{
    private int _pointSerial = 1;
    private int _heightSerial = 1;

    public List<SurveyPoint> Points { get; } = [];
    public List<HeightObservation> HeightObservations { get; } = [];
    public List<BoardLine> Lines { get; } = [];

    public SurveyPoint AddPoint(string? name, PointF canvasLocation)
    {
        var point = new SurveyPoint
        {
            Id = Guid.NewGuid(),
            Name = string.IsNullOrWhiteSpace(name) ? NextPointName() : name.Trim(),
            CanvasLocation = canvasLocation,
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

    public HeightObservation AddHeightObservation(SurveyPoint from, SurveyPoint to, double value)
    {
        AddLine(from, to);
        var observation = new HeightObservation
        {
            Name = $"h{_heightSerial++}",
            From = from,
            To = to,
            Value = value,
            Sigma = 1.0,
        };
        HeightObservations.Add(observation);
        return observation;
    }

    public void RemovePoint(SurveyPoint point)
    {
        HeightObservations.RemoveAll(obs => ReferenceEquals(obs.From, point) || ReferenceEquals(obs.To, point));
        Lines.RemoveAll(line => ReferenceEquals(line.From, point) || ReferenceEquals(line.To, point));
        Points.Remove(point);
    }

    public void RemoveHeightObservation(HeightObservation observation)
    {
        HeightObservations.Remove(observation);

        var hasOtherObservationOnSameLine = HeightObservations.Any(obs =>
            ReferenceEquals(obs.From, observation.From) && ReferenceEquals(obs.To, observation.To)
            || ReferenceEquals(obs.From, observation.To) && ReferenceEquals(obs.To, observation.From));

        if (!hasOtherObservationOnSameLine)
        {
            Lines.RemoveAll(line => line.Connects(observation.From, observation.To));
        }
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
        }
    }

    public void Clear()
    {
        Points.Clear();
        Lines.Clear();
        HeightObservations.Clear();
        _pointSerial = 1;
        _heightSerial = 1;
    }

    private string NextPointName()
    {
        var index = _pointSerial++;
        var letter = (char)('A' + (index - 1) % 26);
        var round = (index - 1) / 26;
        return round == 0 ? letter.ToString() : $"{letter}{round + 1}";
    }
}

internal sealed class SurveyPoint
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }
    public required PointF CanvasLocation { get; set; }
    public bool IsHeightFixed { get; set; }
    public double? Height { get; set; }

    public override string ToString()
    {
        return IsHeightFixed
            ? $"点 {Name}  已知H={Height:F3}"
            : $"点 {Name}";
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
    public required double Sigma { get; set; }

    public override string ToString()
    {
        return $"{Name}: {From.Name}->{To.Name}  Δh={Value:F3}";
    }
}
