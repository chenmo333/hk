using MachineVision.Defect.Extensions;
using MachineVision.Defect.ViewModels.Components;
using MachineVision.Defect.ViewModels.Components.Models;
using Newtonsoft.Json;

namespace MachineVision.Defect.Models
{
    public class InspecRegionModel : IDisposable
    {
        public int Id { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// 项目ID
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 检测区域的设定参数 (JSON)
        /// </summary>
        public string Parameter { get; set; }

        /// <summary>
        /// 检测区域模型及位置参数 (JSON)
        /// </summary>
        public string MatchParameter { get; set; }

        /// <summary>
        /// 检测区域模型及位置参数对象
        /// </summary>
        public TemplateSetting MatchSetting { get; set; }

        /// <summary>
        /// 检测区域服务
        /// </summary>
        [JsonIgnore]
        public IRegionContext Context { get; set; }

        /// <summary>
        /// 初始化区域上下文
        /// </summary>
        public void InitRegionContext()
        {
            if (!string.IsNullOrWhiteSpace(MatchParameter))
                MatchSetting = JsonConvert.DeserializeObject<TemplateSetting>(MatchParameter);
            else
                MatchSetting = new TemplateSetting();

            Context = this.GetRegionContext();

            if (Context is LocalDeformableContext conext)
                conext.InitStandardId(this.GetRegionUrl());

            MatchSetting.InitParameter(this.GetRegionUrl());
        }

        public void Dispose()
        {
            Context?.Disponse();
            MatchSetting?.Dispose();

            Parameter = string.Empty;
            MatchParameter = string.Empty;
            MatchSetting = null;
        }
    }
}
