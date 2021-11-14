using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Bill.Helper;
using Bill.Model;

namespace Bill
{
    class Program
    {
        public static string connSolar { get; set; }
        public static string connRoot { get; set; }
        public static string connBill { get; set; }
        public static string account { get; set; }
        public static string password { get; set; }
        public static string url { get; set; }
        static async Task Main(string[] args)
        {

            connRoot = Tool.ReadFromAppSettings().Get<SolarModel>().Root;
            connSolar = Tool.ReadFromAppSettings().Get<SolarModel>().Solar;
            connBill = Tool.ReadFromAppSettings().Get<SolarModel>().Bill;
            account = Tool.ReadFromAppSettings().Get<SolarModel>().account;
            password = Tool.ReadFromAppSettings().Get<SolarModel>().password;
            url = Tool.ReadFromAppSettings().Get<SolarModel>().url;

            int ts = 5;

            if (args.Length == 0)
            {
                ts = 5;
            }
            else if (args.Length == 1)
            {
                int num;
                bool test = int.TryParse(args[0], out num);
                ts = num;
            }

            await Satrt(ts);

        }

        public static async Task Satrt(int ts)
        {

            // * 1.取得api廠商資料
            var q = await FetchBill();
            foreach (var p in q)
            {
                var account = p.Account;
                var password = p.Password;
                var url = p.UrlAddress;

                // * 2.讀取db -- 最後更新時間
                var q1 = await FetchCollectors(p.Cases_Guid);
                foreach (var p1 in q1)
                {
                    var CollectorId = p1.Guid;
                    var siteNo = p1.MacAddress;
                    var lt = p1.LastUploadTime2;

                    if (lt != DateTime.Parse("2000-01-01 00:00:00.000"))
                    {
                        CS1(p.Cases_Name);

                        var startDatetime = lt.AddMinutes(1).ToString("yyyy-MM-dd HH:mm:ss");
                        var endDatetime = lt.AddMinutes(ts).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        if (lt.AddMinutes(ts) <= DateTime.Now)
                        {
                            if (lt.Hour < 20 && lt.Hour > 4)
                            {

                                var ds = lt.ToString("yyyy-MM-dd HH:00:00.000");
                                var de = lt.AddMinutes(ts).ToString("yyyy-MM-dd HH:59:00.000");
                                int timeStamp = Convert.ToInt32(DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
                                var sec = password + timeStamp;
                                var token = Tool.MD5code(sec) + account;

                                #region 逆變器
                                var q2 = await FetchInverters(CollectorId);
                                foreach (var p2 in q2)
                                {
                                    var dataNo = p2.SerialNumber;
                                    var Sort = p2.Sort;
                                    await FetchPower(siteNo, dataNo, startDatetime, endDatetime, token, timeStamp, url, Sort, CollectorId);
                                }
                                #endregion

                                #region 日照計
                                var q3 = await FetchSunlightMeter(CollectorId);
                                if (q3 != null)
                                {
                                    int i = 1;
                                    foreach (var p3 in q3)
                                    {
                                        var dataNo = p3.SerialNumber;
                                        var Sort = p3.Sort;
                                        await FetchSunPower(siteNo, dataNo, startDatetime, endDatetime, token, timeStamp, url, Sort, CollectorId, i);
                                        i++;
                                    }
                                }

                                #endregion

                                #region 環境溫度計
                                var q4 = await FetchTempSurface(CollectorId);
                                if (q4 != null)
                                {
                                    foreach (var p4 in q4)
                                    {
                                        var dataNo = p4.SerialNumber;
                                        var Sort = p4.Sort;
                                        await FetchTempSurfacePower(siteNo, dataNo, startDatetime, endDatetime, token, timeStamp, url, Sort, CollectorId);

                                    }
                                }

                                #endregion

                                #region 模組溫度計
                                var q5 = await FetchTempBack(CollectorId);
                                if (q5 != null)
                                {
                                    foreach (var p5 in q5)
                                    {
                                        var dataNo = p5.SerialNumber;
                                        var Sort = p5.Sort;
                                        await FetchTempBackPower(siteNo, dataNo, startDatetime, endDatetime, token, timeStamp, url, Sort, CollectorId);
                                    }
                                }

                                #endregion

                                #region 風速計
                                var q6 = await FetchWind(CollectorId);
                                if (q6 != null)
                                {
                                    foreach (var p6 in q6)
                                    {
                                        var dataNo = p6.SerialNumber;
                                        var Sort = p6.Sort;
                                        await FetchWindPower(siteNo, dataNo, startDatetime, endDatetime, token, timeStamp, url, Sort, CollectorId);
                                    }
                                }

                                #endregion
                            }
                            await setLastDay(CollectorId, endDatetime);
                        }
                    }
                }
            }
            CS1("ENd");
        }

        #region 逆變器
        public static async Task FetchPower(string siteNo, string dataNo, string startDatetime, string endDatetime, string token, int timeStamp, string url, int Sort, Guid CollectorId)
        {
            var _Para = new JObject();
            _Para["siteNo"] = siteNo;
            _Para["dataNo"] = dataNo;
            _Para["startDatetime"] = startDatetime;
            _Para["endDatetime"] = endDatetime;

            var _Raw = new JObject();
            _Raw["api"] = "ctInverterRawData";
            _Raw["token"] = token;
            _Raw["langCode"] = "zh_TW";
            _Raw["sendTimestamp"] = timeStamp;
            _Raw["para"] = _Para;

            string JsonString = JsonConvert.SerializeObject(_Raw);
            var str = string.Format("逆變器 dataNo:{0}:{1}", dataNo, startDatetime);
            CS1(str);

            try
            {

                using (var client = new HttpClient())
                {
                    var res = client.PostAsync(url, new StringContent(JsonString, Encoding.UTF8, "application/json")).GetAwaiter().GetResult(); ;
                    var res2 = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Result1 r = new Result1(res2);
                    foreach (var p in r.data)
                    {
                        var p1 = new Result2(p.ToString());
                        var up = p1.datatimeR;


                        await toRawPower(p1, CollectorId, Sort, dataNo, DateTime.Parse(up));

                        if (p1.mateWarn != "")
                        {
                            var err = new dtoError();
                            err.Guid = Guid.NewGuid();
                            err.Collector_Guid = CollectorId;
                            err.Types = "逆變器";
                            err.Sort = Sort;
                            err.MateStat = p1.mateStat;
                            err.MateWarn = p1.mateWarn;
                            err.UploadTime = DateTime.Parse(up);
                            await toError(err);
                        }

                    }

                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("FetchPower : " + e.Message.ToString());
            }
        }

        public static async Task toRawPower(Result2 p1, Guid CollectorId, int Sort, string dataNo, DateTime up)
        {

            var raw = new RawPower();
            raw.Guid = Guid.NewGuid();
            raw.Collector_Guid = CollectorId;
            raw.Sort = Sort;

            raw.dataNo = dataNo;
            raw.datatimeR = up;
            raw.acPf = NullToZero(p1.acPf);
            raw.freq = NullToZero(p1.freq);
            raw.dcPower = NullToZero(p1.dcPower);
            raw.acPower = NullToZero(p1.acPower);
            raw.dayPowerH = NullToZero(p1.dayPowerH);
            raw.totalPowerH = NullToZero(p1.totalPowerH);
            raw.temp = NullToZero(p1.temp);
            raw.mateStat = NullToString(p1.mateStat);
            raw.mateWarn = NullToString(p1.mateWarn);

            var last = await LastDayPowerH(CollectorId, up, Sort);
            var dayPowerHs = raw.dayPowerH - last;
            // todo test
            if (dayPowerHs < 0)
            {
                dayPowerHs = 0.0;
            }
            raw.dayPowerHs = dayPowerHs;
            raw.Sunshine = 0.0;
            raw.TemperatureS = 0.0;
            raw.TemperatureB = 0.0;
            raw.Wind = 0.0;

            raw.pDc1 = NullToString(p1.pDc1);
            raw.vDc1 = NullToString(p1.vDc1);
            raw.cDc1 = NullToString(p1.cDc1);
            raw.pDc2 = NullToString(p1.pDc2);
            raw.vDc2 = NullToString(p1.vDc2);
            raw.cDc2 = NullToString(p1.cDc2);
            raw.pDc3 = NullToString(p1.pDc3);
            raw.vDc3 = NullToString(p1.vDc3);
            raw.cDc3 = NullToString(p1.cDc3);
            raw.pDc4 = NullToString(p1.pDc4);
            raw.vDc4 = NullToString(p1.vDc4);
            raw.cDc4 = NullToString(p1.cDc4);
            raw.pDc5 = NullToString(p1.pDc5);
            raw.vDc5 = NullToString(p1.vDc5);
            raw.cDc5 = NullToString(p1.cDc5);
            raw.pDc6 = NullToString(p1.pDc6);
            raw.vDc6 = NullToString(p1.vDc6);
            raw.cDc6 = NullToString(p1.cDc6);
            raw.pDc7 = NullToString(p1.pDc7);
            raw.vDc7 = NullToString(p1.vDc7);
            raw.cDc7 = NullToString(p1.cDc7);
            raw.pDc8 = NullToString(p1.pDc8);
            raw.vDc8 = NullToString(p1.vDc8);
            raw.cDc8 = NullToString(p1.cDc8);
            raw.pDc9 = NullToString(p1.pDc9);
            raw.vDc9 = NullToString(p1.vDc9);
            raw.cDc9 = NullToString(p1.cDc9);
            raw.pDc10 = NullToString(p1.pDc10);
            raw.vDc10 = NullToString(p1.vDc10);
            raw.cDc10 = NullToString(p1.cDc10);
            raw.pDc11 = NullToString(p1.pDc11);
            raw.vDc11 = NullToString(p1.vDc11);
            raw.cDc11 = NullToString(p1.cDc11);
            raw.pDc12 = NullToString(p1.pDc12);
            raw.vDc12 = NullToString(p1.vDc12);
            raw.cDc12 = NullToString(p1.cDc12);
            raw.pAc1 = NullToString(p1.pAc1);
            raw.vAc1 = NullToString(p1.vAc1);
            raw.cAc1 = NullToString(p1.cAc1);
            raw.pAc2 = NullToString(p1.pAc2);
            raw.vAc2 = NullToString(p1.vAc2);
            raw.cAc2 = NullToString(p1.cAc2);
            raw.pAc3 = NullToString(p1.pAc3);
            raw.vAc3 = NullToString(p1.vAc3);
            raw.cAc3 = NullToString(p1.cAc3);
            raw.pStr1 = NullToString(p1.pStr1);
            raw.vStr1 = NullToString(p1.vStr1);
            raw.cStr1 = NullToString(p1.cStr1);
            raw.pStr2 = NullToString(p1.pStr2);
            raw.vStr2 = NullToString(p1.vStr2);
            raw.cStr2 = NullToString(p1.cStr2);
            raw.pStr3 = NullToString(p1.pStr3);
            raw.vStr3 = NullToString(p1.vStr3);
            raw.cStr3 = NullToString(p1.cStr3);
            raw.pStr4 = NullToString(p1.pStr4);
            raw.vStr4 = NullToString(p1.vStr4);
            raw.cStr4 = NullToString(p1.cStr4);
            raw.pStr5 = NullToString(p1.pStr5);
            raw.vStr5 = NullToString(p1.vStr5);
            raw.cStr5 = NullToString(p1.cStr5);
            raw.pStr6 = NullToString(p1.pStr6);
            raw.vStr6 = NullToString(p1.vStr6);
            raw.cStr6 = NullToString(p1.cStr6);
            raw.pStr7 = NullToString(p1.pStr7);
            raw.vStr7 = NullToString(p1.vStr7);
            raw.cStr7 = NullToString(p1.cStr7);
            raw.pStr8 = NullToString(p1.pStr8);
            raw.vStr8 = NullToString(p1.vStr8);
            raw.cStr8 = NullToString(p1.cStr8);
            raw.pStr9 = NullToString(p1.pStr9);
            raw.vStr9 = NullToString(p1.vStr9);
            raw.cStr9 = NullToString(p1.cStr9);
            raw.pStr10 = NullToString(p1.pStr10);
            raw.vStr10 = NullToString(p1.vStr10);
            raw.cStr10 = NullToString(p1.cStr10);
            raw.pStr11 = NullToString(p1.pStr11);
            raw.vStr11 = NullToString(p1.vStr11);
            raw.cStr11 = NullToString(p1.cStr11);
            raw.pStr12 = NullToString(p1.pStr12);
            raw.vStr12 = NullToString(p1.vStr12);
            raw.cStr12 = NullToString(p1.cStr12);
            raw.pStr13 = NullToString(p1.pStr13);
            raw.vStr13 = NullToString(p1.vStr13);
            raw.cStr13 = NullToString(p1.cStr13);
            raw.pStr14 = NullToString(p1.pStr14);
            raw.vStr14 = NullToString(p1.vStr14);
            raw.cStr14 = NullToString(p1.cStr14);
            raw.pStr15 = NullToString(p1.pStr15);
            raw.vStr15 = NullToString(p1.vStr15);
            raw.cStr15 = NullToString(p1.cStr15);
            raw.pStr16 = NullToString(p1.pStr16);
            raw.vStr16 = NullToString(p1.vStr16);
            raw.cStr16 = NullToString(p1.cStr16);
            raw.pStr17 = NullToString(p1.pStr17);
            raw.vStr17 = NullToString(p1.vStr17);
            raw.cStr17 = NullToString(p1.cStr17);
            raw.pStr18 = NullToString(p1.pStr18);
            raw.vStr18 = NullToString(p1.vStr18);
            raw.cStr18 = NullToString(p1.cStr18);
            raw.CreateTime = DateTime.Now;
            var insertQuery = "insert into RawPower VALUES " +
                "(@Guid ,@Collector_Guid ,@Sort ,@dayPowerHs ,@Sunshine ,@TemperatureS,@TemperatureB,@Wind" +
                ",@dataNo,@datatimeR ,@acPf,@freq,@dcPower ,@acPower ,@dayPowerH ,@totalPowerH ,@temp,@mateStat,@mateWarn" +
                ",@pDc1,@vDc1,@cDc1,@pDc2,@vDc2,@cDc2,@pDc3,@vDc3,@cDc3,@pDc4,@vDc4,@cDc4,@pDc5,@vDc5,@cDc5,@pDc6,@vDc6,@cDc6,@pDc7,@vDc7,@cDc7" +
                ",@pDc8,@vDc8,@cDc8,@pDc9,@vDc9,@cDc9,@pDc10,@vDc10,@cDc10,@pDc11,@vDc11,@cDc11,@pDc12,@vDc12,@cDc12" +
                ",@pAc1,@vAc1,@cAc1,@pAc2,@vAc2,@cAc2,@pAc3,@vAc3,@cAc3,@pStr1,@vStr1,@cStr1,@pStr2,@vStr2,@cStr2" +
                ",@pStr3,@vStr3,@cStr3,@pStr4,@vStr4,@cStr4,@pStr5,@vStr5,@cStr5,@pStr6,@vStr6,@cStr6,@pStr7,@vStr7,@cStr7" +
                ",@pStr8,@vStr8,@cStr8,@pStr9,@vStr9,@cStr9,@pStr10,@vStr10,@cStr10,@pStr11,@vStr11,@cStr11,@pStr12,@vStr12,@cStr12" +
                ",@pStr13,@vStr13,@cStr13,@pStr14,@vStr14,@cStr14,@pStr15,@vStr15,@cStr15" +
                ",@pStr16,@vStr16,@cStr16,@pStr17,@vStr17,@cStr17,@pStr18,@vStr18,@cStr18,@CreateTime)";
            using (var cn = new SqlConnection(connBill))
            {
                var result = await cn.ExecuteAsync(insertQuery, raw);
            }
        }

        public static async Task<double> LastDayPowerH(Guid CollectorId, DateTime up, int Sort)
        {
            using (var cn = new SqlConnection(connBill))
            {
                var sqlstr = string.Format("execute SP_GetLastDayPowerH '{0}','{1}',{2};", CollectorId, up.ToString("yyyy-MM-dd HH:mm:ss.fff"), Sort);
                var result = await cn.QueryAsync<dtoDayPowerH>(sqlstr);
                if (result.FirstOrDefault() != null)
                {
                    return result.FirstOrDefault().dayPowerH;
                }
                else
                {
                    return 0.0;
                }

            }
        }

        public static async Task toError(dtoError dto)
        {
            try
            {
                using (var cn = new SqlConnection(connSolar))
                {
                    string insertQuery = @"INSERT INTO BillionwattsError (Guid, Collector_Guid, Types , Sort, MateStat, MateWarn , UploadTime) " +
                                        "VALUES (@Guid, @Collector_Guid, @Types , @Sort, @MateStat, @MateWarn, @UploadTime )";
                    var result = await cn.ExecuteAsync(insertQuery, dto);
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("toError :" + e.Message.ToString());
            }
        }


        // 宜蘭展鋒案場 專用??
        // public static async Task toDB3(Guid CollectorId, string startDatetime, string endDatetime)
        // {
        //     var SqlStr = string.Format("Execute SP_PowertoHour '{0}' , '{1}' , '{2}' ; ", CollectorId.ToString(), startDatetime, endDatetime);
        //     using (var cn = new SqlConnection(connSolar))
        //     {
        //         try
        //         {
        //             await cn.ExecuteAsync(SqlStr);
        //         }
        //         catch (System.Exception e)
        //         {
        //             Console.WriteLine("toDB3 :" + e.Message.ToString());
        //         }
        //     }
        // }
        public static async Task setLastDay(Guid CollectorId, string up)
        {
            using (var cn = new SqlConnection(connRoot))
            {
                var SqlStr = string.Format("update Collector set LastUploadTime2 = '{1}' where Guid = '{0}';", CollectorId.ToString(), up);
                await cn.ExecuteAsync(SqlStr);
            }
        }

        public static async Task<List<dtoBills>> FetchBill()
        {
            try
            {
                using (var cn = new SqlConnection(connRoot))
                {
                    var SqlStr1 = string.Format("Execute SP_GetPartners;");
                    var q = await cn.QueryAsync<dtoBills>(SqlStr1);
                    if (q.FirstOrDefault() != null)
                    {
                        return q.ToList();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("FetchBill :" + e.Message.ToString());
                return null;
            }

        }

        public static async Task<List<dtoCollector>> FetchCollectors(Guid CaseId)
        {
            try
            {
                using (var cn = new SqlConnection(connBill))
                {
                    var SqlStr1 = string.Format("Execute SP_GetCollectors '{0}';", CaseId.ToString());
                    var q = await cn.QueryAsync<dtoCollector>(SqlStr1);
                    if (q.FirstOrDefault() != null)
                    {
                        return q.ToList();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("FetchCollectors :" + e.Message.ToString());
                return null;
            }

        }

        public static async Task<List<dtoInverters>> FetchInverters(Guid CollectorId)
        {
            try
            {
                using (var cn = new SqlConnection(connRoot))
                {
                    var SqlStr1 = string.Format("Execute SP_GetInverters '{0}';", CollectorId.ToString());
                    var q = await cn.QueryAsync<dtoInverters>(SqlStr1);
                    if (q.FirstOrDefault() != null)
                    {
                        return q.ToList();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("FetchInverters :" + e.Message.ToString());
                return null;
            }

        }

        #endregion

        #region 日照計
        public static async Task<List<dtoSunlightMeter>> FetchSunlightMeter(Guid CollectorId)
        {
            try
            {
                using (var cn = new SqlConnection(connRoot))
                {
                    var SqlStr1 = string.Format("Execute SP_GetSunlightMeter '{0}';", CollectorId.ToString());
                    var q = await cn.QueryAsync<dtoSunlightMeter>(SqlStr1);
                    if (q.FirstOrDefault() == null)
                    {
                        return null;
                    }
                    else
                    {
                        return q.ToList();
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("FetchSunlightMeter :" + e.Message.ToString());
                return null;
            }
        }

        public static async Task FetchSunPower(string siteNo, string dataNo, string startDatetime, string endDatetime, string token, int timeStamp, string url, int Sort, Guid CollectorId, int times)
        {
            var _Para = new JObject();
            _Para["siteNo"] = siteNo;
            _Para["dataNo"] = dataNo;
            _Para["startDatetime"] = startDatetime;
            _Para["endDatetime"] = endDatetime;

            var _Raw = new JObject();
            _Raw["api"] = "ctActinometerRawData";
            _Raw["token"] = token;
            _Raw["langCode"] = "zh_TW";
            _Raw["sendTimestamp"] = timeStamp;
            _Raw["para"] = _Para;

            string JsonString = JsonConvert.SerializeObject(_Raw);
            var str = string.Format("日照計 dataNo:{0}:{1}", dataNo, startDatetime);
            CS1(str);
            try
            {
                var l = new List<dtoSunlight>();
                using (var client = new HttpClient())
                {
                    var res = client.PostAsync(url, new StringContent(JsonString, Encoding.UTF8, "application/json")).GetAwaiter().GetResult(); ;
                    var res2 = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Result1 r = new Result1(res2);
                    foreach (var p in r.data)
                    {
                        var p1 = new Result3(p.ToString());
                        var sunPower = 0.0;
                        if (p1.sunPower != null)
                        {
                            sunPower = double.Parse(p1.sunPower);
                        }

                        var up = p1.datatimeR;

                        var b = new dtoSunlight();
                        b.Guid = Guid.NewGuid();
                        b.Collector_Guid = CollectorId;
                        b.UploadTime = DateTime.Parse(up);
                        b.Sort = Sort;
                        b.TValues = Math.Round(sunPower, 2);
                        b.DEBUG = "";

                        if (p1.mateWarn != "")
                        {
                            var err = new dtoError();
                            err.Guid = Guid.NewGuid();
                            err.Collector_Guid = CollectorId;
                            err.Types = "日照計";
                            err.Sort = Sort;
                            err.MateStat = p1.mateStat;
                            err.MateWarn = p1.mateWarn;
                            err.UploadTime = DateTime.Parse(up);
                            await toError(err);
                        }

                        l.Add(b);
                    }
                }
                // * 寫入db
                await toDBSunPower(l, times);
            }
            catch (System.Exception e)
            {
                Console.WriteLine("FetchSunPower : " + e.Message.ToString());
            }
        }

        public static async Task toDBSunPower(List<dtoSunlight> bsp, int times)
        {
            try
            {
                using (var cn = new SqlConnection(connBill))
                {
                    foreach (var p in bsp)
                    {
                        string updtaeQuery = "";
                        if (times == 1)
                        {
                            updtaeQuery = string.Format("update RawPower set Sunshine = {0} where datatimeR = '{1}' and Collector_Guid = '{2}';"
                                                   , p.TValues, p.UploadTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), p.Collector_Guid);

                        }
                        else
                        {
                            updtaeQuery = string.Format("update RawPower set Sunshine = (Sunshine + {0}) / {3} where datatimeR = '{1}' and Collector_Guid = '{2}';"
                                                   , p.TValues, p.UploadTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), p.Collector_Guid, times);

                        }
                        await cn.ExecuteAsync(updtaeQuery);
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("toDBSunPower : " + e.Message.ToString());
            }
        }

        #endregion

        #region 環境溫度計
        public static async Task<List<dtoTempSurface>> FetchTempSurface(Guid CollectorId)
        {
            try
            {
                using (var cn = new SqlConnection(connRoot))
                {
                    var SqlStr1 = string.Format("Execute SP_GetTempSurface '{0}';", CollectorId.ToString());
                    var q = await cn.QueryAsync<dtoTempSurface>(SqlStr1);
                    if (q.FirstOrDefault() != null)
                    {
                        return q.ToList();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("FetchTempSurface : " + e.Message.ToString());
                return null;
            }
        }

        public static async Task FetchTempSurfacePower(string siteNo, string dataNo, string startDatetime, string endDatetime, string token, int timeStamp, string url, int Sort, Guid CollectorId)
        {
            var _Para = new JObject();
            _Para["siteNo"] = siteNo;
            _Para["dataNo"] = dataNo;
            _Para["startDatetime"] = startDatetime;
            _Para["endDatetime"] = endDatetime;

            var _Raw = new JObject();
            _Raw["api"] = "ctActinometerRawData";
            _Raw["token"] = token;
            _Raw["langCode"] = "zh_TW";
            _Raw["sendTimestamp"] = timeStamp;
            _Raw["para"] = _Para;

            string JsonString = JsonConvert.SerializeObject(_Raw);
            var str = string.Format("環境溫度計 dataNo:{0}:{1}", dataNo, startDatetime);
            CS1(str);
            try
            {
                var l = new List<dtoTemperature>();
                using (var client = new HttpClient())
                {
                    var res = client.PostAsync(url, new StringContent(JsonString, Encoding.UTF8, "application/json")).GetAwaiter().GetResult(); ;
                    var res2 = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Result1 r = new Result1(res2);
                    foreach (var p in r.data)
                    {
                        var p1 = new Result3(p.ToString());
                        var surfaceTemp = 0.0;
                        if (p1.surfaceTemp != null)
                        {
                            surfaceTemp = double.Parse(p1.surfaceTemp);
                        }
                        else
                        {
                            if (p1.backTemp != null)
                            {
                                surfaceTemp = double.Parse(p1.backTemp);
                            }
                        }

                        var up = p1.datatimeR;

                        var b = new dtoTemperature();
                        b.Guid = Guid.NewGuid();
                        b.Collector_Guid = CollectorId;
                        b.UploadTime = DateTime.Parse(up);
                        b.Sort = Sort;
                        b.TValues = Math.Round(surfaceTemp, 2);
                        b.DEBUG = "";

                        if (p1.mateWarn != "")
                        {
                            var err = new dtoError();
                            err.Guid = Guid.NewGuid();
                            err.Collector_Guid = CollectorId;
                            err.Types = "環境溫度計";
                            err.Sort = Sort;
                            err.MateStat = p1.mateStat;
                            err.MateWarn = p1.mateWarn;
                            err.UploadTime = DateTime.Parse(up);
                            await toError(err);
                        }
                        l.Add(b);
                    }
                }
                // * 寫入db
                await toDBTemp(l);
            }
            catch (System.Exception e)
            {
                Console.WriteLine("FetchTempSurfacePower : " + e.Message.ToString());
            }
        }

        public static async Task toDBTemp(List<dtoTemperature> bsp)
        {
            try
            {
                using (var cn = new SqlConnection(connBill))
                {
                    foreach (var p in bsp)
                    {
                        string updtaeQuery = string.Format("update RawPower set TemperatureS = {0} where datatimeR = '{1}' and Collector_Guid = '{2}';"
                        , p.TValues, p.UploadTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), p.Collector_Guid);
                        await cn.ExecuteAsync(updtaeQuery);
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("toDBTemp : " + e.Message.ToString());
            }
        }

        #endregion

        #region 模組溫度計
        public static async Task<List<dtoTempBack>> FetchTempBack(Guid CollectorId)
        {
            try
            {
                using (var cn = new SqlConnection(connRoot))
                {
                    var SqlStr1 = string.Format("Execute SP_GetTempBack '{0}';", CollectorId.ToString());
                    var q = await cn.QueryAsync<dtoTempBack>(SqlStr1);
                    if (q.FirstOrDefault() != null)
                    {
                        return q.ToList();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("FetchTempBack : " + e.Message.ToString());
                return null;
            }
        }

        public static async Task FetchTempBackPower(string siteNo, string dataNo, string startDatetime, string endDatetime, string token, int timeStamp, string url, int Sort, Guid CollectorId)
        {
            var _Para = new JObject();
            _Para["siteNo"] = siteNo;
            _Para["dataNo"] = dataNo;
            _Para["startDatetime"] = startDatetime;
            _Para["endDatetime"] = endDatetime;

            var _Raw = new JObject();
            _Raw["api"] = "ctActinometerRawData";
            _Raw["token"] = token;
            _Raw["langCode"] = "zh_TW";
            _Raw["sendTimestamp"] = timeStamp;
            _Raw["para"] = _Para;

            string JsonString = JsonConvert.SerializeObject(_Raw);
            var str = string.Format("模組溫度計 dataNo:{0}:{1}", dataNo, startDatetime);
            CS1(str);
            try
            {
                var l = new List<dtoTemperature>();
                using (var client = new HttpClient())
                {
                    var res = client.PostAsync(url, new StringContent(JsonString, Encoding.UTF8, "application/json")).GetAwaiter().GetResult(); ;
                    var res2 = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Result1 r = new Result1(res2);
                    foreach (var p in r.data)
                    {
                        var p1 = new Result3(p.ToString());
                        var backTemp = 0.0;
                        if (p1.backTemp != null)
                        {
                            backTemp = double.Parse(p1.backTemp);
                        }
                        else
                        {
                            if (p1.surfaceTemp != null)
                            {
                                backTemp = double.Parse(p1.surfaceTemp);
                            }
                        }

                        var up = p1.datatimeR;

                        var b = new dtoTemperature();
                        b.Guid = Guid.NewGuid();
                        b.Collector_Guid = CollectorId;
                        b.UploadTime = DateTime.Parse(up);
                        b.Sort = Sort;
                        b.TValues = Math.Round(backTemp, 2);
                        b.DEBUG = "";

                        if (p1.mateWarn != "")
                        {
                            var err = new dtoError();
                            err.Guid = Guid.NewGuid();
                            err.Collector_Guid = CollectorId;
                            err.Types = "模組溫度計";
                            err.Sort = Sort;
                            err.MateStat = p1.mateStat;
                            err.MateWarn = p1.mateWarn;
                            err.UploadTime = DateTime.Parse(up);
                            await toError(err);
                        }

                        l.Add(b);
                    }
                }
                // * 寫入db
                await toDBTempBack(l);
            }
            catch (System.Exception e)
            {
                Console.WriteLine("FetchTempBackPower : " + e.Message.ToString());
            }
        }

        public static async Task toDBTempBack(List<dtoTemperature> bsp)
        {
            try
            {
                using (var cn = new SqlConnection(connBill))
                {
                    foreach (var p in bsp)
                    {
                        string updtaeQuery = string.Format("update RawPower set TemperatureB = {0} where datatimeR = '{1}' and Collector_Guid = '{2}';"
                        , p.TValues, p.UploadTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), p.Collector_Guid);
                        await cn.ExecuteAsync(updtaeQuery);
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("toDBTempBack : " + e.Message.ToString());
            }
        }

        #endregion

        #region 風速計
        public static async Task<List<dtoWind>> FetchWind(Guid CollectorId)
        {
            try
            {
                using (var cn = new SqlConnection(connRoot))
                {
                    var SqlStr1 = string.Format("Execute SP_GetWind '{0}';", CollectorId.ToString());
                    var q = await cn.QueryAsync<dtoWind>(SqlStr1);
                    if (q.FirstOrDefault() != null)
                    {
                        return q.ToList();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("FetchWind : " + e.Message.ToString());
                return null;
            }
        }

        public static async Task FetchWindPower(string siteNo, string dataNo, string startDatetime, string endDatetime, string token, int timeStamp, string url, int Sort, Guid CollectorId)
        {
            var _Para = new JObject();
            _Para["siteNo"] = siteNo;
            _Para["dataNo"] = dataNo;
            _Para["startDatetime"] = startDatetime;
            _Para["endDatetime"] = endDatetime;

            var _Raw = new JObject();
            _Raw["api"] = "ctAirmeterRawData";
            _Raw["token"] = token;
            _Raw["langCode"] = "zh_TW";
            _Raw["sendTimestamp"] = timeStamp;
            _Raw["para"] = _Para;

            string JsonString = JsonConvert.SerializeObject(_Raw);
            var str = string.Format("風速計 dataNo:{0}:{1}", dataNo, startDatetime);
            CS1(str);
            try
            {
                var l = new List<dtoWindP>();
                using (var client = new HttpClient())
                {
                    var res = client.PostAsync(url, new StringContent(JsonString, Encoding.UTF8, "application/json")).GetAwaiter().GetResult(); ;
                    var res2 = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Result1 r = new Result1(res2);
                    foreach (var p in r.data)
                    {
                        var p1 = new Result3(p.ToString());
                        var windSpeed = 0.0;
                        if (p1.windSpeed != null)
                        {
                            windSpeed = double.Parse(p1.windSpeed);
                        }

                        var up = p1.datatimeR;

                        var b = new dtoWindP();
                        b.Guid = Guid.NewGuid();
                        b.Collector_Guid = CollectorId;
                        b.UploadTime = DateTime.Parse(up);
                        b.Sort = Sort;
                        b.TValues = Math.Round(windSpeed, 2);
                        b.DEBUG = "";

                        if (p1.mateWarn != "")
                        {
                            var err = new dtoError();
                            err.Guid = Guid.NewGuid();
                            err.Collector_Guid = CollectorId;
                            err.Types = "風速計";
                            err.Sort = Sort;
                            err.MateStat = p1.mateStat;
                            err.MateWarn = p1.mateWarn;
                            err.UploadTime = DateTime.Parse(up);
                            await toError(err);
                        }

                        l.Add(b);
                    }
                }
                // * 寫入db
                await toDBWind(l);
            }
            catch (System.Exception e)
            {
                Console.WriteLine("FetchWindPower : " + e.Message.ToString());
            }
        }

        public static async Task toDBWind(List<dtoWindP> bsp)
        {
            try
            {
                using (var cn = new SqlConnection(connBill))
                {
                    foreach (var p in bsp)
                    {
                        string updtaeQuery = string.Format("update RawPower set Wind = {0} where datatimeR = '{1}' and Collector_Guid = '{2}';"
                        , p.TValues, p.UploadTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), p.Collector_Guid);
                        await cn.ExecuteAsync(updtaeQuery);
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("toDBWind : " + e.Message.ToString());
            }
        }


        #endregion

        public static string NullToString(object value)
        {
            return value == null ? "" : value.ToString();
        }

        public static double NullToZero(object value)
        {
            try
            {
                return value == null ? 0.0 : double.Parse(value.ToString());
            }
            catch (System.Exception)
            {

                return 0.0;
            }

        }

        public static void CS1(string a)
        {
            var str = string.Format("{0} Start : {1}", DateTime.Now.ToString("HH:mm:ss fff"), a);
            Console.WriteLine(str);
        }

    }
}
