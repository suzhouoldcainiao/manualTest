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
            mes = MesHelper.Instance;
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

        List<TextBox> sfrSpecTextboxList = new List<TextBox>();
        List<Label> sfrResultLableList = new List<Label>();
        int colorT=0;
        MesHelper mes;
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
        public void Info2LogMES(string str)
        {
            try
            {
                textBoxFromMes.Invoke((MethodInvoker)delegate
                {
                    textBoxFromMes.AppendText("[" + DateTime.Now.ToLongTimeString() + ":] " + str + "\r\n");
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
        int cam;
        private void startpreview_Click(object sender, EventArgs e)
        {
            if (listIsStart.Count == 0) return;
            cam = channelNum.SelectedIndex;
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
            //saveImage.Enabled = false;
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
                            if (checkBoxCross.Checked)
                            {
                                DrawCrossLine(hWindowControl1.HalconWindow, bmp.Width, bmp.Height);
                            }
                            //DrawCrossLine(hWindowControl1.HalconWindow, bmp.Width, bmp.Height);
                            uint a1 = 0;
                            uint a2 = 0;
                            RlgPlatInterface.GetInstance().HisFX3GetTotalFrame(ref a1, ref a2, 1, cam);
                            label1.Text = string.Format("总的正确帧数是{0}FPS,总的错误帧数是{1}FPS", a1, a2);
                            if (checkBoxSfr.Checked)
                            {
                                SfrTestRun(bmp, hWindowControl1.HalconWindow);
                            }
                            //SfrTestRun(bmp, hWindowControl1.HalconWindow);
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
                HTuple yy = 120;
                HOperatorSet.SetFont(win, $"-Courier New-{textBoxSfrSize.Text}-*-*-*-*-1-");
                HOperatorSet.SetTposition(win, ThisSFRInfo.SearchROI[0].y-yy, ThisSFRInfo.SearchROI[0].x);
                HOperatorSet.WriteString(win, SFRCenter.ToString("0.00"));
                HOperatorSet.SetTposition(win, ThisSFRInfo.SearchROI[1].y-yy, ThisSFRInfo.SearchROI[1].x);
                HOperatorSet.WriteString(win, SFR_LU.ToString("0.00"));
                HOperatorSet.SetTposition(win, ThisSFRInfo.SearchROI[2].y-yy, ThisSFRInfo.SearchROI[2].x);
                HOperatorSet.WriteString(win, SFR_RU.ToString("0.00"));
                HOperatorSet.SetTposition(win, ThisSFRInfo.SearchROI[3].y-yy, ThisSFRInfo.SearchROI[3].x);
                HOperatorSet.WriteString(win, SFR_LD.ToString("0.00"));
                HOperatorSet.SetTposition(win, ThisSFRInfo.SearchROI[4].y-yy, ThisSFRInfo.SearchROI[4].x);
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
                        HOperatorSet.GenRectangle2(out ho_Rectangle, ThisSFRInfo.SearchROI[i].y + ThisSFRInfo.SearchROI[i].height / 2, 
                            ThisSFRInfo.SearchROI[i].x + ThisSFRInfo.SearchROI[i].width / 2, 0, ThisSFRInfo.SearchROI[i].width / 2,
                            ThisSFRInfo.SearchROI[i].height / 2);
                        HOperatorSet.DispObj(ho_Rectangle, wHandle);

                        HOperatorSet.SetColor(wHandle, "red");
                        HOperatorSet.SetLineWidth(wHandle, 1);
                        for (int j = 0; j < 5; j++)
                        {
                            HOperatorSet.GenRectangle2(out ho_Rectangle, ThisSFRInfo.ObjectROI[i][j].y + ThisSFRInfo.ObjectROI[i][j].height / 2,
                                ThisSFRInfo.ObjectROI[i][j].x + ThisSFRInfo.ObjectROI[i][j].width / 2, 0, ThisSFRInfo.ObjectROI[i][j].width / 2, 
                                ThisSFRInfo.ObjectROI[i][j].height / 2);
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
            sfrSpecTextboxList.Add(textBoxSfrSpecOC);
            sfrSpecTextboxList.Add(textBoxSfrSpecLU);
            sfrSpecTextboxList.Add(textBoxSfrSpecRU);
            sfrSpecTextboxList.Add(textBoxSfrSpecLB);
            sfrSpecTextboxList.Add(textBoxSfrSpecRB);
            sfrSpecTextboxList.Add(textBoxSfrSpecLU2);
            sfrSpecTextboxList.Add(textBoxSfrSpecRU2);
            sfrSpecTextboxList.Add(textBoxSfrSpecLB2);
            sfrSpecTextboxList.Add(textBoxSfrSpecRB2);
            sfrResultLableList.Add(labelCenterSFR);
            sfrResultLableList.Add(labelLUSFR);
            sfrResultLableList.Add(labelRUSFR);
            sfrResultLableList.Add(labelLBSFR);
            sfrResultLableList.Add(labelRBSFR);
            sfrResultLableList.Add(labelLUSFR2);
            sfrResultLableList.Add(labelRUSFR2);
            sfrResultLableList.Add(labelLBSFR2);
            sfrResultLableList.Add(labelRBSFR2);
        }
        bool bOcTest;
        private void buttonOcTest_Click(object sender, EventArgs e)
        {
            string stationName = "中心值测试";
            string ss = imagedir + "\\" + stationName + ".bmp";
            StationOcTest OcTest = new StationOcTest(new FormTest(), "中心值测试", textBox1,
                comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, int.Parse(textBoxOcW.Text) / 2, int.Parse(textBoxOcH.Text) / 2, ss);
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
                bOcTest = OcTest.Test();
                textBoxPicSizeW.Text = OcTest.width.ToString();
                textBoxPicSizeH.Text = OcTest.height.ToString();
                if (bOcTest)
                {
                    labelOcResultX.Text = OcTest.dW.ToString("f1");
                    labelOcResultY.Text = OcTest.dH.ToString("f1");
                    textBoxOcPOX.Text = OcTest.centerW.ToString("f0");
                    textBoxOcPOY.Text = OcTest.centerH.ToString("f0");
                    info2Log("中心值测试完成");
                    if (Math.Abs( OcTest.dW)>=double.Parse(textBoxOCXSpec.Text)||Math.Abs( OcTest.dH)>=double.Parse(textBoxOCYSpec.Text))
                    {
                        ChangeJudgeLable(labelJudgeOc, false,true,"NG");
                    }
                    else
                    {
                        ChangeJudgeLable(labelJudgeOc, true,true,"OK");
                    }
                }
                else
                {
                    info2Log("中心值测试失败");
                    ChangeJudgeLable(labelJudgeOc, false, true, "NoTest");
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
            //fs.InitialDirectory = "D:\\TestImage";
            if (fs.ShowDialog() == DialogResult.OK)
            {
                string stationName = "中心值测试";
                string ss = fs.FileName;
                StationOcTest OcTest = new StationOcTest(new FormTest(), "中心值测试", textBox1,
                    comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, int.Parse(textBoxOcW.Text) / 2, int.Parse(textBoxOcH.Text) / 2, ss);
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
                    bOcTest = OcTest.Test();
                    textBoxPicSizeW.Text = OcTest.width.ToString();
                    textBoxPicSizeH.Text = OcTest.height.ToString();
                    if (bOcTest)
                    {
                        labelOcResultX.Text = OcTest.dW.ToString("f1");
                        labelOcResultY.Text = OcTest.dH.ToString("f1");
                        textBoxOcPOX.Text = OcTest.centerW.ToString("f0");
                        textBoxOcPOY.Text = OcTest.centerH.ToString("f0");
                        info2Log("中心值测试完成");
                        if (Math.Abs(OcTest.dW) >= double.Parse(textBoxOCXSpec.Text) || Math.Abs(OcTest.dH) >= double.Parse(textBoxOCYSpec.Text))
                        {
                            ChangeJudgeLable(labelJudgeOc, false,true,"NG");
                        }
                        else
                        {
                            ChangeJudgeLable(labelJudgeOc, true,true,"OK");
                        }
                    }
                    else
                    {
                        info2Log("中心值测试失败");
                        ChangeJudgeLable(labelJudgeOc, false, true, "测试失败");
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
            StationSfrTest SfrTest = new StationSfrTest(new FormTest(), stationName, textBox1, 
                comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss,this.checkBox9Point.Checked);
            bool b = SfrTest.Test();
            if (b)
            {
                labelCenterSFR.Text = SfrTest.SFRCenter.ToString("0.00");
                labelLUSFR.Text = SfrTest.SFR_LU.ToString("0.00");
                labelLBSFR.Text = SfrTest.SFR_LD.ToString("0.00");
                labelRUSFR.Text = SfrTest.SFR_RU.ToString("0.00");
                labelRBSFR.Text = SfrTest.SFR_RD.ToString("0.00");
                if (this.checkBox9Point.Checked)
                {
                    labelLUSFR2.Text = SfrTest.SFR_LU2.ToString("0.00");
                    labelLBSFR2.Text = SfrTest.SFR_LD2.ToString("0.00");
                    labelRUSFR2.Text = SfrTest.SFR_RU2.ToString("0.00");
                    labelRBSFR2.Text = SfrTest.SFR_RD2.ToString("0.00");

                    for (int i = 0; i < sfrResultLableList.Count; i++)
                    {
                        b = b && (double.Parse(sfrResultLableList[i].Text) >= double.Parse(sfrSpecTextboxList[i].Text));
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        b = b && (double.Parse(sfrResultLableList[i].Text) >= double.Parse(sfrSpecTextboxList[i].Text));
                    }
                }
                if (b)
                {
                    ChangeJudgeLable(labelJudgeSfr, true, true, "OK");
                }
                else
                {
                    ChangeJudgeLable(labelJudgeSfr, false, true, "NG");
                }

                info2Log("SFR测试完成");
                
            }
            else
            {
                info2Log("SFR测试失败");
                ChangeJudgeLable(labelJudgeSfr, false, true, "NoTest");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            info2Log("SFR测试开始");
            InitPathF("NST_CameraImageTest.ini");
            OpenFileDialog fs = new OpenFileDialog();
            //fs.InitialDirectory = "D:\\TestImage";
            if (fs.ShowDialog() == DialogResult.OK)
            {
                string stationName = "SFR测试";
                string ss = fs.FileName;
                StationSfrTest SfrTest = new StationSfrTest(new FormTest(), stationName, textBox1,
                    comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss,this.checkBox9Point.Checked);
                bool b = SfrTest.Test();
                if (b)
                {
                    labelCenterSFR.Text = SfrTest.SFRCenter.ToString("0.00");
                    labelLUSFR.Text = SfrTest.SFR_LU.ToString("0.00");
                    labelLBSFR.Text = SfrTest.SFR_LD.ToString("0.00");
                    labelRUSFR.Text = SfrTest.SFR_RU.ToString("0.00");
                    labelRBSFR.Text = SfrTest.SFR_RD.ToString("0.00");
                    info2Log("SFR测试完成");
                    if (this.checkBox9Point.Checked)
                    {
                        labelLUSFR2.Text = SfrTest.SFR_LU2.ToString("0.00");
                        labelLBSFR2.Text = SfrTest.SFR_LD2.ToString("0.00");
                        labelRUSFR2.Text = SfrTest.SFR_RU2.ToString("0.00");
                        labelRBSFR2.Text = SfrTest.SFR_RD2.ToString("0.00");

                        for (int i = 0; i < sfrResultLableList.Count; i++)
                        {
                            b = b && (double.Parse(sfrResultLableList[i].Text) >= double.Parse(sfrSpecTextboxList[i].Text));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            b = b && (double.Parse(sfrResultLableList[i].Text) >= double.Parse(sfrSpecTextboxList[i].Text));
                        }
                    }
                    if (b)
                    {
                        ChangeJudgeLable(labelJudgeSfr, true, true, "OK");
                    }
                    else
                    {
                        ChangeJudgeLable(labelJudgeSfr, false, true, "NG");
                    }

                    info2Log("SFR测试完成");

                }
                else
                {
                    info2Log("SFR测试失败");
                    ChangeJudgeLable(labelJudgeSfr, false, true, "NoTest");
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            info2Log("ColorAndGray测试开始");
            string addrs = InitPathF("ROI.xml");
            string stationName = "ColorAndGray测试";
            string ss = imagedir + "\\" + stationName + ".bmp";
            StationColorAndGray ColorAndGrey = new StationColorAndGray(new FormTest(), stationName, 
                textBox1, comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
            ColorAndGrey.ReadColorRoi(addrs);
            ColorAndGrey.ReadStdRoiLab(addrs);
            bool b = ColorAndGrey.Test();
            if (b)
            {
                info2Log("ColorAndGray测试完成");
                for (int i = 0; i < ColorAndGrey.deltaE.Count(); i++)
                {
                    labelColor[i].Text = ColorAndGrey.deltaE[i].ToString("f0");
                    b = (b && (ColorAndGrey.deltaE[i] <= double.Parse(textBoxColorSpec.Text)));
                    
                }
                for (int i = 0; i < ColorAndGrey.deltaGray.Count()-1; i++)
                {
                    labelGrey[i].Text = ColorAndGrey.deltaGray[i].ToString("f0");
                    b = (b && (ColorAndGrey.deltaGray[i] >= double.Parse(textBoxGraySpec.Text)));
                }
                if (b)
                {
                    ChangeJudgeLable(labelJudgeColor, true, true, "OK");
                }
                else
                {
                    ChangeJudgeLable(labelJudgeColor, false, true, "NG");
                }
            }
            else
            {
                info2Log("ColorAndGray测试失败");
                ChangeJudgeLable(labelJudgeColor, false, true, "NoTest");
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
                StationColorAndGray ColorAndGrey = new StationColorAndGray(new FormTest(), stationName, 
                    textBox1, comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
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
            //fs.InitialDirectory = "D:\\TestImage";
            if (fs.ShowDialog() == DialogResult.OK)
            {
                string stationName = "ColorAndGray测试";
                string ss = fs.FileName;
                StationColorAndGray ColorAndGrey = new StationColorAndGray(new FormTest(), stationName, textBox1,
                    comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
                ColorAndGrey.ReadColorRoi(addrs);
                ColorAndGrey.ReadStdRoiLab(addrs);
                bool b = ColorAndGrey.Test();
                if (b)
                {
                    info2Log("ColorAndGray测试完成");
                    for (int i = 0; i < ColorAndGrey.deltaE.Count(); i++)
                    {
                        labelColor[i].Text = ColorAndGrey.deltaE[i].ToString("f0");
                        b = (b && (ColorAndGrey.deltaE[i] <= double.Parse(textBoxColorSpec.Text)));

                    }
                    for (int i = 0; i < ColorAndGrey.deltaGray.Count() - 1; i++)
                    {
                        labelGrey[i].Text = ColorAndGrey.deltaGray[i].ToString("f0");
                        b = (b && (ColorAndGrey.deltaGray[i] >= double.Parse(textBoxGraySpec.Text)));
                    }
                    if (b)
                    {
                        ChangeJudgeLable(labelJudgeColor, true, true, "OK");
                    }
                    else
                    {
                        ChangeJudgeLable(labelJudgeColor, false, true, "NG");
                    }
                }
                else
                {
                    info2Log("ColorAndGray测试失败");
                    ChangeJudgeLable(labelJudgeColor, false, true, "NoTest");
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            info2Log("Blemish测试开始");
            string addrs = InitPathF("NST_CameraImageTest.ini");
            string stationName = "Blemish测试";
            string ss = imagedir + "\\" + stationName + ".bmp";
            StationBlemishTest Blemish = new StationBlemishTest(new FormTest(), stationName, 
                textBox1, comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
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
            StationBlemishTest1 Blemish = new StationBlemishTest1(new FormTest(), stationName, 
                textBox1, comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
            bool b = Blemish.Test();
            if (b)
            {
                
                info2Log("Blemish测试OK");
                ChangeJudgeLable(labelJudgeBlemish, true, true, "OK");
            }
            else
            {
                labeBlemishCount.Text = Blemish.blCount.ToString("f0");
                info2Log("Blemish测试有污渍");
                ChangeJudgeLable(labelJudgeBlemish, false, true, "NG");
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            labeBlemishCount.Text = "0";
            info2Log("Blemish测试开始");
            string addrs = InitPathF("NST_CameraImageTest.ini");
            string stationName = "Blemish测试";
            OpenFileDialog fs = new OpenFileDialog();
            //fs.InitialDirectory = "D:\\TestImage";
            if (fs.ShowDialog() == DialogResult.OK)
            {
                string ss = fs.FileName;
                StationBlemishTest1 Blemish = new StationBlemishTest1(new FormTest(), stationName,
                    textBox1, comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
                bool b = Blemish.Test();
                if (b)
                {

                    info2Log("Blemish测试OK");
                    ChangeJudgeLable(labelJudgeBlemish, true, true, "OK");
                }
                else
                {
                    labeBlemishCount.Text = Blemish.blCount.ToString("f0");
                    info2Log("Blemish测试有污渍");
                    ChangeJudgeLable(labelJudgeBlemish, false, true, "NG");
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
                StationSfrTest SfrTest = new StationSfrTest(new FormTest(), stationName, textBox1, 
                    comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss,this.checkBox9Point.Checked);
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
            //if (loadImage)
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
            //bitmapcache.Save("D:\\TestImage\\"+comboBoxStation.SelectedItem.ToString()+".bmp");
            bitmapcache.Save("D:\\TestImage\\" + comboBoxStation.SelectedItem.ToString() + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            labelParticleS.Text = "0";
            labelParticleM.Text = "0";
            labelParticleB.Text = "0";
            info2Log("Particle测试开始");
            string addrs = InitPathF("NST_CameraImageTest.ini");
            string stationName = "ParticleTest测试";
            OpenFileDialog fs = new OpenFileDialog();
            //fs.InitialDirectory = "D:\\TestImage";
            if (fs.ShowDialog() == DialogResult.OK)
            {
                string ss = fs.FileName;
                StationParticleTest particle = new StationParticleTest(new FormTest(), stationName, textBox1,
                    comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
                bool b = particle.Test();
                if (b)
                {
                    labelParticleS.Text = particle.smallP.ToString("f0");
                    labelParticleM.Text = particle.mediumP.ToString("f0");
                    labelParticleB.Text = particle.bigP.ToString("f0");
                    info2Log("Particle测试有污渍"); b = b && (int.Parse(textBoxParticleSpecSmall.Text) >= particle.smallP) && 
                        (int.Parse(textBoxParticleSpecMedium.Text) >= particle.mediumP) && (int.Parse(textBoxParticleSpecBig.Text) >= particle.bigP);
                    if (b)
                    {
                        ChangeJudgeLable(labelJudgeParticle, true, true, "OK");
                    }
                    else
                    {
                        ChangeJudgeLable(labelJudgeParticle, false, true, "NG");
                    }
                }
                else
                {
                    info2Log("Particle测试OK");
                    ChangeJudgeLable(labelJudgeParticle, false, true, "未测试");
                }
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            labelParticleS.Text = "0";
            labelParticleM.Text = "0";
            labelParticleB.Text = "0";
            info2Log("Particle测试开始");
            string addrs = InitPathF("NST_CameraImageTest.ini");
            string stationName = "ParticleTest测试";
            string ss = imagedir + "\\" + stationName + ".bmp";
            StationParticleTest particle = new StationParticleTest(new FormTest(), stationName, textBox1,
                comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss);
            bool b = particle.Test();
            if (b)
            {
                labelParticleS.Text = particle.smallP.ToString("f0");
                labelParticleM.Text = particle.mediumP.ToString("f0");
                labelParticleB.Text = particle.bigP.ToString("f0");
                info2Log("Particle测试有污渍");
                b = b && (int.Parse(textBoxParticleSpecSmall.Text) >= particle.smallP) && 
                    (int.Parse(textBoxParticleSpecMedium.Text) >=particle.mediumP) && (int.Parse(textBoxParticleSpecBig.Text) >= particle.bigP);
                if (b)
                {
                    ChangeJudgeLable(labelJudgeParticle, true, true, "OK");
                }
                else
                {
                    ChangeJudgeLable(labelJudgeParticle, false, true, "NG");
                }
            }
            else
            {
                info2Log("Particle测试OK");
                ChangeJudgeLable(labelJudgeParticle, false, true, "未测试");
            }
        }
        public void ChangeJudgeLable(Label label,bool gORr,bool changeValue,string value=null)
        {
            if (gORr)
            {
                label.BackColor = Color.Green;
                
                
            }
            else
            {
                label.BackColor = Color.Red;
                
            }
            if (changeValue)
            {
                label.Text = value;
            }
        }

        private void timerjudge_Tick(object sender, EventArgs e)
        {

        }



        private void textBox2SfrSpecOC_TextChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox9Point.Checked==true)
            {
                for (int i = 0; i < 4; i++)
                {
                    sfrSpecTextboxList[i + 5].Visible = true;
                    sfrResultLableList[i + 5].Visible = true;
                   
                }
                label31.Visible = true;
                label29.Visible = true;
                label24.Visible = true;
                label20.Visible = true;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    sfrSpecTextboxList[i + 5].Visible = false;
                    sfrResultLableList[i + 5].Visible = false;

                }
                label31.Visible = false;
                label29.Visible = false;
                label24.Visible = false;
                label20.Visible = false;
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            labelLensPCR.Text = "0";
            labelLensPPR.Text = "0";
            
            info2Log("LensShading测试开始");
            string addrs = InitPathF("NST_CameraImageTest.ini");
            string stationName = "LensShading测试";
            string ss = imagedir + "\\" + stationName + ".bmp";
            StationLensShadingTest lensShading = new StationLensShadingTest(new FormTest(), stationName, textBox1,
                comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss, Convert.ToInt32(this.textBoxShadingROI.Text),
                Convert.ToDouble(this.textBoxShadingX.Text), Convert.ToDouble(this.textBoxShadingY.Text));
            bool b = lensShading.Test();
            if (b)
            {
                labelLensPPR.Text = lensShading.greyppr.ToString("f6");
                labelLensPCR.Text = lensShading.greypcr.ToString("f6");
                
                info2Log("LensShading测试开始");
                b = b && (double.Parse(textBoxShadingPCR.Text) >= lensShading.greypcr) && (double.Parse(textBoxShadingPPR.Text) >= lensShading.greyppr);
                if (b)
                {
                    ChangeJudgeLable(labelJudgeLensShading, true, true, "OK");
                }
                else
                {
                    ChangeJudgeLable(labelJudgeLensShading, false, true, "NG");
                }
            }
            else
            {
                info2Log("LensShading测试失败");
                ChangeJudgeLable(labelJudgeLensShading, false, true, "未测试");
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            labelLensPCR.Text = "0";
            labelLensPPR.Text = "0";
            labelPcrR.Text = "0";
            labelPcrG.Text = "0";
            labelPcrB.Text = "0";
            labelPprR.Text = "0";
            labelPprG.Text = "0";
            labelPprB.Text = "0";

            info2Log("LensShading测试开始");
            string addrs = InitPathF("NST_CameraImageTest.ini");
            string stationName = "LensShading测试";
            string ss = imagedir + "\\" + stationName + ".bmp";
            StationLensShadingTest lensShading = new StationLensShadingTest(new FormTest(), stationName, textBox1, 
                comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss,Convert.ToInt32( this.textBoxShadingROI.Text),
                Convert.ToDouble(this.textBoxShadingX.Text),Convert.ToDouble(this.textBoxShadingY.Text));
            if (radioButtonRGB.Checked==false)
            {
                bool b = lensShading.Test1();
                if (b)
                {
                    labelLensPPR.Text = lensShading.greyppr.ToString("f6");
                    labelLensPCR.Text = lensShading.greypcr.ToString("f6");

                    info2Log("LensShading测试开始");
                    b = b && (double.Parse(textBoxShadingPCR.Text) <= lensShading.greypcr) && (double.Parse(textBoxShadingPPR.Text) >= lensShading.greyppr);
                    if (b)
                    {
                        ChangeJudgeLable(labelJudgeLensShading, true, true, "OK");
                    }
                    else
                    {
                        ChangeJudgeLable(labelJudgeLensShading, false, true, "NG");
                    }
                }
                else
                {
                    info2Log("LensShading测试失败");
                    ChangeJudgeLable(labelJudgeLensShading, false, true, "未测试");
                }
            }
            else
            {
                bool b = lensShading.Test2();
                if (b)
                {
                    labelPcrR.Text = lensShading.pcr[0].ToString("f6");
                    labelPcrG.Text = lensShading.pcr[1].ToString("f6");
                    labelPcrB.Text = lensShading.pcr[2].ToString("f6");
                    labelPprR.Text = lensShading.ppr[0].ToString("f6");
                    labelPprG.Text = lensShading.ppr[1].ToString("f6");
                    labelPprB.Text = lensShading.ppr[2].ToString("f6");
                    info2Log("LensShading测试开始");
                    double dpcrmin = double.Parse(textBox2.Text);
                    double dpcrmax = double.Parse(textBox4.Text);
                    double dppr = double.Parse(textBox3.Text);
                    b = b && (dpcrmax >= lensShading.pcr[0] && dpcrmax >= lensShading.pcr[1] && dpcrmax >=lensShading.pcr[2]&& 
                        dpcrmin <= lensShading.pcr[0])&& (dpcrmin <= lensShading.pcr[1]) && (dpcrmin <= lensShading.pcr[2]) && 
                        (dppr >= lensShading.ppr[0]) && (dppr >= lensShading.ppr[1]) && (dppr >= lensShading.ppr[2]);
                    if (b)
                    {
                        ChangeJudgeLable(labelJudgeLensShading, true, true, "OK");
                    }
                    else
                    {
                        ChangeJudgeLable(labelJudgeLensShading, false, true, "NG");
                    }
                }
                else
                {
                    info2Log("LensShading测试失败");
                    ChangeJudgeLable(labelJudgeLensShading, false, true, "未测试");
                }
            }
            
        }

        private void radioButtonRGB_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonRGB.Checked==false)
            {
                groupBox1.Visible = false;
            }
            else
            {
                groupBox1.Visible = true;
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            labelLensPCR.Text = "0";
            labelLensPPR.Text = "0";
            labelPcrR.Text = "0";
            labelPcrG.Text = "0";
            labelPcrB.Text = "0";
            labelPprR.Text = "0";
            labelPprG.Text = "0";
            labelPprB.Text = "0";

            info2Log("LensShading测试开始");
            string addrs = InitPathF("NST_CameraImageTest.ini");
            string stationName = "LensShading测试";
            OpenFileDialog fs = new OpenFileDialog();
            //fs.InitialDirectory = "D:\\TestImage";
            if (fs.ShowDialog() == DialogResult.OK)
            {
                string ss = fs.FileName;
                //string ss = imagedir + "\\" + stationName + ".bmp";
                StationLensShadingTest lensShading = new StationLensShadingTest(new FormTest(), stationName, textBox1, 
                    comboBoxProType.SelectedItem.ToString(), textBoxProSN.Text, ss, Convert.ToInt32(this.textBoxShadingROI.Text),
                    Convert.ToDouble(this.textBoxShadingX.Text), Convert.ToDouble(this.textBoxShadingY.Text));
                if (radioButtonRGB.Checked == false)
                {
                    bool b = lensShading.Test1();
                    if (b)
                    {
                        labelLensPPR.Text = lensShading.greyppr.ToString("f6");
                        labelLensPCR.Text = lensShading.greypcr.ToString("f6");

                        info2Log("LensShading测试开始");
                        b = b && (double.Parse(textBoxShadingPCR.Text) <= lensShading.greypcr) && (double.Parse(textBoxShadingPPR.Text) >= lensShading.greyppr);
                        if (b)
                        {
                            ChangeJudgeLable(labelJudgeLensShading, true, true, "OK");
                        }
                        else
                        {
                            ChangeJudgeLable(labelJudgeLensShading, false, true, "NG");
                        }
                    }
                    else
                    {
                        info2Log("LensShading测试失败");
                        ChangeJudgeLable(labelJudgeLensShading, false, true, "未测试");
                    }
                }
                else
                {
                    bool b = lensShading.Test2();
                    if (b)
                    {
                        labelPcrR.Text = lensShading.pcr[0].ToString("f6");
                        labelPcrG.Text = lensShading.pcr[1].ToString("f6");
                        labelPcrB.Text = lensShading.pcr[2].ToString("f6");
                        labelPprR.Text = lensShading.ppr[0].ToString("f6");
                        labelPprG.Text = lensShading.ppr[1].ToString("f6");
                        labelPprB.Text = lensShading.ppr[2].ToString("f6");
                        info2Log("LensShading测试开始");
                        double dpcrmin = double.Parse(textBox2.Text);
                        double dpcrmax = double.Parse(textBox4.Text);
                        double dppr = double.Parse(textBox3.Text);
                        b = b && (dpcrmax >= lensShading.pcr[0] && dpcrmax >= lensShading.pcr[1] && dpcrmax >= lensShading.pcr[2] && 
                            dpcrmin <= lensShading.pcr[0]) && (dpcrmin <= lensShading.pcr[1]) && (dpcrmin <= lensShading.pcr[2]) && 
                            (dppr >= lensShading.ppr[0]) && (dppr >= lensShading.ppr[1]) && (dppr >= lensShading.ppr[2]);
                        if (b)
                        {
                            ChangeJudgeLable(labelJudgeLensShading, true, true, "OK");
                        }
                        else
                        {
                            ChangeJudgeLable(labelJudgeLensShading, false, true, "NG");
                        }
                    }
                    else
                    {
                        info2Log("LensShading测试失败");
                        ChangeJudgeLable(labelJudgeLensShading, false, true, "未测试");
                    }
                }
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            string stationName;
            switch (colorT)
            {
                case 0:
                    label28001.Text = "0";
                    label28002.Text = "0";
                    label28003.Text = "0";
                    label28004.Text = "0";
                    label28005.Text = "0";
                    stationName = "WB-2800测试";
                    break;
                case 1:
                    label40001.Text = "0";
                    label40002.Text = "0";
                    label40003.Text = "0";
                    label40004.Text = "0";
                    label40005.Text = "0";
                    stationName = "WB-4000测试";
                    break;
                case 2:
                    label55001.Text = "0";
                    label55002.Text = "0";
                    label55003.Text = "0";
                    label55004.Text = "0";
                    label55005.Text = "0";
                    stationName = "WB-5500测试";
                    break;
                default:
                    stationName = null;
                    break;
            }
            
            info2Log("WB测试开始");
            string addrs = InitPathF("NST_CameraImageTest.ini");
            
            string ss = imagedir + "\\" + stationName + ".bmp";
            StationWBTest WB = new StationWBTest(new FormTest(), stationName, textBox1, comboBoxProType.SelectedItem.ToString(),
                textBoxProSN.Text, ss,Convert.ToInt32(textBoxWBW.Text),Convert.ToInt32(textBoxWBH.Text),colorT);
            bool b = WB.Test();
            if (b)
            {
                switch (colorT)
                {
                    case 0:
                        label28001.Text = WB.RG.ToString("f2");
                        label28002.Text = WB.BG.ToString("f2");
                        label28003.Text = WB.R.ToString("f2");
                        label28004.Text = WB.G.ToString("f2");
                        label28005.Text = WB.B.ToString("f2");
                        info2Log("WB测试2800K");
                        break;
                    case 1:
                        label40001.Text = WB.RG.ToString("f2");
                        label40002.Text = WB.BG.ToString("f2");
                        label40003.Text = WB.R.ToString("f2");
                        label40004.Text = WB.G.ToString("f2");
                        label40005.Text = WB.B.ToString("f2");
                        info2Log("WB测试4000K");
                        break;
                    case 2:
                        label55001.Text = WB.RG.ToString("f2");
                        label55002.Text = WB.BG.ToString("f2");
                        label55003.Text = WB.R.ToString("f2");
                        label55004.Text = WB.G.ToString("f2");
                        label55005.Text = WB.B.ToString("f2");
                        info2Log("WB测试5500K");
                        break;
                    default:
                        break;
                }
                
                b = b && (double.Parse(textBoxRGUpL.Text) >= WB.RG) && (double.Parse(textBoxBGUpL.Text) >= WB.BG);
                if (b)
                {
                    ChangeJudgeLable(labelJudgeWB, true, true, "OK");
                }
                else
                {
                    ChangeJudgeLable(labelJudgeWB, false, true, "NG");
                }
            }
            else
            {
                info2Log("WB测试失败");
                ChangeJudgeLable(labelJudgeWB, false, true, "未测试");
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked==true)
            {
                colorT = 1;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked==true)
            {
                colorT = 2;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked==true)
            {
                colorT = 0;
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            string stationName;
            switch (colorT)
            {
                case 0:
                    label28001.Text = "0";
                    label28002.Text = "0";
                    label28003.Text = "0";
                    label28004.Text = "0";
                    label28005.Text = "0";
                    stationName = "WB-2800测试";
                    break;
                case 1:
                    label40001.Text = "0";
                    label40002.Text = "0";
                    label40003.Text = "0";
                    label40004.Text = "0";
                    label40005.Text = "0";
                    stationName = "WB-4000测试";
                    break;
                case 2:
                    label55001.Text = "0";
                    label55002.Text = "0";
                    label55003.Text = "0";
                    label55004.Text = "0";
                    label55005.Text = "0";
                    stationName = "WB-5500测试";
                    break;
                default:
                    stationName = null;
                    break;
            }

            info2Log("WB测试开始");
            string addrs = InitPathF("NST_CameraImageTest.ini");

            string ss = imagedir + "\\" + stationName + ".bmp";
            StationWBTest WB = new StationWBTest(new FormTest(), stationName, textBox1, comboBoxProType.SelectedItem.ToString(),
                textBoxProSN.Text, ss, Convert.ToInt32(textBoxWBW.Text), Convert.ToInt32(textBoxWBH.Text), colorT);
            bool b = WB.Test1();
            if (b)
            {
                switch (colorT)
                {
                    case 0:
                        label28001.Text = WB.RG.ToString("f2");
                        label28002.Text = WB.BG.ToString("f2");
                        label28003.Text = WB.R.ToString("f2");
                        label28004.Text = WB.G.ToString("f2");
                        label28005.Text = WB.B.ToString("f2");
                        info2Log("WB测试2800K");
                        break;
                    case 1:
                        label40001.Text = WB.RG.ToString("f2");
                        label40002.Text = WB.BG.ToString("f2");
                        label40003.Text = WB.R.ToString("f2");
                        label40004.Text = WB.G.ToString("f2");
                        label40005.Text = WB.B.ToString("f2");
                        info2Log("WB测试4000K");
                        break;
                    case 2:
                        label55001.Text = WB.RG.ToString("f2");
                        label55002.Text = WB.BG.ToString("f2");
                        label55003.Text = WB.R.ToString("f2");
                        label55004.Text = WB.G.ToString("f2");
                        label55005.Text = WB.B.ToString("f2");
                        info2Log("WB测试5500K");
                        break;
                    default:
                        break;
                }

                b = b && (double.Parse(textBoxRGUpL.Text) >= WB.RG) && (double.Parse(textBoxBGUpL.Text) >= WB.BG);
                if (b)
                {
                    ChangeJudgeLable(labelJudgeWB, true, true, "OK");
                }
                else
                {
                    ChangeJudgeLable(labelJudgeWB, false, true, "NG");
                }
            }
            else
            {
                info2Log("WB测试失败");
                ChangeJudgeLable(labelJudgeWB, false, true, "未测试");
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            string stationName;
            switch (colorT)
            {
                case 0:
                    label28001.Text = "0";
                    label28002.Text = "0";
                    label28003.Text = "0";
                    label28004.Text = "0";
                    label28005.Text = "0";
                    stationName = "WB-2800测试";
                    break;
                case 1:
                    label40001.Text = "0";
                    label40002.Text = "0";
                    label40003.Text = "0";
                    label40004.Text = "0";
                    label40005.Text = "0";
                    stationName = "WB-4000测试";
                    break;
                case 2:
                    label55001.Text = "0";
                    label55002.Text = "0";
                    label55003.Text = "0";
                    label55004.Text = "0";
                    label55005.Text = "0";
                    stationName = "WB-5500测试";
                    break;
                default:
                    stationName = null;
                    break;
            }

            info2Log("WB测试开始");
            string addrs = InitPathF("NST_CameraImageTest.ini");
            OpenFileDialog fs = new OpenFileDialog();
            //fs.InitialDirectory = "D:\\TestImage";
            if (fs.ShowDialog() == DialogResult.OK)
            {
                string ss = fs.FileName;
                //string ss = imagedir + "\\" + stationName + ".bmp";
                StationWBTest WB = new StationWBTest(new FormTest(), stationName, textBox1, comboBoxProType.SelectedItem.ToString(), 
                    textBoxProSN.Text, ss, Convert.ToInt32(textBoxWBW.Text), Convert.ToInt32(textBoxWBH.Text), colorT);
                bool b = WB.Test1();
                if (b)
                {
                    switch (colorT)
                    {
                        case 0:
                            label28001.Text = WB.RG.ToString("f2");
                            label28002.Text = WB.BG.ToString("f2");
                            label28003.Text = WB.R.ToString("f2");
                            label28004.Text = WB.G.ToString("f2");
                            label28005.Text = WB.B.ToString("f2");
                            info2Log("WB测试2800K");
                            break;
                        case 1:
                            label40001.Text = WB.RG.ToString("f2");
                            label40002.Text = WB.BG.ToString("f2");
                            label40003.Text = WB.R.ToString("f2");
                            label40004.Text = WB.G.ToString("f2");
                            label40005.Text = WB.B.ToString("f2");
                            info2Log("WB测试4000K");
                            break;
                        case 2:
                            label55001.Text = WB.RG.ToString("f2");
                            label55002.Text = WB.BG.ToString("f2");
                            label55003.Text = WB.R.ToString("f2");
                            label55004.Text = WB.G.ToString("f2");
                            label55005.Text = WB.B.ToString("f2");
                            info2Log("WB测试5500K");
                            break;
                        default:
                            break;
                    }

                    b = b && (double.Parse(textBoxRGUpL.Text) >= WB.RG) && (double.Parse(textBoxBGUpL.Text) >= WB.BG);
                    if (b)
                    {
                        ChangeJudgeLable(labelJudgeWB, true, true, "OK");
                    }
                    else
                    {
                        ChangeJudgeLable(labelJudgeWB, false, true, "NG");
                    }
                }
                else
                {
                    info2Log("WB测试失败");
                    ChangeJudgeLable(labelJudgeWB, false, true, "未测试");
                }
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            label50.Text = "";
            if (RlgPlatInterface.GetInstance()!=null)
            {
                double c = 0;
                //bool b=listRlgPlatform[cam].GetBoxTotalCurrent(ref c);
                uint i= listRlgPlatform[cam].GetIICSpeed();
                label50.Text = i.ToString();
            }
            


        }

        private void button20_Click(object sender, EventArgs e)
        {
            try
            {
                label51.Text = "";
                if (RlgPlatInterface.GetInstance() != null)
                {
                    //double c = 0;
                    //bool b=listRlgPlatform[cam].GetBoxTotalCurrent(ref c);
                    //bool a = listRlgPlatform[cam].SetSensorIICConnectType();
                    //bool b = listRlgPlatform[cam].ConnectSensorIIC(true);
                    byte slave = Convert.ToByte(textBoxSlaveID.Text, 16);
                    uint reg = Convert.ToUInt32(textBoxReg.Text, 16);
                    long data = 9999;
                    ushort type = Convert.ToUInt16(textBoxReadType.Text, 16);
                    bool b = listRlgPlatform[cam].ReadIIC(slave, reg, ref data, type);
                    if (b == true)
                    {
                        label51.Text = data.ToString("x4");
                    }
                    else
                    {
                        label51.Text = "没读到";
                    }
                }
            }
            catch (Exception)
            {

                label51.Text = "出错了";
            }
            
        }



        private void button21_Click(object sender, EventArgs e)
        {
            try
            {
                if (RlgPlatInterface.GetInstance() !=null)
                {
                    byte slave = Convert.ToByte(textBoxSlaveID.Text, 16);
                    uint reg = Convert.ToUInt32(textBoxReg.Text, 16);
                    ulong data = Convert.ToUInt64(textBoxWriteContext.Text, 16);
                    ushort type = Convert.ToUInt16(textBoxReadType.Text, 16);
                    bool ask = true;
                    bool b = listRlgPlatform[cam].WriteIIC(slave, reg, data, type, ask);
                    if (b)
                    {
                        textBoxShowMW.AppendText("单次写入成功\r\n");
                    }
                    else
                    {
                        textBoxShowMW.AppendText("单次写入失败\r\n");
                    }
                }
            }
            catch (Exception)
            {
                textBoxShowMW.AppendText("单次写入报错\r\n");

            }
            
        }

        private void tabPageMES_Click(object sender, EventArgs e)
        {

        }

        private void button24_Click(object sender, EventArgs e)
        {
           
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            if (checkBoxmes.Checked)
            {
                bool b = mes.Connect();
                Info2LogMES(mes.infoFromMes);

                if (b)
                {
                    button24.BackColor = Color.Green;
                    button24.Text = "已经连接";
                }
                else
                {
                    button24.BackColor = Color.Red;
                    button24.Text = "未连接";
                }
            }
            else
            {
                button24.BackColor= Color.Red;
                button24.Text = "未连接";
            }
        }

        private void button25_Click(object sender, EventArgs e)
        {
            textBoxToMES.Clear();
        }

        private void button25_Click_1(object sender, EventArgs e)
        {
            JsonCreater jc = new JsonCreater();
            jc.Show();
        }

        private void button26_Click(object sender, EventArgs e)
        {
            textBoxFromMes.Clear();
        }
    }
    
}

