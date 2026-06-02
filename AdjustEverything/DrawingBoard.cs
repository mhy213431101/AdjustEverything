using System.Drawing.Drawing2D;

namespace AdjustEverything;

// 自绘画板控件。它只负责交互和显示，真正的数据保存在 AdjustmentProject 中。
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

        // 绘制顺序很重要：先网格，再线和观测标注，最后画点，保证点始终在最上层。
        DrawGrid(g);
        DrawLines(g);
        DrawObservationLabels(g);
        DrawPoints(g);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();

        // 点击优先级：点 > 观测线 > 空白区域。这样拖动点时不会被线段误选中。
        var point = HitTestPoint(e.Location);
        if (point is not null)
        {
            HandlePointClick(point, e.Location);
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
            // 非已知坐标点拖动后，同步更新近似坐标，给测边网迭代提供初值。
            _draggingPoint.X = _draggingPoint.CanvasLocation.X;
            _draggingPoint.Y = _draggingPoint.CanvasLocation.Y;
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
            case ToolMode.AddHeight:
                ConnectPoint(point, ObservationCreation.Height);
                break;
            case ToolMode.AddDistance:
                ConnectPoint(point, ObservationCreation.Distance);
                break;
            case ToolMode.FixedHeight:
                SelectObject(point);
                point.IsHeightFixed = true;
                point.Height ??= 0.0;
                ProjectChanged?.Invoke(this, EventArgs.Empty);
                StatusChanged?.Invoke(this, $"点 {point.Name} 已设为已知高程点，可在属性面板修改高程。");
                break;
            case ToolMode.FixedCoordinate:
                SelectObject(point);
                point.IsCoordinateFixed = true;
                point.X ??= point.CanvasLocation.X;
                point.Y ??= point.CanvasLocation.Y;
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

    private void ConnectPoint(SurveyPoint point, ObservationCreation creation)
    {
        if (_pendingPoint is null)
        {
            // 高差/距离/普通线都采用“两次点选”的创建方式：先记住起点，再等终点。
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
            case ObservationCreation.Height:
                var height = _project.AddHeightObservation(_pendingPoint, point, 0.0);
                SelectObject(height);
                StatusChanged?.Invoke(this, $"已添加高差观测 {height.Name}，可在属性面板填写 Δh 和中误差。");
                break;
            case ObservationCreation.Distance:
                var distance = _project.AddDistanceObservation(_pendingPoint, point, 0.0);
                SelectObject(distance);
                StatusChanged?.Invoke(this, $"已添加距离观测 {distance.Name}，可在属性面板填写距离和中误差。");
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
        // 距离观测后画在标签下方，命中时优先选距离观测，便于同一条线上同时存在 h 和 s。
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
        // 用点到线段的距离做命中测试，比只点标签更容易选中观测。
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
            _ => "",
        };
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
            var selected = IsLineSelected(line);

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

    private bool IsLineSelected(BoardLine line)
    {
        return SelectedObject switch
        {
            HeightObservation obs => ReferenceEquals(obs.From, line.From) && ReferenceEquals(obs.To, line.To),
            DistanceObservation obs => ReferenceEquals(obs.From, line.From) && ReferenceEquals(obs.To, line.To),
            _ => false,
        };
    }

    private void DrawObservationLabels(Graphics g)
    {
        using var font = new Font("Segoe UI", 11F, FontStyle.Italic);

        foreach (var obs in _project.HeightObservations)
        {
            DrawObservationLabel(g, font, obs.From, obs.To, obs.Name, -18, ReferenceEquals(SelectedObject, obs));
        }

        foreach (var obs in _project.DistanceObservations)
        {
            DrawObservationLabel(g, font, obs.From, obs.To, obs.Name, 4, ReferenceEquals(SelectedObject, obs));
        }
    }

    private static void DrawObservationLabel(
        Graphics g,
        Font font,
        SurveyPoint from,
        SurveyPoint to,
        string label,
        float offsetY,
        bool selected)
    {
        var mid = new PointF(
            (from.CanvasLocation.X + to.CanvasLocation.X) / 2F,
            (from.CanvasLocation.Y + to.CanvasLocation.Y) / 2F);
        using var brush = new SolidBrush(selected ? Color.FromArgb(31, 94, 184) : Color.FromArgb(30, 36, 42));
        g.DrawString(label, font, brush, mid.X + 6, mid.Y + offsetY);
    }

    private void DrawPoints(Graphics g)
    {
        foreach (var point in _project.Points)
        {
            var center = point.CanvasLocation;
            var isPending = ReferenceEquals(point, _pendingPoint);
            var isSelected = ReferenceEquals(point, SelectedObject);

            // 蓝色表示已知平面坐标点，绿色表示已知高程点，白色表示普通未知点。
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

    private enum ObservationCreation
    {
        None,
        Height,
        Distance,
    }
}
