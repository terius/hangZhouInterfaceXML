using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Model
{
    [Serializable]
    [XmlRoot("RequestMessage", Namespace = "http://www.cnetong.com/AMS")]

    public class NEWXMLInfo2
    {

        public head2 Head { get; set; }
        public body2 Body { get; set; }
    }

    public class head2
    {
        public string MessageID { get; set; }

        public string MessageType { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string SendTime { get; set; }
        public string Version { get; set; }
    }


    public class body2
    {
        [XmlElement("CMA_INFO")]
        public List<CMA_INFO> CMA_INFO { get; set; }

    }

    public class CMA_INFO
    {
        [XmlElement("GOODS_HEAD")]
        public GOODS_HEAD GOODS_HEAD { get; set; }
        [XmlElement("GOODS_BODY")]
        public List<GOODS_BODY> GOODS_BODY { get; set; }


        public string MAIN_G_NAME
        {
            get
            {
                string str = "";
                double itemp = 0;
                foreach (var item in GOODS_BODY)
                {
                    itemp = 0;
                    double.TryParse(item.QTY, out itemp);
                   
                    if (str == "")
                    {
                        str = item.GOODS_NAME + "*" + (int)itemp;
                    }
                    else
                    {
                        str += "¦" + item.GOODS_NAME + "*" + (int)itemp;
                    }
                }
                return str;
            }
        }
    }

    public class GOODS_HEAD
    {
        public string TRAF_NAME { get; set; }
        public string VOYAGE_NO { get; set; }
        public string IE_FLAG { get; set; }
        public string LOG_NO { get; set; }
        public string PACK_NUM { get; set; }
        public string GROSS_WT { get; set; }
        public string GOODS_VALUE { get; set; }
        public string DEC_TYPE { get; set; }
        public string BILL_NO { get; set; }
    }

    public class GOODS_BODY
    {
        public string LIST_NO { get; set; }
        public string GOODS_NAME { get; set; }
        public string QTY { get; set; }
        public string UNIT { get; set; }
    }



}