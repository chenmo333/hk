using MachineVision.Defect.Extensions;
using MachineVision.Defect.ViewModels.Components.Models;
using Newtonsoft.Json;
using Prism.Mvvm; 

namespace MachineVision.Defect.Models
{
    public class ProjectModel : BindableBase
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ReferParameter { get; set; }

        /// <summary>
        /// 参考点数据
        /// </summary>
        public TemplateSetting ReferSetting { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// 初始化项目参数
        /// </summary>
        public void InitParameter()
        {
            if (!string.IsNullOrWhiteSpace(ReferParameter))
            {
                //初始化参考点参数
                ReferSetting = JsonConvert.DeserializeObject<TemplateSetting>(ReferParameter);
                ReferSetting.InitParameter(this.GetReferUrl());
            }
            else
            {
                ReferSetting = new TemplateSetting();
            }
        }
    }
}
