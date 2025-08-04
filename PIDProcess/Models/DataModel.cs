using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PIDProcess.Models
{
    public class DataModel
    {
        public DateTime TimeStamp { get; set; }
        public double InputValue { get; set; }
        public double SetPoint { get; set; }
        public double OutputValue { get; set; }
    }
}