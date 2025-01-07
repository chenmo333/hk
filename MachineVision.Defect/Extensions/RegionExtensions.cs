using HalconDotNet;
using MachineVision.Core.TemplateMatch.LocalDeformable;
using MachineVision.Defect.Models;

namespace MachineVision.Defect.Extensions
{
    /// <summary>
    /// 检测区域的扩展服务
    /// </summary>
    public static partial class RegionExtensions
    {
        /// <summary>
        /// 更新区域的检测定位模板(形变匹配)
        /// </summary>
        /// <param name="Region"></param>
        /// <param name="Template"></param>
        /// <returns></returns>
        public static async Task UpdateRegionTemplate(this InspecRegionModel Region, HObject Template)
        {
            var url = Region.GetRegionUrl();

            var setting = Region.MatchSetting;
            setting.PrewViewFileName = "default.png";
            setting.TemplateFileName = "default.dfm";
            setting.ModelId = await CreateLocalDeformableModel(Template, url + setting.TemplateFileName);
        }

        /// <summary>
        /// 创建形变匹配模板
        /// </summary>
        /// <param name="Template"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        private static async Task<HTuple> CreateLocalDeformableModel(HObject Template, string Url)
        {
            //创建形变匹配模型的默认参数
            var input = new LocalDeformableInputParameter();
            input.ApplyDefaultParameter();

            return await Task.Run(() =>
            {
                //创建形变匹配模型
                HOperatorSet.CreateLocalDeformableModel(Template,
                        input.NumLevels,
                        input.AngleStart,
                        input.AngleExtent,
                        input.AngleStep,
                        input.ScaleRmin,
                        input.ScaleRmax,
                        input.ScaleRstep,
                        input.ScaleCmin,
                        input.ScaleCmax,
                        input.ScaleCstep,
                        input.Optimization,
                        input.Metric,
                        input.Contrast,
                        input.MinContrast, new HTuple(), new HTuple(), out HTuple modelId);

                string ImageFileName = Url.Replace(".dfm", "") + ".png";

                HOperatorSet.WriteImage(Template, "png", 0, ImageFileName);
                HOperatorSet.WriteDeformableModel(modelId, Url);

                return modelId;
            });
        }
    }
}
