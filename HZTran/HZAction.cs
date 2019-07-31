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
        //  private StringBuilder sbLog;
        IList<ColumnMap> map;
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
        readonly string basePath = AppDomain.CurrentDomain.BaseDirectory;
      

        string errorFilePath = "";
        string oldFilePath = "";
        private void CheckDirectory()
        {
            var oldFilePath = basePath + "oldFiles";
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
                string bill_no;
                foreach (DataRow dr in data.Rows)
                {
                    bill_no = dr["bill_no"].ToString();
                    PutData(bill_no);
                }
            }
            catch (Exception ex)
            {
                Loger.LogMessage(ex);
            }
        }





        readonly int _saveLog = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["SaveLog"]);
        private string AppendTextWithTime(string msg)
        {
            if (_saveLog != 1)
            {
                return "";
            }
            return string.Format("{0}----{1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg);
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
                    var info = XmlHelper.DeserializeFromFile<NEWXMLInfo2>(file.FullName);
                    if (info != null)
                    {
                        rs = da.SaveGetData(info);
                        if (rs > 0)
                        {
                            var oldFile = Path.Combine(oldFilePath, DateTime.Now.ToString("yyyyMMdd"), file.Name);
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
               
            }
            catch (Exception ex)
            {
                Loger.LogMessage(ex);
            }
        }



        private void PutData(string bill_no)
        {
            var rs = ServerHelper.putData(bill_no);
            if (rs == null)
            {
                throw new Exception("发送机检反馈错误");
            }
            if (rs["status"] == "0")
            {
                var errmsg = rs["errMsg"];
             //   da.UpdateSendFailInfoToTMP(bill_no, errmsg);
            }
            else
            {
              //  da.UpdateSendSuccessInfoToTMP(bill_no);
            }
        }

        private bool HasValue(DataSet eData)
        {
            return eData != null && eData.Tables.Count > 0 && eData.Tables[0].Rows.Count > 0;
        }

        //private bool HasValue(XMLInfo eData)
        //{
        //    return eData.head.status == 1;
        //}

        private bool HasValue(DataTable eData)
        {
            return eData != null && eData.Rows.Count > 0;
        }




        public void EndRun()
        {
            FileHelper.WriteLog("服务中止！\r\n\r\n\r\n");
            isRun = false;
        }
    }
}
