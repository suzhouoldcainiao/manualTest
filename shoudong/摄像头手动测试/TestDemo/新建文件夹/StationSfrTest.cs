using HalconDotNet;
using ImageAlgorithm;
using NST_CameraImageTest;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestDemo
{
    class StationSfrTest:StationBase
    {
        public static SFRInfo ThisSFRInfo = new ImageAlgorithm.SFRInfo();
        public SINGLECHART thisSigleChart = new SINGLECHART();
        public static SFRInfo[] SFRInfo = new ImageAlgorithm.SFRInfo[5] { new SFRInfo(), new SFRInfo(), new SFRInfo(), new SFRInfo(), new SFRInfo() };
        double SFRCenter_Gain = 1;
        double SFRCorner_Gain = 1;
        public double SFRCenter;
        public double SFR_LU;
        public double SFR_RU;
        public double SFR_LD;
        public double SFR_RD;

        public StationSfrTest(FormTest formtest, string stationName, TextBox textbox, string type, string sn, string imageDIR) : base(formtest, stationName, textbox, type, sn, imageDIR)
        {
            //InitLib();
        }
        public override bool Test()
        {
            LoadImage();
            SavePicture(pictureSavePath, win);
            if (GetFinalTestSFRValue(bmp, ref SFRInfo[0], win, false))//以后可以加入SFR数值判断
            {
                SFRCenter = ((ThisSFRInfo.SFR[0][0] + ThisSFRInfo.SFR[0][1]) / 2) * SFRCenter_Gain;
                SFR_LU = ((ThisSFRInfo.SFR[1][0] + ThisSFRInfo.SFR[1][1]) / 2) * SFRCorner_Gain;
                SFR_RU = ((ThisSFRInfo.SFR[2][0] + ThisSFRInfo.SFR[2][1]) / 2) * SFRCorner_Gain;
                SFR_LD = ((ThisSFRInfo.SFR[3][0] + ThisSFRInfo.SFR[3][1]) / 2) * SFRCorner_Gain;
                SFR_RD = ((ThisSFRInfo.SFR[4][0] + ThisSFRInfo.SFR[4][1]) / 2) * SFRCorner_Gain;

                HOperatorSet.SetColor(win, "green");

                HOperatorSet.SetFont(win, "-Courier New-18-*-*-*-*-1-");
                HOperatorSet.SetTposition(win, ThisSFRInfo.SearchROI[0].y, ThisSFRInfo.SearchROI[0].x);
                HOperatorSet.WriteString(win, SFRCenter.ToString("0.00"));
                HOperatorSet.SetTposition(win, ThisSFRInfo.SearchROI[1].y, ThisSFRInfo.SearchROI[1].x);
                HOperatorSet.WriteString(win, SFR_LU.ToString("0.00"));
                HOperatorSet.SetTposition(win, ThisSFRInfo.SearchROI[2].y, ThisSFRInfo.SearchROI[2].x);
                HOperatorSet.WriteString(win, SFR_RU.ToString("0.00"));
                HOperatorSet.SetTposition(win, ThisSFRInfo.SearchROI[3].y, ThisSFRInfo.SearchROI[3].x);
                HOperatorSet.WriteString(win, SFR_LD.ToString("0.00"));
                HOperatorSet.SetTposition(win, ThisSFRInfo.SearchROI[4].y, ThisSFRInfo.SearchROI[4].x);
                HOperatorSet.WriteString(win, SFR_RD.ToString("0.00"));
                SavePicture(pictureSavePath, win);
                return true;
            }
            else
            {
                return false;
            }
        }
        public void TestRuntime()
        {

        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SINGLECHART
        {
            //INPUT
            public RECT SearchRT;                           //定位框
            //Rectangle searchRT;
            public double m_dFreq;                             //空间频率
            public int m_nBinaryThrehoid;                      //二值化
            public int m_nRoiDistance;                         //Roi相对于中心的距离
            public int m_nPatternSize;                         //框体识别大小
            public int m_nRoiW;                                //Roi宽
            public int m_nRoiH;                                //Roi高
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public int[] m_nRoiEnable;                    //四个Roi的开关

            public bool m_bUseMTF;                             //0:计算sfr,1:计算MTF
            public int m_nMtfPer;                              //计算MTF时的 MTF的基准

            //OUTPUT
            public double m_dOCx;                                  //宝马chart的中心点X坐标
            public double m_dOCy;                                  //宝马chart的中心点Y坐标
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public double[] m_dSfrValue;                      //宝马chart的四周Roi计算出的分数 0-3 对应下、右。上、左
            public double m_dSfrAvgValue;                      //宝马chart的计算出的总分数平均值
            public int m_nLightValue;                          //宝马chart的白色区域的亮度值
        }
        [DllImport("OfilmEolTest.dll", EntryPoint = "InitLib")]
        static extern void InitLib();
        [DllImport("OfilmEolTest.dll", EntryPoint = "SfrTest")]
        static unsafe extern int SfrTest(byte* BGRImage, int nimgW, int nimgH, ref  SINGLECHART sSigleChart);
        public unsafe bool GetFinalTestSFRValue2(Bitmap bitmap, ref SINGLECHART sSigleChart, HTuple wHandle, bool offset)
        {
            try
            {
                int  b;

                byte[] byBuffer = ConvertBitmapToByteArray(bitmap);
                fixed (byte* pbuffer = byBuffer)
                {
                    //CameraImageTest.SetParam("D:\\NST_CameraImageTest.ini");
                    b = SfrTest(pbuffer, bitmap.Width, bitmap.Height, ref sSigleChart);
                    info2Log($"新算法返回值为{b}");
                    //b = ActiveAlignment.GetSFRValue_Collimators(pbuffer, bitmap.Width, bitmap.Height, ref sFR_cinfo, 0.25);
                   
                }

                //if (offset)
                //{
                //    //ThisSFRInfo = sFRInfo;
                //    DrawPicRectangle1(wHandle, "SFR");
                //}
                //else
                //{
                //    //ThisSFRInfo = sFRInfo;
                //    DrawPicRectangle(wHandle, "SFR");
                //}
                return  true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public bool Test1()
        {
            LoadImage();
            RECT rect1 = new RECT();
            rect1.Left = 1225;
            rect1.Top = 380;
            rect1.Right = 2990;
            rect1.Bottom = 1810;
            thisSigleChart.SearchRT = rect1;
            thisSigleChart.m_dFreq = 0.25;
            thisSigleChart.m_nBinaryThrehoid = 60;
            thisSigleChart.m_nRoiDistance = 0;
            thisSigleChart.m_nPatternSize = 20;
            thisSigleChart.m_nRoiW = 0;
            thisSigleChart.m_nRoiH = 0;
            thisSigleChart.m_nRoiEnable = new int[] { 0, 0, 1, 1 };
            thisSigleChart.m_bUseMTF = false;
            thisSigleChart.m_nMtfPer = 0;
            if (GetFinalTestSFRValue2(bmp, ref thisSigleChart, win, false))//以后可以加入SFR数值判断
            {
                //SFRCenter = ((ThisSFRInfo.SFR[0][0] + ThisSFRInfo.SFR[0][1]) / 2) * SFRCenter_Gain;
                //SFR_LU = ((ThisSFRInfo.SFR[1][0] + ThisSFRInfo.SFR[1][1]) / 2) * SFRCorner_Gain;
                //SFR_RU = ((ThisSFRInfo.SFR[2][0] + ThisSFRInfo.SFR[2][1]) / 2) * SFRCorner_Gain;
                //SFR_LD = ((ThisSFRInfo.SFR[3][0] + ThisSFRInfo.SFR[3][1]) / 2) * SFRCorner_Gain;
                //SFR_RD = ((ThisSFRInfo.SFR[4][0] + ThisSFRInfo.SFR[4][1]) / 2) * SFRCorner_Gain;

                HOperatorSet.SetColor(win, "green");
                DrawRect(win, rect1.Top, rect1.Left, rect1.Bottom, rect1.Right);
                HOperatorSet.SetFont(win, "-Courier New-18-*-*-*-*-1-");
                //HOperatorSet.SetTposition(win, ThisSFRInfo.SearchROI[0].y, ThisSFRInfo.SearchROI[0].x);
                //HOperatorSet.WriteString(win, SFRCenter.ToString("0.00"));
                //HOperatorSet.SetTposition(win, ThisSFRInfo.SearchROI[1].y, ThisSFRInfo.SearchROI[1].x);
                //HOperatorSet.WriteString(win, SFR_LU.ToString("0.00"));
                //HOperatorSet.SetTposition(win, ThisSFRInfo.SearchROI[2].y, ThisSFRInfo.SearchROI[2].x);
                //HOperatorSet.WriteString(win, SFR_RU.ToString("0.00"));
                //HOperatorSet.SetTposition(win, ThisSFRInfo.SearchROI[3].y, ThisSFRInfo.SearchROI[3].x);
                //HOperatorSet.WriteString(win, SFR_LD.ToString("0.00"));
                //HOperatorSet.SetTposition(win, ThisSFRInfo.SearchROI[4].y, ThisSFRInfo.SearchROI[4].x);
                //HOperatorSet.WriteString(win, SFR_RD.ToString("0.00"));
                return true;
            }
            else
            {
                return false;
            }
        }

        [DllImport("kernel32.dll")]
        public static extern void CopyMemory(int Destination, int add, int Length);
        public Bitmap HObjectToBitmap(HObject ho_Image)
        {
            try
            {
                HOperatorSet.GetImagePointer1(ho_Image, out HTuple pointer, out HTuple type, out HTuple width, out HTuple height);
                Bitmap bmpImage = new Bitmap(width.I, height.I, PixelFormat.Format8bppIndexed);
                ColorPalette pal = bmpImage.Palette;
                for (int i = 0; i < 256; i++)
                {
                    pal.Entries[i] = Color.FromArgb(255, i, i, i);
                }
                bmpImage.Palette = pal;
                BitmapData bitmapData = bmpImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                int pixelSize = Bitmap.GetPixelFormatSize(bitmapData.PixelFormat) / 8;
                int stride = bitmapData.Stride;
                int ptr = bitmapData.Scan0.ToInt32();
                if (width % 4 == 0)
                    CopyMemory(ptr, pointer, width * height * pixelSize);
                else
                {
                    for (int i = 0; i < height; i++)
                    {
                        CopyMemory(ptr, pointer, width * pixelSize);
                        pointer += width;
                        ptr += bitmapData.Stride;
                    }
                }
                bmpImage.UnlockBits(bitmapData);
                return bmpImage;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public  unsafe bool GetFinalTestSFRValue1(Bitmap bitmap1, ref SFRInfo sFRInfo, HTuple wHandle, bool offset)
        {
            try
            {
                bool b;
                HObject hbmp = null;
                Bitmap2HObject1(bitmap1, ref hbmp);
                HObject bmpG = null;
                HOperatorSet.Rgb1ToGray(hbmp, out bmpG);
                Bitmap bitmap = HObjectToBitmap(bmpG);
                //bitmap.Save(Path + "\\T" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + ".bmp");
                byte[] byBuffer = ConvertBitmapToByteArray(bitmap);
                fixed (byte* pbuffer = byBuffer)
                {                  
                    b = CameraImageTest.GetSFRValue(pbuffer, bitmap.Width, bitmap.Height, ref sFRInfo);
                }

                if (offset)
                {
                    ThisSFRInfo = sFRInfo;
                    DrawPicRectangle1(wHandle, "SFR");
                }
                else
                {
                    ThisSFRInfo = sFRInfo;
                    DrawPicRectangle(wHandle, "SFR");
                }
                return b;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public unsafe bool GetFinalTestSFRValue(Bitmap bitmap, ref SFRInfo sFRInfo, HTuple wHandle, bool offset)
        {
            try
            {
                bool b;
                
                byte[] byBuffer = ConvertBitmapToByteArray(bitmap);
                fixed (byte* pbuffer = byBuffer)
                {
                    b = CameraImageTest.GetSFRValue(pbuffer, bitmap.Width, bitmap.Height, ref sFRInfo);
                }
                if (offset)
                {
                    ThisSFRInfo = sFRInfo;
                    DrawPicRectangle1(wHandle, "SFR");
                }
                else
                {
                    ThisSFRInfo = sFRInfo;
                    DrawPicRectangle(wHandle, "SFR");
                }
                return b;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        //public  byte[] ConvertBitmapToByteArray(Bitmap bitmap)
        //{
        //    BitmapData bitmapData = null;
        //    byte[] result;
        //    try
        //    {
        //        //bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);//@@@
        //        bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);//@@@
        //        int num = bitmapData.Stride * bitmap.Height;
        //        byte[] array = new byte[num];
        //        System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, array, 0, num);
        //        if (bitmapData.Stride == bitmap.Width * 3)
        //        {
        //            result = array;
        //        }
        //        else if (bitmapData.Stride == bitmap.Width)
        //        {
        //            result = array;
        //        }
        //        else
        //        {
        //            byte[] array2 = new byte[bitmap.Width * 3 * bitmap.Height];
        //            for (int i = 0; i < bitmapData.Height; i++)
        //            {
        //                Buffer.BlockCopy(array, i * bitmapData.Stride, array2, i * bitmap.Width * 3, bitmap.Width * 3);
        //            }
        //            result = array2;
        //        }
        //    }
        //    finally
        //    {
        //        if (bitmapData != null)
        //        {
        //            bitmap.UnlockBits(bitmapData);
        //        }
        //    }
        //    return result;
        //}


        public static void DrawPicRectangle1(HTuple wHandle, string TestItem)
        {

            HObject ho_Rectangle;
            HOperatorSet.SetDraw(wHandle, "margin");
            HOperatorSet.SetColor(wHandle, "blue");
            HOperatorSet.SetLineWidth(wHandle, 1);
            switch (TestItem)
            {
                case "SFR":
                    for (int i = 0; i < 9; i++)
                    {
                        HOperatorSet.SetColor(wHandle, "yellow");
                        HOperatorSet.GenRectangle2(out ho_Rectangle, ThisSFRInfo.SearchROI[i].y + ThisSFRInfo.SearchROI[i].height / 2, ThisSFRInfo.SearchROI[i].x + ThisSFRInfo.SearchROI[i].width / 2, 0, ThisSFRInfo.SearchROI[i].width / 2, ThisSFRInfo.SearchROI[i].height / 2);
                        HOperatorSet.DispObj(ho_Rectangle, wHandle);

                        HOperatorSet.SetColor(wHandle, "red");
                        HOperatorSet.SetLineWidth(wHandle, 1);
                        for (int j = 0; j < 5; j++)
                        {
                            HOperatorSet.GenRectangle2(out ho_Rectangle, ThisSFRInfo.ObjectROI[i][j].y + ThisSFRInfo.ObjectROI[i][j].height / 2, ThisSFRInfo.ObjectROI[i][j].x + ThisSFRInfo.ObjectROI[i][j].width / 2, 0, ThisSFRInfo.ObjectROI[i][j].width / 2, ThisSFRInfo.ObjectROI[i][j].height / 2);
                            HOperatorSet.DispObj(ho_Rectangle, wHandle);
                        }


                    }
                    break;
                default:
                    break;
            }
        }
        public  void DrawPicRectangle(HTuple wHandle, string TestItem)
        {

            HObject ho_Rectangle;
            HOperatorSet.SetDraw(wHandle, "margin");
            HOperatorSet.SetColor(wHandle, "blue");
            HOperatorSet.SetLineWidth(wHandle, 1);
            switch (TestItem)
            {
                case "SFR":
                    for (int i = 0; i < 9; i++)
                    {
                        HOperatorSet.SetColor(wHandle, "yellow");
                        HOperatorSet.GenRectangle2(out ho_Rectangle, ThisSFRInfo.SearchROI[i].y + ThisSFRInfo.SearchROI[i].height / 2, ThisSFRInfo.SearchROI[i].x + ThisSFRInfo.SearchROI[i].width / 2, 0, ThisSFRInfo.SearchROI[i].width / 2, ThisSFRInfo.SearchROI[i].height / 2);
                        HOperatorSet.DispObj(ho_Rectangle, wHandle);

                        HOperatorSet.SetColor(wHandle, "red");
                        HOperatorSet.SetLineWidth(wHandle, 1);
                        for (int j = 0; j < 5; j++)
                        {
                            HOperatorSet.GenRectangle2(out ho_Rectangle, ThisSFRInfo.ObjectROI[i][j].y + ThisSFRInfo.ObjectROI[i][j].height / 2, ThisSFRInfo.ObjectROI[i][j].x + ThisSFRInfo.ObjectROI[i][j].width / 2, 0, ThisSFRInfo.ObjectROI[i][j].width / 2, ThisSFRInfo.ObjectROI[i][j].height / 2);
                            HOperatorSet.DispObj(ho_Rectangle, wHandle);
                        }


                    }


                    break;
                default:
                    break;
            }


        }
    }
    
}
