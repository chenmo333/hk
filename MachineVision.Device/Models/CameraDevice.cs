using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvCameraControl;

namespace MachineVision.TemplateMatch.Models
{
    public class CameraDevice
    {
        public string CameraName { get; set; }
        public IDeviceInfo DeviceInfo { get; set; }
    }
}
