using HalconDotNet; 

namespace MachineVision.Defect.Models
{
    /// <summary>
    /// 亮缺陷与暗缺陷显示区域
    /// </summary>
    public class LightAndDarkRegion
    {
        public HObject Image { get; set; }

        public HObject Light { get; set; }

        public HObject Dark { get; set; }
    }
}
