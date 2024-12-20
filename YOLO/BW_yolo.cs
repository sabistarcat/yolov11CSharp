using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
//更新时间2024/3/29
/* 说明
你可以创建一个.net6或者net8的桌面应用程序直接将该cs文件导入,如使用控制台,需自行引用相应的框架,FrameWork必须用4.8以上版本
目前支持yolo5、yolo6、yolo8、yolo9的onnx检测模型,包含seg.onnx对分割模型,cls.onnx对分类,pose.onnx动作模型,obb.onnx定向边界模型
本类引用了2个库(免cuda)
Microsoft.ML.OnnxRuntime.DirectML
OpenCvSharp4.Windows
此项目对OpenCv库的依赖度很低,仅仅在分割时有少量引用,图片的处理和绘制全都使用C#自带的库
如果在GPU推理时,刚开始速度很快,然后帧数很快降到很低,多数是显卡降频了,先更改一下设置
显卡设置:
为了发挥更高的性能,请在NVIDIA控制面板中设置以下选项
3D设置-通过预览调整图像质量-使用我的优先选择-性能拉满
管理3D设置-电源管理模式-最高性能优先;低延时模式-超高
 */
namespace yolo目标检测
{
    class BW_yolo
    {
        private InferenceSession 模型会话;
        private int 张量宽度, 张量高度;
        private string 模型输入名;
        private string 模型输出名;
        private int[] 输入张量信息;
        private int[] 输出张量信息;
        private int[] 输出张量信息2_分割;
        private int 推理图片宽度, 推理图片高度;
        private DenseTensor<float> 输入张量;
        private int yolo版本;
        private float mask缩放比例W = 0;
        private float mask缩放比例H = 0;
        private string 模型版本 = "";
        private string 任务类型 = "";
        private int 语义分割宽度 = 0;
        private int 动作宽度 = 0;
        private float 缩放比例 = 1;
        public string[] 标签组 { get; set; }
        private int 执行任务模式 = 0;
        /// <summary>
        /// 默认自动识别,若设置指定的任务模式:0=分类,1=检测,2=分割,3=检测+分割,4=动作,5=检测+动作,6=定向边界,如指定了模型不支持的模式,程序会自动识别并修改为支持的模式,当无法识别时,会默认为检测模式
        /// </summary>
        public int 任务模式
        {
            get { return 执行任务模式; }
            set
            {
                if (任务类型 == "classify")
                {
                    执行任务模式 = 0;
                }
                //检测
                else if (任务类型 == "detect")
                {
                    执行任务模式 = 1;
                }
                //分割
                else if (任务类型 == "segment")
                {
                    if (value == 1 || value == 2 || value == 3)
                    {
                        执行任务模式 = value;
                    }
                    else
                    {
                        执行任务模式 = 3;
                    }
                }
                //动作
                else if (任务类型 == "pose")
                {
                    if (value == 1 || value == 4 || value == 5)
                    {
                        执行任务模式 = value;
                    }
                    else
                    {
                        执行任务模式 = 5;
                    }
                }
                else if (任务类型 == "obb")
                {
                    if (value == 6)
                    {
                        执行任务模式 = value;
                    }
                    else
                    {
                        执行任务模式 = 6;
                    }
                }
                else
                {
                    //如果什么都数据都没有,默认是检测模型
                    执行任务模式 = 1;
                }
            }
        }
        /// <summary>
        /// 构造函数,目前支持yoloV5,yoloV6,yoloV8所转换的onnx模型,模型的详细信息可以推理网站https://netron.app/
        /// </summary>
        /// <param name="模型路径">必须使用ONNX模型</param>      
        /// <param name="yolo版本">如5,6,8为yolo版本号,默认为0自动检测,但可能会发生错误的判断,比如yolo6就需要指定，也可能是没有正确训练或进行过特殊调整导致的,如果错误判断,请手动指定</param>
        /// <param name="启用gpu">默认为false</param>
        public BW_yolo(string 模型路径, int Yolo版本 = 0, int GPU索引 = 0, bool 启用gpu = false)
        {
            try
            {
                if (启用gpu)
                {
                    SessionOptions 模式 = new SessionOptions();
                    模式.AppendExecutionProvider_DML(GPU索引);
                    模型会话 = new InferenceSession(模型路径, 模式);
                }
                else
                {
                    模型会话 = new InferenceSession(模型路径);
                }
                模型输入名 = 模型会话.InputNames.First();
                模型输出名 = 模型会话.OutputNames.First();
                输入张量信息 = 模型会话.InputMetadata[模型输入名].Dimensions;
                输出张量信息 = 模型会话.OutputMetadata[模型输出名].Dimensions;
                var 模型信息 = 模型会话.ModelMetadata.CustomMetadataMap;
                if (模型信息.Keys.Contains("names"))
                {
                    标签组 = 分割标签名(模型信息["names"]);
                }
                else
                {
                    标签组 = new string[0];
                }
                if (模型信息.Keys.Contains("version"))
                {
                    模型版本 = 模型信息["version"];
                }
                if (模型信息.Keys.Contains("task"))
                {
                    任务类型 = 模型信息["task"];
                    if (任务类型 == "segment")
                    {
                        string 模型输出名2 = 模型会话.OutputNames[1];
                        输出张量信息2_分割 = 模型会话.OutputMetadata[模型输出名2].Dimensions;
                        语义分割宽度 = 输出张量信息2_分割[1];
                        mask缩放比例W = 1f * 输出张量信息2_分割[3] / 输入张量信息[3];
                        mask缩放比例H = 1f * 输出张量信息2_分割[2] / 输入张量信息[2];
                    }
                    else if (任务类型 == "pose")
                    {
                        if (输出张量信息[1] > 输出张量信息[2])
                        {
                            动作宽度 = 输出张量信息[2] - 5;
                        }
                        else
                        {
                            动作宽度 = 输出张量信息[1] - 5;
                        }
                    }
                }
                else
                {
                    if (输出张量信息.Length == 2)
                    {
                        任务类型 = "classify";
                    }
                    else if (输出张量信息.Length == 3)
                    {
                        if (模型会话.OutputNames.Count == 1)
                        {
                            任务类型 = "detect";
                        }
                        else if (模型会话.OutputNames.Count == 2)
                        {
                            string 模型输出名2 = 模型会话.OutputNames[1];
                            输出张量信息2_分割 = 模型会话.OutputMetadata[模型输出名2].Dimensions;
                            语义分割宽度 = 输出张量信息2_分割[1];
                            mask缩放比例W = 1f * 输出张量信息2_分割[3] / 输入张量信息[3];
                            mask缩放比例H = 1f * 输出张量信息2_分割[2] / 输入张量信息[2];
                            任务类型 = "segment";
                        }
                        else
                        {
                            // throw new Exception("暂不支持的模型");
                        }
                    }
                    else
                    {
                        throw new Exception("暂不支持的模型");
                    }
                }
                任务模式 = 0;
                yolo版本 = 判断模型版本(Yolo版本);
                张量宽度 = 输入张量信息[3];
                张量高度 = 输入张量信息[2];          
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
        /// <summary>
        /// 推理主要函数
        /// </summary>
        /// <param name="图片数据"></param>
        /// <param name="置信度">0-1的小数,数值越高越快,检测精度要求也会越高.</param>
        /// <param name="iou阈值">0-1的小数,数值越高出现重复框的概率就越大,该值代表允许方框的最大重复面积</param>
        /// <param name="全局iou">false代表不同种类的框按允许重叠,true代表所有不同标签种类的框都要按照iou阈值不准重叠</param>  
        /// <param name="预处理模式">0表示高精度模式,对小物体有更高精度的检测;1表示高速模式,尤其是对大图像,大幅提高推理速度</param>   
        /// <returns>返回列表的基本数据格式{目标中心点x,目标中心点y,识别宽度,识别高度,置信度,标签索引}</returns>
        public List<yolo数据> 模型推理(Bitmap 图片数据, float 置信度 = 0.5f, float iou阈值 = 0.3f, bool 全局iou = false, int 预处理模式 = 1)
        {
            输入张量 = new DenseTensor<float>(输入张量信息);
            var sp = 输入张量.Buffer.Span;
            缩放比例 = 1;
            推理图片宽度 = 图片数据.Width;
            推理图片高度 = 图片数据.Height;
            if (预处理模式 == 0)
            {
                图片数据 = 图片缩放(图片数据);
                输入张量 = 图片写到张量_内存并行(图片数据, 输入张量信息);
            }
            else if (预处理模式 == 1)
            {
                输入张量 = 无插值写入张量(图片数据, 输入张量信息);
            }
            IReadOnlyCollection<NamedOnnxValue> 容器 = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(模型输入名, 输入张量) };
            Tensor<float> output0;
            Tensor<float> output1;
            List<yolo数据> 过滤数据数组;
            List<yolo数据> 最终返回数据 = new List<yolo数据>();
            if (执行任务模式 == 0)
            {
                output0 = 模型会话.Run(容器).First().AsTensor<float>();
                最终返回数据 = 置信度过滤_分类(output0, 置信度);
            }
            else if (执行任务模式 == 1)
            {
                output0 = 模型会话.Run(容器).First().AsTensor<float>();
                if (yolo版本 == 8)
                {
                    过滤数据数组 = 置信度过滤_yolo8_9检测(output0, 置信度);
                }
                else if (yolo版本 == 5)
                {
                    过滤数据数组 = 置信度过滤_yolo5检测(output0, 置信度);
                }
                else
                {
                    过滤数据数组 = 置信度过滤_yolo6检测(output0, 置信度);
                }
                最终返回数据 = nms过滤(过滤数据数组, iou阈值, 全局iou);
            }
            else if (执行任务模式 == 2 || 执行任务模式 == 3)
            {
                var 返回数据 = 模型会话.Run(容器);
                output0 = 返回数据.First().AsTensor<float>();
                output1 = 返回数据.ElementAtOrDefault(1)?.AsTensor<float>();
                if (yolo版本 == 8)
                {
                    过滤数据数组 = 置信度过滤_yolo8分割(output0, 置信度);
                }
                else
                {
                    过滤数据数组 = 置信度过滤_yolo5分割(output0, 置信度);
                }
                最终返回数据 = nms过滤(过滤数据数组, iou阈值, 全局iou);
                还原掩膜(ref 最终返回数据, output1);
            }
            else if (执行任务模式 == 4 || 执行任务模式 == 5)
            {
                output0 = 模型会话.Run(容器).First().AsTensor<float>();
                过滤数据数组 = 置信度过滤_动作(output0, 置信度);
                最终返回数据 = nms过滤(过滤数据数组, iou阈值, 全局iou);
            }
            else if (执行任务模式 == 6)
            {
                output0 = 模型会话.Run(容器).First().AsTensor<float>();
                过滤数据数组 = 置信度过滤_obb(output0, 置信度);
                最终返回数据 = nms过滤(过滤数据数组, iou阈值, 全局iou);
            }
            还原返回坐标(ref 最终返回数据);
            if (执行任务模式!=0)
            {
                去除越界坐标(ref 最终返回数据);
            }
            return 最终返回数据;
        }
        private Bitmap 图片缩放(Bitmap 图片数据)
        {
            float 缩放图片宽度 = 推理图片宽度;
            float 缩放图片高度 = 推理图片高度;
            if (缩放图片宽度 > 张量宽度 || 缩放图片高度 > 张量高度)
            {
                缩放比例 = (张量宽度 / 缩放图片宽度) < (张量高度 / 缩放图片高度) ? (张量宽度 / 缩放图片宽度) : (张量高度 / 缩放图片高度);
                缩放图片宽度 = 缩放图片宽度 * 缩放比例;
                缩放图片高度 = 缩放图片高度 * 缩放比例;
            }
            Bitmap 缩放后的图片 = new Bitmap((int)缩放图片宽度, (int)缩放图片高度);
            using (Graphics graphics = Graphics.FromImage(缩放后的图片))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                graphics.DrawImage(图片数据, 0, 0, 缩放图片宽度, 缩放图片高度);
            }
            return 缩放后的图片;
        }
        private DenseTensor<float> 图片写到张量_内存并行(Bitmap 图片数据, int[] 输入张量信息)
        {
            int 高 = 图片数据.Height;
            int 宽 = 图片数据.Width;
            BitmapData 内存图片数据 = 图片数据.LockBits(new Rectangle(0, 0, 宽, 高), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int stride = 内存图片数据.Stride; 
            IntPtr scan0 = 内存图片数据.Scan0; 
            float[,,] 临时数据 = new float[输入张量信息[1], 输入张量信息[2], 输入张量信息[3]];
            try
            {
                Parallel.For(0, 高, y =>
                {
                    for (int x = 0; x < 宽; x++)  
                    {
                        IntPtr 像素 = IntPtr.Add(scan0, y * stride + x * 3);
                        临时数据[2, y, x] = Marshal.ReadByte(像素) / 255f; 
                        像素 = IntPtr.Add(像素, 1);
                        临时数据[1, y, x] = Marshal.ReadByte(像素) / 255f;
                        像素 = IntPtr.Add(像素, 1);
                        临时数据[0, y, x] = Marshal.ReadByte(像素) / 255f;
                    }
                });
            }
            finally
            {
                图片数据.UnlockBits(内存图片数据); 
            }
            float[] 展开临时数据 = new float[输入张量信息[1] * 输入张量信息[2] * 输入张量信息[3]];
            Buffer.BlockCopy(临时数据, 0, 展开临时数据, 0, 展开临时数据.Length * 4);
            return new DenseTensor<float>(展开临时数据, 输入张量信息);
        }
        private DenseTensor<float> 无插值写入张量(Bitmap 图片数据, int[] 输入张量信息)
        {
            BitmapData 内存图片数据 = 图片数据.LockBits(new Rectangle(0, 0, 图片数据.Width, 图片数据.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int stride = 内存图片数据.Stride;  
            IntPtr scan0 = 内存图片数据.Scan0;  
            float[,,] 临时数据 = new float[输入张量信息[1], 输入张量信息[2], 输入张量信息[3]];
            float 缩放图片宽度 = 推理图片宽度;
            float 缩放图片高度 = 推理图片高度;
            if (缩放图片宽度 > 张量宽度 || 缩放图片高度 > 张量高度)
            {
                缩放比例 = (张量宽度 / 缩放图片宽度) < (张量高度 / 缩放图片高度) ? (张量宽度 / 缩放图片宽度) : (张量高度 / 缩放图片高度);
                缩放图片宽度 = 缩放图片宽度 * 缩放比例;
                缩放图片高度 = 缩放图片高度 * 缩放比例;
            }
            int x位置, y位置;
            float 系数 = 1 / 缩放比例;
            for (int y = 0; y < (int)缩放图片高度; y++)
            {
                for (int x = 0; x < (int)缩放图片宽度; x++)
                {
                    x位置 = (int)(x * 系数);
                    y位置 = (int)(y * 系数);
                    IntPtr 像素 = IntPtr.Add(scan0, y位置 * stride + x位置 * 3);
                    临时数据[2, y, x] = Marshal.ReadByte(像素) / 255f; 
                    像素 = IntPtr.Add(像素, 1);
                    临时数据[1, y, x] = Marshal.ReadByte(像素) / 255f;
                    像素 = IntPtr.Add(像素, 1);
                    临时数据[0, y, x] = Marshal.ReadByte(像素) / 255f;
                }
            }
            图片数据.UnlockBits(内存图片数据);
            float[] 展开临时数据 = new float[输入张量信息[1] * 输入张量信息[2] * 输入张量信息[3]];
            Buffer.BlockCopy(临时数据, 0, 展开临时数据, 0, 展开临时数据.Length * 4);
            return new DenseTensor<float>(展开临时数据, 输入张量信息);
        }
        public byte[] bitmap到byte(Bitmap 图片)
        {
            byte[] result = null;
            using (MemoryStream stream = new MemoryStream())
            {
                图片.Save(stream, ImageFormat.Bmp);
                result = stream.ToArray();
            }
            return result;
        }
        private List<yolo数据> 置信度过滤_yolo8分割(Tensor<float> 数据, float 置信度)
        {
            bool 中间是尺寸 = 数据.Dimensions[1] < 数据.Dimensions[2] ? true : false;
            if (中间是尺寸)
            {
                ConcurrentBag<yolo数据> 返回数组 = new ConcurrentBag<yolo数据>();
                Parallel.For(0, 数据.Dimensions[2], i =>
                {
                    float 临时置信度 = 0f;
                    int 索引 = -1;
                    for (int j = 0; j < 数据.Dimensions[1] - 4 - 语义分割宽度; j++)
                    {
                        if (数据[0, j + 4, i] >= 置信度)
                        {
                            if (临时置信度 < 数据[0, j + 4, i])
                            {
                                临时置信度 = 数据[0, j + 4, i];
                                索引 = j;
                            }
                        }
                    }
                    if (索引 != -1)
                    {
                        float[] 待加入数据 = new float[6];
                        yolo数据 临时 = new yolo数据();
                        Mat mask = new Mat(1, 32, MatType.CV_32F);
                        待加入数据[0] = 数据[0, 0, i];
                        待加入数据[1] = 数据[0, 1, i];
                        待加入数据[2] = 数据[0, 2, i];
                        待加入数据[3] = 数据[0, 3, i];
                        待加入数据[4] = 临时置信度;
                        待加入数据[5] = 索引;
                        for (int ii = 0; ii < 语义分割宽度; ii++)
                        {
                            int 位置 = 数据.Dimensions[1] - 语义分割宽度 + ii;
                            mask.At<float>(0, ii) = 数据[0, 位置, i];
                        }
                        临时.掩膜数据 = mask;
                        临时.基本数据 = 待加入数据;
                        返回数组.Add(临时);
                    }
                });
                return 返回数组.ToList<yolo数据>();
            }
            else
            {
                List<yolo数据> 返回数组 = new List<yolo数据>();
                int 输出尺寸 = 数据.Dimensions[2];
                float 临时置信度 = 0f;
                int 索引 = -1;
                float[] 数据2 = 数据.ToArray();
                for (int i = 0; i < 数据2.Length; i += 输出尺寸)
                {
                    临时置信度 = 0f;
                    索引 = -1;
                    for (int j = 0; j < 输出尺寸 - 4 - 语义分割宽度; j++)
                    {
                        if (数据2[i + 4 + j] > 置信度)
                        {
                            if (临时置信度 < 数据2[i + 4 + j])
                            {
                                临时置信度 = 数据2[i + 4 + j];
                                索引 = j;
                            }
                        }
                    }
                    if (索引 != -1)
                    {
                        float[] 待加入数据 = new float[6];
                        yolo数据 temp = new yolo数据();
                        Mat mask = new Mat(1, 32, MatType.CV_32F);
                        待加入数据[0] = 数据2[i];
                        待加入数据[1] = 数据2[i + 1];
                        待加入数据[2] = 数据2[i + 2];
                        待加入数据[3] = 数据2[i + 3];
                        待加入数据[4] = 临时置信度;
                        待加入数据[5] = 索引;
                        for (int ii = 0; ii < 语义分割宽度; ii++)
                        {
                            int 位置 = i + 输出尺寸 - 语义分割宽度 + ii;
                            mask.At<float>(0, ii) = 数据2[位置];
                        }
                        temp.掩膜数据 = mask;
                        temp.基本数据 = 待加入数据;
                        返回数组.Add(temp);
                    }
                }
                return 返回数组;
            }
        }
        private List<yolo数据> 置信度过滤_yolo8_9检测(Tensor<float> 数据, float 置信度)
        {
            bool 中间是尺寸 = 数据.Dimensions[1] < 数据.Dimensions[2] ? true : false;
            if (中间是尺寸)
            {
                ConcurrentBag<yolo数据> 返回数组 = new ConcurrentBag<yolo数据>();
                Parallel.For(0, 数据.Dimensions[2], i =>
                {
                    float 临时置信度 = 0f;
                    int 索引 = -1;
                    for (int j = 0; j < 数据.Dimensions[1] - 4 - 语义分割宽度 - 动作宽度; j++)
                    {
                        if (数据[0, j + 4, i] >= 置信度)
                        {
                            if (临时置信度 < 数据[0, j + 4, i])
                            {
                                临时置信度 = 数据[0, j + 4, i];
                                索引 = j;
                            }
                        }
                    }
                    if (索引 != -1)
                    {
                        float[] 待加入数据 = new float[6];
                        yolo数据 临时 = new yolo数据();
                        待加入数据[0] = 数据[0, 0, i];
                        待加入数据[1] = 数据[0, 1, i];
                        待加入数据[2] = 数据[0, 2, i];
                        待加入数据[3] = 数据[0, 3, i];
                        待加入数据[4] = 临时置信度;
                        待加入数据[5] = 索引;
                        临时.基本数据 = 待加入数据;
                        返回数组.Add(临时);
                    }
                });
                return 返回数组.ToList<yolo数据>();
            }
            else
            {
                List<yolo数据> 返回数组 = new List<yolo数据>();
                int 输出尺寸 = 数据.Dimensions[2];
                float 临时置信度 = 0f;
                int 索引 = -1;
                float[] 数据2 = 数据.ToArray();
                for (int i = 0; i < 数据2.Length; i += 输出尺寸)
                {
                    临时置信度 = 0f;
                    索引 = -1;
                    for (int j = 0; j < 输出尺寸 - 4 - 语义分割宽度 - 动作宽度; j++)
                    {
                        if (数据2[i + 4 + j] > 置信度)
                        {
                            if (临时置信度 < 数据2[i + 4 + j])
                            {
                                临时置信度 = 数据2[i + 4 + j];
                                索引 = j;
                            }
                        }
                    }
                    if (索引 != -1)
                    {
                        float[] 待加入数据 = new float[6];
                        yolo数据 临时 = new yolo数据();
                        待加入数据[0] = 数据2[i];
                        待加入数据[1] = 数据2[i + 1];
                        待加入数据[2] = 数据2[i + 2];
                        待加入数据[3] = 数据2[i + 3];
                        待加入数据[4] = 临时置信度;
                        待加入数据[5] = 索引;
                        临时.基本数据 = 待加入数据;
                        返回数组.Add(临时);
                    }
                }
                return 返回数组;
            }
        }
        private List<yolo数据> 置信度过滤_yolo5分割(Tensor<float> 数据, float 置信度)
        {
            bool 中间是尺寸 = 数据.Dimensions[1] < 数据.Dimensions[2] ? true : false;
            if (中间是尺寸)
            {
                ConcurrentBag<yolo数据> 返回数组 = new ConcurrentBag<yolo数据>();
                Parallel.For(0, 数据.Dimensions[2], i =>
                {
                    float 临时置信度 = 0f;
                    int 索引 = -1;
                    if (数据[0, 4, i] >= 置信度)
                    {
                        for (int j = 0; j < 数据.Dimensions[1] - 5 - 语义分割宽度; j++)
                        {
                            if (临时置信度 < 数据[0, j + 5, i])
                            {
                                临时置信度 = 数据[0, j + 5, i];
                                索引 = j;
                            }
                        }
                        if (索引 != -1)
                        {
                            float[] 待加入数据 = new float[6];
                            yolo数据 临时 = new yolo数据();
                            Mat mask = new Mat(1, 32, MatType.CV_32F);
                            待加入数据[0] = 数据[0, 0, i];
                            待加入数据[1] = 数据[0, 1, i];
                            待加入数据[2] = 数据[0, 2, i];
                            待加入数据[3] = 数据[0, 3, i];
                            待加入数据[4] = 临时置信度;
                            待加入数据[5] = 索引;
                            for (int ii = 0; ii < 语义分割宽度; ii++)
                            {
                                int 位置 = 数据.Dimensions[1] - 语义分割宽度 + ii;
                                mask.At<float>(0, ii) = 数据[0, 位置, i];
                            }
                            临时.掩膜数据 = mask;
                            临时.基本数据 = 待加入数据;
                            返回数组.Add(临时);
                        }
                    }
                });
                return 返回数组.ToList<yolo数据>();
            }
            else
            {
                List<yolo数据> 返回数组 = new List<yolo数据>();
                int 输出尺寸 = 数据.Dimensions[2];
                float 临时置信度 = 0f;
                int 索引 = -1;
                float[] 数据2 = 数据.ToArray();
                for (int i = 0; i < 数据2.Length; i += 输出尺寸)
                {
                    if (数据2[i + 4] >= 置信度)
                    {
                        临时置信度 = 0f;
                        for (int j = 0; j < 输出尺寸 - 5 - 语义分割宽度; j++)
                        {
                            if (临时置信度 < 数据2[i + 5 + j])
                            {
                                临时置信度 = 数据2[i + 5 + j];
                                索引 = j;
                            }
                        }
                        if (索引 != -1)
                        {
                            float[] 待加入数据 = new float[6];
                            yolo数据 临时 = new yolo数据();
                            Mat mask = new Mat(1, 32, MatType.CV_32F);
                            待加入数据[0] = 数据2[i];
                            待加入数据[1] = 数据2[i + 1];
                            待加入数据[2] = 数据2[i + 2];
                            待加入数据[3] = 数据2[i + 3];
                            待加入数据[4] = 数据2[i + 4];
                            待加入数据[5] = 索引;
                            for (int ii = 0; ii < 语义分割宽度; ii++)
                            {
                                int 位置 = i + 输出尺寸 - 语义分割宽度 + ii;
                                mask.At<float>(0, ii) = 数据2[位置];
                            }
                            临时.基本数据 = 待加入数据;
                            临时.掩膜数据 = mask;
                            返回数组.Add(临时);
                        }
                    }
                }
                return 返回数组;
            }
        }
        private List<yolo数据> 置信度过滤_yolo5检测(Tensor<float> 数据, float 置信度)
        {
            bool 中间是尺寸 = 数据.Dimensions[1] < 数据.Dimensions[2] ? true : false;
            if (中间是尺寸)
            {
                ConcurrentBag<yolo数据> 返回数组 = new ConcurrentBag<yolo数据>();
                Parallel.For(0, 数据.Dimensions[2], i =>
                {
                    float 临时置信度 = 0f;
                    int 索引 = -1;
                    if (数据[0, 4, i] >= 置信度)
                    {
                        for (int j = 0; j < 数据.Dimensions[1] - 5 - 语义分割宽度; j++)
                        {
                            if (临时置信度 < 数据[0, j + 5, i])
                            {
                                临时置信度 = 数据[0, j + 5, i];
                                索引 = j;
                            }
                        }
                        if (索引 != -1)
                        {
                            float[] 待加入数据 = new float[6];
                            yolo数据 临时 = new yolo数据();
                            待加入数据[0] = 数据[0, 0, i];
                            待加入数据[1] = 数据[0, 1, i];
                            待加入数据[2] = 数据[0, 2, i];
                            待加入数据[3] = 数据[0, 3, i];
                            待加入数据[4] = 临时置信度;
                            待加入数据[5] = 索引;
                            临时.基本数据 = 待加入数据;
                            返回数组.Add(临时);
                        }
                    }
                });
                return 返回数组.ToList<yolo数据>();
            }
            else
            {
                List<yolo数据> 返回数组 = new List<yolo数据>();
                int 输出尺寸 = 数据.Dimensions[2];
                float 临时置信度 = 0f;
                int 索引 = -1;
                float[] 数据2 = 数据.ToArray();
                for (int i = 0; i < 数据2.Length; i += 输出尺寸)
                {
                    if (数据2[i + 4] >= 置信度)
                    {
                        临时置信度 = 0f;
                        for (int j = 0; j < 输出尺寸 - 5 - 语义分割宽度; j++)
                        {
                            if (临时置信度 < 数据2[i + 5 + j])
                            {
                                临时置信度 = 数据2[i + 5 + j];
                                索引 = j;
                            }
                        }
                        if (索引 != -1)
                        {
                            float[] 待加入数据 = new float[6];
                            yolo数据 临时 = new yolo数据();
                            待加入数据[0] = 数据2[i];
                            待加入数据[1] = 数据2[i + 1];
                            待加入数据[2] = 数据2[i + 2];
                            待加入数据[3] = 数据2[i + 3];
                            待加入数据[4] = 数据2[i + 4];
                            待加入数据[5] = 索引;
                            临时.基本数据 = 待加入数据;
                            返回数组.Add(临时);
                        }
                    }
                }
                return 返回数组;
            }
        }
        private List<yolo数据> 置信度过滤_yolo6检测(Tensor<float> 数据, float 置信度)
        {
            bool 中间是尺寸 = 数据.Dimensions[1] < 数据.Dimensions[2] ? true : false;
            if (中间是尺寸)
            {
                ConcurrentBag<yolo数据> 返回数组 = new ConcurrentBag<yolo数据>();
                Parallel.For(0, 数据.Dimensions[2], i =>
                {
                    float 临时置信度 = 0f;
                    int 索引 = -1;
                    for (int j = 0; j < 数据.Dimensions[1] - 5 - 语义分割宽度 - 动作宽度; j++)
                    {
                        if (数据[0, j + 5, i] >= 置信度)
                        {
                            if (临时置信度 < 数据[0, j + 5, i])
                            {
                                临时置信度 = 数据[0, j + 5, i];
                                索引 = j;
                            }
                        }
                    }
                    if (索引 != -1)
                    {
                        float[] 待加入数据 = new float[6];
                        yolo数据 临时 = new yolo数据();
                        待加入数据[0] = 数据[0, 0, i];
                        待加入数据[1] = 数据[0, 1, i];
                        待加入数据[2] = 数据[0, 2, i];
                        待加入数据[3] = 数据[0, 3, i];
                        待加入数据[4] = 临时置信度;
                        待加入数据[5] = 索引;
                        临时.基本数据 = 待加入数据;
                        返回数组.Add(临时);
                    }
                });
                return 返回数组.ToList<yolo数据>();
            }
            else
            {
                List<yolo数据> 返回数组 = new List<yolo数据>();
                int 输出尺寸 = 数据.Dimensions[2];
                float 临时置信度 = 0f;
                int 索引 = -1;
                float[] 数据2 = 数据.ToArray();
                for (int i = 0; i < 数据2.Length; i += 输出尺寸)
                {
                    临时置信度 = 0f;
                    索引 = -1;
                    for (int j = 0; j < 输出尺寸 - 5 - 语义分割宽度 - 动作宽度; j++)
                    {
                        if (数据2[i + 5 + j] > 置信度)
                        {
                            if (临时置信度 < 数据2[i + 5 + j])
                            {
                                临时置信度 = 数据2[i + 5 + j];
                                索引 = j;
                            }
                        }
                    }
                    if (索引 != -1)
                    {
                        float[] 待加入数据 = new float[6];
                        yolo数据 临时 = new yolo数据();
                        待加入数据[0] = 数据2[i];
                        待加入数据[1] = 数据2[i + 1];
                        待加入数据[2] = 数据2[i + 2];
                        待加入数据[3] = 数据2[i + 3];
                        待加入数据[4] = 临时置信度;
                        待加入数据[5] = 索引;
                        临时.基本数据 = 待加入数据;
                        返回数组.Add(临时);
                    }
                }
                return 返回数组;
            }
        }
        private List<yolo数据> 置信度过滤_分类(Tensor<float> 数据, float 置信度)
        {
            List<yolo数据> 返回数组 = new List<yolo数据>();
            for (int i = 0; i < 数据.Dimensions[1]; i++)
            {
                if (数据[0, i] >= 置信度)
                {
                    float[] 过滤信息 = new float[2];
                    yolo数据 临时 = new yolo数据();
                    //标签的置信度
                    过滤信息[0] = 数据[0, i];
                    //标签的索引
                    过滤信息[1] = i;
                    临时.基本数据 = 过滤信息;
                    返回数组.Add(临时);
                }
            }
            冒泡排序置信度(ref 返回数组);
            return 返回数组;
        }
        private List<yolo数据> 置信度过滤_动作(Tensor<float> 数据, float 置信度)
        {
            bool 中间是尺寸 = 数据.Dimensions[1] < 数据.Dimensions[2] ? true : false;
            if (中间是尺寸)
            {
                ConcurrentBag<yolo数据> 返回数组 = new ConcurrentBag<yolo数据>();
                Parallel.For(0, 数据.Dimensions[2], i =>
                {
                    float 临时置信度 = 0f;
                    int 索引 = -1;
                    for (int j = 0; j < 数据.Dimensions[1] - 4 - 语义分割宽度 - 动作宽度; j++)
                    {
                        if (数据[0, j + 4, i] >= 置信度)
                        {
                            if (临时置信度 < 数据[0, j + 4, i])
                            {
                                临时置信度 = 数据[0, j + 4, i];
                                索引 = j;
                            }
                        }
                    }
                    if (索引 != -1)
                    {
                        float[] 待加入数据 = new float[6];
                        yolo数据 临时 = new yolo数据();
                        待加入数据[0] = 数据[0, 0, i];
                        待加入数据[1] = 数据[0, 1, i];
                        待加入数据[2] = 数据[0, 2, i];
                        待加入数据[3] = 数据[0, 3, i];
                        待加入数据[4] = 临时置信度;
                        待加入数据[5] = 索引;
                        临时.基本数据 = 待加入数据;
                        int 姿势索引 = 0;
                        Pose[] p = new Pose[动作宽度 / 3];
                        for (int ii = 0; ii < 动作宽度; ii += 3)
                        {
                            Pose p1 = new Pose();
                            p1.X = 数据[0, 5 + ii, i];
                            p1.Y = 数据[0, 6 + ii, i];
                            p1.V = 数据[0, 7 + ii, i];
                            p[姿势索引] = p1;
                            姿势索引++;
                        }
                        临时.关键点 = p;
                        返回数组.Add(临时);
                    }
                });
                return 返回数组.ToList<yolo数据>();
            }
            else
            {
                List<yolo数据> 返回数组 = new List<yolo数据>();
                float[] 数据2 = 数据.ToArray();
                int 输出尺寸 = 数据.Dimensions[2];
                float 临时置信度 = 0f;
                int 索引 = -1;
                for (int i = 0; i < 数据2.Length; i += 输出尺寸)
                {
                    临时置信度 = 0f;
                    索引 = -1;
                    for (int j = 0; j < 输出尺寸 - 4 - 动作宽度; j++)
                    {
                        if (数据2[i + 4 + j] > 置信度)
                        {
                            if (临时置信度 < 数据2[i + 4 + j])
                            {
                                临时置信度 = 数据2[i + 4 + j];
                                索引 = j;
                            }
                        }
                        if (索引 != -1)
                        {
                            float[] 待加入数据 = new float[6];
                            yolo数据 临时 = new yolo数据();
                            待加入数据[0] = 数据2[i];
                            待加入数据[1] = 数据2[i + 1];
                            待加入数据[2] = 数据2[i + 2];
                            待加入数据[3] = 数据2[i + 3];
                            待加入数据[4] = 临时置信度;
                            待加入数据[5] = 索引;
                            临时.基本数据 = 待加入数据;
                            int 姿势索引 = 0;
                            Pose[] p = new Pose[动作宽度 / 3];
                            for (int ii = 0; ii < 动作宽度; ii += 3)
                            {
                                Pose p1 = new Pose();
                                p1.X = 数据2[i + 5 + ii];
                                p1.Y = 数据2[i + 6 + ii];
                                p1.V = 数据2[i + 7 + ii];
                                p[姿势索引] = p1;
                                姿势索引++;
                            }
                            临时.关键点 = p;
                            返回数组.Add(临时);
                        }
                    }
                }
                return 返回数组;
            }
        }
        private List<yolo数据> 置信度过滤_obb(Tensor<float> 数据, float 置信度)
        {
            bool 中间是尺寸 = 数据.Dimensions[1] < 数据.Dimensions[2] ? true : false;
            if (中间是尺寸)
            {
                ConcurrentBag<yolo数据> 返回数组 = new ConcurrentBag<yolo数据>();
                int 输出尺寸 = 数据.Dimensions[1];
                Parallel.For(0, 数据.Dimensions[2], i =>
                {
                    float 临时置信度 = 0f;
                    int 索引 = -1;
                    for (int j = 0; j < 数据.Dimensions[1] - 5; j++)
                    {
                        if (数据[0, j + 4, i] >= 置信度)
                        {
                            if (临时置信度 < 数据[0, j + 4, i])
                            {
                                临时置信度 = 数据[0, j + 4, i];
                                索引 = j;
                            }
                        }
                    }
                    if (索引 != -1)
                    {
                        float[] 待加入数据 = new float[7];
                        yolo数据 临时 = new yolo数据();
                        待加入数据[0] = 数据[0, 0, i];
                        待加入数据[1] = 数据[0, 1, i];
                        待加入数据[2] = 数据[0, 2, i];
                        待加入数据[3] = 数据[0, 3, i];
                        待加入数据[4] = 临时置信度;
                        待加入数据[5] = 索引;
                        待加入数据[6] = 数据[0, 输出尺寸 - 1, i];
                        临时.基本数据 = 待加入数据;
                        返回数组.Add(临时);
                    }
                });
                return 返回数组.ToList<yolo数据>();
            }
            else
            {
                List<yolo数据> 返回数组 = new List<yolo数据>();
                int 输出尺寸 = 数据.Dimensions[2];
                float 临时置信度 = 0f;
                int 索引 = -1;
                float[] 数据2 = 数据.ToArray();
                for (int i = 0; i < 数据2.Length; i += 输出尺寸)
                {
                    临时置信度 = 0f;
                    索引 = -1;
                    for (int j = 0; j < 输出尺寸 - 5; j++)
                    {
                        if (数据2[i + 4 + j] > 置信度)
                        {
                            if (临时置信度 < 数据2[i + 4 + j])
                            {
                                临时置信度 = 数据2[i + 4 + j];
                                索引 = j;
                            }
                        }
                    }
                    if (索引 != -1)
                    {
                        float[] 待加入数据 = new float[7];
                        yolo数据 临时 = new yolo数据();
                        待加入数据[0] = 数据2[i];
                        待加入数据[1] = 数据2[i + 1];
                        待加入数据[2] = 数据2[i + 2];
                        待加入数据[3] = 数据2[i + 3];
                        待加入数据[4] = 临时置信度;
                        待加入数据[5] = 索引;
                        待加入数据[6] = 数据2[i + 输出尺寸 - 1];
                        临时.基本数据 = 待加入数据;
                        返回数组.Add(临时);
                    }
                }
                return 返回数组;
            }
        }
        private int 判断模型版本(int 版本号)
        {
            if (任务类型 == "classify")
            {
                return 5;
            }
            if (版本号 >= 8)
            {
                return 8;
            }
            else if (版本号 < 8 && 版本号 >= 5)
            {
                return 版本号;
            }
            if (模型版本 != "")
            {
                int 版本 = int.Parse(模型版本.Split('.')[0]);
                return 版本;
            }
            int 中间 = 输出张量信息[1];
            int 右边 = 输出张量信息[2];
            int 尺寸 = 中间 < 右边 ? 中间 : 右边;
            int 标签数量 = 标签组.Length;
            if (标签数量 == 尺寸 - 4 - 语义分割宽度)
            {
                return 8;
            }
            if (标签数量 == 0 && 中间 < 右边)
            {
                return 8;
            }
            return 5;
        }
        private List<yolo数据> nms过滤(List<yolo数据> 首次过滤数组, float iou阈值, bool 全局iou)
        {
            冒泡排序置信度(ref 首次过滤数组);
            List<yolo数据> nms过滤数组 = new List<yolo数据>();
            bool 是否满足 = true;
            for (int i = 0; i < 首次过滤数组.Count; i++)
            {
                for (int j = 0; j < nms过滤数组.Count; j++)
                {
                    if (全局iou || 首次过滤数组[i].基本数据[5] == nms过滤数组[j].基本数据[5])
                    {
                        float a = 计算交并比(首次过滤数组[i].基本数据, nms过滤数组[j].基本数据);
                        if (a > iou阈值)
                        {
                            是否满足 = false;
                            break;
                        }
                        else
                        {
                            是否满足 = true;
                        }
                    }
                    else
                    {
                        是否满足 = true;
                    }
                }
                if (是否满足) nms过滤数组.Add(首次过滤数组[i]);
            }
            return nms过滤数组;
        }
        private float 计算交并比(float[] 矩形1, float[] 矩形2)
        {
            float[] 矩形3 = new float[4];
            float[] 矩形4 = new float[4];
            矩形3[0] = 矩形1[0] - 矩形1[2] / 2;
            矩形3[1] = 矩形1[1] - 矩形1[3] / 2;
            矩形3[2] = 矩形1[0] + 矩形1[2] / 2;
            矩形3[3] = 矩形1[1] + 矩形1[3] / 2;
            矩形4[0] = 矩形2[0] - 矩形2[2] / 2;
            矩形4[1] = 矩形2[1] - 矩形2[3] / 2;
            矩形4[2] = 矩形2[0] + 矩形2[2] / 2;
            矩形4[3] = 矩形2[1] + 矩形2[3] / 2;
            float 交集面积, 并集面积;
            float 左边界 = Math.Max(矩形3[0], 矩形4[0]);
            float 上边界 = Math.Max(矩形3[1], 矩形4[1]);
            float 右边界 = Math.Min(矩形3[2], 矩形4[2]);
            float 下边界 = Math.Min(矩形3[3], 矩形4[3]);
            if (左边界 < 右边界 && 上边界 < 下边界)
            {
                交集面积 = (右边界 - 左边界) * (下边界 - 上边界);
            }
            else
            {
                交集面积 = 0;
            }
            float 面积1 = (矩形3[2] - 矩形3[0]) * (矩形3[3] - 矩形3[1]);
            float 面积2 = (矩形4[2] - 矩形4[0]) * (矩形4[3] - 矩形4[1]);
            并集面积 = 面积1 + 面积2 - 交集面积;
            return 交集面积 / 并集面积;
        }
        private void 冒泡排序置信度(ref List<yolo数据> 数据数组)
        {
            int n = 数据数组.Count;
            if (n > 0)
            {
                if (数据数组[0].基本数据.Length == 2)
                {
                    for (int i = 0; i < n - 1; i++)
                    {
                        for (int j = 0; j < n - i - 1; j++)
                        {
                            if (数据数组[j].基本数据[0] < 数据数组[j + 1].基本数据[0])
                            {
                                yolo数据 temp = 数据数组[j];
                                数据数组[j] = 数据数组[j + 1];
                                数据数组[j + 1] = temp;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < n - 1; i++)
                    {
                        for (int j = 0; j < n - i - 1; j++)
                        {
                            if (数据数组[j].基本数据[4] < 数据数组[j + 1].基本数据[4])
                            {
                                yolo数据 temp = 数据数组[j];
                                数据数组[j] = 数据数组[j + 1];
                                数据数组[j + 1] = temp;
                            }
                        }
                    }
                }
            }
        }
        private string[] 分割标签名(string 名)
        {
            string 删括号后 = 名.Replace("{", "").Replace("}", "");
            string[] 分割数组 = 删括号后.Split(',');
            string[] 返回数组 = new string[分割数组.Length];
            for (int i = 0; i < 分割数组.Length; i++)
            {
                int 分割起点 = 分割数组[i].IndexOf(':') + 3;
                int 分割终点 = 分割数组[i].Length - 1;
                返回数组[i] = 分割数组[i].Substring(分割起点, 分割终点 - 分割起点);
            }
            return 返回数组;
        }
        /// <summary>
        /// 取出模型中预设分类标签名
        /// </summary>
        /// <param name="索引"></param>
        /// <returns></returns>
        public string 取指定索引标签名(int 索引)
        {
            if (索引 > 标签组.Length || 索引 < 0)
            {
                return "";
            }
            return 标签组[索引];
        }
        private void 还原返回坐标(ref List<yolo数据> 数据列表)
        {
            if (数据列表.Count > 0)
            {
                if (数据列表[0].基本数据.Length > 2)
                {
                    for (int i = 0; i < 数据列表.Count; i++)
                    {
                        数据列表[i].基本数据[0] = 数据列表[i].基本数据[0] / 缩放比例;
                        数据列表[i].基本数据[1] = 数据列表[i].基本数据[1] / 缩放比例;
                        数据列表[i].基本数据[2] = 数据列表[i].基本数据[2] / 缩放比例;
                        数据列表[i].基本数据[3] = 数据列表[i].基本数据[3] / 缩放比例;
                    }
                }
                if (数据列表[0].关键点 != null)
                {
                    for (int i = 0; i < 数据列表.Count; i++)
                    {
                        for (int j = 0; j < 数据列表[i].关键点.Length; j++)
                        {
                            数据列表[i].关键点[j].X = 数据列表[i].关键点[j].X / 缩放比例;
                            数据列表[i].关键点[j].Y = 数据列表[i].关键点[j].Y / 缩放比例;
                        }
                    }
                }
            }
        }
        private void 还原画图坐标(ref List<yolo数据> 数据列表)
        {
            if (数据列表.Count > 0 && 数据列表[0].基本数据.Length > 2)
            {
                for (int i = 0; i < 数据列表.Count; i++)
                {
                    数据列表[i].基本数据[0] = 数据列表[i].基本数据[0] - 数据列表[i].基本数据[2] / 2;
                    数据列表[i].基本数据[1] = 数据列表[i].基本数据[1] - 数据列表[i].基本数据[3] / 2;
                }
            }
        }
        private void 还原中心坐标(ref List<yolo数据> 数据列表)
        {
            if (数据列表.Count > 0 && 数据列表[0].基本数据.Length > 2)
            {
                for (int i = 0; i < 数据列表.Count; i++)
                {
                    数据列表[i].基本数据[0] = 数据列表[i].基本数据[0] + 数据列表[i].基本数据[2] / 2;
                    数据列表[i].基本数据[1] = 数据列表[i].基本数据[1] + 数据列表[i].基本数据[3] / 2;
                }
            }
        }
        private void 去除越界坐标(ref List<yolo数据> 数据列表)
        {
            //倒序移除
            for (int i = 数据列表.Count - 1; i >= 0; i--)
            {
                if (数据列表[i].基本数据[0] > 推理图片宽度 || 数据列表[i].基本数据[1] > 推理图片高度 || 数据列表[i].基本数据[2] > 推理图片宽度 || 数据列表[i].基本数据[3] > 推理图片高度)
                {
                    数据列表.RemoveAt(i);
                }
            }
        }
        private void 还原掩膜(ref List<yolo数据> 数据, Tensor<float>? output1)
        {
            Mat ot1 = new Mat(语义分割宽度, 输出张量信息2_分割[2] * 输出张量信息2_分割[3], MatType.CV_32F, output1.ToArray());
            for (int i = 0; i < 数据.Count; i++)
            {
                Mat 原始mask = 数据[i].掩膜数据 * ot1;
                Parallel.For(0, 原始mask.Cols, col =>
                {
                    原始mask.At<float>(0, col) = Sigmoid(原始mask.At<float>(0, col));
                });
                Mat 重塑mask = 原始mask.Reshape(1, 输出张量信息2_分割[2], 输出张量信息2_分割[3]);
                int maskX1 = Math.Abs((int)((数据[i].基本数据[0] - 数据[i].基本数据[2] / 2) * mask缩放比例W));
                int masky1 = Math.Abs((int)((数据[i].基本数据[1] - 数据[i].基本数据[3] / 2) * mask缩放比例H));
                int maskx2 = (int)(数据[i].基本数据[2] * mask缩放比例W);
                int masky2 = (int)(数据[i].基本数据[3] * mask缩放比例H);
                if (maskx2 + maskX1 > 输出张量信息2_分割[3]) maskx2 = 输出张量信息2_分割[3] - maskX1;
                if (masky1 + masky2 > 输出张量信息2_分割[2]) masky2 = 输出张量信息2_分割[2] - masky1;
                Rect 区域 = new Rect(maskX1, masky1, maskx2, masky2);
                Mat 裁剪后 = new Mat(重塑mask, 区域);
                Mat 还原原图掩膜 = new Mat();
                int 放大宽度 = (int)(裁剪后.Width / mask缩放比例W / 缩放比例);
                int 放大高度 = (int)(裁剪后.Height / mask缩放比例H / 缩放比例);
                Cv2.Resize(裁剪后, 还原原图掩膜, new OpenCvSharp.Size(放大宽度, 放大高度));
                Cv2.Threshold(还原原图掩膜, 还原原图掩膜, 0.5, 1, ThresholdTypes.Binary);
                数据[i].掩膜数据 = 还原原图掩膜;
            }
        }
        private float Sigmoid(float value)
        {
            return 1 / (1 + (float)Math.Exp(-value));
        }
        /// <returns>返回OBB矩形结构,分别代表了四个点的坐标</returns>
        public OBB矩形结构 OBB坐标转换(yolo数据 数据)
        {
            float x = 数据.基本数据[0];
            float y = 数据.基本数据[1];
            float w = 数据.基本数据[2];
            float h = 数据.基本数据[3];
            float r = 数据.基本数据[6];
            float cos_value = (float)Math.Cos(r);
            float sin_value = (float)Math.Sin(r);
            float[] vec1 = { w / 2 * cos_value, w / 2 * sin_value };
            float[] vec2 = { -h / 2 * sin_value, h / 2 * cos_value };
            OBB矩形结构 oBBrectangle = new OBB矩形结构();
            oBBrectangle.pt1 = new PointF(x + vec1[0] + vec2[0], y + vec1[1] + vec2[1]);
            oBBrectangle.pt2 = new PointF(x + vec1[0] - vec2[0], y + vec1[1] - vec2[1]);
            oBBrectangle.pt3 = new PointF(x - vec1[0] - vec2[0], y - vec1[1] - vec2[1]);
            oBBrectangle.pt4 = new PointF(x - vec1[0] + vec2[0], y - vec1[1] + vec2[1]);
            return oBBrectangle;
        }
        /// <summary>
        /// 在原图像上绘制推理结果,同时受任务模式影响，如在分割模型上指定任务模式为检测，那么调用该方法生成的图像也只会画框，不会出现掩膜
        /// </summary>
        /// <param name="图片">原图像</param>
        /// <param name="yolo返回数据">由处理坐标返回的坐标组</param>
        /// <param name="标签组">类属性 标签组 或者手动传入一个数组</param>
        /// <param name="边框画笔">指定一个边框画笔,如果为空则根据图片尺寸自适应</param>
        /// <param name="字体">指定一个字体,如果为空则根据图片尺寸自适应</param>
        /// <param name="文字颜色笔刷">指定一个文字颜色笔刷,默认为黑</param>
        /// <param name="文字底色笔刷">指定一个文字底色笔刷,默认为橙色</param>
        /// <param name="分割掩膜随机色">每一个目标都使用随机色掩膜,默认为真,为假时,统一使用绿色。当下个参数指定了掩膜颜色时,该参数失效</param>
        /// <param name="指定标签掩膜颜色">使用Color的ARGB颜色数组为同一个标签类别指定相同的颜色，当提供的数组的数量小于的标签的实际索引时，会使用默认的绿色，所以请确保数组的颜色数量大于标签数量</param>
        /// <param name="非掩膜背景色">非掩膜的背景蒙版填充,默认无。与掩膜色相同,使用ARGB颜色,注意,该颜色的透明度会与掩膜颜色的透明度叠加,需自行调整到一个合理的值</param>
        /// <param name="分类显示数量">分类模型显示分类标签的数量,默认为5,即最多显示5个,分类标签的文字颜色、大小、底色,可直接从前面的参数指定</param>
        /// <param name="关键点可信度阈值">用于决定显示关键点的可信度阈值,通常默认0.5,即高于0.5就显示,低于0.5就不显示,通常预测的点在框外的话就会低于0.5</param>
        /// <returns>返回绘制后的图像</returns>
        public Image 生成图像(Image 图片, List<yolo数据> 返回数据, string[] 标签组, Pen? 边框画笔 = null, Font 字体 = null, SolidBrush 文字颜色笔刷 = null, SolidBrush 文字底色笔刷 = null, bool 分割掩膜随机色 = true, Color[] 指定标签掩膜颜色 = null, Color? 非掩膜背景色 = null, int 分类显示数量 = 5, float 关键点可信度阈值 = 0.5f)
        {
            Bitmap 返回图像 = new Bitmap(图片.Width, 图片.Height);
            Graphics 绘制类 = Graphics.FromImage(返回图像);
            绘制类.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            if (边框画笔 == null)
            {
                int 画笔宽 = (图片.Width > 图片.Height ? 图片.Height : 图片.Width) / 135;
                边框画笔 = new Pen(Color.BlueViolet, 画笔宽);
            }
            if (字体 == null)
            {
                int 字体宽 = (图片.Width > 图片.Height ? 图片.Height : 图片.Width) / 38;
                字体 = new Font("宋体", 字体宽, FontStyle.Bold);
            }
            if (文字颜色笔刷 == null) 文字颜色笔刷 = new SolidBrush(Color.Black);
            if (文字底色笔刷 == null) 文字底色笔刷 = new SolidBrush(Color.Orange);
            float 文字宽度;
            float 文字高度;
            绘制类.DrawImage(图片, 0, 0, 图片.Width, 图片.Height);
            string 写字内容;
            //分类
            if (执行任务模式 == 0)
            {
                还原画图坐标(ref 返回数据);
                float x位置 = 10;
                float y位置 = 10;
                for (int i = 0; i < 返回数据.Count; i++)
                {
                    if (i >= 分类显示数量) break;
                    int 标签序号 = (int)返回数据[i].基本数据[1];
                    string 置信度 = 返回数据[i].基本数据[0].ToString("_0.00");
                    string 标签名;
                    if (标签序号 + 1 > 标签组.Length)
                    {
                        标签名 = "无类别名称";
                    }
                    else
                    {
                        标签名 = 标签组[标签序号];
                    }
                    写字内容 = 标签名 + 置信度;
                    文字宽度 = 绘制类.MeasureString(写字内容 + "_0.00", 字体).Width;
                    文字高度 = 绘制类.MeasureString(写字内容 + "_0.00", 字体).Height;
                    绘制类.FillRectangle(文字底色笔刷, x位置, y位置, 文字宽度 * 0.8f, 文字高度);
                    绘制类.DrawString(写字内容, 字体, 文字颜色笔刷, new PointF(x位置, y位置));
                    y位置 += 文字高度;
                }
                还原中心坐标(ref 返回数据);
            }
            //画掩膜
            if (执行任务模式 == 2 || 执行任务模式 == 3)
            {
                还原画图坐标(ref 返回数据);
                if (非掩膜背景色 != null)
                {
                    Bitmap 背景图 = new Bitmap(图片.Width, 图片.Height);
                    Graphics 背景绘制 = Graphics.FromImage(背景图);
                    背景绘制.Clear((Color)非掩膜背景色);
                    背景绘制.Dispose();
                    绘制类.DrawImage(背景图, PointF.Empty);
                }
                for (int i = 0; i < 返回数据.Count; i++)
                {
                    Rectangle 矩形 = new Rectangle((int)返回数据[i].基本数据[0], (int)返回数据[i].基本数据[1], (int)返回数据[i].基本数据[2], (int)返回数据[i].基本数据[3]);
                    Color 颜色 = new Color();
                    if (指定标签掩膜颜色 == null)
                    {
                        if (分割掩膜随机色)
                        {
                            Random R = new Random();
                            颜色 = Color.FromArgb(180, R.Next(0, 255), R.Next(0, 255), R.Next(0, 255));
                        }
                        else
                        {
                            颜色 = Color.FromArgb(180, 0, 255, 0);
                        }
                    }
                    else
                    {
                        if ((int)返回数据[i].基本数据[5] + 1 > 指定标签掩膜颜色.Length)
                        {
                            颜色 = Color.FromArgb(180, 0, 255, 0);
                        }
                        else
                        {
                            颜色 = 指定标签掩膜颜色[(int)返回数据[i].基本数据[5]];
                        }
                    }
                    Bitmap 掩膜 = 生成掩膜图像_内存并行(返回数据[i].掩膜数据, 颜色);
                    绘制类.DrawImage(掩膜, 矩形);
                }
                还原中心坐标(ref 返回数据);
            }
            if (执行任务模式 == 1 || 执行任务模式 == 3 || 执行任务模式 == 5)
            {
                还原画图坐标(ref 返回数据);
                for (int i = 0; i < 返回数据.Count; i++)
                {
                    string 置信度 = 返回数据[i].基本数据[4].ToString("_0.00");
                    if ((int)返回数据[i].基本数据[5] + 1 > 标签组.Length)
                    {
                        写字内容 = 置信度;
                    }
                    else
                    {
                        写字内容 = 标签组[(int)返回数据[i].基本数据[5]] + 置信度;
                    }
                    文字宽度 = 绘制类.MeasureString(写字内容 + "_0.00", 字体).Width;
                    文字高度 = 绘制类.MeasureString(写字内容 + "_0.00", 字体).Height;
                    Rectangle 矩形 = new Rectangle((int)返回数据[i].基本数据[0], (int)返回数据[i].基本数据[1], (int)返回数据[i].基本数据[2], (int)返回数据[i].基本数据[3]);
                    绘制类.DrawRectangle(边框画笔, 矩形);
                    绘制类.FillRectangle(文字底色笔刷, 返回数据[i].基本数据[0] - 边框画笔.Width / 2 - 1, 返回数据[i].基本数据[1] - 文字高度 - 边框画笔.Width / 2 - 1, 文字宽度 * 0.8f, 文字高度);
                    绘制类.DrawString(写字内容, 字体, 文字颜色笔刷, 返回数据[i].基本数据[0] - 边框画笔.Width / 2 - 1, 返回数据[i].基本数据[1] - 文字高度 - 边框画笔.Width / 2 - 1);
                }
                还原中心坐标(ref 返回数据);
            }
            if (执行任务模式 == 4 || 执行任务模式 == 5)
            {
                还原画图坐标(ref 返回数据);
                if (返回数据.Count > 0 && 返回数据[0].关键点.Length == 17)
                {
                    Color[] 颜色组 = new Color[]
{ 
        Color.Yellow,
        Color.LawnGreen,
        Color.LawnGreen,
        Color.SpringGreen,
        Color.SpringGreen,
        Color.Blue,
        Color.Blue,
        Color.Firebrick,
        Color.Firebrick,
        Color.Firebrick,
        Color.Firebrick,
        Color.Blue,
        Color.Blue,
        Color.Orange,
        Color.Orange,
        Color.Orange,
        Color.Orange
};
                    int 圆点半径 = (图片.Width > 图片.Height ? 图片.Height : 图片.Width) / 100;
                    int 线条宽度 = (图片.Width > 图片.Height ? 图片.Height : 图片.Width) / 150;
                    Pen 线条样式;
                    for (int i = 0; i < 返回数据.Count; i++)
                    {
                        线条样式 = new Pen(new SolidBrush(颜色组[0]), 线条宽度);
                        PointF 肩膀中心点 = new PointF((返回数据[i].关键点[5].X + 返回数据[i].关键点[6].X) / 2 + 圆点半径, (返回数据[i].关键点[5].Y + 返回数据[i].关键点[6].Y) / 2 + 圆点半径);
                        if (返回数据[i].关键点[0].V > 关键点可信度阈值 && 返回数据[i].关键点[5].V > 关键点可信度阈值 && 返回数据[i].关键点[6].V > 关键点可信度阈值) 绘制类.DrawLine(线条样式, new PointF(返回数据[i].关键点[0].X + 圆点半径, 返回数据[i].关键点[0].Y + 圆点半径), 肩膀中心点);
                        线条样式 = new Pen(new SolidBrush(颜色组[5]), 线条宽度);
                        if (返回数据[i].关键点[5].V > 关键点可信度阈值 && 返回数据[i].关键点[6].V > 关键点可信度阈值) 绘制类.DrawLine(线条样式, new PointF(返回数据[i].关键点[5].X + 圆点半径, 返回数据[i].关键点[5].Y + 圆点半径), new PointF(返回数据[i].关键点[6].X + 圆点半径, 返回数据[i].关键点[6].Y + 圆点半径));
                        if (返回数据[i].关键点[11].V > 关键点可信度阈值 && 返回数据[i].关键点[12].V > 关键点可信度阈值) 绘制类.DrawLine(线条样式, new PointF(返回数据[i].关键点[11].X + 圆点半径, 返回数据[i].关键点[11].Y + 圆点半径), new PointF(返回数据[i].关键点[12].X + 圆点半径, 返回数据[i].关键点[12].Y + 圆点半径));
                        if (返回数据[i].关键点[5].V > 关键点可信度阈值 && 返回数据[i].关键点[11].V > 关键点可信度阈值) 绘制类.DrawLine(线条样式, new PointF(返回数据[i].关键点[5].X + 圆点半径, 返回数据[i].关键点[5].Y + 圆点半径), new PointF(返回数据[i].关键点[11].X + 圆点半径, 返回数据[i].关键点[11].Y + 圆点半径));
                        if (返回数据[i].关键点[6].V > 关键点可信度阈值 && 返回数据[i].关键点[12].V > 关键点可信度阈值) 绘制类.DrawLine(线条样式, new PointF(返回数据[i].关键点[6].X + 圆点半径, 返回数据[i].关键点[6].Y + 圆点半径), new PointF(返回数据[i].关键点[12].X + 圆点半径, 返回数据[i].关键点[12].Y + 圆点半径));
                        线条样式 = new Pen(new SolidBrush(颜色组[0]), 线条宽度);
                        if (返回数据[i].关键点[0].V > 关键点可信度阈值 && 返回数据[i].关键点[1].V > 关键点可信度阈值) 绘制类.DrawLine(线条样式, new PointF(返回数据[i].关键点[0].X + 圆点半径, 返回数据[i].关键点[0].Y + 圆点半径), new PointF(返回数据[i].关键点[1].X + 圆点半径, 返回数据[i].关键点[1].Y + 圆点半径));
                        if (返回数据[i].关键点[0].V > 关键点可信度阈值 && 返回数据[i].关键点[2].V > 关键点可信度阈值) 绘制类.DrawLine(线条样式, new PointF(返回数据[i].关键点[0].X + 圆点半径, 返回数据[i].关键点[0].Y + 圆点半径), new PointF(返回数据[i].关键点[2].X + 圆点半径, 返回数据[i].关键点[2].Y + 圆点半径));
                        线条样式 = new Pen(new SolidBrush(颜色组[1]), 线条宽度);
                        if (返回数据[i].关键点[1].V > 关键点可信度阈值 && 返回数据[i].关键点[3].V > 关键点可信度阈值) 绘制类.DrawLine(线条样式, new PointF(返回数据[i].关键点[1].X + 圆点半径, 返回数据[i].关键点[1].Y + 圆点半径), new PointF(返回数据[i].关键点[3].X + 圆点半径, 返回数据[i].关键点[3].Y + 圆点半径));
                        if (返回数据[i].关键点[2].V > 关键点可信度阈值 && 返回数据[i].关键点[4].V > 关键点可信度阈值) 绘制类.DrawLine(线条样式, new PointF(返回数据[i].关键点[2].X + 圆点半径, 返回数据[i].关键点[2].Y + 圆点半径), new PointF(返回数据[i].关键点[4].X + 圆点半径, 返回数据[i].关键点[4].Y + 圆点半径));
                        for (int j = 5; j < 返回数据[i].关键点.Length - 2; j++)
                        {
                            if (返回数据[i].关键点[j].V > 关键点可信度阈值 && 返回数据[i].关键点[j + 2].V > 关键点可信度阈值)
                            {
                                if (j != 9 && j != 10)
                                {
                                    线条样式 = new Pen(new SolidBrush(颜色组[j + 2]), 线条宽度);
                                    绘制类.DrawLine(线条样式, new PointF(返回数据[i].关键点[j].X + 圆点半径, 返回数据[i].关键点[j].Y + 圆点半径), new PointF(返回数据[i].关键点[j + 2].X + 圆点半径, 返回数据[i].关键点[j + 2].Y + 圆点半径));
                                }
                            }
                        }
                        for (int j = 0; j < 返回数据[i].关键点.Length; j++)
                        {
                            if (返回数据[i].关键点[j].V > 关键点可信度阈值)
                            {
                                Rectangle 位置 = new Rectangle((int)返回数据[i].关键点[j].X, (int)返回数据[i].关键点[j].Y, 圆点半径 * 2, 圆点半径 * 2);
                                绘制类.FillEllipse(new SolidBrush(颜色组[j]), 位置);
                            }
                        }
                    }
                }
                else if (返回数据.Count > 0)
                {
                    Color[] 颜色组 = new Color[]
{
        Color.Yellow,
        Color.Red,
        Color.SpringGreen,
        Color.Blue,
        Color.Firebrick,
        Color.Blue,
        Color.Orange,
        Color.Beige,
        Color.LightGreen,
        Color.DarkGreen,
        Color.Magenta,
        Color.White,
        Color.OrangeRed,
        Color.Orchid,
        Color.PaleGoldenrod,
        Color.PaleGreen,
        Color.PaleTurquoise,
        Color.PaleVioletRed,
        Color.PaleGreen,
        Color.PaleTurquoise,
};
                    int 圆点半径 = (图片.Width > 图片.Height ? 图片.Height : 图片.Width) / 100;
                    foreach (var item in 返回数据)
                    {
                        for (int i = 0; i < item.关键点.Length; i++)
                        {
                            if (item.关键点[i].V > 关键点可信度阈值)
                            {
                                Rectangle 位置 = new Rectangle((int)item.关键点[i].X, (int)item.关键点[i].Y, 圆点半径 * 2, 圆点半径 * 2);
                                绘制类.FillEllipse(i > 20 ? new SolidBrush(Color.SaddleBrown) : new SolidBrush(颜色组[i]), 位置);
                            }
                        }
                    }
                }
                还原中心坐标(ref 返回数据);
            }
            else if (执行任务模式 == 6)
            {
                for (int i = 0; i < 返回数据.Count; i++)
                {
                    string 置信度 = 返回数据[i].基本数据[4].ToString("_0.00");
                    if ((int)返回数据[i].基本数据[5] + 1 > 标签组.Length)
                    {
                        写字内容 = 置信度;
                    }
                    else
                    {
                        写字内容 = 标签组[(int)返回数据[i].基本数据[5]] + 置信度;
                    }
                    文字宽度 = 绘制类.MeasureString(写字内容 + "_0.00", 字体).Width;
                    文字高度 = 绘制类.MeasureString(写字内容 + "_0.00", 字体).Height;
                    OBB矩形结构 obb = OBB坐标转换(返回数据[i]);
                    PointF[] pf = { obb.pt1, obb.pt2, obb.pt3, obb.pt4, obb.pt1 };
                    绘制类.DrawLines(边框画笔, pf);
                    PointF 右下角点 = pf[0];
                    foreach (var point in pf)
                    {
                        if (point.X >= 右下角点.X && point.Y >= 右下角点.Y)
                        {
                            右下角点 = point;
                        }
                    }
                    绘制类.FillRectangle(文字底色笔刷, 右下角点.X - 边框画笔.Width / 2 - 1, 右下角点.Y + 边框画笔.Width / 2 - 1, 文字宽度 * 0.8f, 文字高度);
                    绘制类.DrawString(写字内容, 字体, 文字颜色笔刷, 右下角点.X - 边框画笔.Width / 2 - 1, 右下角点.Y + 边框画笔.Width / 2 - 1);
                }
            }
            绘制类.Dispose();
            return 返回图像;
        }
        private Bitmap 生成掩膜图像_内存并行(Mat mat数据, Color 颜色)
        {
            Bitmap 掩膜图 = new Bitmap(mat数据.Width, mat数据.Height, PixelFormat.Format32bppArgb);
            BitmapData 掩膜图数据 = 掩膜图.LockBits(new Rectangle(0, 0, 掩膜图.Width, 掩膜图.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int 高 = 掩膜图.Height;
            int 宽 = 掩膜图.Width;
            Parallel.For(0, 高, i =>
            {
                for (int j = 0; j < 宽; j++)
                {
                    if (mat数据.At<float>(i, j) == 1)
                    {
                        IntPtr 起始像素 = IntPtr.Add(掩膜图数据.Scan0, i * 掩膜图数据.Stride + j * 4);
                        byte[] 颜色信息 = new byte[] { 颜色.B, 颜色.G, 颜色.R, 颜色.A };
                        Marshal.Copy(颜色信息, 0, 起始像素, 4);
                    }
                }
            });
            掩膜图.UnlockBits(掩膜图数据);
            return 掩膜图;
        }
        /// <summary>
        /// 使用结束后,应调用本方法释放资源
        /// </summary>
        public void 释放资源()
        {
            模型会话.Dispose();
        }
    }
    class yolo数据
    {
        /// <summary>
        /// 用于存放6个基本数据:中心x,中心y,宽,高,置信度,标签索引；如果是obb模型会返回7个数据,最后是旋转角度r；如使用了分类模型,则只会返回2个数据,分别代表置信度和标签索引
        /// </summary>
        public float[] 基本数据 { get; set; }
        /// <summary>
        /// 初期用于存放分割模型的32个mask数据,在后处理中,会对该值重新赋予一个还原后的掩膜数据。它是由0和1组成的矩阵，1代表掩膜部分、掩膜的起始坐标就是对应目标检测框的左上角,你可以理解成,这个掩膜跟检测框一样大,你通过检测框的坐标,就知道掩膜的坐标了
        /// </summary>
        public Mat 掩膜数据 { get; set; }
        /// <summary>
        /// 使用pose模型的关键点信息
        /// </summary>
        public Pose[] 关键点 { get; set; }
    }
    class Pose
    {
        /// <summary>
        /// pose点的x坐标
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// post点的y坐标
        /// </summary>
        public float Y { get; set; }
        /// <summary>
        /// pose点的可信度,当该值较低时,实际大概率是在框外面,一般以0.5为阈值
        /// </summary>
        public float V { get; set; }
    }
    /// <summary>
    /// 用于二维平面物体运动的坐标预测,实现目标追踪的重要算法
    /// </summary>
    class 卡尔曼滤波
    {
        KalmanFilter kalman = new KalmanFilter(4, 2, 0);
        卡尔曼滤波()
        {
            kalman.MeasurementMatrix = new Mat(2, 4, MatType.CV_32F, new float[]
                      {
            1, 0, 0, 0,
            0, 1, 0, 0
                      });
            //(状态转移矩阵）：该属性表示系统模型中状态变量的转移关系。状态转移矩阵描述了当前时刻状态向量与下一时刻状态向量之间的线性关系。它是一个矩阵，其维度为状态向量维度 x 状态向量维度。
            kalman.TransitionMatrix = new Mat(4, 4, MatType.CV_32F, new float[]
            {
            1, 0, 1, 0,
            0, 1, 0, 1,
            0, 0, 1, 0,
            0, 0, 0, 1
            });
            //（控制矩阵）：该属性表示外部控制输入对状态变量的影响关系。控制矩阵用于将外部控制输入与状态变量之间的关系建模。它是一个矩阵，其行数等于状态向量的维度，列数等于控制输入向量的维度。
            kalman.ControlMatrix = new Mat(4, 2, MatType.CV_32F, new float[]
            {
            0, 0,
            0, 0,
            1, 0,
            0, 1
            });
            // 过程噪声协方差矩阵）：该属性表示系统模型中过程噪声的协方差矩阵。它用于描述状态转移过程中的不确定性或噪声水平。过程噪声协方差矩阵通常也是一个对角矩阵，对角线上的元素表示各个状态变量的方差。
            kalman.ProcessNoiseCov = new Mat(4, 4, MatType.CV_32F, new float[]
            {
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
            });
            //(测量噪声协方差矩阵）：该属性表示观测噪声的协方差矩阵。它用于描述观测值的不确定性或噪声水平。测量噪声协方差矩阵通常是一个对角矩阵，对角线上的元素表示各个观测维度的方差。
            kalman.MeasurementNoiseCov = new Mat(2, 2, MatType.CV_32F, new float[]
            {
            1, 0,
            0, 1
            });
        }
        /// <summary>
        /// 二维平面预测下一个位置,通过几次预测和更新正确坐标,会得到较为准确的预测
        /// </summary>
        /// <returns>返回预测的坐标点</returns>
        public PointF 预测下一个位置()
        {
            Mat 预测 = kalman.Predict();
            PointF 返回 = new PointF(预测.At<float>(0), 预测.At<float>(1));
            return 返回;
        }
        /// <summary>
        /// 预测后通过更新正确的坐标,来逐步提高预测的准确性
        /// </summary>
        /// <param name="修正坐标"></param>
        public void 更新正确坐标(PointF 修正坐标)
        {
            Mat 更新 = new Mat(2, 1, MatType.CV_32F, new float[] { 修正坐标.X, 修正坐标.Y });
            kalman.Correct(更新);
        }
    }
    public struct OBB矩形结构
    {
        public PointF pt1;
        public PointF pt2;
        public PointF pt3;
        public PointF pt4;
    }
}