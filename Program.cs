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
using Partner.Helper;
using Partner.Model;

namespace Partner
{
    class Program
    {
        public static string connSolar { get; set; }
        public static string connRoot { get; set; }
        public static string connIvt { get; set; }
        public static string account { get; set; }
        public static string password { get; set; }
        public static string url { get; set; }
        static async Task Main(string[] args)
        {
            connRoot = Tool.ReadFromAppSettings().Get<SolarModel>().Root;
            connSolar = Tool.ReadFromAppSettings().Get<SolarModel>().Solar;
            connIvt = Tool.ReadFromAppSettings().Get<SolarModel>().Ivt;
            account = Tool.ReadFromAppSettings().Get<SolarModel>().account;
            password = Tool.ReadFromAppSettings().Get<SolarModel>().password;
            url = Tool.ReadFromAppSettings().Get<SolarModel>().url;

            int ts = 5;
            string guid = "";
            if (args.Length == 0)
            {
                ts = 5;
                guid = "";
            }
            else if (args.Length == 1)
            {
                int num;
                bool test = int.TryParse(args[0], out num);
                ts = num;
            }
            else
            {
                int num;
                bool test = int.TryParse(args[0], out num);
                ts = num;

                guid = args[1];
                Console.WriteLine("取得 guid = " + guid);
            }

            Console.WriteLine("取得 " + (ts + 1) + " 分鐘資料 ");

            if (guid != "")
            {
                for (int i = 0; i < 12; i++)
                {
                    await Reload(guid, ts);
                }
            }
            else
            {
                // Console.WriteLine("Start");
                // await Start(ts);
                await Test(ts);
                // TestSunTemp(ts);
            }
        }

