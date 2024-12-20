using OpenCvSharp;
using OpenCvSharp.Extensions;

using System.Diagnostics;
using System.Drawing.Imaging;
using yoloĿ����;


namespace YOLO
{
    public partial class ������ : Form
    {
        public ������()
        {
            InitializeComponent();
        }
        BW_yolo yolo;
        string ģ��·�� = AppDomain.CurrentDomain.BaseDirectory + "ONNX";
        bool ����gpu = false;
        int ����ģʽ = 1;
        string ģ���� = "";
        int �汾�� = 0;
        bool ֹͣ = false;
        int �Կ����� = 0;
        private void ������_Load(object sender, EventArgs e)
        {
            ��������������.SelectedIndex = 0;
            string[] ģ������ = Directory.GetFiles(ģ��·��, "*.onnx", SearchOption.AllDirectories);
            foreach (var item in ģ������)
            {
                FileInfo ģ�� = new FileInfo(item);
                ģ��������.Items.Add(ģ��.Name);
            }
            if (ģ��������.Items.Count > 0)
            {
                ģ��������.SelectedIndex = 0;
            }
            �汾�� = int.Parse(�汾�ű༭��.Text);
            �Կ����� = int.Parse(�Կ������༭��.Text.Trim());
            yolo = new BW_yolo(Path.Combine(ģ��·��, ģ����), �汾��, �Կ�����, ����gpu);
            ��������������.SelectedIndex = yolo.����ģʽ;
        }
        private void ��ͼƬ��ť_Click(object sender, EventArgs e)
        {


            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "ȫ��|*.*|JPG��ʽ|*.jpg|PNG��ʽ|*.png|BMP��ʽ|*.bmp";
            open.Title = "��Ҫʶ���ͼƬ";
            if (open.ShowDialog() == DialogResult.OK)
            {
                string ͼƬ·�� = open.FileName;
                Bitmap ͼƬ = new Bitmap(ͼƬ·��);

                float ���Ŷ� = float.Parse(���Ŷȱ༭��.Text);
                float iou = float.Parse(��ֵ�༭��.Text);
                bool ȫ��iou = false;
                if (ȫ��IOU��ѡ��.Checked) ȫ��iou = true;
                List<yolo����> �������� = null;
                int ѭ������ = int.Parse(ѭ�������༭��.Text);
                Stopwatch ��ʱ�� = new Stopwatch();
                ��ʱ��.Start();
                for (int i = 0; i < ѭ������; i++)
                {

                    �������� = yolo.ģ������(ͼƬ, ���Ŷ�, iou, ȫ��iou, ����ģʽ);

                }
                ��ʱ��.Stop();

                ��ʱ��ǩ.Text = "��ʱ:  " + ��ʱ��.ElapsedMilliseconds.ToString() + " ms";
                int fps = (int)(1000f / ((float)��ʱ��.ElapsedMilliseconds / (float)ѭ������));
                FPS��ǩ.Text = "FPS:  " + fps.ToString();

                ͼƬ��.Image = yolo.����ͼ��(ͼƬ, ��������, yolo.��ǩ��);

               // ͼƬ��.Image.Save("1.bmp");



            }
        }
        private void ģ��������_SelectedIndexChanged(object sender, EventArgs e)
        {
            ģ���� = ģ��������.Text;
            if (yolo != null)
            {
                yolo.�ͷ���Դ();
                �汾�� = int.Parse(�汾�ű༭��.Text);
                �Կ����� = int.Parse(�Կ������༭��.Text.Trim());
                yolo = new BW_yolo(Path.Combine(ģ��·��, ģ����), �汾��, �Կ�����, ����gpu);
                ��������������.SelectedIndex = yolo.����ģʽ;
                ѭ�������༭��.Text = "1";
            }
            // GC.Collect();
        }
        private void ��������������_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (yolo != null)
            {
                yolo.����ģʽ = ��������������.SelectedIndex;
                if (yolo.����ģʽ != ��������������.SelectedIndex)
                {
                    ��������������.SelectedIndex = yolo.����ģʽ;
                }
                ѭ�������༭��.Text = "1";
            }
        }
        private void GPU���ٸ�ѡ��_CheckedChanged(object sender, EventArgs e)
        {
            ����gpu = GPU���ٸ�ѡ��.Checked;
            if (yolo != null)
            {
                yolo.�ͷ���Դ();
                �汾�� = int.Parse(�汾�ű༭��.Text);
                �Կ����� = int.Parse(�Կ������༭��.Text.Trim());
                yolo = new BW_yolo(Path.Combine(ģ��·��, ģ����), �汾��, �Կ�����, ����gpu);
                ��������������.SelectedIndex = yolo.����ģʽ;
                ѭ�������༭��.Text = "1";
            }
        }

