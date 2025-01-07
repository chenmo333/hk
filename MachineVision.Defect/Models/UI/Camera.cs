using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvCameraControl;

namespace MachineVision.Defect.Models.UI
{
    public class Camera
    {
        public string CameraName { get; set; }
        public IDeviceInfo DeviceInfo { get; set; }
    }

}
