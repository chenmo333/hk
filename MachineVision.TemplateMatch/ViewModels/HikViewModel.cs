using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using HalconDotNet;
using MachineVision.Core;
using MachineVision.Core.TemplateMatch;
using MachineVision.Shared.Controls;
using MvCameraControl;
using Prism.Commands;
using Prism.Ioc;
using System.IO;
using MachineVision.Device.Services;
using MachineVision.Device;
using MachineVision.TemplateMatch.Models;

namespace MachineVision.TemplateMatch.ViewModels;

public class HikViewModel : NavigationViewModel
{
    public ITemplateMatchService MatchService  { get; set; }
    public ICameraService        CameraService { get; set; }


    //相机曝光
    private double exposureTime;

    //相机增益
    private double gain;

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
        CameraService = ContainerLocator.Current.Resolve<ICameraService>(nameof(CameraType.HK));


        DeviceInfo              = new ObservableCollection<DeviceInfoItem>();                // 初始化集合
        GetParametersCommand    = new DelegateCommand(GetParameters);                        // 修改参数
        ModifyParametersCommand = new DelegateCommand(ModifyParameters);                     // 获取参数
        ScanCameraCommand       = new DelegateCommand(ScanCamera);                           // 扫描相机
        StartCameraCommand      = new DelegateCommand(async () => await StartCameraAsync()); // 启动相机
        CaptureImageCommand     = new DelegateCommand(CaptureImage);                         // 捕获图像
        StopCameraCommand       = new DelegateCommand(StopCamera);                           // 停止相机
        SaveImageCommand        = new DelegateCommand(SaveImage);                            // 保存图像

        SavePath = "C:/Users/Public/Pictures/_image.png"; // 默认图像保存路径

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
    private void GetParameters()
    {
        ExposureTime = CameraService.GetExposureTime(); // 曝光
        Gain         = CameraService.GetGain();         // 增益
    }

    /// <summary>
    /// 修改相机参数
    /// </summary>
    private void ModifyParameters()
    {
        CameraService.SetExposureTime((float)ExposureTime);
        CameraService.SetGain((float)Gain);

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
        var deviceInfoList = CameraService.ScanCamera();

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

    private async Task StartCameraAsync()
    {
        var deviceInfo = SelectedCamera.DeviceInfo;

        // 启动相机的后台任务
        await Task.Run(() => { CameraService.StartCamera(deviceInfo); })
            // 任务完成后继续执行
            .ContinueWith(t =>
            {
                // 在任务完成后检查相机是否成功打开
                if (!CameraService.IsCameraNull())
                    // 相机成功打开，更新界面状态
                    AddDeviceInfo("相机已打开");
                else
                    // 相机打开失败，更新界面状态
                    AddDeviceInfo("相机打开失败");
            }, TaskScheduler.FromCurrentSynchronizationContext()); // 确保在 UI 线程中执行
    }


    /// <summary>
    /// 捕获图像
    /// </summary>
    private void CaptureImage()
    {
        IFrameOut frameOut;
        var       nRet = CameraService.CaptureImage(out frameOut);
        if (!nRet)
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
    }

    /// <summary>
    /// 停止相机
    /// </summary>
    private void StopCamera()
    {
        CameraService.StopCamera();

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