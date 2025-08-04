# PID控制器项目

## 项目概述
这是一个基于WPF的PID控制器应用程序，用于实时监控和控制过程变量。该应用程序提供了友好的用户界面，可以加载Excel数据，实时显示过程变量、设定值和输出值，并通过图表直观展示趋势变化。

## 功能特点
- 实时PID控制算法实现
- 参数可调（比例系数Kp、积分系数Ki、微分系数Kd）
- 输出范围限制（最小输出MinOutput、最大输出MaxOutput）
- Excel数据导入功能
- 数据表格和趋势图表双视图显示
- 详细日志记录
- PID控制器状态重置

## 使用方法
1. 运行应用程序
2. 点击"浏览..."按钮选择Excel数据文件
3. 设置PID参数（Kp、Ki、Kd）
4. 点击"开始监控"按钮启动PID控制
5. 通过"数据表格"和"趋势图表"选项卡查看数据
6. 点击"停止监控"按钮停止PID控制

## 技术栈
- C#
- WPF (Windows Presentation Foundation)
- .NET Framework
- OxyPlot（图表绘制库）
- Excel数据处理

## 项目结构
```
PIDProcess/
├── App.xaml                # 应用程序入口
├── MainWindow.xaml         # 主窗口
├── Models/
│   ├── PIDController.cs    # PID控制器核心实现
│   └── DataModel.cs        # 数据模型
├── ViewModels/
│   └── MainViewModel.cs    # 主视图模型
├── Services/
│   └── ExcelService.cs     # Excel服务
├── Converters/
│   └── BooleanToOppositeConverter.cs # 布尔值反转转换器
└── Logger.cs               # 日志记录器
```

## PID参数调整说明
PID控制器的性能取决于三个关键参数的调整：
- **比例系数(Kp)**：增大Kp会增加系统响应速度，但过大可能导致超调和不稳定
- **积分系数(Ki)**：消除稳态误差，增大Ki会加快消除误差的速度，但可能导致超调
- **微分系数(Kd)**：减小超调，提高稳定性，但过大可能导致系统对噪声敏感

建议调整顺序：先调Kp，再调Ki，最后调Kd。

## 数据可视化
应用程序使用OxyPlot库提供两种数据可视化方式：
- **数据表格**：显示详细的时间戳、输入值、设定值和输出值
- **趋势图表**：直观展示数据随时间的变化趋势

## 示例数据
项目根目录下提供了示例数据文件：
- SampleData.csv
- SampleData.xlsx

可以使用这些文件测试应用程序的功能。