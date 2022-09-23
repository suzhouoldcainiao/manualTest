using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;
using System.Xml;
using System.Drawing;

namespace TestDemo
{
    public class StationColorAndGray:StationBase
    {
        //double[] row1 = new double[6];
        //double[] collmn1 = new double[6];
        //double[] row2 = new double[6];
        //double[] collmn2 = new double[6];
        public List<double> rowColor1 = new List<double>();
        public List<double> columnColor1 = new List<double>();
        public List<double> rowColor2 = new List<double>();
        public List<double> columnColor2 = new List<double>();
        public List<double> rowGray1 = new List<double>();
        public List<double> columnGray1 = new List<double>();
        public List<double> rowGray2 = new List<double>();
        public List<double> columnGray2 = new List<double>();
        public List<double> meanL = new List<double>();
        public List<double> meanA = new List<double>();
        public List<double> meanB = new List<double>();
        public List<double> meanLStd = new List<double>();
        public List<double> meanAStd = new List<double>();
        public List<double> meanBStd = new List<double>();
        public List<double> meanGray = new List<double>();
        public double[] deltaE = new double[6];
        public double[] deltaGray = new double[6];

        public StationColorAndGray(FormTest formtest, string stationName, TextBox textbox, string type, string sn, string imageDIR):base(formtest,stationName, textbox, type,  sn, imageDIR)
        {

        }
        public bool StdProductTest()
        {
            LoadImage();
            for (int i = 0; i < rowColor1.Count; i++)
            {
                double r1 = rowColor1[i];
                double c1 = columnColor1[i];
                double r2 = rowColor2[i];
                double c2 = columnColor2[i];
                DrawRect(win, r1, c1, r2,  c2);
                double rr1 = rowGray1[i];
                double cc1 = columnGray1[i];
                double rr2 = rowGray2[i];
                double cc2 = columnGray2[i];
                DrawRect(win, rr1, cc1, rr2, cc2);
                double meanl = 0;
                double meana = 0;
                double meanb = 0;
                GetRoiLAB(bmp, r1, c1, r2, c2, out meanl, out meana, out meanb);
                meanL.Add(meanl);
                meanA.Add(meana);
                meanB.Add(meanb);
            }
            
            //HObject ho_Image = null;
            //Bitmap2HObject1((Bitmap)bitmap.Clone(), ref ho_Image);
            
            return true;
        }
        
