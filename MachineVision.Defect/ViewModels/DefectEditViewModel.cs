using DryIoc;
using HalconDotNet;
using MachineVision.Core;
using MachineVision.Core.Extensions;
using MachineVision.Core.TemplateMatch;
using MachineVision.Defect.Events;
using MachineVision.Defect.Extensions;
using MachineVision.Defect.Models;
using MachineVision.Defect.Models.UI;
using MachineVision.Defect.Services;
using MachineVision.Defect.ViewModels.Components;
using MachineVision.Defect.ViewModels.Components.Models;
using MachineVision.Shared.Controls;
using MachineVision.Shared.Services.Session;
using MachineVision.Shared.Services.Tables;
using MvCameraControl;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;
using System.IO;
using Camera = MachineVision.Defect.Models.UI.Camera;
using System.Timers;
using Timer = System.Timers.Timer;
using S7.Net;
using System.Windows;
using MachineVision.Device.Services;
using MachineVision.Shared.Services;
using MessageBox = System.Windows.MessageBox;
using MachineVision.Device;
using Prism.Ioc;

namespace MachineVision.Defect.ViewModels;

internal class DefectEditViewModel : NavigationViewModel
{
    private Timer _timer;

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

    public IPlcService PlcService { set; get; }

    public ICameraService CameraService { set; get; }

    private ObservableCollection<Camera> _cameraList;

    public ObservableCollection<Camera> CameraList
    {
        get => _cameraList;
        set
        {
            _cameraList = value;
            RaisePropertyChanged(nameof(CameraList));
        }
    }

    private Camera _selectedCamera;

    public Camera SelectedCamera
    {
        get => _selectedCamera;
        set
        {
            _selectedCamera = value;
            RaisePropertyChanged(nameof(SelectedCamera));
            // 你可以在这里添加逻辑，当选择的相机改变时执行相应操作
        }
    }

    private MatchResult matchResult;

    public MatchResult MatchResult
    {
        get => matchResult;
        set
        {
            matchResult = value;
            RaisePropertyChanged();
        }
    }

    private MatchResultSetting setting;

    /// <summary>
    /// 匹配结果显示设置
    /// </summary>
    public MatchResultSetting Setting
    {
        get => setting;
        set
        {
            setting = value;
            RaisePropertyChanged();
        }
    }

    private HWindow hWindow;

    public HWindow HWindow
    {
        get => hWindow;
        set
        {
            hWindow = value;
            RaisePropertyChanged();
        }
    }

    public DefectEditViewModel(TargetService targetService,
        ProjectService                       appService,
        InspectionService                    inspec,
        IHostDialogService                   dialog,
        IEventAggregator                     aggregator)
    {
        PlcService    = ContainerLocator.Current.Resolve<IPlcService>(nameof(PLCType.S7_1200));
        CameraService = ContainerLocator.Current.Resolve<ICameraService>(nameof(CameraType.HK));

        // 创建定时器，设置为 2000 毫秒（2 秒）触发一次
        _timer = new Timer(1000);

        // 绑定定时器的 Elapsed 事件
        _timer.Elapsed += OnTimedEvent;

        Files = new ObservableCollection<ImageFile>();

        DrawingObjectInfos = new ObservableCollection<HDrawingObjectInfo>();
        RegionList         = new ObservableCollection<InspecRegionModel>();

        this.targetService = targetService;
        this.appService    = appService;
        this.inspec        = inspec;
        this.dialog        = dialog;
        this.aggregator    = aggregator;

        Setting     = new MatchResultSetting();
        MatchResult = new MatchResult();

        InitBindingCommands();
    }

    #region 接口服务

    private readonly TargetService      targetService;
    private readonly ProjectService     appService;
    public           InspectionService  inspec { get; }
    private readonly IHostDialogService dialog;
    private readonly IEventAggregator   aggregator;

    #endregion

    #region 命令

