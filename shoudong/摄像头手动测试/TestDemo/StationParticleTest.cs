using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;
using ImageAlgorithm;
using NST_CameraImageTest;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace TestDemo
{
    public class StationParticleTest:StationBase
    {
        public int smallP;
        public int mediumP;
        public int bigP;
        public bool resultOfTest;
        public List<_LABEL> lab;
        public ParticleInfo pInfo=new ParticleInfo();
        public StationParticleTest(FormTest formtest, string stationName, TextBox textbox, string type, string sn, string imageDIR) : base(formtest, stationName, textbox, type, sn, imageDIR)
        {

        }
        public override bool Test()
        {
            bool b = false;
            try
            {
                LoadImage();
                SavePicture(pictureSavePath, win);
                b=GetParticleValue(bmp, pInfo, win);
                SavePicture(pictureSavePath, win);
                SaveParticleTestData(dataPath);
                return b;
            }
            catch (Exception)
            {

                return false;
            }
        }
        public unsafe bool GetParticleValue(Bitmap bitmap,ParticleInfo info,HTuple hwindle)
        {
            try
            {
                bool b=false;
                byte[] byBuffer = ConvertBitmapToByteArray(bitmap);
                fixed (byte* pbuffer = byBuffer)
                {
                    b= CameraImageTest.ParticleTest(pbuffer, bitmap.Width, bitmap.Height, ref info);
                   
                }
                resultOfTest = info.paResult.bPA_Res;
                smallP = info.paResult.nPA_SmallCount;
                mediumP = info.paResult.nPA_MediumCount;
                bigP = info.paResult.nPA_BigCount;
                lab = info.paResult.lPA_Pos;
                DrawPicCircleParticle(win, lab);
                return b;
            }
            catch (Exception e)
            {
                return false;
                
            }
            
        }
        public void SaveParticleTestData(string dataPath)
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
                info = $"SN,时间,结果,小面积坏点,中面积坏点,大面积坏点";
                using (StreamWriter sw = new StreamWriter(dataPath, true))
                {
                    sw.WriteLine(info);
                }
            }
            info = $"{PSN},{DateTime.Now.ToString("HH：mm：ss")},{this.pInfo.paResult.bPA_Res},{this.pInfo.paResult.nPA_SmallCount},{this.pInfo.paResult.nPA_MediumCount},{this.pInfo.paResult.nPA_BigCount}";
            using (StreamWriter sw = new StreamWriter(dataPath, true))
            {
                sw.WriteLine(info);
            }
        }
        public void DrawPicCircleParticle(HTuple wHandle, List<_LABEL> blP)
        {

            HObject ho_Rectangle;
            HOperatorSet.SetDraw(wHandle, "margin");
            HOperatorSet.SetColor(wHandle, "blue");
            HOperatorSet.SetLineWidth(wHandle, 1);
            for (int i = 0; i < blP.Count; i++)
            {
                HOperatorSet.SetColor(wHandle, "red");
                HObject hCircle = null;
                if (blP[i].dRadius>=0)
                {
                    HOperatorSet.GenCircle(out hCircle, blP[i].nY, blP[i].nX, blP[i].dRadius+10);
                    HOperatorSet.DispObj(hCircle, wHandle);
                }

            }
        }
    }
}

