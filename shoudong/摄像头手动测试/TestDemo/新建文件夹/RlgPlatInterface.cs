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
