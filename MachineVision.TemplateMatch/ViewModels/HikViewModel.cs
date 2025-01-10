﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
using HalconDotNet;
using MachineVision.Core;
using MachineVision.Core.TemplateMatch;
using MachineVision.Shared.Controls;
using MachineVision.TemplateMatch.Models;
using MvCameraControl;
using Prism.Commands;
using Prism.Ioc;
using Microsoft.Win32;
using ImTools;
using static MachineVision.Core.TemplateMatch.MatchResult;


namespace MachineVision.TemplateMatch.ViewModels
{
    
   public class HikViewModel : NavigationViewModel
    {
    
        public ITemplateMatchService MatchService { get; set; }

        #region 相机标志位
        readonly DeviceTLayerType enumTLayerType = DeviceTLayerType.MvGigEDevice | DeviceTLayerType.MvUsbDevice
    | DeviceTLayerType.MvGenTLGigEDevice | DeviceTLayerType.MvGenTLCXPDevice | DeviceTLayerType.MvGenTLCameraLinkDevice | DeviceTLayerType.MvGenTLXoFDevice;

        List<IDeviceInfo> deviceInfoList = new List<IDeviceInfo>();
        IDevice device = null;

        bool isGrabbing = false;        // ch:是否正在取图 | en: Grabbing flag

        //相机曝光
        double exposureTime;

        //相机增益
        double gain;



        #endregion

        private HObject maskObject;
        private HObject image;
        private ObservableCollection<DrawingObjectInfo> drawObjectList;
        public HikViewModel()
        {

       
            MatchService = ContainerLocator.Current.Resolve<ITemplateMatchService>(nameof(TempalteMatchType.ShapeModel));
            CreateTemplateCommand = new DelegateCommand(CreateTemplate);
            DrawObjectList = new ObservableCollection<DrawingObjectInfo>();


            ScanCameraCommand = new DelegateCommand(ScanCamera);
            StartCameraCommand = new DelegateCommand(StartCamera);
            CaptureImageCommand = new DelegateCommand(CaptureImage);
            StopCameraCommand = new DelegateCommand(StopCamera);
            SaveImageCommand = new DelegateCommand(SaveImage);
            // Other command initializations...

            // Set default save path
            SavePath = "C:\\Users\\Public\\Pictures"; // Default path, can be adjusted as needed
            #region 默认文本
            // Set default values

            IPAddress = "192.168.8.23"; // Default IP
            SubnetMask = "255.255.255.0"; // 子网掩码
            Gateway = "192.168.8.254"; // 网关
            SerialNumber = "L34412692"; // 序列号
            ExposureTime = 5000; // 曝光
            Gain = 1.0; // 增益
            #endregion
        }
        #region 文本

        private string _ipAddress;
        public string IPAddress
        {
            get { return _ipAddress; }
            set
            {
                if (_ipAddress != value)
                {
                    _ipAddress = value;
                    RaisePropertyChanged(nameof(IPAddress));
                }
            }
        }

        private string _subnetMask;
        public string SubnetMask
        {
            get { return _subnetMask; }
            set
            {
                if (_subnetMask != value)
                {
                    _subnetMask = value;
                    RaisePropertyChanged(nameof(SubnetMask));
                }
            }
        }

        private string _gateway;
        public string Gateway
        {
            get { return _gateway; }
            set
            {
                if (_gateway != value)
                {
                    _gateway = value;
                    RaisePropertyChanged(nameof(Gateway));
                }
            }
        }

        private string _serialNumber;
        public string SerialNumber
        {
            get { return _serialNumber; }
            set
            {
                if (_serialNumber != value)
                {
                    _serialNumber = value;
                    RaisePropertyChanged(nameof(SerialNumber));
                }
            }
        }
        // Save Path
        private string _savePath;
        public string SavePath
        {
            get { return _savePath; }
            set
            {
                if (_savePath != value)
                {
                    _savePath = value;
                    RaisePropertyChanged(nameof(SavePath));
                }
            }
        }

        private double _gain;
        public double Gain
        {
            get { return _gain; }
            set
            {
                if (_gain != value)
                {
                    _gain = value;
                    RaisePropertyChanged(nameof(Gain));
                }
            }
        }
        // Method for saving the image

        private double _exposureTime;
        public double ExposureTime
        {
            get { return _exposureTime; }
            set
            {
                if (_exposureTime != value)
                {
                    _exposureTime = value;
                    RaisePropertyChanged(nameof(ExposureTime));
                }
            }
        }



        #endregion
        #region 按钮
        public DelegateCommand CreateTemplateCommand { get; private set; }
        public DelegateCommand ScanCameraCommand { get; private set; }
        public DelegateCommand StartCameraCommand { get; private set; }
        public DelegateCommand CaptureImageCommand { get; private set; }
        public DelegateCommand StopCameraCommand { get; private set; }

        public DelegateCommand SaveImageCommand { get; private set; }

        public ObservableCollection<CameraDevice> CameraList { get; set; } = new ObservableCollection<CameraDevice>();
        public CameraDevice SelectedCamera { get; set; }  // Used to store the selected camera


