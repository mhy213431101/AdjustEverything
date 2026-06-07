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
- 高程网误差方程建模
- 测边网误差方程建模
- 线性网/非线性网误差方程最小二乘平差解算
- 平差结果精度评定

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
AdjustmentProject.cs
DistanceModel.cs
DrawingBoard.cs
Form1.cs
HeightModel.cs
IAdjustmentModel.cs
ILinearizable.cs
LeastSquaresResult.cs
LeastSquaresSolver.cs
MatrixUtility.cs
NonlinearLeastSquaresSolver.cs
NonlinearResult.cs
PrecisionEstimator.cs
PrecisionResult.cs
Program.cs
ProjectDiagnostics.cs
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
- 构建输入数据
- 调用 `...Model` 进行平差建模
- 调用`NonlinearLeastSquaresSolver` 进行平差解算
- 调用`PrecisionEstimator` 进行精度评定
- 组合生成平差结果报告
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

### `HeightModel.cs`

高程网间接平差建模

它采用线性观测模型，并嵌入统一最小二乘框架中：

H_To - H_From = Δh + v

处理流程如下：

1. 提取控制点与未知高程点，建立参数向量 X0。
2. 为未知高程点建立参数索引映射。
3. 根据高差观测构造线性误差方程：
   系数矩阵 B
   常数向量 W=l−f(X0)
   观测向量 L
   权阵 P
4. 将模型封装为 HeightModel，通过 IAdjustmentModel 与 ILinearizable 接口实现后续统一调度求解：

由于高程网本质为线性模型，因此在当前框架下：
不需要迭代过程
但仍统一纳入非线性最小二乘求解器接口，以保证模型体系一致性与可扩展性（支持未来扩展测边网、测角网等非线性模型）。

### `DistanceModel.cs`

测边网坐标平差求解器。

它使用非线性距离模型：

```text
S = sqrt((X_j - X_i)^2 + (Y_j - Y_i)^2) + v
```

处理流程：

1. 提取已知坐标点与未知平面点，构建参数向量 X⁰ = [X1, Y1, X2, Y2, ...]。
2. 为每个未知点分配两个参数索引（X/Y）。
3. 使用画板坐标或已赋初值作为未知点初始近似坐标。
4. 根据当前近似坐标计算理论距离值 f(X)。
5. 对非线性距离函数进行泰勒展开线性化，构造误差方程：
  系数矩阵 B（雅可比矩阵）
  常数项 W = L - f(X)
  观测向量 L
  权阵 P
6. 将模型封装为 DistanceModel，并通过 IAdjustmentModel 与 ILinearizable 接口实现后续统一调度求解：

当前测边网暂时要求至少两个已知平面坐标点，不处理自由网平差。

### `NonlinearLeastSquaresSolver.cs`

非线性最小二乘统一迭代求解器（Gauss-Newton 框架）

该模块用于对所有平差模型进行统一求解，支持线性与非线性模型的统一处理。

其核心思想为 Gauss-Newton 迭代最小二乘估计：

```text
X(k+1) = X(k) + ΔX
其中 ΔX 由线性化后的法方程求解得到。
```
处理流程如下：

1. 从 IAdjustmentModel 接口获取：
  参数维数 t
  观测数 n
  初始近似值 X⁰
2. 初始化参数向量：X ← X⁰

3. 进入迭代过程（最多 MaxIterations 次）：
 3.1 调用 ILinearizable.Linearize：
  - 根据当前 X 计算模型线性化结果
  - 得到：
    - B（雅可比矩阵）
    - W（常数项）
    - L（观测向量）
    - P（权阵）

 3.2 调用 LeastSquaresSolver：
  - 解线性最小二乘问题：
    B · ΔX ≈ W
  - 得到参数改正数 ΔX

  3.3 更新参数：
    - X ← X + ΔX

  3.4 收敛性判断：
    - 若 max|ΔX| < Tol，则认为收敛成功并退出迭代

4. 输出非线性平差结果至new NonlinearResult：
    X̂：最终参数估计值
    LS：最后一次线性化的最小二乘解
    Iterations：迭代次数
    Success：是否收敛
    Report：迭代过程记录

