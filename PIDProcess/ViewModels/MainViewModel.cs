using System;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using PIDProcess.Models;
using PIDProcess.Services;
using PIDProcess; // 添加对Logger的引用
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Wpf;

namespace PIDProcess.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly PIDController _pidController;
    private readonly ExcelService _excelService;
    private ObservableCollection<DataModel> _dataModels;
    private string _excelFilePath;
    private double _kp;
    private double _ki;
    private double _kd;
    private double _minOutput;
    private double _maxOutput;
    private bool _isMonitoring;
    private PlotModel _plotModel = null!;

        public ObservableCollection<DataModel> DataModels
        {
            get => _dataModels;
            set
            {
                _dataModels = value;
                OnPropertyChanged();
            }
        }

        public string ExcelFilePath
        {
            get => _excelFilePath;
            set
            {
                _excelFilePath = value;
                OnPropertyChanged();
            }
        }

        public double Kp
        {
            get => _kp;
            set
            {
                _kp = value;
                _pidController.Kp = value;
                OnPropertyChanged();
            }
        }

        public double Ki
        {
            get => _ki;
            set
            {
                _ki = value;
                _pidController.Ki = value;
                OnPropertyChanged();
            }
        }

        public double Kd
        {
            get => _kd;
            set
            {
                _kd = value;
                _pidController.Kd = value;
                OnPropertyChanged();
            }
        }

        public double MinOutput
        {
            get => _minOutput;
            set
            {
                _minOutput = value;
                _pidController.MinOutput = value;
                OnPropertyChanged();
            }
        }

        public double MaxOutput
        {
            get => _maxOutput;
            set
            {
                _maxOutput = value;
                _pidController.MaxOutput = value;
                OnPropertyChanged();
            }
        }

        public bool IsMonitoring
        {
            get => _isMonitoring;
            set
            {
                _isMonitoring = value;
                OnPropertyChanged();
            }
        }

        public PlotModel PlotModel
        {
            get => _plotModel;
            set
            {
                _plotModel = value;
                OnPropertyChanged();
            }
        }

        public ICommand StartMonitoringCommand { get; }
        public ICommand StopMonitoringCommand { get; }
        public ICommand BrowseFileCommand { get; }

        public MainViewModel()
        {
            Logger.Log("MainViewModel constructor started");
            StartMonitoringCommand = new RelayCommand(StartMonitoring);
            StopMonitoringCommand = new RelayCommand(StopMonitoring);
            BrowseFileCommand = new RelayCommand(BrowseFile);

            _pidController = new PIDController();
            _excelService = new ExcelService();
        _excelService.DataUpdated += OnDataUpdated;
        _dataModels = new ObservableCollection<DataModel>();
        DataModels = _dataModels;  // 设置属性以触发通知
        _excelFilePath = string.Empty;

            // 初始化图表
            Logger.Log("Initializing plot model");
            InitializePlotModel();

            // 设置默认值
        Kp = 0.3;
        Ki = 0.005;
        Kd = 0.05;
        MinOutput = 0;
        MaxOutput = 100;
        IsMonitoring = false;
            Logger.Log("MainViewModel constructor completed");
    }

        private void OnDataUpdated(object? sender, List<DataModel> data)
    {
        Logger.Log("Data updated received");
        // 应用PID控制算法到每个数据项
        foreach (var item in data)
        {
            item.OutputValue = _pidController.Calculate(item.SetPoint, item.InputValue);
        }

        // 实现增量更新
        if (Application.Current != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
            // 移除不再存在的项
            var itemsToRemove = _dataModels.Except(data).ToList();
            foreach (var item in itemsToRemove)
            {
                _dataModels.Remove(item);
            }

            // 添加或更新新项
            foreach (var newItem in data)
            {
                var existingItem = _dataModels.FirstOrDefault(item => item.TimeStamp == newItem.TimeStamp);
                if (existingItem != null)
                {
                    // 更新现有项
                    existingItem.InputValue = newItem.InputValue;
                    existingItem.SetPoint = newItem.SetPoint;
                    existingItem.OutputValue = newItem.OutputValue;
                }
                else
                {
                    // 添加新项
                    _dataModels.Add(newItem);
                }
            }

            // 按时间戳排序
            var sortedItems = _dataModels.OrderBy(item => item.TimeStamp).ToList();
            _dataModels.Clear();
            foreach (var item in sortedItems)
            {
                _dataModels.Add(item);
            }
            Logger.Log($"Updated data models count: {_dataModels.Count}");

            // 刷新图表
            if (_plotModel != null)
            {
                _plotModel.InvalidatePlot(true);
            }
        });
        }
    }

        public void StartMonitoring()
        {
            Logger.Log($"Start monitoring called. ExcelFilePath: {ExcelFilePath}, IsMonitoring: {IsMonitoring}");
            if (!string.IsNullOrEmpty(ExcelFilePath) && !IsMonitoring)
            {
                try
                {
                    // 检查文件是否存在
                    if (File.Exists(ExcelFilePath))
                    {
                        Logger.Log($"Excel file exists: {ExcelFilePath}");
                        // 设置忽略文件修改时间，确保数据定期更新
                        _excelService.IgnoreFileModifiedTime = true;
                        _excelService.StartMonitoring(ExcelFilePath);
                        IsMonitoring = true;
                        Logger.Log("Monitoring started successfully");
                    }
                    else
                    {
                        Logger.Log($"Excel file not found: {ExcelFilePath}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error starting monitoring", ex);
                }
            }
        }

        public void StopMonitoring()
        {
            if (IsMonitoring)
            {
                _excelService.StopMonitoring();
                IsMonitoring = false;
            }
        }

        private void InitializePlotModel()
        {
            Logger.Log("InitializePlotModel started");
            try
            {
                _plotModel = new PlotModel { Title = "PID控制过程趋势图" };
                Logger.Log("PlotModel created");

                // 添加坐标轴
                var xAxis = new DateTimeAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = "时间",
                    StringFormat = "HH:mm:ss"
                };
                var yAxis = new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Title = "数值"
                };
                _plotModel.Axes.Add(xAxis);
                _plotModel.Axes.Add(yAxis);
                Logger.Log("Axes added to plot model");

                // 添加输出值曲线
                var outputSeries = new LineSeries
                {
                    Title = "输出值",
                    Color = OxyColors.Blue,
                    MarkerType = MarkerType.None,
                    ItemsSource = _dataModels,
                    Mapping = item =>
                    {
                        var dataItem = (DataModel)item;
                        return new DataPoint(DateTimeAxis.ToDouble(dataItem.TimeStamp), dataItem.OutputValue);
                    }
                };

                // 添加设定值曲线
                var setPointSeries = new LineSeries
                {
                    Title = "设定值",
                    Color = OxyColors.Red,
                    StrokeThickness = 2,
                    LineStyle = LineStyle.Dash,
                    MarkerType = MarkerType.None,
                    ItemsSource = _dataModels,
                    Mapping = item =>
                    {
                        var dataItem = (DataModel)item;
                        return new DataPoint(DateTimeAxis.ToDouble(dataItem.TimeStamp), dataItem.SetPoint);
                    }
                };

                _plotModel.Series.Add(outputSeries);
                _plotModel.Series.Add(setPointSeries);
                Logger.Log("Series added to plot model");

                // 添加图例
                _plotModel.Legends.Add(new Legend { LegendPosition = LegendPosition.TopRight });
                Logger.Log("Legend added to plot model");

                PlotModel = _plotModel;
                Logger.Log("PlotModel property set");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in InitializePlotModel", ex);
            }
            Logger.Log("InitializePlotModel completed");
        }

        private void BrowseFile()
        {
            Logger.Log("BrowseFile called");
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls;*.csv|All Files|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ExcelFilePath = openFileDialog.FileName;
                Logger.Log($"ExcelFilePath set to: {ExcelFilePath}");
            }
        }

        //  RelayCommand 实现
        private class RelayCommand : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool>? _canExecute;

            public event EventHandler? CanExecuteChanged = delegate { };

            public RelayCommand(Action execute, Func<bool>? canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public bool CanExecute(object? parameter)
            {
                return _canExecute == null || _canExecute();
            }

            public void Execute(object? parameter)
            {
                _execute();
            }

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
        }
    }
}