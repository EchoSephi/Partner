using System;

namespace Partner.Model
{
    public class dtoBillionwattsPower
    {
        public Guid Guid { get; set; }
        public Guid Collector_Guid { get; set; }
        public int Sort { get; set; }
        public double ACPower { get; set; }
        public double DCPower { get; set; }
        public double Sunshine { get; set; }
        public double TemperatureB { get; set; }
        public double TemperatureS { get; set; }
        public double Wind{ get; set; }
        public string STATUS { get; set; }
        public DateTime UploadTime { get; set; }
        public double vDc { get; set; }
        public double cDc { get; set; }
        public double vAc { get; set; }
        public double cAc { get; set; }
    }

    public class dtoIvt
    {
        public DateTime UploadTime { get; set; }
        public int Sort { get; set; }
        public double ACPower { get; set; } // AC功率(W)
    }

    public class dtoSunlight
    {
        public Guid Guid { get; set; }
        public Guid Collector_Guid { get; set; }
        public DateTime UploadTime { get; set; }
        public int Sort { get; set; }
        public double TValues { get; set; }
        public string DEBUG { get; set; }
    }

    public class dtoTemperature
    {
        public Guid Guid { get; set; }
        public Guid Collector_Guid { get; set; }
        public DateTime UploadTime { get; set; }
        public int Sort { get; set; }
        public double TValues { get; set; }
        public string DEBUG { get; set; }
    }

    public class dtoWindP
    {
        public Guid Guid { get; set; }
        public Guid Collector_Guid { get; set; }
        public DateTime UploadTime { get; set; }
        public int Sort { get; set; }
        public double TValues { get; set; }
        public string DEBUG { get; set; }

    }


}