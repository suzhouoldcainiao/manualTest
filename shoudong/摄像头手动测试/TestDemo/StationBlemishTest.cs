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
    class StationBlemishTest:StationBase
    {
        public  BlemishInfo ThisBlemishInfo = new BlemishInfo();
        public double blCount;
        public StationBlemishTest(FormTest formtest, string stationName, TextBox textbox, string type, string sn, string imageDIR) : base(formtest, stationName, textbox, type, sn, imageDIR)
        {

        }
        public override bool Test()
        {
            try
            {
                
                LoadImage();
                string NGCutScreenImgPath = "D:\\TestImage\\NGCutScreenImgPath\\" + "BL-" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".bmp";
                HTuple hv_countNg;
                HObject NGRegions;
                BlemishTest(NGCutScreenImgPath, win, bmp, ref ThisBlemishInfo, out hv_countNg, out NGRegions);
                blCount = hv_countNg.D;
                return true;
            }
            catch (Exception e)
            {

                return false;
            }
        }
        public void BlemishTest(string NGCutScreenImgPath, HTuple WhiteWHandel, Bitmap Whitetestbitmap, ref BlemishInfo blemishInfo, out HTuple hv_countNg, out HObject NGRegions)
        {
            HObject ho_Image3 = null;
            HTuple AreaMax = new HTuple(), maxRow = new HTuple(), maxCol = new HTuple();
            HTuple smallCount = new HTuple(), mediumCount = new HTuple(), bigCount = new HTuple();
            Bitmap2HObject1(Whitetestbitmap, ref ho_Image3);
            blemishInfo = new BlemishInfo();
            //下面这组数据要稍后确定
            //int sigmamax=0;
            //int sigmamin=0;
            //int blthreshold = 0;
            int blThreshold =30;
            int blArea = 40;
            int SigmaMax = 21;
            int SigmaMin = 10;
            //string NGCutScreenImgPath = PathHelper.ImageWhiteTestPath(BStationNum) + manager.Products[BStationNum].SN + "-BL-" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".bmp";
            WhiteTestByFreq(ho_Image3, SigmaMax, SigmaMin, blThreshold, NGCutScreenImgPath, WhiteWHandel, blArea, ref blemishInfo, out hv_countNg, out NGRegions);
            ShowBlemish(NGCutScreenImgPath, WhiteWHandel, NGRegions);

            //UserTest.Result[Socketnum].BlemishNGCount = blemishInfo.blResult.nBL_Count;
            //UserTest.Result[Socketnum].BlemishResult = blemishInfo.blResult.bBL_Res;
        }
        public  void WhiteTestByFreq(HObject ho_Image, int sigmaMax, int sigmaMin, HTuple hv_ThresoldRegion, HTuple hv_strPath,
HTuple hv_HandWnd, HTuple hv_MinArea, ref ImageAlgorithm.BlemishInfo blemishInfo, out HTuple hv_countNg, out HObject NGRegions)
        {

            HObject ho_ImageOut = null, ho_GrayImage, ho_Region1;
            HObject ho_RegionErosion1, ho_ConnectedRegions, ho_SelectedRegions2;
            HObject ho_RegionClosing, ho_RegionErosion, ho_RegionFillUp;
            HObject ho_Image1, ho_GaussFilter1, ho_GaussFilter2, ho_Filter;
            HObject ho_ImageFFT, ho_ImageConvol, ho_ImageFiltered, ho_ImageScaled;
            HObject ho_Region, ho_SelectedRegions, ho_RegionOpening;
            HObject ho_RegionIntersection, ho_RegionOpening1, ho_ConnectedRegions1;
            HObject ho_SelectedRegions1, smallRegion, mediumRegion, bigRegion;
            NGRegions = null;

            // Local control variables 

            HTuple hv_Width = null, hv_Height = null, hv_Sigma1 = null;
            HTuple hv_Sigma2 = null, area = new HTuple(), row = new HTuple(), col = new HTuple(), index = null, sortArea = null;
            HTuple smallRegionCount = new HTuple(), mediumRegionCount = new HTuple(), bigRegionCount = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageOut);
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_Region1);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion1);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions2);
            HOperatorSet.GenEmptyObj(out ho_RegionClosing);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_Image1);
            HOperatorSet.GenEmptyObj(out ho_GaussFilter1);
            HOperatorSet.GenEmptyObj(out ho_GaussFilter2);
            HOperatorSet.GenEmptyObj(out ho_Filter);
            HOperatorSet.GenEmptyObj(out ho_ImageFFT);
            HOperatorSet.GenEmptyObj(out ho_ImageConvol);
            HOperatorSet.GenEmptyObj(out ho_ImageFiltered);
            HOperatorSet.GenEmptyObj(out ho_ImageScaled);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_RegionIntersection);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening1);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            HOperatorSet.GenEmptyObj(out smallRegion);
            HOperatorSet.GenEmptyObj(out mediumRegion);
            HOperatorSet.GenEmptyObj(out bigRegion);

            hv_countNg = 0;
            area = 0;
            row = 0;
            col = 0;
            try
            {
                //HOperatorSet.SetDraw(hv_HandWnd, "margin");
                //HOperatorSet.ClearWindow(hv_HandWnd);
                HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
                //HOperatorSet.SetPart(hv_HandWnd, 0, 0, hv_Height, hv_Width);
                // HOperatorSet.SetColor(hv_HandWnd, "red");

                ho_ImageOut.Dispose();
                ho_ImageOut = ho_Image.CopyObj(1, -1);

                ho_GrayImage.Dispose();
                HOperatorSet.Rgb1ToGray(ho_ImageOut, out ho_GrayImage);
                ho_Region1.Dispose();
                HOperatorSet.Threshold(ho_GrayImage, out ho_Region1, hv_ThresoldRegion, 255);
                ho_RegionErosion1.Dispose();
                HOperatorSet.ErosionCircle(ho_Region1, out ho_RegionErosion1, 1);  //djx 0712
                //HOperatorSet.ErosionCircle(ho_Region1, out ho_RegionErosion1, 2);  //djx 0712
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_RegionErosion1, out ho_ConnectedRegions);
                ho_SelectedRegions2.Dispose();
                HOperatorSet.SelectShapeStd(ho_ConnectedRegions, out ho_SelectedRegions2, "max_area",
                    70);
                ho_RegionClosing.Dispose();
                HOperatorSet.ClosingRectangle1(ho_SelectedRegions2, out ho_RegionClosing, 520,
                    520);
                ho_RegionErosion.Dispose();
                HOperatorSet.ErosionCircle(ho_RegionClosing, out ho_RegionErosion, 4.5);
                ho_RegionFillUp.Dispose();
                HOperatorSet.FillUp(ho_RegionErosion, out ho_RegionFillUp);
                ho_Image1.Dispose();
                HOperatorSet.ReduceDomain(ho_ImageOut, ho_RegionErosion, out ho_Image1);
                //read_image (ImageOut, 'E:/项目/镜头白场测试/image.bmp')

                hv_Sigma1 = sigmaMax;
                hv_Sigma2 = sigmaMin;

                //*在频域上产生高斯滤波器
                ho_GaussFilter1.Dispose();
                HOperatorSet.GenGaussFilter(out ho_GaussFilter1, hv_Sigma1, hv_Sigma1, 0.0, "none",
                    "rft", hv_Width, hv_Height);
                ho_GaussFilter2.Dispose();
                HOperatorSet.GenGaussFilter(out ho_GaussFilter2, hv_Sigma2, hv_Sigma2, 0.0, "none",
                    "rft", hv_Width, hv_Height);
                ho_Filter.Dispose();
                HOperatorSet.SubImage(ho_GaussFilter1, ho_GaussFilter2, out ho_Filter, 1, 0);

                ho_GrayImage.Dispose();
                HOperatorSet.Rgb1ToGray(ho_Image1, out ho_GrayImage);
                ho_ImageFFT.Dispose();
                HOperatorSet.RftGeneric(ho_GrayImage, out ho_ImageFFT, "to_freq", "none", "complex",
                    hv_Width);
                ho_ImageConvol.Dispose();
                HOperatorSet.ConvolFft(ho_ImageFFT, ho_Filter, out ho_ImageConvol);
                ho_ImageFiltered.Dispose();
                HOperatorSet.RftGeneric(ho_ImageConvol, out ho_ImageFiltered, "from_freq", "n",
                    "real", hv_Width);
                ho_ImageScaled.Dispose();
                scale_image_range(ho_ImageFiltered, out ho_ImageScaled, 0, 255);
                ho_Region.Dispose();
                HOperatorSet.Threshold(ho_ImageScaled, out ho_Region, 2, 255);

                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_Region, out ho_ConnectedRegions);
                ho_SelectedRegions.Dispose();
                HOperatorSet.SelectShapeProto(ho_ConnectedRegions, ho_ConnectedRegions, out ho_SelectedRegions,
                    "distance_contour", 1, 999);
                ho_RegionOpening.Dispose();
                HOperatorSet.OpeningCircle(ho_SelectedRegions, out ho_RegionOpening, 7.5);


                //dev_display (ImageOut)
                //dev_display (Region)
                ho_RegionIntersection.Dispose();
                HOperatorSet.Intersection(ho_RegionErosion, ho_Region, out ho_RegionIntersection
                    );
                ho_RegionOpening1.Dispose();
                //HOperatorSet.OpeningRectangle1(ho_RegionIntersection, out ho_RegionOpening1,
                //    1, 5);
                HOperatorSet.OpeningRectangle1(ho_RegionIntersection, out ho_RegionOpening1,
                    10, 10);  //djx0712

                hv_countNg = 0;
                ho_ConnectedRegions1.Dispose();
                HOperatorSet.Connection(ho_RegionOpening1, out ho_ConnectedRegions1);
                ho_SelectedRegions1.Dispose();
                HOperatorSet.SelectShape(ho_ConnectedRegions1, out ho_SelectedRegions1, "area",
                    "and", hv_MinArea, 5000000);

                HOperatorSet.Union1(ho_SelectedRegions1, out NGRegions);
                HOperatorSet.CountObj(ho_SelectedRegions1, out hv_countNg);
                //HOperatorSet.DispObj(ho_Image, hv_HandWnd);

                ThisBlemishInfo.blResult = new ImageAlgorithm._BLResult();
                if (hv_countNg > 0)
                {
                    ThisBlemishInfo.blResult.bBL_Res = false;
                    ThisBlemishInfo.blResult.nBL_Count = hv_countNg[0].I;
                }
                else
                {
                    ThisBlemishInfo.blResult.bBL_Res = true;
                    ThisBlemishInfo.blResult.nBL_Count = 0;
                }
                blemishInfo = ThisBlemishInfo;
                #region 增加脏污的面积大小和坐标

                //if (hv_countNg > 0)
                //{
                //    //HOperatorSet.DispObj(ho_SelectedRegions1, hv_HandWnd);
                //    HOperatorSet.AreaCenter(ho_SelectedRegions1, out area, out row, out col);
                //    //这里要区分大中小
                //    smallRegion.Dispose();
                //    HOperatorSet.SelectShape(ho_SelectedRegions1, out smallRegion, "area", "and", hv_MinArea, MyManager.GetInstace().productConfig.blSmallArea);
                //    HOperatorSet.CountObj(smallRegion, out smallCount);
                //    mediumRegion.Dispose();
                //    HOperatorSet.SelectShape(ho_SelectedRegions1, out mediumRegion, "area", "and", MyManager.GetInstace().productConfig.blSmallArea, MyManager.GetInstace().productConfig.blBigArea);
                //    HOperatorSet.CountObj(mediumRegion, out mediumCount);
                //    bigRegion.Dispose();
                //    HOperatorSet.SelectShape(ho_SelectedRegions1, out bigRegion, "area", "and", MyManager.GetInstace().productConfig.blBigArea, 99999999);
                //    HOperatorSet.CountObj(bigRegion, out bigCount);

                //    //sortArea = 0;
                //    //HOperatorSet.TupleSort(area, out sortArea);
                //    //HOperatorSet.TupleInverse(sortArea, out sortArea);
                //    //if (hv_countNg == 1)
                //    //{
                //    //    HOperatorSet.TupleMax(area, out AreaMax);
                //    //    HOperatorSet.TupleFind(area, AreaMax, out index);
                //    //    HOperatorSet.TupleSelect(row, index, out maxRow);
                //    //    HOperatorSet.TupleSelect(col, index, out maxCol);
                //    //}
                //    //else if (hv_countNg == 2)
                //    //{
                //    //    HOperatorSet.TupleFind(area, sortArea[0], out index);
                //    //    HOperatorSet.TupleSelect(row, index, out HTuple maxRow1);
                //    //    HOperatorSet.TupleSelect(col, index, out HTuple maxCol1);

                //    //    HOperatorSet.TupleFind(area, sortArea[1], out index);
                //    //    HOperatorSet.TupleSelect(row, index, out HTuple maxRow2);
                //    //    HOperatorSet.TupleSelect(col, index, out HTuple maxCol2);

                //    //    HOperatorSet.TupleConcat(sortArea[0], sortArea[1], out AreaMax);
                //    //    //HOperatorSet.TupleConcat(AreaMax, sortArea[1], out AreaMax);

                //    //    HOperatorSet.TupleConcat(maxRow1, maxRow2, out maxRow);
                //    //    //HOperatorSet.TupleConcat(maxRow, maxRow2, out maxRow);

                //    //    HOperatorSet.TupleConcat(maxCol1, maxCol2, out maxCol);
                //    //    //HOperatorSet.TupleConcat(maxCol, maxCol2, out maxCol);

                //    //}
                //    //else
                //    //{
                //    //    HOperatorSet.TupleFind(area, sortArea[0], out index);
                //    //    HOperatorSet.TupleSelect(row, index, out HTuple maxRow1);
                //    //    HOperatorSet.TupleSelect(col, index, out HTuple maxCol1);

                //    //    HOperatorSet.TupleFind(area, sortArea[1], out index);
                //    //    HOperatorSet.TupleSelect(row, index, out HTuple maxRow2);
                //    //    HOperatorSet.TupleSelect(col, index, out HTuple maxCol2);

                //    //    HOperatorSet.TupleFind(area, sortArea[2], out index);
                //    //    HOperatorSet.TupleSelect(row, index, out HTuple maxRow3);
                //    //    HOperatorSet.TupleSelect(col, index, out HTuple maxCol3);

                //    //    //HOperatorSet.TupleConcat(AreaMax, sortArea[0], out AreaMax);
                //    //    //HOperatorSet.TupleConcat(AreaMax, sortArea[1], out AreaMax);
                //    //    //HOperatorSet.TupleConcat(AreaMax, sortArea[2], out AreaMax);
                //    //    HOperatorSet.TupleConcat(sortArea[0], sortArea[1], out AreaMax);
                //    //    HOperatorSet.TupleConcat(AreaMax, sortArea[2], out AreaMax);

                //    //    HOperatorSet.TupleConcat(maxRow1, maxRow2, out maxRow);
                //    //    //HOperatorSet.TupleConcat(maxRow, maxRow2, out maxRow);
                //    //    HOperatorSet.TupleConcat(maxRow, maxRow3, out maxRow);

                //    //    HOperatorSet.TupleConcat(maxCol1, maxCol2, out maxCol);
                //    //    //HOperatorSet.TupleConcat(maxCol, maxCol2, out maxCol);
                //    //    HOperatorSet.TupleConcat(maxCol, maxCol3, out maxCol);
                //    //}
                //}
                //else
                //{
                //    AreaMax = area;
                //    maxRow = row;
                //    maxCol = col;
                //}
                #endregion



                //    if ((int)((new HTuple(hv_strPath.TupleNotEqual(""))).TupleAnd(new HTuple(hv_HandWnd.TupleNotEqual(
                //    0)))) != 0)
                //{
                //    ho_Image1.Dispose();
                //    HOperatorSet.DumpWindowImage(out ho_Image1, hv_HandWnd);
                //    HOperatorSet.WriteImage(ho_Image1, "bmp", 0, hv_strPath);
                //}
            }
            catch (HalconException e)
            {
                return;
            }
            finally
            {
                ho_ImageOut?.Dispose();
                ho_GrayImage?.Dispose();
                ho_Region1?.Dispose();
                ho_RegionErosion1?.Dispose();
                ho_ConnectedRegions?.Dispose();
                ho_SelectedRegions2?.Dispose();
                ho_RegionClosing?.Dispose();
                ho_RegionErosion?.Dispose();
                ho_RegionFillUp?.Dispose();
                ho_Image1?.Dispose();
                ho_GaussFilter1?.Dispose();
                ho_GaussFilter2?.Dispose();
                ho_Filter?.Dispose();
                ho_ImageFFT?.Dispose();
                ho_ImageConvol?.Dispose();
                ho_ImageFiltered?.Dispose();
                ho_ImageScaled?.Dispose();
                ho_Region?.Dispose();
                ho_SelectedRegions?.Dispose();
                ho_RegionOpening?.Dispose();
                ho_RegionIntersection?.Dispose();
                ho_RegionOpening1?.Dispose();
                ho_ConnectedRegions1?.Dispose();
                ho_SelectedRegions1?.Dispose();
            }



            return;
        }

        public  void scale_image_range(HObject ho_Image, out HObject ho_ImageScaled, HTuple hv_Min,
          HTuple hv_Max)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_SelectedChannel = null, ho_LowerRegion = null;
            HObject ho_UpperRegion = null;

            // Local copy input parameter variables 
            HObject ho_Image_COPY_INP_TMP;
            ho_Image_COPY_INP_TMP = ho_Image.CopyObj(1, -1);



            // Local control variables 

            HTuple hv_LowerLimit = new HTuple(), hv_UpperLimit = new HTuple();
            HTuple hv_Mult = null, hv_Add = null, hv_Channels = null;
            HTuple hv_Index = null, hv_MinGray = new HTuple(), hv_MaxGray = new HTuple();
            HTuple hv_Range = new HTuple();
            HTuple hv_Max_COPY_INP_TMP = hv_Max.Clone();
            HTuple hv_Min_COPY_INP_TMP = hv_Min.Clone();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageScaled);
            HOperatorSet.GenEmptyObj(out ho_SelectedChannel);
            HOperatorSet.GenEmptyObj(out ho_LowerRegion);
            HOperatorSet.GenEmptyObj(out ho_UpperRegion);

            try
            {
                if ((int)(new HTuple((new HTuple(hv_Min_COPY_INP_TMP.TupleLength())).TupleEqual(
              2))) != 0)
                {
                    hv_LowerLimit = hv_Min_COPY_INP_TMP[1];
                    hv_Min_COPY_INP_TMP = hv_Min_COPY_INP_TMP[0];
                }
                else
                {
                    hv_LowerLimit = 0.0;
                }
                if ((int)(new HTuple((new HTuple(hv_Max_COPY_INP_TMP.TupleLength())).TupleEqual(
                    2))) != 0)
                {
                    hv_UpperLimit = hv_Max_COPY_INP_TMP[1];
                    hv_Max_COPY_INP_TMP = hv_Max_COPY_INP_TMP[0];
                }
                else
                {
                    hv_UpperLimit = 255.0;
                }
                //
                //Calculate scaling parameters
                hv_Mult = (((hv_UpperLimit - hv_LowerLimit)).TupleReal()) / (hv_Max_COPY_INP_TMP - hv_Min_COPY_INP_TMP);
                hv_Add = ((-hv_Mult) * hv_Min_COPY_INP_TMP) + hv_LowerLimit;
                //
                //Scale image
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ScaleImage(ho_Image_COPY_INP_TMP, out ExpTmpOutVar_0, hv_Mult, hv_Add);
                    ho_Image_COPY_INP_TMP.Dispose();
                    ho_Image_COPY_INP_TMP = ExpTmpOutVar_0;
                }
                //
                //Clip gray values if necessary
                //This must be done for each channel separately
                HOperatorSet.CountChannels(ho_Image_COPY_INP_TMP, out hv_Channels);
                HTuple end_val48 = hv_Channels;
                HTuple step_val48 = 1;
                for (hv_Index = 1; hv_Index.Continue(end_val48, step_val48); hv_Index = hv_Index.TupleAdd(step_val48))
                {
                    ho_SelectedChannel.Dispose();
                    HOperatorSet.AccessChannel(ho_Image_COPY_INP_TMP, out ho_SelectedChannel, hv_Index);
                    HOperatorSet.MinMaxGray(ho_SelectedChannel, ho_SelectedChannel, 0, out hv_MinGray,
                        out hv_MaxGray, out hv_Range);
                    ho_LowerRegion.Dispose();
                    HOperatorSet.Threshold(ho_SelectedChannel, out ho_LowerRegion, ((hv_MinGray.TupleConcat(
                        hv_LowerLimit))).TupleMin(), hv_LowerLimit);
                    ho_UpperRegion.Dispose();
                    HOperatorSet.Threshold(ho_SelectedChannel, out ho_UpperRegion, hv_UpperLimit,
                        ((hv_UpperLimit.TupleConcat(hv_MaxGray))).TupleMax());
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.PaintRegion(ho_LowerRegion, ho_SelectedChannel, out ExpTmpOutVar_0,
                            hv_LowerLimit, "fill");
                        ho_SelectedChannel.Dispose();
                        ho_SelectedChannel = ExpTmpOutVar_0;
                    }
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.PaintRegion(ho_UpperRegion, ho_SelectedChannel, out ExpTmpOutVar_0,
                            hv_UpperLimit, "fill");
                        ho_SelectedChannel.Dispose();
                        ho_SelectedChannel = ExpTmpOutVar_0;
                    }
                    if ((int)(new HTuple(hv_Index.TupleEqual(1))) != 0)
                    {
                        ho_ImageScaled.Dispose();
                        HOperatorSet.CopyObj(ho_SelectedChannel, out ho_ImageScaled, 1, 1);
                    }
                    else
                    {
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.AppendChannel(ho_ImageScaled, ho_SelectedChannel, out ExpTmpOutVar_0
                                );
                            ho_ImageScaled.Dispose();
                            ho_ImageScaled = ExpTmpOutVar_0;
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                ho_Image_COPY_INP_TMP?.Dispose();
                ho_SelectedChannel?.Dispose();
                ho_LowerRegion?.Dispose();
                ho_UpperRegion?.Dispose();

            }


            return;
        }
        public void ShowBlemish(string picPath, HTuple hwindow, HObject obj)
        {
            try
            {
                
                
                    HObject img = null;
                    HOperatorSet.GenEmptyObj(out img);
                    img.Dispose();
                    HOperatorSet.SetDraw(hwindow, "margin");
                    HOperatorSet.SetColor(hwindow, "red");
                    HOperatorSet.SetLineWidth(hwindow, 10);
                    if (obj != null)
                    {
                        HOperatorSet.DispObj(obj, hwindow);
                    }
                    HOperatorSet.DumpWindowImage(out img, hwindow);
                    HOperatorSet.WriteImage(img, "bmp", 0, picPath);
                    img.Dispose();
                
            }
            catch (HalconException ex)
            {

            }
        }
    }
}
