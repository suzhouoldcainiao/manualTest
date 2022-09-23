
namespace TestDemo
{
    partial class FormTest
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.hWindowControl1 = new HalconDotNet.HWindowControl();
            this.label16 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.checkBoxSuo = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // hWindowControl1
            // 
            this.hWindowControl1.BackColor = System.Drawing.Color.Black;
            this.hWindowControl1.BorderColor = System.Drawing.Color.Black;
            this.hWindowControl1.ImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hWindowControl1.Location = new System.Drawing.Point(106, 12);
            this.hWindowControl1.Name = "hWindowControl1";
            this.hWindowControl1.Size = new System.Drawing.Size(1241, 766);
            this.hWindowControl1.TabIndex = 0;
            this.hWindowControl1.WindowSize = new System.Drawing.Size(1241, 766);
            this.hWindowControl1.HMouseWheel += new HalconDotNet.HMouseEventHandler(this.hWindowControl1_HMouseWheel);
            this.hWindowControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.hWindowControl1_MouseMove);
            // 
            // label16
            // 
            this.label16.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label16.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label16.Location = new System.Drawing.Point(12, 145);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(76, 19);
            this.label16.TabIndex = 59;
            this.label16.Text = "row";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label14
            // 
            this.label14.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label14.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label14.Location = new System.Drawing.Point(12, 103);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(76, 19);
            this.label14.TabIndex = 58;
            this.label14.Text = "0";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label13
            // 
            this.label13.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label13.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label13.Location = new System.Drawing.Point(12, 180);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(76, 19);
            this.label13.TabIndex = 57;
            this.label13.Text = "0";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label12
            // 
            this.label12.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label12.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label12.Location = new System.Drawing.Point(12, 64);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(76, 19);
            this.label12.TabIndex = 56;
            this.label12.Text = "column";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // checkBoxSuo
            // 
            this.checkBoxSuo.AutoSize = true;
            this.checkBoxSuo.Location = new System.Drawing.Point(12, 213);
            this.checkBoxSuo.Name = "checkBoxSuo";
            this.checkBoxSuo.Size = new System.Drawing.Size(48, 16);
            this.checkBoxSuo.TabIndex = 60;
            this.checkBoxSuo.Text = "缩放";
            this.checkBoxSuo.UseVisualStyleBackColor = true;
            this.checkBoxSuo.CheckedChanged += new System.EventHandler(this.checkBoxSuo_CheckedChanged);
            // 
            // FormTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1355, 787);
            this.Controls.Add(this.checkBoxSuo);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.hWindowControl1);
            this.Name = "FormTest";
            this.Text = "OcTest";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public HalconDotNet.HWindowControl hWindowControl1;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox checkBoxSuo;
    }
}