using System.Drawing;
using System.Threading.Tasks;

namespace CameraControl
{
    /// <summary>
    /// 定义相机的基本操作接口
    /// </summary>
    public interface ICamera
    {
        /// <summary>
        /// 启动相机并开始连续捕捉
        /// </summary>
        void Start();

        /// <summary>
        /// 停止相机的捕捉功能
        /// </summary>
        void Stop();

        /// <summary>
        /// 触发相机拍照并获取单张图像
        /// </summary>
        /// <returns>捕获的图像</returns>
        Task<Image> CaptureAsync();

        /// <summary>
        /// 保存当前捕获的图像到指定路径
        /// </summary>
        /// <param name="image">要保存的图像</param>
        /// <param name="filePath">保存的文件路径</param>
        void Save(Image image, string filePath);

        /// <summary>
        /// 获取或设置相机的曝光时间（微秒）
        /// </summary>
        double ExposureTime { get; set; }

        /// <summary>
        /// 获取或设置相机的增益
        /// </summary>
        double Gain { get; set; }

        /// <summary>
        /// 图像捕获完成时触发的事件
        /// </summary>
        event EventHandler<ImageCapturedEventArgs> ImageCaptured;
    }

    /// <summary>
    /// 图像捕获事件参数
    /// </summary>
    public class ImageCapturedEventArgs : EventArgs
    {
        /// <summary>
        /// 捕获到的图像
        /// </summary>
        public Image CapturedImage { get; private set; }

        public ImageCapturedEventArgs(Image image)
        {
            CapturedImage = image;
        }
    }
}
