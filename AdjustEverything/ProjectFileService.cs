using System.Drawing;
using System.Text.Json;

namespace AdjustEverything;

internal static class ProjectFileService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    public static void Save(AdjustmentProject project, string path, string? resultText)
    {
        var snapshot = new ProjectSnapshot
        {
            Version = 1,
            NetworkType = InferNetworkType(project),
            SavedAt = DateTimeOffset.Now,
            LastResult = string.IsNullOrWhiteSpace(resultText) ? null : resultText,
            Points = project.Points.Select(PointDto.FromPoint).ToList(),
            Lines = project.Lines.Select(LineDto.FromLine).ToList(),
            KnownPoints = project.KnownPoints.Select(p => new KnownPointDto { Point = p.Point.Name }).ToList(),
            KnownSides = project.KnownSides.Select(KnownSideDto.FromKnownSide).ToList(),
            Baselines = project.Baselines.Select(BaselineDto.FromBaseline).ToList(),
            HeightObservations = project.HeightObservations.Select(HeightObservationDto.FromObservation).ToList(),
            DistanceObservations = project.DistanceObservations.Select(DistanceObservationDto.FromObservation).ToList(),
            AngleObservations = project.AngleObservations.Select(AngleObservationDto.FromObservation).ToList(),
        };

        var json = JsonSerializer.Serialize(snapshot, Options);
        File.WriteAllText(path, json);
    }

    public static string Load(string path, AdjustmentProject project)
    {
        var json = File.ReadAllText(path);
        var snapshot = JsonSerializer.Deserialize<ProjectSnapshot>(json, Options)
            ?? throw new InvalidOperationException("项目文件为空或格式不正确。");

        project.Clear();

        var points = new Dictionary<string, SurveyPoint>(StringComparer.OrdinalIgnoreCase);
        foreach (var dto in snapshot.Points)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new InvalidOperationException("项目文件中存在未命名的点。");
            }

            if (points.ContainsKey(dto.Name))
            {
                throw new InvalidOperationException($"项目文件中存在重复点名：{dto.Name}");
            }

            var point = project.AddPoint(dto.Name, new PointF(dto.CanvasX, dto.CanvasY));
            point.IsHeightFixed = dto.IsHeightFixed;
            point.Height = dto.Height;
            point.IsCoordinateFixed = dto.IsCoordinateFixed;
            point.X = dto.X;
            point.Y = dto.Y;
            point.CurrentDisplayMode = dto.CurrentDisplayMode;
            points.Add(point.Name, point);
        }

        foreach (var line in snapshot.Lines)
        {
            project.AddLine(FindPoint(points, line.From), FindPoint(points, line.To));
        }

        foreach (var known in snapshot.KnownPoints)
        {
            project.AddKnownPoint(FindPoint(points, known.Point));
        }

        foreach (var point in project.Points.Where(p => p.IsCoordinateFixed))
        {
            project.AddKnownPoint(point);
        }

        foreach (var side in snapshot.KnownSides)
        {
            var knownSide = project.AddKnownSide(
                FindPoint(points, side.From),
                FindPoint(points, side.To),
                side.Length);
            knownSide.Name = side.Name;
        }

        foreach (var baseline in snapshot.Baselines)
        {
            var imported = project.AddBaseline(
                FindPoint(points, baseline.From),
                FindPoint(points, baseline.To));
            imported.Name = baseline.Name;
        }

        foreach (var observation in snapshot.HeightObservations)
        {
            var imported = project.AddHeightObservation(
                FindPoint(points, observation.From),
                FindPoint(points, observation.To),
                observation.Value,
                observation.Length);
            imported.Name = observation.Name;
            imported.Sigma = observation.Sigma;
        }

        foreach (var observation in snapshot.DistanceObservations)
        {
            var imported = project.AddDistanceObservation(
                FindPoint(points, observation.From),
                FindPoint(points, observation.To),
                observation.Value);
            imported.Name = observation.Name;
            imported.Sigma = observation.Sigma;
        }

        foreach (var observation in snapshot.AngleObservations)
        {
            var imported = project.AddAngleObservation(
                FindPoint(points, observation.From),
                FindPoint(points, observation.Vertex),
                FindPoint(points, observation.To));
            imported.Name = observation.Name;
            imported.Value = observation.Value;
            imported.Sigma = observation.Sigma;
        }

        return snapshot.LastResult ?? "";
    }

    private static SurveyPoint FindPoint(Dictionary<string, SurveyPoint> points, string name)
    {
        if (points.TryGetValue(name, out var point))
        {
            return point;
        }

        throw new InvalidOperationException($"项目文件引用了不存在的点：{name}");
    }

    private static string InferNetworkType(AdjustmentProject project)
    {
        if (project.AngleObservations.Count > 0 && project.DistanceObservations.Count > 0)
        {
            return "AngleDistance";
        }

        if (project.AngleObservations.Count > 0)
        {
            return "Angle";
        }

        if (project.DistanceObservations.Count > 0)
        {
            return "Distance";
        }

        if (project.HeightObservations.Count > 0)
        {
            return "Height";
        }

        return "Sketch";
    }

    private sealed class ProjectSnapshot
    {
        public int Version { get; set; }
        public string NetworkType { get; set; } = "";
        public DateTimeOffset SavedAt { get; set; }
        public string? LastResult { get; set; }
        public List<PointDto> Points { get; set; } = [];
        public List<LineDto> Lines { get; set; } = [];
        public List<KnownPointDto> KnownPoints { get; set; } = [];
        public List<KnownSideDto> KnownSides { get; set; } = [];
        public List<BaselineDto> Baselines { get; set; } = [];
        public List<HeightObservationDto> HeightObservations { get; set; } = [];
        public List<DistanceObservationDto> DistanceObservations { get; set; } = [];
        public List<AngleObservationDto> AngleObservations { get; set; } = [];
    }

    private sealed class PointDto
    {
        public string Name { get; set; } = "";
        public float CanvasX { get; set; }
        public float CanvasY { get; set; }
        public bool IsHeightFixed { get; set; }
        public double? Height { get; set; }
        public bool IsCoordinateFixed { get; set; }
        public double? X { get; set; }
        public double? Y { get; set; }
        public DisplayMode CurrentDisplayMode { get; set; }

        public static PointDto FromPoint(SurveyPoint point)
        {
            return new PointDto
            {
                Name = point.Name,
                CanvasX = point.CanvasLocation.X,
                CanvasY = point.CanvasLocation.Y,
                IsHeightFixed = point.IsHeightFixed,
                Height = point.Height,
                IsCoordinateFixed = point.IsCoordinateFixed,
                X = point.X,
                Y = point.Y,
                CurrentDisplayMode = point.CurrentDisplayMode,
            };
        }
    }

    private sealed class LineDto
    {
        public string From { get; set; } = "";
        public string To { get; set; } = "";

        public static LineDto FromLine(BoardLine line)
        {
            return new LineDto
            {
                From = line.From.Name,
                To = line.To.Name,
            };
        }
    }

    private sealed class KnownPointDto
    {
        public string Point { get; set; } = "";
    }

    private sealed class KnownSideDto
    {
        public string Name { get; set; } = "";
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public double Length { get; set; }

        public static KnownSideDto FromKnownSide(KnownSide side)
        {
            return new KnownSideDto
            {
                Name = side.Name,
                From = side.From.Name,
                To = side.To.Name,
                Length = side.Length,
            };
        }
    }

    private sealed class BaselineDto
    {
        public string Name { get; set; } = "";
        public string From { get; set; } = "";
        public string To { get; set; } = "";

        public static BaselineDto FromBaseline(Baseline baseline)
        {
            return new BaselineDto
            {
                Name = baseline.Name,
                From = baseline.From.Name,
                To = baseline.To.Name,
            };
        }
    }

    private sealed class HeightObservationDto
    {
        public string Name { get; set; } = "";
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public double Value { get; set; }
        public double Length { get; set; }
        public double Sigma { get; set; }

        public static HeightObservationDto FromObservation(HeightObservation observation)
        {
            return new HeightObservationDto
            {
                Name = observation.Name,
                From = observation.From.Name,
                To = observation.To.Name,
                Value = observation.Value,
                Length = observation.Length,
                Sigma = observation.Sigma,
            };
        }
    }

    private sealed class DistanceObservationDto
    {
        public string Name { get; set; } = "";
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public double Value { get; set; }
        public double Sigma { get; set; }

        public static DistanceObservationDto FromObservation(DistanceObservation observation)
        {
            return new DistanceObservationDto
            {
                Name = observation.Name,
                From = observation.From.Name,
                To = observation.To.Name,
                Value = observation.Value,
                Sigma = observation.Sigma,
            };
        }
    }

    private sealed class AngleObservationDto
    {
        public string Name { get; set; } = "";
        public string From { get; set; } = "";
        public string Vertex { get; set; } = "";
        public string To { get; set; } = "";
        public double Value { get; set; }
        public double Sigma { get; set; }

        public static AngleObservationDto FromObservation(AngleObservation observation)
        {
            return new AngleObservationDto
            {
                Name = observation.Name,
                From = observation.From.Name,
                Vertex = observation.Vertex.Name,
                To = observation.To.Name,
                Value = observation.Value,
                Sigma = observation.Sigma,
            };
        }
    }
}
