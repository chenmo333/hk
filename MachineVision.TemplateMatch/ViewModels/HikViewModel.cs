using System;
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
using System.Security.Cryptography;
using System.Collections.ObjectModel;
using System.IO;

namespace MachineVision.TemplateMatch.ViewModels;

public class HikViewModel : NavigationViewModel
{
    public ITemplateMatchService MatchService { get; set; }

    #region 相机标志位

    private readonly DeviceTLayerType enumTLayerType = DeviceTLayerType.MvGigEDevice | DeviceTLayerType.MvUsbDevice
        | DeviceTLayerType.MvGenTLGigEDevice | DeviceTLayerType.MvGenTLCXPDevice |
        DeviceTLayerType.MvGenTLCameraLinkDevice | DeviceTLayerType.MvGenTLXoFDevice;

    private List<IDeviceInfo> deviceInfoList = new();
    private IDevice           device         = null;

    private bool isGrabbing = false; // ch:是否正在取图 | en: Grabbing flag

    //相机曝光
    private double exposureTime;

    //相机增益
    private double gain;

    #endregion

    private HObject                                 maskObject;
    private HObject                                 _image;
    private ObservableCollection<DrawingObjectInfo> drawObjectList;


    /// <summary>
    /// Halcon 图像
    /// </summary>
    public HObject Image
    {
        get => _image;
        set => SetProperty(ref _image, value);
    }


    public HikViewModel()
    {
        MatchService = ContainerLocator.Current.Resolve<ITemplateMatchService>(nameof(TempalteMatchType.ShapeModel));
        CreateTemplateCommand = new DelegateCommand(CreateTemplate);
        DrawObjectList = new ObservableCollection<DrawingObjectInfo>();


        DeviceInfo              = new ObservableCollection<DeviceInfoItem>(); // 初始化集合
        GetParametersCommand    = new DelegateCommand(Getparameters);         //修改参数
        ModifyParametersCommand = new DelegateCommand(ModifyParameters);      //获取参数
        ScanCameraCommand       = new DelegateCommand(ScanCamera);
        StartCameraCommand      = new DelegateCommand(async () => await OpenDeviceAndStartGrabAsync());
        CaptureImageCommand     = new DelegateCommand(CaptureImage);
        StopCameraCommand       = new DelegateCommand(StopCamera);
        SaveImageCommand        = new DelegateCommand(SaveImage); //保存图像
        // Other command initializations...

        // Set default save path
        SavePath = "C:/Users/Public/Pictures/_image.png"; // Default path, can be adjusted as needed

        #region 默认文本

        // Set default values

        IPAddress    = "192.168.8.23";  // Default IP
        SubnetMask   = "255.255.255.0"; // 子网掩码
        Gateway      = "192.168.8.254"; // 网关
        SerialNumber = "L34412692";     // 序列号
        ExposureTime = 5000;            // 曝光
        Gain         = 1.0;             // 增益

        #endregion
    }

    #region 文本

    private string _ipAddress;

    #region 用于绑定 DataGrid 的集合

    public ObservableCollection<DeviceInfoItem> DeviceInfo { get; set; }

    // 数据类
    public class DeviceInfoItem
    {
        public string Time { get; set; }
        public string Info { get; set; }
    }

    /// <summary>
    /// 添加设备信息
    /// </summary>
    /// <param name="info"></param>
    public void AddDeviceInfo(string info)
    {
        DeviceInfo.Insert(0, new DeviceInfoItem
        {
            Time = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            Info = info
        });
    }

    #endregion

    public string IPAddress
    {
        get => _ipAddress;
        set => SetProperty(ref _ipAddress, value);
    }

    private string _subnetMask;

    public string SubnetMask
    {
        get => _subnetMask;
        set => SetProperty(ref _subnetMask, value);
    }

    private string _gateway;

    public string Gateway
    {
        get => _gateway;
        set => SetProperty(ref _gateway, value);
    }

    private string _serialNumber;

