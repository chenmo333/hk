using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace MachineVision.Defect.ViewModels.Components.Models
{
    /// <summary>
    /// 缺陷检测参数配置
    /// 说明: 包含模型名称，模型的参数
    /// </summary>
    public class VariationSetting : BindableBase
    {
        public VariationSetting()
        {
            Parameters = new ObservableCollection<VariationParameter>();
        }

        private ObservableCollection<VariationParameter> parameters;

        /// <summary>
        /// 缺陷检测参数
        /// </summary>
        public ObservableCollection<VariationParameter> Parameters
        {
            get { return parameters; }
            set { parameters = value; RaisePropertyChanged(); }
        }

        private string stdFileName;

        /// <summary>
        /// 差异模型名称
        /// </summary>
        public string StdFileName
        {
            get { return stdFileName; }
            set { stdFileName = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 初始化检测参数
        /// </summary>
        public void InitParameters()
        {
            foreach (var parameter in Parameters)
                parameter.InitThresholds();
        }
    }
}
