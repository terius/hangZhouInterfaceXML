using Common;
using DAL;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HangZhouTran
{
    public class HZAction
    {
        DataAction da = new DataAction();
        private volatile bool isRun = false;
        private readonly int LoopTime1 = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["LoopTime1"]);
        private readonly int LoopTime2 = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["LoopTime1"]);
        private readonly string getFilePath = System.Configuration.ConfigurationManager.AppSettings["GetFilePath"];
        private readonly string sendFilePath = System.Configuration.ConfigurationManager.AppSettings["SendFilePath"];
        readonly string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string errorFilePath = "";
        string oldFilePath = "";

        public void BeginRun()
        {
            try
            {


                FileHelper.WriteLog("服务已启动");
                // sbLog = new StringBuilder();
                isRun = true;
                CheckDirectory();

                Thread MainThread = new Thread(RunTask);
                MainThread.IsBackground = true;
                MainThread.Name = "HangZhouXrayServer";
                MainThread.Start();

                Thread MainThread2 = new Thread(RunTask2);
                MainThread2.IsBackground = true;
                MainThread2.Name = "HangZhouXrayServer2";
                MainThread2.Start();

            }
            catch (Exception ex)
            {
                Loger.LogMessage("服务报错：" + ex.ToString());
            }
        }

        private void CheckDirectory()
        {
            oldFilePath = basePath + "oldFiles";
            if (!Directory.Exists(oldFilePath))
            {
                Directory.CreateDirectory(oldFilePath);
            }



            errorFilePath = basePath + "errorFiles";
            if (!Directory.Exists(errorFilePath))
            {
                Directory.CreateDirectory(errorFilePath);
            }


        }

        object obRun = new object();
        private void RunTask()
        {
            try
            {
                lock (obRun)
                {
                    while (isRun)
                    {
                        // sbLog.Clear();
                        //FileHelper.WriteLog("开始读取数据");
                        UpdateHead();
                        // FileHelper.WriteLog(sbLog.ToString());
                        Thread.Sleep(LoopTime1);
                    }
                }

            }
            catch (Exception ex)
            {
                FileHelper.WriteLog("任务出错，服务中止！错误信息：" + ex.Message);
                Loger.LogMessage(ex);
            }
        }

        object obRun2 = new object();
        private void RunTask2()
        {
            try
            {
                lock (obRun2)
                {
                    while (isRun)
                    {
                        UpdateSendData();
                        Thread.Sleep(LoopTime2);
                    }
                }

            }
            catch (Exception ex)
            {
                FileHelper.WriteLog("任务出错，服务中止！错误信息：" + ex.Message);
                Loger.LogMessage(ex);
            }
        }

        private void UpdateSendData()
        {
            try
            {
                var data = da.GetNoSendData();
                string sendfile;
                string bill_no;
                foreach (DataRow dr in data.Rows)
                {
                    var info = ConvertToSendXMLInfo(dr);
                    //sendfile = Path.Combine(sendFilePath, DateTime.Now.ToString("yyyyMMdd"));
                    //if (!Directory.Exists(sendfile))
                    //{
                    //    Directory.CreateDirectory(sendfile);
                    //}
                    bill_no = dr["BILL_NO"].ToString();
                    sendfile = Path.Combine(sendFilePath, DateTime.Now.ToString("yyyyMMddHHmmssfff") +"_" + bill_no + ".xml");
                    XmlHelper.SerializerToFile(info, sendfile);
                    da.UpdateSendDataFlag(bill_no);
                }
            }
            catch (Exception ex)
            {
            Loger.LogMessage(ex);
            }
        }

        private SendXMLInfo ConvertToSendXMLInfo(DataRow dr)
        {
            var dtNow = DateTime.Now.ToString("yyyyMMddHHmmss");
            var info = new SendXMLInfo();
            info.Head.SendTime = dtNow;
            info.Body.CMA_INFO_REC.BILL_NO = dr["BILL_NO1"].ToString();
            info.Body.CMA_INFO_REC.IE_FLAG = dr["I_E_FLAG"].ToString();
            info.Body.CMA_INFO_REC.LINE_ID = dr["MX_FLAG"].ToString();
            info.Body.CMA_INFO_REC.VOYAGE_NO = dr["VOYAGE_NO"].ToString();
            info.Body.CMA_INFO_REC.LOG_NO = dr["BILL_NO"].ToString();
            info.Body.CMA_INFO_REC.TRADE_CODE = dr["TRADE_CODE"].ToString();
            info.Body.CMA_INFO_REC.TRADE_NAME = dr["TRADE_NAME"].ToString();
            info.Body.CMA_INFO_REC.CHECK_TIME = dtNow;
            info.Body.CMA_INFO_REC.CHECK_MAN = dr["OPT_ID"].ToString();
            if (dr["M_RESULT"].ToString() == "0" && dr["DEC_TYPE"].ToString() == "0")
            {
                info.Body.CMA_INFO_REC.CHECK_INFO = "机检放行";
            }
            else if (dr["DEC_TYPE"].ToString() == "1")
            {
                info.Body.CMA_INFO_REC.CHECK_INFO = "审单查验";
            }
            else if (dr["M_RESULT"].ToString() == "1")
            {
                info.Body.CMA_INFO_REC.CHECK_INFO = "即决查验";
            }
            return info;
        }



        private void UpdateHead()
        {
            try
            {
                StringBuilder sbMsg = new StringBuilder();
                var dir = new DirectoryInfo(getFilePath);
                int rs = 0;
                foreach (var file in dir.GetFiles())
                {
                    try
                    {
                        var info = XmlHelper.DeserializeFromFile<NEWXMLInfo2>(file.FullName);
                        if (info != null)
                        {
                            rs = da.SaveGetData(info);
                            if (rs > 0)
                            {
                                var oldFile = Path.Combine(oldFilePath, DateTime.Now.ToString("yyyyMMdd"));
                                if (!Directory.Exists(oldFile))
                                {
                                    Directory.CreateDirectory(oldFile);
                                }
                                oldFile = Path.Combine(oldFile, file.Name);
                              //  Loger.LogMessage(oldFile);
                             //   Thread.Sleep(10);
                                file.MoveTo(oldFile);
                            }
                            else
                            {
                                file.MoveTo(errorFilePath + "\\" + file.Name);
                            }
                        }
                        else
                        {
                            file.MoveTo(errorFilePath + "\\" + file.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        Loger.LogMessage(ex.ToString());
                        throw;
                    }
                  
                }

            }
            catch (Exception ex)
            {
                Loger.LogMessage(ex);
            }
        }



        public void EndRun()
        {
            FileHelper.WriteLog("服务中止！\r\n\r\n\r\n");
            isRun = false;
        }
    }
}