        private void ����Ƶ��ť_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "mp4��ʽ|*.mp4";
            open.Title = "����Ƶ";
            ֹͣ = true;
            int fps = 0;
            int i = 0;
            if (open.ShowDialog() == DialogResult.OK)
            {
                ֹͣ = false;
                string ��Ƶ·�� = open.FileName;
                VideoCapture ��Ƶ = new VideoCapture(��Ƶ·��);

                float ���Ŷ� = float.Parse(���Ŷȱ༭��.Text);
                float iou = float.Parse(��ֵ�༭��.Text);
                bool ȫ��iou = false;

                if (!��Ƶ.IsOpened())
                {
                    Debug.WriteLine("�޷�����");
                }
                // Cv2.NamedWindow("Video Frame", WindowFlags.AutoSize);
                List<yolo����> �������� = null;
                Bitmap ͼ��;
                Stopwatch ��ʱ�� = new Stopwatch();
                //����ͼƬ������
                int num = 0;
                while (!ֹͣ)
                {
                    using (var frame = new Mat())
                    {
                        //ȡ2֡,���̲���ʱ��
                        if (!��Ƶ.Read(frame)) break;
                        if (!��Ƶ.Read(frame)) break;
 
                        ͼ�� = BitmapConverter.ToBitmap(frame);
                        
                        num++;
                        ��ʱ��.Restart();
                        �������� = yolo.ģ������(ͼ��, ���Ŷ�, iou, ȫ��iou, ����ģʽ);
                        ��ʱ��.Stop();
                        fps += (int)(1000f / ((float)��ʱ��.ElapsedMilliseconds));
                        i++;
                        //ÿ30֡��ʾһ��fps��ֹ��˸
                        if (i == 30)
                        {
                            fps = fps / 30;
                            FPS��ǩ.Text = "FPS:  " + fps.ToString();
                            fps = 0;
                            i = 0;
                        }
                        if (ͼƬ��.Image != null)
                        {
                            ͼƬ��.Image.Dispose();
                        }
                        ͼƬ��.Image = yolo.����ͼ��(ͼ��, ��������, yolo.��ǩ��);
                        Application.DoEvents();
                        //Cv2.ImShow("Video Frame", frame);
                    }

                }
            }
        }

        private void ��ͼƬ����ť_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            var result = fb.ShowDialog();
            ֹͣ = true;
            int j = 0;
            int fps = 0;
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fb.SelectedPath))
            {

                float ���Ŷ� = float.Parse(���Ŷȱ༭��.Text);
                float iou = float.Parse(��ֵ�༭��.Text);
                bool ȫ��iou = false;
                ֹͣ = false;
                // ��ȡѡ�е��ļ���·��
                string �ļ���·�� = fb.SelectedPath;
                string[] �ļ��� = Directory.GetFiles(�ļ���·��, "*.bmp", SearchOption.AllDirectories);
                //����
                var �����ļ��� = �ļ���.OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f))).ToArray();
                List<yolo����> �������� = null;
                Stopwatch ��ʱ�� = new Stopwatch();
                for (int i = 0; i < �����ļ���.LongLength; i++)
                {
                    if (ֹͣ) break;

                    if (ͼƬ��.Image != null)
                    {
                        ͼƬ��.Image.Dispose();
                    }
                    Bitmap ͼ�� = new Bitmap(�����ļ���[i]);
                    ��ʱ��.Restart();
                    �������� = yolo.ģ������(ͼ��, ���Ŷ�, iou, ȫ��iou,����ģʽ);
                    ��ʱ��.Stop();
                    fps += (int)(1000f / ((float)��ʱ��.ElapsedMilliseconds));
                    j++;
                    if (j == 30)
                    {
                        fps = fps / 30;
                        FPS��ǩ.Text = "FPS:  " + fps.ToString();
                        fps = 0;
                        j = 0;
                    }

                    // FPS��ǩ.Text = "FPS:  " + fps.ToString();
                    ͼƬ��.Image = yolo.����ͼ��(ͼ��, ��������, yolo.��ǩ��) ;

                    ͼ��.Dispose();
                    Application.DoEvents();
                }
            }
        }

        private void Ԥ����ģʽ��ѡ��_CheckedChanged(object sender, EventArgs e)
        {
            if (Ԥ����ģʽ��ѡ��.Checked)
            {
                ����ģʽ = 1;
            }
            else
            {
                ����ģʽ = 0;
            }

            Debug.WriteLine(����ģʽ);
        }
    }
}