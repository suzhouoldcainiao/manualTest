using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;

namespace TestDemo
{
    public class StationOcTest:StationBase
    {
        public double dW, dH;
        public double centerW, centerH;
        public int textBoxW, textBoxH;
        public string darkORLight;
        public StationOcTest(FormTest formtest, string stationName, TextBox textbox, string type, string sn,int textboxW,int textboxH,string imageDIR) : base(formtest, stationName, textbox, type, sn,imageDIR)
        {
            textBoxW = textboxW;
            textBoxH = textboxH;
        }
        public override bool Test()
        {
            LoadImage();
            SavePicture(pictureSavePath, win);
            
            DrawCrossLine(win, width, height);
            
            if (OpticalCenterTest1(imagedir, darkORLight, ref dW, ref dH, ref centerW,ref centerH, textBoxW, textBoxH))
            {
                DrawRectAndCross(win, centerW, centerH, textBoxW, textBoxH);
                SavePicture(pictureSavePath, win);
                return true;
            }
            
            return false;
        }
        public bool OpticalCenterTest(Bitmap bitmap, ref double dW, ref double dH,ref double CW,ref double CH, int textboxW,int textboxH)
        {


            dW = 999; dH = 999; bool result = true;

            HObject ho_Image = null, ho_GrayImage = null; HTuple hv_Area0 = new HTuple();
            try
            {
                Bitmap2HObject1(bitmap, ref ho_Image);
                //BitmapToHobject3 (bitmap, out ho_Image);//@@@
                if (ho_Image == null)
                {

                    return false;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
            try
            {

                HObject ho_SelectedRegions0 = null, Rect1 = null, imageReduce = null, Region = null;
                HObject connectedRegions = null, regionColsing = null;

                HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
                HTuple hv_Row0 = new HTuple(), hv_Column0 = new HTuple();
                HObject ho_Region;
                HTuple hv_UsedThreshold;

                HOperatorSet.Rgb1ToGray(ho_Image, out ho_GrayImage);
                HOperatorSet.GetImageSize(ho_GrayImage, out hv_Width, out hv_Height);




                
                try
                {

                    HTuple row1 = ((bitmap.Height) - textboxH) / 2;//Convert.ToInt32(.Text)) / 2;
                    HTuple column1 = ((bitmap.Width) - textboxW) / 2;//Convert.ToInt32(textBox1.Text)) / 2;
                    HTuple row2 = ((bitmap.Height) + textboxH) / 2;//Convert.ToInt32(textBox2.Text)) / 2;
                    HTuple column2 = ((bitmap.Width) + textboxW) / 2;//Convert.ToInt32(textBox1.Text)) / 2;

                    HOperatorSet.GenRectangle1(out Rect1, row1, column1, row2, column2);

                    HOperatorSet.ReduceDomain(ho_GrayImage, Rect1, out imageReduce);
                    HOperatorSet.BinaryThreshold(imageReduce, out ho_Region, "max_separability", "dark", out hv_UsedThreshold);
                    //HOperatorSet.BinaryThreshold(imageReduce, out ho_Region, "max_separability", "light", out hv_UsedThreshold);
                    HOperatorSet.ClosingCircle(ho_Region, out regionColsing, 3);
                    HOperatorSet.Connection(regionColsing, out connectedRegions);
                    HOperatorSet.SelectShapeStd(connectedRegions, out ho_SelectedRegions0, "max_area", 70);
                }
                catch (Exception ex)
                {
                    return false;
                }
                HOperatorSet.AreaCenter(ho_SelectedRegions0, out hv_Area0, out hv_Row0, out hv_Column0);
                //dx3 = (hv_Column0.D - hv_Width / 2) * pixelSize / 1000.0;
                //dy3 = (hv_Row0.D - hv_Height / 2) * pixelSize / 1000.0;
                //drawRect(win, hv_Column0.D, hv_Row0.D, Convert.ToDouble(textBox1.Text) / 2, Convert.ToDouble(textBox2.Text) / 2);
                //label13.Text = (hv_Column0.D - hv_Width / 2).ToString();
                //label14.Text = (hv_Row0.D - hv_Height / 2).ToString();
                CW = hv_Column0.D;
                CH = hv_Row0.D;
                dW = hv_Column0.D-hv_Width/2;
                dH = hv_Row0.D-hv_Height/2;
            }
            catch (Exception e)
            {

                return false;
            }
            return true;
        }
        public bool OpticalCenterTest1(string imagedir,string darkORlight, ref double dW, ref double dH, ref double CW, ref double CH, int textboxW, int textboxH)
        {

           
            // Local iconic variables 

            HObject ho_Image, ho_GrayImage, ho_Rectangle;
            HObject ho_ImageReduced, ho_Region, ho_RegionClosing, ho_ConnectedRegions;
            HObject ho_SelectedRegions;

            // Local control variables 

            HTuple hv_Width = null, hv_Height = null, hv_UsedThreshold = null;
            HTuple hv_Area = null, hv_Row = null, hv_Column = null;
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionClosing);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            ho_Image.Dispose();
            HOperatorSet.ReadImage(out ho_Image, imagedir);
            ho_GrayImage.Dispose();
            HOperatorSet.Rgb1ToGray(ho_Image, out ho_GrayImage);
            HOperatorSet.GetImageSize(ho_GrayImage, out hv_Width, out hv_Height);
            ho_Rectangle.Dispose();

            HTuple row1 = (hv_Height - textboxH) / 2;//Convert.ToInt32(.Text)) / 2;
            HTuple column1 = (hv_Width - textboxW) / 2;//Convert.ToInt32(textBox1.Text)) / 2;
            HTuple row2 = (hv_Height + textboxH) / 2;//Convert.ToInt32(textBox2.Text)) / 2;
            HTuple column2 = (hv_Width + textboxW) / 2;//Convert.ToInt32(textBox1.Text)) / 2;

            //HOperatorSet.GenRectangle1(out ho_Rectangle, 840, 1700, 1280, 2230);
            HOperatorSet.GenRectangle1(out ho_Rectangle, row1, column1, row2, column2);

            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_GrayImage, ho_Rectangle, out ho_ImageReduced);
            ho_Region.Dispose();
            HOperatorSet.BinaryThreshold(ho_ImageReduced, out ho_Region, "max_separability",
                darkORlight, out hv_UsedThreshold);
            ho_RegionClosing.Dispose();
            HOperatorSet.ClosingCircle(ho_Region, out ho_RegionClosing, 3);
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_RegionClosing, out ho_ConnectedRegions);
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShapeStd(ho_ConnectedRegions, out ho_SelectedRegions, "max_area",
                70);
            HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row, out hv_Column);


            CW = hv_Column.D;
            CH = hv_Row.D;
            dW = hv_Column.D - hv_Width / 2;
            dH = hv_Row.D - hv_Height / 2;
            return true;

            ho_Image.Dispose();
            ho_GrayImage.Dispose();
            ho_Rectangle.Dispose();
            ho_ImageReduced.Dispose();
            ho_Region.Dispose();
            ho_RegionClosing.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
        }
    }
}
