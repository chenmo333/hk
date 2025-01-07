using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Threading.Tasks;
using DryIoc;
using MvCameraControl;

namespace CameraControl
{
    /// <summary>
    /// 海康威视相机的封装类，实现ICamera接口
    /// </summary>
    public class HKCamera : ICamera, IDisposable
    {
        /// <summary>
        /// 定义相机支持的设备类型
        /// </summary>
        private readonly DeviceTLayerType enumTLayerType = DeviceTLayerType.MvGigEDevice | DeviceTLayerType.MvUsbDevice
            | DeviceTLayerType.MvGenTLGigEDevice | DeviceTLayerType.MvGenTLCXPDevice
            | DeviceTLayerType.MvGenTLCameraLinkDevice | DeviceTLayerType.MvGenTLXoFDevice;

        /// <summary>
        /// 相机设备信息列表
        /// </summary>
        private List<IDeviceInfo> deviceInfoList = new List<IDeviceInfo>();

        /// <summary>
        /// 当前使用的相机设备
        /// </summary>
        private IDevice device = null;

        /// <summary>
        /// 当前选中的设备信息
        /// </summary>
        private IDeviceInfo currentDeviceInfo = null;

        /// <summary>
        /// 相机是否已打开
        /// </summary>
        private bool isDeviceOpen = false;

        /// <summary>
        /// 相机是否正在抓取
        /// </summary>
        private bool isGrabbing = false;

        /// <summary>
        /// 相机曝光时间（微秒）
        /// </summary>
        private double exposureTime;

        /// <summary>
        /// 相机增益
        /// </summary>
        private double gain;

        /// <summary>
        /// 最近捕获的图像
        /// </summary>
        private Image lastCapturedImage = null;

        /// <summary>
        /// 用于锁定共享资源的对象
        /// </summary>
        private readonly object locker = new object();

        /// <summary>
        /// 图像捕获完成时触发的事件
        /// </summary>
        public event EventHandler<ImageCapturedEventArgs> ImageCaptured;

        /// <summary>
        /// 初始化相机资源
        /// </summary>
        public HKCamera()
        {
            SDKSystem.Initialize(); // 初始化SDK资源
        }