        public HObject MaskObject
        {
            get { return maskObject; }
            set { maskObject = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 绘制形状集合
        /// </summary>
        public ObservableCollection<DrawingObjectInfo> DrawObjectList
        {
            get { return drawObjectList; }
            set { drawObjectList = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 创建匹配模板
        /// </summary>
        private void CreateTemplate()
        {
            var hobject = DrawObjectList.FirstOrDefault();
            if (hobject != null)
            {
                if (MaskObject != null)
                {
                    HOperatorSet.Difference(hobject.Hobject, MaskObject, out HObject difference);
                    MatchService.CreateTemplate(Image, difference);
                }
                else
                {
                    MatchService.CreateTemplate(Image, hobject.Hobject);
                }
            }
        }


        private void ScanCamera()
        {
       
            SDKSystem.Initialize(); // Initialize SDK resources

            // Ensure CameraList is initialized
            if (CameraList == null)
            {
                CameraList = new ObservableCollection<CameraDevice>(); // Initialize if not already
            }

            // Clear existing cameras
            CameraList.Clear();

            // Enumerate devices
            int nRet = DeviceEnumerator.EnumDevices(enumTLayerType, out deviceInfoList);

            if (nRet != MvError.MV_OK)
            {
                MessageBox.Show("设备枚举失败！");
                return;
            }

            // Add devices to CameraList
            foreach (var deviceInfo in deviceInfoList)
            {
                string cameraName = !string.IsNullOrEmpty(deviceInfo.UserDefinedName) ?
                    $"{deviceInfo.TLayerType}: {deviceInfo.UserDefinedName} ({deviceInfo.SerialNumber})" :
                    $"{deviceInfo.TLayerType}: {deviceInfo.ManufacturerName} {deviceInfo.ModelName} ({deviceInfo.SerialNumber})";

                CameraList.Add(new CameraDevice
                {
                    CameraName = cameraName,
                    DeviceInfo = deviceInfo
                });
            }
        }



        private void StartCamera()
        {
            Template1();
         
        }
        public async Task Template1()
        {
            await Task.Run(() =>
            {
                // 获取选择的设备信息 | Get selected device information
                IDeviceInfo deviceInfo = SelectedCamera?.DeviceInfo;

                try
                {
                    // 创建设备
                    device = DeviceFactory.CreateDevice(deviceInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("创建设备失败！" + ex.Message);
                    return;
                }

                int result = device.Open();


                // 判断是否为GigE设备并设置包大小
                if (device is IGigEDevice gigEDevice)
                {
                    int optionPacketSize;
                    result = gigEDevice.GetOptimalPacketSize(out optionPacketSize);
                    if (result == MvError.MV_OK)
                    {
                        result = device.Parameters.SetIntValue("GevSCPSPacketSize", (long)optionPacketSize);

                    }

                }

                // 设置心跳超时
                device.Parameters.SetIntValue("GevHeartbeatTimeout", 3000);

                // **设置触发源为软件**
                result = device.Parameters.SetEnumValueByString("TriggerSource", "Software");


                // **启用触发模式**
                result = device.Parameters.SetEnumValueByString("TriggerMode", "On");


                // 设置采集模式为连续
                result = device.Parameters.SetEnumValueByString("AcquisitionMode", "Continuous");


                StartGrab();
            });
        }

        private void StartGrab()
        {
            Template2();
        }
        public async Task Template2()
        {
            await Task.Run(() =>
            {

                isGrabbing = true;

                // ch:开始采集 | en:Start Grabbing
                int result = device.StreamGrabber.StartGrabbing();
                if (result != MvError.MV_OK)
                {
                    isGrabbing = false;

                    return;
                }
            });
        }

        private void CaptureImage()
        {
            Template3();
            int nRet;
            IFrameOut frameOut;
            HImage hImage = new HImage();//读取主图片
            nRet = device.StreamGrabber.GetImageBuffer(500, out frameOut);
            if (nRet != MvError.MV_OK)
            {
                return;
            }
            else
            {
                var img = new HImage();
                Image mimage = frameOut.Image.ToBitmap();

                // 指定保存路径
                string filePath = @"C:\image.jpg"; // 可以修改文件名和格式

                // 将图像保存到指定路径
                mimage.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg); // 可以根据需要修改格式，如 PNG、BMP 等

                hImage.ReadImage("C:/image.jpg");
                Image = hImage;

                device.StreamGrabber.FreeImageBuffer(frameOut);
            }

        }
        public async Task Template3()
        {
            await Task.Run(() =>
            {
                try
                {
                    // 发送软件触发命令
                    int result = device.Parameters.SetCommandValue("TriggerSoftware");
                    if (result != MvError.MV_OK)
                    {

                        return;
                    }


                   

                }
                catch (Exception ex)
                {
                    MessageBox.Show("发送软件触发时出错：" + ex.Message, "错误");
                }
            });
        }

        private void StopCamera()
        {
            
        }
        private void SaveImage()
        {
            // Implement the logic for saving the image here
            // Example: Save the image to the specified SavePath
            Console.WriteLine($"Image saved to {SavePath}");
        }
        public HObject Image
        {
            get { return image; }
            set { image = value; RaisePropertyChanged(); }
        }
        #endregion
    }
}