### `LeastSquaresSolver.cs`

线性最小二乘解算核心模块（法方程法）

该模块用于求解标准加权最小二乘问题，是整个平差系统的基础数值计算内核。

其数学模型为：

```text
B · x = l + v

通过加权最小二乘准则：
min (vᵀ P v)

构造法方程：
N x = U

其中：
N = Bᵀ P B
U = Bᵀ P l
```
处理流程如下：

1. 接收输入矩阵：
  B：系数矩阵（设计矩阵 / 雅可比矩阵）
  l：常数项向量（闭合差 / 观测减理论）
  P：观测权阵
2. 计算加权法方程：
  N = Bᵀ P B
  U = Bᵀ P l
3. 求解未知参数改正数：
  x̂ = N⁻¹ U（通过矩阵求解实现）
4. 计算观测残差改正数：
  v = B x̂ − l
5. 输出结果结构 LeastSquaresResult：
  n：观测数
  t：未知数个数
  r：多余观测数
  N：法方程矩阵
  U：常数项向量
  xHat：参数改正数
  V：观测残差
  B：设计矩阵
  P：权阵
  W：原始常数项向量(l)

### `PrecisionEstimator.cs`
最小二乘平差精度评定模块（协因数与协方差分析）

该模块基于最小二乘估计结果，对平差系统的精度进行统计分析与误差传播计算，是测绘平差结果可靠性评价的核心组件。

其理论基础为：

```text
σ₀² = (Vᵀ P V) / r

QXX = N⁻¹
QLL = B · Qxx · Bᵀ
QVV = P⁻¹ - QLL
QXL = Qxx · Bᵀ

D = σ₀² · Q
```
处理流程:

1. 输入平差结果 LeastSquaresResult（包含 B、V、P、N 等）
2. 将残差统一换算为毫米单位（mm）
3. 计算单位权方差：σ₀² = Vᵀ P V / r
4. 构造协因数阵体系，并通过单位权方差放大为协方差阵，得到：
  Dxx：参数协方差阵
  Dll：观测值平差协方差阵
  Dvv：残差协方差阵
  Dxl：参数-观测协方差阵
5. 生成完整精度评定报告，包括：
- 单位权精度指标
  σ₀²（单位权方差）
  σ₀（单位权中误差）
- 协方差分析矩阵
  DVV（残差协方差阵）
  DLL（观测平差值协方差阵）
  DXX（参数协方差阵）
  DXL（参数-观测协方差阵）

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
  -> Foam1 构造输入数据
  -> ...Model 完成平差建模与线性化
  -> NonlinearLeastSquaresSolver 执行最小二乘平差解算
  -> PrecisionEstimator 进行平差结果的精度评定
  -> Form1 把结果写回点对象
  -> DrawingBoard 和结果面板刷新
```

也就是说：

- `AdjustmentProject` 是数据核心
- `DrawingBoard` 是图形交互层
- `Form1` 是界面协调层
- `ProjectDiagnostics` 是模型检查层
- `IAdjustmentModel` 是输入数据接口
- `ILinearizable` 是线性化接口
- `HeightModel` 和 `DistanceModel` 是网型建模层
- `NonlinearLeastSquaresSolver` 是方程求解层
- `LeastSquaresSolver` 是最小二乘计算核心层
- `LeastSquaresResult` 是单次计算的结果层
- `NonlinearResult` 是平差最终结果层
- `PrecisionEstimator` 是精度评定层
- `PrecisionResult` 是精度评定结果层

## 当前状态

这是早期原型版本，重点是验证“画板建模 + 平差求解”的基本技术路线。

已实现：

- 高程网建模
- 测边网建模
- 图形化建模
- 属性编辑
- 计算前诊断
- 统一的平差模型接口与线性化接口
- 误差方程最小二乘求解
- 平差结果精度评定
- 规范的单位系统
- 常用矩阵运算函数库
  
后续计划逐步加入：

- 测角网平差
- 边角网联合平差
- 更完整的结果表
- 项目保存与读取
- 规范的定权方法
