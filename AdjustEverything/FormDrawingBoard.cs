using System.Text;

namespace AdjustEverything;

public partial class FormDrawingBoard : Form
{
    private readonly AdjustmentProject _project = new();
    private readonly DrawingBoard _board;
    private readonly ListBox _objectList = new();
    private readonly TextBox _resultBox = new();
    private readonly Panel _propertyPanel = new();
    private readonly Label _statusLabel = new();
    private bool _syncingObjectList;

    public FormDrawingBoard()
    {
        InitializeComponent();

        Text = "AdjustEverything - 平差建模原型";
        MinimumSize = new Size(1180, 720);
        Size = new Size(1380, 820);
        Font = new Font("Microsoft YaHei UI", 10F);
        KeyPreview = true;
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Delete && ActiveControl is not TextBoxBase)
            {
                DeleteSelectedObject();
                e.Handled = true;
            }
        };

        _board = new DrawingBoard(_project)
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(238, 241, 241),
        };
        _board.ProjectChanged += (_, _) =>
        {
            RefreshProjectViews();
            ShowSelectionProperties(_board.SelectedObject);
        };
        _board.StatusChanged += (_, text) => _statusLabel.Text = text;
        _board.SelectionChanged += (_, selected) =>
        {
            SyncObjectListSelection(selected);
            ShowSelectionProperties(selected);
        };

        _objectList.SelectedIndexChanged += (_, _) =>
        {
            if (_syncingObjectList || _objectList.SelectedItem is not ObjectListItem item)
            {
                return;
            }

            _board.SelectObject(item.Value);
        };

        BuildLayout();
        LoadSampleNetwork();
        SetMode(ToolMode.AddPoint);
    }

    private void BuildLayout()
    {
        Controls.Clear();

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(14),
            BackColor = Color.FromArgb(224, 229, 229),
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(root);

        root.Controls.Add(BuildToolPanel(), 0, 0);

        var workArea = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
        };
        workArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));
        workArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        workArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        root.Controls.Add(workArea, 1, 0);

        var top = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(12, 0, 0, 12),
        };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        workArea.Controls.Add(top, 0, 0);

        top.Controls.Add(BuildGroup("已添加对象", _objectList), 0, 0);
        top.Controls.Add(BuildGroup("属性", _propertyPanel), 1, 0);

        _resultBox.Multiline = true;
        _resultBox.ReadOnly = true;
        _resultBox.ScrollBars = ScrollBars.Vertical;
        _resultBox.BorderStyle = BorderStyle.None;
        _resultBox.BackColor = Color.White;
        _resultBox.Font = new Font("Consolas", 9.5F);
        top.Controls.Add(BuildGroup("平差结果", _resultBox), 2, 0);

        var boardPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 0, 0, 0),
        };
        workArea.Controls.Add(boardPanel, 0, 1);

        var boardFrame = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.White,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
        };
        boardFrame.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        boardFrame.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        boardPanel.Controls.Add(boardFrame);

        var boardHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        var title = new Label
        {
            Text = "画板",
            Dock = DockStyle.Left,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Width = 140,
            Padding = new Padding(12, 0, 0, 0),
            Font = new Font(Font, FontStyle.Bold),
        };
        var distanceCalculate = new Button
        {
            Text = "▷ 测边计算",
            Dock = DockStyle.Right,
            Width = 150,
            FlatStyle = FlatStyle.Flat,
        };
        distanceCalculate.Click += (_, _) => RunDistanceAdjustment();
        var heightCalculate = new Button
        {
            Text = "▷ 高程计算",
            Dock = DockStyle.Right,
            Width = 150,
            FlatStyle = FlatStyle.Flat,
        };
        heightCalculate.Click += (_, _) => RunHeightAdjustment();
        boardHeader.Controls.Add(distanceCalculate);
        boardHeader.Controls.Add(heightCalculate);
        boardHeader.Controls.Add(title);
        boardFrame.Controls.Add(boardHeader, 0, 0);
        boardFrame.Controls.Add(_board, 0, 1);

        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.Padding = new Padding(14, 0, 0, 0);
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        workArea.Controls.Add(_statusLabel, 0, 2);
    }

    private Control BuildToolPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            BackColor = Color.White,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
            Padding = new Padding(12),
        };

        AddToolButton(panel, "◎ 已知高程", ToolMode.FixedHeight);
        AddToolButton(panel, "◎ 已知坐标", ToolMode.FixedCoordinate);
        AddToolButton(panel, "━ 基线", ToolMode.Select);
        AddToolButton(panel, "○ 添加点", ToolMode.AddPoint);
        AddToolButton(panel, "─ 添加线", ToolMode.AddLine);
        AddToolButton(panel, "(x,y) 添加坐标", ToolMode.FixedCoordinate);
        AddToolButton(panel, "∠α 添加角", ToolMode.Select);
        AddToolButton(panel, "↔ 添加距离", ToolMode.AddDistance);
        AddToolButton(panel, "↕ 添加高程", ToolMode.AddHeight);

        var sample = BuildSideButton("载入示例网");
        sample.Click += (_, _) =>
        {
            LoadSampleNetwork();
            SetMode(ToolMode.AddPoint);
        };
        panel.Controls.Add(sample);

        var heightCheck = BuildSideButton("检查高程网");
        heightCheck.Click += (_, _) => RunHeightNetworkCheck();
        panel.Controls.Add(heightCheck);

        var distanceCheck = BuildSideButton("检查测边网");
        distanceCheck.Click += (_, _) => RunDistanceNetworkCheck();
        panel.Controls.Add(distanceCheck);

        var delete = BuildSideButton("删除所选");
        delete.Click += (_, _) => DeleteSelectedObject();
        panel.Controls.Add(delete);

        var clear = BuildSideButton("清空");
        clear.Click += (_, _) =>
        {
            _project.Clear();
            _board.ClearSelection();
            RefreshProjectViews();
            _resultBox.Clear();
        };
        panel.Controls.Add(clear);

        return panel;
    }

    private static Button BuildSideButton(string text)
    {
        return new Button
        {
            Text = text,
            Dock = DockStyle.Top,
            Height = 44,
            FlatStyle = FlatStyle.Flat,
        };
    }

    private static Control BuildGroup(string title, Control content)
    {
        var group = new GroupBox
        {
            Text = title,
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
        };
        content.Dock = DockStyle.Fill;
        group.Controls.Add(content);
        return group;
    }

    private void AddToolButton(TableLayoutPanel panel, string text, ToolMode mode)
    {
        var button = new Button
        {
            Text = text,
            Tag = mode,
            Dock = DockStyle.Top,
            Height = 48,
            TextAlign = ContentAlignment.MiddleLeft,
            FlatStyle = FlatStyle.Flat,
            Padding = new Padding(12, 0, 0, 0),
        };
        button.Click += (_, _) => SetMode(mode);
        panel.Controls.Add(button);
    }

    private void SetMode(ToolMode mode)
    {
        _board.Mode = mode;

        foreach (var button in GetAllControls(this).OfType<Button>())
        {
            if (button.Tag is not ToolMode buttonMode)
            {
                continue;
            }

            button.BackColor = buttonMode == mode
                ? Color.FromArgb(218, 232, 252)
                : Color.White;
        }

        _statusLabel.Text = mode switch
        {
            ToolMode.AddPoint => "添加点：在画板空白处单击。",
            ToolMode.AddLine => "添加线：依次单击两个点，创建普通连线。",
            ToolMode.AddHeight => "添加高程：依次单击两个点，创建高差观测；数值在属性面板填写。",
            ToolMode.AddDistance => "添加距离：依次单击两个点，创建距离观测；数值在属性面板填写。",
            ToolMode.FixedHeight => "已知高程：单击点设为已知高程点；高程在属性面板修改。",
            ToolMode.FixedCoordinate => "已知坐标：单击点设为已知平面点；X/Y 在属性面板修改。",
            _ => "选择模式：单击点或观测查看属性。"
        };
    }

    private static IEnumerable<Control> GetAllControls(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;
            foreach (var nested in GetAllControls(child))
            {
                yield return nested;
            }
        }
    }

    private void LoadSampleNetwork()
    {
        _project.Clear();

        var a = _project.AddPoint("A", new PointF(250, 250));
        var b = _project.AddPoint("B", new PointF(640, 250));
        var c = _project.AddPoint("C", new PointF(455, 120));
        var d = _project.AddPoint("D", new PointF(420, 390));

        a.IsHeightFixed = true;
        a.Height = 100.000;
        b.IsHeightFixed = true;
        b.Height = 101.230;

        a.IsCoordinateFixed = true;
        a.X = 0.000;
        a.Y = 0.000;
        b.IsCoordinateFixed = true;
        b.X = 0.000;
        b.Y = 300.000;
        c.X = -180.000;
        c.Y = 80.000;
        d.X = 250.000;
        d.Y = 220.000;

        _project.AddHeightObservation(a, c, 1.215, 1.8);
        _project.AddHeightObservation(a, d, 0.385, 1.7);
        _project.AddHeightObservation(d, c, 0.821, 2.0);
        _project.AddHeightObservation(c, b, 0.028, 2.3);
        _project.AddHeightObservation(d, b, 0.842, 1.7);

        _project.AddDistanceObservation(a, c, 196.977);
        _project.AddDistanceObservation(c, b, 284.250);
        _project.AddDistanceObservation(a, d, 333.017);
        _project.AddDistanceObservation(d, b, 262.489);
        _project.AddDistanceObservation(c, d, 452.214);

        _board.ClearSelection();
        RefreshProjectViews();
        ShowSelectionProperties(null);
        _resultBox.Clear();
    }

    private void RefreshProjectViews()
    {
        var selected = _board.SelectedObject;
        _syncingObjectList = true;
        _objectList.Items.Clear();

        foreach (var point in _project.Points)
        {
            _objectList.Items.Add(new ObjectListItem(point));
        }

        foreach (var obs in _project.HeightObservations)
        {
            _objectList.Items.Add(new ObjectListItem(obs));
        }

        foreach (var obs in _project.DistanceObservations)
        {
            _objectList.Items.Add(new ObjectListItem(obs));
        }

        _syncingObjectList = false;
        SyncObjectListSelection(selected);
        _board.Invalidate();
    }

    private void SyncObjectListSelection(object? selected)
    {
        _syncingObjectList = true;
        _objectList.ClearSelected();

        for (var i = 0; i < _objectList.Items.Count; i++)
        {
            if (_objectList.Items[i] is ObjectListItem item && ReferenceEquals(item.Value, selected))
            {
                _objectList.SelectedIndex = i;
                break;
            }
        }

        _syncingObjectList = false;
    }

    private void ShowSelectionProperties(object? selected)
    {
        _propertyPanel.Controls.Clear();

        switch (selected)
        {
            case SurveyPoint point:
                BuildPointProperties(point);
                break;
            case HeightObservation observation:
                BuildHeightObservationProperties(observation);
                break;
            case DistanceObservation observation:
                BuildDistanceObservationProperties(observation);
                break;
            default:
                _propertyPanel.Controls.Add(new Label
                {
                    Text = "未选中对象。\r\n\r\n在画板或对象列表中选择点、高差观测、距离观测后，可在这里编辑参数。",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                });
                break;
        }
    }

    private void BuildPointProperties(SurveyPoint point)
    {
        var table = BuildPropertyTable();
        var nameBox = AddTextRow(table, "点名", point.Name);

        var fixedHeightCheck = AddCheckRow(table, "作为已知高程点", point.IsHeightFixed);
        var heightBox = AddTextRow(table, "高程 H(m)", point.Height?.ToString("F4") ?? "");

        var fixedCoordinateCheck = AddCheckRow(table, "作为已知平面坐标点", point.IsCoordinateFixed);
        var xBox = AddTextRow(table, "X", point.X?.ToString("F4") ?? "");
        var yBox = AddTextRow(table, "Y", point.Y?.ToString("F4") ?? "");

        var apply = AddApplyButton(table);
        var delete = AddDeleteButton(table);
        apply.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(nameBox.Text))
            {
                MessageBox.Show("点名不能为空。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryReadNullableDouble(heightBox.Text, "高程 H(m)", out var height)
                || !TryReadNullableDouble(xBox.Text, "X", out var x)
                || !TryReadNullableDouble(yBox.Text, "Y", out var y))
            {
                return;
            }

            point.Name = nameBox.Text.Trim();
            point.IsHeightFixed = fixedHeightCheck.Checked;
            point.Height = height;
            point.IsCoordinateFixed = fixedCoordinateCheck.Checked;
            point.X = x;
            point.Y = y;
            RefreshProjectViews();
            _board.SelectObject(point);
            _statusLabel.Text = $"点 {point.Name} 的属性已更新。";
        };
        delete.Click += (_, _) => DeleteSelectedObject();

        _propertyPanel.Controls.Add(table);
    }

    private void BuildHeightObservationProperties(HeightObservation observation)
    {
        var table = BuildPropertyTable();
        var nameBox = AddTextRow(table, "观测名", observation.Name);
        AddReadonlyRow(table, "起点", observation.From.Name);
        AddReadonlyRow(table, "终点", observation.To.Name);
        var valueBox = AddTextRow(table, "高差 Δh(m)", observation.Value.ToString("F4"));
        var lengthBox = AddTextRow(table, "测段长度 S(km)", observation.Length.ToString("F4"));
        var sigmaBox = AddTextRow(table, "中误差 m(可选)", observation.Sigma.ToString("F4"));
        var apply = AddApplyButton(table);
        var delete = AddDeleteButton(table);

        apply.Click += (_, _) =>
        {
            if (!ReadObservationHeight(nameBox, valueBox, lengthBox, sigmaBox, "高差 Δh(m)", out var name, out var value, out var length, out var sigma))
            {
                return;
            }

            observation.Name = name;
            observation.Value = value;
            observation.Length = length;
            observation.Sigma = sigma;
            RefreshProjectViews();
            _board.SelectObject(observation);
            _statusLabel.Text = $"高差观测 {observation.Name} 的属性已更新。";
        };
        delete.Click += (_, _) => DeleteSelectedObject();

        _propertyPanel.Controls.Add(table);
    }

    private void BuildDistanceObservationProperties(DistanceObservation observation)
    {
        var table = BuildPropertyTable();
        var nameBox = AddTextRow(table, "观测名", observation.Name);
        AddReadonlyRow(table, "起点", observation.From.Name);
        AddReadonlyRow(table, "终点", observation.To.Name);
        var valueBox = AddTextRow(table, "距离 S(m)", observation.Value.ToString("F4"));
        var sigmaBox = AddTextRow(table, "中误差 σ", observation.Sigma.ToString("F4"));
        var apply = AddApplyButton(table);
        var delete = AddDeleteButton(table);

        apply.Click += (_, _) =>
        {
            if (!ReadObservationDistance(nameBox, valueBox, sigmaBox, "距离 S(m)", out var name, out var value, out var sigma))
            {
                return;
            }

            observation.Name = name;
            observation.Value = value;
            observation.Sigma = sigma;
            RefreshProjectViews();
            _board.SelectObject(observation);
            _statusLabel.Text = $"距离观测 {observation.Name} 的属性已更新。";
        };
        delete.Click += (_, _) => DeleteSelectedObject();

        _propertyPanel.Controls.Add(table);
    }

    private static TableLayoutPanel BuildPropertyTable()
    {
        return new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoScroll = true,
        };
    }

    private static CheckBox AddCheckRow(TableLayoutPanel table, string text, bool checkedValue)
    {
        var check = new CheckBox
        {
            Text = text,
            Checked = checkedValue,
            Dock = DockStyle.Fill,
        };
        var row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        table.Controls.Add(check, 0, row);
        table.SetColumnSpan(check, 2);
        return check;
    }

    private static TextBox AddTextRow(TableLayoutPanel table, string label, string value)
    {
        var box = new TextBox { Text = value, Dock = DockStyle.Fill };
        AddRow(table, label, box);
        return box;
    }

    private static void AddReadonlyRow(TableLayoutPanel table, string label, string value)
    {
        AddRow(table, label, new Label
        {
            Text = value,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
        });
    }

    private static void AddRow(TableLayoutPanel table, string label, Control control)
    {
        var row = table.RowCount++;

        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var lbl = new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = true,
            MaximumSize = new Size(0, 0)
        };

        table.Controls.Add(lbl, 0, row);
        table.Controls.Add(control, 1, row);
    }

    private static Button AddApplyButton(TableLayoutPanel table)
    {
        var button = new Button
        {
            Text = "应用",
            Dock = DockStyle.Top,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
        };
        var row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        table.Controls.Add(button, 0, row);
        table.SetColumnSpan(button, 2);
        return button;
    }

    private static Button AddDeleteButton(TableLayoutPanel table)
    {
        var button = new Button
        {
            Text = "删除所选对象",
            Dock = DockStyle.Top,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.FromArgb(150, 35, 35),
        };
        var row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        table.Controls.Add(button, 0, row);
        table.SetColumnSpan(button, 2);
        return button;
    }

    private static bool ReadObservationHeight(
        TextBox nameBox,
        TextBox valueBox,
        TextBox lengthBox,
        TextBox sigmaBox,
        string valueLabel,
        out string name,
        out double value,
        out double length,
        out double sigma)
    {
        name = nameBox.Text.Trim();
        value = 0;
        length = 0;
        sigma = 0;

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("观测名不能为空。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (!double.TryParse(valueBox.Text, out value))
        {
            MessageBox.Show($"{valueLabel} 必须是数字。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (!double.TryParse(lengthBox.Text, out length) || length <= 0)
        {
            MessageBox.Show($"测段长度 S 必须是大于 0 的数字。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (!double.TryParse(sigmaBox.Text, out sigma) || sigma <= 0)
        {
            MessageBox.Show("中误差 m 必须是大于 0 的数字。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    private static bool ReadObservationDistance(
    TextBox nameBox,
    TextBox valueBox,
    TextBox sigmaBox,
    string valueLabel,
    out string name,
    out double value,
    out double sigma)
    {
        name = nameBox.Text.Trim();
        value = 0;
        sigma = 0;

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("观测名不能为空。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (!double.TryParse(valueBox.Text, out value))
        {
            MessageBox.Show($"{valueLabel} 必须是数字。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (!double.TryParse(sigmaBox.Text, out sigma) || sigma <= 0)
        {
            MessageBox.Show("中误差 m 必须是大于 0 的数字。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    private static bool TryReadNullableDouble(string text, string label, out double? value)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            value = null;
            return true;
        }

        if (double.TryParse(text, out var parsed))
        {
            value = parsed;
            return true;
        }

        MessageBox.Show($"{label} 必须为空或数字。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        value = null;
        return false;
    }

    private void RunHeightAdjustment()
    {
        var validation = ProjectDiagnostics.ValidateHeightNetwork(_project);

        if (validation.HasErrors)
        {
            _resultBox.Text = validation.ToReport("高程网检查");
            _statusLabel.Text = "高程网检查未通过，请根据结果面板中的错误修改模型。";
            return;
        }

        //1. 构造输入数据
        var fixedPoints = _project.Points
            .Where(p => p.IsHeightFixed && p.Height.HasValue)
            .ToList();

        var unknownPoints = _project.Points
            .Where(p => !p.IsHeightFixed)
            .ToList();

        var observations = _project.HeightObservations.ToList();

        var index = unknownPoints
            .Select((p, i) => (p, i))
            .ToDictionary(x => x.p, x => x.i);

        var X0 = new double[unknownPoints.Count];

        for (int i = 0; i < unknownPoints.Count; i++)
        {
            X0[i] = unknownPoints[i].Height ?? 0.0;
        }

        //2. 导入模型
        var model = new HeightModel(
            unknownPoints,
            observations,
            index,
            X0);

        //3. 完成平差
        var result = NonlinearLeastSquaresSolver.Solve(model, model);

        //4. 精度评定
        var precision = PrecisionEstimator.Estimate(result.LS);

        //5. 报告生成
        var sb = new StringBuilder();

        sb.AppendLine(validation.ToReport("高程网平差结果"));

        sb.AppendLine("已知点信息：");
        foreach (var point in fixedPoints)
        {
            point.CurrentDisplayMode = DisplayMode.Height;
            sb.AppendLine(point.ToString());
        }
        sb.AppendLine();

        sb.AppendLine("未知点信息：");
        foreach (var point in unknownPoints)
        {
            sb.AppendLine(point.ToString());
        }
        sb.AppendLine();

        sb.AppendLine("观测信息：");
        foreach (var point in observations)
        {
            sb.AppendLine(point.ToString());
        }
        sb.AppendLine();

        sb.AppendLine($"观测数 n = {observations.Count}");
        sb.AppendLine($"未知数 t = {unknownPoints.Count}");
        sb.AppendLine($"多余观测 r = {observations.Count - unknownPoints.Count}");

        sb.AppendLine();
        sb.AppendLine("未知点高程 H(m)：");
        for (int i = 0; i < unknownPoints.Count; i++)
        {
            sb.AppendLine(
                $"{unknownPoints[i].Name,-4}  " +
                $"{result.XHat[i]:F4}");
        }

        sb.AppendLine();
        sb.AppendLine("观测改正数 v(mm)：");

        for (int i = 0; i < result.LS.V.Length; i++)
        {
            sb.AppendLine(
                $"{observations[i].Name,-4}  " +
                $"{1000 * result.LS.V[i]:F1}");
        }

        sb.AppendLine(precision.Report);

        _resultBox.Text = sb.ToString();

        if (result.Success)
        {
            for (int i = 0; i < unknownPoints.Count; i++)
            {
                unknownPoints[i].Height = result.XHat[i];
            }
        }

        RefreshProjectViews();
        ShowSelectionProperties(_board.SelectedObject);
    }

    private void RunDistanceAdjustment()
    {
        var validation =
            ProjectDiagnostics.ValidateDistanceNetwork(_project);

        if (validation.HasErrors)
        {
            _resultBox.Text =
                validation.ToReport("测边网检查");

            _statusLabel.Text =
                "测边网检查未通过，请根据结果面板中的错误修改模型。";

            return;
        }

        var fixedPoints =
            _project.Points
                .Where(p =>
                    p.IsCoordinateFixed &&
                    p.X.HasValue &&
                    p.Y.HasValue)
                .ToList();

        var unknownPoints =
            _project.Points
                .Where(p => !p.IsCoordinateFixed)
                .ToList();

        var observations =
            _project.DistanceObservations
                .ToList();

        var parameterIndex = new Dictionary<SurveyPoint, int>();

        for (int i = 0; i < unknownPoints.Count; i++)
        {
            parameterIndex[unknownPoints[i]] = 2 * i;
        }

        var x0 = new double[unknownPoints.Count * 2];

        for (int i = 0; i < unknownPoints.Count; i++)
        {
            var point = unknownPoints[i];

            x0[2 * i] =
                point.X
                ?? point.CanvasLocation.X;

            x0[2 * i + 1] =
                point.Y
                ?? point.CanvasLocation.Y;
        }

        var model =
            new DistanceModel(
                unknownPoints,
                observations,
                parameterIndex,
                x0);

        var result = NonlinearLeastSquaresSolver.Solve(model, model);

        var precision = PrecisionEstimator.Estimate(result.LS);
        
        var sb = new StringBuilder();

        sb.AppendLine(validation.ToReport("测边网平差结果"));

        sb.AppendLine("已知点信息：");
        foreach (var point in fixedPoints)
        {
            point.CurrentDisplayMode = DisplayMode.Coordinate;
            sb.AppendLine(point.ToString());
        }
        sb.AppendLine();

        sb.AppendLine("未知点信息：");
        foreach (var point in unknownPoints)
        {
            sb.AppendLine(point.ToString());
        }
        sb.AppendLine();

        sb.AppendLine("观测信息：");
        foreach (var point in observations)
        {
            sb.AppendLine(point.ToString());
        }
        sb.AppendLine();

        sb.AppendLine($"观测数 n = {observations.Count}");
        sb.AppendLine($"未知数 t = {unknownPoints.Count}");
        sb.AppendLine($"多余观测 r = {observations.Count - unknownPoints.Count}");
        sb.AppendLine();

        sb.AppendLine(result.Report);

        sb.AppendLine("未知点坐标(m)：");

        for (int i = 0; i < unknownPoints.Count; i++)
        {
            var p = unknownPoints[i];

            sb.AppendLine(
                $"{p.Name,-6} " +
                $"X={result.XHat[2 * i]:F4} " +
                $"Y={result.XHat[2 * i + 1]:F4}");
        }

        sb.AppendLine();
        sb.AppendLine("观测改正数 v(mm)");

        var v = result.LS.V;

        for (int i = 0; i < observations.Count; i++)
        {
            sb.AppendLine(
                $"{observations[i].Name,-6} " +
                $"{v[i] * 1000:F1}");
        }

        sb.AppendLine(precision.Report);

        _resultBox.Text = sb.ToString();

        if (result.Success)
        {
            for (int i = 0; i < unknownPoints.Count; i++)
            {
                unknownPoints[i].X =
                    result.XHat[2 * i];

                unknownPoints[i].Y =
                    result.XHat[2 * i + 1];
            }

            _statusLabel.Text =
                $"测边网平差完成（迭代 {result.Iterations} 次）";
        }
        else
        {
            _statusLabel.Text =
                "测边网平差未收敛";
        }

        RefreshProjectViews();

        ShowSelectionProperties(
            _board.SelectedObject);
    }

    private void RunHeightNetworkCheck()
    {
        var validation = ProjectDiagnostics.ValidateHeightNetwork(_project);
        _resultBox.Text = validation.ToReport("高程网检查");
        _statusLabel.Text = validation.HasErrors
            ? "高程网检查未通过，请先修正错误。"
            : "高程网检查通过，可以开始计算。";
    }

    private void RunDistanceNetworkCheck()
    {
        var validation = ProjectDiagnostics.ValidateDistanceNetwork(_project);
        _resultBox.Text = validation.ToReport("测边网检查");
        _statusLabel.Text = validation.HasErrors
            ? "测边网检查未通过，请先修正错误。"
            : "测边网检查通过，可以开始计算。";
    }

    private void DeleteSelectedObject()
    {
        var selected = _board.SelectedObject;
        if (selected is null)
        {
            _statusLabel.Text = "没有选中可删除的对象。";
            return;
        }

        var name = selected switch
        {
            SurveyPoint point => $"点 {point.Name}",
            HeightObservation observation => $"高差观测 {observation.Name}",
            DistanceObservation observation => $"距离观测 {observation.Name}",
            _ => "所选对象",
        };

        var confirm = MessageBox.Show(
            $"确定删除 {name} 吗？\r\n\r\n如果删除点，与该点相连的观测和连线也会被删除。",
            "删除对象",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Question);
        if (confirm != DialogResult.OK)
        {
            return;
        }

        _project.RemoveObject(selected);
        _board.ClearSelection();
        RefreshProjectViews();
        ShowSelectionProperties(null);
        _statusLabel.Text = $"{name} 已删除。";
    }

    private sealed class ObjectListItem(object value)
    {
        public object Value { get; } = value;

        public override string ToString()
        {
            return Value.ToString() ?? "";
        }
    }
}

internal enum ToolMode
{
    Select,
    AddPoint,
    AddLine,
    AddHeight,
    AddDistance,
    FixedHeight,
    FixedCoordinate,
}
