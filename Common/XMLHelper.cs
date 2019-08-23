﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Xml;
using System.Xml.Serialization;

namespace Common
{
    /// <summary>
    /// Xml序列化与反序列化
    /// </summary>
    public class XmlHelper
    {
        #region 反序列化
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="xml">XML字符串</param>
        /// <returns></returns>
        public static T Deserialize<T>(string xml)
        {
            try
            {
                Encoding encoding = Encoding.UTF8;
                XmlSerializer mySerializer = new XmlSerializer(typeof(T));
                using (MemoryStream ms = new MemoryStream(encoding.GetBytes(xml)))
                {
                    using (StreamReader sr = new StreamReader(ms, encoding))
                    {
                        return (T)mySerializer.Deserialize(sr);
                    }
                }
            }
            catch (Exception e)
            {
                return default(T);
            }
          



            //try
            //{
            //    using (StringReader sr = new StringReader(xml))
            //    {
            //        XmlSerializer xmldes = new XmlSerializer(typeof(T));
                
            //        return (T)xmldes.Deserialize(sr);
            //    }
            //}
            //catch (Exception e)
            //{

            //    return default(T);
            //}
        }


        public static T DeserializeFromFile<T>(string fileName)
        {
            FileStream fs = null;
            
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                XmlSerializer xs = new XmlSerializer(typeof(T));
                
                return (T)xs.Deserialize(fs);
            }
            catch (Exception ex)
            {
                Loger.LogMessage(ex);
                return default(T);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static object Deserialize(Type type, Stream stream)
        {
            XmlSerializer xmldes = new XmlSerializer(type);
            return xmldes.Deserialize(stream);
        }
        #endregion

        #region 序列化
        ///// <summary>
        ///// 序列化
        ///// </summary>
        ///// <param name="type">类型</param>
        ///// <param name="obj">对象</param>
        ///// <returns></returns>
        //public static string SerializerOld<T>(T obj)
        //{
        //    MemoryStream Stream = new MemoryStream();

        //    XmlSerializer xml = new XmlSerializer(typeof(T));

        //    try
        //    {
        //        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

        //        //Add an empty namespace and empty value
        //        ns.Add("", "");
        //        XmlTextWriter textWriter = new XmlTextWriter(Stream, Encoding.UTF8);//定义输出的编码格式

        //        //序列化对象
        //        xml.Serialize(textWriter, obj, ns);
        //    }
        //    catch (InvalidOperationException)
        //    {
        //        throw;
        //    }
        //    Stream.Position = 0;
        //    StreamReader sr = new StreamReader(Stream);
        //    string str = sr.ReadToEnd();

        //    sr.Dispose();
        //    Stream.Dispose();

        //    return str;
        //}

        public static string Serializer<T>(T obj)
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
           
            var xml = "";
            using (var sww = new Utf8StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    //   XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

                    //Add an empty namespace and empty value
                    //  ns.Add("", "");
                    xsSubmit.Serialize(writer, obj);
                    xml = sww.ToString(); // Your XML
                }
            }
            return xml;
        }

        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }

        public static void SerializerToFile<T>(T obj, string fileName)
        {
            var xmlString = Serializer<T>(obj);

            SaveToFile(xmlString, fileName);

        }

        public static void SerializerDataSetToFile(DataSet ds, string fileName)
        {
            FileInfo fi = new FileInfo(fileName);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            ds.WriteXml(fileName);
        }

        public static void SaveToFile(string xmlString, string fileName)
        {
            if (!string.IsNullOrEmpty(xmlString))
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.Write(xmlString);
                        sw.Close();
                    }
                }
            }
        }

        public static string ReadXMLToString(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            return doc.OuterXml;
        }

        #endregion
    }
}
