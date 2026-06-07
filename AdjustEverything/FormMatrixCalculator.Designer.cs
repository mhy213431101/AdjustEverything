namespace MeasurementAdjustment
{
    partial class FormMatrixCalculator
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // ==================== 窗体级控件 ====================
        private System.Windows.Forms.TabControl tabControl;

        // ==================== 间接平差控件 ====================
        private System.Windows.Forms.TabPage tabIndirect;
        private System.Windows.Forms.GroupBox groupIndirectInput;
        private System.Windows.Forms.Label lblIndirectB;
        private System.Windows.Forms.Label lblIndirectL;
        private System.Windows.Forms.Label lblIndirectConst;
        private System.Windows.Forms.Label lblIndirectP;
        private System.Windows.Forms.TextBox txtIndirectB;
        private System.Windows.Forms.TextBox txtIndirectConst;
        private System.Windows.Forms.TextBox txtIndirectP;
        private System.Windows.Forms.Button btnIndirectCalc;
        private System.Windows.Forms.GroupBox groupIndirectOutput;
        private System.Windows.Forms.TextBox txtIndirectResult;

        // ==================== 条件平差控件 ====================
        private System.Windows.Forms.TabPage tabCondition;
        private System.Windows.Forms.GroupBox groupConditionInput;
        private System.Windows.Forms.Label lblConditionA;
        private System.Windows.Forms.Label lblConditionL;
        private System.Windows.Forms.Label lblConditionW0;
        private System.Windows.Forms.Label lblConditionP;
        private System.Windows.Forms.TextBox txtConditionA;
        private System.Windows.Forms.TextBox txtConditionL;
        private System.Windows.Forms.TextBox txtConditionW0;
        private System.Windows.Forms.TextBox txtConditionP;
        private System.Windows.Forms.Button btnConditionCalc;
        private System.Windows.Forms.GroupBox groupConditionOutput;
        private System.Windows.Forms.TextBox txtConditionResult;

        // ==================== 附有参数的条件平差控件 ====================
        private System.Windows.Forms.TabPage tabParamCondition;
        private System.Windows.Forms.GroupBox groupParamConditionInput;
        private System.Windows.Forms.Label lblParamConditionA;
        private System.Windows.Forms.Label lblParamConditionB;
        private System.Windows.Forms.Label lblParamConditionW;
        private System.Windows.Forms.Label lblParamConditionL;
        private System.Windows.Forms.Label lblParamConditionP;
        private System.Windows.Forms.TextBox txtParamConditionA;
        private System.Windows.Forms.TextBox txtParamConditionB;
        private System.Windows.Forms.TextBox txtParamConditionW;
        private System.Windows.Forms.TextBox txtParamConditionL;
        private System.Windows.Forms.TextBox txtParamConditionP;
        private System.Windows.Forms.Button btnParamConditionCalc;
        private System.Windows.Forms.GroupBox groupParamConditionOutput;
        private System.Windows.Forms.TextBox txtParamConditionResult;

        // ==================== 附有限制条件的间接平差控件 ====================
        private System.Windows.Forms.TabPage tabConstrainedIndirect;
        private System.Windows.Forms.GroupBox groupConstrainedIndirectInput;
        private System.Windows.Forms.Label lblConstrainedIndirectB;
        private System.Windows.Forms.Label lblConstrainedIndirectL;
        private System.Windows.Forms.Label lblConstrainedIndirectConst;
        private System.Windows.Forms.Label lblConstrainedIndirectC;
        private System.Windows.Forms.Label lblConstrainedIndirectWx;
        private System.Windows.Forms.Label lblConstrainedIndirectP;
        private System.Windows.Forms.TextBox txtConstrainedIndirectB;
        private System.Windows.Forms.TextBox txtConstrainedIndirectL;
        private System.Windows.Forms.TextBox txtConstrainedIndirectConst;
        private System.Windows.Forms.TextBox txtConstrainedIndirectC;
        private System.Windows.Forms.TextBox txtConstrainedIndirectWx;
        private System.Windows.Forms.TextBox txtConstrainedIndirectP;
        private System.Windows.Forms.Button btnConstrainedIndirectCalc;
        private System.Windows.Forms.GroupBox groupConstrainedIndirectOutput;
        private System.Windows.Forms.TextBox txtConstrainedIndirectResult;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            tabControl = new TabControl();
            tabIndirect = new TabPage();
            groupIndirectOutput = new GroupBox();
            txtIndirectResult = new TextBox();
            groupIndirectInput = new GroupBox();
            btnIndirectCalc = new Button();
            txtIndirectP = new TextBox();
            lblIndirectP = new Label();
            lblIndirectConst = new Label();
            txtIndirectConst = new TextBox();
            lblIndirectL = new Label();
            txtIndirectB = new TextBox();
            lblIndirectB = new Label();
            tabCondition = new TabPage();
            groupConditionOutput = new GroupBox();
            txtConditionResult = new TextBox();
            groupConditionInput = new GroupBox();
            btnConditionCalc = new Button();
            txtConditionP = new TextBox();
            lblConditionP = new Label();
            txtConditionW0 = new TextBox();
            lblConditionW0 = new Label();
            txtConditionL = new TextBox();
            lblConditionL = new Label();
            txtConditionA = new TextBox();
            lblConditionA = new Label();
            tabParamCondition = new TabPage();
            groupParamConditionOutput = new GroupBox();
            txtParamConditionResult = new TextBox();
            groupParamConditionInput = new GroupBox();
            btnParamConditionCalc = new Button();
            txtParamConditionP = new TextBox();
            lblParamConditionP = new Label();
            txtParamConditionL = new TextBox();
            lblParamConditionL = new Label();
            txtParamConditionW = new TextBox();
            lblParamConditionW = new Label();
            txtParamConditionB = new TextBox();
            lblParamConditionB = new Label();
            txtParamConditionA = new TextBox();
            lblParamConditionA = new Label();
            tabConstrainedIndirect = new TabPage();
            groupConstrainedIndirectOutput = new GroupBox();
            txtConstrainedIndirectResult = new TextBox();
            groupConstrainedIndirectInput = new GroupBox();
            btnConstrainedIndirectCalc = new Button();
            txtConstrainedIndirectP = new TextBox();
            lblConstrainedIndirectP = new Label();
            txtConstrainedIndirectWx = new TextBox();
            lblConstrainedIndirectWx = new Label();
            txtConstrainedIndirectC = new TextBox();
            lblConstrainedIndirectC = new Label();
            lblConstrainedIndirectConst = new Label();
            txtConstrainedIndirectConst = new TextBox();
            lblConstrainedIndirectL = new Label();
            txtConstrainedIndirectB = new TextBox();
            lblConstrainedIndirectB = new Label();
            txtIndirectL = new TextBox();
            txtConstrainedIndirectL = new TextBox();
            tabControl.SuspendLayout();
            tabIndirect.SuspendLayout();
            groupIndirectOutput.SuspendLayout();
            groupIndirectInput.SuspendLayout();
            tabCondition.SuspendLayout();
            groupConditionOutput.SuspendLayout();
            groupConditionInput.SuspendLayout();
            tabParamCondition.SuspendLayout();
            groupParamConditionOutput.SuspendLayout();
            groupParamConditionInput.SuspendLayout();
            tabConstrainedIndirect.SuspendLayout();
            groupConstrainedIndirectOutput.SuspendLayout();
            groupConstrainedIndirectInput.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabIndirect);
            tabControl.Controls.Add(tabCondition);
            tabControl.Controls.Add(tabParamCondition);
            tabControl.Controls.Add(tabConstrainedIndirect);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1184, 861);
            tabControl.TabIndex = 0;
            // 
            // tabIndirect
            // 
            tabIndirect.Controls.Add(groupIndirectOutput);
            tabIndirect.Controls.Add(groupIndirectInput);
            tabIndirect.Location = new Point(4, 34);
            tabIndirect.Name = "tabIndirect";
            tabIndirect.Padding = new Padding(3);
            tabIndirect.Size = new Size(1176, 823);
            tabIndirect.TabIndex = 0;
            tabIndirect.Text = "间接平差";
            tabIndirect.UseVisualStyleBackColor = true;
            // 
            // groupIndirectOutput
            // 
            groupIndirectOutput.Controls.Add(txtIndirectResult);
            groupIndirectOutput.Location = new Point(640, 15);
            groupIndirectOutput.Name = "groupIndirectOutput";
            groupIndirectOutput.Size = new Size(520, 800);
            groupIndirectOutput.TabIndex = 1;
            groupIndirectOutput.TabStop = false;
            groupIndirectOutput.Text = "输出结果 (W, N, K, V, 平差值L̂)";
            // 
            // txtIndirectResult
            // 
            txtIndirectResult.Font = new Font("宋体", 12F);
            txtIndirectResult.Location = new Point(15, 25);
            txtIndirectResult.Multiline = true;
            txtIndirectResult.Name = "txtIndirectResult";
            txtIndirectResult.ScrollBars = ScrollBars.Vertical;
            txtIndirectResult.Size = new Size(485, 750);
            txtIndirectResult.TabIndex = 0;
            // 
            // groupIndirectInput
            // 
            groupIndirectInput.Controls.Add(txtIndirectL);
            groupIndirectInput.Controls.Add(btnIndirectCalc);
            groupIndirectInput.Controls.Add(txtIndirectP);
            groupIndirectInput.Controls.Add(lblIndirectP);
            groupIndirectInput.Controls.Add(lblIndirectConst);
            groupIndirectInput.Controls.Add(txtIndirectConst);
            groupIndirectInput.Controls.Add(lblIndirectL);
            groupIndirectInput.Controls.Add(txtIndirectB);
            groupIndirectInput.Controls.Add(lblIndirectB);
            groupIndirectInput.Location = new Point(15, 15);
            groupIndirectInput.Name = "groupIndirectInput";
            groupIndirectInput.Size = new Size(600, 800);
            groupIndirectInput.TabIndex = 0;
            groupIndirectInput.TabStop = false;
            groupIndirectInput.Text = "输入参数";
            // 
            // btnIndirectCalc
            // 
            btnIndirectCalc.Location = new Point(218, 696);
            btnIndirectCalc.Name = "btnIndirectCalc";
            btnIndirectCalc.Size = new Size(124, 35);
            btnIndirectCalc.TabIndex = 8;
            btnIndirectCalc.Text = "计算";
            btnIndirectCalc.UseVisualStyleBackColor = true;
            // 
            // txtIndirectP
            // 
            txtIndirectP.Location = new Point(15, 490);
            txtIndirectP.Multiline = true;
            txtIndirectP.Name = "txtIndirectP";
            txtIndirectP.ScrollBars = ScrollBars.Vertical;
            txtIndirectP.Size = new Size(552, 118);
            txtIndirectP.TabIndex = 7;
            // 
            // lblIndirectP
            // 
            lblIndirectP.AutoSize = true;
            lblIndirectP.Location = new Point(15, 449);
            lblIndirectP.Name = "lblIndirectP";
            lblIndirectP.Size = new Size(382, 24);
            lblIndirectP.TabIndex = 6;
            lblIndirectP.Text = "权阵 P (n×n) 可空(默认单位阵):";
            // 
            // lblIndirectConst
            // 
            lblIndirectConst.AutoSize = true;
            lblIndirectConst.Location = new Point(15, 333);
            lblIndirectConst.Name = "lblIndirectConst";
            lblIndirectConst.Size = new Size(202, 24);
            lblIndirectConst.TabIndex = 4;
            lblIndirectConst.Text = "常数项 l (n×1):";
            // 
            // txtIndirectConst
            // 
            txtIndirectConst.Location = new Point(15, 376);
            txtIndirectConst.Multiline = true;
            txtIndirectConst.Name = "txtIndirectConst";
            txtIndirectConst.Size = new Size(552, 51);
            txtIndirectConst.TabIndex = 5;
            txtIndirectConst.Text = "1;2;2";
            // 
            // lblIndirectL
            // 
            lblIndirectL.AutoSize = true;
            lblIndirectL.Location = new Point(15, 231);
            lblIndirectL.Name = "lblIndirectL";
            lblIndirectL.Size = new Size(250, 24);
            lblIndirectL.TabIndex = 2;
            lblIndirectL.Text = "观测值向量 L (n×1):";
            // 
            // txtIndirectB
            // 
            txtIndirectB.Location = new Point(15, 62);
            txtIndirectB.Multiline = true;
            txtIndirectB.Name = "txtIndirectB";
            txtIndirectB.ScrollBars = ScrollBars.Vertical;
            txtIndirectB.Size = new Size(552, 151);
            txtIndirectB.TabIndex = 1;
            txtIndirectB.Text = "1,1;\r\n1,2;\r\n1,3";
            // 
            // lblIndirectB
            // 
            lblIndirectB.AutoSize = true;
            lblIndirectB.Location = new Point(15, 35);
            lblIndirectB.Name = "lblIndirectB";
            lblIndirectB.Size = new Size(226, 24);
            lblIndirectB.TabIndex = 0;
            lblIndirectB.Text = "设计矩阵 B (n×t):";
            // 
            // tabCondition
            // 
            tabCondition.Controls.Add(groupConditionOutput);
            tabCondition.Controls.Add(groupConditionInput);
            tabCondition.Location = new Point(4, 34);
            tabCondition.Name = "tabCondition";
            tabCondition.Padding = new Padding(3);
            tabCondition.Size = new Size(1176, 823);
            tabCondition.TabIndex = 1;
            tabCondition.Text = "条件平差";
            tabCondition.UseVisualStyleBackColor = true;
            // 
            // groupConditionOutput
            // 
            groupConditionOutput.Controls.Add(txtConditionResult);
            groupConditionOutput.Location = new Point(640, 15);
            groupConditionOutput.Name = "groupConditionOutput";
            groupConditionOutput.Size = new Size(520, 800);
            groupConditionOutput.TabIndex = 1;
            groupConditionOutput.TabStop = false;
            groupConditionOutput.Text = "输出结果 (W, N, K, V, 平差值L̂)";
            // 
            // txtConditionResult
            // 
            txtConditionResult.Font = new Font("宋体", 12F);
            txtConditionResult.Location = new Point(15, 25);
            txtConditionResult.Multiline = true;
            txtConditionResult.Name = "txtConditionResult";
            txtConditionResult.ScrollBars = ScrollBars.Vertical;
            txtConditionResult.Size = new Size(485, 750);
            txtConditionResult.TabIndex = 0;
            // 
            // groupConditionInput
            // 
            groupConditionInput.Controls.Add(btnConditionCalc);
            groupConditionInput.Controls.Add(txtConditionP);
            groupConditionInput.Controls.Add(lblConditionP);
            groupConditionInput.Controls.Add(txtConditionW0);
            groupConditionInput.Controls.Add(lblConditionW0);
            groupConditionInput.Controls.Add(txtConditionL);
            groupConditionInput.Controls.Add(lblConditionL);
            groupConditionInput.Controls.Add(txtConditionA);
            groupConditionInput.Controls.Add(lblConditionA);
            groupConditionInput.Location = new Point(15, 15);
            groupConditionInput.Name = "groupConditionInput";
            groupConditionInput.Size = new Size(600, 800);
            groupConditionInput.TabIndex = 0;
            groupConditionInput.TabStop = false;
            groupConditionInput.Text = "输入参数";
            // 
            // btnConditionCalc
            // 
            btnConditionCalc.Location = new Point(214, 695);
            btnConditionCalc.Name = "btnConditionCalc";
            btnConditionCalc.Size = new Size(123, 35);
            btnConditionCalc.TabIndex = 8;
            btnConditionCalc.Text = "计算";
            btnConditionCalc.UseVisualStyleBackColor = true;
            // 
            // txtConditionP
            // 
            txtConditionP.Location = new Point(15, 489);
            txtConditionP.Multiline = true;
            txtConditionP.Name = "txtConditionP";
            txtConditionP.ScrollBars = ScrollBars.Vertical;
            txtConditionP.Size = new Size(555, 135);
            txtConditionP.TabIndex = 7;
            // 
            // lblConditionP
            // 
            lblConditionP.AutoSize = true;
            lblConditionP.Location = new Point(6, 446);
            lblConditionP.Name = "lblConditionP";
            lblConditionP.Size = new Size(382, 24);
            lblConditionP.TabIndex = 6;
            lblConditionP.Text = "权阵 P (n×n) 可空(默认单位阵):";
            // 
            // txtConditionW0
            // 
            txtConditionW0.Location = new Point(15, 375);
            txtConditionW0.Multiline = true;
            txtConditionW0.Name = "txtConditionW0";
            txtConditionW0.Size = new Size(552, 50);
            txtConditionW0.TabIndex = 5;
            txtConditionW0.Text = "0";
            // 
            // lblConditionW0
            // 
            lblConditionW0.AutoSize = true;
            lblConditionW0.Location = new Point(6, 328);
            lblConditionW0.Name = "lblConditionW0";
            lblConditionW0.Size = new Size(286, 24);
            lblConditionW0.TabIndex = 4;
            lblConditionW0.Text = "闭合差常数项 W0 (r×1):";
            // 
            // txtConditionL
            // 
            txtConditionL.Location = new Point(15, 259);
            txtConditionL.Multiline = true;
            txtConditionL.Name = "txtConditionL";
            txtConditionL.Size = new Size(552, 50);
            txtConditionL.TabIndex = 3;
            txtConditionL.Text = "2;3;4";
            // 
            // lblConditionL
            // 
            lblConditionL.AutoSize = true;
            lblConditionL.Location = new Point(6, 217);
            lblConditionL.Name = "lblConditionL";
            lblConditionL.Size = new Size(202, 24);
            lblConditionL.TabIndex = 2;
            lblConditionL.Text = "观测值 L (n×1):";
            // 
            // txtConditionA
            // 
            txtConditionA.Location = new Point(15, 62);
            txtConditionA.Multiline = true;
            txtConditionA.Name = "txtConditionA";
            txtConditionA.ScrollBars = ScrollBars.Vertical;
            txtConditionA.Size = new Size(552, 138);
            txtConditionA.TabIndex = 1;
            txtConditionA.Text = "1,1,1";
            // 
            // lblConditionA
            // 
            lblConditionA.AutoSize = true;
            lblConditionA.Location = new Point(15, 35);
            lblConditionA.Name = "lblConditionA";
            lblConditionA.Size = new Size(226, 24);
            lblConditionA.TabIndex = 0;
            lblConditionA.Text = "条件矩阵 A (r×n):";
            // 
            // tabParamCondition
            // 
            tabParamCondition.Controls.Add(groupParamConditionOutput);
            tabParamCondition.Controls.Add(groupParamConditionInput);
            tabParamCondition.Location = new Point(4, 34);
            tabParamCondition.Name = "tabParamCondition";
            tabParamCondition.Size = new Size(1176, 823);
            tabParamCondition.TabIndex = 2;
            tabParamCondition.Text = "附有参数的条件平差";
            tabParamCondition.UseVisualStyleBackColor = true;
            // 
            // groupParamConditionOutput
            // 
            groupParamConditionOutput.Controls.Add(txtParamConditionResult);
            groupParamConditionOutput.Location = new Point(640, 15);
            groupParamConditionOutput.Name = "groupParamConditionOutput";
            groupParamConditionOutput.Size = new Size(520, 800);
            groupParamConditionOutput.TabIndex = 1;
            groupParamConditionOutput.TabStop = false;
            groupParamConditionOutput.Text = "输出结果 (W, N, K, V, 平差值L̂)";
            // 
            // txtParamConditionResult
            // 
            txtParamConditionResult.Font = new Font("宋体", 12F);
            txtParamConditionResult.Location = new Point(15, 25);
            txtParamConditionResult.Multiline = true;
            txtParamConditionResult.Name = "txtParamConditionResult";
            txtParamConditionResult.ScrollBars = ScrollBars.Vertical;
            txtParamConditionResult.Size = new Size(485, 750);
            txtParamConditionResult.TabIndex = 0;
            // 
            // groupParamConditionInput
            // 
            groupParamConditionInput.Controls.Add(btnParamConditionCalc);
            groupParamConditionInput.Controls.Add(txtParamConditionP);
            groupParamConditionInput.Controls.Add(lblParamConditionP);
            groupParamConditionInput.Controls.Add(txtParamConditionL);
            groupParamConditionInput.Controls.Add(lblParamConditionL);
            groupParamConditionInput.Controls.Add(txtParamConditionW);
            groupParamConditionInput.Controls.Add(lblParamConditionW);
            groupParamConditionInput.Controls.Add(txtParamConditionB);
            groupParamConditionInput.Controls.Add(lblParamConditionB);
            groupParamConditionInput.Controls.Add(txtParamConditionA);
            groupParamConditionInput.Controls.Add(lblParamConditionA);
            groupParamConditionInput.Location = new Point(15, 15);
            groupParamConditionInput.Name = "groupParamConditionInput";
            groupParamConditionInput.Size = new Size(600, 800);
            groupParamConditionInput.TabIndex = 0;
            groupParamConditionInput.TabStop = false;
            groupParamConditionInput.Text = "输入参数";
            // 
            // btnParamConditionCalc
            // 
            btnParamConditionCalc.Location = new Point(213, 740);
            btnParamConditionCalc.Name = "btnParamConditionCalc";
            btnParamConditionCalc.Size = new Size(135, 35);
            btnParamConditionCalc.TabIndex = 10;
            btnParamConditionCalc.Text = "计算";
            btnParamConditionCalc.UseVisualStyleBackColor = true;
            // 
            // txtParamConditionP
            // 
            txtParamConditionP.Location = new Point(15, 613);
            txtParamConditionP.Multiline = true;
            txtParamConditionP.Name = "txtParamConditionP";
            txtParamConditionP.ScrollBars = ScrollBars.Vertical;
            txtParamConditionP.Size = new Size(555, 81);
            txtParamConditionP.TabIndex = 9;
            // 
            // lblParamConditionP
            // 
            lblParamConditionP.AutoSize = true;
            lblParamConditionP.Location = new Point(15, 575);
            lblParamConditionP.Name = "lblParamConditionP";
            lblParamConditionP.Size = new Size(382, 24);
            lblParamConditionP.TabIndex = 8;
            lblParamConditionP.Text = "权阵 P (n×n) 可空(默认单位阵):";
            // 
            // txtParamConditionL
            // 
            txtParamConditionL.Location = new Point(15, 503);
            txtParamConditionL.Multiline = true;
            txtParamConditionL.Name = "txtParamConditionL";
            txtParamConditionL.Size = new Size(555, 50);
            txtParamConditionL.TabIndex = 7;
            txtParamConditionL.Text = "1;2;3";
            // 
            // lblParamConditionL
            // 
            lblParamConditionL.AutoSize = true;
            lblParamConditionL.Location = new Point(15, 464);
            lblParamConditionL.Name = "lblParamConditionL";
            lblParamConditionL.Size = new Size(202, 24);
            lblParamConditionL.TabIndex = 6;
            lblParamConditionL.Text = "观测值 L (n×1):";
            // 
            // txtParamConditionW
            // 
            txtParamConditionW.Location = new Point(15, 395);
            txtParamConditionW.Multiline = true;
            txtParamConditionW.Name = "txtParamConditionW";
            txtParamConditionW.Size = new Size(555, 50);
            txtParamConditionW.TabIndex = 5;
            txtParamConditionW.Text = "0";
            // 
            // lblParamConditionW
            // 
            lblParamConditionW.AutoSize = true;
            lblParamConditionW.Location = new Point(15, 359);
            lblParamConditionW.Name = "lblParamConditionW";
            lblParamConditionW.Size = new Size(202, 24);
            lblParamConditionW.TabIndex = 4;
            lblParamConditionW.Text = "闭合差 W (r×1):";
            // 
            // txtParamConditionB
            // 
            txtParamConditionB.Location = new Point(15, 224);
            txtParamConditionB.Multiline = true;
            txtParamConditionB.Name = "txtParamConditionB";
            txtParamConditionB.ScrollBars = ScrollBars.Vertical;
            txtParamConditionB.Size = new Size(555, 121);
            txtParamConditionB.TabIndex = 3;
            txtParamConditionB.Text = "1,0";
            // 
            // lblParamConditionB
            // 
            lblParamConditionB.AutoSize = true;
            lblParamConditionB.Location = new Point(15, 197);
            lblParamConditionB.Name = "lblParamConditionB";
            lblParamConditionB.Size = new Size(226, 24);
            lblParamConditionB.TabIndex = 2;
            lblParamConditionB.Text = "参数系数 B (r×u):";
            // 
            // txtParamConditionA
            // 
            txtParamConditionA.Location = new Point(15, 62);
            txtParamConditionA.Multiline = true;
            txtParamConditionA.Name = "txtParamConditionA";
            txtParamConditionA.ScrollBars = ScrollBars.Vertical;
            txtParamConditionA.Size = new Size(555, 119);
            txtParamConditionA.TabIndex = 1;
            txtParamConditionA.Text = "1,1,0";
            // 
            // lblParamConditionA
            // 
            lblParamConditionA.AutoSize = true;
            lblParamConditionA.Location = new Point(15, 35);
            lblParamConditionA.Name = "lblParamConditionA";
            lblParamConditionA.Size = new Size(226, 24);
            lblParamConditionA.TabIndex = 0;
            lblParamConditionA.Text = "条件系数 A (r×n):";
            // 
            // tabConstrainedIndirect
            // 
            tabConstrainedIndirect.Controls.Add(groupConstrainedIndirectOutput);
            tabConstrainedIndirect.Controls.Add(groupConstrainedIndirectInput);
            tabConstrainedIndirect.Location = new Point(4, 34);
            tabConstrainedIndirect.Name = "tabConstrainedIndirect";
            tabConstrainedIndirect.Size = new Size(1176, 823);
            tabConstrainedIndirect.TabIndex = 3;
            tabConstrainedIndirect.Text = "附有限制条件的间接平差";
            tabConstrainedIndirect.UseVisualStyleBackColor = true;
            // 
            // groupConstrainedIndirectOutput
            // 
            groupConstrainedIndirectOutput.Controls.Add(txtConstrainedIndirectResult);
            groupConstrainedIndirectOutput.Location = new Point(640, 15);
            groupConstrainedIndirectOutput.Name = "groupConstrainedIndirectOutput";
            groupConstrainedIndirectOutput.Size = new Size(520, 800);
            groupConstrainedIndirectOutput.TabIndex = 1;
            groupConstrainedIndirectOutput.TabStop = false;
            groupConstrainedIndirectOutput.Text = "输出结果 (W, N, K, V, 平差值L̂)";
            // 
            // txtConstrainedIndirectResult
            // 
            txtConstrainedIndirectResult.Font = new Font("宋体", 12F);
            txtConstrainedIndirectResult.Location = new Point(15, 25);
            txtConstrainedIndirectResult.Multiline = true;
            txtConstrainedIndirectResult.Name = "txtConstrainedIndirectResult";
            txtConstrainedIndirectResult.ScrollBars = ScrollBars.Vertical;
            txtConstrainedIndirectResult.Size = new Size(485, 750);
            txtConstrainedIndirectResult.TabIndex = 0;
            // 
            // groupConstrainedIndirectInput
            // 
            groupConstrainedIndirectInput.Controls.Add(txtConstrainedIndirectL);
            groupConstrainedIndirectInput.Controls.Add(btnConstrainedIndirectCalc);
            groupConstrainedIndirectInput.Controls.Add(txtConstrainedIndirectP);
            groupConstrainedIndirectInput.Controls.Add(lblConstrainedIndirectP);
            groupConstrainedIndirectInput.Controls.Add(txtConstrainedIndirectWx);
            groupConstrainedIndirectInput.Controls.Add(lblConstrainedIndirectWx);
            groupConstrainedIndirectInput.Controls.Add(txtConstrainedIndirectC);
            groupConstrainedIndirectInput.Controls.Add(lblConstrainedIndirectC);
            groupConstrainedIndirectInput.Controls.Add(lblConstrainedIndirectConst);
            groupConstrainedIndirectInput.Controls.Add(txtConstrainedIndirectConst);
            groupConstrainedIndirectInput.Controls.Add(lblConstrainedIndirectL);
            groupConstrainedIndirectInput.Controls.Add(txtConstrainedIndirectB);
            groupConstrainedIndirectInput.Controls.Add(lblConstrainedIndirectB);
            groupConstrainedIndirectInput.Location = new Point(15, 15);
            groupConstrainedIndirectInput.Name = "groupConstrainedIndirectInput";
            groupConstrainedIndirectInput.Size = new Size(600, 800);
            groupConstrainedIndirectInput.TabIndex = 0;
            groupConstrainedIndirectInput.TabStop = false;
            groupConstrainedIndirectInput.Text = "输入参数";
            // 
            // btnConstrainedIndirectCalc
            // 
            btnConstrainedIndirectCalc.Location = new Point(200, 740);
            btnConstrainedIndirectCalc.Name = "btnConstrainedIndirectCalc";
            btnConstrainedIndirectCalc.Size = new Size(118, 35);
            btnConstrainedIndirectCalc.TabIndex = 12;
            btnConstrainedIndirectCalc.Text = "计算";
            btnConstrainedIndirectCalc.UseVisualStyleBackColor = true;
            // 
            // txtConstrainedIndirectP
            // 
            txtConstrainedIndirectP.Location = new Point(15, 630);
            txtConstrainedIndirectP.Multiline = true;
            txtConstrainedIndirectP.Name = "txtConstrainedIndirectP";
            txtConstrainedIndirectP.ScrollBars = ScrollBars.Vertical;
            txtConstrainedIndirectP.Size = new Size(555, 75);
            txtConstrainedIndirectP.TabIndex = 11;
            // 
            // lblConstrainedIndirectP
            // 
            lblConstrainedIndirectP.AutoSize = true;
            lblConstrainedIndirectP.Location = new Point(15, 592);
            lblConstrainedIndirectP.Name = "lblConstrainedIndirectP";
            lblConstrainedIndirectP.Size = new Size(382, 24);
            lblConstrainedIndirectP.TabIndex = 10;
            lblConstrainedIndirectP.Text = "权阵 P (n×n) 可空(默认单位阵):";
            // 
            // txtConstrainedIndirectWx
            // 
            txtConstrainedIndirectWx.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtConstrainedIndirectWx.Location = new Point(15, 531);
            txtConstrainedIndirectWx.Multiline = true;
            txtConstrainedIndirectWx.Name = "txtConstrainedIndirectWx";
            txtConstrainedIndirectWx.Size = new Size(555, 43);
            txtConstrainedIndirectWx.TabIndex = 9;
            txtConstrainedIndirectWx.Text = "0";
            // 
            // lblConstrainedIndirectWx
            // 
            lblConstrainedIndirectWx.AutoSize = true;
            lblConstrainedIndirectWx.Location = new Point(15, 494);
            lblConstrainedIndirectWx.Name = "lblConstrainedIndirectWx";
            lblConstrainedIndirectWx.Size = new Size(238, 24);
            lblConstrainedIndirectWx.TabIndex = 8;
            lblConstrainedIndirectWx.Text = "限制常数 Wx (c×1):";
            // 
            // txtConstrainedIndirectC
            // 
            txtConstrainedIndirectC.Location = new Point(15, 405);
            txtConstrainedIndirectC.Multiline = true;
            txtConstrainedIndirectC.Name = "txtConstrainedIndirectC";
            txtConstrainedIndirectC.ScrollBars = ScrollBars.Vertical;
            txtConstrainedIndirectC.Size = new Size(555, 75);
            txtConstrainedIndirectC.TabIndex = 7;
            txtConstrainedIndirectC.Text = "1,1";
            // 
            // lblConstrainedIndirectC
            // 
            lblConstrainedIndirectC.AutoSize = true;
            lblConstrainedIndirectC.Location = new Point(15, 362);
            lblConstrainedIndirectC.Name = "lblConstrainedIndirectC";
            lblConstrainedIndirectC.Size = new Size(226, 24);
            lblConstrainedIndirectC.TabIndex = 6;
            lblConstrainedIndirectC.Text = "限制系数 C (c×t):";
            // 
            // lblConstrainedIndirectConst
            // 
            lblConstrainedIndirectConst.AutoSize = true;
            lblConstrainedIndirectConst.Location = new Point(15, 273);
            lblConstrainedIndirectConst.Name = "lblConstrainedIndirectConst";
            lblConstrainedIndirectConst.Size = new Size(202, 24);
            lblConstrainedIndirectConst.TabIndex = 4;
            lblConstrainedIndirectConst.Text = "常数项 l (n×1):";
            // 
            // txtConstrainedIndirectConst
            // 
            txtConstrainedIndirectConst.Location = new Point(15, 300);
            txtConstrainedIndirectConst.Multiline = true;
            txtConstrainedIndirectConst.Name = "txtConstrainedIndirectConst";
            txtConstrainedIndirectConst.Size = new Size(555, 40);
            txtConstrainedIndirectConst.TabIndex = 5;
            txtConstrainedIndirectConst.Text = "1;1;2";
            // 
            // lblConstrainedIndirectL
            // 
            lblConstrainedIndirectL.AutoSize = true;
            lblConstrainedIndirectL.Location = new Point(15, 179);
            lblConstrainedIndirectL.Name = "lblConstrainedIndirectL";
            lblConstrainedIndirectL.Size = new Size(202, 24);
            lblConstrainedIndirectL.TabIndex = 2;
            lblConstrainedIndirectL.Text = "观测值 L (n×1):";
            // 
            // txtConstrainedIndirectB
            // 
            txtConstrainedIndirectB.Location = new Point(15, 62);
            txtConstrainedIndirectB.Multiline = true;
            txtConstrainedIndirectB.Name = "txtConstrainedIndirectB";
            txtConstrainedIndirectB.ScrollBars = ScrollBars.Vertical;
            txtConstrainedIndirectB.Size = new Size(555, 104);
            txtConstrainedIndirectB.TabIndex = 1;
            txtConstrainedIndirectB.Text = "1,0;0,1;1,1";
            // 
            // lblConstrainedIndirectB
            // 
            lblConstrainedIndirectB.AutoSize = true;
            lblConstrainedIndirectB.Location = new Point(15, 35);
            lblConstrainedIndirectB.Name = "lblConstrainedIndirectB";
            lblConstrainedIndirectB.Size = new Size(226, 24);
            lblConstrainedIndirectB.TabIndex = 0;
            lblConstrainedIndirectB.Text = "设计矩阵 B (n×t):";
            // 
            // txtIndirectL
            // 
            txtIndirectL.Location = new Point(15, 267);
            txtIndirectL.Multiline = true;
            txtIndirectL.Name = "txtIndirectL";
            txtIndirectL.Size = new Size(552, 51);
            txtIndirectL.TabIndex = 9;
            txtIndirectL.Text = "2;3;4";
            // 
            // txtConstrainedIndirectL
            // 
            txtConstrainedIndirectL.Location = new Point(15, 217);
            txtConstrainedIndirectL.Multiline = true;
            txtConstrainedIndirectL.Name = "txtConstrainedIndirectL";
            txtConstrainedIndirectL.Size = new Size(555, 38);
            txtConstrainedIndirectL.TabIndex = 13;
            txtConstrainedIndirectL.Text = "2;3;4";
            // 
            // FormMatrixCalculator
            // 
            AutoScaleDimensions = new SizeF(12F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 861);
            Controls.Add(tabControl);
            Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            Name = "FormMatrixCalculator";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "测量平差矩阵计算器 (四种平差模型)";
            tabControl.ResumeLayout(false);
            tabIndirect.ResumeLayout(false);
            groupIndirectOutput.ResumeLayout(false);
            groupIndirectOutput.PerformLayout();
            groupIndirectInput.ResumeLayout(false);
            groupIndirectInput.PerformLayout();
            tabCondition.ResumeLayout(false);
            groupConditionOutput.ResumeLayout(false);
            groupConditionOutput.PerformLayout();
            groupConditionInput.ResumeLayout(false);
            groupConditionInput.PerformLayout();
            tabParamCondition.ResumeLayout(false);
            groupParamConditionOutput.ResumeLayout(false);
            groupParamConditionOutput.PerformLayout();
            groupParamConditionInput.ResumeLayout(false);
            groupParamConditionInput.PerformLayout();
            tabConstrainedIndirect.ResumeLayout(false);
            groupConstrainedIndirectOutput.ResumeLayout(false);
            groupConstrainedIndirectOutput.PerformLayout();
            groupConstrainedIndirectInput.ResumeLayout(false);
            groupConstrainedIndirectInput.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TextBox txtIndirectL;
    }
}