    public string SerialNumber
    {
        get => _serialNumber;
        set => SetProperty(ref _serialNumber, value);
    }

    // Save Path
    private string _savePath;

    public string SavePath
    {
        get => _savePath;
        set => SetProperty(ref _savePath, value);
    }

    private double _gain;

    public double Gain
    {
        get => _gain;
        set => SetProperty(ref _gain, value);
    }
    // Method for saving the _image

    private double _exposureTime;

    public double ExposureTime
    {
        get => _exposureTime;
        set => SetProperty(ref _exposureTime, value);
    }

    #endregion

    #region 按钮

    public DelegateCommand GetParametersCommand    { get; private set; }
    public DelegateCommand ModifyParametersCommand { get; private set; }
    public DelegateCommand CreateTemplateCommand   { get; private set; }
    public DelegateCommand ScanCameraCommand       { get; private set; }
    public DelegateCommand StartCameraCommand      { get; private set; }
    public DelegateCommand CaptureImageCommand     { get; private set; }
    public DelegateCommand StopCameraCommand       { get; private set; }

    public DelegateCommand SaveImageCommand { get; private set; }

    public ObservableCollection<CameraDevice> CameraList     { get; set; } = new();
    public CameraDevice                       SelectedCamera { get; set; } // Used to store the selected camera


    public HObject MaskObject
    {
        get => maskObject;
        set
        {
            maskObject = value;
            RaisePropertyChanged();
        }
    }


