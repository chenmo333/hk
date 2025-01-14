using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MachineVision.TemplateMatch.Models;
using MvCameraControl;

namespace MachineVision.Device.Services;

public interface ICameraService
{
    List<IDeviceInfo> ScanCamera();

    bool StartCamera(IDeviceInfo deviceInfo);

    bool CaptureImage(out IFrameOut frame);

    void StopCamera();

    bool IsGrabing();

    void SetExposureTime(float value);

    void SetGain(float value);

    float GetExposureTime();

    float GetGain();

    bool IsCameraNull();
}