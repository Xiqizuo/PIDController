using System;
using PIDProcess; // 添加对Logger的引用

namespace PIDProcess.Models
{
    public class PIDController
    {
        // PID参数
        public double Kp { get; set; } = 1.0;
        public double Ki { get; set; } = 0.0;
        public double Kd { get; set; } = 0.0;

        // 限制输出范围
        public double MinOutput { get; set; } = 0.0;
        public double MaxOutput { get; set; } = 100.0;

        // 内部变量
        private double _previousError = 0.0;
        private double _integral = 0.0;
        private DateTime _lastTime;

        public PIDController()
        {
            _lastTime = DateTime.Now;
        }

        public double Calculate(double setPoint, double processVariable)
        {
            Logger.Log($"PID Calculate called: setPoint={setPoint}, processVariable={processVariable}");
            DateTime now = DateTime.Now;
            double timeSpan = (now - _lastTime).TotalSeconds;
            _lastTime = now;

            // 计算误差
            double error = setPoint - processVariable;
            Logger.Log($"PID Error: {error}");

            // 积分项
            _integral += error * timeSpan;
            Logger.Log($"PID Integral: {_integral}");

            // 微分项
            double derivative = (error - _previousError) / timeSpan;
            Logger.Log($"PID Derivative: {derivative}");
            _previousError = error;

            // 计算PID输出
            double output = Kp * error + Ki * _integral + Kd * derivative;
            Logger.Log($"PID Raw Output: {output}");

            // 限制输出范围
            double clampedOutput = Math.Clamp(output, MinOutput, MaxOutput);
            Logger.Log($"PID Final Output: {clampedOutput}");
            return clampedOutput;
        }

        // 重置PID控制器状态
        public void Reset()
        {
            _previousError = 0.0;
            _integral = 0.0;
            _lastTime = DateTime.Now;
        }
    }
}