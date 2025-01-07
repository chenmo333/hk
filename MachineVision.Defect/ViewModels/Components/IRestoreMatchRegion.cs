using HalconDotNet;
using MachineVision.Defect.Models; 

namespace MachineVision.Defect.ViewModels.Components
{
    public interface IRestoreMatchRegion
    {
        void RestorePostion(HObject Image, InspecRegionModel Model, double Row, double Column);
    }
}
