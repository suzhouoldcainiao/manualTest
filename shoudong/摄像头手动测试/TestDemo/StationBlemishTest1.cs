using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;
using ImageAlgorithm;
using NST_CameraImageTest;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace TestDemo
{
    public class StationBlemishTest1:StationBase
    {
        public BlemishInfo ThisBlemishInfo = new BlemishInfo();
        public double blCount;
        public StationBlemishTest1(FormTest formtest, string stationName, TextBox textbox, string type, string sn, string imageDIR) : base(formtest, stationName, textbox, type, sn, imageDIR)
        {

        }
        public override bool Test()
        {
            bool b = false;
            try
            {
                
                LoadImage();
                SavePicture(pictureSavePath, win);
                string NGCutScreenImgPath = "D:\\TestImage\\NGCutScreenImgPath\\" + "BL-" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".bmp";
                int hv_countNg;
                HObject NGRegions;
                b= BlemishTest(NGCutScreenImgPath, win, bmp, ref ThisBlemishInfo, out hv_countNg);
                blCount = hv_countNg;
                SavePicture(pictureSavePath, win);
                SaveBlemishTestData(dataPath);
                return b;
            }
            catch (Exception e)
            {

                return b;
            }
        }
        public unsafe bool BlemishTest(string ngPath,HTuple winHandle,Bitmap bmp, ref BlemishInfo info,out int hvCountNg)
        {
            bool b = false;
            hvCountNg = 0;
            try
            {
                
                byte[] byBuffer = ConvertBitmapToByteArray(bmp);

                fixed (byte* pbuffer = byBuffer)
                {
                    CameraImageTest.BlemishTest(pbuffer, bmp.Width, bmp.Height, ref info);
                    hvCountNg = info.blResult.nBL_Count;
                    b = info.blResult.bBL_Res;
                    List<_LABEL> blP= info.blResult.lBL_Pos;
                    DrawPicCircle(win, blP);
                }
                return b ;
            }
            catch (Exception e)
            {

                return false;
            }
            

        }
        
        public void SaveBlemishTestData(string dataPath)
        {
            string info;
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            dataPath += DateTime.Now.ToString("yyyy-MM-dd") + ".cvs";
            if (!File.Exists(dataPath))
            {
                //File.Create(dataPath);
                info = $"SN,时间,结果,脏污数量";
                using (StreamWriter sw = new StreamWriter(dataPath, true))
                {
                    sw.WriteLine(info);
                }
            }
             info = $"{PSN},{DateTime.Now.ToString("HH：mm：ss")},{this.ThisBlemishInfo.blResult.bBL_Res},{this.ThisBlemishInfo.blResult.nBL_Count}";
            using (StreamWriter sw = new StreamWriter(dataPath, true))
            {
                sw.WriteLine(info);
            }
        }
    }
}
