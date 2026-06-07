namespace AdjustEverything
{
    partial class FormHome
    {
        private System.ComponentModel.IContainer components = null;

        // 控件声明
        private System.Windows.Forms.Button btnDrawingBoard;
        private System.Windows.Forms.Button btnMatrixCalculator;
        private System.Windows.Forms.Button btnAbout;
        private System.Windows.Forms.Button btnReturn;
        private System.Windows.Forms.Panel pnlAboutInfo;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblAboutTitle;
        private System.Windows.Forms.Label lblLine1;
        private System.Windows.Forms.Label lblLine3;
        private System.Windows.Forms.Label lblLine4;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            btnDrawingBoard = new Button();
            btnMatrixCalculator = new Button();
            btnAbout = new Button();
            btnReturn = new Button();
            pnlAboutInfo = new Panel();
            label1 = new Label();
            lblAboutTitle = new Label();
            lblLine1 = new Label();
            lblLine3 = new Label();
            lblLine4 = new Label();
            lblTitle = new Label();
            pnlAboutInfo.SuspendLayout();
            SuspendLayout();
            // 
            // btnDrawingBoard
            // 
            btnDrawingBoard.BackColor = Color.FromArgb(41, 128, 185);
            btnDrawingBoard.Cursor = Cursors.Hand;
            btnDrawingBoard.FlatAppearance.BorderSize = 0;
            btnDrawingBoard.FlatStyle = FlatStyle.Flat;
            btnDrawingBoard.Font = new Font("微软雅黑", 14F, FontStyle.Bold);
            btnDrawingBoard.ForeColor = Color.White;
            btnDrawingBoard.Location = new Point(110, 176);
            btnDrawingBoard.Name = "btnDrawingBoard";
            btnDrawingBoard.Size = new Size(280, 60);
            btnDrawingBoard.TabIndex = 1;
            btnDrawingBoard.Text = "平差画板👍";
            btnDrawingBoard.UseVisualStyleBackColor = false;
            btnDrawingBoard.Click += btnDrawingBoard_Click;
            // 
            // btnMatrixCalculator
            // 
            btnMatrixCalculator.BackColor = Color.FromArgb(46, 204, 113);
            btnMatrixCalculator.Cursor = Cursors.Hand;
            btnMatrixCalculator.FlatAppearance.BorderSize = 0;
            btnMatrixCalculator.FlatStyle = FlatStyle.Flat;
            btnMatrixCalculator.Font = new Font("微软雅黑", 14F, FontStyle.Bold);
            btnMatrixCalculator.ForeColor = Color.White;
            btnMatrixCalculator.Location = new Point(110, 266);
            btnMatrixCalculator.Name = "btnMatrixCalculator";
            btnMatrixCalculator.Size = new Size(280, 60);
            btnMatrixCalculator.TabIndex = 2;
            btnMatrixCalculator.Text = "误差方程计算器";
            btnMatrixCalculator.UseVisualStyleBackColor = false;
            btnMatrixCalculator.Click += btnMatrixCalculator_Click;
            // 
            // btnAbout
            // 
            btnAbout.BackColor = Color.FromArgb(155, 89, 182);
            btnAbout.Cursor = Cursors.Hand;
            btnAbout.FlatAppearance.BorderSize = 0;
            btnAbout.FlatStyle = FlatStyle.Flat;
            btnAbout.Font = new Font("微软雅黑", 14F, FontStyle.Bold);
            btnAbout.ForeColor = Color.White;
            btnAbout.Location = new Point(110, 356);
            btnAbout.Name = "btnAbout";
            btnAbout.Size = new Size(280, 60);
            btnAbout.TabIndex = 3;
            btnAbout.Text = "ⓘ 关于我们";
            btnAbout.UseVisualStyleBackColor = false;
            btnAbout.Click += btnAbout_Click;
            // 
            // btnReturn
            // 
            btnReturn.BackColor = Color.FromArgb(52, 73, 94);
            btnReturn.Cursor = Cursors.Hand;
            btnReturn.FlatAppearance.BorderSize = 0;
            btnReturn.FlatStyle = FlatStyle.Flat;
            btnReturn.Font = new Font("微软雅黑", 12F, FontStyle.Bold);
            btnReturn.ForeColor = Color.White;
            btnReturn.Location = new Point(190, 462);
            btnReturn.Name = "btnReturn";
            btnReturn.Size = new Size(120, 40);
            btnReturn.TabIndex = 4;
            btnReturn.Text = "返回";
            btnReturn.UseVisualStyleBackColor = false;
            btnReturn.Visible = false;
            btnReturn.Click += btnReturn_Click;
            // 
            // pnlAboutInfo
            // 
            pnlAboutInfo.BackColor = Color.FromArgb(236, 240, 241);
            pnlAboutInfo.BorderStyle = BorderStyle.FixedSingle;
            pnlAboutInfo.Controls.Add(label1);
            pnlAboutInfo.Controls.Add(lblAboutTitle);
            pnlAboutInfo.Controls.Add(lblLine1);
            pnlAboutInfo.Controls.Add(lblLine3);
            pnlAboutInfo.Controls.Add(lblLine4);
            pnlAboutInfo.Location = new Point(50, 155);
            pnlAboutInfo.Name = "pnlAboutInfo";
            pnlAboutInfo.Size = new Size(400, 291);
            pnlAboutInfo.TabIndex = 5;
            pnlAboutInfo.Visible = false;
            // 
            // label1
            // 
            label1.Font = new Font("微软雅黑", 11F);
            label1.Location = new Point(10, 166);
            label1.Name = "label1";
            label1.Size = new Size(379, 75);
            label1.TabIndex = 6;
            label1.Text = "开发人员：朱国俊、李乾凯而、                         汤博翔、丁常天、刘武林";
            // 
            // lblAboutTitle
            // 
            lblAboutTitle.Font = new Font("微软雅黑", 14F, FontStyle.Bold);
            lblAboutTitle.ForeColor = Color.FromArgb(52, 73, 94);
            lblAboutTitle.Location = new Point(10, 15);
            lblAboutTitle.Name = "lblAboutTitle";
            lblAboutTitle.Size = new Size(380, 50);
            lblAboutTitle.TabIndex = 0;
            lblAboutTitle.Text = "制作人员信息";
            lblAboutTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblLine1
            // 
            lblLine1.Font = new Font("微软雅黑", 11F);
            lblLine1.Location = new Point(10, 88);
            lblLine1.Name = "lblLine1";
            lblLine1.Size = new Size(380, 38);
            lblLine1.TabIndex = 1;
            lblLine1.Text = "项目名称：AdjustEverything";
            // 
            // lblLine3
            // 
            lblLine3.Font = new Font("微软雅黑", 11F);
            lblLine3.Location = new Point(10, 126);
            lblLine3.Name = "lblLine3";
            lblLine3.Size = new Size(380, 36);
            lblLine3.TabIndex = 3;
            lblLine3.Text = "开发小组：第一小组";
            // 
            // lblLine4
            // 
            lblLine4.Font = new Font("微软雅黑", 11F);
            lblLine4.Location = new Point(10, 241);
            lblLine4.Name = "lblLine4";
            lblLine4.Size = new Size(380, 41);
            lblLine4.TabIndex = 4;
            lblLine4.Text = "开发工具：C# WinForms";
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("微软雅黑", 20F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(52, 73, 94);
            lblTitle.Location = new Point(50, 36);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(400, 113);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "AdjustEverything测绘平差计算系统";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // FormHome
            // 
            ClientSize = new Size(498, 527);
            Controls.Add(lblTitle);
            Controls.Add(btnDrawingBoard);
            Controls.Add(btnMatrixCalculator);
            Controls.Add(btnAbout);
            Controls.Add(btnReturn);
            Controls.Add(pnlAboutInfo);
            Name = "FormHome";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "测绘平差系统 - 主界面";
            FormClosing += FormHome_FormClosing;
            pnlAboutInfo.ResumeLayout(false);
            ResumeLayout(false);
        }
        private Label label1;
    }
}