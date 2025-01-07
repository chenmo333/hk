using HalconDotNet;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineVision.Defect.Events
{
    public class ImageTrainInfo
    {
        public string Name { get; set; }

        public HObject Image { get; set; }
    }

    internal class ImageTrainEvent : PubSubEvent<ImageTrainInfo>
    {
    }
}