        /// <summary>
        /// 启动相机并开始连续捕捉
        /// </summary>
        public void Start()
        {
            Task.Run(() =>
            {
                try
                {
                    if (!isDeviceOpen)
                    {
                        // 枚举设备
                        int ret = DeviceEnumerator.EnumDevices(enumTLayerType, out deviceInfoList);
                        if (ret != MvError.MV_OK)
                        {
                            throw new Exception($"枚举设备失败，错误码: {ret:X}");
                        }

                        if (deviceInfoList.Count == 0)
                        {
                            throw new Exception("未找到任何相机设备。");
                        }

                        // 选择第一个设备进行打开
                        currentDeviceInfo = deviceInfoList[0];
                        device = DeviceFactory.CreateDevice(currentDeviceInfo);
                        ret = device.Open();
                        if (ret != MvError.MV_OK)
                        {
                            throw new Exception($"打开设备失败，错误码: {ret:X}");
                        }

                        // 判断是否为GigE设备并设置包大小
                        if (device is IGigEDevice gigEDevice)
                        {
                            int optimalPacketSize;
                            ret = gigEDevice.GetOptimalPacketSize(out optimalPacketSize);
                            if (ret == MvError.MV_OK)
                            {
                                ret = device.Parameters.SetIntValue("GevSCPSPacketSize", (long)optimalPacketSize);
                                if (ret != MvError.MV_OK)
                                {
                                    throw new Exception($"设置包大小失败，错误码: {ret:X}");
                                }
                            }
                            else
                            {
                                throw new Exception($"获取包大小失败，错误码: {ret:X}");
                            }
                        }

                        // 设置心跳超时
                        device.Parameters.SetIntValue("GevHeartbeatTimeout", 3000);

                        // 设置触发源为软件
                        ret = device.Parameters.SetEnumValueByString("TriggerSource", "Software");
                        if (ret != MvError.MV_OK)
                        {
                            throw new Exception($"设置触发源为软件失败，错误码: {ret:X}");
                        }

                        // 启用触发模式
                        ret = device.Parameters.SetEnumValueByString("TriggerMode", "On");
                        if (ret != MvError.MV_OK)
                        {
                            throw new Exception($"启用触发模式失败，错误码: {ret:X}");
                        }

                        // 设置采集模式为连续
                        ret = device.Parameters.SetEnumValueByString("AcquisitionMode", "Continuous");
                        if (ret != MvError.MV_OK)
                        {
                            throw new Exception($"设置采集模式为连续失败，错误码: {ret:X}");
                        }

                        // 获取并设置曝光时间和增益
                        GetExposureGain();

                        // 开始抓取图像
                        ret = device.StreamGrabber.StartGrabbing();
                        if (ret != MvError.MV_OK)
                        {
                            throw new Exception($"开始抓取图像失败，错误码: {ret:X}");
                        }

                        isDeviceOpen = true;
                        isGrabbing = true;
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常，释放资源
                    Stop();
                    // 可以通过日志记录错误
                    Console.WriteLine($"启动相机失败: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// 停止相机的捕捉功能并释放资源
        /// </summary>
        public void Stop()
        {
            Task.Run(() =>
            {
                try
                {
                    if (isGrabbing && device != null)
                    {
                        // 停止抓取
                        int ret = device.StreamGrabber.StopGrabbing();
                        if (ret != MvError.MV_OK)
                        {
                            throw new Exception($"停止抓取图像失败，错误码: {ret:X}");
                        }
                        isGrabbing = false;
                    }

                    if (isDeviceOpen && device != null)
                    {
                        // 关闭设备
                        device.Close();
                        device.Dispose();
                        device = null;
                        isDeviceOpen = false;
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    Console.WriteLine($"停止相机失败: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// 触发相机拍照并获取单张图像
        /// </summary>
        /// <returns>捕获的图像</returns>
        public async Task<Image> CaptureAsync()
        {
            return await Task.Run(() =>
            {
                lock (locker)
                {
                    try
                    {
                        if (!isDeviceOpen)
                        {
                            throw new Exception("相机未打开。");
                        }

                        // 发送软件触发命令
                        int ret = device.Parameters.SetCommandValue("TriggerSoftware");
                        if (ret != MvError.MV_OK)
                        {
                            throw new Exception($"发送软件触发失败，错误码: {ret:X}");
                        }

                        // 等待图像到达
                        IFrameOut frameOut;
                        ret = device.StreamGrabber.GetImageBuffer(500, out frameOut);
                        if (ret != MvError.MV_OK)
                        {
                            throw new Exception($"获取图像缓冲区失败，错误码: {ret:X}");
                        }

                        // 将图像转换为Bitmap
                        Image image = frameOut.Image.ToBitmap();
                        lastCapturedImage = image;

                        // 释放图像缓冲区
                        device.StreamGrabber.FreeImageBuffer(frameOut);

                        // 触发图像捕获完成事件
                        ImageCaptured?.Invoke(this, new ImageCapturedEventArgs(image));

                        return image;
                    }
                    catch (Exception ex)
                    {
                        // 处理异常
                        Console.WriteLine($"捕获图像失败: {ex.Message}");
                        return null;
                    }
                }
            });
        }

        /// <summary>
        /// 保存当前捕获的图像到指定路径
        /// </summary>
        /// <param name="image">要保存的图像</param>
        /// <param name="filePath">保存的文件路径</param>
        public void Save(Image image, string filePath)
        {
            try
            {
                if (image == null)
                {
                    throw new Exception("没有要保存的图像。");
                }

                // 保存图像到文件
                image.Save(filePath);
            }
            catch (Exception ex)
            {
                // 处理异常
                Console.WriteLine($"保存图像失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取并设置相机的曝光时间和增益
        /// </summary>
        private void GetExposureGain()
        {
            try
            {
                // 获取曝光时间
                int ret = device.Parameters.GetFloatValue("ExposureTime", out IFloatValue exposureTimeIF);
                if (ret != MvError.MV_OK)
                {
                    throw new Exception($"获取曝光时间失败，错误码: {ret:X}");
                }
                exposureTime = exposureTimeIF.CurValue;

                // 获取增益
                ret = device.Parameters.GetFloatValue("Gain", out IFloatValue gainIF);
                if (ret != MvError.MV_OK)
                {
                    throw new Exception($"获取增益失败，错误码: {ret:X}");
                }
                gain = gainIF.CurValue;
            }
            catch (Exception ex)
            {
                // 处理异常
                Console.WriteLine($"获取曝光和增益失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取或设置相机的曝光时间（微秒）
        /// </summary>
        public double ExposureTime
        {
            get
            {
                lock (locker)
                {
                    return exposureTime;
                }
            }
            set
            {
                lock (locker)
                {
                    try
                    {
                        if (!isDeviceOpen)
                        {
                            throw new Exception("相机未打开。");
                        }

                        // 设置曝光时间
                        int ret = device.Parameters.SetFloatValue("ExposureTime", (float)value);
                        if (ret != MvError.MV_OK)
                        {
                            throw new Exception($"设置曝光时间失败，错误码: {ret:X}");
                        }

                        exposureTime = value;
                    }
                    catch (Exception ex)
                    {
                        // 处理异常
                        Console.WriteLine($"设置曝光时间失败: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 获取或设置相机的增益
        /// </summary>
        public double Gain
        {
            get
            {
                lock (locker)
                {
                    return gain;
                }
            }
            set
            {
                lock (locker)
                {
                    try
                    {
                        if (!isDeviceOpen)
                        {
                            throw new Exception("相机未打开。");
                        }

                        // 设置增益
                        int ret = device.Parameters.SetFloatValue("Gain", (float)value);
                        if (ret != MvError.MV_OK)
                        {
                            throw new Exception($"设置增益失败，错误码: {ret:X}");
                        }

                        gain = value;
                    }
                    catch (Exception ex)
                    {
                        // 处理异常
                        Console.WriteLine($"设置增益失败: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 释放相机资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放相机资源的具体实现
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 释放托管资源
                Stop();
                SDKSystem.Finalize(); // 释放SDK资源
            }
            // 释放非托管资源（如果有）
        }

        /// <summary>
        /// 析构函数，确保资源被释放
        /// </summary>
        ~HKCamera()
        {
            Dispose(false);
        }
    }
}
