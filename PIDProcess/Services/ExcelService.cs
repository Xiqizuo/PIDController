using System;
using System.Collections.Generic;
using System.IO;
using PIDProcess.Models;
using OfficeOpenXml;

namespace PIDProcess.Services
{
    public class ExcelService
    {
        private System.Timers.Timer? _monitoringTimer;
    private DateTime _lastFileModifiedTime;
    private List<DataModel> _cachedData = new List<DataModel>();

        public List<DataModel> ReadExcelData(string filePath)
        {
            var dataModels = new List<DataModel>();

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }

            string extension = Path.GetExtension(filePath).ToLower();

            if (extension == ".xlsx" || extension == ".xls")
            {
                // 处理Excel文件
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // 读取第一个工作表

                    int rowCount = worksheet.Dimension.Rows;

                    // 假设第一行是标题行，从第二行开始读取数据
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var dataModel = new DataModel
                        {
                            TimeStamp = worksheet.Cells[row, 1].GetValue<DateTime>(),
                            InputValue = worksheet.Cells[row, 2].GetValue<double>(),
                            SetPoint = worksheet.Cells[row, 3].GetValue<double>(),
                            OutputValue = 0 // 初始化为0，将由PID控制器计算
                        };

                        dataModels.Add(dataModel);
                    }
                }
            }
            else if (extension == ".csv")
            {
                // 处理CSV文件
                using (var reader = new StreamReader(filePath))
                {
                    // 跳过标题行
                    reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line != null)
                        {
                            var values = line.Split(',');
                            if (values.Length >= 3)
                            {
                                var dataModel = new DataModel
                                {
                                    TimeStamp = DateTime.Parse(values[0]),
                                    InputValue = double.Parse(values[1]),
                                    SetPoint = double.Parse(values[2]),
                                    OutputValue = 0 // 初始化为0，将由PID控制器计算
                                };

                                dataModels.Add(dataModel);
                            }
                        }
                    }
                }
            }
            else
            {
                throw new NotSupportedException($"Unsupported file format: {extension}");
            }

            return dataModels;
        }

        // 监控Excel文件变化的方法
        public event EventHandler<List<DataModel>> DataUpdated = delegate { };

        // 模拟实时读取Excel数据的方法
        // 添加一个属性来控制是否忽略文件修改时间
        public bool IgnoreFileModifiedTime { get; set; } = false;

        public void StartMonitoring(string filePath, int intervalMs = 100)
        {
            StopMonitoring(); // 确保之前的timer已停止

            // 初始化文件修改时间和缓存数据
            _lastFileModifiedTime = File.GetLastWriteTime(filePath);
            _cachedData = ReadExcelData(filePath);
            // 立即触发数据更新事件，确保首次启动监控时显示数据
            DataUpdated?.Invoke(this, _cachedData);

            _monitoringTimer = new System.Timers.Timer(intervalMs);
            _monitoringTimer.Elapsed += (sender, e) =>
            {
                try
                {
                    // 检查是否忽略文件修改时间或文件是否被修改
                    var currentModifiedTime = File.GetLastWriteTime(filePath);
                    if (IgnoreFileModifiedTime || currentModifiedTime > _lastFileModifiedTime)
                    {
                        _lastFileModifiedTime = currentModifiedTime;
                        _cachedData = ReadExcelData(filePath);
                        DataUpdated?.Invoke(this, _cachedData);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error reading Excel file", ex);
                }
            };
            _monitoringTimer.Start();
        }

        public void StopMonitoring()
        {
            if (_monitoringTimer != null && _monitoringTimer.Enabled)
            {
                _monitoringTimer.Stop();
                _monitoringTimer.Dispose();
                _monitoringTimer = null;
            }
        }
    }
}