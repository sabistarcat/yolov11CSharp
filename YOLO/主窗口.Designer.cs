namespace YOLO
{
    partial class 主窗口
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBox1 = new GroupBox();
            图片框 = new PictureBox();
            打开图片按钮 = new Button();
            groupBox2 = new GroupBox();
            置信度编辑框 = new TextBox();
            groupBox3 = new GroupBox();
            全局IOU复选框 = new CheckBox();
            阈值编辑框 = new TextBox();
            groupBox4 = new GroupBox();
            任务类型下拉框 = new ComboBox();
            groupBox5 = new GroupBox();
            模型下拉框 = new ComboBox();
            groupBox6 = new GroupBox();
            显卡索引编辑框 = new TextBox();
            预处理模式复选框 = new CheckBox();
            GPU加速复选框 = new CheckBox();
            groupBox7 = new GroupBox();
            循环次数编辑框 = new TextBox();
            groupBox8 = new GroupBox();
            FPS标签 = new Label();
            用时标签 = new Label();
            groupBox9 = new GroupBox();
            版本号编辑框 = new TextBox();
            打开视频按钮 = new Button();
            打开图片集按钮 = new Button();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)图片框).BeginInit();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox5.SuspendLayout();
            groupBox6.SuspendLayout();
            groupBox7.SuspendLayout();
            groupBox8.SuspendLayout();
            groupBox9.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(图片框);
            groupBox1.Location = new Point(11, 22);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(672, 701);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            // 
            // 图片框
            // 
            图片框.Location = new Point(8, 16);
            图片框.Name = "图片框";
            图片框.Size = new Size(658, 679);
            图片框.SizeMode = PictureBoxSizeMode.Zoom;
            图片框.TabIndex = 0;
            图片框.TabStop = false;
            // 
            // 打开图片按钮
            // 
            打开图片按钮.Location = new Point(693, 628);
            打开图片按钮.Name = "打开图片按钮";
            打开图片按钮.Size = new Size(144, 29);
            打开图片按钮.TabIndex = 1;
            打开图片按钮.Text = "单张图片";
            打开图片按钮.UseVisualStyleBackColor = true;
            打开图片按钮.Click += 打开图片按钮_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(置信度编辑框);
            groupBox2.Location = new Point(693, 21);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(144, 56);
            groupBox2.TabIndex = 9;
            groupBox2.TabStop = false;
            groupBox2.Text = "置信度";
            // 
            // 置信度编辑框
            // 
            置信度编辑框.Location = new Point(15, 22);
            置信度编辑框.Name = "置信度编辑框";
            置信度编辑框.Size = new Size(63, 23);
            置信度编辑框.TabIndex = 4;
            置信度编辑框.Text = "0.5";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(全局IOU复选框);
            groupBox3.Controls.Add(阈值编辑框);
            groupBox3.Location = new Point(693, 79);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(144, 83);
            groupBox3.TabIndex = 10;
            groupBox3.TabStop = false;
            groupBox3.Text = "IOU阈值";
            // 
            // 全局IOU复选框
            // 
            全局IOU复选框.AutoSize = true;
            全局IOU复选框.Location = new Point(14, 51);
            全局IOU复选框.Name = "全局IOU复选框";
            全局IOU复选框.Size = new Size(98, 21);
            全局IOU复选框.TabIndex = 7;
            全局IOU复选框.Text = "开启全局IOU";
            全局IOU复选框.UseVisualStyleBackColor = true;
            // 
            // 阈值编辑框
            // 
            阈值编辑框.Location = new Point(14, 20);
            阈值编辑框.Name = "阈值编辑框";
            阈值编辑框.Size = new Size(64, 23);
            阈值编辑框.TabIndex = 6;
            阈值编辑框.Text = "0.3";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(任务类型下拉框);
            groupBox4.Location = new Point(693, 163);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(144, 62);
            groupBox4.TabIndex = 11;
            groupBox4.TabStop = false;
            groupBox4.Text = "任务类型";
            // 
            // 任务类型下拉框
            // 
            任务类型下拉框.DropDownStyle = ComboBoxStyle.DropDownList;
            任务类型下拉框.FormattingEnabled = true;
            任务类型下拉框.Items.AddRange(new object[] { "分类", "检测", "分割", "检测+分割", "关键点", "检测+关键点", "OBB检测" });
            任务类型下拉框.Location = new Point(14, 22);
            任务类型下拉框.Name = "任务类型下拉框";
            任务类型下拉框.Size = new Size(96, 25);
            任务类型下拉框.TabIndex = 9;
            任务类型下拉框.SelectedIndexChanged += 任务类型下拉框_SelectedIndexChanged;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(模型下拉框);
            groupBox5.Location = new Point(693, 227);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(144, 61);
            groupBox5.TabIndex = 12;
            groupBox5.TabStop = false;
            groupBox5.Text = "ONNX模型";
            // 
            // 模型下拉框
            // 
            模型下拉框.DropDownStyle = ComboBoxStyle.DropDownList;
            模型下拉框.FormattingEnabled = true;
            模型下拉框.Location = new Point(14, 22);
            模型下拉框.Name = "模型下拉框";
            模型下拉框.Size = new Size(119, 25);
            模型下拉框.TabIndex = 10;
            模型下拉框.SelectedIndexChanged += 模型下拉框_SelectedIndexChanged;
            // 
            // groupBox6
            // 
            groupBox6.Controls.Add(显卡索引编辑框);
            groupBox6.Controls.Add(预处理模式复选框);
            groupBox6.Controls.Add(GPU加速复选框);
            groupBox6.Location = new Point(693, 292);
            groupBox6.Name = "groupBox6";
            groupBox6.Size = new Size(144, 82);
            groupBox6.TabIndex = 14;
            groupBox6.TabStop = false;
            groupBox6.Text = "模式";
            // 
            // 显卡索引编辑框
            // 
            显卡索引编辑框.Location = new Point(88, 20);
            显卡索引编辑框.Name = "显卡索引编辑框";
            显卡索引编辑框.Size = new Size(48, 23);
            显卡索引编辑框.TabIndex = 2;
            显卡索引编辑框.Text = "0";
            显卡索引编辑框.TextAlign = HorizontalAlignment.Center;
            // 
            // 预处理模式复选框
            // 
            预处理模式复选框.AutoSize = true;
            预处理模式复选框.Checked = true;
            预处理模式复选框.CheckState = CheckState.Checked;
            预处理模式复选框.Location = new Point(14, 49);
            预处理模式复选框.Name = "预处理模式复选框";
            预处理模式复选框.Size = new Size(87, 21);
            预处理模式复选框.TabIndex = 1;
            预处理模式复选框.Text = "预处理加速";
            预处理模式复选框.UseVisualStyleBackColor = true;
            预处理模式复选框.CheckedChanged += 预处理模式复选框_CheckedChanged;
            // 
            // GPU加速复选框
            // 
            GPU加速复选框.AutoSize = true;
            GPU加速复选框.Location = new Point(14, 22);
            GPU加速复选框.Name = "GPU加速复选框";
            GPU加速复选框.Size = new Size(76, 21);
            GPU加速复选框.TabIndex = 0;
            GPU加速复选框.Text = "GPU加速";
            GPU加速复选框.UseVisualStyleBackColor = true;
            GPU加速复选框.CheckedChanged += GPU加速复选框_CheckedChanged;
            // 
            // groupBox7
            // 
            groupBox7.Controls.Add(循环次数编辑框);
            groupBox7.Location = new Point(693, 465);
            groupBox7.Name = "groupBox7";
            groupBox7.Size = new Size(144, 65);
            groupBox7.TabIndex = 15;
            groupBox7.TabStop = false;
            groupBox7.Text = "循环测试次数";
            // 
            // 循环次数编辑框
            // 
            循环次数编辑框.Location = new Point(14, 22);
            循环次数编辑框.Name = "循环次数编辑框";
            循环次数编辑框.Size = new Size(64, 23);
            循环次数编辑框.TabIndex = 0;
            循环次数编辑框.Text = "1";
            // 
            // groupBox8
            // 
            groupBox8.Controls.Add(FPS标签);
            groupBox8.Controls.Add(用时标签);
            groupBox8.Location = new Point(693, 537);
            groupBox8.Name = "groupBox8";
            groupBox8.Size = new Size(144, 83);
            groupBox8.TabIndex = 16;
            groupBox8.TabStop = false;
            groupBox8.Text = "信息";
            // 
            // FPS标签
            // 
            FPS标签.AutoSize = true;
            FPS标签.Location = new Point(17, 54);
            FPS标签.Name = "FPS标签";
            FPS标签.Size = new Size(31, 17);
            FPS标签.TabIndex = 18;
            FPS标签.Text = "FPS:";
            // 
            // 用时标签
            // 
            用时标签.AutoSize = true;
            用时标签.Location = new Point(16, 24);
            用时标签.Name = "用时标签";
            用时标签.Size = new Size(35, 17);
            用时标签.TabIndex = 17;
            用时标签.Text = "用时:";
            // 
            // groupBox9
            // 
            groupBox9.Controls.Add(版本号编辑框);
            groupBox9.Location = new Point(694, 398);
            groupBox9.Name = "groupBox9";
            groupBox9.Size = new Size(142, 63);
            groupBox9.TabIndex = 17;
            groupBox9.TabStop = false;
            groupBox9.Text = "指定版本(0自动识别)";
            // 
            // 版本号编辑框
            // 
            版本号编辑框.Location = new Point(13, 25);
            版本号编辑框.Name = "版本号编辑框";
            版本号编辑框.Size = new Size(64, 23);
            版本号编辑框.TabIndex = 7;
            版本号编辑框.Text = "0";
            // 
            // 打开视频按钮
            // 
            打开视频按钮.Location = new Point(693, 663);
            打开视频按钮.Name = "打开视频按钮";
            打开视频按钮.Size = new Size(144, 27);
            打开视频按钮.TabIndex = 18;
            打开视频按钮.Text = "实时视频 开/关";
            打开视频按钮.UseVisualStyleBackColor = true;
            打开视频按钮.Click += 打开视频按钮_Click;
            // 
            // 打开图片集按钮
            // 
            打开图片集按钮.Location = new Point(693, 696);
            打开图片集按钮.Name = "打开图片集按钮";
            打开图片集按钮.Size = new Size(144, 27);
            打开图片集按钮.TabIndex = 19;
            打开图片集按钮.Text = "图片集合 开/关";
            打开图片集按钮.UseVisualStyleBackColor = true;
            打开图片集按钮.Click += 打开图片集按钮_Click;
            // 
            // 主窗口
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(872, 741);
            Controls.Add(打开图片集按钮);
            Controls.Add(打开视频按钮);
            Controls.Add(groupBox9);
            Controls.Add(groupBox8);
            Controls.Add(groupBox7);
            Controls.Add(groupBox6);
            Controls.Add(groupBox5);
            Controls.Add(groupBox4);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(打开图片按钮);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "主窗口";
            Text = "BW_yolo";
            Load += 主窗口_Load;
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)图片框).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox5.ResumeLayout(false);
            groupBox6.ResumeLayout(false);
            groupBox6.PerformLayout();
            groupBox7.ResumeLayout(false);
            groupBox7.PerformLayout();
            groupBox8.ResumeLayout(false);
            groupBox8.PerformLayout();
            groupBox9.ResumeLayout(false);
            groupBox9.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private PictureBox 图片框;
        private Button 打开图片按钮;
        private GroupBox groupBox2;
        private TextBox 置信度编辑框;
        private GroupBox groupBox3;
        private CheckBox 全局IOU复选框;
        private TextBox 阈值编辑框;
        private GroupBox groupBox4;
        private ComboBox 任务类型下拉框;
        private GroupBox groupBox5;
        private ComboBox 模型下拉框;
        private GroupBox groupBox6;
        private CheckBox GPU加速复选框;
        private GroupBox groupBox7;
        private TextBox 循环次数编辑框;
        private GroupBox groupBox8;
        private Label FPS标签;
        private Label 用时标签;
        private GroupBox groupBox9;
        private TextBox 版本号编辑框;
        private Button 打开视频按钮;
        private Button 打开图片集按钮;
        private CheckBox 预处理模式复选框;
        private TextBox 显卡索引编辑框;
    }
}