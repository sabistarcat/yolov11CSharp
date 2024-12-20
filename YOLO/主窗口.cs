using OpenCvSharp;
using OpenCvSharp.Extensions;

using System.Diagnostics;
using System.Drawing.Imaging;
using yolo目标检测;


namespace YOLO
{
    public partial class 主窗口 : Form
    {
        public 主窗口()
        {
            InitializeComponent();
        }
        BW_yolo yolo;
        string 模型路径 = AppDomain.CurrentDomain.BaseDirectory + "ONNX";
        bool 启用gpu = false;
        int 高速模式 = 1;
        string 模型名 = "";
        int 版本号 = 0;
        bool 停止 = false;
        int 显卡索引 = 0;
        private void 主窗口_Load(object sender, EventArgs e)
        {
            任务类型下拉框.SelectedIndex = 0;
            string[] 模型数组 = Directory.GetFiles(模型路径, "*.onnx", SearchOption.AllDirectories);
            foreach (var item in 模型数组)
            {
                FileInfo 模型 = new FileInfo(item);
                模型下拉框.Items.Add(模型.Name);
            }
            if (模型下拉框.Items.Count > 0)
            {
                模型下拉框.SelectedIndex = 0;
            }
            版本号 = int.Parse(版本号编辑框.Text);
            显卡索引 = int.Parse(显卡索引编辑框.Text.Trim());
            yolo = new BW_yolo(Path.Combine(模型路径, 模型名), 版本号, 显卡索引, 启用gpu);
            任务类型下拉框.SelectedIndex = yolo.任务模式;
        }
        private void 打开图片按钮_Click(object sender, EventArgs e)
        {


            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "全部|*.*|JPG格式|*.jpg|PNG格式|*.png|BMP格式|*.bmp";
            open.Title = "打开要识别的图片";
            if (open.ShowDialog() == DialogResult.OK)
            {
                string 图片路径 = open.FileName;
                Bitmap 图片 = new Bitmap(图片路径);

                float 置信度 = float.Parse(置信度编辑框.Text);
                float iou = float.Parse(阈值编辑框.Text);
                bool 全局iou = false;
                if (全局IOU复选框.Checked) 全局iou = true;
                List<yolo数据> 返回数据 = null;
                int 循环次数 = int.Parse(循环次数编辑框.Text);
                Stopwatch 计时器 = new Stopwatch();
                计时器.Start();
                for (int i = 0; i < 循环次数; i++)
                {

                    返回数据 = yolo.模型推理(图片, 置信度, iou, 全局iou, 高速模式);

                }
                计时器.Stop();

                用时标签.Text = "用时:  " + 计时器.ElapsedMilliseconds.ToString() + " ms";
                int fps = (int)(1000f / ((float)计时器.ElapsedMilliseconds / (float)循环次数));
                FPS标签.Text = "FPS:  " + fps.ToString();

                图片框.Image = yolo.生成图像(图片, 返回数据, yolo.标签组);

               // 图片框.Image.Save("1.bmp");



            }
        }
        private void 模型下拉框_SelectedIndexChanged(object sender, EventArgs e)
        {
            模型名 = 模型下拉框.Text;
            if (yolo != null)
            {
                yolo.释放资源();
                版本号 = int.Parse(版本号编辑框.Text);
                显卡索引 = int.Parse(显卡索引编辑框.Text.Trim());
                yolo = new BW_yolo(Path.Combine(模型路径, 模型名), 版本号, 显卡索引, 启用gpu);
                任务类型下拉框.SelectedIndex = yolo.任务模式;
                循环次数编辑框.Text = "1";
            }
            // GC.Collect();
        }
        private void 任务类型下拉框_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (yolo != null)
            {
                yolo.任务模式 = 任务类型下拉框.SelectedIndex;
                if (yolo.任务模式 != 任务类型下拉框.SelectedIndex)
                {
                    任务类型下拉框.SelectedIndex = yolo.任务模式;
                }
                循环次数编辑框.Text = "1";
            }
        }
        private void GPU加速复选框_CheckedChanged(object sender, EventArgs e)
        {
            启用gpu = GPU加速复选框.Checked;
            if (yolo != null)
            {
                yolo.释放资源();
                版本号 = int.Parse(版本号编辑框.Text);
                显卡索引 = int.Parse(显卡索引编辑框.Text.Trim());
                yolo = new BW_yolo(Path.Combine(模型路径, 模型名), 版本号, 显卡索引, 启用gpu);
                任务类型下拉框.SelectedIndex = yolo.任务模式;
                循环次数编辑框.Text = "1";
            }
        }

