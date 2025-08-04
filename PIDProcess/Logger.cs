using System;
using System.IO;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System;

namespace PIDProcess
{
    public static class Logger
    {
        private static readonly string _logFilePath = "d:\\Desktop\\TraeJob\\PIDProcess\\V01\\app.log";

        static Logger()
        {
            Console.WriteLine("Logger initialized. Log path: " + _logFilePath);
            // 确保日志文件存在
            if (!File.Exists(_logFilePath))
            {
                try
                {
                    // 确保目录存在
                    string? directory = Path.GetDirectoryName(_logFilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        Console.WriteLine("Log directory created: " + directory);
                    }
                    File.Create(_logFilePath).Dispose();
                    Console.WriteLine("Log file created successfully at: " + _logFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error creating log file: " + ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        public static void Log([AllowNull] string message)
        {
            if (message == null)
            {
                message = "[NULL]";
            }
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
            Console.WriteLine(logEntry);
            try
            {
                File.AppendAllText(_logFilePath, logEntry);
                // 验证写入是否成功
                if (File.Exists(_logFilePath))
                {
                    long fileSize = new FileInfo(_logFilePath).Length;
                    Console.WriteLine($"Log written successfully. File size: {fileSize} bytes");
                }
                else
                {
                    Console.WriteLine("Log file does not exist after writing");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static void LogError(string message, Exception ex)
        {
            Log($"ERROR: {message} - {ex.Message}");
            Log(ex.StackTrace);
        }
    }
}