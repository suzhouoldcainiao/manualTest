using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NST_CameraImageTest;
using HalconDotNet;
using System.Drawing;
using ImageAlgorithm;
using System.IO;

namespace TestDemo
{
    class StationLensShadingTest : StationBase
    {
        public double[] pcr=new double[3];
        public double[] ppr=new double[3];
        public LensShadingInfo linfo = new LensShadingInfo();
        int roi;
        double x, y;
        _ROI[] _ROIs1 = new _ROI[5];
        LensShadingInfo shadingInfo;
        double[] centerToCorner=new double[3];
        double[] cornerToCorner=new double[3];
        public double greypcr;
        public double greyppr;
        public StationLensShadingTest(FormTest formtest, string stationName, TextBox textbox, string type, string sn, string imageDIR,int roiLenght,double x,double y) : base(formtest, stationName, textbox, type, sn, imageDIR)
        {
            roi = roiLenght;
            this.x = x;
            this.y = y;
            shadingInfo = new LensShadingInfo();
            LoadImage();
            for (int i = 0; i < _ROIs1.Length; i++)
            {
                _ROIs1[i] = new _ROI();
            }
            GetRoi(roi, x, y, _ROIs1);
            shadingInfo.lsResult.aryLS_ROI = _ROIs1;
        }
        public void GetRoi(int roi,double x,double y,params _ROI[] _ROIs1)
        {
            int w = bmp.Width / 2;
            int h = bmp.Height / 2;
            _ROIs1[0].x = w;
            _ROIs1[0].y = h;
            _ROIs1[1].x = (int)(w * (1 - x));
            _ROIs1[1].y = (int)(h * (1 - y));
            _ROIs1[2].x = (int)(w * (1 - x));
            _ROIs1[2].y = (int)(h * (1 + y));
            _ROIs1[3].x = (int)(w * (1 + x));
            _ROIs1[3].y = (int)(h * (1 - y));
            _ROIs1[4].x = (int)(w * (1 + x));
            _ROIs1[4].y = (int)(h * (1 + y));
            foreach (_ROI item in _ROIs1)
            {
                item.width = item.height = roi;
            }
        }
        public override bool Test()
        {
            bool b = false;
            try
            {
                LoadImage();
                SavePicture(pictureSavePath, win);
                b = GetLensShadingInfo(bmp, linfo, win);
                SavePicture(pictureSavePath, win);
                SaveLensShadingTestData(dataPath);
                
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
                //LoadImage();
                //GetRoi(roi, x, y);
                SavePicture(pictureSavePath, win);
                b = GetLensShadingInfo1(bmp, shadingInfo, win);
                SavePicture(pictureSavePath, win);
                SaveLensShadingTestData(dataPath);

                return b;
            }
            catch (Exception)
            {

                return false;
            }
        }
        public bool Test2()
        {
            bool b = false;
            try
            {
                //LoadImage();
                //GetRoi(roi, x, y);
                SavePicture(pictureSavePath, win);
                b = GetLensShadingInfo2(bmp, shadingInfo, win);
                SavePicture(pictureSavePath, win);
                SaveLensShadingTestData(dataPath);

                return b;
            }
            catch (Exception)
            {

                return false;
            }
        }
        /// <summary>
        /// dll原有算法，只有四周没有中心，也不清楚是gray或者color
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="info"></param>
        /// <param name="win"></param>
        /// <returns></returns>
        public unsafe bool GetLensShadingInfo(Bitmap bitmap, LensShadingInfo info, HWindow win)
        {
            bool b;
            try
            {
                byte[] byBuffer = ConvertBitmapToByteArray(bitmap);
                fixed (byte* pbuffer = byBuffer)
                {
                   b= CameraImageTest.LensShadingTest(pbuffer, bitmap.Width, bitmap.Height, ref info);
                    
                }
                if (b)
                {
                    greyppr = info.lsResult.dLS_Delta / ((info.lsResult.dLS_LL + info.lsResult.dLS_LR + info.lsResult.dLS_UL + info.lsResult.dLS_UR) / 4);
                    foreach (_ROI item in info.lsResult.aryLS_ROI)
                    { 
                        DrawRect1(win, item.y, item.x, item.width, item.height);
                    }
                }
                return b;
            }
            catch (Exception e)
            {

                return false;
            }
        }
        /// <summary>
        /// gray
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="info"></param>
        /// <param name="win"></param>
        /// <returns></returns>
        public  bool GetLensShadingInfo1(Bitmap bitmap,LensShadingInfo info,HWindow win)
        {
            try
            {
                HObject ho_Image, ho_Image1, ho_Image2, ho_Image3;
                HObject ho_ImageGray, ho_rect, ho_ImageReduced;

                // Local control variables 

                HTuple hv_Width = null, hv_Height = null, hv_Width1 = null;
                HTuple hv_Height1 = null, hv_Mean = null, hv_Deviation = null;
                // Initialize local and output iconic variables 
                HOperatorSet.GenEmptyObj(out ho_Image);
                HOperatorSet.GenEmptyObj(out ho_Image1);
                HOperatorSet.GenEmptyObj(out ho_Image2);
                HOperatorSet.GenEmptyObj(out ho_Image3);
                HOperatorSet.GenEmptyObj(out ho_ImageGray);
                HOperatorSet.GenEmptyObj(out ho_rect);
                HOperatorSet.GenEmptyObj(out ho_ImageReduced);
                ho_Image.Dispose();
                HOperatorSet.ReadImage(out ho_Image, imagedir);
                ho_Image1.Dispose(); ho_Image2.Dispose(); ho_Image3.Dispose();
                HOperatorSet.Decompose3(ho_Image, out ho_Image1, out ho_Image2, out ho_Image3);
                ho_ImageGray.Dispose();
                HOperatorSet.Rgb3ToGray(ho_Image1, ho_Image2, ho_Image3, out ho_ImageGray);
                //rgb1_to_gray (Image, GrayImage)
                HOperatorSet.GetImageSize(ho_ImageGray, out hv_Width, out hv_Height);
                ho_rect.Dispose();
                int roiI = info.lsResult.aryLS_ROI.Length;
                double[] means = new double[roiI];
                double[] deviations = new double[roiI];
                for (int i = 0; i < roiI; i++)
                {
                    HOperatorSet.GenRectangle2(out ho_rect, hv_Height / 2, hv_Width / 2, 0, info.lsResult.aryLS_ROI[i].x, info.lsResult.aryLS_ROI[i].y);
                    ho_ImageReduced.Dispose();
                    HOperatorSet.ReduceDomain(ho_ImageGray, ho_rect, out ho_ImageReduced);
                    HOperatorSet.Intensity(ho_rect, ho_ImageReduced, out hv_Mean, out hv_Deviation);
                    means[i] = hv_Mean;
                    deviations[i] = hv_Deviation;
                }
                double[] means1 = new double[means.Length - 1];
                double sum = 0;
                for (int i = 1; i < means.Length; i++)
                {
                    means1[i - 1] = means[i];
                    sum += means[i];
                }
                Array.Sort(means1);
                centerToCorner[0] = Math.Abs(means[0] - means1[0]);
                cornerToCorner[0] = Math.Abs(means1[0] - means1[means1.Length - 1]);
                greyppr = cornerToCorner[0] / (sum / 4);
                greypcr =  means1[0]/means[0];
                foreach (_ROI item in info.lsResult.aryLS_ROI)
                {
                    DrawRect1(win, item.y, item.x, item.width, item.height);
                }
                //HOperatorSet.GenRectangle2(out ho_rect, hv_Height / 2, hv_Width / 2, 0, info.lsResult.aryLS_ROI[0].x, info.lsResult.aryLS_ROI[0].y);
                //ho_ImageReduced.Dispose();
                //HOperatorSet.ReduceDomain(ho_ImageGray, ho_rect, out ho_ImageReduced);
                //HOperatorSet.GetImageSize(ho_ImageReduced, out hv_Width1, out hv_Height1);
                //crop_domain (ImageReduced, ImagePart)
                //get_image_size (ImagePart, Width2, Height2)
                //HOperatorSet.Intensity(ho_rect, ho_ImageReduced, out hv_Mean, out hv_Deviation);


                ho_Image.Dispose();
                ho_Image1.Dispose();
                ho_Image2.Dispose();
                ho_Image3.Dispose();
                ho_ImageGray.Dispose();
                ho_rect.Dispose();
                ho_ImageReduced.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// color
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="info"></param>
        /// <param name="win"></param>
        /// <returns></returns>
        public bool GetLensShadingInfo2(Bitmap bitmap, LensShadingInfo info, HWindow win)
        {
            try
            {
                HObject ho_Image, ho_Image1, ho_Image2, ho_Image3;
                HObject ho_rect, ho_ImageReduced;

                // Local control variables 

                HTuple hv_Width = null, hv_Height = null, hv_Width1 = null;
                HTuple hv_Height1 = null, hv_Mean = null, hv_Deviation = null;
                // Initialize local and output iconic variables 
                HOperatorSet.GenEmptyObj(out ho_Image);
                HOperatorSet.GenEmptyObj(out ho_Image1);
                HOperatorSet.GenEmptyObj(out ho_Image2);
                HOperatorSet.GenEmptyObj(out ho_Image3);
                
                HOperatorSet.GenEmptyObj(out ho_rect);
                HOperatorSet.GenEmptyObj(out ho_ImageReduced);
                ho_Image.Dispose();
                HOperatorSet.ReadImage(out ho_Image, imagedir);
                ho_Image1.Dispose(); ho_Image2.Dispose(); ho_Image3.Dispose();
                HOperatorSet.Decompose3(ho_Image, out ho_Image1, out ho_Image2, out ho_Image3
                    );
                //ho_ImageGray.Dispose();
                //HOperatorSet.Rgb3ToGray(ho_Image1, ho_Image2, ho_Image3, out ho_ImageGray);
                //rgb1_to_gray (Image, GrayImage)
                HOperatorSet.GetImageSize(ho_Image1, out hv_Width, out hv_Height);
                ho_rect.Dispose();
                int roiI = info.lsResult.aryLS_ROI.Length;
                double[] meansR = new double[roiI];
                double[] meansG = new double[roiI];
                double[] meansB = new double[roiI];
                double[] deviationsR = new double[roiI];
                double[] deviationsG = new double[roiI];
                double[] deviationsB = new double[roiI];
                for (int i = 0; i < roiI; i++)
                {
                    HOperatorSet.GenRectangle2(out ho_rect, hv_Height / 2, hv_Width / 2, 0, info.lsResult.aryLS_ROI[i].x, info.lsResult.aryLS_ROI[i].y);
                    ho_ImageReduced.Dispose();
                    HOperatorSet.ReduceDomain(ho_Image1, ho_rect, out ho_ImageReduced);
                    HOperatorSet.Intensity(ho_rect, ho_ImageReduced, out hv_Mean, out hv_Deviation);
                    meansR[i] = hv_Mean;
                    deviationsR[i] = hv_Deviation;
                }
                for (int i = 0; i < roiI; i++)
                {
                    HOperatorSet.GenRectangle2(out ho_rect, hv_Height / 2, hv_Width / 2, 0, info.lsResult.aryLS_ROI[i].x, info.lsResult.aryLS_ROI[i].y);
                    ho_ImageReduced.Dispose();
                    HOperatorSet.ReduceDomain(ho_Image2, ho_rect, out ho_ImageReduced);
                    HOperatorSet.Intensity(ho_rect, ho_ImageReduced, out hv_Mean, out hv_Deviation);
                    meansG[i] = hv_Mean;
                    deviationsG[i] = hv_Deviation;
                }
                for (int i = 0; i < roiI; i++)
                {
                    HOperatorSet.GenRectangle2(out ho_rect, hv_Height / 2, hv_Width / 2, 0, info.lsResult.aryLS_ROI[i].x, info.lsResult.aryLS_ROI[i].y);
                    ho_ImageReduced.Dispose();
                    HOperatorSet.ReduceDomain(ho_Image3, ho_rect, out ho_ImageReduced);
                    HOperatorSet.Intensity(ho_rect, ho_ImageReduced, out hv_Mean, out hv_Deviation);
                    meansB[i] = hv_Mean;
                    deviationsB[i] = hv_Deviation;
                }
                double[] means1 = new double[meansR.Length - 1];
                double sum = 0;
                for (int i = 1; i < meansR.Length; i++)
                {
                    means1[i - 1] = meansR[i];
                    sum += meansR[i];
                }
                Array.Sort(means1);
                centerToCorner[0] = Math.Abs(meansR[0] - means1[0]) > Math.Abs(meansR[0] - means1[means1.Length - 1]) ?  means1[0] :  means1[means1.Length - 1];
                cornerToCorner[0] = Math.Abs(means1[0] - means1[means1.Length - 1]);
                ppr[0] = cornerToCorner[0] / (sum / 4);
                pcr[0] = centerToCorner[0] / meansR[0];
                sum = 0;
                for (int i = 1; i < meansG.Length; i++)
                {
                    means1[i - 1] = meansG[i];
                    sum += meansG[i];
                }
                Array.Sort(means1);
                centerToCorner[1] = Math.Abs(meansG[0] - means1[0]) > Math.Abs(meansG[0] - means1[means1.Length - 1]) ? means1[0] : means1[means1.Length - 1];
                cornerToCorner[1] = Math.Abs(means1[0] - means1[means1.Length - 1]);
                ppr[1] = cornerToCorner[1] / (sum / 4);
                pcr[1] = centerToCorner[1] / meansG[0];
                sum = 0;
                for (int i = 1; i < meansG.Length; i++)
                {
                    means1[i - 1] = meansG[i];
                    sum += meansG[i];
                }
                Array.Sort(means1);
                centerToCorner[2] = Math.Abs(meansB[0] - means1[0]) > Math.Abs(meansB[0] - means1[means1.Length - 1]) ? means1[0] : means1[means1.Length - 1];
                cornerToCorner[2] = Math.Abs(means1[0] - means1[means1.Length - 1]);
                ppr[2] = cornerToCorner[2] / (sum / 4);
                pcr[2] = centerToCorner[2] / meansB[0];
                foreach (_ROI item in info.lsResult.aryLS_ROI)
                {
                    DrawRect1(win, item.y, item.x, item.width, item.height);
                }
                //HOperatorSet.GenRectangle2(out ho_rect, hv_Height / 2, hv_Width / 2, 0, info.lsResult.aryLS_ROI[0].x, info.lsResult.aryLS_ROI[0].y);
                //ho_ImageReduced.Dispose();
                //HOperatorSet.ReduceDomain(ho_ImageGray, ho_rect, out ho_ImageReduced);
                //HOperatorSet.GetImageSize(ho_ImageReduced, out hv_Width1, out hv_Height1);
                //crop_domain (ImageReduced, ImagePart)
                //get_image_size (ImagePart, Width2, Height2)
                //HOperatorSet.Intensity(ho_rect, ho_ImageReduced, out hv_Mean, out hv_Deviation);


                ho_Image.Dispose();
                ho_Image1.Dispose();
                ho_Image2.Dispose();
                ho_Image3.Dispose();
                
                ho_rect.Dispose();
                ho_ImageReduced.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public void SaveLensShadingTestData(string dataPath)
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
                info = $"SN,时间,灰阶四周比中心,灰阶四周差比四周均值,色图R四周比中心,色图R四周差比四周均值,色图G四周比中心,色图G四周差比四周均值,色图B四周比中心,色图B四周差比四周均值";
                using (StreamWriter sw = new StreamWriter(dataPath, true))
                {
                    sw.WriteLine(info);
                }
            }
            info = $"{PSN},{DateTime.Now.ToString("HH：mm：ss")},{this.greypcr},{this.greyppr},{this.pcr[0]},{this.ppr[0]},{this.pcr[1]},{this.ppr[1]},{this.pcr[2]},{this.ppr[2]}";
            using (StreamWriter sw = new StreamWriter(dataPath, true))
            {
                sw.WriteLine(info);
            }
        }
        
    }
}