        private void 打开视频按钮_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "mp4格式|*.mp4";
            open.Title = "打开视频";
            停止 = true;
            int fps = 0;
            int i = 0;
            if (open.ShowDialog() == DialogResult.OK)
            {
                停止 = false;
                string 视频路径 = open.FileName;
                VideoCapture 视频 = new VideoCapture(视频路径);

                float 置信度 = float.Parse(置信度编辑框.Text);
                float iou = float.Parse(阈值编辑框.Text);
                bool 全局iou = false;

                if (!视频.IsOpened())
                {
                    Debug.WriteLine("无法播放");
                }
                // Cv2.NamedWindow("Video Frame", WindowFlags.AutoSize);
                List<yolo数据> 返回数据 = null;
                Bitmap 图像;
                Stopwatch 计时器 = new Stopwatch();
                //保存图片计数用
                int num = 0;
                while (!停止)
                {
                    using (var frame = new Mat())
                    {
                        //取2帧,缩短播放时长
                        if (!视频.Read(frame)) break;
                        if (!视频.Read(frame)) break;
 
                        图像 = BitmapConverter.ToBitmap(frame);
                        
                        num++;
                        计时器.Restart();
                        返回数据 = yolo.模型推理(图像, 置信度, iou, 全局iou, 高速模式);
                        计时器.Stop();
                        fps += (int)(1000f / ((float)计时器.ElapsedMilliseconds));
                        i++;
                        //每30帧显示一次fps防止闪烁
                        if (i == 30)
                        {
                            fps = fps / 30;
                            FPS标签.Text = "FPS:  " + fps.ToString();
                            fps = 0;
                            i = 0;
                        }
                        if (图片框.Image != null)
                        {
                            图片框.Image.Dispose();
                        }
                        图片框.Image = yolo.生成图像(图像, 返回数据, yolo.标签组);
                        Application.DoEvents();
                        //Cv2.ImShow("Video Frame", frame);
                    }

                }
            }
        }

        private void 打开图片集按钮_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            var result = fb.ShowDialog();
            停止 = true;
            int j = 0;
            int fps = 0;
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fb.SelectedPath))
            {

                float 置信度 = float.Parse(置信度编辑框.Text);
                float iou = float.Parse(阈值编辑框.Text);
                bool 全局iou = false;
                停止 = false;
                // 获取选中的文件夹路径
                string 文件夹路径 = fb.SelectedPath;
                string[] 文件名 = Directory.GetFiles(文件夹路径, "*.bmp", SearchOption.AllDirectories);
                //排序
                var 排序文件名 = 文件名.OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f))).ToArray();
                List<yolo数据> 返回数据 = null;
                Stopwatch 计时器 = new Stopwatch();
                for (int i = 0; i < 排序文件名.LongLength; i++)
                {
                    if (停止) break;

                    if (图片框.Image != null)
                    {
                        图片框.Image.Dispose();
                    }
                    Bitmap 图像 = new Bitmap(排序文件名[i]);
                    计时器.Restart();
                    返回数据 = yolo.模型推理(图像, 置信度, iou, 全局iou,高速模式);
                    计时器.Stop();
                    fps += (int)(1000f / ((float)计时器.ElapsedMilliseconds));
                    j++;
                    if (j == 30)
                    {
                        fps = fps / 30;
                        FPS标签.Text = "FPS:  " + fps.ToString();
                        fps = 0;
                        j = 0;
                    }

                    // FPS标签.Text = "FPS:  " + fps.ToString();
                    图片框.Image = yolo.生成图像(图像, 返回数据, yolo.标签组) ;

                    图像.Dispose();
                    Application.DoEvents();
                }
            }
        }

        private void 预处理模式复选框_CheckedChanged(object sender, EventArgs e)
        {
            if (预处理模式复选框.Checked)
            {
                高速模式 = 1;
            }
            else
            {
                高速模式 = 0;
            }

            Debug.WriteLine(高速模式);
        }
    }
}