    public DelegateCommand         ExecNextCommand { get; private set; }
    public DelegateCommand<string> ExecuteCommand  { get; private set; }

    public DelegateCommand ReturnCommand { get; private set; }

    public DelegateCommand TrainCommand { get; private set; }

    public DelegateCommand RunCommand { get; private set; }

    /// <summary>
    /// 加载图像列表
    /// </summary>
    public DelegateCommand LoadImageCommand { get; private set; }

    //相机
    public DelegateCommand xjCommand                { get; private set; }
    public DelegateCommand closeDeviceCommand       { get; private set; }//关闭相机
    public DelegateCommand TextrecognitionCommand   { get; private set; }
    public DelegateCommand RefreshDeviceListCommand { get; private set; } //寻找相机
    public DelegateCommand OpenDeviceCommand        { get; private set; } //打开相机
    public DelegateCommand OnceSoftTriggerCommand   { get; private set; } //相机拍照
    public DelegateCommand ConnectPlcCommand        { get; private set; } //连接plc

    /// <summary>
    /// 设置项目参数
    /// </summary>
    public DelegateCommand SetModelParamCommand { get; private set; }

    /// <summary>
    /// 更新项目参数
    /// </summary>
    public DelegateCommand UpdateModelParamCommand { get; private set; }

    /// <summary>
    /// 新建检测区域
    /// </summary>
    public DelegateCommand CreateRegionCommand { get; private set; }

    /// <summary>
    /// 更新区域参数
    /// </summary>
    public DelegateCommand UpdateRegionCommand { get; private set; }

    /// <summary>
    /// 删除检测区域
    /// </summary>
    public DelegateCommand<InspecRegionModel> DeleteInspecRegionCommand { get; private set; }

    /// <summary>
    /// 编辑检测区域
    /// </summary>
    public DelegateCommand<InspecRegionModel> EditInspecRegionCommand { get; private set; }

    /// <summary>
    /// 初始化绑定命令RefreshDeviceList
    /// </summary>
    private void InitBindingCommands()
    {
        LoadImageCommand         = new DelegateCommand(LoadImage);
        ConnectPlcCommand        = new DelegateCommand(ConnectPlc);
        RefreshDeviceListCommand = new DelegateCommand(RefreshDeviceList); //枚举相机

        OpenDeviceCommand      = new DelegateCommand(async () => await OpenDeviceAndStartGrabAsync()); //打开相机
        OnceSoftTriggerCommand = new DelegateCommand(async () => await CaptureImageAsync());           //拍照

        TextrecognitionCommand = new DelegateCommand(ShapeRecognition); //识别形状
        closeDeviceCommand     = new DelegateCommand(closeDevice);

        SetModelParamCommand    = new DelegateCommand(SetModelParam);
        UpdateModelParamCommand = new DelegateCommand(UpdateModelParam);
        CreateRegionCommand     = new DelegateCommand(CreateRegion);
        UpdateRegionCommand     = new DelegateCommand(UpdateRegion);

        EditInspecRegionCommand   = new DelegateCommand<InspecRegionModel>(EditInspecRegion);
        DeleteInspecRegionCommand = new DelegateCommand<InspecRegionModel>(DeleteInspecRegion);

        RunCommand = new DelegateCommand(Run);

        TrainCommand = new DelegateCommand(() =>
        {
            var param = new DialogParameters();
            param.Add("Value", RegionList);
            dialog.ShowDialog("TrainView", param, callback => { });
        });

        ReturnCommand = new DelegateCommand(() =>
        {
            if (Journal.CanGoBack)
                Journal.GoBack();
        });

        ExecNextCommand = new DelegateCommand(Execute);
        ExecuteCommand  = new DelegateCommand<string>(Execute);
    }

    #endregion

    #region 编辑器绑定

    private HObject                                  image;
    private bool                                     isModelEditMode;
    private ObservableCollection<HDrawingObjectInfo> drawingObjectInfos;

