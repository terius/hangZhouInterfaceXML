using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Model
{
    [Serializable]
    [XmlRoot("RequestMessage", Namespace = "http://www.cnetong.com/AMS")]
    public class SendXMLInfo
    {
        public SendXMLInfo()
        {
            this.Head = new sendhead();
            this.Body = new sendbody();
        }
        [XmlElement("Head")]
        public sendhead Head { get; set; }
        [XmlElement("Body")]
        public sendbody Body { get; set; }
    }

    public class sendhead
    {
        public string MessageID { get; set; } = "f66f6b11-6b60-4a43-b6fc";

        public string MessageType { get; set; } = "802";

        public string RefMessageID { get; set; } = "38473";
        public string Sender { get; set; } = "SMS";
        public string Receiver { get; set; } = "CUS";
        public string SendTime { get; set; }
        public string FunctionCode { get; set; } = "21";
        public string Version { get; set; } = "1.0";
    }

    public class sendbody
    {
        public sendbody()
        {
            this.CMA_INFO_REC = new CMA_INFO_REC();
        }
        [XmlElement("CMA_INFO_REC")]
        public CMA_INFO_REC CMA_INFO_REC { get; set; }
    }

    public class CMA_INFO_REC
    {
        public string BILL_NO { get; set; }
        public string IE_FLAG { get; set; }
        public string LINE_ID { get; set; }
        public string VOYAGE_NO { get; set; }
        public string LOG_NO { get; set; }
        public string TRADE_CODE { get; set; }
        public string TRADE_NAME { get; set; }
        public string CHECK_RESULT { get; set; } = "P";
        public string CHECK_TIME { get; set; }
        public string CHECK_TYPE { get; set; } = "A";
        public string CHECK_MAN { get; set; }
        public string CHECK_INFO { get; set; }
    }
}
