using HalconDotNet;
using Newtonsoft.Json;
using System.IO;

namespace MachineVision.Defect.ViewModels.Components.Models
{
    public class TemplateSetting : RectangleSetting
    {
        private string templateFileName, prewViewFileName;

        /// <summary>
        /// 参考点模板文件
        /// </summary>
        public string TemplateFileName
        {
            get { return templateFileName; }
            set
            {
                templateFileName = value; RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 参考点预览图像
        /// </summary>
        public string PrewViewFileName
        {
            get { return prewViewFileName; }
            set
            {
                prewViewFileName = value; RaisePropertyChanged();
            }
        }

        [JsonIgnore]
        public HTuple ModelId;

        /// <summary>
        /// 初始化模板设置参数
        /// </summary>
        public void InitParameter(string url)
        {
            if (!string.IsNullOrWhiteSpace(TemplateFileName))
            {
                if (TemplateFileName.Contains("ncm"))
                {
                    if (File.Exists(url + TemplateFileName))
                        HOperatorSet.ReadNccModel(url + TemplateFileName, out ModelId);

                }
                else if (TemplateFileName.Contains("dfm"))
                {
                    if (File.Exists(url + TemplateFileName))
                        HOperatorSet.ReadDeformableModel(url + TemplateFileName, out ModelId);
                }
            }
        }

        /// <summary>
        /// 释放非托管的资源
        /// </summary>
        public void Dispose()
        {
            TemplateFileName = string.Empty;
            PrewViewFileName = string.Empty;

            ModelId?.Dispose();
        }
    }
}