    public HObject Image
    {
        get => image;
        set
        {
            image = value;
            RaisePropertyChanged();
        }
    }

    public bool IsModelEditMode
    {
        get => isModelEditMode;
        set
        {
            isModelEditMode = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// 绘制形状集合
    /// </summary>
    public ObservableCollection<HDrawingObjectInfo> DrawingObjectInfos
    {
        get => drawingObjectInfos;
        set
        {
            drawingObjectInfos = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region 项目/检测区域/图像列表

    private int selectedImageIndex;

    public int SelectedImageIndex
    {
        get => selectedImageIndex;
        set
        {
            selectedImageIndex = value;
            RaisePropertyChanged();
        }
    }

    private ProjectModel                            model;
    private ObservableCollection<InspecRegionModel> regionList;
    private ObservableCollection<ImageFile>         files;
    private InspecRegionModel                       selectedRegion;

    /// <summary>
    /// 项目
    /// </summary>
    public ProjectModel Model
    {
        get => model;
        set
        {
            model = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// 检测区域列表
    /// </summary>
    public ObservableCollection<InspecRegionModel> RegionList
    {
        get => regionList;
        set
        {
            regionList = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// 选中检测区域
    /// </summary>
    public InspecRegionModel SelectedRegion
    {
        get => selectedRegion;
        set
        {
            selectedRegion = value;
            RestoreRegionParameter();
        }
    }

    public ObservableCollection<ImageFile> Files
    {
        get => files;
        set
        {
            files = value;
            RaisePropertyChanged();
        }
    }

    private string message;

    public string Message
    {
        get => message;
        set
        {
            message = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region 命令方法

    /// <summary>
    /// 处理下一张图像
    /// </summary>
    private void Execute()
    {
        SelectedImageIndex += 1;
        if (Image == null || !Image.IsInitialized()) return;
        Result = inspec.ExecuteAsync(Image, Model, RegionList);
    }

    /// <summary>
    /// 执行操作
    /// </summary>
    /// <param name="arg"></param>
    private void Execute(string arg)
    {
        switch (arg)
        {
            case "Left":
                break;
            case "Right":
                break;
            case "Top":
                break;
            case "Bottom":
                break;
            case "Restore":
                break;
        }
    }

    #region plc程序

    private static byte[] D;
    private static int    PZ;
    private static Plc    plc;

    // 定时器触发的事件处理方法
    private void OnTimedEvent(object sender, ElapsedEventArgs e)
    {
        try
        {
            PZ = PlcService.ReadDbInt(501, 4);
            PlcService.WriteDbInt(501, 6, 6);
        }
        catch
        {
        }
    }

    #endregion

    private void closeDevice()
    {
        CameraService.StopCamera();

        Console.WriteLine("相机已关闭");
    }

    private void ConnectPlc()
    {
        _timer.Start();
        // Step 1: 创建并连接到 PLC
        // plc = new Plc(CpuType.S71200, "192.168.8.30", 0, 1); // CPU 类型，PLC IP 地址，Rack 和 Slot
        // plc.Open();                                          // 打开连接
        PlcService.InitPLC(CpuType.S71200, "192.168.8.30", 0, 1);
    }

    private void RefreshDeviceList()
    {

        // 检查 CameraList 是否已初始化
        if (CameraList == null) CameraList = new ObservableCollection<Camera>(); // 如果未初始化，进行初始化
        // 清空 CameraList
        CameraList.Clear();

        var deviceInfoList = CameraService.ScanCamera();
        // 在 CameraList 中添加设备
        foreach (var deviceInfo in deviceInfoList)
        {
            var cameraName = !string.IsNullOrEmpty(deviceInfo.UserDefinedName)
                ? $"{deviceInfo.TLayerType}: {deviceInfo.UserDefinedName} ({deviceInfo.SerialNumber})"
                : $"{deviceInfo.TLayerType}: {deviceInfo.ManufacturerName} {deviceInfo.ModelName} ({deviceInfo.SerialNumber})";

            CameraList.Add(new Camera
            {
                CameraName = cameraName,
                DeviceInfo = deviceInfo
            });
        }
    }

    /// <summary>
    /// 打开设备并开始抓取
    /// </summary>
    /// <returns></returns>
    public async Task OpenDeviceAndStartGrabAsync()
    {
        var deviceInfo = CameraList[0].DeviceInfo;

        // 启动相机的后台任务
        await Task.Run(() => { CameraService.StartCamera(deviceInfo); })
            // 任务完成后继续执行
            .ContinueWith(t =>
            {
                // 在任务完成后检查相机是否成功打开
                if (!CameraService.IsCameraNull())
                    // 相机成功打开，更新界面状态
                    Console.WriteLine("相机已打开");
                else
                    // 相机打开失败，更新界面状态
                    MessageBox.Show("相机打开失败");
            }, TaskScheduler.FromCurrentSynchronizationContext()); // 确保在 UI 线程中执行
    }



    /// <summary>
    /// 捕获图像
    /// </summary>
    /// <returns></returns>
    public async Task CaptureImageAsync()
    {
        IFrameOut frameOut;
        var       nRet = CameraService.CaptureImage(out frameOut);
        if (!nRet)
        {
            MessageBox.Show("获取图像失败");
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
            Console.WriteLine($"保存图像失败: {ex.Message}");
            return;
        }

        try
        {
            var hImage = new HImage();
            hImage.ReadImage(filePath);
            Image = hImage;
            Console.WriteLine("捕获图像");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载图像失败: {ex.Message}");
        }
    }


    private void ShapeRecognition()
    {
        var    modelid  = new HTuple();             ////定义变量给模板
        HTuple hv_Row   = new(), hv_Column = new(); //定义变量
        HTuple hv_Angle = new(), hv_Score  = new(); //定义变量

        MatchResult.Reset();

        try
        {
            HOperatorSet.ReadShapeModel("C:/Users/Public/Pictures/crop.shm", out modelid); //读取本地模板

            HOperatorSet.FindShapeModel(Image, modelid, -0.39, 0.79, 0.5, 1, 0.5, "least_squares", 0, 0.9, out hv_Row,
                out hv_Column, out hv_Angle, out hv_Score);
            System.Diagnostics.Debug.WriteLine($"score:{hv_Score} angle:{hv_Angle} row:{hv_Row} column{hv_Column}");
            //获取形状模板轮廓
            HOperatorSet.GetShapeModelContours(out var modelContours, modelid, 1);

            for (var i = 0; i < hv_Score.Length; i++)
            {
                //计算轮廓匹配的目标位置对象
                HOperatorSet.VectorAngleToRigid(0, 0, 0, hv_Row.DArr[i], hv_Column.DArr[i], hv_Angle.DArr[i],
                    out var homMat2D);
                HOperatorSet.AffineTransContourXld(modelContours, out var contoursAffineTrans, homMat2D);
                //HWindow.DispObj(contoursAffineTrans);
                MatchResult.Results.Add(new TemplateMatchResult()
                {
                    Index    = i + 1,
                    Row      = hv_Row.DArr[i],
                    Column   = hv_Column.DArr[i],
                    Angle    = hv_Angle.DArr[i],
                    Score    = hv_Score.DArr[i],
                    Contours = contoursAffineTrans
                });
            }

            //在窗口中渲染结果
            if (MatchResult.Results != null)
                foreach (var item in MatchResult.Results)
                {
                    if (Setting.IsShowCenter)
                        HWindow.DispCross(item.Row, item.Column, 30, item.Angle);

                    if (Setting.IsShowDisplayText)
                        HWindow.SetString($"({Math.Round(item.Row, 2)},{Math.Round(item.Column, 2)})", "Image",
                            item.Row, item.Column, "black", "true");

                    if (Setting.IsShowMatchRange)
                        HWindow.SetColor("red");
                    HWindow.DispObj(item.Contours);
                }
        }
        catch
        {
        }
    }

    /// <summary>
    /// 加载图像
    /// </summary>
    private void LoadImage()
    {
        //FolderBrowserDialog 是 Windows 窗体应用程序中的一种对话框控件，允许用户选择文件夹而不是单个文件。
        var dialog = new FolderBrowserDialog();
        //dialog.Description 设置了对话框的说明文本，显示为“请选择导入图像”。
        dialog.Description = "请选择导入图像";

        //dialog.ShowDialog() 方法会显示对话框。返回值 System.Windows.Forms.DialogResult.OK 表示用户选择了文件夹并点击了“确定”按钮。
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            //将每个 ImageFile 对象添加到 Files 集合中。
            var files = new DirectoryInfo(dialog.SelectedPath).GetFiles();

            Files.Clear();

            foreach (var file in files)
            {
                if (Path.GetExtension(file.FullName) != ".bmp") continue;
                Files.Add(new ImageFile()
                {
                    FileName = file.Name,    //文件名
                    FilePath = file.FullName //文件路径
                });
            }
        }
    }

    /// <summary>
    /// 设置项目参数
    /// </summary>
    private void SetModelParam()
    {
        //1.如果本身的项目参数中存在，就把对应的参数还原到界面上, 模板匹配的过程
        //2.如果不存在参数，则进入编辑参数的模式 
        targetService.GetRefer(Image, Model);
        IsModelEditMode = !IsModelEditMode;

        Message = "全局参数设置, 右键创建基准点。";
    }

    /// <summary>
    /// 更新项目参数
    /// </summary> 
    private async void UpdateModelParam()
    {
        var drawingObj = DrawingObjectInfos.FirstOrDefault(q => q.Color == "green");
        if (drawingObj != null)
        {
            //1.记录当前的形状的尺寸信息: 左上角，右下角，宽度，高度
            var refer = Model.ReferSetting;
            refer.SetReferParam(drawingObj);

            //2.生成一个NCC匹配模板，保存到本地，数据库则存储模板的绝对路径 
            var template = Image.ReduceDomain(refer.X1, refer.Y1, refer.X2, refer.Y2).CropDomain();
            await Model.UpdateReferTemplate(template);

            //3.把上面所设置的信息都保存到数据当中。 
            await appService.CreateOrUpdateAsync(Model);
        }

        Message = "全局参数更新完成。";
    }

    /// <summary>
    /// 创建检测区域
    /// </summary>
    private async void CreateRegion()
    {
        await appService.CreateRegionAsync(new InspecRegionModel()
        {
            Name           = "P" + (RegionList.Count + 1),
            ProjectId      = Model.Id,
            MatchParameter = string.Empty,
            Parameter      = string.Empty
        });

        GetRegionListAsync();
        SelectedRegion = RegionList.Last();
        Message        = "当前选中区域：" + SelectedRegion.Name;
    }

    /// <summary>
    /// 编辑检测区域参数
    /// </summary>
    /// <param name="input"></param>
    private async void EditInspecRegion(InspecRegionModel input)
    {
        if (input == null) return;

        var param = new DialogParameters();
        param.Add("Value", input);

        await dialog.ShowDialogAsync("RegionParameterView", param);
    }

    /// <summary>
    /// 删除检测区域
    /// </summary>
    /// <param name="input">选择删除的检测区域</param> 
    private async void DeleteInspecRegion(InspecRegionModel input)
    {
        if (input == null) return;

        var region = RegionList.FirstOrDefault(q => q.Id == input.Id);

        if (region != null)
        {
            region.Dispose();
            await appService.DeleteRegionAsync(region.Id);
            RegionList.Remove(region);
        }
    }

    /// <summary>
    /// 还原检测区域参数
    /// </summary>
    private void RestoreRegionParameter()
    {
        if (Image == null || SelectedRegion == null)
            return;

        //1.先查找基准点位置
        targetService.GetRefer(Image, Model);

        //2.还原选中检测区域的实际位置
        if (SelectedRegion.Context is IRestoreMatchRegion restore)
            restore.RestorePostion(Image, SelectedRegion, Model.ReferSetting.Row, Model.ReferSetting.Column);

        RaisePropertyChanged(nameof(SelectedRegion));
    }

    /// <summary>
    /// 更新区域参数
    /// </summary>
    private async void UpdateRegion()
    {
        var drawingObj = DrawingObjectInfos.FirstOrDefault(q => q.Color == "red");
        if (drawingObj != null)
        {
            //1. 保存区域的尺寸信息以及相对基准点的偏移参数
            var temp = new TemplateSetting();
            temp.SetReferParam(drawingObj);
            temp.RowSpacing    = Model.ReferSetting.Row    - temp.Row;
            temp.ColumnSpacing = Model.ReferSetting.Column - temp.Column;

            SelectedRegion.MatchSetting = temp;

            //2. 保存区域的模板数据
            var template = Image.ReduceDomain(temp.X1, temp.Y1, temp.X2, temp.Y2)
                .CropDomain()
                .Rgb1ToGray();
            await SelectedRegion.UpdateRegionTemplate(template);

            //3. 保存区域的检测模型 
            var Context = new LocalDeformableContext();
            Context.UpdateVariationModel(template, SelectedRegion);
            SelectedRegion.Context = Context;

            //4. 更新数据库参数
            await appService.UpdateRegionAsync(SelectedRegion);
        }
    }

    /// <summary>
    /// 获取检测区域列表
    /// </summary>
    private async void GetRegionListAsync()
    {
        var list = await appService.GetRegionListAsync(Model.Id);
        RegionList.Clear();
        foreach (var region in list)
        {
            region.ProjectName = Model.Name;
            region.InitRegionContext();
            RegionList.Add(region);
        }
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 重置绘制对象
    /// </summary>
    private void ResetDrawingObject()
    {
        DrawingObjectInfos.Clear();
    }

    #endregion

    #region 导航服务

    private IRegionNavigationJournal Journal;

    public override void OnNavigatedFrom(NavigationContext navigationContext)
    {
        aggregator.GetEvent<ImageTrainEvent>().Unsubscribe(ImageTrain);

        base.OnNavigatedFrom(navigationContext);
    }

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters.ContainsKey("Value"))
        {
            Model = navigationContext.Parameters.GetValue<ProjectModel>("Value");
            GetRegionListAsync();
        }

        aggregator.GetEvent<ImageTrainEvent>().Subscribe(ImageTrain);
        base.OnNavigatedTo(navigationContext);

        Journal = navigationContext.NavigationService.Journal;
    }

    #endregion

    #region 检测服务

    private InspectionResult result;

    /// <summary>
    /// 缺陷检测结果
    /// </summary>
    public InspectionResult Result
    {
        get => result;
        set
        {
            result = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// 检测图像
    /// </summary>
    public void Run()
    {
        Result = inspec.ExecuteAsync(Image, Model, RegionList);
    }

    #endregion

    #region 模型和训练

    /// <summary>
    /// 检测区域模型训练
    /// </summary>
    /// <param name="info"></param>
    private void ImageTrain(ImageTrainInfo info)
    {
        var region = RegionList.FirstOrDefault(t => t.Name == info.Name);
        if (region != null)
        {
            var url = region.GetRegionTrainUrl() + DateTime.Now.ToString("yyyyMMddhhmmss") + ".bmp";
            info.Image.SaveIamge(url);

            if (region.Context is LocalDeformableContext context)
                context.AddTrainImage(region, info.Image);
        }
    }

    #endregion
}