        public static async Task Reload(string CasesGuid, int ts)
        {
            var Cases_Guid = Guid.Parse(CasesGuid);
            int timeStamp = Convert.ToInt32(DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            var s1 = password + timeStamp;
            var token = Tool.MD5code(s1) + account;
            // * 2.讀取db -- 最後更新時間
            var q1 = await FetchCollectors(Cases_Guid);
            foreach (var p1 in q1)
            {
                var CollectorId = p1.Guid;
                var siteNo = p1.MacAddress;
                var lt = p1.LastUploadTime;

                var d = lt.AddMinutes(1);
                var startDatetime = d.ToString("yyyy-MM-dd HH:mm:ss");
                var endDatetime = d.AddMinutes(ts).ToString("yyyy-MM-dd HH:mm:ss");
                var ds = d.ToString("yyyy-MM-dd HH:00:00");
                var de = d.AddMinutes(ts).ToString("yyyy-MM-dd HH:59:00");

                #region 逆變器
                var q2 = await FetchInverters(CollectorId);
                foreach (var p2 in q2)
                {
                    var dataNo = p2.SerialNumber;
                    var Sort = p2.Sort;
                    await FetchPower(siteNo, dataNo, startDatetime, endDatetime, token, timeStamp, url, Sort, CollectorId);
                }

                // * 產生後台可以檢視的資料
                await toDB2(lt, CollectorId, siteNo);

                // * 寫入 PowerHour
                await toDB3(CollectorId, ds, de);

                await toDB4(CollectorId, d.ToString("yyyy-MM-dd"), endDatetime);
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

                Console.WriteLine("endDatetime :" + endDatetime);

            }

        }
        public static async Task Test(int ts)
        {
            var Cases_Guid = Guid.Parse("4F4A414D-5A04-40D6-993A-AE7FA8AD851B");
            int timeStamp = Convert.ToInt32(DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            var s1 = password + timeStamp;
            var token = Tool.MD5code(s1) + account;

            // * 2.讀取db -- 最後更新時間
            var q1 = await FetchCollectors(Cases_Guid);
            foreach (var p1 in q1)
            {
                var CollectorId = p1.Guid;
                var siteNo = p1.MacAddress;
                var lt = p1.LastUploadTime;
                // var lt = DateTime.Parse("2021-07-20 05:59");
                var d = lt.AddMinutes(1);
                var startDatetime = d.ToString("yyyy-MM-dd HH:mm:ss");
                var endDatetime = d.AddMinutes(ts).ToString("yyyy-MM-dd HH:mm:ss");
                var ds = d.ToString("yyyy-MM-dd HH:00:00");
                var de = d.AddMinutes(ts).ToString("yyyy-MM-dd HH:59:00");

                #region 逆變器
                var q2 = await FetchInverters(CollectorId);
                foreach (var p2 in q2)
                {
                    var dataNo = p2.SerialNumber;
                    var Sort = p2.Sort;
                    await FetchPower(siteNo, dataNo, startDatetime, endDatetime, token, timeStamp, url, Sort, CollectorId);
                }

                // * 產生後台可以檢視的資料
                await toDB2(lt, CollectorId, siteNo);

                // * 寫入 PowerHour
                await toDB3(CollectorId, ds, de);

                await toDB4(CollectorId, d.ToString("yyyy-MM-dd"), endDatetime);
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

                Console.WriteLine("endDatetime :" + endDatetime);
            }

        }

        public static async Task TestSunTemp(int ts)
        {
            int timeStamp = Convert.ToInt32(DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            var s1 = password + timeStamp;
            var token = Tool.MD5code(s1) + account;

            var Cases_Guid = Guid.Parse("124AC786-7E20-41B6-9F36-D38ECBDFA6BC");

            var CollectorId = Guid.Parse("3F11689D-9314-499A-B8FD-E10A9F389CBF");
            var siteNo = "P2021126";
            var lt = DateTime.Parse("2021-07-02 17:00:00.000");

            var d = lt.AddMinutes(1);
            var startDatetime = d.ToString("yyyy-MM-dd HH:mm:ss");
            var endDatetime = d.AddMinutes(ts).ToString("yyyy-MM-dd HH:mm:ss");
            // var startDatetime = "2021-07-13 04:59:00.000";
            // var endDatetime = "2021-07-13 20:00:00.000";

            #region 日照計
            var q3 = await FetchSunlightMeter(CollectorId);
            int i = 1;
            foreach (var p3 in q3)
            {
                var dataNo = p3.SerialNumber;
                var Sort = p3.Sort;
                await FetchSunPower(siteNo, dataNo, startDatetime, endDatetime, token, timeStamp, url, Sort, CollectorId, i);
                i++;
            }

            #endregion

            #region 環境溫度計
            var q4 = await FetchTempSurface(CollectorId);
            foreach (var p4 in q4)
            {
                var dataNo = p4.SerialNumber;
                var Sort = p4.Sort;
                await FetchTempSurfacePower(siteNo, dataNo, startDatetime, endDatetime, token, timeStamp, url, Sort, CollectorId);

            }

            #endregion

            #region 模組溫度計
            var q5 = await FetchTempBack(CollectorId);
            foreach (var p5 in q5)
            {
                var dataNo = p5.SerialNumber;
                var Sort = p5.Sort;
                await FetchTempBackPower(siteNo, dataNo, startDatetime, endDatetime, token, timeStamp, url, Sort, CollectorId);
            }

            #endregion

            Console.WriteLine("endDatetime :" + endDatetime);
        }

        public static async Task Start(int ts)
        {
            // * 1.取得api廠商資料
            var q = await FetchPartner();
            foreach (var p in q)
            {


                var account = p.Account;
                var password = p.Password;
                var url = p.UrlAddress;

                // * 2.讀取db -- 最後更新時間
                var q1 = await FetchCollectors(p.Cases_Guid);
                foreach (var p1 in q1)
                {

                    await Task.Run(async () =>
                    {
                        var s1 = string.Format("{0} 開始:{1}", p.Cases_Name, DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                        Console.WriteLine(s1);

                        var CollectorId = p1.Guid;
                        var siteNo = p1.MacAddress;
                        var lt = p1.LastUploadTime;

                        var d = lt.AddMinutes(1);
                        var startDatetime = d.ToString("yyyy-MM-dd HH:mm:ss");
                        var endDatetime = d.AddMinutes(ts).ToString("yyyy-MM-dd HH:mm:ss");
                        var ds = d.ToString("yyyy-MM-dd HH:00:00");
                        var de = d.AddMinutes(ts).ToString("yyyy-MM-dd HH:59:00");

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

                        // * 產生後台可以檢視的資料
                        await toDB2(lt, CollectorId, siteNo);

                        // * 寫入 PowerHour
                        await toDB3(CollectorId, ds, de);

                        await toDB4(CollectorId, d.ToString("yyyy-MM-dd"), endDatetime);
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

                        var s2 = string.Format("{0} 結束:{1}", p.Cases_Name, endDatetime);
                        Console.WriteLine(s2);
                    });
                }

            }

            var s3 = string.Format("End:{0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            Console.WriteLine(s3);

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
            Console.WriteLine("逆變器 dataNo: " + dataNo);

            try
            {
                var l = new List<dtoBillionwattsPower>();
                using (var client = new HttpClient())
                {
                    var res = client.PostAsync(url, new StringContent(JsonString, Encoding.UTF8, "application/json")).GetAwaiter().GetResult(); ;
                    var res2 = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Result1 r = new Result1(res2);
                    foreach (var p in r.data)
                    {
                        var p1 = new Result2(p.ToString());

                        /* DC總功率(W) */
                        var DcPowerTotal = 0.0;
                        if (p1.dcPower != null)
                        {
                            DcPowerTotal = DcPowerTotal + double.Parse(p1.dcPower);
                        }

                        /* AC總功率(W) */
                        var AcPowerTotal = 0.0;
                        if (p1.acPower != null)
                        {
                            AcPowerTotal = AcPowerTotal + double.Parse(p1.acPower);
                        }

                        #region DC電壓,DC電流

                        /* DC功率 pDC(n) 取加總 */
                        var pDcTotal = 0.0;

                        /* DC電壓 vDc(n) 平均值 */
                        var vDcTotal = 0.0;
                        var vDcAvg = 0;
                        var vDcPower = 0.0; //* 寫入db的result

                        /* DC電流 cDC(n) 取加總 */
                        var cDcTotal = 0.0; //* 寫入db的result

                        if (p1.pDc1 != null)
                        {
                            pDcTotal = pDcTotal + double.Parse(p1.pDc1);
                            vDcTotal = vDcTotal + double.Parse(p1.vDc1);
                            cDcTotal = cDcTotal + double.Parse(p1.cDc1);
                            vDcAvg = vDcAvg + 1;
                        }
                        if (p1.pDc2 != null)
                        {
                            pDcTotal = pDcTotal + double.Parse(p1.pDc2);
                            vDcTotal = vDcTotal + double.Parse(p1.vDc2);
                            cDcTotal = cDcTotal + double.Parse(p1.cDc2);
                            vDcAvg = vDcAvg + 1;
                        }
                        if (p1.pDc3 != null)
                        {
                            pDcTotal = pDcTotal + double.Parse(p1.pDc3);
                            vDcTotal = vDcTotal + double.Parse(p1.vDc3);
                            cDcTotal = cDcTotal + double.Parse(p1.cDc3);
                            vDcAvg = vDcAvg + 1;
                        }
                        if (p1.pDc4 != null)
                        {
                            pDcTotal = pDcTotal + double.Parse(p1.pDc4);
                            vDcTotal = vDcTotal + double.Parse(p1.vDc4);
                            cDcTotal = cDcTotal + double.Parse(p1.cDc4);
                            vDcAvg = vDcAvg + 1;
                        }
                        if (p1.pDc5 != null)
                        {
                            pDcTotal = pDcTotal + double.Parse(p1.pDc5);
                            vDcTotal = vDcTotal + double.Parse(p1.vDc5);
                            cDcTotal = cDcTotal + double.Parse(p1.cDc5);
                            vDcAvg = vDcAvg + 1;
                        }
                        if (p1.pDc6 != null)
                        {
                            pDcTotal = pDcTotal + double.Parse(p1.pDc6);
                            vDcTotal = vDcTotal + double.Parse(p1.vDc6);
                            cDcTotal = cDcTotal + double.Parse(p1.cDc6);
                            vDcAvg = vDcAvg + 1;
                        }
                        if (p1.pDc7 != null)
                        {
                            pDcTotal = pDcTotal + double.Parse(p1.pDc7);
                            vDcTotal = vDcTotal + double.Parse(p1.vDc7);
                            cDcTotal = cDcTotal + double.Parse(p1.cDc7);
                            vDcAvg = vDcAvg + 1;
                        }
                        if (p1.pDc8 != null)
                        {
                            pDcTotal = pDcTotal + double.Parse(p1.pDc8);
                            vDcTotal = vDcTotal + double.Parse(p1.vDc8);
                            cDcTotal = cDcTotal + double.Parse(p1.cDc8);
                            vDcAvg = vDcAvg + 1;
                        }
                        if (p1.pDc9 != null)
                        {
                            pDcTotal = pDcTotal + double.Parse(p1.pDc9);
                            vDcTotal = vDcTotal + double.Parse(p1.vDc9);
                            cDcTotal = cDcTotal + double.Parse(p1.cDc9);
                            vDcAvg = vDcAvg + 1;
                        }
                        if (p1.pDc10 != null)
                        {
                            pDcTotal = pDcTotal + double.Parse(p1.pDc10);
                            vDcTotal = vDcTotal + double.Parse(p1.vDc10);
                            cDcTotal = cDcTotal + double.Parse(p1.cDc10);
                            vDcAvg = vDcAvg + 1;
                        }
                        if (p1.pDc12 != null)
                        {
                            pDcTotal = pDcTotal + double.Parse(p1.pDc12);
                            vDcTotal = vDcTotal + double.Parse(p1.vDc12);
                            cDcTotal = cDcTotal + double.Parse(p1.cDc12);
                            vDcAvg = vDcAvg + 1;
                        }
                        if (p1.pDc12 != null)
                        {
                            pDcTotal = pDcTotal + double.Parse(p1.pDc12);
                            vDcTotal = vDcTotal + double.Parse(p1.vDc12);
                            cDcTotal = cDcTotal + double.Parse(p1.cDc12);
                            vDcAvg = vDcAvg + 1;
                        }

                        if (vDcAvg > 0)
                        {
                            vDcPower = vDcTotal / vDcAvg;
                        }

                        #endregion

                        #region AC電壓,AC電流

                        /* AC功率 pAC(n) 取加總*/
                        var pAcTotal = 0.0;

                        /* AC電壓 vAC(n) 平均值 */
                        var vAcTotal = 0.0;
                        var vAcAvg = 0;
                        var vAcPower = 0.0; //* 寫入db的result

                        /* AC電流 cAC(n) 取加總(感覺要取平均值才正確) */
                        var cAcTotal = 0.0;
                        var cAcPower = 0.0; //* 寫入db的result

                        if (p1.vAc1 != null)
                        {
                            pAcTotal = pAcTotal + double.Parse(p1.pAc1);
                            vAcTotal = vAcTotal + double.Parse(p1.vAc1);
                            cAcTotal = cAcTotal + double.Parse(p1.cAc1);
                            vAcAvg = vAcAvg + 1;
                        }
                        if (p1.vAc2 != null)
                        {
                            pAcTotal = pAcTotal + double.Parse(p1.pAc2);
                            vAcTotal = vAcTotal + double.Parse(p1.vAc2);
                            cAcTotal = cAcTotal + double.Parse(p1.cAc2);
                            vAcAvg = vAcAvg + 1;
                        }
                        if (p1.vAc3 != null)
                        {
                            pAcTotal = pAcTotal + double.Parse(p1.pAc3);
                            vAcTotal = vAcTotal + double.Parse(p1.vAc3);
                            cAcTotal = cAcTotal + double.Parse(p1.cAc3);
                            vAcAvg = vAcAvg + 1;
                        }

                        if (vAcAvg > 0)
                        {
                            vAcPower = vAcTotal / vAcAvg;
                            cAcPower = cAcTotal / vAcAvg;
                        }
                        #endregion

                        #region 組串功率,電壓,電流
                        // var pStrTotal = 0.0;
                        // var vStrTotal = 0.0;
                        // var cStrTotal = 0.0;

                        // if (p1.pStr1 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr1);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr1);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr1);
                        // }

                        // if (p1.pStr2 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr2);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr2);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr2);
                        // }

                        // if (p1.pStr3 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr3);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr3);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr3);
                        // }

                        // if (p1.pStr4 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr4);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr4);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr4);
                        // }

                        // if (p1.pStr5 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr5);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr5);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr5);
                        // }

                        // if (p1.pStr6 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr6);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr6);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr6);
                        // }

                        // if (p1.pStr7 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr7);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr7);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr7);
                        // }

                        // if (p1.pStr8 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr8);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr8);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr8);
                        // }

                        // if (p1.pStr9 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr9);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr9);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr9);
                        // }

                        // if (p1.pStr10 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr10);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr10);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr10);
                        // }

                        // if (p1.pStr11 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr11);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr11);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr11);
                        // }

                        // if (p1.pStr12 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr12);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr12);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr12);
                        // }

                        // if (p1.pStr13 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr13);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr13);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr13);
                        // }

                        // if (p1.pStr14 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr14);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr14);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr14);
                        // }

                        // if (p1.pStr15 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr15);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr15);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr15);
                        // }

                        // if (p1.pStr16 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr16);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr16);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr16);
                        // }

                        // if (p1.pStr17 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr17);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr17);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr17);
                        // }

                        // if (p1.pStr18 != null)
                        // {
                        //     pStrTotal = pStrTotal + double.Parse(p1.pStr18);
                        //     vStrTotal = vStrTotal + double.Parse(p1.vStr18);
                        //     cStrTotal = cStrTotal + double.Parse(p1.cStr18);
                        // }
                        #endregion

                        var up = p1.datatimeR;

                        var b = new dtoBillionwattsPower();
                        b.Guid = Guid.NewGuid();
                        b.Collector_Guid = CollectorId;
                        b.Sort = Sort;
                        b.ACPower = Math.Round(AcPowerTotal, 2);
                        b.DCPower = Math.Round(DcPowerTotal, 2);
                        b.vDc = Math.Round(vDcPower, 2);
                        b.cDc = Math.Round(cDcTotal, 2);
                        b.vAc = Math.Round(vAcPower, 2);
                        b.cAc = Math.Round(cAcPower, 2);
                        // b.vAC = vStrTotal == 0 ? vAcTotal : vStrTotal;
                        // b.cAC = cStrTotal == 0 ? cAcTotal : cStrTotal;

                        b.Sunshine = 0;
                        b.TemperatureB = 0;
                        b.TemperatureS = 0;
                        if (p1.mateWarn != "")
                        {
                            b.STATUS = p1.mateWarn;
                        }
                        else
                        {
                            b.STATUS = "";
                        }

                        b.UploadTime = DateTime.Parse(up);
                        l.Add(b);
                    }
                    Console.WriteLine("逆變器 Post End");
                }
                // * 寫入db BillionwattsPower
                await toDBIvtPower(l);
            }
            catch (System.Exception e)
            {
                Console.WriteLine("FetchPower : " + e.Message.ToString());
                throw;
            }
        }

        public static async Task toDBIvtPower(List<dtoBillionwattsPower> bsp)
        {
            try
            {
                using (var cn = new SqlConnection(connSolar))
                {
                    foreach (var p in bsp)
                    {
                        string insertQuery = @"INSERT INTO BillionwattsPower (Guid, Collector_Guid, Sort, ACPower, DCPower , Sunshine , TemperatureS , TemperatureB , STATUS , UploadTime , vDc , cDc , vAc , cAc ) " +
                        "VALUES (@Guid, @Collector_Guid, @Sort, @ACPower, @DCPower, @Sunshine , @TemperatureS , @TemperatureB , @STATUS , @UploadTime , @vDc , @cDc , @vAc , @cAc)";

                        var result = await cn.ExecuteAsync(insertQuery, p);
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("toDBIvtPower :" + e.Message.ToString());
                throw;
            }

        }

        public static async Task toDB2(DateTime dt, Guid CollectorId, string siteNo)
        {
            var tbn = "ivtS_" + siteNo;
            var info = "";

            using (var cn = new SqlConnection(connIvt))
            {
                var d = dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string str = string.Format("Execute SP_GetBillionwattsPowerRaw '{0}','{1}';", CollectorId.ToString(), d);
                // var result = cn.Query<dtoIvt>(str).ToList();
                var result1 = await cn.QueryAsync<dtoIvt>(str);
                if (result1.FirstOrDefault() == null)
                {
                    return;
                }

                var result = result1.ToList();

                var q = result.GroupBy(x => x.UploadTime, (u, s) => new
                {
                    UploadTime = u,
                    MaxSort = s.Max(p => p.Sort)
                });

                Dictionary<string, dtoIvt> dict;
                dict = result.ToDictionary(
                                    o => string.Format("{0},{1}", o.UploadTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), o.Sort),
                                    o => o);

                foreach (var p in q)
                {
                    var d1 = p.UploadTime;
                    var cnt = p.MaxSort;

                    var SqlStr = "insert into " + tbn + " (UploadTime,info,";
                    for (int i = 1; i < cnt + 1; i++)
                    {
                        SqlStr = SqlStr + "M" + i.ToString() + ",";
                    }

                    SqlStr = SqlStr.Substring(0, SqlStr.Length - 1);
                    SqlStr = SqlStr + ") values ('" + d1.ToString("yyyy-MM-dd HH:mm:ss.fff") + "','" + info + "',";

                    for (int i = 1; i < p.MaxSort + 1; i++)
                    {
                        try
                        {
                            var Ac_Power = 0.0;
                            var q1 = dict[string.Format("{0},{1}", d1.ToString("yyyy-MM-dd HH:mm:ss.fff"), i.ToString())];
                            Ac_Power = double.Parse(q1.ACPower.ToString());
                            SqlStr = SqlStr + Math.Round(Ac_Power, 2) + ",";
                        }
                        catch (Exception e)
                        {
                            var s = e.Message.ToString();
                            SqlStr = SqlStr + "0,";
                            // Console.WriteLine("toDB2 : i=" + i.ToString() + " , " + e.Message);
                        }
                    }

                    SqlStr = SqlStr.Substring(0, SqlStr.Length - 1);
                    SqlStr = SqlStr + ")";

                    try
                    {
                        await cn.ExecuteAsync(SqlStr);
                    }
                    catch (System.Exception e)
                    {
                        Console.WriteLine("toDB2 :" + e.Message.ToString());
                        throw;
                    }
                }
            }
        }

        public static async Task toDB3(Guid CollectorId, string startDatetime, string endDatetime)
        {
            var SqlStr = string.Format("Execute SP_PowertoHour '{0}' , '{1}' , '{2}' ; ", CollectorId.ToString(), startDatetime, endDatetime);
            using (var cn = new SqlConnection(connSolar))
            {
                try
                {
                    await cn.ExecuteAsync(SqlStr);
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("toDB3 :" + e.Message.ToString());
                    throw;
                }
            }
        }

        public static async Task toDB4(Guid CollectorId, string startDatetime, string endDatetime)
        {
            var SqlStr = string.Format("Execute SP_GetCollectorDay '{0}' , '{1}' , '{2}' ;", CollectorId.ToString(), startDatetime, endDatetime);
            using (var cn = new SqlConnection(connRoot))
            {
                try
                {
                    await cn.ExecuteAsync(SqlStr);
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("toDB4 :" + e.Message.ToString());
                    throw;
                }
            }
        }

        public static async Task<List<dtoPartners>> FetchPartner()
        {
            try
            {
                using (var cn = new SqlConnection(connRoot))
                {
                    var SqlStr1 = string.Format("Execute SP_GetPartners;");
                    var q = await cn.QueryAsync<dtoPartners>(SqlStr1);
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
                Console.WriteLine("FetchPartner :" + e.Message.ToString());
                throw;
            }

        }

        public static async Task<List<dtoCollector>> FetchCollectors(Guid CaseId)
        {
            try
            {
                using (var cn = new SqlConnection(connRoot))
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
                throw;
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
                throw;
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
                throw;
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
            Console.WriteLine("日照計 dataNo : " + dataNo);

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

                        if (p1.mateWarn != "")
                        {
                            b.DEBUG = p1.mateWarn;
                        }
                        else
                        {
                            b.DEBUG = "";
                        }
                        l.Add(b);
                    }
                    Console.WriteLine("日照計 Post End");
                }
                // * 寫入db
                await toDBSunPower(l, times);
            }
            catch (System.Exception e)
            {
                Console.WriteLine("FetchSunPower : " + e.Message.ToString());
                throw;
            }
        }

        public static async Task toDBSunPower(List<dtoSunlight> bsp, int times)
        {
            try
            {
                using (var cn = new SqlConnection(connSolar))
                {
                    foreach (var p in bsp)
                    {
                        string insertQuery = @"INSERT INTO BillionwattsSunshine (Guid, Collector_Guid, UploadTime , Sort, TValues , DEBUG) " +
                        "VALUES (@Guid, @Collector_Guid, @UploadTime , @Sort, @TValues , @DEBUG )";

                        await cn.ExecuteAsync(insertQuery, p);

                        string updtaeQuery = "";
                        if (times == 1)
                        {
                            updtaeQuery = string.Format("update BillionwattsPower set Sunshine = {0} where UploadTime = '{1}' and Collector_Guid = '{2}';"
                                                   , p.TValues, p.UploadTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), p.Collector_Guid);

                        }
                        else
                        {
                            updtaeQuery = string.Format("update BillionwattsPower set Sunshine = (Sunshine + {0}) / {3} where UploadTime = '{1}' and Collector_Guid = '{2}';"
                                                   , p.TValues, p.UploadTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), p.Collector_Guid, times);

                        }
                        await cn.ExecuteAsync(updtaeQuery);
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("toDBSunPower : " + e.Message.ToString());
                throw;
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
                throw;
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
            Console.WriteLine("環境溫度計 dataNo: " + dataNo);

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

                        var up = p1.datatimeR;

                        var b = new dtoTemperature();
                        b.Guid = Guid.NewGuid();
                        b.Collector_Guid = CollectorId;
                        b.UploadTime = DateTime.Parse(up);
                        b.Sort = Sort;
                        b.TValues = Math.Round(surfaceTemp, 2);

                        if (p1.mateWarn != "")
                        {
                            b.DEBUG = p1.mateWarn;
                        }
                        else
                        {
                            b.DEBUG = "";
                        }

                        l.Add(b);
                    }
                    Console.WriteLine("環境溫度計 Post End");
                }
                // * 寫入db
                await toDBTemp(l);
            }
            catch (System.Exception e)
            {
                Console.WriteLine("FetchTempSurfacePower : " + e.Message.ToString());
                throw;
            }
        }

        public static async Task toDBTemp(List<dtoTemperature> bsp)
        {
            try
            {
                using (var cn = new SqlConnection(connSolar))
                {
                    foreach (var p in bsp)
                    {
                        string insertQuery = @"INSERT INTO BillionwattsTemp (Guid, Collector_Guid, UploadTime , Sort, TValues , DEBUG ) " +
                                            "VALUES (@Guid, @Collector_Guid, @UploadTime , @Sort, @TValues , @DEBUG )";
                        await cn.ExecuteAsync(insertQuery, p);

                        string updtaeQuery = string.Format("update BillionwattsPower set TemperatureS = {0} where UploadTime = '{1}' and Collector_Guid = '{2}';"
                        , p.TValues, p.UploadTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), p.Collector_Guid);
                        await cn.ExecuteAsync(updtaeQuery);
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("toDBTemp : " + e.Message.ToString());
                throw;
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
                throw;
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
            Console.WriteLine("模組溫度計 dataNo: " + dataNo);

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

                        var up = p1.datatimeR;

                        var b = new dtoTemperature();
                        b.Guid = Guid.NewGuid();
                        b.Collector_Guid = CollectorId;
                        b.UploadTime = DateTime.Parse(up);
                        b.Sort = Sort;
                        b.TValues = Math.Round(backTemp, 2);

                        if (p1.mateWarn != "")
                        {
                            b.DEBUG = p1.mateWarn;
                        }
                        else
                        {
                            b.DEBUG = "";
                        }

                        l.Add(b);
                    }
                    Console.WriteLine("模組溫度計 Post End");
                }
                // * 寫入db
                await toDBTempBack(l);
            }
            catch (System.Exception e)
            {
                Console.WriteLine("FetchTempBackPower : " + e.Message.ToString());
                throw;
            }
        }

        public static async Task toDBTempBack(List<dtoTemperature> bsp)
        {
            try
            {
                using (var cn = new SqlConnection(connSolar))
                {
                    foreach (var p in bsp)
                    {
                        string insertQuery = @"INSERT INTO BillionwattsTemp (Guid, Collector_Guid, UploadTime , Sort, TValues , DEBUG ) " +
                                           "VALUES (@Guid, @Collector_Guid , @UploadTime , @Sort, @TValues ,@DEBUG )";
                        await cn.ExecuteAsync(insertQuery, p);

                        string updtaeQuery = string.Format("update BillionwattsPower set TemperatureB = {0} where UploadTime = '{1}' and Collector_Guid = '{2}';"
                        , p.TValues, p.UploadTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), p.Collector_Guid);
                        await cn.ExecuteAsync(updtaeQuery);
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("toDBTempBack : " + e.Message.ToString());
                throw;
            }
        }

        #endregion

    }
}
