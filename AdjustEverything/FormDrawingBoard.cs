using System.Diagnostics;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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

    private const double RHO = 180.0 / Math.PI * 3600.0;

    public FormDrawingBoard()
    {
        InitializeComponent();

        Text = "AdjustEverything - 平差建模原型";
        MinimumSize = new Size(1380, 820);
        Size = new Size(1688, 1120);
        StartPosition = FormStartPosition.CenterScreen;
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
        SetMode(ToolMode.Select);
    }

    private void BuildLayout()
    {
        Controls.Clear();

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(28),
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
            Padding = new Padding(14, 0, 0, 0),
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
            Padding = new Padding(14, 0, 0, 0),
            Font = new Font(Font, FontStyle.Bold),
        };

        var heightCalculate = new Button
        {
            Text = "▷ 水准网平差",
            Dock = DockStyle.Right,
            Width = 150,
            FlatStyle = FlatStyle.Flat,
        };
        heightCalculate.Click += (_, _) => RunHeightAdjustment();

        var distanceCalculate = new Button
        {
            Text = "▷ 测边网平差",
            Dock = DockStyle.Right,
            Width = 150,
            FlatStyle = FlatStyle.Flat,
        };
        distanceCalculate.Click += (_, _) => RunDistanceAdjustment();

        var angleCalculate = new Button
        {
            Text = "▷ 测角网平差",
            Dock = DockStyle.Right,
            Width = 150,
            FlatStyle = FlatStyle.Flat,
        };
        angleCalculate.Click += (_, _) => RunAngleAdjustment();

        var angleDistanceCalculate = new Button
        {
            Text = "▷ 边角网平差",
            Dock = DockStyle.Right,
            Width = 150,
            FlatStyle = FlatStyle.Flat,
        };
        angleDistanceCalculate.Click += (_, _) => RunAngleDistanceAdjustment();

        boardHeader.Controls.Add(distanceCalculate);
        boardHeader.Controls.Add(heightCalculate);
        boardHeader.Controls.Add(angleCalculate);
        boardHeader.Controls.Add(angleDistanceCalculate);
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
            Padding = new Padding(8),
            AutoScroll = true,
        };

        AddToolButton(panel, "☞ 选择对象", ToolMode.Select);
        AddToolButton(panel, "◎ 添加已知高程点", ToolMode.FixedHeight);
        AddToolButton(panel, "◎ 添加已知坐标点", ToolMode.AddKnownPoint);
        AddToolButton(panel, "━ 添加基线", ToolMode.AddBaseLine);
        AddToolButton(panel, "○ 添加点", ToolMode.AddPoint);
        AddToolButton(panel, "─ 添加线", ToolMode.AddLine);
        AddToolButton(panel, "∠β添加角度观测", ToolMode.AddAngle);
        AddToolButton(panel, "↔ 添加距离观测", ToolMode.AddDistance);
        AddToolButton(panel, "↕ 添加高差观测", ToolMode.AddHeight);

        var levelheightsample = BuildSideButton("载入水准网示例");
        levelheightsample.Click += (_, _) =>
        {
            LoadLevelHeightSampleNetwork();
            SetMode(ToolMode.AddPoint);
        };
        panel.Controls.Add(levelheightsample);

        var distancesample = BuildSideButton("载入测边网示例");
        distancesample.Click += (_, _) =>
        {
            LoadDistanceSampleNetwork();
            SetMode(ToolMode.AddPoint);
        };
        panel.Controls.Add(distancesample);

        var angleSample = BuildSideButton("载入测角网示例");
        angleSample.Click += (_, _) =>
        {
            LoadAngleSampleNetwork();
            SetMode(ToolMode.AddPoint);
        };
        panel.Controls.Add(angleSample);

        var angleDistanceSample = BuildSideButton("载入边角网示例");
        angleDistanceSample.Click += (_, _) =>
        {
            LoadAngleDistanceSampleNetwork();
            SetMode(ToolMode.AddPoint);
        };
        panel.Controls.Add(angleDistanceSample);

        var importProject = BuildSideButton("导入项目");
        importProject.Click += (_, _) => ImportProjectFromFile();
        panel.Controls.Add(importProject);

        var exportProject = BuildSideButton("导出项目");
        exportProject.Click += (_, _) => ExportProjectToFile();
        panel.Controls.Add(exportProject);

        var exportResult = BuildSideButton("导出结果");
        exportResult.Click += (_, _) => ExportResultToFile();
        panel.Controls.Add(exportResult);

        var heightCheck = BuildSideButton("检查水准网");
        heightCheck.Click += (_, _) => RunHeightNetworkCheck();
        panel.Controls.Add(heightCheck);

        var distanceCheck = BuildSideButton("检查测边网");
        distanceCheck.Click += (_, _) => RunDistanceNetworkCheck();
        panel.Controls.Add(distanceCheck);

        var angleCheck = BuildSideButton("检查测角网");
        angleCheck.Click += (_, _) => RunAngleNetworkCheck();
        panel.Controls.Add(angleCheck);

        var angledistanceCheck = BuildSideButton("检查边角网");
        angledistanceCheck.Click += (_, _) => RunAngleDistanceNetworkCheck();
        panel.Controls.Add(angledistanceCheck);

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
            ToolMode.AddBaseLine => "添加基线：依次点击两个已知坐标点，创建基线。",
            ToolMode.AddHeight => "添加高程：依次单击两个点，创建高差观测；数值在属性面板填写。",
            ToolMode.AddDistance => "添加距离：依次单击两个点，创建距离观测；数值在属性面板填写。",
            ToolMode.AddAngle => "添加角度：依次点击后视点A、测站点B、前视点C。",
            ToolMode.FixedHeight => "已知高程：单击点设为已知高程点；高程在属性面板修改。",
            ToolMode.AddKnownPoint => "添加已知坐标点：在画板空白处单击添加已知坐标点；数值在属性面板修改。",
            ToolMode.AddKnownSide => "添加已知边：依次单击两个点，创建已知边；数值在属性面板修改。",
            ToolMode.Select => "选择模式：单击点或观测查看属性。",
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

    private void ImportProjectFromFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "导入 AdjustEverything 项目",
            Filter = "AdjustEverything 项目 (*.aep.json)|*.aep.json|JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var resultText = ProjectFileService.Load(dialog.FileName, _project);
            _board.ClearSelection();
            RefreshProjectViews();
            ShowSelectionProperties(null);
            _resultBox.Text = resultText;
            _statusLabel.Text = "项目文件已导入。";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "导入失败：\r\n\r\n" + ex.Message,
                "导入项目",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ExportProjectToFile()
    {
        using var dialog = new SaveFileDialog
        {
            Title = "导出 AdjustEverything 项目",
            Filter = "AdjustEverything 项目 (*.aep.json)|*.aep.json|JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            DefaultExt = "aep.json",
            AddExtension = true,
            FileName = "AdjustEverythingProject.aep.json",
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            ProjectFileService.Save(_project, dialog.FileName, _resultBox.Text);
            _statusLabel.Text = "项目文件已导出。";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "导出失败：\r\n\r\n" + ex.Message,
                "导出项目",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ExportResultToFile()
    {
        using var dialog = new SaveFileDialog
        {
            Title = "导出平差结果",
            Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
            DefaultExt = "txt",
            AddExtension = true,
            FileName = "AdjustmentResult.txt",
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            File.WriteAllText(dialog.FileName, _resultBox.Text, Encoding.UTF8);
            _statusLabel.Text = "平差结果已导出。";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "导出失败：\r\n\r\n" + ex.Message,
                "导出结果",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void LoadLevelHeightSampleNetwork()
    {
        _project.Clear();

        ProjectFileService.Load("sampleNet/LevelHeightSample.aep.json", _project);
        
        ResetBoardAfterLoadingSample();
    }

    private void LoadDistanceSampleNetwork()
    {
        _project.Clear();

        ProjectFileService.Load("sampleNet/DistanceSample.aep.json", _project);

        ResetBoardAfterLoadingSample();
    }

    private void LoadAngleSampleNetwork()
    {
        _project.Clear();

        ProjectFileService.Load("sampleNet/AngleSample.aep.json", _project);

        ResetBoardAfterLoadingSample();
    }

    private void LoadAngleDistanceSampleNetwork()
    {
        _project.Clear();

        ProjectFileService.Load("sampleNet/AngleDistanceSample.aep.json", _project);

        ResetBoardAfterLoadingSample();
    }

    private static double NormalizeDegrees(double degrees)
    {
        while (degrees < 0.0)
        {
            degrees += 360.0;
        }

        while (degrees >= 360.0)
        {
            degrees -= 360.0;
        }

        return degrees;
    }

    private void ResetBoardAfterLoadingSample()
    {
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

        foreach (var point in _project.KnownPoints)
        {
            _objectList.Items.Add(new ObjectListItem(point));
        }

        foreach (var baseline in _project.Baselines)
        {
            _objectList.Items.Add(new ObjectListItem(baseline));
        }

        foreach (var obs in _project.HeightObservations)
        {
            _objectList.Items.Add(new ObjectListItem(obs));
        }

        foreach (var obs in _project.DistanceObservations)
        {
            _objectList.Items.Add(new ObjectListItem(obs));
        }
        foreach (var obs in _project.AngleObservations)
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
            case KnownPoint known:
                BuildKnownPointProperties(known);
                break;

            case Baseline baseline:
                BuildBaselineProperties(baseline);
                break;
            case HeightObservation observation:
                BuildHeightObservationProperties(observation);
                break;
            case DistanceObservation observation:
                BuildDistanceObservationProperties(observation);
                break;
            case AngleObservation observation:
                BuildAngleObservationProperties(observation);
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

    private void BuildKnownPointProperties(KnownPoint known)
    {
        var table = BuildPropertyTable();
        AddReadonlyRow(table, "点名", known.Point.Name);
        AddReadonlyRow(table, "X", known.Point.X?.ToString("F4") ?? "");
        AddReadonlyRow(table, "Y", known.Point.Y?.ToString("F4") ?? "");

        var delete = AddDeleteButton(table);
        delete.Click += (_, _) => DeleteSelectedObject();

        _propertyPanel.Controls.Add(table);
    }

    private void BuildBaselineProperties(
    Baseline baseline)
    {
        var table = BuildPropertyTable();
        AddReadonlyRow(table, "名称", baseline.Name);
        AddReadonlyRow(table, "起点", baseline.From.Name);
        AddReadonlyRow(table, "终点", baseline.To.Name);
        AddReadonlyRow(table, "长度(m)", baseline.Length.ToString("F4"));

        var delete = AddDeleteButton(table);
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

    private void BuildAngleObservationProperties(AngleObservation observation)
    {
        var table = BuildPropertyTable();

        var nameBox =AddTextRow(table, "观测名", observation.Name);

        AddReadonlyRow(table, "后视点", observation.From.Name);

        AddReadonlyRow(table, "测站点", observation.Vertex.Name);

        AddReadonlyRow(table, "前视点", observation.To.Name);
        
        var valueBox =AddTextRow(table, "角度值 dd°mm'ss\"", AngleFormatter.ToDms(observation.Value));

        var sigmaBox = AddTextRow(table, "中误差σ", observation.Sigma.ToString("F3"));

        var apply = AddApplyButton(table);

        var delete = AddDeleteButton(table);

        apply.Click += (_, _) =>
        {
            if (!ReadAngleObservation(
                    nameBox,
                    valueBox,
                    sigmaBox,
                    out var name,
                    out var value,
                    out var sigma))
            {
                return;
            }

            value = NormalizeDegrees(value);

            observation.Name = name;
            observation.Sigma = sigma;
            observation.Value = value;

            RefreshProjectViews();

            _board.SelectObject(observation);

            _statusLabel.Text =
                $"角度观测 {observation.Name} 已更新";
        };

        delete.Click += (_, _) =>
            DeleteSelectedObject();

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

    private static bool ReadAngleObservation(
        TextBox nameBox,
        TextBox valueBox,
        TextBox sigmaBox,
        out string name,
        out double value,
        out double sigma)
    {
        name = nameBox.Text.Trim();
        value = 0.0;
        sigma = 0.0;

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("观测名不能为空。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (!AngleFormatter.TryParseDms(valueBox.Text, out value))
        {
            MessageBox.Show("角度值必须使用 dd°mm'ss\" 格式，也可以输入十进制度。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (!double.TryParse(sigmaBox.Text, out sigma) || sigma <= 0)
        {
            MessageBox.Show("中误差必须是大于 0 的数字。", "属性错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        try
        {
            var validation = ProjectDiagnostics.ValidateHeightNetwork(_project);

            if (validation.HasErrors)
            {
                _resultBox.Text = validation.ToReport("水准网检查");
                _statusLabel.Text = "水准网检查未通过，请根据结果面板中的错误修改模型。";
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

            var X0 =
                ApproximateHeightBuilder.BuildX0(
                    _project.Points,
                    unknownPoints,
                    observations);

            //2. 导入模型
            var model = new LevelHeightModel(
                unknownPoints,
                observations,
                index,
                X0);

            //3. 完成平差
            var result = NonlinearLeastSquaresSolver.Solve(model, model);

            //4. 精度评定
            var precision = PrecisionEstimator.Estimate(result, PrecisionEstimator.NetType.LevelHeight);

            //5. 报告生成
            var sb = new StringBuilder();

            sb.AppendLine(validation.ToReport("水准网平差结果"));

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
                    $"{result.XHat[i]:F3}");
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

        }
        catch (Exception ex)
        {
            _resultBox.Text = "高程网平差失败\r\n\r\n" + ex.Message;

            _statusLabel.Text = "计算失败";
        }

        RefreshProjectViews();
        ShowSelectionProperties(_board.SelectedObject);
    }

    private void RunDistanceAdjustment()
    {
        try
        {
            var validation = ProjectDiagnostics.ValidateDistanceNetwork(_project);

            if (validation.HasErrors)
            {
                _resultBox.Text = validation.ToReport("测边网检查");
                _statusLabel.Text = "测边网检查未通过，请根据结果面板中的错误修改模型。";

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

            //1. 解出所有近似坐标
            var approximateCoordinates = ApproximateCoordinateBuilder.Build(_project);


            // 2. 生成所有初值组合
            var initialSolutions =
                ApproximateCoordinateBuilder.BuildInitialSolutions(
                    _project,
                    unknownPoints,
                    approximateCoordinates);

            double bestVPV = double.MaxValue;
            var result = default(NonlinearResult);

            // 3. 根据最小VPV值选择几何最优平差解
            foreach (var x0Candidate in initialSolutions)
            {
                var x0 = x0Candidate;

                var model =
                    new DistanceModel(
                        unknownPoints,
                        observations,
                        parameterIndex,
                        x0);

                var resultP =
                    NonlinearLeastSquaresSolver.Solve(model, model);

                double vpv =
                    MatrixUtility.ComputeVPV(
                        resultP.LS.V,
                        resultP.LS.P);

                if (vpv < bestVPV)
                {
                    bestVPV = vpv;
                    result = resultP;
                }
            }

            var precision = PrecisionEstimator.Estimate(result, PrecisionEstimator.NetType.Distance);

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
                    $"X={result.XHat[2 * i]:F3} " +
                    $"Y={result.XHat[2 * i + 1]:F3}");
            }

            sb.AppendLine();
            sb.AppendLine("观测改正数 v(mm)");

            var v = result.LS.V;

            for (int i = 0; i < observations.Count; i++)
            {
                sb.AppendLine(
                    $"{observations[i].Name,-6} " +
                    $"{1000 * v[i]:F1}");
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

        }
        catch (Exception ex)
        {
            _resultBox.Text = "测边网平差失败\r\n\r\n" + ex.Message;
            _statusLabel.Text = "计算失败";

        }

        RefreshProjectViews();
        ShowSelectionProperties(_board.SelectedObject);
    }

    private void RunAngleAdjustment()
    {
        try
        {
            var validation = ProjectDiagnostics.ValidateAngleNetwork(_project);

            if (validation.HasErrors)
            {
                _resultBox.Text = validation.ToReport("测角网检查");
                _statusLabel.Text = "测角网检查未通过，请根据结果面板中的错误修改模型。";
                return;
            }

            // 已知点和未知点
            var fixedPoints = _project.Points.Where(p => p.IsCoordinateFixed && p.X.HasValue && p.Y.HasValue).ToList();
            var unknownPoints = _project.Points.Where(p => !p.IsCoordinateFixed).ToList();
            var observations = _project.AngleObservations.ToList();

            // 构造参数索引
            var paramIndex = new Dictionary<SurveyPoint, int>();
            for (int i = 0; i < unknownPoints.Count; i++)
                paramIndex[unknownPoints[i]] = 2 * i;

            // 构造模型
            var model = new AngleModel(unknownPoints, observations, paramIndex);

            // 非线性最小二乘求解
            var result = NonlinearLeastSquaresSolver.Solve(model, model);

            // 精度评定
            var precision = PrecisionEstimator.Estimate(result, PrecisionEstimator.NetType.Angle);

            var sb = new System.Text.StringBuilder();

            sb.AppendLine(validation.ToReport("测角网平差结果"));
            sb.AppendLine();

            sb.AppendLine("未知点信息：");
            foreach (var point in unknownPoints)
                sb.AppendLine(point.ToString());
            sb.AppendLine();

            sb.AppendLine("观测信息：");
            foreach (var obs in observations)
                sb.AppendLine(obs.ToString());
            sb.AppendLine();

            sb.AppendLine($"观测数 n = {observations.Count}");
            sb.AppendLine($"未知数 t = {unknownPoints.Count * 2}");
            sb.AppendLine($"多余观测 r = {observations.Count - unknownPoints.Count * 2}");
            sb.AppendLine();

            sb.AppendLine("未知点坐标(m)：");
            for (int i = 0; i < unknownPoints.Count; i++)
            {
                var p = unknownPoints[i];
                sb.AppendLine($"{p.Name,-6} X={result.XHat[2 * i]:F3} Y={result.XHat[2 * i + 1]:F4}");
            }
            sb.AppendLine();

            sb.AppendLine("观测改正数 v(″)：");


            for (int i = 0; i < observations.Count; i++)
            {
                sb.AppendLine(
                    $"{observations[i].Name,-6} " +
                    $"{RHO * result.LS.V[i]:F2}");
            }

            sb.AppendLine(precision.Report);

            _resultBox.Text = sb.ToString();

            if (result.Success)
            {
                for (int i = 0; i < unknownPoints.Count; i++)
                {
                    unknownPoints[i].X = result.XHat[2 * i];
                    unknownPoints[i].Y = result.XHat[2 * i + 1];
                }
                _statusLabel.Text = $"测角网平差完成（迭代 {result.Iterations} 次）";
            }
            else
                _statusLabel.Text = "测角网平差未收敛";

        }
        catch (Exception ex)
        {
            _resultBox.Text = "测角网平差失败\r\n\r\n" + ex.Message;
            _statusLabel.Text = "计算失败";
        }
        RefreshProjectViews();
        ShowSelectionProperties(_board.SelectedObject);
    }

    private void RunAngleDistanceAdjustment()
    {
        var validation = ProjectDiagnostics.ValidateAngleDistanceNetwork(_project);

        if (validation.HasErrors)
        {
            _resultBox.Text = validation.ToReport("边角网检查");
            _statusLabel.Text = "边角网检查未通过，请根据结果面板中的错误修改模型。";

            return;
        }

        try
        {
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

            var distanceObs =
                _project.DistanceObservations
                    .ToList();

            var angleObs =
                _project.AngleObservations
                    .ToList();


            var parameterIndex =
                new Dictionary<SurveyPoint, int>();

            for (int i = 0; i < unknownPoints.Count; i++)
            {
                parameterIndex[
                    unknownPoints[i]]
                    = 2 * i;
            }

            var x0 =
                new double[
                    unknownPoints.Count * 2];

            for (int i = 0; i < unknownPoints.Count; i++)
            {
                var p = unknownPoints[i];

                x0[2 * i]
                    = p.X
                      ?? SurveyCoordinateMapper.XFromCanvas(p.CanvasLocation);

                x0[2 * i + 1]
                    = p.Y
                      ?? SurveyCoordinateMapper.YFromCanvas(p.CanvasLocation);
            }

            var model =
                new AngleDistanceModel(
                    unknownPoints,
                    distanceObs,
                    angleObs,
                    parameterIndex,
                    x0);

            var result = NonlinearLeastSquaresSolver.Solve(model, model);

            var precision = PrecisionEstimator.Estimate(result, PrecisionEstimator.NetType.AngleDistance);

            var sb = new System.Text.StringBuilder();

            sb.AppendLine(validation.ToReport("边角网平差结果"));
            sb.AppendLine();

            sb.AppendLine("未知点信息：");
            foreach (var point in unknownPoints)
                sb.AppendLine(point.ToString());
            sb.AppendLine();

            sb.AppendLine("观测信息：");
            foreach (var obs in angleObs)
                sb.AppendLine(obs.ToString());
            foreach (var obs in distanceObs)
                sb.AppendLine(obs.ToString());
            sb.AppendLine();

            sb.AppendLine($"观测数 n = {angleObs.Count + distanceObs.Count}");
            sb.AppendLine($"未知数 t = {unknownPoints.Count * 2}");
            sb.AppendLine($"多余观测 r = {angleObs.Count + distanceObs.Count - unknownPoints.Count * 2}");
            sb.AppendLine();

            sb.AppendLine("未知点坐标(m)：");
            for (int i = 0; i < unknownPoints.Count; i++)
            {
                var p = unknownPoints[i];
                sb.AppendLine($"{p.Name,-6} X={result.XHat[2 * i]:F3} Y={result.XHat[2 * i + 1]:F3}");
            }
            sb.AppendLine();

            sb.AppendLine("观测改正数 v：距离(mm)，角度(″)");

            for (int i = 0; i < distanceObs.Count; i++)
            {
                sb.AppendLine(
                    $"{distanceObs[i].Name,-6} " +
                    $"{1000 * result.LS.V[i]:F2} mm");
            }

            for (int i = 0; i < angleObs.Count; i++)
            {
                var row = distanceObs.Count + i;
                sb.AppendLine(
                    $"{angleObs[i].Name,-6} " +
                    $"{RHO * result.LS.V[row]:F2} ″");
            }


            sb.AppendLine(precision.Report);

            _resultBox.Text = sb.ToString();

            if (result.Success)
            {
                for (int i = 0; i < unknownPoints.Count; i++)
                {
                    unknownPoints[i].X = result.XHat[2 * i];
                    unknownPoints[i].Y = result.XHat[2 * i + 1];
                }
                _statusLabel.Text = $"边角网平差完成（迭代 {result.Iterations} 次）";
            }
            else
                _statusLabel.Text = "边角网平差未收敛";
        }
        catch (Exception ex)
        {
            _resultBox.Text = "边角网平差失败\r\n\r\n" + ex.Message;
            _statusLabel.Text = "计算失败";
        }

        RefreshProjectViews();
        ShowSelectionProperties(_board.SelectedObject);
    }

    private void RunHeightNetworkCheck()
    {
        var validation = ProjectDiagnostics.ValidateHeightNetwork(_project);
        _resultBox.Text = validation.ToReport("水准网检查");
        _statusLabel.Text = validation.HasErrors
            ? "水准网检查未通过，请先修正错误。"
            : "水准网检查通过，可以开始计算。";
    }

    private void RunDistanceNetworkCheck()
    {
        var validation = ProjectDiagnostics.ValidateDistanceNetwork(_project);
        _resultBox.Text = validation.ToReport("测边网检查");
        _statusLabel.Text = validation.HasErrors
            ? "测边网检查未通过，请先修正错误。"
            : "测边网检查通过，可以开始计算。";
    }

    private void RunAngleNetworkCheck()
    {
        var validation = ProjectDiagnostics.ValidateAngleNetwork(_project);

        _resultBox.Text = validation.ToReport("测角网检查");

        _statusLabel.Text = validation.HasErrors
            ? "测角网检查未通过，请先修正错误。"
            : "测角网检查通过，可以开始计算。";
    }

    private void RunAngleDistanceNetworkCheck()
    {
        var validation = ProjectDiagnostics.ValidateAngleDistanceNetwork(_project);

        _resultBox.Text = validation.ToReport("边角网检查");

        _statusLabel.Text = validation.HasErrors
            ? "边角网检查未通过，请先修正错误。"
            : "边角网检查通过，可以开始计算。";
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
            SurveyPoint point =>
                $"点 {point.Name}",

            KnownPoint point =>
                $"已知点 {point.Point.Name}",

            Baseline baseline =>
                $"基线 {baseline.Name}",

            HeightObservation observation =>
                $"高差观测 {observation.Name}",

            DistanceObservation observation =>
                $"距离观测 {observation.Name}",

            AngleObservation observation =>
                $"角度观测 {observation.Name}",

            _ =>
                "所选对象",
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
    AddKnownPoint,
    AddKnownSide,
    AddLine,
    AddBaseLine,
    AddHeight,
    AddDistance,
    AddAngle,
    FixedHeight
}
