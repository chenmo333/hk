using HalconDotNet;
using MachineVision.Core.Extensions;
using MachineVision.Defect.Extensions;
using MachineVision.Defect.Models;

namespace MachineVision.Defect.Services
{
    public class TargetService
    {
        private int angleStart = 0, angleExtend = 0;
        private double minScore = 0.7;
        private int numMatches = 1;
        private double maxOverlap = 0.5;
        private string subPixel = "true";
        private int numLevels = 0;

        private HTuple hv_Row = new HTuple();
        private HTuple hv_Column = new HTuple();
        private HTuple hv_Angle = new HTuple();
        private HTuple hv_Score = new HTuple();

        /// <summary>
        /// 查找参考点
        /// </summary>
        /// <param name="Image"></param>
        /// <param name="Model"></param>
        public bool GetRefer(HObject Image, ProjectModel Model)
        {
            var refer = Model.ReferSetting;

            if (refer.ModelId == null) return false;
            HOperatorSet.FindNccModel(Image.Rgb1ToGray(), refer.ModelId,
                angleStart, angleExtend,
                minScore, numMatches, maxOverlap,
                subPixel, numLevels, out hv_Row, out hv_Column, out hv_Angle, out hv_Score);

            if (hv_Score.Length > 0)
            {
                //1.根据已保存的宽度和高度，以及定位到的中点坐标，计算出2个点的坐标
                var rc = RectangleExtensions.GetRectangleLocation(refer.Width, refer.Height, hv_Row, hv_Column);
                //2.还原参考点的左上角和右下角坐标, 用于还原真实的参考点位置
                refer.X1 = rc.X1;
                refer.Y1 = rc.Y1;
                refer.X2 = rc.X2;
                refer.Y2 = rc.Y2;

                refer.Row = hv_Row.D;
                refer.Column = hv_Column.D;
                return true;
            }

            return false;
        }
    }
}
