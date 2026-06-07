using MeasurementAdjustment;
using System;
using System.Windows.Forms;

namespace AdjustEverything
{
    public partial class FormHome : Form
    {
        public FormHome()
        {
            InitializeComponent();
        }

        private void btnDrawingBoard_Click(object sender, EventArgs e)
        {
            FormDrawingBoard drawingBoard = new FormDrawingBoard();
            drawingBoard.FormClosed += (s, args) => this.Show();
            drawingBoard.Show();
            this.Hide();
        }

        private void btnMatrixCalculator_Click(object sender, EventArgs e)
        {
            FormMatrixCalculator matrixCalculator = new FormMatrixCalculator();
            matrixCalculator.FormClosed += (s, args) => this.Show();
            matrixCalculator.Show();
            this.Hide();
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            // 隐藏三个功能按钮
            btnDrawingBoard.Visible = false;
            btnMatrixCalculator.Visible = false;
            btnAbout.Visible = false;

            // 显示制作人员信息面板和返回按钮
            pnlAboutInfo.Visible = true;
            btnReturn.Visible = true;
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            // 隐藏制作人员信息面板和返回按钮
            pnlAboutInfo.Visible = false;
            btnReturn.Visible = false;

            // 显示三个功能按钮
            btnDrawingBoard.Visible = true;
            btnMatrixCalculator.Visible = true;
            btnAbout.Visible = true;
        }

        private void FormHome_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}