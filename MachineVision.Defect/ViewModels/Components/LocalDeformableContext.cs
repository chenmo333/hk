using HalconDotNet;
using MachineVision.Core.Extensions;
using MachineVision.Core.TemplateMatch.LocalDeformable;
using MachineVision.Defect.Extensions;
using MachineVision.Defect.Models;
using MachineVision.Defect.ViewModels.Components.Models;
using Newtonsoft.Json;
using System.IO;
using System.Security.Policy;

namespace MachineVision.Defect.ViewModels.Components
{
    /// <summary>
    /// 缺陷检测服务
    /// </summary>
    public class LocalDeformableContext : IRegionContext, IRestoreMatchRegion
    {
        public LocalDeformableContext()
        {
            Setting = new VariationSetting();
            input.ApplyDefaultParameter();
        }

        private LocalDeformableRunParameter input = new LocalDeformableRunParameter();
        public VariationSetting Setting { get; set; }
        HTuple hv_Score = new HTuple();
        HTuple hv_Row = new HTuple();
        HTuple hv_Column = new HTuple();
        private HTuple StandardId = new HTuple();

        public string GetJsonParameter() => JsonConvert.SerializeObject(Setting);

        public void Import(string Parameter)
        {
            if (!string.IsNullOrWhiteSpace(Parameter))
                Setting = JsonConvert.DeserializeObject<VariationSetting>(Parameter);
            else
                Setting = new VariationSetting();

            Setting.InitParameters();
        }

        public void InitStandardId(string url)
        {
            var fileName = url + Setting.StdFileName;
            if (File.Exists(fileName))
                HOperatorSet.ReadVariationModel(fileName, out StandardId);
        }

        public RegionContextResult Run(HObject Image, InspecRegionModel Model)
        {
            RegionContextResult contextResult = new RegionContextResult();

            //Image : 等待形变匹配的一个图像
            //Model : 待检测区域的对象
            HOperatorSet.FindLocalDeformableModel(Image.Rgb1ToGray(),
                out input.ImageRectified,
                out input.VectorField,
                out input.DeformedContours,
                Model.MatchSetting.ModelId,
                input.AngleStart,
                input.AngleExtent,
                input.ScaleRmin,
                input.ScaleRmax,
                input.ScaleCmin,
                input.ScaleCmax,
                input.MinScore,
                input.NumMatches,
                input.MaxOverlap,
                input.NumLevels,
                input.Greediness,
                ((new HTuple("image_rectified"))
                .TupleConcat("vector_field"))
                .TupleConcat("deformed_contours"),
                (new HTuple("deformation_smoothness").TupleConcat("expand_border").TupleConcat("subpixel").TupleConcat("scale_c_step").TupleConcat("scale_r_step")),
                (new HTuple(50).TupleConcat(0).TupleConcat(1).TupleConcat(0.1).TupleConcat(0.1)),
                out hv_Score, out hv_Row, out hv_Column); ;

            if (hv_Score.Length > 0)
            {
                //获取实际检测的目标位置
                contextResult.Location = RectangleExtensions.GetRectangleLocation(Model.MatchSetting.Width, Model.MatchSetting.Height, hv_Row.D, hv_Column.D);

                //input.ImageRectified.SaveIamge("C:\\Users\\henji\\Pictures\\Saved Pictures\\ImageRectified.bmp");
                //input.ImageRectified  : 最终形变纠正后的标准图像
                //拿这个图像和差分模型中的ModelId进行差分
                //差分过程中,将我们界面设置的条件进行筛选: 亮阈值，面积，暗阈值，面积进行筛选
                //最终输出结果
                var render = GetPrepareVariationModel();
                if (render != null)
                {
                    contextResult.Name = Model.Name;//检测区域名称
                    contextResult.IsSuccess = false;
                    contextResult.Render = render;
                }
                else
                {
                    contextResult.IsSuccess = true;
                }
            }
            else
            {
                contextResult.IsSuccess = false;
                contextResult.Message = "未匹配";
            }
            return contextResult;
        }

        /// <summary>
        /// 获取模型中的缺陷数据汇总: 亮缺陷以及暗缺陷
        /// </summary>
        /// <returns></returns>
        private LightAndDarkRegion GetPrepareVariationModel()
        {
            foreach (var item in Setting.Parameters)
            {
                //AbsThreshold: 0~255  [0~255,0~255]  ,当你的参数是一个值的时候，这个值代表了亮和暗的绝对阈值,如果是一个数组，那么它就是[亮,暗]
                //VarThreshold: 相对阈值 halcon的示例中，该值默认在3

                //亮缺陷筛选
                HOperatorSet.PrepareVariationModel(StandardId, item.H_AbsThreshold, item.H_VarThreshold);
                HOperatorSet.CompareVariationModel(input.ImageRectified, out HObject light, StandardId);
                HOperatorSet.Connection(light, out HObject lightRegions);
                HOperatorSet.SelectShape(lightRegions, out HObject lightErrors, "area", "and", item.MinArea, 999999999);

                //暗缺陷筛选
                HOperatorSet.PrepareVariationModel(StandardId, item.H_DarkAbsThreshold, item.H_DarkVarThreshold);
                HOperatorSet.CompareVariationModel(input.ImageRectified, out HObject dark, StandardId);
                HOperatorSet.Connection(dark, out HObject darkRegions);
                HOperatorSet.SelectShape(darkRegions, out HObject darkErrors, "area", "and", item.MinDarkArea, 999999999);

                HOperatorSet.CountObj(lightErrors, out HTuple lightCount);
                HOperatorSet.CountObj(darkErrors, out HTuple darkCount);

                if (lightCount.D == 0 && darkCount.D == 0)
                    return null;

                return new LightAndDarkRegion()
                {
                    Image = input.ImageRectified,
                    Light = lightErrors,
                    Dark = darkErrors
                };
            }
            return null;
        }

