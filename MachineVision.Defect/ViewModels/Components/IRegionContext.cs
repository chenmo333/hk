using HalconDotNet;
using MachineVision.Defect.Models;

namespace MachineVision.Defect.ViewModels.Components
{
    /// <summary>
    /// 区域检测算法
    /// </summary>
    public interface IRegionContext
    {
        void Import(string Parameter);

        RegionContextResult Run(HObject Image, InspecRegionModel Model);

        /// <summary>
        /// 获取算法的参数设定
        /// </summary>
        /// <returns></returns>
        string GetJsonParameter();

        void Disponse();
    }
}
