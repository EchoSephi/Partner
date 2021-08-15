using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Partner.Model
{
    public class Result1
    {
        public Result1(string json)
        {
            JObject jObject = JObject.Parse(json);
            JToken jUser = jObject;
            api = (string)jUser["api"];
            startTimestamp = (string)jUser["startTimestamp"];
            endTimestamp = (string)jUser["endTimestamp"];
            returnStatus = (string)jUser["returnStatus"];
            retrunMessage = (string)jUser["retrunMessage"];
            total = (string)jUser["total"];
            data = jUser["data"].ToArray();
        }

        public string api { get; set; } // API名稱
        public string startTimestamp { get; set; } // 開始日期時間
        public string endTimestamp { get; set; } // 結束日期時間
        public string returnStatus { get; set; } // 回傳狀態(S:成功 , F:失敗)
        public string retrunMessage { get; set; } // 回傳訊息
        public string total { get; set; } // 回傳總筆數
        public Array data { get; set; }
    }

    public class Result2
    {
        /*
            Brian:現場設備本身 就沒有pDC(n) , vStr(n)的累積值
                  我們系統，也不會 去加總這些
            pDC(n) 有值的話，可以用,不過 有時 是， pDC(n)無值、vStr(n)有值
            pDC(1) 的組成，通常是 vStr(1)+ vStr(2)，組合而成的
            電壓 是均值、 電流是加總
        */
        public Result2(string json)
        {
            JObject jObject = JObject.Parse(json);
            JToken jResult2 = jObject;
            dataNo = (string)jResult2["dataNo"];
            datatimeR = (string)jResult2["datatimeR"];
            acPf = (string)jResult2["acPf"];
            freq = (string)jResult2["freq"];
            dcPower = (string)jResult2["dcPower"];
            acPower = (string)jResult2["acPower"];

            dayPowerH = (string)jResult2["dayPowerH"];
            totalPowerH = (string)jResult2["totalPowerH"];
            temp = (string)jResult2["temp"];
            mateStat = (string)jResult2["mateStat"];
            mateWarn = (string)jResult2["mateWarn"];

            pDc1 = (string)jResult2["pDc1"];
            vDc1 = (string)jResult2["vDc1"];
            cDc1 = (string)jResult2["cDc1"];
            pDc2 = (string)jResult2["pDc2"];
            vDc2 = (string)jResult2["vDc2"];
            cDc2 = (string)jResult2["cDc2"];
            pDc3 = (string)jResult2["pDc3"];
            vDc3 = (string)jResult2["vDc3"];
            cDc3 = (string)jResult2["cDc3"];
            pDc4 = (string)jResult2["pDc4"];
            vDc4 = (string)jResult2["vDc4"];
            cDc4 = (string)jResult2["cDc4"];
            pDc5 = (string)jResult2["pDc5"];
            vDc5 = (string)jResult2["vDc5"];
            cDc5 = (string)jResult2["cDc5"];
            pDc6 = (string)jResult2["pDc6"];
            vDc6 = (string)jResult2["vDc6"];
            cDc6 = (string)jResult2["cDc6"];
            pDc7 = (string)jResult2["pDc7"];
            vDc7 = (string)jResult2["vDc7"];
            cDc7 = (string)jResult2["cDc7"];
            pDc8 = (string)jResult2["pDc8"];
            vDc8 = (string)jResult2["vDc8"];
            cDc8 = (string)jResult2["cDc8"];
            pDc9 = (string)jResult2["pDc9"];
            vDc9 = (string)jResult2["vDc9"];
            cDc9 = (string)jResult2["cDc9"];
            pDc10 = (string)jResult2["pDc10"];
            vDc10 = (string)jResult2["vDc10"];
            cDc10 = (string)jResult2["cDc10"];
            pDc11 = (string)jResult2["pDc11"];
            vDc11 = (string)jResult2["vDc11"];
            cDc11 = (string)jResult2["cDc11"];
            pDc12 = (string)jResult2["pDc12"];
            vDc12 = (string)jResult2["vDc12"];
            cDc12 = (string)jResult2["cDc12"];

            pAc1 = (string)jResult2["pAc1"];
            vAc1 = (string)jResult2["vAc1"];
            cAc1 = (string)jResult2["cAc1"];

            pAc2 = (string)jResult2["pAc2"];
            vAc2 = (string)jResult2["vAc2"];
            cAc2 = (string)jResult2["cAc2"];

            pAc3 = (string)jResult2["pAc3"];
            vAc3 = (string)jResult2["vAc3"];
            cAc3 = (string)jResult2["cAc3"];

            pStr1 = (string)jResult2["pStr1"];
            vStr1 = (string)jResult2["vStr1"];
            cStr1 = (string)jResult2["cStr1"];
            pStr2 = (string)jResult2["pStr2"];
            vStr2 = (string)jResult2["vStr2"];
            cStr2 = (string)jResult2["cStr2"];
            pStr3 = (string)jResult2["pStr3"];
            vStr3 = (string)jResult2["vStr3"];
            cStr3 = (string)jResult2["cStr3"];
            pStr4 = (string)jResult2["pStr4"];
            vStr4 = (string)jResult2["vStr4"];
            cStr4 = (string)jResult2["cStr4"];
            pStr5 = (string)jResult2["pStr5"];
            vStr5 = (string)jResult2["vStr5"];
            cStr5 = (string)jResult2["cStr5"];
            pStr6 = (string)jResult2["pStr6"];
            vStr6 = (string)jResult2["vStr6"];
            cStr6 = (string)jResult2["cStr6"];
            pStr7 = (string)jResult2["pStr7"];
            vStr7 = (string)jResult2["vStr7"];
            cStr7 = (string)jResult2["cStr7"];
            pStr8 = (string)jResult2["pStr8"];
            vStr8 = (string)jResult2["vStr8"];
            cStr8 = (string)jResult2["cStr8"];
            pStr9 = (string)jResult2["pStr9"];
            vStr9 = (string)jResult2["vStr9"];
            cStr9 = (string)jResult2["cStr9"];
            pStr10 = (string)jResult2["pStr10"];
            vStr10 = (string)jResult2["vStr10"];
            cStr10 = (string)jResult2["cStr10"];
            pStr11 = (string)jResult2["pStr11"];
            vStr11 = (string)jResult2["vStr11"];
            cStr11 = (string)jResult2["cStr11"];
            pStr12 = (string)jResult2["pStr12"];
            vStr12 = (string)jResult2["vStr12"];
            cStr12 = (string)jResult2["cStr12"];
            pStr13 = (string)jResult2["pStr13"];
            vStr13 = (string)jResult2["vStr13"];
            cStr13 = (string)jResult2["cStr13"];
            pStr14 = (string)jResult2["pStr14"];
            vStr14 = (string)jResult2["vStr14"];
            cStr14 = (string)jResult2["cStr14"];
            pStr15 = (string)jResult2["pStr15"];
            vStr15 = (string)jResult2["vStr15"];
            cStr15 = (string)jResult2["cStr15"];
            pStr16 = (string)jResult2["pStr16"];
            vStr16 = (string)jResult2["vStr16"];
            cStr16 = (string)jResult2["cStr16"];
            pStr17 = (string)jResult2["pStr17"];
            vStr17 = (string)jResult2["vStr17"];
            cStr17 = (string)jResult2["cStr17"];
            pStr18 = (string)jResult2["pStr18"];
            vStr18 = (string)jResult2["vStr18"];
            cStr18 = (string)jResult2["cStr18"];


        }

        public string dataNo { get; set; } // 設備識別碼
        public string datatimeR { get; set; } // 觀測時間
        public string acPf { get; set; } // 功率因素(PF)
        public string freq { get; set; } // 頻率
        public string dcPower { get; set; } // DC總功率(W)
        public string acPower { get; set; } // AC總功率(W)
        public string dayPowerH { get; set; } // 本日累積發電量(Wh)
        public string totalPowerH { get; set; } // 總累積發電(Wh)
        public string temp { get; set; } // 溫度(C)
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
    }

    public class Result3
    {
        public Result3(string json)
        {
            JObject jObject = JObject.Parse(json);
            JToken jResult2 = jObject;
            dataNo = (string)jResult2["dataNo"];
            datatimeR = (string)jResult2["datatimeR"];
            sunPower = (string)jResult2["sunPower"];
            sunPowerH = (string)jResult2["sunPowerH"];
            surfaceTemp = (string)jResult2["surfaceTemp"];
            backTemp = (string)jResult2["backTemp"];
            mateStat = (string)jResult2["mateStat"];
            mateWarn = (string)jResult2["mateWarn"];
        }

        public string dataNo { get; set; } // 設備識別碼
        public string datatimeR { get; set; } // 觀測時間
        public string sunPower { get; set; }
        public string sunPowerH { get; set; }
        public string surfaceTemp { get; set; }
        public string backTemp { get; set; }
        public string mateStat { get; set; } // 設備狀態碼
        public string mateWarn { get; set; } // 設備警示碼

    }

}