        public void GetRoiLAB(Bitmap bmp,double r1,double c1,double r2,double c2,out double meanL,out double meanA,out double meanB)
        {
            HObject hImage=new HObject();
            Bitmap2HObject1(bmp, ref hImage);
            HObject R = new HObject();
            HObject G = new HObject();
            HObject B = new HObject();
            HOperatorSet.Decompose3(hImage, out R, out G, out B);
            HObject l = new HObject();
            HObject a = new HObject();
            HObject b = new HObject();
            HOperatorSet.TransFromRgb(R, G, B,out  l,out  a,out b, "cielab");
            HObject rectangle = new HObject();
            HOperatorSet.GenRectangle1(out rectangle, r1, c1, r2, c2);
            HObject imageReducedL = new HObject();
            HOperatorSet.ReduceDomain(l, rectangle, out imageReducedL);
            HTuple hdiviationL = 0;
            HTuple hmeanL = 0;
            HOperatorSet.Intensity(imageReducedL, l, out hmeanL, out hdiviationL);
            meanL = hmeanL.D;
            HObject imageReducedA = new HObject();
            HOperatorSet.ReduceDomain(a, rectangle, out imageReducedA);
            HTuple hdiviationA = 0;
            HTuple hmeanA = 0;
            HOperatorSet.Intensity(imageReducedA, a, out hmeanA, out hdiviationA);
            meanA = hmeanA.D;
            HObject imageReducedB = new HObject();
            HOperatorSet.ReduceDomain(b, rectangle, out imageReducedB);
            HTuple hdiviationB = 0;
            HTuple hmeanB = 0;
            HOperatorSet.Intensity(imageReducedB, a, out hmeanB, out hdiviationB);
            meanB = hmeanB.D;

        }
        public void ReadColorRoi(string addr)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(addr);
            XmlNodeList nods = doc.SelectNodes("Config/ROICfg/ParamSet");
            foreach (XmlNode item in nods)
            {
                if (item.Attributes["ParamName"].Value.ToString().Contains("LUROW"))
                {
                    rowColor1.Add(double.Parse(item.Attributes["ParamVal"].Value));
                }
                if (item.Attributes["ParamName"].Value.ToString().Contains("LUCOLUMN"))
                {
                    columnColor1.Add(double.Parse(item.Attributes["ParamVal"].Value));
                }
                if (item.Attributes["ParamName"].Value.ToString().Contains("RDROW"))
                {
                    rowColor2.Add(double.Parse(item.Attributes["ParamVal"].Value));
                }
                if (item.Attributes["ParamName"].Value.ToString().Contains("RDCOLUMN"))
                {
                    columnColor2.Add(double.Parse(item.Attributes["ParamVal"].Value));
                }
            }
            XmlNodeList nods1 = doc.SelectNodes("Config/GrayCfg/ParamSet");
            foreach (XmlNode item in nods1)
            {
                if (item.Attributes["ParamName"].Value.ToString().Contains("LUROW"))
                {
                    rowGray1.Add(double.Parse(item.Attributes["ParamVal"].Value));
                }
                if (item.Attributes["ParamName"].Value.ToString().Contains("LUCOLUMN"))
                {
                    columnGray1.Add(double.Parse(item.Attributes["ParamVal"].Value));
                }
                if (item.Attributes["ParamName"].Value.ToString().Contains("RDROW"))
                {
                    rowGray2.Add(double.Parse(item.Attributes["ParamVal"].Value));
                }
                if (item.Attributes["ParamName"].Value.ToString().Contains("RDCOLUMN"))
                {
                    columnGray2.Add(double.Parse(item.Attributes["ParamVal"].Value));
                }
            }
        }
        public void SetStdRoiLab(string addr)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(addr);
            }
            catch (Exception)
            {

                MessageBox.Show("颜色配置文件缺失");
            }
            XmlNodeList nods=null;

            try
            {
                nods = doc.SelectNodes("Config/StdRoiLab/ParamSet");
            }
            catch (Exception)
            {

                MessageBox.Show("颜色配置文件异常");
            }
            XmlElement xe = null;
            for (int i=0; i < nods.Count; i++)
            {
                if (nods[i].Attributes["ParamName"].Value.ToString().Contains("meanl"))
                {
                    xe = (XmlElement)nods[i];
                    xe.SetAttribute("ParamVal", meanL[i].ToString("f0"));
                    
                }
                if (nods[i].Attributes["ParamName"].Value.ToString().Contains("meana"))
                {
                    xe = (XmlElement)nods[i];
                    xe.SetAttribute("ParamVal", meanA[i-6].ToString("f0"));
                }
                if (nods[i].Attributes["ParamName"].Value.ToString().Contains("meanb"))
                {
                    xe = (XmlElement)nods[i];
                    xe.SetAttribute("ParamVal", meanB[i-12].ToString("f0"));
                }
               
            }
            doc.Save(addr);
        }
        public override bool Test()
        {
            try
            {
                LoadImage();
                SavePicture(pictureSavePath, win);
                for (int i = 0; i < rowColor1.Count; i++)
                {
                    double r1 = rowColor1[i];
                    double c1 = columnColor1[i];
                    double r2 = rowColor2[i];
                    double c2 = columnColor2[i];
                    DrawRect(win, r1, c1, r2, c2);
                    double rr1 = rowGray1[i];
                    double cc1 = columnGray1[i];
                    double rr2 = rowGray2[i];
                    double cc2 = columnGray2[i];
                    DrawRect(win, rr1, cc1, rr2, cc2);
                    double meanl = 0;
                    double meana = 0;
                    double meanb = 0;
                    GetRoiLAB(bmp, r1, c1, r2, c2, out meanl, out meana, out meanb);
                    meanL.Add(meanl);
                    meanA.Add(meana);
                    meanB.Add(meanb);
                    double meang = 0;
                    GetRoiGrayScale(bmp, rr1, cc1, rr2, cc2, out meang);
                    meanGray.Add(meang);
                }
                for (int i = 0; i < meanL.Count; i++)
                {
                    //xiaoyu 15 OK
                    double l1 = (meanL[i] - meanLStd[i]) * (meanL[i] - meanLStd[i]);
                    double a1 = (meanA[i] - meanAStd[i]) * (meanA[i] - meanAStd[i]);
                    double b1 = (meanB[i] - meanBStd[i]) * (meanB[i] - meanBStd[i]);
                    double mm = Math.Sqrt(l1 + a1 + b1);
                    deltaE[i] = mm;
                }
                for (int i = 0; i < meanGray.Count-1; i++)
                {
                    deltaGray[i] = Math.Abs( meanGray[i + 1] - meanGray[i]);
                }
                SavePicture(pictureSavePath, win);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }
        public void GetRoiGrayScale(Bitmap bmp, double r1, double c1, double r2, double c2, out double mean)
        {
            HObject hImage = new HObject();
            Bitmap2HObject1(bmp, ref hImage);
            HObject R = new HObject();
            HObject G = new HObject();
            HObject B = new HObject();
            HOperatorSet.Decompose3(hImage, out R, out G, out B);
            HObject ho_ImageGray = new HObject();
            HOperatorSet. Rgb3ToGray(R, G, B, out ho_ImageGray);
            HObject ho_Rectangle = new HObject();
            HOperatorSet.GenRectangle1(out ho_Rectangle, r1, c1, r2, c2);
            HObject ho_ImageReduced = new HObject();
            HOperatorSet.ReduceDomain(ho_ImageGray, ho_Rectangle, out ho_ImageReduced);
            HTuple hv_Mean = new HTuple();
            HTuple hv_Deviation = new HTuple();
            HOperatorSet.Intensity(ho_Rectangle, ho_ImageGray, out hv_Mean, out hv_Deviation);
            mean = hv_Mean.D;
        }
        public void ReadStdRoiLab(string addr)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(addr);
            XmlNodeList nods = doc.SelectNodes("Config/StdRoiLab/ParamSet");
            foreach (XmlNode item in nods)
            {
                if (item.Attributes["ParamName"].Value.ToString().Contains("meanl"))
                {
                    meanLStd.Add(double.Parse(item.Attributes["ParamVal"].Value));
                }
                if (item.Attributes["ParamName"].Value.ToString().Contains("meana"))
                {
                    meanAStd.Add(double.Parse(item.Attributes["ParamVal"].Value));
                }
                if (item.Attributes["ParamName"].Value.ToString().Contains("meanb"))
                {
                    meanBStd.Add(double.Parse(item.Attributes["ParamVal"].Value));
                }
                
            }
        }
    }
}
