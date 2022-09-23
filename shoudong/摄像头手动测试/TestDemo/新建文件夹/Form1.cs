using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RLGCSHisFX3Platform;
using HalconDotNet;
using NST_CameraImageTest;
using System.IO;
using RlgCSSDKDemo;
using System.Text.RegularExpressions;
using System.Threading;
using System.Drawing.Imaging;
using ImageAlgorithm;

namespace TestDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            comboBox_Type.Items.Clear();
            CameraConfigList = GetCamreConfig(@".\SensorLightCfg", "*.ini");
            foreach (var item in CameraConfigList)
            {
                comboBox_Type.Items.Add(item.Substring(0, item.LastIndexOf(".")));
            }
            if (comboBox_Type.Items.Count != 0)
            {
                comboBox_Type.SelectedIndex = 0;
            }
            DirectoryInfo dirType = new DirectoryInfo(@"D:\PruductionConfig");
            foreach (DirectoryInfo item in dirType.GetDirectories())
            {
                proTypeList.Add(item.ToString());
            }
            foreach (string item in proTypeList)
            {
                comboBoxProType.Items.Add(item);
            }
            if (comboBoxProType.Items.Count != 0)
            {
                comboBoxProType.SelectedIndex = 0;
            }
            DirectoryInfo bmpType = new DirectoryInfo(@"D:\TestImage");
            foreach (FileInfo item in bmpType.GetFiles("*.bmp")) 
            {
                //stationBmp.Add("D:\\TestImage\\" + item.ToString());
                stationBmp.Add(item.ToString());
            }
            foreach (string item in stationBmp)
            {
                comboBoxStation.Items.Add(item.Split('.')[0]);
            }
            if (comboBoxProType.Items.Count != 0)
            {
                comboBoxStation.SelectedIndex = 0;
            }

        }
        //Dictionary<string, Bitmap> stationBmp = new Dictionary<string, Bitmap>();
        List<string> stationBmp = new List<string>();
        List<string> proTypeList = new List<string>();
        List<RlgPlatInterface> listRlgPlatform = new List<RlgPlatInterface>();

        List<bool> listIsStart = new List<bool>();

        List<bool> listIsGrab = new List<bool>();

        List<_CSRect> mtfRect = new List<_CSRect>();

        const string dirPath = @".\MTFROIConfig.txt";

        bool loadImage = false;



        Bitmap bitmapcache = null;

        List<string> CameraConfigList = new List<string>();

        public string imagedir = "D:\\TestImage";

        Label[] labelColor;
        Label[] labelGrey;
        private void enumDevice_Click(object sender, EventArgs e)
        {
            string[] listDevice = new string[4];
            int nBoxCnt = 0;
            try
            {
                RlgPlatInterface.GetInstance().HisFX3EnumDev(ref listDevice, ref nBoxCnt);//查找软龙格设备

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            comboBoxBoxSerial.Items.Clear();
            for (int i = 0; i < nBoxCnt; i++)
            {
                comboBoxBoxSerial.Items.Add(listDevice[i]);
                RlgCommFuction.SetString("Device" + i.ToString(), "BoxSerialNum", listDevice[i]);//修改INI配置里面的序列号
            }
            if (comboBoxBoxSerial.Items.Count == 0)
            {
                comboBoxBoxSerial.SelectedIndex = -1;
                info2Log("未检查到任何点亮盒子");
            }
            else
            {
                comboBoxBoxSerial.SelectedIndex = 0;
            }
        }
        public string iniPath = "D:\\PruductionConfig\\NST_CameraImageTest.ini";
        public void info2Log(string strTemp)//日志记录
        {
            try
            {
                textBox1.Invoke((MethodInvoker)delegate
                {
                    textBox1.AppendText("[" + DateTime.Now.ToLongTimeString() + ":] " + strTemp + "\r\n");
                });
            }
            catch (Exception)
            {

            }
        }
        private List<string> GetCamreConfig(string path, string extension)

        {
            DirectoryInfo directoryinfo = new DirectoryInfo(path);
            List<string> list = new List<string>();
            foreach (var item in directoryinfo.GetFiles(extension))
            {
                list.Add(item.Name);
            }
            return list;
        }


        HObject hImage;

        private void opendevice_Click(object sender, EventArgs e)
        {
            listRlgPlatform.Clear();
            listIsStart.Clear();
            listIsGrab.Clear();
            channelNum.Items.Clear();
            RlgCommFuction.SetString("LoadingPath", "CameraCH1", '"' + @"./SensorLightCfg" + "/" + CameraConfigList[comboBox_Type.SelectedIndex].ToString() + '"');//修改INI配置文件
            string pType = RlgCommFuction.GetString("DeviceInfo", "PlatformType", "R2C");//读取配置文件
            setdeviceType(pType);
            int nCamCnt = GetPlatTypeCnt(pType);
            string strVal = RlgCommFuction.GetString("DeviceInfo", "Count", "1");
            int cnt = int.Parse(strVal);
            //int cnt = Convert.ToInt32("1");
            int nRst1 = 0;
            for (int box = 0; box < cnt; box++)
            {
                string key = string.Format("Device{0}", box);
                strVal = RlgCommFuction.GetString(key, "BoxSerialNum", "");
                nRst1 = RlgPlatInterface.GetInstance().HisFX3OpenDevice(box, strVal);
                if (nRst1 != 0)
                {
                    info2Log(string.Format("index{0}, opendevice fail", box));
                }
                else
                {
                    info2Log(string.Format("index{0}, opendevice success", box));
                }
                for (int n = 0; n < nCamCnt; n++)
                {
                    channelNum.Items.Add(string.Format("CH{0}", box * nCamCnt + n));
                    listRlgPlatform.Add(new RlgPlatInterface(box * nCamCnt + n));
                    listIsStart.Add(false);
                    listIsGrab.Add(false);
                }
            }
            if (channelNum.Items.Count == 0)
            {
                channelNum.SelectedIndex = -1;
                info2Log("未正确连接到点亮盒子");
            }
            else
            {
                channelNum.SelectedIndex = 0;
            }
        }
        private void setdeviceType(string pType)
        {

            string dType = RlgCommFuction.GetString("DeviceInfo", "DeserializerType", "TI954");
            _CSHisFX3_Platform_Type boxType = devStrToType(pType);
            _CSHisFX3_Deserializer deserializerType = DeserializerType2Enum(dType);

            if (pType == "VC8D" || pType == "VM16F_8" || pType == "VM16F_16")
            {
                RlgPlatInterface.GetInstance().HisFX3SetCurrentPlatformType(boxType);
                RlgPlatInterface.GetInstance().HisFX3SetDeserializerType(deserializerType);
                if (pType == "VM16F_8")
                {
                    RlgPlatInterface.GetInstance().HisFX3SetCamMode16OnOff_VM16F(false);
                }
                else if (pType == "VM16F_16")
                {
                    RlgPlatInterface.GetInstance().HisFX3SetCamMode16OnOff_VM16F(true);
                }
            }
        }
        int GetPlatTypeCnt(string strType)
        {
            if (strType == "R2C") return 1;
            else if (strType == "VC8D" || strType == "VM16F_8") return 8;
            else if (strType == "VM16F_16") return 16;
            return 1;
        }
        _CSHisFX3_Platform_Type devStrToType(string strType)
        {
            if (strType == "R2C") return _CSHisFX3_Platform_Type._HisFX3_Platform_Type_R2C;
            else if (strType == "VC8D") return _CSHisFX3_Platform_Type._HisFX3_Platform_Type_VC8D;
            else if (strType == "VM16F_8" || strType == "VM16F_16")
                return _CSHisFX3_Platform_Type._HisFX3_Platform_Type_VM16F;
            return _CSHisFX3_Platform_Type._HisFX3_Platform_Type_UnKnowed;
        }
        _CSHisFX3_Deserializer DeserializerType2Enum(string s)
        {
            if (s == "TI954") return _CSHisFX3_Deserializer._HisFX3_Deserializer_TI954;
            else if (s == "TI972") return _CSHisFX3_Deserializer._HisFX3_Deserializer_TI972;
            else if (s == "MAXIM9296") return _CSHisFX3_Deserializer._HisFX3_Deserializer_MAXIM9296;
            else if (s == "MAXIM96706") return _CSHisFX3_Deserializer._HisFX3_Deserializer_MAXIM96706;
            else if (s == "MAXIM9280") return _CSHisFX3_Deserializer._HisFX3_Deserializer_MAXIM9280;
            else if (s == "MAXIM96716") return _CSHisFX3_Deserializer._HisFX3_Deserializer_MAXIM96716;

            return _CSHisFX3_Deserializer._HisFX3_Deserializer_UNKNOWN;
        }

        private void startpreview_Click(object sender, EventArgs e)
        {
            if (listIsStart.Count == 0) return;
            int cam = channelNum.SelectedIndex;
            bool bStartOk = listRlgPlatform[cam].SensorStartPreview();
            //imageViewer.Roi.Clear();
            if (!bStartOk)
            {
                info2Log("startPreview fail, ");
                return;
            }
            info2Log("startPreview ok, ");
            //roiColorPicker.Enabled = false;
            //OCColorPicker.Enabled = false;
            //OCVisibleCheckBox.Enabled = false;
            saveImage.Enabled = false;
            threadGrabImage(cam);
        }
        public void DrawCrossLine(HWindow win, int width, int height)
        {
            win.SetColor("white");
            win.DispLine(0, (double)(width - 1) / 2, height - 1, (double)(width - 1) / 2);
            win.DispLine((double)(height - 1) / 2, 0, (double)(height - 1) / 2, width - 1);
        }
        private void threadGrabImage(int cam)//抓取图像
        {

            //根据选择的配置文件选择需要运行的程序
            string picType = comboBox_Type.SelectedItem.ToString();
            bool isYUV = Regex.IsMatch(picType, "_YUV", RegexOptions.IgnoreCase);
            //int num = comboBox_Type.SelectedIndex;
            bool GrabOK;
            if (listIsStart[cam])//检查是否已经开始出图
                return;
            InitPathF("NST_CameraImageTest.ini");
            listIsStart[cam] = true;
            listIsGrab[cam] = true;
            bool firstpreview = true;
            if (isYUV)

                listRlgPlatform[cam].GetImageInfo(GrabDataFormat.YUVFormat);//获取图像的信息
            else
                listRlgPlatform[cam].GetImageInfo(GrabDataFormat.RGBFormat);//获取图像的信息-------------------------


            Thread thread = new Thread(new ThreadStart(new Action(() =>
            {
                while (listIsStart[cam])
                {
                    try
                    {
                        IntPtr pBuf = new IntPtr();
                        if (isYUV)
                            GrabOK = listRlgPlatform[cam].GrabFrame(ref pBuf, GrabDataFormat.YUVFormat);
                        else
                            GrabOK = listRlgPlatform[cam].GrabFrame(ref pBuf, GrabDataFormat.RGBFormat);
                        //if (listRlgPlatform[cam].GrabFrame(ref pBuf, GrabDataFormat.RGBFormat))//检测抓取图像是否成功--------------
                        if (GrabOK)//检测抓取图像是否成功
                        {
                            int nwidth = (int)listRlgPlatform[cam].width;
                            int nheight = (int)listRlgPlatform[cam].height;
                            Bitmap bmp;
                            //if (isYUV)//是否选择的RGB格式
                            //    //bmp = YUVTOBITMAP_RGB(pBuf, nwidth, nheight);

                            //else
                            bmp = RGBTOBITMAP(pBuf, nwidth, nheight);////////-----------------

                            bitmapcache = bmp;
                            if (firstpreview)
                            {
                                //hWindowControl1.Size = new Size { Width = bmp.Width, Height = bmp.Height };
                                //imageViewer.Image.SetSize(bmp.Width, bmp.Height);//修改图像大小
                                //hWindowControl1.HalconWindow.GetType()
                                //imageViewer.Image.Type = ImageType.Rgb32;
                                firstpreview = false;
                            }
                            //if (isYUV)
                            //    //imageViewer.Image.ArrayToImage(YUVTORGB(pBuf, nwidth, nheight));
                            //else
                            //    //imageViewer.Image.ArrayToImage(bmpToRGBs(bmp));//将图像放入控件中---------------
                            Bitmap2HObject1(bmp, ref hImage);
                            hWindowControl1.HalconWindow.DispObj(hImage);
                            HOperatorSet.SetPart(hWindowControl1.HalconWindow, 0, 0, bmp.Height - 1, bmp.Width - 1);
                            DrawCrossLine(hWindowControl1.HalconWindow, bmp.Width, bmp.Height);
                            uint a1 = 0;
                            uint a2 = 0;
                            RlgPlatInterface.GetInstance().HisFX3GetTotalFrame(ref a1, ref a2, 1, cam);
                            label1.Text = string.Format("总的正确帧数是{0}FPS,总的错误帧数是{1}FPS", a1, a2);
                            SfrTestRun(bmp, hWindowControl1.HalconWindow);
                            //imageViewer.Image.Overlays.Default.Clear();//清除控件中的覆盖线
                            //                                           // showROIRect(mtfRect, imageViewer, roiColorPicker.Value);//显示ROI
                            //if (OCVisibleCheckBox.Checked)
                            //{
                            //    OC_LineOverlays(imageViewer, OCColorPicker.Value);
                            //    //PointContour pointContour = Image_Processing.FindOC(imageViewer.Image);
                            //    //Label_OCInfo.Text = $"Sensor_OC.X:{bmp.Width / 2}\r\nSensor_OC.Y:{bmp.Height / 2}\r\nChart_OC.X:{pointContour.X}\r\nChart_OC.Y:{pointContour.Y}\r\nOffset.X:{pointContour.X - bmp.Width / 2}\r\nOffset.Y:{pointContour.Y - bmp.Height / 2}";
                            //}
                            //roi_TextOverlays(mtfRect, imageViewer, roiColorPicker.Value);
                            //showMTF(mtfRect, imageViewer, bitmapcache, roiColorPicker.Value);
                            if (!loadImage)
                            {
                                loadImage = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        info2Log(ex.Message);
                    }
                    Thread.Sleep(100);
                }
                listIsGrab[cam] = false;
                listIsStart[cam] = false;
            })));
            thread.Start();
        }

        double SFRCenter = 0;
        double SFR_LU = 0;
        double SFR_RU = 0;
        double SFR_LD = 0;
        double SFR_RD = 0;
        double SFRCenter_Gain = 1;
        double SFRCorner_Gain = 1;
        SFRInfo ThisSFRInfo = new SFRInfo();
        public void SfrTestRun(Bitmap bitmap, HTuple win)
        {


            

            if (GetFinalTestSFRValue(bitmap, ref ThisSFRInfo, hWindowControl1.HalconWindow))
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
                //return true;
            }
            else
            {
                //return false;
            }
        }
        public unsafe bool GetFinalTestSFRValue(Bitmap bitmap, ref SFRInfo sFRInfo, HTuple wHandle)
        {
            try
            {
                bool b;

                byte[] byBuffer = ConvertBitmapToByteArray(bitmap);
                fixed (byte* pbuffer = byBuffer)
                {
                    //CameraImageTest.SetParam("D:\\NST_CameraImageTest.ini");
                    b = CameraImageTest.GetSFRValue(pbuffer, bitmap.Width, bitmap.Height, ref sFRInfo);
                    //b = ActiveAlignment.GetSFRValue_Collimators(pbuffer, bitmap.Width, bitmap.Height, ref sFR_cinfo, 0.25);
                   
                }

                //if (offset)
                //{
                //    //ThisSFRInfo = sFRInfo;
                //    StationSfrTest.DrawPicRectangle1(wHandle, "SFR");
                //}
                //else
                //{
                    //ThisSFRInfo = sFRInfo;
                    DrawPicRectangle(wHandle, "SFR");
                //}
                return b;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public void DrawPicRectangle(HTuple wHandle, string TestItem)
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
        public Bitmap RGBTOBITMAP(IntPtr srcImage, int width/*图像的宽度*/, int height/*图像的高度*/)
        {
            //申请目标位图的变量，并将其内存区域锁定
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            //获得图像的参数
            int stride = bmpData.Stride; //扫描线的宽度
            IntPtr iptr = bmpData.Scan0; //获得 bmpData的内存起始位置
            int scanBytes = stride * height; //用Stride宽度,表示内存区域的大小
            RlgCommFuction.CopyMemory(iptr, srcImage, (uint)scanBytes);
            bmp.UnlockBits(bmpData); //解锁内存区域
            return bmp;
        }
        struct hwsize
        {
            public int width;
            public int height;
        }
        public void Bitmap2HObject1(Bitmap bmp, ref HObject image)
        {

            try
            {
                Bitmap bitmap = (Bitmap)bmp.Clone();
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData srcBmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                //if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    //HOperatorSet.GenImageInterleaved(out image, srcBmpData.Scan0, "bgr", bmp.Width, bmp.Height, 0, "byte", 0, 0, 0, 0, -1, 0);
                    HOperatorSet.GenImageInterleaved(out image, srcBmpData.Scan0, "rgb", bmp.Width, bmp.Height, 0, "byte", 0, 0, 0, 0, -1, 0);
                }
                //else
                {
                    // HOperatorSet.GenImage1(out image, "byte", bmp.Width, bmp.Height, srcBmpData.Scan0);
                }
                bitmap.UnlockBits(srcBmpData);
                bitmap?.Dispose();

            }
            catch (Exception ex)
            {
                image?.Dispose();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            labelColor = new Label[] { labelColor1, labelColor2, labelColor3, labelColor4, labelColor5, labelColor6 };
            labelGrey = new Label[] { labelGray1, labelGray2, labelGray3, labelGray4, labelGray5, labelGray6 };
            CheckForIllegalCrossThreadCalls = false;
        }

        private void buttonOcTest_Click(object sender, EventArgs e)
        {
            string stationName = "中心值测试";
            string ss = imagedir + "\\" + stationName + ".bmp";
            StationOcTest OcTest = new StationOcTest(new FormTest(), "中心值测试", textBox1, comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, int.Parse(textBoxOcW.Text) / 2, int.Parse(textBoxOcH.Text) / 2, ss);
            if (listBox1.SelectedIndex != -1)
            {
                if (listBox1.SelectedIndex == 0)
                {
                    OcTest.darkORLight = "light";
                }
                else
                {
                    OcTest.darkORLight = "dark";
                }
                bool b = OcTest.Test();
                textBoxPicSizeW.Text = OcTest.width.ToString();
                textBoxPicSizeH.Text = OcTest.height.ToString();
                if (b)
                {
                    labelOcResultX.Text = OcTest.dW.ToString("f1");
                    labelOcResultY.Text = OcTest.dH.ToString("f1");
                    textBoxOcPOX.Text = OcTest.centerW.ToString("f0");
                    textBoxOcPOY.Text = OcTest.centerH.ToString("f0");
                    info2Log("中心值测试完成");
                }
                else
                {
                    info2Log("中心值测试失败");
                }
            }
            else
            {
                MessageBox.Show("请输入图案明暗");
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            info2Log("中心值测试开始");
            OpenFileDialog fs = new OpenFileDialog();
            fs.InitialDirectory = "D:\\TestImage";
            if (fs.ShowDialog() == DialogResult.OK)
            {
                string stationName = "中心值测试";
                string ss = fs.FileName;
                StationOcTest OcTest = new StationOcTest(new FormTest(), "中心值测试", textBox1, comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, int.Parse(textBoxOcW.Text) / 2, int.Parse(textBoxOcH.Text) / 2, ss);
                if (listBox1.SelectedIndex != -1)
                {
                    if (listBox1.SelectedIndex == 0)
                    {
                        OcTest.darkORLight = "light";
                    }
                    else
                    {
                        OcTest.darkORLight = "dark";
                    }
                    bool b = OcTest.Test();
                    textBoxPicSizeW.Text = OcTest.width.ToString();
                    textBoxPicSizeH.Text = OcTest.height.ToString();
                    if (b)
                    {
                        labelOcResultX.Text = OcTest.dW.ToString("f1");
                        labelOcResultY.Text = OcTest.dH.ToString("f1");
                        textBoxOcPOX.Text = OcTest.centerW.ToString("f0");
                        textBoxOcPOY.Text = OcTest.centerH.ToString("f0");
                        info2Log("中心值测试完成");
                    }
                    else
                    {
                        info2Log("中心值测试失败");
                    }
                }
                else
                {
                    MessageBox.Show("请输入图案明暗");
                }


            }
        }
        public string InitPathF(string filename)
        {
            if (comboBoxProType.SelectedItem.ToString() == "")
            {
                Init(iniPath);
                return "D:\\PruductionConfig\\" + filename;
            }
            else
            {
                string ss = "D:\\PruductionConfig";
                string sss = ss + "\\" + comboBoxProType.SelectedItem.ToString();
                if (Directory.Exists(sss))
                {
                    //bool b = File.Exists(sss + "\\" + "NST_CameraImageTest.ini");
                    if (!(File.Exists(sss + "\\" + filename)))
                    {
                        File.Copy(ss + "\\" + filename, sss + "\\" + filename);

                    }

                }
                else
                {
                    Directory.CreateDirectory(sss);
                    File.Copy(ss + "\\" + filename, sss + "\\" + filename);
                }
                Init(sss + "\\" + filename);
                return sss + "\\" + filename;
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

        private void button2_Click(object sender, EventArgs e)
        {
            info2Log("SFR测试开始");
            InitPathF("NST_CameraImageTest.ini");
            string stationName = "SFR测试";
            string ss = imagedir + "\\" + stationName + ".bmp";
            StationSfrTest SfrTest = new StationSfrTest(new FormTest(), stationName, textBox1, comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
            bool b = SfrTest.Test();
            if (b)
            {
                labelCenterSFR.Text = SfrTest.SFRCenter.ToString("0.00");
                labelLUSFR.Text = SfrTest.SFR_LU.ToString("0.00");
                labelLBSFR.Text = SfrTest.SFR_LD.ToString("0.00");
                labelRUSFR.Text = SfrTest.SFR_RU.ToString("0.00");
                labelRBSFR.Text = SfrTest.SFR_RD.ToString("0.00");
                info2Log("SFR测试完成");
            }
            else
            {
                info2Log("SFR测试失败");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            info2Log("SFR测试开始");
            InitPathF("NST_CameraImageTest.ini");
            OpenFileDialog fs = new OpenFileDialog();
            fs.InitialDirectory = "D:\\TestImage";
            if (fs.ShowDialog() == DialogResult.OK)
            {
                string stationName = "SFR测试";
                string ss = fs.FileName;
                StationSfrTest SfrTest = new StationSfrTest(new FormTest(), stationName, textBox1, comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
                bool b = SfrTest.Test();
                if (b)
                {
                    labelCenterSFR.Text = SfrTest.SFRCenter.ToString("0.00");
                    labelLUSFR.Text = SfrTest.SFR_LU.ToString("0.00");
                    labelLBSFR.Text = SfrTest.SFR_LD.ToString("0.00");
                    labelRUSFR.Text = SfrTest.SFR_RU.ToString("0.00");
                    labelRBSFR.Text = SfrTest.SFR_RD.ToString("0.00");
                    info2Log("SFR测试完成");
                }
                else
                {
                    info2Log("SFR测试失败");
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            info2Log("ColorAndGray测试开始");
            string addrs = InitPathF("ROI.xml");
            string stationName = "ColorAndGray测试";
            string ss = imagedir + "\\" + stationName + ".bmp";
            StationColorAndGray ColorAndGrey = new StationColorAndGray(new FormTest(), stationName, textBox1, comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
            ColorAndGrey.ReadColorRoi(addrs);
            ColorAndGrey.ReadStdRoiLab(addrs);
            bool b = ColorAndGrey.Test();
            if (b)
            {
                info2Log("ColorAndGray测试完成");
                for (int i = 0; i < ColorAndGrey.deltaE.Count(); i++)
                {
                    labelColor[i].Text = ColorAndGrey.deltaE[i].ToString("f0");
                }
            }
            else
            {
                info2Log("ColorAndGray测试失败");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            info2Log("开始标准件测试");
            string addrs = InitPathF("ROI.xml");
            OpenFileDialog fs = new OpenFileDialog();
            fs.InitialDirectory = "D:\\TestImage";
            if (fs.ShowDialog() == DialogResult.OK)
            {
                string stationName = "ColorAndGray测试";
                string ss = fs.FileName;
                StationColorAndGray ColorAndGrey = new StationColorAndGray(new FormTest(), stationName, textBox1, comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
                ColorAndGrey.ReadColorRoi(addrs);
                //ColorAndGrey.ReadStdRoiLab(addrs);
                bool b = ColorAndGrey.StdProductTest();
                if (b)
                {
                    ColorAndGrey.SetStdRoiLab(addrs);
                    info2Log("标准件测试完成");
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            info2Log("ColorAndGray测试开始");
            string addrs = InitPathF("ROI.xml");
            OpenFileDialog fs = new OpenFileDialog();
            fs.InitialDirectory = "D:\\TestImage";
            if (fs.ShowDialog() == DialogResult.OK)
            {
                string stationName = "ColorAndGray测试";
                string ss = fs.FileName;
                StationColorAndGray ColorAndGrey = new StationColorAndGray(new FormTest(), stationName, textBox1, comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
                ColorAndGrey.ReadColorRoi(addrs);
                ColorAndGrey.ReadStdRoiLab(addrs);
                bool b = ColorAndGrey.Test();
                if (b)
                {
                    info2Log("ColorAndGray测试完成");
                    for (int i = 0; i < ColorAndGrey.deltaE.Count(); i++)
                    {
                        labelColor[i].Text = ColorAndGrey.deltaE[i].ToString("f0");
                        labelGrey[i].Text = ColorAndGrey.deltaGray[i].ToString("f0");
                    }
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            info2Log("Blemish测试开始");
            string addrs = InitPathF("NST_CameraImageTest.ini");
            string stationName = "Blemish测试";
            string ss = imagedir + "\\" + stationName + ".bmp";
            StationBlemishTest Blemish = new StationBlemishTest(new FormTest(), stationName, textBox1, comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
            bool b = Blemish.Test();
            if (b)
            {
                labeBlemishCount.Text = Blemish.blCount.ToString("f0");
                info2Log("Blemish测试完成");

            }
            else
            {
                info2Log("Blemish测试失败");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            info2Log("Blemish测试开始");
            string addrs = InitPathF("NST_CameraImageTest.ini");
            string stationName = "Blemish测试";
            string ss = imagedir + "\\" + stationName + ".bmp";
            StationBlemishTest1 Blemish = new StationBlemishTest1(new FormTest(), stationName, textBox1, comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
            bool b = Blemish.Test();
            if (!b)
            {
                labeBlemishCount.Text = Blemish.blCount.ToString("f0");
                info2Log("Blemish测试有污渍");

            }
            else
            {
                info2Log("Blemish测试OK");
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            labeBlemishCount.Text = "0";
            info2Log("Blemish测试开始");
            string addrs = InitPathF("NST_CameraImageTest.ini");
            string stationName = "Blemish测试";
            OpenFileDialog fs = new OpenFileDialog();
            fs.InitialDirectory = "D:\\TestImage";
            if (fs.ShowDialog() == DialogResult.OK)
            {
                string ss = fs.FileName;
                StationBlemishTest1 Blemish = new StationBlemishTest1(new FormTest(), stationName, textBox1, comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
                bool b = Blemish.Test();
                if (!b)
                {
                    labeBlemishCount.Text = Blemish.blCount.ToString("f0");
                    info2Log("Blemish测试有污渍");
                }
                else
                {
                    info2Log("Blemish测试OK");
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            info2Log("SFR测试开始");
            InitPathF("NST_CameraImageTest.ini");
            OpenFileDialog fs = new OpenFileDialog();
            fs.InitialDirectory = "D:\\TestImage";
            if (fs.ShowDialog() == DialogResult.OK)
            {
                string stationName = "SFR测试";
                string ss = fs.FileName;
                StationSfrTest SfrTest = new StationSfrTest(new FormTest(), stationName, textBox1, comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
                bool b = SfrTest.Test1();
                if (b)
                {
                    labelCenterSFR.Text = SfrTest.thisSigleChart.m_dSfrAvgValue.ToString("0.00");
                    //labelLUSFR.Text = SfrTest.SFR_LU.ToString("0.00");
                    //labelLBSFR.Text = SfrTest.SFR_LD.ToString("0.00");
                    //labelRUSFR.Text = SfrTest.SFR_RU.ToString("0.00");
                    //labelRBSFR.Text = SfrTest.SFR_RD.ToString("0.00");
                    info2Log("SFR测试完成");
                }
                else
                {
                    info2Log("SFR测试失败");
                }
            }
        }

        private void stoppreview_Click(object sender, EventArgs e)
        {
            if (listIsStart.Count == 0) return;
            int cam = channelNum.SelectedIndex;
            if (!listIsGrab[cam])
            {
                info2Log("stopGrab already, ");
                return;
            }
            stoppGrab(cam);
            bool bStartOk = listRlgPlatform[cam].SensorStopPreview();
            if (!bStartOk)
            {
                info2Log("stopPreview fail, ");
                return;
            }
            info2Log("stopPreview ok, ");
            //imageViewer.Image.Overlays.Default.Clear();
            //showROI(mtfRect, imageViewer, roiColorPicker.Value);
            
            //roi_TextOverlays(mtfRect, imageViewer, roiColorPicker.Value);
            //showMTF(mtfRect, imageViewer, bitmapcache, roiColorPicker.Value);
            //roiColorPicker.Enabled = !false;
            //OCColorPicker.Enabled = !false;
            //OCVisibleCheckBox.Enabled = !false;
            saveImage.Enabled = !false;
            
        }
        private void stoppGrab(int cam)
        {
            listIsStart[cam] = false;

            while (listIsGrab[cam])
            {
                Application.DoEvents();
            }
        }

        private void hWindowControl1_HMouseWheel(object sender, HMouseEventArgs e)
        {
            double h = hWindowControl1.Size.Height;
            double w = hWindowControl1.Size.Width;
            if (e.Delta != 0)
            {
                h = (1 + e.Delta * 0.001) * h;
                w = (1 + e.Delta * 0.001) * w;

                hWindowControl1.Size = new Size((int)w, (int)h);
            }
        }




        

        private void hWindowControl1_MouseMove(object sender, MouseEventArgs e)
        {
            HTuple r = 0;
            HTuple c = 0;
            HTuple b = 0;
            
            HOperatorSet.GetMposition(hWindowControl1.HalconWindow, out r, out c, out b);
            label14.Text = c.ToString();
            label13.Text = r.ToString();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void closedevice_Click(object sender, EventArgs e)
        {
            int cnt = RlgPlatInterface.GetInstance().HisFX3GetDevCount();
            int nRst = 0;
            try
            {
                for (int i = 0; i < cnt; i++)
                {
                    nRst = RlgPlatInterface.GetInstance().HisFX3CloseDevice(i);
                    if (nRst != 0)
                    {
                        info2Log(string.Format("index{0} closedevice fail", i));
                    }
                    else
                    {
                        info2Log(string.Format("index{0} closedevice succeed", i));
                    }
                }
            }
            catch (Exception ex)
            {
                info2Log(ex.Message);
            }
        }

        private void saveImage_Click(object sender, EventArgs e)
        {
            if (loadImage)
            {
                SaveFileDialog fs = new SaveFileDialog();
                fs.InitialDirectory = "D:\\Patch";
                fs.Filter = "图像文件（*.bmp）|*.bmp";
                if (fs.ShowDialog() == DialogResult.OK)
                {
                    //imageViewer.Image.WriteBmpFile(fs.FileName);
                    //hWindowControl1.HalconWindow.GetWindowBackgroundImage().WriteImage("bitmap", 0, fs.FileName);
                    HImage h = hWindowControl1.HalconWindow.DumpWindowImage();// WriteImage("bitmap", 0, fs.FileName);
                    HOperatorSet.WriteImage(h, "bmp", 0, fs.FileName);
                }
            }
        }

        private void saveDevice_Click(object sender, EventArgs e)
        {
            bitmapcache.Save("D:\\TestImage\\"+comboBoxStation.SelectedText+".bmp");
        }
    }
    
}
