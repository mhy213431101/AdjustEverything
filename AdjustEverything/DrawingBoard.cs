using System.Drawing.Drawing2D;

namespace AdjustEverything;

internal sealed class DrawingBoard : Control
{
    private const float PointRadius = 9F;
    private readonly AdjustmentProject _project;
    private SurveyPoint? _pendingPoint;
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
        DrawHeightLabels(g);
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

        var observation = HitTestHeightObservation(e.Location);
        if (observation is not null)
        {
            SelectObject(observation);
            StatusChanged?.Invoke(this, $"选中高差观测 {observation.Name}。");
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
            Cursor = HitTestPoint(e.Location) is null && HitTestHeightObservation(e.Location) is null
                ? Cursors.Cross
                : Cursors.Hand;
            return;
        }

        _draggingPoint.CanvasLocation = new PointF(e.X + _dragOffset.X, e.Y + _dragOffset.Y);
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
                ConnectPoint(point, asHeightObservation: false);
                break;
            case ToolMode.AddHeight:
                ConnectPoint(point, asHeightObservation: true);
                break;
            case ToolMode.FixedHeight:
                SelectObject(point);
                point.IsHeightFixed = true;
                if (!point.Height.HasValue)
                {
                    point.Height = 0.0;
                }
                ProjectChanged?.Invoke(this, EventArgs.Empty);
                StatusChanged?.Invoke(this, $"点 {point.Name} 已设为已知高程点，可在属性面板修改高程。");
                break;
            default:
                SelectObject(point);
                _draggingPoint = point;
                _dragOffset = new PointF(point.CanvasLocation.X - location.X, point.CanvasLocation.Y - location.Y);
                StatusChanged?.Invoke(this, $"选中点 {point.Name}。拖动可调整画板位置。");
                break;
        }
    }

    private void ConnectPoint(SurveyPoint point, bool asHeightObservation)
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

        if (!asHeightObservation)
        {
            _project.AddLine(_pendingPoint, point);
            StatusChanged?.Invoke(this, $"已添加连线 {_pendingPoint.Name}-{point.Name}。");
            SelectObject(point);
        }
        else
        {
            var observation = _project.AddHeightObservation(_pendingPoint, point, 0.0);
            SelectObject(observation);
            StatusChanged?.Invoke(this, $"已添加高差观测 {observation.Name}，可在属性面板填写 Δh 和中误差。");
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

    private HeightObservation? HitTestHeightObservation(Point location)
    {
        return _project.HeightObservations.LastOrDefault(obs =>
        {
            var distance = DistanceToSegment(location, obs.From.CanvasLocation, obs.To.CanvasLocation);
            return distance <= 8.0;
        });
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

    private void DrawLines(Graphics g)
    {
        foreach (var line in _project.Lines)
        {
            var selected = _project.HeightObservations.Any(obs =>
                ReferenceEquals(SelectedObject, obs)
                && ReferenceEquals(obs.From, line.From)
                && ReferenceEquals(obs.To, line.To));

            using var linePen = new Pen(
                selected ? Color.FromArgb(31, 94, 184) : Color.FromArgb(36, 45, 52),
                selected ? 4F : 2.5F)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
            };

            g.DrawLine(linePen, line.From.CanvasLocation, line.To.CanvasLocation);
        }
    }

    private void DrawHeightLabels(Graphics g)
    {
        using var font = new Font("Segoe UI", 11F, FontStyle.Italic);

        foreach (var obs in _project.HeightObservations)
        {
            var mid = new PointF(
                (obs.From.CanvasLocation.X + obs.To.CanvasLocation.X) / 2F,
                (obs.From.CanvasLocation.Y + obs.To.CanvasLocation.Y) / 2F);
            var selected = ReferenceEquals(SelectedObject, obs);
            using var brush = new SolidBrush(selected ? Color.FromArgb(31, 94, 184) : Color.FromArgb(30, 36, 42));
            g.DrawString(obs.Name, font, brush, mid.X + 6, mid.Y - 18);
        }
    }

    private void DrawPoints(Graphics g)
    {
        foreach (var point in _project.Points)
        {
            var center = point.CanvasLocation;
            var isPending = ReferenceEquals(point, _pendingPoint);
            var isSelected = ReferenceEquals(point, SelectedObject);

            using var fill = new SolidBrush(point.IsHeightFixed
                ? Color.FromArgb(20, 83, 45)
                : Color.White);
            using var outline = new Pen(
                isPending ? Color.FromArgb(220, 120, 20) : isSelected ? Color.FromArgb(31, 94, 184) : Color.Black,
                isPending || isSelected ? 3.2F : 2F);
            var rect = new RectangleF(center.X - PointRadius, center.Y - PointRadius, PointRadius * 2, PointRadius * 2);
            g.FillEllipse(fill, rect);
            g.DrawEllipse(outline, rect);

            if (point.IsHeightFixed)
            {
                using var knownPen = new Pen(isSelected ? Color.FromArgb(31, 94, 184) : Color.Black, 2F);
                g.DrawEllipse(knownPen, center.X - PointRadius - 6, center.Y - PointRadius - 6, (PointRadius + 6) * 2, (PointRadius + 6) * 2);
            }

            using var nameFont = new Font("Segoe UI", 13F, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.Black);
            g.DrawString(point.Name, nameFont, textBrush, center.X + 12, center.Y + 8);

            if (point.Height.HasValue)
            {
                using var heightFont = new Font("Segoe UI", 8.5F);
                g.DrawString($"H={point.Height.Value:F3}", heightFont, textBrush, center.X + 12, center.Y - 14);
            }
        }
    }
}