    /// <summary>
    /// 绘制形状集合
    /// </summary>
    public ObservableCollection<DrawingObjectInfo> DrawObjectList
    {
        get => drawObjectList;
        set
        {
            drawObjectList = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// 获取相机曝光时间及增益
    /// </summary>
    private void Getparameters()
    {
        var nRet = device.Parameters.GetFloatValue("ExposureTime", out var exposureTime_IF);
        nRet         = device.Parameters.GetFloatValue("Gain", out var gain_IF);
        exposureTime = exposureTime_IF.CurValue;
        gain         = gain_IF.CurValue;
        ExposureTime = exposureTime; // 曝光
        Gain         = gain;         // 增益
    }

    /// <summary>
    /// 修改相机参数
    /// </summary>
    private void ModifyParameters()
    {
        var nRet = device.Parameters.SetFloatValue("ExposureTime", (float)ExposureTime);
        nRet = device.Parameters.SetFloatValue("Gain", (float)Gain);

        AddDeviceInfo($"相机增益已修改为: {ExposureTime}, 曝光时间修改为: {Gain}");
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
                HOperatorSet.Difference(hobject.Hobject, MaskObject, out var difference);
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
        if (CameraList == null) CameraList = new ObservableCollection<CameraDevice>(); // Initialize if not already

        // Clear existing cameras
        CameraList.Clear();

        // Enumerate devices
        var nRet = DeviceEnumerator.EnumDevices(enumTLayerType, out deviceInfoList);

        if (nRet != MvError.MV_OK)
        {
            MessageBox.Show("设备枚举失败！");
            return;
        }

        // Add devices to CameraList
        foreach (var deviceInfo in deviceInfoList)
        {
            var cameraName = !string.IsNullOrEmpty(deviceInfo.UserDefinedName)
                ? $"{deviceInfo.TLayerType}: {deviceInfo.UserDefinedName} ({deviceInfo.SerialNumber})"
                : $"{deviceInfo.TLayerType}: {deviceInfo.ManufacturerName} {deviceInfo.ModelName} ({deviceInfo.SerialNumber})";

            CameraList.Add(new CameraDevice
            {
                CameraName = cameraName,
                DeviceInfo = deviceInfo
            });

            AddDeviceInfo($"成功寻找到相机: {cameraName}");
        }
    }

    public async Task OpenDeviceAndStartGrabAsync()
    {
        // 打开设备
        await StartCameraAsync();


        // 在设备成功打开后自动调用开始抓取
        if (device != null)
        {
            AddDeviceInfo("相机已打开");
            await StartGrabAsync();
        }
        else
        {
            AddDeviceInfo("相机打开失败");
        }
    }

    private async Task StartCameraAsync()
    {
        await Task.Run(() =>
        {
            // 获取选择的设备信息 | Get selected device information
            var deviceInfo = SelectedCamera?.DeviceInfo;

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

            var result = device.Open();


            // 判断是否为GigE设备并设置包大小
            if (device is IGigEDevice gigEDevice)
            {
                int optionPacketSize;
                result = gigEDevice.GetOptimalPacketSize(out optionPacketSize);
                if (result == MvError.MV_OK)
                    result = device.Parameters.SetIntValue("GevSCPSPacketSize", (long)optionPacketSize);
            }

            // 设置心跳超时
            device.Parameters.SetIntValue("GevHeartbeatTimeout", 3000);

            // **设置触发源为软件**
            result = device.Parameters.SetEnumValueByString("TriggerSource", "Software");


            // **启用触发模式**
            result = device.Parameters.SetEnumValueByString("TriggerMode", "On");


            // 设置采集模式为连续
            result = device.Parameters.SetEnumValueByString("AcquisitionMode", "Continuous");
        });
    }

    private async Task StartGrabAsync()
    {
        await Task.Run(() =>
        {
            isGrabbing = true;

            // ch:开始采集 | en:Start Grabbing
            var result = device.StreamGrabber.StartGrabbing();
            if (result != MvError.MV_OK)
            {
                isGrabbing = false;

                return;
            }
        });
    }

    /// <summary>
    /// 捕获图像
    /// </summary>
    private void CaptureImage()
    {
        try
        {
            // 发送软件触发命令
            var result = device.Parameters.SetCommandValue("TriggerSoftware");
            if (result != MvError.MV_OK)
            {
                AddDeviceInfo("软件触发命令发送失败");
                return;
            }
        }
        catch (Exception ex)
        {
            AddDeviceInfo($"发送软件触发时出错: {ex.Message}");
        }

        IFrameOut frameOut;
        var       nRet = device.StreamGrabber.GetImageBuffer(500, out frameOut);
        if (nRet != MvError.MV_OK)
        {
            AddDeviceInfo("获取图像失败");
            return;
        }

        Image mimage = frameOut.Image.ToBitmap();

        // 指定保存路径
        var folderPath = @"./Images";
        var filePath   = Path.Combine(folderPath, "img.jpg");

        // 如果文件夹不存在，则创建文件夹
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        // 将图像保存到指定路径
        try
        {
            mimage.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
        }
        catch (Exception ex)
        {
            AddDeviceInfo($"保存图像失败: {ex.Message}");
            return;
        }

        try
        {
            var hImage = new HImage();
            hImage.ReadImage(filePath);
            Image = hImage;
            AddDeviceInfo("捕获图像");
        }
        catch (Exception ex)
        {
            AddDeviceInfo($"加载图像失败: {ex.Message}");
        }
        finally
        {
            // 释放图像缓冲区
            device.StreamGrabber.FreeImageBuffer(frameOut);
        }
    }

    /// <summary>
    /// 停止相机
    /// </summary>
    private void StopCamera()
    {
        // ch:停止采集 | en:Stop Grabbing
        var result = device.StreamGrabber.StopGrabbing();
        // ch:关闭设备 | en:Close Device
        device.Close();
        device.Dispose();

        AddDeviceInfo("相机已关闭");
    }

    /// <summary>
    /// 保存图像
    /// </summary>
    private void SaveImage()
    {
        var hImage = new HImage(); //读取主图片

        hImage.ReadImage("./Images/img.jpg");
        HOperatorSet.WriteImage(hImage, "png", 0, SavePath); //保存模板路径

        // Implement the logic for saving the _image here
        // Example: Save the _image to the specified SavePath

        AddDeviceInfo($"图像已保存到路径: {SavePath}");
    }

    #endregion
}