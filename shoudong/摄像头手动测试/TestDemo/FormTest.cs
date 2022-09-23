using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestDemo
{
    public partial class FormTest : Form
    {
        HWindow hw;
        HObject im=new HObject();
        HTuple r1, c1, r2, c2, width, height;

        public FormTest()
        {
            InitializeComponent();
            hw = hWindowControl1.HalconWindow;
            r1 = null;
            c1 = null;
            r2 = null;
            c2 = null;
            width = null;
            height = null;
        }

        private void checkBoxSuo_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSuo.Checked=true)
            {
                checkBoxSuo.Visible = false;
                HOperatorSet.DumpWindowImage(out im, hw);
                HOperatorSet.GetPart(hw, out r1, out c1, out r2, out c2);
                HTuple pointer, type;
                HOperatorSet.GetImagePointer1(im, out pointer, out type, out width, out height);
                label12.Visible = false;
                label13.Visible = false;
                label14.Visible = false;
                label16.Visible = false;
                
            }
        }

        private void label14_MouseEnter(object sender, EventArgs e)
        {
            HOperatorSet.DumpWindowImage(out im, hw);
            HOperatorSet.GetPart(hw, out r1, out c1, out r2, out c2);
            HTuple pointer, type;
            HOperatorSet.GetImagePointer1(im, out pointer, out type, out width, out height);
            

        }

        private void hWindowControl1_HMouseWheel2(object sender, HalconDotNet.HMouseEventArgs e)
        {
            double h = hWindowControl1.Size.Height;
            double w = hWindowControl1.Size.Width;
            if (e.Delta != 0)
            {
                h = (1 + e.Delta * 0.001) * h;
                w = (1 + e.Delta * 0.001) * w;
                HOperatorSet.DumpWindowImage(out im, hw);

                HOperatorSet.GetPart(hw, out r1, out c1, out r2, out c2);
                hWindowControl1.Size = new Size((int)w, (int)h);
                hw.SetPart(0, 0,(int) r2-r1, (int)c2-c1);
                HOperatorSet.ClearWindow(hw);
                //HOperatorSet.SetPart(hw, 0, 0, (1 + e.Delta * 0.001) * r2, (1 + e.Delta * 0.001) * c2);
                hw.DispObj(im);
            }
        }
        private void hWindowControl1_HMouseWheel(object sender, HalconDotNet.HMouseEventArgs e)
        {
            double h = hWindowControl1.Size.Height;
            double w = hWindowControl1.Size.Width;
            if (checkBoxSuo.Visible==false)
            {
                if (e.Delta != 0)
                {
                    h = (1 + e.Delta * 0.001) * h;
                    w = (1 + e.Delta * 0.001) * w;


                    //HOperatorSet.GetPart(hw, out r1, out c1, out r2, out c2);
                    hWindowControl1.Size = new Size((int)w, (int)h);
                    hw.SetPart(0, 0, (int)height - 1, (int)width - 1);
                    //HOperatorSet.ClearWindow(hw);
                    //HOperatorSet.SetPart(hw, 0, 0, (1 + e.Delta * 0.001) * r2, (1 + e.Delta * 0.001) * c2);
                    hw.DispObj(im);
                }
            }
            
        }
        private void hWindowControl1_HMouseWheel1(object sender, HalconDotNet.HMouseEventArgs e)
        {
            
            HTuple Zoom, Row, Col, Button;
            HTuple Row0, Column0, Row00, Column00, Ht, Wt, r1, c1, r2, c2;
            if (e.Delta > 0)
            {
                Zoom = 1.1;
            }
            else
            {
                Zoom = 0.1;
            }
            HOperatorSet.GetMposition(hw, out Row, out Col, out Button);
            HOperatorSet.GetPart(hw, out Row0, out Column0, out Row00, out Column00);
            Ht = Row00 - Row0;
            Wt = Column00 - Column0;
            if (Ht * Wt < 32000 * 32000 || Zoom == 1.5)//普通版halcon能处理的图像最大尺寸是32K*32K。如果无限缩小原图像，导致显示的图像超出限制，则会造成程序崩溃
            {
                if (e.Delta>0)
                {
                    r1 = (Row0 + ((1 - (1.0 / Zoom)) * (Row - Row0)));
                    c1 = (Column0 + ((1 - (1.0 / Zoom)) * (Col - Column0)));
                    r2 = 0 + Ht * e.Delta * 0.001; //(Ht / Zoom);
                    c2 = 0 + Wt * e.Delta * 0.001; //(Wt / Zoom);
                }
                else
                {
                    r1 = (Row0 + ((1 - (1.0 / Zoom)) * (Row - Row0)));
                    c1 = (Column0 + ((1 - (1.0 / Zoom)) * (Col - Column0)));
                    r2 = 0 - Ht * e.Delta * 0.001; //(Ht / Zoom);
                    c2 = 0 - Wt * e.Delta * 0.001; //(Wt / Zoom);
                }
                
                HOperatorSet.SetPart(hw, 0, 0, r2, c2);
                HOperatorSet.ClearWindow(hw);
                HOperatorSet.DispObj(im, hw);
            }
        }
        HTuple r = 0;
        HTuple c = 0;
        HTuple b = 0;
        private void hWindowControl1_MouseMove(object sender, MouseEventArgs e)
        {
            r = 0;
            c = 0;
            b = 0;

            HOperatorSet.GetMposition(hWindowControl1.HalconWindow, out r, out c, out b);
            label14.Text = c.ToString();
            label13.Text = r.ToString();
        }
    }
}
