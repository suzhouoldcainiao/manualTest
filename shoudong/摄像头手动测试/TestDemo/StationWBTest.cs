using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using NST_CameraImageTest;
using ImageAlgorithm;
using HalconDotNet;
using System.IO;

namespace TestDemo
{
    public class StationWBTest:StationBase
    {
        public StationWBTest(FormTest formtest, string stationName, TextBox textbox, string type, string sn, string imageDIR,int roiW,int roiH,int colorTempareture) : base(formtest, stationName, textbox, type, sn, imageDIR)
        {
            this.roiW = roiW;
            this.roiH = roiH;
            
            switch (colorTempareture)
            {
                case 0:
                    colorT = ColorT.k2800;
                    break;
                case 1:
                    colorT = ColorT.k4000;
                    break;
                case 2:
                    colorT = ColorT.k5500;
                    break;
                default:
                    break;
            }
        }
        int roiW;
        int roiH;
        ColorT colorT;
        public WhiteBalanceInfo wBinfo = new WhiteBalanceInfo();
        public double RG;
        public double BG;
        public double R;
        public double G;
        public double B;
        public override bool Test()
        {
            bool b = false;
            try
            {
                LoadImage();
                SavePicture(pictureSavePath, win);
                b = GetWBValue(bmp, wBinfo, win);
                SavePicture(pictureSavePath, win);
                SaveWBTestData(dataPath);
                return b;
            }
            catch (Exception)
            {

                return false;
            }
        }
        public  bool Test1()
        {
            bool b = false;
            try
            {
                LoadImage();
                SavePicture(pictureSavePath, win);
                b = GetWBValue1(bmp, wBinfo, win);
                SavePicture(pictureSavePath, win);
                SaveWBTestData(dataPath);
                return b;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
        public bool GetWBValue1(Bitmap bitmap,WhiteBalanceInfo wbinfo,HWindow win)
        {
            try
            {
                HObject ho_Image, ho_Image1, ho_Image2, ho_Image3;
                HObject ho_rect, ho_ImageReduced1, ho_ImageReduced2, ho_ImageReduced3;

                // Local control variables 

                HTuple hv_Width = null, hv_Height = null, hv_Mean1 = null;
                HTuple hv_Deviation1 = null, hv_Mean2 = null, hv_Deviation2 = null;
                HTuple hv_Mean3 = null, hv_Deviation3 = null;
                // Initialize local and output iconic variables 
                HOperatorSet.GenEmptyObj(out ho_Image);
                HOperatorSet.GenEmptyObj(out ho_Image1);
                HOperatorSet.GenEmptyObj(out ho_Image2);
                HOperatorSet.GenEmptyObj(out ho_Image3);
                HOperatorSet.GenEmptyObj(out ho_rect);
                HOperatorSet.GenEmptyObj(out ho_ImageReduced1);
                HOperatorSet.GenEmptyObj(out ho_ImageReduced2);
                HOperatorSet.GenEmptyObj(out ho_ImageReduced3);
                ho_Image.Dispose();
                HOperatorSet.ReadImage(out ho_Image, imagedir);

                ho_Image1.Dispose(); ho_Image2.Dispose(); ho_Image3.Dispose();
                HOperatorSet.Decompose3(ho_Image, out ho_Image1, out ho_Image2, out ho_Image3
                    );
                HOperatorSet.GetImageSize(ho_Image1, out hv_Width, out hv_Height);
                ho_rect.Dispose();
                HOperatorSet.GenRectangle2(out ho_rect, hv_Height / 2, hv_Width / 2, 0, roiW,roiH);

                ho_ImageReduced1.Dispose();
                HOperatorSet.ReduceDomain(ho_Image1, ho_rect, out ho_ImageReduced1);
                ho_ImageReduced2.Dispose();
                HOperatorSet.ReduceDomain(ho_Image2, ho_rect, out ho_ImageReduced2);
                ho_ImageReduced3.Dispose();
                HOperatorSet.ReduceDomain(ho_Image3, ho_rect, out ho_ImageReduced3);

                HOperatorSet.Intensity(ho_rect, ho_ImageReduced1, out hv_Mean1, out hv_Deviation1);
                HOperatorSet.Intensity(ho_rect, ho_ImageReduced2, out hv_Mean2, out hv_Deviation2);
                HOperatorSet.Intensity(ho_rect, ho_ImageReduced3, out hv_Mean3, out hv_Deviation3);
                RG = wbinfo.wbResult.dWB_RG = hv_Mean1.D / hv_Mean2.D;
                BG = wbinfo.wbResult.dWB_BG = hv_Mean3.D / hv_Mean2.D;
                R = wbinfo.wbResult.dWB_R = hv_Mean1;
                G = wbinfo.wbResult.dWB_G = hv_Mean2;
                B = wbinfo.wbResult.dWB_B = hv_Mean3;
                DrawRect1(win, bitmap.Height / 2, bitmap.Width / 2, roiW, roiH);
                ho_Image.Dispose();
                ho_Image1.Dispose();
                ho_Image2.Dispose();
                ho_Image3.Dispose();
                ho_rect.Dispose();
                ho_ImageReduced1.Dispose();
                ho_ImageReduced2.Dispose();
                ho_ImageReduced3.Dispose();
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
        public unsafe bool  GetWBValue(Bitmap bitmap,WhiteBalanceInfo wbinfo,HWindow hWindow)
        {
            bool b;
            try
            {
                byte[] byBuffer = ConvertBitmapToByteArray(bitmap);
                fixed (byte* pbuffer = byBuffer)
                {
                    b = CameraImageTest.WhiteBalanceTest(pbuffer, bitmap.Width, bitmap.Height, ref wbinfo);

                }
                if (b)
                {
                    RG = wbinfo.wbResult.dWB_RG/100;
                    BG = wbinfo.wbResult.dWB_BG/100;
                    R = wbinfo.wbResult.dWB_R;
                    G = wbinfo.wbResult.dWB_G;
                    B = wbinfo.wbResult.dWB_B;
                    double lenght1 = Convert.ToDouble(NSTIni.ReadValue("WBParam", "nWB_ROIWidth"));
                    double lenght2 = Convert.ToDouble(NSTIni.ReadValue("WBParam", "nWB_ROIHeight"));
                    DrawRect1(win, bitmap.Height/2, bitmap.Width/2,lenght1,lenght2);      
                }
                return b;
            }
            catch (Exception e)
            {

                return false;
            }
        }
        public void SaveWBTestData(string dataPath)
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
                info = $"SN,时间,色温,R/G,B/G,R,G,B";
                using (StreamWriter sw = new StreamWriter(dataPath, true))
                {
                    sw.WriteLine(info);
                }
            }
            info = $"{PSN},{DateTime.Now.ToString("HH：mm：ss")},{this.colorT.ToString()},{this.RG},{this.BG},{this.R},{this.G},{this.B}";
            using (StreamWriter sw = new StreamWriter(dataPath, true))
            {
                sw.WriteLine(info);
            }
        }
        enum ColorT
        {
            k2800,
            k4000,
            k5500
        }
    }
}
