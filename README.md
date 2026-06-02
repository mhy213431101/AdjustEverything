# AdjustEverything

AdjustEverything 是一个面向测量平差学习与实验的 WinForms 原型项目。它的目标是把“画板建模”和“平差计算”结合起来：用户先在画板上添加点、线和观测值，再由程序根据这些对象自动组织平差模型并输出结果。

当前版本支持：

- 在画板上添加、拖动点
- 添加普通连线
- 添加高差观测
- 添加距离观测
- 设置已知高程点
- 设置已知平面坐标点
- 编辑点名、X/Y、高程 H、观测值和中误差
- 删除点、观测和关联连线
- 高程网计算前检查
- 测边网计算前检查
- 高程网间接平差
- 测边网迭代最小二乘平差

## 开发环境

- Visual Studio 2022
- .NET 8 SDK
- Windows

## 如何运行

1. 下载或克隆本仓库。
2. 用 Visual Studio 打开 `AdjustEverything.sln`。
3. 选择 `AdjustEverything` 项目。
4. 点击运行按钮，或按 `F5` 启动。

## 项目结构

```text
AdjustEverything.sln
README.md
.gitignore

AdjustEverything/
  AdjustEverything.csproj
  Program.cs
  Form1.cs
  Form1.Designer.cs
  Form1.resx
  AdjustmentProject.cs
  DrawingBoard.cs
  ProjectDiagnostics.cs
  HeightNetworkSolver.cs
  DistanceNetworkSolver.cs
```

## 各文件职责

### `Program.cs`

程序入口文件。它调用 WinForms 的初始化逻辑，并启动主窗体 `Form1`。

### `Form1.cs`

主窗口逻辑，是整个程序的“总控层”。主要负责：

- 构建左侧工具栏、对象列表、属性面板、结果面板和画板区域
- 响应工具按钮，例如添加点、添加高程、添加距离、检查、计算、删除
- 接收画板发出的项目变更和对象选中事件
- 根据选中对象动态生成属性编辑面板
- 调用 `ProjectDiagnostics` 做计算前检查
- 调用 `HeightNetworkSolver` 和 `DistanceNetworkSolver` 执行平差
- 把平差结果写回点对象，并刷新界面

### `Form1.Designer.cs` 和 `Form1.resx`

WinForms 设计器生成文件。当前主要保留默认窗体资源和初始化结构，不建议手工大改。

### `AdjustmentProject.cs`

核心数据模型文件。它定义了项目中的主要对象：

- `AdjustmentProject`：项目总容器，保存点、线、高差观测和距离观测
- `SurveyPoint`：测点对象，包含画板坐标、点名、高程 H、平面坐标 X/Y、是否为已知点等属性
- `BoardLine`：画板上的视觉连线
- `HeightObservation`：高差观测，表示 `From -> To` 的观测高差
- `DistanceObservation`：距离观测，表示两点之间的水平距离

这个文件不负责界面绘制，也不负责求解，只负责保存和维护数据关系。

### `DrawingBoard.cs`

自绘画板控件。它负责把项目数据画出来，并把鼠标操作转化为项目对象。

主要功能：

- 绘制网格、点、线、高差标签和距离标签
- 单击空白处添加点
- 拖动点改变画板位置
- 两次点选创建普通连线、高差观测或距离观测
- 单击点或观测线进行选中
- 用颜色区分普通点、已知高程点和已知坐标点

它不直接进行平差计算，只通过事件通知 `Form1` 刷新界面。

### `ProjectDiagnostics.cs`

计算前检查器。它负责判断当前模型是否具备平差条件，并给出用户能理解的提示。

目前包含：

- `ValidateHeightNetwork`：检查高程网
- `ValidateDistanceNetwork`：检查测边网

高程网检查内容包括：

- 是否有点和高差观测
- 是否至少有一个已知高程点
- 已知高程点是否填写 H
- 观测数是否足够
- 是否有孤立点
- 子网是否缺少高程基准
- 中误差是否大于 0
- 点名和观测名是否重复

测边网检查内容包括：

- 是否有点和距离观测
- 是否至少有两个已知平面坐标点
- 已知坐标点是否填写完整 X/Y
- 距离观测数是否足够
- 距离和中误差是否大于 0
- 子网是否缺少坐标基准
- 点名和观测名是否重复

### `HeightNetworkSolver.cs`

高程网间接平差求解器。

它使用线性模型：

```text
H_To - H_From = Δh + v
```

处理流程：

1. 找出未知高程点。
2. 为每个未知高程点分配参数序号。
3. 根据高差观测组装误差方程。
4. 组装法方程。
5. 求解未知高程。
6. 计算改正数、单位权中误差和结果报告。

由于高程网方程是线性的，所以当前求解器不需要迭代。

### `DistanceNetworkSolver.cs`

测边网坐标平差求解器。

它使用非线性距离模型：

```text
S = sqrt((X_j - X_i)^2 + (Y_j - Y_i)^2) + v
```

处理流程：

1. 找出未知平面点。
2. 为每个未知点分配 `X/Y` 两个参数。
3. 使用点属性中的 X/Y 作为近似坐标；没有填写时使用画板坐标。
4. 根据当前近似坐标计算距离理论值。
5. 对距离方程线性化，构造系数矩阵。
6. 组装法方程并求解坐标改正数。
7. 更新未知点坐标。
8. 重复迭代直到改正数足够小。
9. 输出平差后坐标、距离改正数和单位权中误差。

当前测边网暂时要求至少两个已知平面坐标点，不处理自由网平差。

## 主程序整合流程

程序运行后的核心数据流如下：

```text
用户操作画板
  -> DrawingBoard 捕获鼠标事件
  -> AdjustmentProject 增删点、线、观测
  -> Form1 刷新对象列表和属性面板
  -> 用户编辑属性
  -> Form1 写回 AdjustmentProject
  -> 用户点击检查或计算
  -> ProjectDiagnostics 检查模型
  -> Solver 执行平差
  -> Form1 把结果写回点对象
  -> DrawingBoard 和结果面板刷新
```

也就是说：

- `AdjustmentProject` 是数据核心
- `DrawingBoard` 是图形交互层
- `Form1` 是界面协调层
- `ProjectDiagnostics` 是模型检查层
- `HeightNetworkSolver` 和 `DistanceNetworkSolver` 是计算层

## 当前状态

这是早期原型版本，重点是验证“画板建模 + 平差求解”的基本技术路线。

已实现：

- 高程网基础平差
- 测边网基础平差
- 图形化建模
- 属性编辑
- 计算前诊断

后续计划逐步加入：

- 测角网平差
- 边角网联合平差
- 更完整的结果表和精度评定
- 项目保存与读取
- 更规范的单位系统
- 更稳健的矩阵求解库
