namespace AdjustEverything;

// 主窗体负责把 UI 控件、画板、项目数据、检查器和求解器串起来。
public partial class Form1 : Form
{
    private readonly AdjustmentProject _project = new();
    private readonly DrawingBoard _board;
    private readonly ListBox _objectList = new();
    private readonly TextBox _resultBox = new();
    private readonly Panel _propertyPanel = new();
    private readonly Label _statusLabel = new();
    private bool _syncingObjectList;

    public Form1()
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
            // 画板改动项目数据后，列表和属性面板都要同步刷新。
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
        // 整体布局：左侧工具栏，上方对象/属性/结果区，中间大画板。
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
        // 示例网同时包含高程观测和距离观测，便于测试两个求解器。
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
        b.X = 300.000;
        b.Y = 0.000;
        c.X = 130.000;
        c.Y = 130.000;
        d.X = 160.000;
        d.Y = -70.000;

        _project.AddHeightObservation(a, c, 1.215);
        _project.AddHeightObservation(a, d, 0.385);
        _project.AddHeightObservation(d, c, 0.821);
        _project.AddHeightObservation(c, b, 0.028);
        _project.AddHeightObservation(d, b, 0.842);

        _project.AddDistanceObservation(a, c, 184.3909);
        _project.AddDistanceObservation(c, b, 228.0351);
        _project.AddDistanceObservation(a, d, 170.0000);
        _project.AddDistanceObservation(d, b, 170.0000);
        _project.AddDistanceObservation(c, d, 202.0000);

        _board.ClearSelection();
        RefreshProjectViews();
        ShowSelectionProperties(null);
        _resultBox.Clear();
    }

    private void RefreshProjectViews()
    {
        // 对象列表不直接保存副本，只保存对模型对象的引用，选中后可回到原对象编辑。
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
        // 属性面板根据对象类型动态生成，后续添加角度观测时也可以沿用这个模式。
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
        var heightBox = AddTextRow(table, "高程 H", point.Height?.ToString("F4") ?? "");

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

            if (!TryReadNullableDouble(heightBox.Text, "高程 H", out var height)
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
        var valueBox = AddTextRow(table, "高差 Δh", observation.Value.ToString("F4"));
        var sigmaBox = AddTextRow(table, "中误差 σ", observation.Sigma.ToString("F4"));
        var apply = AddApplyButton(table);
        var delete = AddDeleteButton(table);

        apply.Click += (_, _) =>
        {
            if (!ReadObservationCommon(nameBox, valueBox, sigmaBox, "高差 Δh", out var name, out var value, out var sigma))
            {
                return;
            }

            observation.Name = name;
            observation.Value = value;
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
        var valueBox = AddTextRow(table, "距离 S", observation.Value.ToString("F4"));
        var sigmaBox = AddTextRow(table, "中误差 σ", observation.Sigma.ToString("F4"));
        var apply = AddApplyButton(table);
        var delete = AddDeleteButton(table);

        apply.Click += (_, _) =>
        {
            if (!ReadObservationCommon(nameBox, valueBox, sigmaBox, "距离 S", out var name, out var value, out var sigma))
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
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        table.Controls.Add(new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
        }, 0, row);
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

    private static bool ReadObservationCommon(
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
            MessageBox.Show("中误差 σ 必须是大于 0 的数字。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        // 先诊断再求解，避免把基准不足、孤立点等建模问题交给矩阵求解器处理。
        var validation = ProjectDiagnostics.ValidateHeightNetwork(_project);
        if (validation.HasErrors)
        {
            _resultBox.Text = validation.ToReport("高程网检查");
            _statusLabel.Text = "高程网检查未通过，请根据结果面板中的错误修改模型。";
            return;
        }

        var result = HeightNetworkSolver.Solve(_project);
        _resultBox.Text = validation.ToReport("高程网检查") + Environment.NewLine + Environment.NewLine + result.Report;

        if (result.Success)
        {
            foreach (var item in result.AdjustedHeights)
            {
                item.Key.Height = item.Value;
            }
        }

        RefreshProjectViews();
        ShowSelectionProperties(_board.SelectedObject);
    }

    private void RunDistanceAdjustment()
    {
        // 测边网同样先做可解性检查，再进入非线性迭代。
        var validation = ProjectDiagnostics.ValidateDistanceNetwork(_project);
        if (validation.HasErrors)
        {
            _resultBox.Text = validation.ToReport("测边网检查");
            _statusLabel.Text = "测边网检查未通过，请根据结果面板中的错误修改模型。";
            return;
        }

        var result = DistanceNetworkSolver.Solve(_project);
        _resultBox.Text = validation.ToReport("测边网检查") + Environment.NewLine + Environment.NewLine + result.Report;

        if (result.Success)
        {
            foreach (var item in result.AdjustedCoordinates)
            {
                item.Key.X = item.Value.X;
                item.Key.Y = item.Value.Y;
            }
        }

        RefreshProjectViews();
        ShowSelectionProperties(_board.SelectedObject);
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
