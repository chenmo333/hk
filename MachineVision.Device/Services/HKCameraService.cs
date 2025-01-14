using System.IO;
using System.Windows;
using System.Drawing.Imaging;
using MvCameraControl;

namespace MachineVision.Device.Services;

public class HKCameraService : ICameraService
{
    // 设备类型
    private readonly DeviceTLayerType enumTLayerType = DeviceTLayerType.MvGigEDevice            |
                                                       DeviceTLayerType.MvUsbDevice             |
                                                       DeviceTLayerType.MvGenTLGigEDevice       |
                                                       DeviceTLayerType.MvGenTLCXPDevice        |
                                                       DeviceTLayerType.MvGenTLCameraLinkDevice |
                                                       DeviceTLayerType.MvGenTLXoFDevice;

    private List<IDeviceInfo> _deviceInfoList; // 设备列表
    private IDevice           _device;         // 当前设备
    private bool              _isGrabbing;     // 捕获标志

    /// <summary>
    /// 构造函数
    /// </summary>
    public HKCameraService()
    {
        _deviceInfoList = new List<IDeviceInfo>();
        _device         = null;
    }

    public bool IsGrabbing => _isGrabbing;

    public List<IDeviceInfo> ScanCamera()
    {
        SDKSystem.Initialize(); // 初始化海康 SDK

        _deviceInfoList.Clear(); // 清空上一次扫描

        // 枚举设备
        var nRet = DeviceEnumerator.EnumDevices(enumTLayerType, out _deviceInfoList);

        if (nRet != MvError.MV_OK) MessageBox.Show("设备枚举失败！");


        return _deviceInfoList;
    }

    /// <summary>
    /// 启动相机
    /// </summary>
    /// <param name="deviceInfo">设备信息</param>
    /// <returns></returns>
    public bool StartCamera(IDeviceInfo deviceInfo)
    {
        try
        {
            if (deviceInfo == null) deviceInfo = _deviceInfoList.FirstOrDefault();

            // 创建设备
            _device = DeviceFactory.CreateDevice(deviceInfo);
            // 打开设备
            var result = _device.Open();
            if (result != MvError.MV_OK)
                // _device = null;
                return false;

            // 判断是否为GigE设备并设置包大小
            if (_device is IGigEDevice gigEDevice)
            {
                int optionPacketSize;
                result = gigEDevice.GetOptimalPacketSize(out optionPacketSize);
                if (result == MvError.MV_OK)
                    result = _device.Parameters.SetIntValue("GevSCPSPacketSize", (long)optionPacketSize);
            }

            // 设置心跳超时
            _device.Parameters.SetIntValue("GevHeartbeatTimeout", 3000);
            // **设置触发源为软件**
            result = _device.Parameters.SetEnumValueByString("TriggerSource", "Software");
            // **启用触发模式**
            result = _device.Parameters.SetEnumValueByString("TriggerMode", "On");
            // 设置采集模式为连续
            result = _device.Parameters.SetEnumValueByString("AcquisitionMode", "Continuous");

            _isGrabbing = true;

            // 开始采集 
            result = _device.StreamGrabber.StartGrabbing();
            if (result != MvError.MV_OK) _isGrabbing = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine("创建设备失败！" + ex.Message);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 拍照
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    public bool CaptureImage(out IFrameOut frame)
    {
        frame = null;

        try
        {
            // 发送软件触发命令
            var result = _device.Parameters.SetCommandValue("TriggerSoftware");
            if (result != MvError.MV_OK)
            {
                Console.WriteLine("软件触发命令发送失败");
                return false;
            }

            var nRet = _device.StreamGrabber.GetImageBuffer(500, out var outFrame);
            frame = outFrame;

            if (nRet != MvError.MV_OK)
            {
                Console.WriteLine("获取图像失败");
                return false;
            }

            _device.StreamGrabber.FreeImageBuffer(outFrame);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"捕获图像时出错: {ex.Message}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 停止相机
    /// </summary>
    /// <returns></returns>
    public void StopCamera()
    {
        var result = _device.StreamGrabber.StopGrabbing();
        _device.Close();
        _device.Dispose();
    }

    public bool IsGrabing()
    {
        return IsGrabbing;
    }

    public void SetExposureTime(float value)
    {
        var nRet = _device.Parameters.SetFloatValue("ExposureTime", value);
    }

    public void SetGain(float value)
    {
        _device.Parameters.SetFloatValue("Gain", value);
    }

    public float GetExposureTime()
    {
        _device.Parameters.GetFloatValue("ExposureTime", out var exposureTime_IF);
        return exposureTime_IF.CurValue;
    }

    public float GetGain()
    {
        _device.Parameters.GetFloatValue("Gain", out var gain_IF);
        return gain_IF.CurValue;
    }

    public bool IsCameraNull()
    {
        return _device == null;
    }
}