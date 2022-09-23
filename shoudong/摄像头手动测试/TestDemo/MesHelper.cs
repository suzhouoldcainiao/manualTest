using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestDemo
{
    public class MesHelper
    {
        public static MesInDataListItem mesInDataListItem = new MesInDataListItem();//存放信息(工位 SN)
        public static DataUpTestDataItem 测试信息 = new DataUpTestDataItem();//存放测试信息
        public static MesInRoot Mes入站 = new MesInRoot();
        public static MesInBackRoot Mes入站返回结果 = new MesInBackRoot();
        public static DataUpRoot Mes数据上传 = new DataUpRoot();
        public static DataUpBackRoot Mes数据上传返回结果 = new DataUpBackRoot();
        SocketClient sc = new SocketClient();
        public static MESID_SAVE MESIDSAVE = new MESID_SAVE();
        private static MesHelper instance = null;
        private static object obj = new object();
        public string infoFromMes;
        public static MesHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (obj)
                    {
                        if (instance == null)
                        {
                            instance = new MesHelper();
                        }
                    }
                }
                return instance;
            }
        }
        public class MESID_SAVE
        {
            public static string LB_ID1 { get; set; } = "Non";
            public static string LB_ID2 { get; set; } = "Non";
            public static string LB_ID3 { get; set; } = "Non";
        }
        private MesHelper()
        {
            //   sc.EventReceive += Form_Main.EnveMesRecive;
            // sc.EventconnectState += Form_Main.EnveMesStatus;
        }
        public bool Connect()
        {
            
            try
            {
                string IP = "192.168.23.100";
                int Port = 12300;
                sc.Connect(IP, Port);
                string res = sc.Send("CHECK");
                infoFromMes = res;
                string res1 = res.Trim().ToLower();
                return res == "check";
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public void DisConnect()
        {
            
        }
        public bool 入站(ref string err)
        {
            
            bool bretry = false;
        retry_mes:
            try
            {
                Mes入站返回结果 = new MesInBackRoot();
                string result = Class2Json(Mes入站);
                string M = Chr(0X02) + result.Length + Chr(0X01) + result + Chr(0X03);
                Thread.Sleep(200);
                string res = sc.Send(M);
                Mes入站返回结果 = Json2Class<MesInBackRoot>(res);
                bool mesback = Mes入站返回结果.ResponseHead.Status.Trim().ToUpper() == "S";
                bool mesbackPN = Mes入站返回结果.ResponseHead.PN.Trim() == "";//ParamSetMgr.GetInstance().CurrentProductFile;
                string S = "";// ParamSetMgr.GetInstance().CurrentProductFile;
                if (!mesback)
                {
                    err = Mes入站返回结果.ResponseHead.ErrorDetail;
                    return false;
                }
                if (!mesbackPN)
                {
                    err = "当前产品型号不符" + Mes入站返回结果.ResponseHead.PN.Trim();
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                if (!bretry)
                {
                    bretry = true;
                    MesHelper.Instance.Connect();
                    goto retry_mes;
                }
                err = $"{ex}";
                return false;
            }
        }
        public static string Chr(int asciiCode)
        {
            if (asciiCode >= 0 && asciiCode <= 255)
            {
                System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                byte[] byteArray = new byte[] { (byte)asciiCode };
                string strCharacter = asciiEncoding.GetString(byteArray);
                return (strCharacter);
            }
            else
            {
                throw new Exception("ASCII Code is not valid.");
            }
        }
        public bool 数据上传(ref string err, DataUpRoot Mes数据上传)
        {
            
            bool bretry = false;
        retry_mes:
            try
            {
                err = "";
                Mes数据上传返回结果 = new DataUpBackRoot();
                string result = Class2Json(Mes数据上传);
                string M = Chr(0X02) + result.Length + Chr(0X01) + result + Chr(0X03);
                Thread.Sleep(200);
                string res = sc.Send(M);
                Mes数据上传返回结果 = Json2Class<DataUpBackRoot>(res);
                bool mesback = Mes数据上传返回结果.ResponseHead.Status.Trim().ToUpper() == "S";
                if (!mesback)
                {
                    err = Mes数据上传返回结果.ResponseHead.ErrorDetail;

                }
                return mesback;
            }
            catch (Exception ex)
            {
                if (!bretry)
                {
                    bretry = true;
                    MesHelper.Instance.Connect();
                    goto retry_mes;
                }
                err = $"{ex}";
                return false;
            }

        }
        public T Json2Class<T>(string value)
        {
            T model = default(T);
            try
            {
                string res = ""; bool star = false;
                value = value.Trim().TrimEnd();
                string b = value.Trim().TrimEnd();
                foreach (var a in value)
                {
                    if (a == '{')
                        star = true;
                    if (star)
                    {
                        res += a;
                    }
                }
                res = res.Replace("\0", "");
                res = res.Remove(res.Length - 1, 1);
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(res)))
                {
                    DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(typeof(T));
                    model = (T)deseralizer.ReadObject(ms);// //反序列化ReadObject
                }
            }
            catch (Exception ex)
            {
                return default(T);
            }
            return model;
        }
        public string Class2Json<T>(T value)
        {
            string json = "";
            try
            {
                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(T));
                MemoryStream msObj = new MemoryStream();
                //将序列化之后的Json格式数据写入流中
                js.WriteObject(msObj, value);
                msObj.Position = 0;
                //从0这个位置开始读取流中的数据
                StreamReader sr = new StreamReader(msObj, Encoding.UTF8);
                json = sr.ReadToEnd();
                sr.Close();
                msObj.Close();
            }
            catch (Exception ex)
            {
                return "";
            }
            return json;
        }
    }

    public class MesInReqHead
    {
        public string RequestID { get; set; } = System.Guid.NewGuid().ToString("N");
        public string ServiceName { get; set; } = "S00002";
        public string SourceSystem { get; set; } = "DEV";
        public string TimeStamp { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }




    public class MesInDataListItem
    {

        public string DEV_ID { get; set; } = "AS01";
        public string Process { get; set; } = "OP60";

        public string LB_ID1 { get; set; } = "Non";
        public string LB_ID2 { get; set; } = "Non";
        public string LB_ID3 { get; set; } = "Non";
    }
    [DataContract]
    public class MesInReqData
    {
        [DataMember(Order = 1)]
        public List<MesInDataListItem> DataList = new List<MesInDataListItem>();
    }
    [DataContract]
    public class MesInRoot
    {
        [DataMember(Order = 1)]
        public MesInReqHead ReqHead = new MesInReqHead();
        [DataMember(Order = 2)]
        public MesInReqData ReqData = new MesInReqData();
    }

    #region 数据上传
    public class DataUpReqHead
    {
        public string RequestID { get; set; } = System.Guid.NewGuid().ToString("N");
        public string ServiceName { get; set; } = "S00003";
        public string SourceSystem { get; set; } = "DEV";
        public string TimeStamp { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public class DataUpDataListItem
    {

        public string DEV_ID { get; set; } = "AS01";
        public string Process { get; set; } = "OP60";
        public string Result { get; set; } = "Fail";

        public string LB_ID1 { get; set; } = "Non";
        public string LB_ID2 { get; set; } = "Non";
        public string LB_ID3 { get; set; } = "Non";
    }

    public class DataUpTestDataItem
    {
        public string Operator { get; set; } = "NG";
        public string Operatortime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string ProgarmName { get; set; } = "NG";
        public string BadTypeId { get; set; } = "NG";
        public string ScrewNum { get; set; } = "";
        public string Torsion1 { get; set; } = "";
        public string Angle1 { get; set; } = "";
        public string Torsion2 { get; set; } = "";
        public string Angle2 { get; set; } = "";
        public string Hight { get; set; } = "";
    }
    [DataContract]
    public class DataUpReqData
    {
        [DataMember(Order = 1)]
        public List<DataUpDataListItem> DataList = new List<DataUpDataListItem>();
        [DataMember(Order = 2)]

        public List<DataUpTestDataItem> TestData = new List<DataUpTestDataItem>();
    }
    [DataContract]
    public class DataUpRoot
    {
        [DataMember(Order = 1)]
        public DataUpReqHead ReqHead = new DataUpReqHead();
        [DataMember(Order = 2)]
        public DataUpReqData ReqData = new DataUpReqData();
    }
    #endregion

    public class MesInBackReqHead
    {
        public string Status { get; set; } = "NG";
        public string ErrorDetail { get; set; } = "NG";
        public string RequestID { get; set; } = "NG";
        public string PN { get; set; } = "NG";
    }
    public class MesInBackDataListItem
    {
        public string LB_ID { get; set; } = "NG";
        public string DEV_ID { get; set; } = "NG";
        public string CHECK_RESULT { get; set; } = "NG";
        public string PN { get; set; } = "NG";

    }
    [DataContract]
    public class MesInDataBackReqData
    {
        [DataMember(Order = 1)]
        public List<MesInBackDataListItem> DataList = new List<MesInBackDataListItem>();
    }
    [DataContract]
    public class MesInBackRoot
    {
        [DataMember(Order = 1)]
        public MesInBackReqHead ResponseHead = new MesInBackReqHead();
        [DataMember(Order = 2)]
        public MesInDataBackReqData ResponseData = new MesInDataBackReqData();

    }
 

 
    public class DataBackReqHead
    {
        public string Status { get; set; } = "NG";
        public string ErrorDetail { get; set; } = "NG";
        public string RequestID { get; set; } = "NG";
    }
    public class DataBackDataListItem
    {

    }
    [DataContract]
    public class DataBackReqData
    {
        [DataMember(Order = 1)]
        public List<DataBackDataListItem> DataList = new List<DataBackDataListItem>();
    }
    [DataContract]
    public class DataUpBackRoot
    {
        [DataMember(Order = 1)]
        public DataBackReqHead ResponseHead = new DataBackReqHead();
        [DataMember(Order = 2)]
        public DataBackReqData ResponseData = new DataBackReqData();

    }
}
