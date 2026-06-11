using System.Drawing.Drawing2D;

namespace AdjustEverything;

internal sealed class DrawingBoard : Control
{
    private const float PointRadius = 9F;
    private readonly AdjustmentProject _project;
    private SurveyPoint? _pendingPoint;
    private SurveyPoint? _angleFrom;
    private SurveyPoint? _angleVertex;
    private SurveyPoint? _draggingPoint;
    private PointF _dragOffset;

    public DrawingBoard(AdjustmentProject project)
    {
        _project = project;
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw, true);
    }

    public event EventHandler? ProjectChanged;
    public event EventHandler<string>? StatusChanged;
    public event EventHandler<object?>? SelectionChanged;

    public ToolMode Mode { get; set; }
    public object? SelectedObject { get; private set; }

    public void SelectObject(object? value)
    {
        SelectedObject = value;
        SelectionChanged?.Invoke(this, value);
        Invalidate();
    }

    public void ClearSelection()
    {
        _pendingPoint = null;
        _draggingPoint = null;
        SelectObject(null);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);

        DrawGrid(g);
        DrawLines(g);
        DrawObservationLabels(g);
        DrawAngles(g);
        DrawPoints(g);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();

        var point = HitTestPoint(e.Location);
        if (point is not null)
        {
            HandlePointClick(point, e.Location);
            return;
        }
        var angleHit = HitTestAngle(e.Location);
        if (angleHit != null)
        {
            SelectObject(angleHit);
            StatusChanged?.Invoke(this, "选中角度观测。");
            return;
        }
        var observation = HitTestObservation(e.Location);
        if (observation is not null)
        {
            SelectObject(observation);
            StatusChanged?.Invoke(this, $"选中观测 {GetObservationName(observation)}。");
            return;
        }

        if (Mode == ToolMode.AddPoint && e.Button == MouseButtons.Left)
        {
            var added = _project.AddPoint(null, e.Location);
            SelectObject(added);
            ProjectChanged?.Invoke(this, EventArgs.Empty);
            StatusChanged?.Invoke(this, "已添加点。继续单击可添加更多点。");
            return;
        }

        SelectObject(null);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_draggingPoint is null || e.Button != MouseButtons.Left)
        {
            Cursor = HitTestPoint(e.Location) is null && HitTestObservation(e.Location) is null
                ? Cursors.Cross
                : Cursors.Hand;
            return;
        }

        _draggingPoint.CanvasLocation = new PointF(e.X + _dragOffset.X, e.Y + _dragOffset.Y);
        if (!_draggingPoint.IsCoordinateFixed)
        {
            var surveyLocation = SurveyCoordinateMapper.FromCanvas(_draggingPoint.CanvasLocation);
            _draggingPoint.X = surveyLocation.X;
            _draggingPoint.Y = surveyLocation.Y;
        }
        ProjectChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _draggingPoint = null;
    }

    private void HandlePointClick(SurveyPoint point, Point location)
    {
        switch (Mode)
        {
            case ToolMode.AddLine:
                ConnectPoint(point, ObservationCreation.None);
                break;
            case ToolMode.AddBaseLine:
                ConnectPoint(point, ObservationCreation.Baseline);
                break;
            case ToolMode.AddHeight:
                ConnectPoint(point, ObservationCreation.Height);
                break;
            case ToolMode.AddDistance:
                ConnectPoint(point, ObservationCreation.Distance);
                break;
            case ToolMode.AddAngle:
                CreateAngle(point);
                break;
            case ToolMode.FixedHeight:
                SelectObject(point);
                point.IsHeightFixed = true;
                point.Height ??= 0.0;
                ProjectChanged?.Invoke(this, EventArgs.Empty);
                StatusChanged?.Invoke(this, $"点 {point.Name} 已设为已知高程点，可在属性面板修改高程。");
                break;
            case ToolMode.AddKnownPoint:
                SelectObject(point);
                _project.AddKnownPoint(point);
                point.X ??= SurveyCoordinateMapper.XFromCanvas(point.CanvasLocation);
                point.Y ??= SurveyCoordinateMapper.YFromCanvas(point.CanvasLocation);
                ProjectChanged?.Invoke(this, EventArgs.Empty);
                StatusChanged?.Invoke(this, $"点 {point.Name} 已设为已知平面坐标点，可在属性面板修改 X/Y。");
                break;
            default:
                SelectObject(point);
                _draggingPoint = point;
                _dragOffset = new PointF(point.CanvasLocation.X - location.X, point.CanvasLocation.Y - location.Y);
                StatusChanged?.Invoke(this, $"选中点 {point.Name}。拖动可调整画板位置。");
                break;
        }
    }
    private void CreateAngle(SurveyPoint point)
    {
        if (_angleFrom is null)
        {
            _angleFrom = point;

            StatusChanged?.Invoke(
                this,
                $"已选择后视点 {point.Name}");

            return;
        }

        if (_angleVertex is null)
        {
            _angleVertex = point;

            StatusChanged?.Invoke(
                this,
                $"已选择测站点 {point.Name}");

            return;
        }

        var angle = _project.AddAngleObservation(
           _angleFrom,
    _angleVertex,
    point);

        _angleFrom = null;
        _angleVertex = null;

        SelectObject(angle);

        ProjectChanged?.Invoke(
            this,
            EventArgs.Empty);

        StatusChanged?.Invoke(
            this,
            $"已创建角度 {angle.Name}");
    }
    private void ConnectPoint(SurveyPoint point, ObservationCreation creation)
    {
        if (_pendingPoint is null)
        {
            _pendingPoint = point;
            SelectObject(point);
            StatusChanged?.Invoke(this, $"已选择起点 {point.Name}，请再选择终点。");
            Invalidate();
            return;
        }

        if (ReferenceEquals(_pendingPoint, point))
        {
            StatusChanged?.Invoke(this, "起点和终点不能是同一个点。");
            return;
        }

        switch (creation)
        {
            case ObservationCreation.Baseline:
                var baseline = _project.AddBaseline(_pendingPoint, point);
                SelectObject(baseline);
                StatusChanged?.Invoke(this, $"已创建基线 {baseline.Name}");
                break;
            case ObservationCreation.Height:
                var height = _project.AddHeightObservation(_pendingPoint, point, 0.0, 0.0);
                SelectObject(height);
                StatusChanged?.Invoke(this, $"已添加高差观测 {height.Name}，可在属性面板填写 Δh 和中误差。");
                break;
            case ObservationCreation.Distance:
                var distance = _project.AddDistanceObservation(_pendingPoint, point, 0.0);
                SelectObject(distance);
                StatusChanged?.Invoke(this, $"已添加距离观测 {distance.Name}，可在属性面板填写距离和中误差。");
                break;
            case ObservationCreation.Angle:
                if (_angleVertex is null)
                {
                    return;
                }

                var angle = _project.AddAngleObservation(_pendingPoint, _angleVertex, point);
                SelectObject(angle);
                StatusChanged?.Invoke(this, $"已添加距离观测 {angle.Name}，可在属性面板填写距离和中误差。");
                break;
            default:
                _project.AddLine(_pendingPoint, point);
                SelectObject(point);
                StatusChanged?.Invoke(this, $"已添加连线 {_pendingPoint.Name}-{point.Name}。");
                break;
        }

        _pendingPoint = null;
        ProjectChanged?.Invoke(this, EventArgs.Empty);
    }

    private SurveyPoint? HitTestPoint(Point location)
    {
        return _project.Points.LastOrDefault(point =>
        {
            var dx = point.CanvasLocation.X - location.X;
            var dy = point.CanvasLocation.Y - location.Y;
            return Math.Sqrt(dx * dx + dy * dy) <= PointRadius + 5;
        });
    }

    private object? HitTestObservation(Point location)
    {
        var distanceObs = _project.DistanceObservations.LastOrDefault(obs =>
            DistanceToSegment(location, obs.From.CanvasLocation, obs.To.CanvasLocation) <= 8.0);
        if (distanceObs is not null)
        {
            return distanceObs;
        }

        return _project.HeightObservations.LastOrDefault(obs =>
            DistanceToSegment(location, obs.From.CanvasLocation, obs.To.CanvasLocation) <= 8.0);
    }

    private static double DistanceToSegment(PointF point, PointF a, PointF b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        if (Math.Abs(dx) < 1e-6 && Math.Abs(dy) < 1e-6)
        {
            return Math.Sqrt(Math.Pow(point.X - a.X, 2) + Math.Pow(point.Y - a.Y, 2));
        }

        var t = ((point.X - a.X) * dx + (point.Y - a.Y) * dy) / (dx * dx + dy * dy);
        t = Math.Max(0, Math.Min(1, t));
        var x = a.X + t * dx;
        var y = a.Y + t * dy;
        return Math.Sqrt(Math.Pow(point.X - x, 2) + Math.Pow(point.Y - y, 2));
    }

    private static string GetObservationName(object value)
    {
        return value switch
        {
            HeightObservation observation => observation.Name,

            DistanceObservation observation => observation.Name,

            AngleObservation observation => observation.Name,

            _ => "",
        };
    }

    private bool IsBaseline(BoardLine line)
    {
        return _project.Baselines.Any(b => line.Connects(b.From, b.To));
    }

    private static void DrawGrid(Graphics g)
    {
        using var gridPen = new Pen(Color.FromArgb(225, 229, 229), 1);
        for (var x = 0; x < g.VisibleClipBounds.Width; x += 32)
        {
            g.DrawLine(gridPen, x, 0, x, g.VisibleClipBounds.Height);
        }

        for (var y = 0; y < g.VisibleClipBounds.Height; y += 32)
        {
            g.DrawLine(gridPen, 0, y, g.VisibleClipBounds.Width, y);
        }
    }

    private static void DrawBaseline(Graphics g, BoardLine line, bool selected)
    {
        var p1 = line.From.CanvasLocation;
        var p2 = line.To.CanvasLocation;

        var dx = p2.X - p1.X;
        var dy = p2.Y - p1.Y;

        var len =
            Math.Sqrt(
                dx * dx +
                dy * dy);

        if (len < 1e-6)
        {
            return;
        }

        dx /= (float)len;
        dy /= (float)len;

        float offset = 3f;

        var nx = -dy * offset;
        var ny = dx * offset;

        using var pen = new Pen(
            selected ? Color.FromArgb(31, 94, 184) : Color.Black,
            selected ? 3f : 2f);

        g.DrawLine(
            pen,
            p1.X + nx,
            p1.Y + ny,
            p2.X + nx,
            p2.Y + ny);

        g.DrawLine(
            pen,
            p1.X - nx,
            p1.Y - ny,
            p2.X - nx,
            p2.Y - ny);
    }

    private void DrawLines(Graphics g)
    {
        foreach (var line in _project.Lines)
        {
            var selected = IsLineSelected(line);

            using var linePen = new Pen(
                selected ? Color.FromArgb(31, 94, 184) : Color.FromArgb(36, 45, 52),
                selected ? 4F : 2.5F)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
            };

            if (IsBaseline(line))
            {
                DrawBaseline(g, line, selected);
            }
            else
            {
                g.DrawLine(linePen, line.From.CanvasLocation, line.To.CanvasLocation);
            }
        }
    }

    private bool IsLineSelected(BoardLine line)
{
    return SelectedObject switch
    {
        HeightObservation obs =>
            ReferenceEquals(obs.From, line.From)
            && ReferenceEquals(obs.To, line.To),

        DistanceObservation obs =>
            ReferenceEquals(obs.From, line.From)
            && ReferenceEquals(obs.To, line.To),

        AngleObservation obs =>
            (ReferenceEquals(obs.From, line.From)
             && ReferenceEquals(obs.Vertex, line.To))
            ||
            (ReferenceEquals(obs.Vertex, line.From)
             && ReferenceEquals(obs.To, line.To)),

        _ => false,
    };
}

    private void DrawObservationLabels(Graphics g)
    {
        using var font = new Font("Segoe UI", 11F, FontStyle.Italic);

        foreach (var obs in _project.HeightObservations)
        {
            DrawObservationLabel(g, font, obs.From, obs.To, obs.Name, 6, -18, ReferenceEquals(SelectedObject, obs));
        }

        foreach (var obs in _project.DistanceObservations)
        {
            DrawObservationLabel(g, font, obs.From, obs.To, obs.Name, 6, 4, ReferenceEquals(SelectedObject, obs));
        }

        foreach (var obs in _project.AngleObservations)
        {
            DrawAngelObsLabel(g, font, obs.Vertex, obs.Name, 10, 0, ReferenceEquals(SelectedObject, obs));
        }

    }

    private static void DrawObservationLabel(
        Graphics g,
        Font font,
        SurveyPoint from,
        SurveyPoint to,
        string label,
        float offsetX,
        float offsetY,
        bool selected)
    {
        var mid = new PointF(
            (from.CanvasLocation.X + to.CanvasLocation.X) / 2F,
            (from.CanvasLocation.Y + to.CanvasLocation.Y) / 2F);
        using var brush = new SolidBrush(selected ? Color.FromArgb(31, 94, 184) : Color.FromArgb(30, 36, 42));
        g.DrawString(label, font, brush, mid.X + offsetX, mid.Y + offsetY);
    }

    private static void DrawAngelObsLabel(
    Graphics g,
    Font font,
    SurveyPoint vertex,
    string label,
    float offsetX,
    float offsetY,
    bool selected)
    {
        var mid = new PointF(vertex.CanvasLocation.X, vertex.CanvasLocation.Y);
        using var brush = new SolidBrush(selected ? Color.FromArgb(31, 94, 184) : Color.FromArgb(30, 36, 42));
        g.DrawString(label, font, brush, mid.X + offsetX, mid.Y + offsetY);
    }

    private void DrawPoints(Graphics g)
    {
        foreach (var point in _project.Points)
        {
            var center = point.CanvasLocation;
            var isPending = ReferenceEquals(point, _pendingPoint);
            var isSelected = ReferenceEquals(point, SelectedObject);

            using var fill = new SolidBrush(point.IsCoordinateFixed
                ? Color.FromArgb(36, 78, 160)
                : point.IsHeightFixed
                    ? Color.FromArgb(20, 83, 45)
                    : Color.White);
            using var outline = new Pen(
                isPending ? Color.FromArgb(220, 120, 20) : isSelected ? Color.FromArgb(31, 94, 184) : Color.Black,
                isPending || isSelected ? 3.2F : 2F);
            var rect = new RectangleF(center.X - PointRadius, center.Y - PointRadius, PointRadius * 2, PointRadius * 2);
            g.FillEllipse(fill, rect);
            g.DrawEllipse(outline, rect);

            if (point.IsHeightFixed || point.IsCoordinateFixed)
            {
                using var knownPen = new Pen(isSelected ? Color.FromArgb(31, 94, 184) : Color.Black, 2F);
                g.DrawEllipse(knownPen, center.X - PointRadius - 6, center.Y - PointRadius - 6, (PointRadius + 6) * 2, (PointRadius + 6) * 2);
            }

            using var nameFont = new Font("Segoe UI", 13F, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.Black);
            g.DrawString(point.Name, nameFont, textBrush, center.X + 12, center.Y + 8);

            using var valueFont = new Font("Segoe UI", 8.5F);
            if (point.IsCoordinateFixed && point.X.HasValue && point.Y.HasValue)
            {
                g.DrawString($"X={point.X.Value:F2}, Y={point.Y.Value:F2}", valueFont, textBrush, center.X + 12, center.Y - 28);
            }

            if (point.Height.HasValue)
            {
                g.DrawString($"H={point.Height.Value:F3}", valueFont, textBrush, center.X + 12, center.Y - 14);
            }
        }
    }

    private void DrawAngles(Graphics g)
    {
        if (_project.AngleObservations == null)
            return;

        foreach (var angle in _project.AngleObservations)
        {
            bool selected = SelectedObject == angle;

            var v = angle.Vertex.CanvasLocation;
            var a = angle.From.CanvasLocation;
            var c = angle.To.CanvasLocation;

            float radius = 18f;

            double a1 = Math.Atan2(a.Y - v.Y, a.X - v.X);
            double a2 = Math.Atan2(c.Y - v.Y, c.X - v.X);

            double start = a1 * 180.0 / Math.PI;
            double sweep = (a2 - a1) * 180.0 / Math.PI;

            if (sweep < 0) sweep += 360;

            using var pen = new Pen(
                selected ? Color.Red : Color.Orange,
                selected ? 3f : 2f);

            g.DrawArc(
                pen,
                v.X - radius,
                v.Y - radius,
                radius * 2,
                radius * 2,
                (float)start,
                (float)sweep);
        }
    }

    private AngleObservation? HitTestAngle(PointF p)
    {
        if (_project.AngleObservations == null)
            return null;

        foreach (var angle in _project.AngleObservations)
        {
            var v = angle.Vertex.CanvasLocation;

            float dx = p.X - v.X;
            float dy = p.Y - v.Y;

            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            // 和点一样：靠近测站点即可选中
            if (dist <= 20f)
                return angle;
        }

        return null;
    }

    private enum ObservationCreation
    {
        None,
        Baseline,
        Height,
        Distance,
        Angle
        
    }
}
