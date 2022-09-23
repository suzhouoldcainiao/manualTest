using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;
using NST_CameraImageTest;
using System.IO;
using System.Threading;
using ImageAlgorithm;

namespace TestDemo
{
    public class StationBase
    {
        public FormTest formTest;
        public HWindowControl hWindowControl;
        public TextBox textBox;
        public string imagedir ;
        public string imageName = "";
        public string PType;
        public string PSN;
        //OpenFileDialog fs = new OpenFileDialog();
        public Bitmap bmp;
        public Bitmap bmp1;
        public HWindow win = null;
        public HImage image = null;
        public int width;
        public int height;
        public string pictureSavePath;
        public string dataPath;
        public Ini NSTIni;
        //public HImage image = null;

        public StationBase()
        {

        }
        public StationBase(FormTest formtest,string stationName,TextBox textbox,string type,string sn,string imageDIR)
        {
            formTest = formtest;
            imageName = stationName;
            textBox = textbox;
            hWindowControl = formTest.hWindowControl1;
            PType = type;
            PSN = sn;
            formTest.Text = stationName+sn;
            formtest.StartPosition = FormStartPosition.Manual;
            formtest.Location = new Point(500, 150);
            formTest.Show();
            //fs.InitialDirectory = "D:\\TestImage";
            //imagedir = imagedir +"\\"+ stationName+".bmp";
            imagedir = imageDIR;
            win = hWindowControl.HalconWindow;
            image = new HImage();
            pictureSavePath = "D:\\TestImage\\" + stationName + "\\" + type + "\\" + sn+"\\";
            dataPath = "D:\\TestImage\\" + stationName + "\\" + type + "\\";
            NSTIni = new Ini("D:\\PruductionConfig" +"\\"+ type + "\\" + "NST_CameraImageTest.ini");
        }
        virtual public bool Test()
        {
            
            return false;
        }
        public void LoadImage()
        {
            //BitmapData bMD1=
            bmp = new Bitmap(Image.FromFile(imagedir));
            //bmp = new Bitmap(bmp1.Width, bmp1.Height, PixelFormat.Format24bppRgb);
            //bmp = bmp1;
            image.ReadImage(imagedir);
            image.GetImageSize(out width, out height);
            win.SetPart(0, 0, height - 1, width - 1);
            win.DispObj(image);
        }
        public void DrawCrossLine(HWindow win, int width, int height)
        {
            win.SetColor("white");
            win.DispLine(0, (double)(width - 1) / 2, height - 1, (double)(width - 1) / 2);
            win.DispLine((double)(height - 1) / 2, 0, (double)(height - 1) / 2, width - 1);
        }
        public void Bitmap2HObject1(Bitmap bmp, ref HObject image)
        {

            try
            {
                Bitmap bitmap = (Bitmap)bmp.Clone();
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                //BitmapData srcBmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                BitmapData srcBmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);
                //if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    //HOperatorSet.GenImageInterleaved(out image, srcBmpData.Scan0, "bgr", bmp.Width, bmp.Height, 0, "byte", 0, 0, 0, 0, -1, 0);
                    HOperatorSet.GenImageInterleaved(out image, srcBmpData.Scan0, "rgb", bmp.Width, bmp.Height, 0, "byte", 0, 0, 0, 0, -1, 0);
                }
                //else
                {
                    //HOperatorSet.GenImage1(out image, "byte", bmp.Width, bmp.Height, srcBmpData.Scan0);
                }
                bitmap.UnlockBits(srcBmpData);
                bitmap?.Dispose();

            }
            catch (Exception ex)
            {
                image?.Dispose();
            }
        }
        public void Bitmap2HObject2(Bitmap bmp, ref HObject image)
        {

            try
            {
                Bitmap bitmap = (Bitmap)bmp.Clone();
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                //BitmapData srcBmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                BitmapData srcBmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                //if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    HOperatorSet.GenImageInterleaved(out image, srcBmpData.Scan0, "bgr", bmp.Width, bmp.Height, 0, "byte", bmp.Width, bmp.Height, 0, 0, -1, 0);
                    //HOperatorSet.GenImageInterleaved(out image, srcBmpData.Scan0, "rgb", bmp.Width, bmp.Height, 0, "byte", 0, 0, 0, 0, -1, 0);
                }
                //else
                {
                    //HOperatorSet.GenImage1(out image, "byte", bmp.Width, bmp.Height, srcBmpData.Scan0);
                }
                //bitmap.UnlockBits(srcBmpData);
                //bitmap.Dispose();

            }
            catch (Exception ex)
            {
                image?.Dispose();
            }
        }
        public void BitmapToHobject3(Bitmap bitmap, out HObject image)
        {
            int height = bitmap.Height;//图像的高度
            int width = bitmap.Width;//图像的宽度


            Rectangle imgRect = new Rectangle(0, 0, width, height);
            BitmapData bitData = bitmap.LockBits(imgRect, ImageLockMode.ReadOnly, bitmap.PixelFormat);


            //由于Bitmap图像每行的字节数必须保持为4的倍数，因此在行的字节数不满足这个条件时，会对行进行补充，步幅数Stride表示的就是补充过后的每行的字节数，也成为扫描宽度
            int stride = bitData.Stride;
            switch (bitmap.PixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    {
                        unsafe
                        {
                            int count = height * width;
                            byte[] data = new byte[count];
                            byte* bptr = (byte*)bitData.Scan0;
                            fixed (byte* pData = data)
                            {
                                for (int i = 0; i < height; i++)
                                {
                                    for (int j = 0; j < width; j++)
                                    {
                                        /*
                                         *
                                         如果直接使用GenImage1，传入BitData的Scan0（图像首元素的指针）作为内存指针的话，如果图像不满足行为4的倍数，那么填充的部分也会参与进来，从而导致图像扭曲
                                         *
                                         *
                                         */


                                        //舍去填充的部分
                                        data[i * width + j] = bptr[i * stride + j];
                                    }

                                }
                                HOperatorSet.GenImage1(out image, "byte", width, height, new IntPtr(pData));
                            }
                        }

                    }
                    break;
                case PixelFormat.Format24bppRgb:
                    {
                        unsafe
                        {
                            int count = height * width * 3;//24位的BitMap每个像素三个字节
                            byte[] data = new byte[count];
                            byte* bptr = (byte*)bitData.Scan0;
                            fixed (byte* pData = data)
                            {
                                for (int i = 0; i < height; i++)
                                {
                                    for (int j = 0; j < width * 3; j++)
                                    {
                                        //每个通道的像素需一一对应
                                        data[i * width * 3 + j] = bptr[i * stride + j];
                                    }
                                }
                                HOperatorSet.GenImageInterleaved(out image, new IntPtr(pData), "bgr", bitmap.Width, bitmap.Height, 0, "byte", bitmap.Width, bitmap.Height, 0, 0, -1, 0);
                            }
                        }
                    }
                    break;
                default:
                    {
                        unsafe
                        {
                            int count = height * width;
                            byte[] data = new byte[count];
                            byte* bptr = (byte*)bitData.Scan0;
                            fixed (byte* pData = data)
                            {
                                for (int i = 0; i < height; i++)
                                {
                                    for (int j = 0; j < width; j++)
                                    {
                                        data[i * width + j] = bptr[i * stride + j];
                                    }

                                }
                                HOperatorSet.GenImage1(out image, "byte", width, height, new IntPtr(pData));
                            }
                        }

                    }
                    break;
            }
            bitmap.UnlockBits(bitData);
        }
    
        public void DrawRectAndCross(HWindow win, double column, double row, double length1, double length2)
        {
            double phi = 0;
            //HTuple color = new HTuple({ Color.Red.R, Color.Red.G, Color.Red.B });
            win.SetDraw("margin");
            
            win.SetColor("yellow");
            //win.DispRectangle2(row, column, phi, length1, length2);
            win.DispRectangle2(bmp.Height/2, bmp.Width / 2, phi, length1, length2);
            win.SetColor("green");
            win.DispLine(row, column - length1 / 2, row, column + length1 / 2);
            win.DispLine(row - length2 / 2, column, row + length2 / 2, column);

        }
        public void DrawRect(HWindow win, double row1, double column1, double row2, double column2)
        {

            win.SetDraw("margin");
            win.SetColor("green");
            win.DispRectangle1(row1, column1, row2, column2);
        }
        public void DrawRect1(HWindow win, double row1, double column1, double lenght1, double lenght2)
        {

            win.SetDraw("margin");
            win.SetColor("red");
            win.DispRectangle2(row1, column1, 0, lenght1, lenght2);
        }
        public void info2Log(string strTemp)//日志记录
        {
            try
            {
                textBox.Invoke((MethodInvoker)delegate
                {
                    textBox.AppendText("[" + DateTime.Now.ToLongTimeString() + ":] " + strTemp + "\r\n");
                });
            }
            catch (Exception)
            {

            }
        }
        public static bool Init(string path)
        {
            try
            {

                bool isOK = false;
                isOK = CameraImageTest.SetParam(path);//@@@
                //isOK = ActiveAlignment.SetParam(path);
                return isOK ? true : false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public byte[] ConvertBitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bitmapData = null;
            byte[] result;
            try
            {
                //bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);//@@@
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);//@@@
                int num = bitmapData.Stride * bitmap.Height;
                byte[] array = new byte[num];
                System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, array, 0, num);
                if (bitmapData.Stride == bitmap.Width * 3)
                {
                    result = array;
                }
                else if (bitmapData.Stride == bitmap.Width)
                {
                    result = array;
                }
                else
                {
                    byte[] array2 = new byte[bitmap.Width * 3 * bitmap.Height];
                    for (int i = 0; i < bitmapData.Height; i++)
                    {
                        Buffer.BlockCopy(array, i * bitmapData.Stride, array2, i * bitmap.Width * 3, bitmap.Width * 3);
                    }
                    result = array2;
                }
            }
            finally
            {
                if (bitmapData != null)
                {
                    bitmap.UnlockBits(bitmapData);
                }
            }
            return result;
        }
        public void SavePicture(string picPath, HTuple hwindow)
        {
            try
            {


                HObject img = null;
                HOperatorSet.GenEmptyObj(out img);
                img.Dispose();
                
                HOperatorSet.DumpWindowImage(out img, hwindow);
                if (!Directory.Exists(picPath))
                {
                    Directory.CreateDirectory(picPath);
                }
                picPath+= DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-fff") + ".bmp"; 
                HOperatorSet.WriteImage(img, "bmp", 0, picPath);
                img.Dispose();
                

            }
            catch (HalconException ex)
            {

            }
        }
        public void DrawPicCircle(HTuple wHandle, List<_LABEL> blP)
        {

            HObject ho_Rectangle;
            HOperatorSet.SetDraw(wHandle, "margin");
            HOperatorSet.SetColor(wHandle, "blue");
            HOperatorSet.SetLineWidth(wHandle, 1);
            for (int i = 0; i < blP.Count; i++)
            {
                HOperatorSet.SetColor(wHandle, "red");
                HObject hCircle = null;
                HOperatorSet.GenCircle(out hCircle, blP[i].nY, blP[i].nX, blP[i].dRadius);
                HOperatorSet.DispObj(hCircle, wHandle);
            }
        }

    }
}
