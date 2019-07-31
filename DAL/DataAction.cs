using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DAL
{
    public class DataAction
    {
         readonly string HeadTableName = System.Configuration.ConfigurationManager.AppSettings["HeadTableName"];
         readonly string TMPTableName = System.Configuration.ConfigurationManager.AppSettings["TMPTableName"];

        private string GetNoSendSQL = "select  bill_no from {0} where send_flag2 = '0'";
        private string updateSQL;
        private string insertSQL;
        public DataAction()
        {
          
            GetNoSendSQL = string.Format(GetNoSendSQL, TMPTableName);

            CheckBILLNO_SQL = string.Format(CheckBILLNO_SQL, HeadTableName);
        }

      

        public DataTable GetNoSendData()
        {
            return DbHelperSQL.Query(GetNoSendSQL).Tables[0];
        }

        
        private void CreateUpdateSql(IList<SqlParameter> sqlparams, string otherUpdateSql = null)
        {
            if (!string.IsNullOrWhiteSpace(updateSQL))
            {
                return;
            }
            StringBuilder sb = new StringBuilder("update " + HeadTableName + " set ");
            foreach (var item in sqlparams)
            {
                sb.AppendFormat("{0}={1},", item.ParameterName.Trim('@'), item.ParameterName);
            }
            if (!string.IsNullOrWhiteSpace(otherUpdateSql))
            {
                sb.Append(otherUpdateSql);
            }
            else
            {
                sb.Remove(sb.Length - 1, 1);
            }
            sb.Append(" where BILL_NO=@BILL_NO");
            updateSQL = sb.ToString();
        }

        private void CreateInsertSql(IList<SqlParameter> sqlparams)
        {
            if (!string.IsNullOrWhiteSpace(insertSQL))
            {
                return;
            }
            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            string sql = "insert into {0}({1}) values({2})";
            foreach (var param in sqlparams)
            {
                sb1.Append(param.ParameterName.Trim('@') + ",");
                sb2.Append(param.ParameterName + ",");
            }


            sb1.Append("READ_flag,READ_time");
            sb2.Append("1,getdate()");
            insertSQL = string.Format(sql, HeadTableName, sb1.ToString(), sb2.ToString());
        }


     


        string CheckBILLNO_SQL = "select count(1) from {0} where BILL_NO=@BILL_NO";

        private bool CheckBillNoExist(string bill_no)
        {
            SqlParameter[] sqlparams = {
                 new SqlParameter("@BILL_NO",bill_no)
            };
            return DbHelperSQL.Exists(CheckBILLNO_SQL, sqlparams);
        }

    
        public int SaveGetData(NEWXMLInfo2 info)
        {
            string billno = "";
            foreach (var item in info.Body.CMA_INFO)
            {
                billno = item.GOODS_HEAD.LOG_NO;
                IList<SqlParameter> sqlparams = new List<SqlParameter>();
                sqlparams.Add(new SqlParameter("@TRAF_NAME", item.GOODS_HEAD.TRAF_NAME));
                sqlparams.Add(new SqlParameter("@VOYAGE_NO", item.GOODS_HEAD.VOYAGE_NO));
                sqlparams.Add(new SqlParameter("@I_E_FLAG", item.GOODS_HEAD.IE_FLAG));
                sqlparams.Add(new SqlParameter("@BILL_NO", item.GOODS_HEAD.LOG_NO));
                sqlparams.Add(new SqlParameter("@PACK_NO", item.GOODS_HEAD.PACK_NUM));
                sqlparams.Add(new SqlParameter("@GROSS_WT", item.GOODS_HEAD.GROSS_WT));
                sqlparams.Add(new SqlParameter("@TOTAL_VALUE", item.GOODS_HEAD.GOODS_VALUE));
                sqlparams.Add(new SqlParameter("@DEC_TYPE", item.GOODS_HEAD.DEC_TYPE));
                sqlparams.Add(new SqlParameter("@BILL_NO1", item.GOODS_HEAD.BILL_NO));
                sqlparams.Add(new SqlParameter("@MAIN_G_NAME", item.MAIN_G_NAME));

                if (CheckBillNoExist(billno)) //已存在就更新
                {
                    CreateUpdateSql(sqlparams, "READ_flag=READ_flag+1,READ_time=getdate()");
                    return DbHelperSQL.ExecuteSql(updateSQL, sqlparams);
                }
                else
                {
                    CreateInsertSql(sqlparams);
                    return DbHelperSQL.ExecuteSql(insertSQL, sqlparams);
                }

            }
            return 0;

        }




    }
}
