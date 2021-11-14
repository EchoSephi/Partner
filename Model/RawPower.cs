using System;

namespace Bill.Model
{
    public class RawPower
    {
        public Guid Guid { get; set; }
        public Guid Collector_Guid { get; set; }
        public int Sort { get; set; }
        public double dayPowerHs { get; set; }
        public double Sunshine { get; set; }
        public double TemperatureS { get; set; }
        public double TemperatureB { get; set; }
        public double Wind { get; set; }
        public string dataNo { get; set; } // 設備識別碼
        public DateTime datatimeR { get; set; } // 觀測時間
        public double acPf { get; set; } // 功率因素(PF)
        public double freq { get; set; } // 頻率
        public double dcPower { get; set; } // DC總功率(W)
        public double acPower { get; set; } // AC總功率(W)
        public double dayPowerH { get; set; } // 本日累積發電量(Wh)
        public double totalPowerH { get; set; } // 總累積發電(Wh)
        public double temp { get; set; } // 溫度(C)
        public string mateStat { get; set; } // 設備狀態碼
        public string mateWarn { get; set; } // 設備警示碼

        // pDC:DC功率(W)
        // vDC:DC電壓(V)
        // cDC:DC電流(I)
        public string pDc1 { get; set; }
        public string vDc1 { get; set; }
        public string cDc1 { get; set; }

        public string pDc2 { get; set; }
        public string vDc2 { get; set; }
        public string cDc2 { get; set; }

        public string pDc3 { get; set; }
        public string vDc3 { get; set; }
        public string cDc3 { get; set; }

        public string pDc4 { get; set; }
        public string vDc4 { get; set; }
        public string cDc4 { get; set; }

        public string pDc5 { get; set; }
        public string vDc5 { get; set; }
        public string cDc5 { get; set; }

        public string pDc6 { get; set; }
        public string vDc6 { get; set; }
        public string cDc6 { get; set; }

        public string pDc7 { get; set; }
        public string vDc7 { get; set; }
        public string cDc7 { get; set; }

        public string pDc8 { get; set; }
        public string vDc8 { get; set; }
        public string cDc8 { get; set; }

        public string pDc9 { get; set; }
        public string vDc9 { get; set; }
        public string cDc9 { get; set; }

        public string pDc10 { get; set; }
        public string vDc10 { get; set; }
        public string cDc10 { get; set; }

        public string pDc11 { get; set; }
        public string vDc11 { get; set; }
        public string cDc11 { get; set; }

        public string pDc12 { get; set; }
        public string vDc12 { get; set; }
        public string cDc12 { get; set; }

        // pAC:AC功率(W)
        // vAC:AC電壓(V)
        // cAC:AC電流(I)
        public string pAc1 { get; set; }
        public string vAc1 { get; set; }
        public string cAc1 { get; set; }

        public string pAc2 { get; set; }
        public string vAc2 { get; set; }
        public string cAc2 { get; set; }

        public string pAc3 { get; set; }
        public string vAc3 { get; set; }
        public string cAc3 { get; set; }

        // pStr:組串功率(W)
        // vStr:組串電壓(V)
        // cStr:組串電流(I)

        public string pStr1 { get; set; }
        public string vStr1 { get; set; }
        public string cStr1 { get; set; }
        public string pStr2 { get; set; }
        public string vStr2 { get; set; }
        public string cStr2 { get; set; }
        public string pStr3 { get; set; }
        public string vStr3 { get; set; }
        public string cStr3 { get; set; }
        public string pStr4 { get; set; }
        public string vStr4 { get; set; }
        public string cStr4 { get; set; }
        public string pStr5 { get; set; }
        public string vStr5 { get; set; }
        public string cStr5 { get; set; }
        public string pStr6 { get; set; }
        public string vStr6 { get; set; }
        public string cStr6 { get; set; }
        public string pStr7 { get; set; }
        public string vStr7 { get; set; }
        public string cStr7 { get; set; }
        public string pStr8 { get; set; }
        public string vStr8 { get; set; }
        public string cStr8 { get; set; }
        public string pStr9 { get; set; }
        public string vStr9 { get; set; }
        public string cStr9 { get; set; }
        public string pStr10 { get; set; }
        public string vStr10 { get; set; }
        public string cStr10 { get; set; }
        public string pStr11 { get; set; }
        public string vStr11 { get; set; }
        public string cStr11 { get; set; }
        public string pStr12 { get; set; }
        public string vStr12 { get; set; }
        public string cStr12 { get; set; }
        public string pStr13 { get; set; }
        public string vStr13 { get; set; }
        public string cStr13 { get; set; }
        public string pStr14 { get; set; }
        public string vStr14 { get; set; }
        public string cStr14 { get; set; }
        public string pStr15 { get; set; }
        public string vStr15 { get; set; }
        public string cStr15 { get; set; }
        public string pStr16 { get; set; }
        public string vStr16 { get; set; }
        public string cStr16 { get; set; }
        public string pStr17 { get; set; }
        public string vStr17 { get; set; }
        public string cStr17 { get; set; }
        public string pStr18 { get; set; }
        public string vStr18 { get; set; }
        public string cStr18 { get; set; }
        public DateTime CreateTime { get; set; }
    }
}