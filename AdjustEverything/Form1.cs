namespace AdjustEverything;

public partial class Form1 : Form
{
    private readonly AdjustmentProject _project = new();
    private readonly DrawingBoard _board;
    private readonly ListBox _objectList = new();
    private readonly TextBox _resultBox = new();
    private readonly Panel _propertyPanel = new();
    private readonly Label _statusLabel = new();
    private bool _syncingObjectList;
    private ToolMode _mode = ToolMode.AddPoint;

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
        var calculate = new Button
        {
            Text = "▷ 开始计算",
            Dock = DockStyle.Right,
            Width = 210,
            FlatStyle = FlatStyle.Flat,
        };
        calculate.Click += (_, _) => RunHeightAdjustment();
        boardHeader.Controls.Add(calculate);
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

        AddToolButton(panel, "◎ 已知点", ToolMode.FixedHeight);
        AddToolButton(panel, "━ 基线", ToolMode.Select);
        AddToolButton(panel, "○ 添加点", ToolMode.AddPoint);
        AddToolButton(panel, "─ 添加线", ToolMode.AddLine);
        AddToolButton(panel, "(x,y) 添加坐标", ToolMode.Select);
        AddToolButton(panel, "∠α 添加角", ToolMode.Select);
        AddToolButton(panel, "↔ 添加距离", ToolMode.Select);
        AddToolButton(panel, "↕ 添加高程", ToolMode.AddHeight);

        var sample = BuildSideButton("载入示例网");
        sample.Click += (_, _) =>
        {
            LoadSampleNetwork();
            SetMode(ToolMode.AddPoint);
        };
        panel.Controls.Add(sample);

        var check = BuildSideButton("检查高程网");
        check.Click += (_, _) => RunHeightNetworkCheck();
        panel.Controls.Add(check);

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
            Height = 52,
            TextAlign = ContentAlignment.MiddleLeft,
            FlatStyle = FlatStyle.Flat,
            Padding = new Padding(12, 0, 0, 0),
        };
        button.Click += (_, _) => SetMode(mode);
        panel.Controls.Add(button);
    }

    private void SetMode(ToolMode mode)
    {
        _mode = mode;
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
            ToolMode.FixedHeight => "已知点：单击点设为已知高程点；高程在属性面板修改。",
            _ => "选择模式：单击点或高差观测查看属性。"
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

        var a = _project.AddPoint("A", new PointF(250, 210));
        var b = _project.AddPoint("B", new PointF(640, 300));
        var c = _project.AddPoint("C", new PointF(455, 120));
        var d = _project.AddPoint("D", new PointF(420, 390));

        a.IsHeightFixed = true;
        a.Height = 100.000;
        b.IsHeightFixed = true;
        b.Height = 101.230;

        _project.AddHeightObservation(a, c, 1.215);
        _project.AddHeightObservation(a, d, 0.385);
        _project.AddHeightObservation(d, c, 0.821);
        _project.AddHeightObservation(c, b, 0.028);
        _project.AddHeightObservation(d, b, 0.842);

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

        if (selected is SurveyPoint point)
        {
            BuildPointProperties(point);
            return;
        }

        if (selected is HeightObservation observation)
        {
            BuildHeightObservationProperties(observation);
            return;
        }

        var empty = new Label
        {
            Text = "未选中对象。\r\n\r\n在画板或对象列表中选择点、高差观测后，可在这里编辑参数。",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
        };
        _propertyPanel.Controls.Add(empty);
    }

    private void BuildPointProperties(SurveyPoint point)
    {
        var table = BuildPropertyTable();
        var nameBox = AddTextRow(table, "点名", point.Name);
        var fixedCheck = new CheckBox
        {
            Text = "作为已知高程点",
            Checked = point.IsHeightFixed,
            Dock = DockStyle.Fill,
        };
        table.Controls.Add(fixedCheck, 0, table.RowCount);
        table.SetColumnSpan(fixedCheck, 2);
        table.RowCount++;

        var heightBox = AddTextRow(table, "高程 H", point.Height?.ToString("F4") ?? "");
        var apply = AddApplyButton(table);
        var delete = AddDeleteButton(table);
        apply.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(nameBox.Text))
            {
                MessageBox.Show("点名不能为空。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            point.Name = nameBox.Text.Trim();
            point.IsHeightFixed = fixedCheck.Checked;
            point.Height = TryReadNullableDouble(heightBox.Text, out var height) ? height : point.Height;
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
            if (string.IsNullOrWhiteSpace(nameBox.Text))
            {
                MessageBox.Show("观测名不能为空。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(valueBox.Text, out var value))
            {
                MessageBox.Show("高差 Δh 必须是数字。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(sigmaBox.Text, out var sigma) || sigma <= 0)
            {
                MessageBox.Show("中误差 σ 必须是大于 0 的数字。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            observation.Name = nameBox.Text.Trim();
            observation.Value = value;
            observation.Sigma = sigma;
            RefreshProjectViews();
            _board.SelectObject(observation);
            _statusLabel.Text = $"高差观测 {observation.Name} 的属性已更新。";
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

    private static bool TryReadNullableDouble(string text, out double? value)
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

        MessageBox.Show("高程 H 必须为空或数字。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        value = null;
        return false;
    }

    private void RunHeightAdjustment()
    {
        var validation = ProjectDiagnostics.ValidateHeightNetwork(_project);
        if (validation.HasErrors)
        {
            _resultBox.Text = validation.ToReport();
            _statusLabel.Text = "检查未通过，请根据平差结果面板中的错误修改模型。";
            return;
        }

        var result = HeightNetworkSolver.Solve(_project);
        _resultBox.Text = validation.ToReport() + Environment.NewLine + Environment.NewLine + result.Report;

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

    private void RunHeightNetworkCheck()
    {
        var validation = ProjectDiagnostics.ValidateHeightNetwork(_project);
        _resultBox.Text = validation.ToReport();
        _statusLabel.Text = validation.HasErrors
            ? "检查未通过，请先修正错误。"
            : "检查通过，可以开始计算。";
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
            _ => "所选对象",
        };

        var confirm = MessageBox.Show(
            $"确定删除 {name} 吗？\r\n\r\n如果删除点，与该点相连的高差观测和连线也会被删除。",
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
    FixedHeight,
}
