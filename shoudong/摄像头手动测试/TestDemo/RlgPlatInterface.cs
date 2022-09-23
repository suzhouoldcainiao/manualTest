using RLGCSHisFX3Platform;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace RlgCSSDKDemo
{
    public enum GrabDataFormat
    {
        SourceFormat = 0,
        BGRFormat = _CSHisFX3_BaylorMode.HisRGB_BGR24,
        RGBFormat = _CSHisFX3_BaylorMode.HisRGB_RGB24,
        YUVFormat = _CSHisFX3_BaylorMode.HisYUV8_422_UYVY
    };

    public class RlgPlatInterface
    {
        #region static变量
        private static CSHisFX3PlatSDK m_pInterface = new CSHisFX3PlatSDK();

        #endregion

        #region 成员变量
        private int m_cam;
        public uint width = 0, height = 0;
        public int nFrameBytes;
        #endregion

        #region static方法
        public static CSHisFX3PlatSDK GetInstance()
        {
            return m_pInterface;
        }
        #endregion

        #region public方法
        public RlgPlatInterface(int cam)
        {
            m_cam = cam;
        }

        public bool SensorStopPreview(string sq = "")
        {
            int nRst = m_pInterface.HisFX3StopPreview(sq, m_cam);
            return (nRst == 0) ? true : false;
        }

        public bool SensorStartPreview()
        {
            int nRst = m_pInterface.HisFX3StartPreview(m_cam);//10792
            return (nRst == 0) ? true : false;
            //return true;
        }

        public bool GrabFrame(ref IntPtr pBuf, GrabDataFormat dataFormat)
        {
            UInt32 frameIndex = 0, recSize = 0;
            bool bErrFrame = false;

            int nRst = m_pInterface.HisFX3GrabFrame(ref pBuf, nFrameBytes, ref frameIndex, ref bErrFrame, ref recSize, (uint)dataFormat, 2000, m_cam);

            return (nRst == 0) ? true : false;
        }

        public bool GetImageInfo(GrabDataFormat dataFormat)
        {
            m_pInterface.getPreviewImageSize(ref width, ref height, ref nFrameBytes, (_CSHisFX3_BaylorMode)dataFormat, m_cam);

            return (nFrameBytes == 0) ? false : true;
        }
        #endregion
        
        public bool GetBoxTotalCurrent(ref double current)
        {
            int i=m_pInterface.HisFX3MeasureBoxTotalCurrent(ref current, 0);
            return (i == 0) ? true : false;
        }
        public uint GetIICAutoAckEnabled()
        {
            uint i = m_pInterface.HisFX3IsIICAutoAckEnabled(m_cam);
            return i;
        }
		//int HisFX3SetIICSpeed(unsigned int speed, int cam);
        public bool SetIICSpeed(uint speed)
        {
            int i = m_pInterface.HisFX3SetIICSpeed(speed, m_cam);
            return (i == 0) ? true : false;
        }
        //unsigned int HisFX3GetIICSpeed(int cam);
        public uint GetIICSpeed()
        {
            return m_pInterface.HisFX3GetIICSpeed(m_cam);
        }
        //bool HisFX3IICResponds(unsigned char slave, int cam);
        public bool GetIICResponds(byte slave)
        {
            return m_pInterface.HisFX3IICResponds(slave, m_cam);
        }
        //! 设置控制通道通信协议
        /*!
		\param[in] protocal 详见_HisFX3_CommunicationProtocal
		\param[in] cam 模组编号
		\return 0:成功  非0:失败
		\sa _HisFX3_CommunicationProtocal
		*/
        //int HisFX3SetControlCommunicationProtocal(_CSHisFX3_CommunicationProtocal protocal, int cam);
        public bool SetControlProtatal(_CSHisFX3_CommunicationProtocal protocal)
        {
            int i = m_pInterface.HisFX3SetControlCommunicationProtocal(_CSHisFX3_CommunicationProtocal._HisFX3_CommProtocal_I2C, m_cam);
            return (i == 0) ? true : false;
        }
        //! 单条IIC写入@@@
        /*!
		\param[in] slave 设备地址
		\param[in] reg 寄存器地址
		\param[in] data 数据
		\param[in] type 类型: 0x0808, 0x1608, 0x1632...
		\param[in] ack 是否判定ACK响应
		\param[in] cam 模组编号
		\return 0:成功  非0:失败
		\sa
		*/
        //int HisFX3WriteIIC(unsigned char slave, unsigned int reg, unsigned __int64 data, unsigned short type, bool ack, int cam);
        public bool WriteIIC(byte slave, uint reg, ulong data, ushort type,bool ack)
        {
            int i = m_pInterface.HisFX3WriteIIC(slave, reg, data, type, ack, m_cam);
            return (i == 0) ? true : false;
        }

        //! 单条IIC读取
        /*!
		\param[in] slave 设备地址
		\param[in] reg 寄存器地址
		\param[out] data 数据
		\param[in] ack 是否判定ACK响应
		\param[in] cam 模组编号
		\return 0:成功  非0:失败
		\sa
		*/
        //int HisFX3ReadIIC(unsigned char slave, unsigned int reg, Int64% data, unsigned short type, int cam);
        public bool ReadIIC(byte slave,uint reg,ref long data,ushort type)
        {
            int i = m_pInterface.HisFX3ReadIIC(slave, reg, ref data, type, m_cam);
            return (i == 0) ? true : false;
        }
        //! 设置SENSOR IIC线连接或断开
        /*!
		\param[in] on true: 链接； false: 断开
		\param[in] cam 模组编号
		\return 0:成功  非0:失败
		\sa
		*/
        //int HisFX3ConnectSensorIIC(bool on, int cam);
        public bool ConnectSensorIIC(bool on)
        {
            int i = m_pInterface.HisFX3ConnectSensorIIC(on, m_cam);
            return (i == 0) ? true : false;
        }

        // 设置SENSOR IIC线连接状态
        /*!
		\param[in] type
		\param[in] cam 模组编号
		\return 0:成功  非0:失败
		\sa _HisFX3_CommunicationProtocal
		*/
        //int HisFX3SetSensorIICConnectType(_CSHisFX3_CommunicationProtocal type, int cam);
        public bool SetSensorIICConnectType()
        {
            int i = m_pInterface.HisFX3SetSensorIICConnectType(_CSHisFX3_CommunicationProtocal._HisFX3_CommProtocal_I2C, m_cam);
            return (i == 0) ? true : false;
        }
        //! 批量IIC读取
        /*!
		\param[in] count IIC条数
		\param[in] slave 设备地址
		\param[in] reg 寄存器地址
		\param[out] data 数据
		\param[in] type 类型: 0x0808, 0x1608, 0x1632...
		\param[in] delay 每条IIC读取之间的间隔时间， 单位:us
		\param[in] cam 模组编号
		\return 0:成功  非0:失败
		\sa
		*/
        //int HisFX3BatchReadIICNoLimit(unsigned int count, array<Byte>^ slave, array<UInt32>^ reg, array<UInt32>^ data, array<UInt16>^ type, unsigned short delay, int cam);
        public bool ReadIICMult(uint count,byte[] slaves,UInt32[] regs, UInt32[] datas,ushort[] types,ushort dalay)
        {
            int i = m_pInterface.HisFX3BatchReadIICNoLimit(count, slaves, regs, datas, types, dalay, m_cam);
            return (i == 0) ? true : false;
        }


        //! 批量IIC写入
        /*!
		\param[in] count IIC条数
		\param[in] slave 设备地址
		\param[in] reg 寄存器地址
		\param[in] data 数据
		\param[in] type 类型: 0x0808, 0x1608, 0x1632...
		\param[in] delay 每条IIC写入之间的间隔时间， 单位:us
		\param[in] cam 模组编号
		\return 0:成功  非0:失败
		\sa
		*/
        //int HisFX3BatchWriteIICNoLimit(unsigned int count, array<Byte>^ slave, array<UInt32>^ reg, array<UInt32>^ data, array<UInt16>^ type, unsigned short delay, int cam);
        public bool WriteIICMult(uint counts,byte[] slaves,UInt32[] regs,UInt32[] datas,ushort[] types,ushort delay)
        {
            int i = m_pInterface.HisFX3BatchWriteIICNoLimit(counts, slaves, regs, datas, types, delay, m_cam);
            return (i == 0) ? true : false;
        }
    }




    public class RlgCommFuction
    {
        #region static变量
        private static CSHisFX3ImageAlg m_pImageAlg = new CSHisFX3ImageAlg();
        #endregion

        public static int HisCCMMTF(IntPtr imageRGBBuf, int iWidth, int iHeight, _CSRect stRange,
            uint uiAlgorithm, ref double flMTFValue, bool bGChannel)
        {
            return m_pImageAlg.HisCCMMTF(imageRGBBuf, iWidth, iHeight, stRange, uiAlgorithm, ref flMTFValue, bGChannel);
        }

        public static string GetString(string section, string key, string def)
        {
            StringBuilder strb = new StringBuilder();
            GetPrivateProfileString(section, key, def, strb, 255, ".\\HisFx3Global.ini");
            return strb.ToString();
        }

        public static void SetString(string section, string key, string val)
        {
            WritePrivateProfileString(section, key, val, ".\\HisFx3Global.ini");
        }


        [DllImport("kernel32", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
        /// <summary>
        /// 修改INI配置文件
        /// </summary>
        /// <param name="section">段落</param>
        /// <param name="key">关键字</param>
        /// <param name="val">值</param>
        /// <param name="filepath">文件完整路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
        internal static void HisCCMMTF(IntPtr pBuf, int nwidth, int nheight, Rectangle rect1, int v1, ref double flMTF, bool v2)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 读INI配置文件
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="def">缺省值</param>
        /// <param name="retval"></param>
        /// <param name="size">指定装载到lpReturnedString缓冲区的最大字符数量</param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);

        internal static void HisCCMMTF(Image image, int v1, int v2, _CSRect cSRect, int v3, ref double flMTF, bool v4)
        {
            throw new NotImplementedException();
        }

    }

}
