using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MachineVision.Device.Services;
using Prism.Ioc;
using Prism.Modularity;

namespace MachineVision.Device;

public class DeviceModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
    }

    public void RegisterTypes(IContainerRegistry services)
    {
        services.RegisterSingleton<IPlcService, PlcService>(nameof(PLCType.S7_1200));
        services.RegisterSingleton<ICameraService, HKCameraService>(nameof(CameraType.HK));
    }
}