        /// <summary>
        /// 更新检测区域的模型
        /// </summary>
        /// <param name="Image"></param>
        /// <param name="Model"></param>
        public void UpdateVariationModel(HObject Image, InspecRegionModel Model)
        {
            var url = Model.GetRegionUrl();
            Setting.StdFileName = "standard.vam";

            //获取Image尺寸
            var size = Image.GetImageSize();

            //设置检测区域的实际尺寸
            Model.MatchSetting.Width = size[0];
            Model.MatchSetting.Height = size[1];

            //创建差异模型
            HOperatorSet.CreateVariationModel(size[0], size[1], "byte", "standard", out StandardId);
            //输入一张Image, 训练模型
            HOperatorSet.TrainVariationModel(Image, StandardId);
            //保存模型
            HOperatorSet.WriteVariationModel(StandardId, url + "\\" + Setting.StdFileName);

            var stdUrl = Model.GetRegionTrainUrl() + "standard.bmp";
            Image.SaveIamge(stdUrl);
        }

        /// <summary>
        /// 添加新的训练图像
        /// </summary>
        /// <param name="Model"></param>
        /// <param name="Image"></param>
        public void AddTrainImage(InspecRegionModel Model, HObject Image)
        {
            var url = Model.GetRegionTrainUrl();
            HOperatorSet.TrainVariationModel(Image, StandardId);
            HOperatorSet.WriteVariationModel(StandardId, url + "\\" + Setting.StdFileName);
        }

        /// <summary>
        /// 更新本地的模型
        /// </summary>
        /// <param name="Model"></param>
        public void RefreshVariationModel(InspecRegionModel Model)
        {
            var url = Model.GetRegionTrainUrl();

            if (Directory.Exists(url))
            {
                var files = Directory.GetFiles(url);
                if (files.Length == 0) return;

                //读取本地的图像缓存到集合中，等待训练
                List<HObject> trainImages = new List<HObject>();
                foreach (var item in files)
                {
                    if (Path.GetExtension(item) != ".bmp") continue;
                    var image = new HImage();
                    image.ReadImage(item);
                    trainImages.Add(image);
                }

                //创建差异模型
                HOperatorSet.CreateVariationModel(Model.MatchSetting.Width, Model.MatchSetting.Height, "byte", "standard", out StandardId);

                //训练本地的缓存图像
                foreach (var item in trainImages)
                    HOperatorSet.TrainVariationModel(item, StandardId);

                //保存模型
                HOperatorSet.WriteVariationModel(StandardId, url + "\\" + Setting.StdFileName);
            }
        }

        /// <summary>
        /// 还原检测区域的实际位置
        /// </summary>
        /// <param name="Image">图像源</param>
        /// <param name="Model">检测区域</param>
        /// <param name="Row">Row</param>
        /// <param name="Column">Column</param>
        public void RestorePostion(HObject Image, InspecRegionModel Model, double Row, double Column)
        {
            var temp = Model.MatchSetting;
            if (temp.ModelId == null) return;

            //1.获取检测区域在大图像当中的相对位置
            var rl = temp.GetMatchRectangle(Row, Column);
            //2.在相对位置中查找该检测区域的实际位置
            HOperatorSet.FindLocalDeformableModel(Image.ReduceDomain(rl.GenRectangle1()),
                out input.ImageRectified,
                out input.VectorField,
                out input.DeformedContours,
                temp.ModelId,
                input.AngleStart,
                input.AngleExtent,
                input.ScaleRmin,
                input.ScaleRmax,
                input.ScaleCmin,
                input.ScaleCmax,
                input.MinScore,
                input.NumMatches,
                input.MaxOverlap,
                input.NumLevels,
                input.Greediness,
                ((new HTuple("image_rectified"))
                .TupleConcat("vector_field"))
                .TupleConcat("deformed_contours"),
                new HTuple(),
                new HTuple(), out hv_Score, out hv_Row, out hv_Column);

            if (hv_Score.Length > 0)
            {
                var location = RectangleExtensions.GetRectangleLocation(temp.Width, temp.Height, hv_Row.D, hv_Column.D);

                temp.X1 = location.X1;
                temp.Y1 = location.Y1;
                temp.Y2 = location.Y2;
                temp.X2 = location.X2;
            }
        }

        public void Disponse()
        {
            hv_Score.Dispose();
            hv_Row.Dispose();
            hv_Column.Dispose();

            input.Disponse();
        }
    }
}
