using HalconDotNet;
using MachineVision.Defect.Models;
using MachineVision.Defect.ViewModels.Components;
using MachineVision.Core.Extensions;

namespace MachineVision.Defect.Extensions
{
    public static class InspecRegionModelExtensions
    {
        public static IRegionContext GetRegionContext(this InspecRegionModel Model)
        {
            var context = new LocalDeformableContext();
            context.Import(Model.Parameter);

            return context;
        }

        /// <summary>
        /// 获取检测预期图像
        /// </summary>
        /// <param name="Model"></param>
        /// <param name="ImageSource"></param>
        /// <param name="Row"></param>
        /// <param name="Column"></param>
        /// <returns></returns>
        public static HObject GetInspecImage(this InspecRegionModel Model, HObject ImageSource, double Row, double Column)
        {
            var temp = Model.MatchSetting;
            var rl = temp.GetMatchRectangle(Row, Column);
            return ImageSource.ReduceDomain(rl.GenRectangle1());
        }
    }
}
